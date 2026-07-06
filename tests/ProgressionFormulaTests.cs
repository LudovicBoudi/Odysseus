using Xunit;
using Odysseus.Core;

namespace Odysseus.Tests;

public class ProgressionFormulaTests
{
	[Theory]
	[InlineData(1, 100)]
	[InlineData(2, 112)]
	[InlineData(10, 277)]
	[InlineData(50, 25803)]
	[InlineData(99, 6658343)]
	public void XpForLevel_MatchesExpectedCurve(int level, long expected)
	{
		long actual = Odysseus.Core.ProgressionFormula.XpForLevel(level);
		Xunit.Assert.Equal(expected, actual);
	}

	[Fact]
	public void VitalStatsForLevel_Level1Base_Returns200HP100Mana()
	{
		var v = Odysseus.Core.ProgressionFormula.VitalStatsForLevel(1, 0, 0);
		Xunit.Assert.Equal(200, v.MaxHp);
		Xunit.Assert.Equal(100, v.MaxMana);
	}

	[Fact]
	public void VitalStatsForLevel_Level50WithConstitutionBonus_IncreasesHp()
	{
		int con = 20;
		var v = Odysseus.Core.ProgressionFormula.VitalStatsForLevel(50, con, 0);
		Xunit.Assert.Equal(200 + 49 * 20 + 200, v.MaxHp);
	}

	[Theory]
	[InlineData(1, 0, 0)]
	[InlineData(20, 0, 2)]
	[InlineData(40, 0, 2)]
	[InlineData(41, 0, 4)]
	[InlineData(80, 0, 6)]
	[InlineData(81, 0, 8)]
	[InlineData(99, 0, 8)]
	[InlineData(50, 1, 12)]
	public void BonusPointsForLevel_MatchesTierRules(int level, int prestige, int expected)
	{
		int actual = Odysseus.Core.ProgressionFormula.BonusPointsForLevel(level, prestige);
		Xunit.Assert.Equal(expected, actual);
	}

	[Theory]
	[InlineData(Odysseus.Core.Quality.White, 4)]
	[InlineData(Odysseus.Core.Quality.Green, 8)]
	[InlineData(Odysseus.Core.Quality.Blue, 12)]
	[InlineData(Odysseus.Core.Quality.Purple, 16)]
	[InlineData(Odysseus.Core.Quality.Orange, 20)]
	[InlineData(Odysseus.Core.Quality.Yellow, 24)]
	[InlineData(Odysseus.Core.Quality.Pink, 28)]
	[InlineData(Odysseus.Core.Quality.Red, 32)]
	public void RuneSlots_MatchGddTableForAllQualities(Odysseus.Core.Quality q, int expected)
	{
		Xunit.Assert.Equal(expected, q.RuneSlots());
	}
}