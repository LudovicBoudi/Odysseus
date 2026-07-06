# AGENTS.md — Voxel-Odysseus

## Stack
- Godot 4.3 .NET (Flatpak `org.godotengine.GodotSharp`)
- C# / .NET 8
- SQLite via `Microsoft.Data.Sqlite` (NuGet)
- Style artistique : voxel (MagicaVoxel export `.glb`), rendu toon/cel-shaded
- Vue : troisième personne, caméra orbit

## Modules tiers
- `addons/terrain_3d/` — Terrain3D 1.0.0 (GDExtension C++, runtime) : terrains GPU clipmap, sculpting, heightmaps, foliage. Activé.
- `addons/asset_placer/` — Godot Asset Placer 1.5.1 (plugin editor, level design Phase 4) : placement d'assets 3D, randomisation, collections. Activé.
- Modular Inventory : **désinstallé** (plugin GDScript non retenu ; on garde C# + SQLite natif pour cohérence).

## Commandes de vérification (À EXÉCUTER APRÈS CHAQUE TÂCHE)
Le CLI `dotnet` n'est pas installé sur l'hôte ; on passe par la sandbox Flatpak GodotSharp via le wrapper `./dev.sh`.

```bash
# Restore NuGet (une fois après modification des csproj)
./dev.sh restore

# Build C# du projet principal (Godot.NET.Sdk via sandbox)
./dev.sh build

# Tests unitaires xUnit (projet tests/Odysseus.Tests.csproj)
./dev.sh test

# Vérification chargement projet Godot (headless editor, quit-after 5)
# Note : le plugin editor asset_placer émet des warnings non fatals en headless, ils sont bénins.
./dev.sh godot-check

# Tout enchaîner
./dev.sh all
```

Commandes flatpak brutes équivalentes (si besoin d'invoquer sans le wrapper) :
```bash
flatpak run --command=dotnet org.godotengine.GodotSharp build <csproj>
flatpak run --command=dotnet org.godotengine.GodotSharp test  <tests.csproj>
flatpak run org.godotengine.GodotSharp --editor --headless --quit-after 5 --path .
```

## Conventions
- Langage par défaut des fichiers/code : **Français** pour les commentaires et noms affichés au joueur (UI, données) ; noms de classes/méthodes/variables en **Anglais** (idiome C#).
- Pas de commentaires dans le code sauf demande explicite.
- Encodage UTF-8, fins de ligne LF.
- Indentation : tabulations (convention Godot).
- Une classe par fichier, nom du fichier = nom de la classe.

## Décisions validées du GDD
- Slots armure : Head / Chest / Legs / Boots / Gloves (5) + Weapon + Offhand(Shield) + Ring1/Ring2 + Earring1/Earring2 + Necklace + Cape.
- Affiliations divines (8) : Zeus (foudre), Poseidon (eau/glace), Hades (mort/vampirisme 10%), Nyx (ténèbres DoT), Ares (feu mixte), Era (terre défensive), Hermes (air offensive), Athena (lumière mixte).
- XP courbe : `next = floor(100 × 1.12^(level-1))`. Niveau 1→100, puis prestige 1→9999 (12 pts/niveau prestige).
- HP/Mana base : 200 HP / 100 Mana au lvl 1, +20 HP / +10 Mana par niveau + bonus Constitution/Intelligence.
- Progression attributs : +5 pts libres +1 complémentaire par niveau. Bonus 20-40: +2, 41-60: +4, 61-80: +6, 81-100: +8, prestige: +12.
- Boutique cosmétique (touche J) : gems gagnées en jeu uniquement (hauts faits + fin donjon), pas de monnaie réelle.
- Inventaire 240 slots, Banque 400 slots, Fashion pouch 240 slots. Pas de loot d'équipement : starter kit complet, évolution via Fioles de Sang.

## Contrôles par défaut
- Déplacement ZQSD (rebindable)
- Ciblage tab-target : Tab / clic souris
- Fiche perso : C ; Quêtes : L ; Map : M ; Boutique : J
- Caméra orbit : maintien clic gauche + déplacement souris (rotation autour du perso)