using Godot;

namespace Odysseus.UI.HUD;

public sealed partial class SkillBarSlot : ColorRect
{
	[Export] public int BarIndex { get; set; }
	[Export] public int SlotIndex { get; set; }
	[Export] public string SkillId { get; set; } = "";
	[Export] public string ShortcutLabel { get; set; } = "";

	private Label? _label;

	public override void _Ready()
	{
		CustomMinimumSize = new Vector2(48, 48);
		Color = new Color(0.15f, 0.15f, 0.2f, 0.85f);
		_label = new Label
		{
			Text = string.IsNullOrEmpty(ShortcutLabel) ? $"{BarIndex + 1}.{SlotIndex + 1}" : ShortcutLabel,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			AnchorsPreset = 15,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill,
		};
		_label.AddThemeFontSizeOverride("font_size", 10);
		AddChild(_label);
	}
}