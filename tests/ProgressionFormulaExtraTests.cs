using System;
using System.IO;
using Xunit;
using Odysseus.Core;
using Odysseus.Data;

namespace Odysseus.Tests;

public class ProgressionFormulaExtraTests
{
	[Theory]
	[InlineData(1, 100)]
	[InlineData(2, 112)]
	[InlineData(10, 277)]
	[InlineData(50, 25803)]
	[InlineData(100, 7457345)]
	public void XpForLevel_NextLevelFloor(int lvl, long expected)
	{
		long actual = ProgressionFormula.XpForLevel(lvl);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void XpForLevel_MonotonicAscending()
	{
		long prev = 0;
		for (int i = 1; i <= 100; i++)
		{
			long v = ProgressionFormula.XpForLevel(i);
			Assert.True(v > prev, $"XP must increase monotonically at lvl {i}");
			prev = v;
		}
	}

	[Fact]
	public void VitalStatsForLevel_100_WithConstitution20_NotZero()
	{
		var v = ProgressionFormula.VitalStatsForLevel(100, 20, 15);
		Assert.Equal(200 + 99 * 20 + 200, v.MaxHp);
		Assert.Equal(100 + 99 * 10 + 75, v.MaxMana);
	}

	[Fact]
	public void BonusPointsForLevel_Lvl20to40_Returns2()
	{
		Assert.Equal(2, ProgressionFormula.BonusPointsForLevel(20, 0));
		Assert.Equal(2, ProgressionFormula.BonusPointsForLevel(40, 0));
	}

	[Fact]
	public void BonusPointsForLevel_Prestige_Returns12()
	{
		Assert.Equal(12, ProgressionFormula.BonusPointsForLevel(25, 1));
		Assert.Equal(12, ProgressionFormula.BonusPointsForLevel(9999, 1));
	}

	[Fact]
	public void Quality_RuneSlots_AllQualitiesMatchGdd()
	{
		Assert.Equal(4, Quality.White.RuneSlots());
		Assert.Equal(32, Quality.Red.RuneSlots());
	}

	[Fact]
	public void DivineAffiliation_HasEightValues()
	{
		Assert.Equal(8, Enum.GetValues<DivineAffiliation>().Length);
	}

	[Fact]
	public void WeaponType_HasEightValues()
	{
		Assert.Equal(8, Enum.GetValues<WeaponType>().Length);
	}

	[Fact]
	public void EquipSlot_HasThirteenValues()
	{
		Assert.Equal(13, Enum.GetValues<EquipSlot>().Length);
	}
}