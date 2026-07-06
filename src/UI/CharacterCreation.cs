using System;
using System.Globalization;
using System.Linq;
using Godot;
using Odysseus.Core;
using Odysseus.Data;
using Odysseus.Managers;

namespace Odysseus.UI;

public sealed partial class CharacterCreation : Control
{
	[Export] public LineEdit? NameEdit;
	[Export] public OptionButton? GenderOption;
	[Export] public ColorPickerButton? SkinColor;
	[Export] public ColorPickerButton? HairColor;
	[Export] public Label? AffiliationLabel;
	[Export] public Label? Announcement;
	[Export] public Button? ConfirmButton;

	private DivineAffiliation _selectedAffiliation = DivineAffiliation.Zeus;

	public override void _Ready()
	{
		BuildGenderOption();
		BuildAffiliationGrid();
		if (ConfirmButton != null)
		{
			ConfirmButton.Text = "Commencer l'aventure";
			ConfirmButton.Pressed += OnConfirm;
		}
		if (NameEdit != null)
		{
			NameEdit.PlaceholderText = "Nom du Perpétuel";
			NameEdit.MaxLength = 16;
			NameEdit.TextChanged += _ => RefreshButton();
		}
		RefreshButton();
	}

	private void BuildGenderOption()
	{
		if (GenderOption == null) return;
		GenderOption.Clear();
		GenderOption.AddItem("Masculin", (int)Gender.Male);
		GenderOption.AddItem("Féminin", (int)Gender.Female);
		GenderOption.Selected = 0;
	}

	private void BuildAffiliationGrid()
	{
		if (AffiliationLabel == null) return;
		var grid = GetNodeOrNull<GridContainer>("Panel/Margin/V/AffiliationGrid");
		if (grid == null) return;
		foreach (DivineAffiliation aff in Enum.GetValues<DivineAffiliation>().Cast<DivineAffiliation>())
		{
			var btn = new Button
			{
				Text = AffiliationDisplayName(aff),
				CustomMinimumSize = new Vector2(160, 48),
				ToggleMode = true,
			};
			btn.Pressed += () => OnAffiliationSelected(aff, btn);
			if (aff == _selectedAffiliation) btn.ButtonPressed = true;
			grid.AddChild(btn);
		}
	}

	private void OnAffiliationSelected(DivineAffiliation aff, Button pressedBtn)
	{
		_selectedAffiliation = aff;
		var grid = GetNodeOrNull<GridContainer>("Panel/Margin/V/AffiliationGrid");
		if (grid != null)
		{
			foreach (var child in grid.GetChildren())
			{
				if (child is Button b && b != pressedBtn) b.ButtonPressed = false;
			}
		}
	}

	private static string AffiliationDisplayName(DivineAffiliation aff) => aff switch
	{
		DivineAffiliation.Zeus => "Zeus — Foudre",
		DivineAffiliation.Poseidon => "Poséidon — Eau/Glace",
		DivineAffiliation.Hades => "Hadès — Mort",
		DivineAffiliation.Nyx => "Nyx — Ténèbres",
		DivineAffiliation.Ares => "Arès — Feu",
		DivineAffiliation.Era => "Éra — Terre",
		DivineAffiliation.Hermes => "Hermès — Air",
		DivineAffiliation.Athena => "Athéna — Lumière",
		_ => aff.ToString(),
	};

	private void RefreshButton()
	{
		if (ConfirmButton != null)
		{
			bool nameValid = !string.IsNullOrWhiteSpace(NameEdit?.Text);
			ConfirmButton.Disabled = !nameValid;
		}
	}

	private void OnConfirm()
	{
		string name = NameEdit?.Text.Trim() ?? "";
		if (string.IsNullOrEmpty(name) || name.Length > 16)
		{
			ShowAnnouncement("Nom invalide (1-16 caractères).");
			return;
		}
		var gender = (Gender)(GenderOption?.Selected ?? 0);
		string seed = BuildSeed();
		var player = new PlayerData
		{
			Name = name,
			Gender = gender,
			DivineAffiliation = _selectedAffiliation,
			CreationSeed = seed,
			Level = 1,
			Prestige = 0,
			Xp = 0,
			Gold = 100,
			Gem = 0,
			BaseAttributes = BaseAttributes.Zero,
			UnspentBasePoints = 0,
			UnspentSecondaryPoints = 0,
			HpCurrent = 200,
			ManaCurrent = 100,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
		};
		GameStateManager.Instance.CreateNewPlayer(player);
		LoadGameScene();
	}

	private string BuildSeed()
	{
		string skin = SkinColor != null ? ColorToHex(SkinColor.Color) : "FFFFFF";
		string hair = HairColor != null ? ColorToHex(HairColor.Color) : "222222";
		return $"skin={skin};hair={hair}";
	}

	private static string ColorToHex(Color c) =>
		$"{(int)(c.R*255):X2}{(int)(c.G*255):X2}{(int)(c.B*255):X2}";

	private void ShowAnnouncement(string msg)
	{
		if (Announcement != null)
		{
			Announcement.Text = msg;
			Announcement.Modulate = new Color(1f, 0.5f, 0.5f);
		}
	}

	private void LoadGameScene()
	{
		GetTree().ChangeSceneToFile("res://src/World/GameScene.tscn");
	}
}