# ASSETS — Inventaire

Bibliothèque d'assets fournie par le joueur. Catégorisée pour intégrration progressive.

## Personnage jouable

| Fichier | Format | Usage |
|---|---|---|
| `Elf_MaleCharacter-Free.fbx` | FBX + texture `Elf_MaleCharacter-Free_0.png` | Mesh + armature du perso. Remplace le capsule placeholder en Phase 1.6/1.A. |
| `Universal Animation Library[Standard]/Unreal-Godot/UAL1_Standard.glb` | GLB | Set d'animations standard (idle, walk, run, attack…) compatibles rig Unreal/Godot. À importer comme AnimationLibrary. |
| `Universal Animation Library[Standard]/Unreal-Godot/UAL1_Standard_RM.glb` | GLB | Variant root-motion. |

## Tenues / armures modulaires

| Dossier | Format | Usage |
|---|---|---|
| `Modular Character Outfits - Fantasy[Standard]/Exports/glTF (Godot-Unreal)/Modular Parts/` | glTF | Pièces d'armure modulaires (casque, plastron, jambières…). Pour le système de fashion/équipement Phase 2.2 / 3.9. |
| `Modular Character Outfits - Fantasy[Standard]/Exports/glTF (Godot-Unreal)/Outfits/` | glTF | Tenues préassemblées. |

## Environnement — Végétation

| Fichier | Format | Usage |
|---|---|---|
| `Dead_Tree.vox`, `Fall_Tree.vox`, `PineFluffy.vox`, `Tree.vox`, `Willow.vox`, `Willow_single_color.vox`, `Winter_Tree.vox` | MagicaVoxel (.vox) | Arbres voxel — parfaits pour map tutorielle et respectent le style voxel du GDD. À importer via convertisseur .vox → .glb. |
| `free-tree-3d-low-poly-pack/` | FBX + textures | Arbres low-poly de complément. |
| `Free Shrubs Flowers and Mushrooms 3D Low Poly Pack/` | FBX + textures | Végétation basse. |

## Environnement — Structures

| Dossier | Format | Usage |
|---|---|---|
| `Medieval Village MegaKit[Standard]/glTF/` | glTF + bin | Kit village médiéval modulaire (balcons, murs, sols, portes, fenêtres). Pour dressage map tutorielle Phase 1.10 et villes Phase 4.1. |
| `Medieval Village MegaKit[Standard]/FBX/` | FBX | Mêmes pièces en FBX. |
| `Medieval Village MegaKit[Standard]/OBJ/` | OBJ + mtl + png | Mêmes pièces en OBJ. |
| `Medieval Town - Free Sample-*` | OBJ + mtl + png | Maison, crate, props médiévaux isolés. |
| `Fantasy Props MegaKit[Standard]/Exports/glTF/` | glTF | Props fantasy (barils, crânes, etc.). |
| Divers FBX (`Door_03`, `DoorFrame_02`, `Floor_Corner_01`, `Armor_01`) | FBX | Props isolés. |

## Environnement — PNJ

| Dossier | Format | Usage |
|---|---|---|
| `Free Medieval 3D People Low Poly Pack/` | FBX + textures | PNJ passifs (villageois) pour map tutorielle et villes. |

## Textures

| Dossier | Format | Usage |
|---|---|---|
| `SBS - Tiny Texture Pack - 512x512/512x512/Bricks/Grass/Roofs/Tile/Wood/` | PNG 512 | Textures sol/murs/toits pour Terrain3D et matériaux. |
| `BAKE_Blocks_DiffuseMap- 4K.png` etc. (assets racine) | PNG 4K | Atlas de bake pour Medieval MegaKit. |

## Musique & ambiances

| Dossier | Format | Usage |
|---|---|---|
| `10 Ambient RPG Tracks/` | mp3 + ogg + wav | 10 thèmes ambient pour maps. |
| `Fantasy Ambiences Music pack/Tracks/` + `Loops/` | mp3 + ogg + wav | Boucles d'ambiance fantasy. |

## Stratégie d'intégration

1. **Phase 1.A / 1.6 upgrade** : remplacer capsule perso par `Elf_MaleCharacter-Free.fbx` + `UAL1_Standard.glb` animations (idle/walk/run).
2. **Phase 1.10** : arbres `.vox` (Dead_Tree, Tree, PineFluffy) scatterés sur map tutorielle + `Free Shrubs` pour végétation basse.
3. **Phase 1.A** : `10 Ambient RPG Tracks` pour musique map tutorielle.
4. **Phase 2.7** : `Medieval Village MegaKit` + `Fantasy Props MegaKit` pour dressage 1-2 maps réelles ; `SBS 512` pour textures Terrain3D.
5. **Phase 2.A** : `Modular Character Outfits - Fantasy` pour système de fashion/armure équipement (remplace les primitives).
6. **Phase 4.1** : suite de `Medieval Village MegaKit` pour dressage Olympe/Enfers/etc. (peut-être teinté céramique pour mythe grec — à déterminer).

## Import / codecs

- Godot 4.3 importe nativement FBX, glTF/GLB, OBJ (via assimp), PNG.
- `.vox` (MagicaVoxel) nécessite un convertisseur → `.glb` ( addon externe, ou script C# MagicaVoxel-Python). À adresser en Phase 1.A.
- Ogg Vorbis recommandé pour la musique (compression, loop).

## Licences

- Vérifier `License.txt` dans chaque sous-dossier avant distribution. Ces packs viennent vraisemblablement d'assets libres (Kenney/Quaternius/etc.) —majority CC0. Le PDF dans `10 Ambient RPG Tracks` précise la licence musique.