using Godot;
using Odysseus.Combat;
using Odysseus.Entities.Mob;
using Odysseus.Entities.Player;
using Odysseus.Managers;

namespace Odysseus.UI.HUD;

public sealed partial class HUD : CanvasLayer
{
	[Export] public ProgressBar? HpBar;
	[Export] public Label? HpLabel;
	[Export] public ProgressBar? ManaBar;
	[Export] public Label? ManaLabel;
	[Export] public Label? PlayerNameplateName;
	[Export] public Label? PlayerLevelLabel;

	[Export] public Label? TargetNameplateName;
	[Export] public Label? TargetLevelLabel;
	[Export] public ProgressBar? TargetHpBar;
	[Export] public Label? TargetHpLabel;
	[Export] public Control? TargetPanel;

	[Export] public Control? Minimap;
	[Export] public SubViewport? MinimapViewport;
	[Export] public Camera3D? MinimapCamera;

	private PlayerController? _player;
	private MobEntity? _currentMobTarget;
	private float _updateAccum;

	public override void _Ready()
	{
		GD.Print("[HUD] _Ready");
		if (CombatManager.Instance != null)
			CombatManager.Instance.TargetChanged += OnTargetChanged;
		CallDeferred(nameof(BindPlayer));
		TargetPanel?.SetVisible(false);
		BuildSkillBars();
	}

	private void BuildSkillBars()
	{
		var grid = GetNodeOrNull<GridContainer>("Bottom/SkillBars");
		if (grid == null) return;
		grid.Columns = 12;
		for (int bar = 0; bar < 4; bar++)
		{
			for (int slot = 0; slot < 12; slot++)
			{
				var s = new SkillBarSlot { BarIndex = bar, SlotIndex = slot };
				grid.AddChild(s);
			}
		}
	}

	private void BindPlayer()
	{
		var gs = GetTree().CurrentScene;
		_player = gs?.GetNodeOrNull<PlayerController>("Player");
		if (_player != null && PlayerNameplateName != null && GameStateManager.Instance?.Player != null)
		{
			PlayerNameplateName.Text = GameStateManager.Instance.Player!.Name;
		}
		if (_player != null && MinimapCamera != null)
		{
			MinimapCamera.GlobalPosition = _player.GlobalPosition + new Vector3(0, 20, 0);
		}
	}

	private void OnTargetChanged(Node3D? target)
	{
		_currentMobTarget = target as MobEntity;
		if (_currentMobTarget != null)
		{
			TargetPanel?.SetVisible(true);
			if (TargetNameplateName != null) TargetNameplateName.Text = _currentMobTarget.DisplayName;
			if (TargetLevelLabel != null) TargetLevelLabel.Text = $"Niv. {_currentMobTarget.Level}";
		}
		else
		{
			TargetPanel?.SetVisible(false);
		}
	}

	public override void _Process(double delta)
	{
		_updateAccum += (float)delta;
		if (_updateAccum < 0.1f) return;
		_updateAccum = 0;
		if (_player != null)
		{
			if (HpBar != null) HpBar.MaxValue = _player.MaxHp;
			if (HpBar != null) HpBar.Value = _player.CurrentHp;
			if (HpLabel != null) HpLabel.Text = $"{_player.CurrentHp}/{_player.MaxHp}";
			if (ManaBar != null) ManaBar.MaxValue = _player.MaxMana;
			if (ManaBar != null) ManaBar.Value = _player.CurrentMana;
			if (ManaLabel != null) ManaLabel.Text = $"{_player.CurrentMana}/{_player.MaxMana}";
			if (PlayerLevelLabel != null) PlayerLevelLabel.Text = GameStateManager.Instance?.Player != null ? $"Niv. {GameStateManager.Instance.Player!.Level}" : "";
			if (MinimapCamera != null && MinimapCamera.IsInsideTree())
			{
				MinimapCamera.GlobalPosition = _player.GlobalPosition + new Vector3(0, 20, 0);
			}
		}
		if (_currentMobTarget != null && GodotObject.IsInstanceValid(_currentMobTarget))
		{
			if (TargetHpBar != null) TargetHpBar.MaxValue = _currentMobTarget.MaxHp;
			if (TargetHpBar != null) TargetHpBar.Value = _currentMobTarget.CurrentHp;
			if (TargetHpLabel != null) TargetHpLabel.Text = $"{_currentMobTarget.CurrentHp}/{_currentMobTarget.MaxHp}";
		}
		else if (_currentMobTarget != null)
		{
			OnTargetChanged(null);
		}
	}
}