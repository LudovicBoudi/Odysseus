using Godot;
using Odysseus.Combat;
using Odysseus.Entities.Mob;
using Odysseus.Managers;

namespace Odysseus.World;

public sealed partial class GameScene : Node3D
{
	public override void _Ready()
	{
		GD.Print($"[GameScene] Loaded. Joueur = {GameStateManager.Instance.Player?.Name ?? "<none>"}");
		var player = GetNodeOrNull<CharacterBody3D>("Player");
		if (player != null && CombatManager.Instance != null)
		{
			CombatManager.Instance.SetPlayerNode(player);
		}
		BindMobDeaths();
	}

	private void BindMobDeaths()
	{
		foreach (var n in GetTree().GetNodesInGroup("mob"))
		{
			if (n is MobEntity mob)
			{
				mob.Died += OnMobDied;
			}
		}
	}

	private void OnMobDied(MobEntity mob)
	{
		long xp = mob.Level * 25 + mob.MaxHp / 2;
		if (LevelManager.Instance != null)
		{
			LevelManager.Instance.GrantXp(xp);
			GD.Print($"[GameScene] Mob '{mob.DisplayName}' mort +{xp} XP");
		}
		AutoLoot(mob);
	}

	private void AutoLoot(MobEntity mob)
	{
		long gold = mob.Level * 3 + GD.RandRange(0, 5);
		if (GameStateManager.Instance?.Player != null)
		{
			GameStateManager.Instance.Player!.Gold += gold;
			GameStateManager.Instance.Save();
			GD.Print($"[Loot] +{gold} gold");
		}
	}
}