namespace Odysseus.Core;

public enum Quality
{
	White = 0,
	Green = 1,
	Blue = 2,
	Purple = 3,
	Orange = 4,
	Yellow = 5,
	Pink = 6,
	Red = 7
}

public enum WeaponType
{
	Sword1H,
	Axe1H,
	Mace1H,
	Sword2H,
	Axe2H,
	Mace2H,
	Staff,
	Spear
}

public enum EquipSlot
{
	Weapon,
	Offhand,
	Head,
	Chest,
	Legs,
	Boots,
	Gloves,
	Ring1,
	Ring2,
	Earring1,
	Earring2,
	Necklace,
	Cape
}

public enum DivineAffiliation
{
	Zeus,
	Poseidon,
	Hades,
	Nyx,
	Ares,
	Era,
	Hermes,
	Athena
}

public enum DamageType
{
	Physical,
	Magical,
	True
}

public enum BloodVialType
{
	Minor,
	Medium,
	Major,
	Demonic,
	Divine
}

public enum RelicQuality
{
	Bronze = 0,
	Silver = 1,
	Gold = 2,
	Platinum = 3
}

public enum RelicCategory
{
	Constellation,
	Specter,
	Marina
}

public enum DamageElement
{
	None,
	Lightning,
	Water,
	Ice,
	Death,
	Shadow,
	Fire,
	Earth,
	Air,
	Light
}

public enum CraftingProfession
{
	Alchemy,
	Inscription,
	Cooking
}

public enum MapReputationTier
{
	Outsider,
	Acquaintance,
	Friend,
	Ally,
	Respected,
	Protector,
	LocalHero,
	Champion,
	RegionalLegend,
	LivingMyth,
	LocalDemigod,
	TutelaryDeity
}

public enum DungeonType
{
	NormalMap,
	BossMap,
	Instanced,
	Challenge,
	Prestige
}

public enum MobBehavior
{
	NonAggressive,
	Aggressive,
	Thief,
	Boss,
	BossSpecter,
	BossMarina
}

public enum Gender
{
	Male,
	Female
}

public static class QualityExtensions
{
	public static int RuneSlots(this Quality q) => (int)q * 4 + 4;
	public static int EvolutionCostRank(this BloodVialType v) => v switch
	{
		BloodVialType.Minor => 1,
		BloodVialType.Medium => 6,
		BloodVialType.Major => 11,
		BloodVialType.Demonic => 0,
		BloodVialType.Divine => 0,
		_ => 1
	};
}