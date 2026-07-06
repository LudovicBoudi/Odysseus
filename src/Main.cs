using Godot;
using Odysseus.Managers;

namespace Odysseus;

public sealed partial class Main : Control
{
	public override void _Ready()
	{
		CallDeferred(nameof(RouteToEntryScene));
	}

	private void RouteToEntryScene()
	{
		if (GameStateManager.Instance != null && GameStateManager.Instance.HasPlayer)
		{
			GetTree().ChangeSceneToFile("res://src/World/GameScene.tscn");
		}
		else
		{
			GetTree().ChangeSceneToFile("res://src/UI/CharacterCreation.tscn");
		}
	}
}