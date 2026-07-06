using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Odysseus.Core;

namespace Odysseus.Data;

public sealed class PlayerData
{
	public int Id { get; set; } = 1;
	public string Name { get; set; } = "";
	public Gender Gender { get; set; }
	public DivineAffiliation DivineAffiliation { get; set; }
	public string CreationSeed { get; set; } = "";
	public int Level { get; set; } = 1;
	public int Prestige { get; set; }
	public long Xp { get; set; }
	public long Gold { get; set; }
	public long Gem { get; set; }
	public BaseAttributes BaseAttributes { get; set; } = BaseAttributes.Zero;
	public int UnspentBasePoints { get; set; }
	public int UnspentSecondaryPoints { get; set; }
	public string? CurrentMapId { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	public int HpCurrent { get; set; } = 200;
	public int ManaCurrent { get; set; } = 100;

	public int MaxHp => ProgressionFormula.VitalStatsForLevel(Level, BaseAttributes.Constitution, BaseAttributes.Intelligence).MaxHp;
	public int MaxMana => ProgressionFormula.VitalStatsForLevel(Level, BaseAttributes.Constitution, BaseAttributes.Intelligence).MaxMana;
}

public static class PlayerRepository
{
	private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = null };

	public static PlayerData Create(Database db, PlayerData player)
	{
		if (player.Id != 1) player.Id = 1;
		var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

		db.Execute(@"
			INSERT INTO player (id, name, gender, divine_affiliation, creation_seed,
			                   level, prestige, xp, gold, gem,
			                   base_attributes, unspent_base_pts, unspent_secondary_pts,
			                   current_map_id, created_at, updated_at)
			VALUES (1, @name, @gender, @aff, @seed,
			        @level, @prestige, @xp, @gold, @gem,
			        @base, @ubp, @usp,
			        @map, @now, @now)
			ON CONFLICT(id) DO UPDATE SET
				name = excluded.name,
				gender = excluded.gender,
				divine_affiliation = excluded.divine_affiliation,
				creation_seed = excluded.creation_seed,
				level = excluded.level,
				prestige = excluded.prestige,
				xp = excluded.xp,
				gold = excluded.gold,
				gem = excluded.gem;",
			("name", player.Name),
			("gender", player.Gender.ToString()),
			("aff", player.DivineAffiliation.ToString()),
			("seed", player.CreationSeed ?? ""),
			("level", player.Level),
			("prestige", player.Prestige),
			("xp", player.Xp),
			("gold", player.Gold),
			("gem", player.Gem),
			("base", SerializeBaseAttributes(player.BaseAttributes)),
			("ubp", player.UnspentBasePoints),
			("usp", player.UnspentSecondaryPoints),
			("map", (object?)player.CurrentMapId ?? DBNull.Value),
			("now", now));

		db.Execute(@"INSERT INTO player_vitals (player_id, hp_current, mana_current)
		             VALUES (1, @hp, @mana)
		             ON CONFLICT(player_id) DO UPDATE SET
		               hp_current = excluded.hp_current,
		               mana_current = excluded.mana_current;",
			("hp", player.HpCurrent),
			("mana", player.ManaCurrent));

		return player;
	}

	public static PlayerData? Load(Database db)
	{
		using var cmd = db.CreateCommand();
		cmd.CommandText = "SELECT id, name, gender, divine_affiliation, creation_seed, level, prestige, xp, gold, gem, base_attributes, unspent_base_pts, unspent_secondary_pts, current_map_id, created_at FROM player WHERE id = 1;";
		using var reader = cmd.ExecuteReader();
		if (!reader.Read()) return null;

		return new PlayerData
		{
			Id = reader.GetInt32(0),
			Name = reader.GetString(1),
			Gender = Enum.Parse<Gender>(reader.GetString(2)),
			DivineAffiliation = Enum.Parse<DivineAffiliation>(reader.GetString(3)),
			CreationSeed = reader.GetString(4),
			Level = reader.GetInt32(5),
			Prestige = reader.GetInt32(6),
			Xp = reader.GetInt64(7),
			Gold = reader.GetInt64(8),
			Gem = reader.GetInt64(9),
			BaseAttributes = DeserializeBaseAttributes(reader.GetString(10)),
			UnspentBasePoints = reader.GetInt32(11),
			UnspentSecondaryPoints = reader.GetInt32(12),
			CurrentMapId = reader.IsDBNull(13) ? null : reader.GetString(13),
			CreatedAt = DateTime.Parse(reader.GetString(14), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
		};
	}

	public static void LoadVitals(Database db, PlayerData player)
	{
		using var cmd = db.CreateCommand();
		cmd.CommandText = "SELECT hp_current, mana_current FROM player_vitals WHERE player_id = 1;";
		using var reader = cmd.ExecuteReader();
		if (reader.Read())
		{
			player.HpCurrent = reader.GetInt32(0);
			player.ManaCurrent = reader.GetInt32(1);
		}
	}

	public static void SaveVitals(Database db, PlayerData player)
	{
		db.Execute(@"INSERT INTO player_vitals (player_id, hp_current, mana_current)
		             VALUES (1, @hp, @mana)
		             ON CONFLICT(player_id) DO UPDATE SET
		               hp_current = excluded.hp_current,
		               mana_current = excluded.mana_current;",
			("hp", player.HpCurrent),
			("mana", player.ManaCurrent));
	}

	public static void SaveProgress(Database db, PlayerData player)
	{
		var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
		db.Execute(@"
			UPDATE player SET
				level = @level, prestige = @prestige, xp = @xp,
				gold = @gold, gem = @gem,
				base_attributes = @base,
				unspent_base_pts = @ubp,
				unspent_secondary_pts = @usp,
				current_map_id = @map,
				updated_at = @now
			WHERE id = 1;",
			("level", player.Level),
			("prestige", player.Prestige),
			("xp", player.Xp),
			("gold", player.Gold),
			("gem", player.Gem),
			("base", SerializeBaseAttributes(player.BaseAttributes)),
			("ubp", player.UnspentBasePoints),
			("usp", player.UnspentSecondaryPoints),
			("map", (object?)player.CurrentMapId ?? DBNull.Value),
			("now", now));
		SaveVitals(db, player);
	}

	public static string SerializeBaseAttributes(BaseAttributes a) =>
		JsonSerializer.Serialize(new Dictionary<string, int>
		{
			["Constitution"] = a.Constitution,
			["Force"] = a.Force,
			["Dexterity"] = a.Dexterity,
			["Intelligence"] = a.Intelligence,
			["Spirit"] = a.Spirit,
		}, JsonOpts);

	public static BaseAttributes DeserializeBaseAttributes(string json)
	{
		if (string.IsNullOrEmpty(json)) return BaseAttributes.Zero;
		using var doc = JsonDocument.Parse(json);
		var root = doc.RootElement;
		return new BaseAttributes
		{
			Constitution = TryGetInt(root, "Constitution"),
			Force = TryGetInt(root, "Force"),
			Dexterity = TryGetInt(root, "Dexterity"),
			Intelligence = TryGetInt(root, "Intelligence"),
			Spirit = TryGetInt(root, "Spirit"),
		};
	}

	private static int TryGetInt(JsonElement el, string name)
	{
		if (el.TryGetProperty(name, out var v) && v.TryGetInt32(out int i)) return i;
		return 0;
	}
}