using Godot;
using Odysseus.Combat;
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
	}
}