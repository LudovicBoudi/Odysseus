# PHASES — Voxel-Odysseus

Découpage du développement. Chaque phase produit un jalon jouable/testable.
Voir la TODO list (opencode) pour le détail par tâche.

## Phase 0 — Pré-production ✅
Analyse GDD, choix techniques (Godot 4.3 C#/.NET, SQLite, art voxel),
inventaire des assets, validation des décisions structurantes.

## Phase 1 — MVP (en cours)

**Cible** : boucle gameplay minimale jouable sur 1 map tutorielle.

### 1.0 Init projet ✅
- Godot 4.3 .NET + csproj + tests xUnit + AGENTS.md + dev.sh + .gitignore
- Désinstallation Modular Inventory, validation Terrain3D + Asset Placer
- Core types C# : Enums, Attributes + ProgressionFormula (testée)

### 1.1 Documentation ✅
- docs/GDD.md, ARCHITECTURE.md, DATA_MODEL.md, PHASES.md canoniques

### 1.2 Couche SQLite
- Database.cs, migrations, SaveManager load/save auto à `user://save.sqlite`

### 1.4 Création de personnage
- UI choix genre + customization minimal + affiliation divine + save

### 1.5 Starter kit
- 1 item Blanc R1 par slot équipable + 1 de chaque arme + bouclier
- Inventaire 240 slots sans Modular Inventory (C# natif)

### 1.6 Player controller
- ZQSD rebindable via InputManager, collisions, animations procédurales base

### 1.7 Caméra orbit 3ème personne
- Derrière perso par défaut, drag clic gauche rotate angle de vue

### 1.8 Combat tab-target
- Tab/clic lock cible, auto-attack 1.5s/3.8s, HP/Mana, mort + respawn

### 1.9 HUD
- 4 barres ×12 centrées + drag SkillBook + keybind config
- Nameplate joueur (bas-gauche), nameplate cible (bas-droite), minimap (haut-droite)

### 1.10 Map tutorielle
- PNJ TP + PNJ formation + PNJ quêtes + mobs non aggro + auto-loot

### 1.11 Niveau / XP
- formule `floor(100 × 1.12^(lvl-1))` + 5 pts libres + 1 complémentaire / lvl

### 1.12 Tests étendus
- DamageFormula, Attributes, LevelManager, Inventory

### 1.A Shaders MVP
- toon_diffuse.gdshader, outline.gdshader, cosmos_aura.gdshader placeholder
- icones placeholder en ColorRect+label

---

## Phase 2 — Boucle RPG complète

- **2.1** 8 affiliations divines + skill sets par arme × affiliation (offensif/DoT/buff)
- **2.2** Equipement : qualités × 15 rangs + évolution via Fioles de Sang (5 types)
- **2.3** Inscription : runes craftées, capacité 4→32 par qualité, unique par item
- **2.4** Alchimie : fioles de sang + potions soin/mana/buff
- **2.5** Cuisine : plats HoT/HoM + buffs (cumulables avec potions & parchemins)
- **2.6** Banque 400 slots + accès via NPC en ville
- **2.7** 1-2 maps réelles (Terrain3D) : mix mobs + donjon (10/sem, scaling niveau)
- **2.8** Quête journal (touche L) + POIs map (touche M) + quêtes tutoriel
- **2.9** Daily challenges : menu dédié, 30 min, 1×/j, rémunérateur
- **2.10** Boss de map : challenge lvl-equivalent, respawn 12h
- **2.11** Parchemins de buff (Inscription, consommables)
- **2.A** Assets Phase 2 : ~50-80 meshs voxel (player M/F, 8 armes, 5 armures,
  4 bijoux, ~10 mobs, boss, PNJ) + ~150 sprites icones + ~15 SFX + 3 musiques
  + shaders VFX élémentaux (foudre/eau/feu/ténèbres/mort/terre/air/lumière)

---

## Phase 3 — Systèmes passifs & collections

- **3.1** Reliques : interface dédiée ; 3 catégories (constellations/108 spectres/
  7 marinas) ; qualité Bronze→Platine via duplicata ; niveau 1→100 via animas ;
  duplicata ≥5 → animas
- **3.2** Bestiaire : carte unique/mob, drop chance progressive (base +0.5%/kill,
  cap 80%), paliers 50/250/1k/5k bonus
- **3.3** Collections de livres : 3 livres × 30 / map lootés sur mobs ; voleurs
  droppent des livres de collections mythologiques
- **3.4** Réputation par map : panneau de quêtes journalières (25 max/j),
  paliers 1→100, bonus XP/drop/dégâts + hauts faits + titres + fashions
  (cape@20, arme@50, armure@75, aura@100)
- **3.5** Achievements + Titles (stats passives permanentes même si non affiché)
- **3.6** Boutique cosmétique (touche J) : fashions vs gems (jeu uniquement)
- **3.7** Familier "mignon" + évolution
- **3.8** Mob voleur (fleeing) : spawn aléatoire maps & donjons
- **3.9** Fashion pouch 240 slots + onglet Fashion touche C
- **3.A** Assets Phase 3 : +120 meshs boss (108 spectres + 7 marinas + constellations)
  + ~250 sprites (icones reliques/cartes bestiaire/couvertures livres/fashion)
  + familier + voleur + ~10 musiques

---

## Phase 4 — Contenu massif & polish

- **4.1** Contenu massif (Terrain3D + Asset Placer pour level design) :
  Olympe / Enfers / Royaume Poséidon / Forêt Dodone / Marais Styx
  + 108 boss spectres + 7 boss marinas + maps intermédiaires
- **4.2** Équilibrage XP/loot/drop rates/stat curves + tuning prestige 9999
- **4.3** Audio/musique/polish + localisation FR (défaut)
- **4.A** Assets Phase 4 : +150 meshs boss + ~30 maps dressées (heightmaps+props
  voxel) + ~25 musiques + vidéo intro création perso (optionnel)

---

## Inventaire artistique consolidé

| Phase          | Modèles 3D                  | Sprites/UI   | Audio        | Charge totale |
|----------------|-----------------------------|--------------|--------------|---------------|
| 1 (MVP)        | 0 (primitives)              | labels       | silence      | min           |
| 2              | ~50-80 (player, armes, mobs…) | ~150 icones | 3 mus + ~15 SFX | forte        |
| 3              | +120 (boss spectres etc.)  | +250 sprites | +10 mus      | majeure       |
| 4              | +150 (suite boss) + 30 maps | tunings     | +25 mus      | critique      |

L'art final est externe (MagicaVoxel main, packs CC0, ou IA générative pour sprites).
Le code (shaders cel-shaded Saint Seiya, génération procédurale, import .vox → .glb)
est produit par l'agent.