using System;
using Godot;
using Odysseus.Core;
using Odysseus.Data;
using Odysseus.Managers;

namespace Odysseus.Managers;

public sealed partial class LevelManager : Node
{
	public static LevelManager Instance { get; private set; } = null!;

	public event Action<int>? LevelUp;

	public override void _EnterTree() => Instance = this;

	public void GrantXp(long amount)
	{
		var p = GameStateManager.Instance.Player;
		if (p == null) return;
		p.Xp += amount;
		int previous = p.Level;
		while (p.Level < 100)
		{
			long needed = ProgressionFormula.XpForLevel(p.Level + 1);
			if (p.Xp < needed) break;
			p.Level++;
			OnLevelUp(p);
		}
		if (p.Level != previous)
		{
			LevelUp?.Invoke(p.Level);
			GameStateManager.Instance.Save();
			GD.Print($"[LevelManager] Level Up: {previous} -> {p.Level}");
		}
		else
		{
			GameStateManager.Instance.Save();
		}
	}

	private void OnLevelUp(PlayerData p)
	{
		p.UnspentBasePoints += 5 + GetBonusBasePoints(p.Level);
		p.UnspentSecondaryPoints += 1;
		var vitals = ProgressionFormula.VitalStatsForLevel(p.Level, p.BaseAttributes.Constitution, p.BaseAttributes.Intelligence);
		p.HpCurrent = Math.Min(p.HpCurrent + (vitals.MaxHp - ProgressionFormula.VitalStatsForLevel(p.Level - 1, p.BaseAttributes.Constitution, p.BaseAttributes.Intelligence).MaxHp), vitals.MaxHp);
		p.ManaCurrent = Math.Min(p.ManaCurrent + (vitals.MaxMana - ProgressionFormula.VitalStatsForLevel(p.Level - 1, p.BaseAttributes.Constitution, p.BaseAttributes.Intelligence).MaxMana), vitals.MaxMana);
		GD.Print($"[LevelManager] Niveau {p.Level}: +{5 + GetBonusBasePoints(p.Level)} pts base, +1 pt complémentaire");
	}

	private static int GetBonusBasePoints(int level)
	{
		if (level >= 81) return 8;
		if (level >= 61) return 6;
		if (level >= 41) return 4;
		if (level >= 20) return 2;
		return 0;
	}

	public bool SpendBasePoint(PlayerData p, string attribute, int amount = 1)
	{
		if (p.UnspentBasePoints < amount) return false;
		switch (attribute)
		{
			case "Constitution": p.BaseAttributes = new BaseAttributes { Constitution = p.BaseAttributes.Constitution + amount, Force = p.BaseAttributes.Force, Dexterity = p.BaseAttributes.Dexterity, Intelligence = p.BaseAttributes.Intelligence, Spirit = p.BaseAttributes.Spirit }; break;
			case "Force": p.BaseAttributes = new BaseAttributes { Constitution = p.BaseAttributes.Constitution, Force = p.BaseAttributes.Force + amount, Dexterity = p.BaseAttributes.Dexterity, Intelligence = p.BaseAttributes.Intelligence, Spirit = p.BaseAttributes.Spirit }; break;
			case "Dexterity": p.BaseAttributes = new BaseAttributes { Constitution = p.BaseAttributes.Constitution, Force = p.BaseAttributes.Force, Dexterity = p.BaseAttributes.Dexterity + amount, Intelligence = p.BaseAttributes.Intelligence, Spirit = p.BaseAttributes.Spirit }; break;
			case "Intelligence": p.BaseAttributes = new BaseAttributes { Constitution = p.BaseAttributes.Constitution, Force = p.BaseAttributes.Force, Dexterity = p.BaseAttributes.Dexterity, Intelligence = p.BaseAttributes.Intelligence + amount, Spirit = p.BaseAttributes.Spirit }; break;
			case "Spirit": p.BaseAttributes = new BaseAttributes { Constitution = p.BaseAttributes.Constitution, Force = p.BaseAttributes.Force, Dexterity = p.BaseAttributes.Dexterity, Intelligence = p.BaseAttributes.Intelligence, Spirit = p.BaseAttributes.Spirit + amount }; break;
			default: return false;
		}
		p.UnspentBasePoints -= amount;
		return true;
	}
}