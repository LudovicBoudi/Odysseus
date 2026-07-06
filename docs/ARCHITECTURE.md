# ARCHITECTURE — Voxel-Odysseus

Source: docs/GDD.md. Ce document décrit l'organisation du code, les modules
tiers, les autoloads, et les conventions d'implémentation.

## Stack
- Godot 4.3 .NET (Flatpak `org.godotengine.GodotSharp`).
- C# / .NET 8 (`Godot.NET.Sdk 4.3.0`).
- SQLite via `Microsoft.Data.Sqlite` (NuGet 8.0.2) — persistance locale.
- Tests xUnit dans `tests/Odysseus.Tests.csproj` (logique pure, sans Godot).
- Art voxel MagicaVoxel (export `.glb`), rendu toon/cel-shaded.

## Modules tiers

### Terrain3D 1.0.0 — `addons/terrain_3d/`
- GDExtension C++ (runtime), accessible depuis C#.
- Terrains GPU clipmap 64m → 65.5 km, 32 textures, heightmaps, sculpting, foliage.
- **Usage** : chaque map instanciée peut hoster un `Terrain3D` node.
  Heightmaps dressés en Phase 4. Pas requis pour le MVP (placeholder PlaneMesh ok).
- Activé dans `project.godot`.

### Godot Asset Placer 1.5.1 — `addons/asset_placer/`
- Plugin editor (pas runtime). Placement click + random + collections.
- **Usage** : level design Phase 4 (dressage maps avec props voxel). Aucun runtime.
- Activé dans `project.godot`.
- Note : émet des warnings non fatals en mode `--headless --editor` (bénins).

### Modular Inventory 2.0.0 — **DÉSINSTALLÉ**
- Plugin GDScript très complet (drag&drop, hotbars, equipment manager).
- Retenu non retenu : architecture C#/.NET + SQLite natif préféré pour un RPG massif.
  Bridge GDScript ↔ C#↔SQLite aurait complexifié sans gain net.
- Code inventaire codé from scratch en C# (UI + persistance SQLite unifiée).

## Arborescence

```
Odysseus/
├── project.godot          # config Godot, autoloads, plugins enabled
├── Odysseus.csproj        # Godot.NET.Sdk 4.3.0, references Microsoft.Data.Sqlite
├── dev.sh                 # wrapper sandboxed flatpak (restore/build/test/godot-check/all)
├── AGENTS.md              # conventions + commandes de vérification
├── docs/                  # GDD, ARCHITECTURE, DATA_MODEL, PHASES
├── data/                  # JSON/SQL seed (items, mobs, reliques, recettes, achievements, ...)
├── artifacts/             # assets finaux (models/, sprites/, audio/, shaders/)
├── addons/                # Terrain3D, Asset Placer
├── src/
│   ├── Core/              # types valeur & enums (Attributes, Elements, Quality, WeaponType, ...)
│   ├── Data/              # Database.cs, Schema.sql, SaveManager, Repos
│   ├── Managers/          # ~22 autoloads singletons
│   ├── Entities/          # Player/, Mob/, Npc/, Boss/, Thief/, Familiar/
│   ├── Combat/            # TabTarget, Skill, DamageFormula
│   ├── Crafting/          # Alchemy, Inscription, Cooking
│   ├── World/             # Maps, Dungeons, MapInstance, LevelScaler
│   ├── Camera/            # ThirdPersonOrbit
│   └── UI/                # HUD/, Windows/, Shaders/
└── tests/                 # xUnit project (compile les fichiers Core en lien)
```

## Autoloads (managers)

Déclarés dans `project.godot` → `[autoload]`. Singletons accessibles globalement.

| Autooad              | Rôle                                                    |
|---------------------|----------------------------------------------------------|
| GameStateManager    | struct joueur courant (level, prestige, attr, location)  |
| SaveManager          | load/save SQLite, autosave                                |
| InventoryManager    | inventaire 240 slots                                      |
| BankManager         | banque 400 slots                                          |
| FashionPouchManager | pouch 240                                                 |
| EquipmentManager    | 13 slots equipables + fashion mirror                      |
| SkillManager        | skills par arme × affiliation, skillbook                  |
| CombatManager       | tab-target, lock, aggro table, DoT                        |
| LevelManager        | XP, pts libres/complémentaires, prestige                  |
| RelicManager        | qualité Bronze→Platine (duplicata), niveau 1→100 (animas) |
| BestiaryManager     | carte unique + drop chance progressive + paliers kills    |
| ReputationManager   | par map, paliers 1→100, 25 dailies max/j                  |
| QuestManager        | journal + POIs map                                        |
| CraftingManager     | Alchemy/Inscription/Cooking, recettes auto-déblocables     |
| AchievementManager  | hauts faits tracking + récompenses                        |
| TitleManager        | titres possédés, stats passives (cumul)                   |
| CurrencyManager     | gold + gem                                                |
| DungeonManager      | scaling niveau, 10 clears/sem, lockout                    |
| ChallengeManager    | daily challenge 1/j 30 min                                |
| TeleportManager     | PNJ TP inter-maps                                         |
| InputManager        | keybinds rebindables + persistence                        |
| HUDManager          | orchestrer HUD (nameplates, skillbars, minimap)          |
| NotificationManager | toasts (loot, niveau, bestiaire, achievement)             |

## Schéma d'execution

1. Boot : Godot lance autoloads dans l'ordre.
2. `SaveManager` ouvre la DB SQLite (`user://save.sqlite`), applique migrations.
3. `GameStateManager` load player (ou montre écran char creation si aucun).
4. `InputManager` charge keybinds depuis SQLite (ou défauts).
5. `DungeonManager` / `ReputationManager` / `BestiaryManager` chargent leur état.
6. Scène `WorldMap` active spawn mobs + NPCs + PNJ TP selon `map_id`.
7. Loop gameplay → mutations d'état → `SaveManager.Autosave()` périodique.

## Conventions de code

- C# / .NET 8. `Nullable` enable.
- namespaces : `Odysseus.<Subdir>...` (ex: `Odysseus.Core`, `Odysseus.Managers`).
- Une classe par fichier, nom de fichier = nom de classe.
- Pas de commentaires sauf demande explicite.
- Tabulations, UTF-8, LF.
- Français pour affichage joueur (UI, données, descriptions) ; Anglais pour code.
- Tests xUnit dans `tests/*.cs` ; české Pure domain logic seulement (pas de Node Godot).

## Workflow de vérification

Exécuter après chaque tâche via `./dev.sh` :
1. `./dev.sh build` → compilation C# via `Godot.NET.Sdk`.
2. `./dev.sh test` → tests xUnit.
3. `./dev.sh godot-check` → import resources + validate scripts/scènes en headless.
   Warnings Asset Placer non fatals.
4. `./dev.sh all` → enchaîne les 3.