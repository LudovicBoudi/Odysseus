using System;
using System.IO;
using Xunit;
using Odysseus.Data;
using Odysseus.Core;

namespace Odysseus.Tests;

public class PlayerRepositoryTests : IDisposable
{
	private readonly string _tmp;
	private readonly Database _db;
	private const string Schema = """
		PRAGMA foreign_keys = ON;
		CREATE TABLE IF NOT EXISTS meta (key TEXT PRIMARY KEY, value TEXT NOT NULL);
		CREATE TABLE IF NOT EXISTS player (
			id                      INTEGER PRIMARY KEY CHECK (id = 1),
			name                    TEXT    NOT NULL,
			gender                  TEXT    NOT NULL,
			divine_affiliation      TEXT    NOT NULL,
			creation_seed           TEXT    NOT NULL DEFAULT '',
			level                   INTEGER NOT NULL DEFAULT 1,
			prestige                INTEGER NOT NULL DEFAULT 0,
			xp                      INTEGER NOT NULL DEFAULT 0,
			gold                    INTEGER NOT NULL DEFAULT 0,
			gem                     INTEGER NOT NULL DEFAULT 0,
			base_attributes         TEXT    NOT NULL DEFAULT '{}',
			unspent_base_pts        INTEGER NOT NULL DEFAULT 0,
			unspent_secondary_pts   INTEGER NOT NULL DEFAULT 0,
			current_map_id          TEXT,
			created_at              TEXT    NOT NULL,
			updated_at              TEXT    NOT NULL
		);
		CREATE TABLE IF NOT EXISTS player_vitals (
			player_id    INTEGER PRIMARY KEY REFERENCES player(id) ON DELETE CASCADE,
			hp_current   INTEGER NOT NULL DEFAULT 200,
			mana_current INTEGER NOT NULL DEFAULT 100
		);
	""";

	public PlayerRepositoryTests()
	{
		_tmp = Path.Combine(Path.GetTempPath(), $"ody_player_{Guid.NewGuid():N}.sqlite");
		_db = Database.Open(_tmp, Schema);
	}

	public void Dispose()
	{
		_db.Dispose();
		try { File.Delete(_tmp); } catch { }
	}

	[Fact]
	public void Load_ReturnsNull_WhenNoPlayer()
	{
		Assert.Null(PlayerRepository.Load(_db));
	}

	[Fact]
	public void Create_InsertsPlayer_And_LoadBacksAllFields()
	{
		var p = new PlayerData
		{
			Name = "Perseus",
			Gender = Gender.Male,
			DivineAffiliation = DivineAffiliation.Zeus,
			CreationSeed = "teint1;cheveux3",
			Level = 12,
			Prestige = 0,
			Xp = 5000,
			Gold = 1234,
			Gem = 7,
			BaseAttributes = new BaseAttributes { Constitution = 10, Force = 20, Dexterity = 5, Intelligence = 8, Spirit = 3 },
			UnspentBasePoints = 42,
			HpCurrent = 250,
			ManaCurrent = 95,
		};

		PlayerRepository.Create(_db, p);
		var loaded = PlayerRepository.Load(_db);

		Assert.NotNull(loaded);
		Assert.Equal("Perseus", loaded!.Name);
		Assert.Equal(Gender.Male, loaded.Gender);
		Assert.Equal(DivineAffiliation.Zeus, loaded.DivineAffiliation);
		Assert.Equal("teint1;cheveux3", loaded.CreationSeed);
		Assert.Equal(12, loaded.Level);
		Assert.Equal(5000L, loaded.Xp);
		Assert.Equal(1234L, loaded.Gold);
		Assert.Equal(7L, loaded.Gem);
		Assert.Equal(10, loaded.BaseAttributes.Constitution);
		Assert.Equal(20, loaded.BaseAttributes.Force);
		Assert.Equal(5, loaded.BaseAttributes.Dexterity);
		Assert.Equal(8, loaded.BaseAttributes.Intelligence);
		Assert.Equal(3, loaded.BaseAttributes.Spirit);
		Assert.Equal(42, loaded.UnspentBasePoints);

		PlayerRepository.LoadVitals(_db, loaded);
		Assert.Equal(250, loaded.HpCurrent);
		Assert.Equal(95, loaded.ManaCurrent);
	}

	[Fact]
	public void Create_OnConflict_UpdatesExisting()
	{
		PlayerRepository.Create(_db, new PlayerData { Name = "A", Gender = Gender.Female, DivineAffiliation = DivineAffiliation.Athena });
		PlayerRepository.Create(_db, new PlayerData { Name = "B", Gender = Gender.Male, DivineAffiliation = DivineAffiliation.Ares, Level = 5 });

		var loaded = PlayerRepository.Load(_db);
		Assert.NotNull(loaded);
		Assert.Equal("B", loaded!.Name);
		Assert.Equal(Gender.Male, loaded.Gender);
		Assert.Equal(DivineAffiliation.Ares, loaded.DivineAffiliation);
		Assert.Equal(5, loaded.Level);
	}

	[Fact]
	public void SaveProgress_PersistsLevel_Xp_Attributes()
	{
		PlayerRepository.Create(_db, new PlayerData { Name = "X", Gender = Gender.Male, DivineAffiliation = DivineAffiliation.Hades });
		var p = PlayerRepository.Load(_db)!;
		p.Level = 50;
		p.Xp = 999999;
		p.BaseAttributes = new BaseAttributes { Constitution = 100 };
		PlayerRepository.SaveProgress(_db, p);

		var loaded = PlayerRepository.Load(_db);
		Assert.Equal(50, loaded!.Level);
		Assert.Equal(999999L, loaded.Xp);
		Assert.Equal(100, loaded.BaseAttributes.Constitution);
	}

	[Fact]
	public void MaxHp_DerivesFromConstitution()
	{
		var p = new PlayerData { Level = 50, BaseAttributes = new BaseAttributes { Constitution = 20 } };
		Assert.Equal(200 + 49 * 20 + 200, p.MaxHp);
	}

	[Fact]
	public void AllEightDivineAffiliations_RoundTrip()
	{
		foreach (var aff in Enum.GetValues<DivineAffiliation>())
		{
			var p = new PlayerData { Name = "T", Gender = Gender.Male, DivineAffiliation = aff };
			PlayerRepository.Create(_db, p);
			var loaded = PlayerRepository.Load(_db)!;
			Assert.Equal(aff, loaded.DivineAffiliation);
		}
		Assert.Equal(1L, (long)_db.Scalar("SELECT COUNT(*) FROM player;"));
		var last = PlayerRepository.Load(_db);
		Assert.Equal(DivineAffiliation.Athena, last!.DivineAffiliation);
	}
}