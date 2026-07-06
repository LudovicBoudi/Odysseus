using Godot;
using Odysseus.Managers;

namespace Odysseus.World;

public sealed partial class GameScene : Node3D
{
	public override void _Ready()
	{
		GD.Print($"[GameScene] Loaded. Joueur = {GameStateManager.Instance.Player?.Name ?? "<none>"}");
	}
}