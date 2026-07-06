using Godot;
using Odysseus.Data;

namespace Odysseus.Managers;

public sealed partial class GameStateManager : Node
{
	public static GameStateManager Instance { get; private set; } = null!;

	public PlayerData? Player { get; private set; }
	public bool HasPlayer => Player != null;

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		Player = PlayerRepository.Load(SaveManager.Instance.Db);
		if (Player != null)
		{
			PlayerRepository.LoadVitals(SaveManager.Instance.Db, Player);
			GD.Print($"[GameState] Joueur chargé : {Player.Name} (lvl {Player.Level}, {Player.DivineAffiliation})");
		}
		else
		{
			GD.Print("[GameState] Aucun joueur — affichage de la création de personnage.");
		}
	}

	public PlayerData CreateNewPlayer(PlayerData newData)
	{
		PlayerRepository.Create(SaveManager.Instance.Db, newData);
		Player = newData;
		GD.Print($"[GameState] Nouveau joueur créé : {Player.Name}");
		return Player;
	}

	public void Save()
	{
		if (Player == null) return;
		PlayerRepository.SaveProgress(SaveManager.Instance.Db, Player);
	}
}