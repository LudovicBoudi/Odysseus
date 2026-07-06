using System;

namespace Odysseus.Core;

public readonly struct BaseAttributes
{
	public int Constitution { get; init; }
	public int Force { get; init; }
	public int Dexterity { get; init; }
	public int Intelligence { get; init; }
	public int Spirit { get; init; }

	public int Sum()
	{
		return Constitution + Force + Dexterity + Intelligence + Spirit;
	}

	public BaseAttributes Increment(SecondaryLevel level)
	{
		int bonus = level switch
		{
			SecondaryLevel.Tier20to40 => 2,
			SecondaryLevel.Tier41to60 => 4,
			SecondaryLevel.Tier61to80 => 6,
			SecondaryLevel.Tier81to100 => 8,
			SecondaryLevel.Prestige => 12,
			SecondaryLevel.Low => 0,
			_ => 0
		};
		return new BaseAttributes { Constitution = Constitution, Force = Force + bonus, Dexterity = Dexterity, Intelligence = Intelligence, Spirit = Spirit };
	}

	public static BaseAttributes Zero => new();
}

public readonly struct SecondaryAttributes
{
	public int Attack { get; init; }
	public int Defense { get; init; }
	public float CriticalChance { get; init; }
	public float CriticalProtection { get; init; }
	public float CriticalBonus { get; init; }
}

public readonly struct VitalStats
{
	public int MaxHp { get; init; }
	public int MaxMana { get; init; }
	public int HpRegen { get; init; }
	public int ManaRegen { get; init; }
}

public enum SecondaryLevel
{
	Low,
	Tier20to40,
	Tier41to60,
	Tier61to80,
	Tier81to100,
	Prestige
}

public static class ProgressionFormula
{
	public static int BonusPointsForLevel(int level, int prestige)
	{
		if (prestige > 0) return 12;
		if (level >= 81) return 8;
		if (level >= 61) return 6;
		if (level >= 41) return 4;
		if (level >= 20) return 2;
		return 0;
	}

	public static long XpForLevel(int level)
	{
		if (level <= 1) return 100;
		double xp = Math.Floor(100.0 * Math.Pow(1.12, level - 1));
		if (xp > long.MaxValue) return long.MaxValue;
		return (long)xp;
	}

	public static VitalStats VitalStatsForLevel(int level, int constitution, int intelligence)
	{
		int maxHp = 200 + (level - 1) * 20 + constitution * 10;
		int maxMana = 100 + (level - 1) * 10 + intelligence * 5;
		return new VitalStats { MaxHp = maxHp, MaxMana = maxMana, HpRegen = 0, ManaRegen = 0 };
	}
}