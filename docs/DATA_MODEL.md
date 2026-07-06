# DATA_MODEL — Voxel-Odysseus

Schéma SQLite de persistance. Source de vérité pour la couche `src/Data/`.
DB file : `user://save.sqlite` (un seul perso jouable — jeu solo).

## Conventions

- PK `id INTEGER PRIMARY KEY AUTOINCREMENT` sauf indication.
- FK `INTEGER REFERENCES table(id) ON DELETE CASCADE`.
- Timestamps ISO8601 TEXT.
- JSON sérialisé en TEXT pour données flexibles (runes, loots tables, attributs bonus).

## Tables — Player & progression

### player
Champ            | Type      | Notes
---|---|---
id               | INTEGER PK| 1 (singleton)
name             | TEXT NOT NULL |
gender           | TEXT NOT NULL | 'Male' / 'Female' (enum Gender)
divine_affiliation| TEXT NOT NULL | enum DivineAffiliation
creation_seed    | TEXT | paramètres customization (teint, cheveux, visage...)
level            | INTEGER NOT NULL DEFAULT 1 |
prestige         | INTEGER NOT NULL DEFAULT 0 |
xp               | INTEGER NOT NULL DEFAULT 0 |
gold             | INTEGER NOT NULL DEFAULT 0 |
gem              | INTEGER NOT NULL DEFAULT 0 |
base_attributes  | TEXT NOT NULL | JSON {Constitution, Force, Dexterity, Intelligence, Spirit}
unspent_base_pts | INTEGER NOT NULL DEFAULT 0 |
unspent_secondary_pts | INTEGER NOT NULL DEFAULT 0 |
created_at       | TEXT NOT NULL |
updated_at       | TEXT NOT NULL |

### player_vitals (cached/computed)
Champ            | Type
---|---
player_id        | INTEGER PK REFERENCES player(id)
hp_current       | INTEGER NOT NULL
mana_current     | INTEGER NOT NULL

### skills_owned
skill_id TEXT PK REFERENCES skill_template(id)
unlocked INTEGER NOT NULL DEFAULT 1
level    INTEGER NOT NULL DEFAULT 1

## Tables — Equipement & Inventaire

### item_template (data statique seed)
id TEXT PK
name TEXT NOT NULL
kind TEXT NOT NULL            -- 'Weapon', 'Armor', 'Jewelry', 'Cape', 'Shield', 'Consumable', 'Material', 'Fashion', 'Rune', 'BloodVial', 'Anima', 'Book'
equip_slot TEXT               -- enum EquipSlot nullable
weapon_type TEXT              -- enum WeaponType nullable
attack_speed REAL             -- 1.5 / 3.8 / null
quality INTEGER NOT NULL      -- enum Quality 0-7
rank INTEGER NOT NULL DEFAULT 1 -- 1-15
base_stats TEXT NOT NULL      -- JSON {Constitution, Force, ...}
attack_bonus INTEGER DEFAULT 0
defense_bonus INTEGER DEFAULT 0
sellable INTEGER NOT NULL DEFAULT 1
stackable INTEGER NOT NULL DEFAULT 1
icon_path TEXT
description TEXT

### inventory_item (instances)
id INTEGER PK
item_id TEXT NOT NULL REFERENCES item_template(id)
quality INTEGER NOT NULL      -- qualité courante (après évolution)
rank INTEGER NOT NULL         -- rang courant
slot_index INTEGER NOT NULL   -- 0-239
qty INTEGER NOT NULL DEFAULT 1
runes TEXT                    -- JSON array of rune_item ids engraved

### bank_item   -- identique structure, 400 slots
### fashion_item -- idem, 240 slots
### equipment_slot
slot TEXT PK                  -- enum EquipSlot
inventory_item_id INTEGER REFERENCES inventory_item(id)
fashion_item_id INTEGER REFERENCES fashion_item(id)

## Tables — Runes & enchantements

### rune_template
id TEXT PK
name TEXT NOT NULL
stat_id TEXT NOT NULL         -- attribut affecté (Force, Defense, CriticalChance, ...)
stat_value INTEGER NOT NULL   -- valeur bonus
inscription_level_req INTEGER NOT NULL

### rune_graved
-- pas d'instance ; mémorisée dans `inventory_item.runes` JSON

## Tables — Skills

### skill_template
id TEXT PK
name TEXT NOT NULL
weapon_type TEXT              -- nullable pour skills universels
divine_affiliation TEXT       -- nullable
skill_type TEXT NOT NULL      -- 'Active', 'Passive', 'AutoAttack'
damage_type TEXT NOT NULL     -- enum DamageType
damage_element TEXT NOT NULL
damage_mult REAL
cooldown REAL
mana_cost INTEGER
description TEXT

## Tables — Combat & Mobs

### mob_template
id TEXT PK
name TEXT NOT NULL
level INTEGER NOT NULL
hp INTEGER NOT NULL
mana INTEGER
behavior TEXT NOT NULL        -- enum MobBehavior
base_damage INTEGER
base_defense INTEGER
aggression_range REAL
loot_table_id TEXT
is_boss INTEGER DEFAULT 0

### mob_loot_table
id TEXT PK
entries TEXT NOT NULL         -- JSON [{item_id, chance, qty_min, qty_max}]

### mob_kill_count
player_id INTEGER NOT NULL
mob_id TEXT NOT NULL
kills INTEGER NOT NULL DEFAULT 0
PRIMARY KEY (player_id, mob_id)

## Tables — Reliques

### relic_template
id TEXT PK
name TEXT NOT NULL
category TEXT NOT NULL        -- 'Constellation', 'Specter', 'Marina'
source_boss TEXT NOT NULL
passive_bonus_per_level TEXT NOT NULL -- JSON

### relic_owned
id INTEGER PK
relic_id TEXT NOT NULL REFERENCES relic_template(id)
quality INTEGER NOT NULL DEFAULT 0  -- 0 Bronze, 1 Argent, 2 Or, 3 Platine
level INTEGER NOT NULL DEFAULT 1
dupes_count INTEGER NOT NULL DEFAULT 1

### anima_inventory
player_id INTEGER NOT NULL
type TEXT NOT NULL            -- 'Minor', 'Medium', 'Major', 'Demonic', 'Divine'
qty INTEGER NOT NULL DEFAULT 0
PRIMARY KEY (player_id, type)

## Tables — Bestiaire

### bestiary_card_owned
player_id INTEGER NOT NULL
mob_id TEXT NOT NULL
obtained INTEGER NOT NULL DEFAULT 1
no_drop_kills INTEGER NOT NULL DEFAULT 0  -- reset quand carte obtenue
PRIMARY KEY (player_id, mob_id)

## Tables — Maps & réputation

### map_template
id TEXT PK
name TEXT NOT NULL
type TEXT NOT NULL           -- 'Tutorial', 'Open', 'Dungeon', 'Challenge'
level_min INTEGER NOT NULL
level_max INTEGER NOT NULL
reputation_enabled INTEGER NOT NULL DEFAULT 1
terrain_heightmap TEXT
scene_path TEXT NOT NULL

### map_reputation
player_id INTEGER NOT NULL
map_id TEXT NOT NULL
reputation INTEGER NOT NULL DEFAULT 0
dailies_done_today INTEGER NOT NULL DEFAULT 0
last_daily_date TEXT
PRIMARY KEY (player_id, map_id)

## Tables — Donjons & boss

### dungeon_clear
player_id INTEGER NOT NULL
dungeon_id TEXT NOT NULL
week_index INTEGER NOT NULL
clears INTEGER NOT NULL DEFAULT 0
PRIMARY KEY (player_id, dungeon_id, week_index)

### boss_respawn
map_id TEXT NOT NULL
boss_id TEXT NOT NULL
next_respawn_at TEXT
PRIMARY KEY (map_id, boss_id)

## Tables — Quests

### quest_template
id TEXT PK
name TEXT NOT NULL
description TEXT NOT NULL
map_id TEXT
prereq_quest TEXT
giver_npc TEXT
objective_type TEXT
objective_target TEXT
objective_qty INTEGER
reward_gold INTEGER
reward_xp INTEGER
reward_item TEXT
reward_fashion TEXT

### quest_progress
player_id INTEGER NOT NULL
quest_id TEXT NOT NULL
state TEXT NOT NULL            -- 'Available', 'Active', 'Completed', 'TurnedIn'
progress INTEGER NOT NULL DEFAULT 0
PRIMARY KEY (player_id, quest_id)

## Tables — Challenges

### daily_challenge_log
player_id INTEGER NOT NULL
date TEXT NOT NULL
challenge_id TEXT NOT NULL
completed INTEGER NOT NULL DEFAULT 0
duration_sec INTEGER
PRIMARY KEY (player_id, date, challenge_id)

## Tables — Achievements & Titres

### achievement_template
id TEXT PK
name TEXT NOT NULL
description TEXT
category TEXT
critère TEXT                 -- JSON décrivant le trigger
reward_type TEXT
reward_id TEXT

### achievement_progress
player_id INTEGER NOT NULL
achievement_id TEXT NOT NULL
progress INTEGER NOT NULL DEFAULT 0
unlocked INTEGER NOT NULL DEFAULT 0
PRIMARY KEY (player_id, achievement_id)

### title_template
id TEXT PK
name TEXT NOT NULL
description TEXT
passive_stats TEXT NOT NULL    -- JSON
source TEXT                   -- achievement / reputation / etc

### title_owned
player_id INTEGER NOT NULL
title_id TEXT NOT NULL
equipped INTEGER NOT NULL DEFAULT 0
PRIMARY KEY (player_id, title_id)

## Tables — Collections de livres

### book_template
id TEXT PK
name TEXT NOT NULL
collection_id TEXT NOT NULL
type TEXT NOT NULL             -- 'Map' ou 'Mythology'
qty_required INTEGER NOT NULL DEFAULT 30

### book_owned
player_id INTEGER NOT NULL
book_id TEXT NOT NULL
qty INTEGER NOT NULL DEFAULT 0
PRIMARY KEY (player_id, book_id)

## Tables — Familier

### familiar
id INTEGER PK
familiar_type TEXT NOT NULL
stage INTEGER NOT NULL DEFAULT 0
level INTEGER NOT NULL DEFAULT 1

## Tables — Keybinds

### keybind
action TEXT PK                  -- ex: 'move_forward', 'skill_slot_1_3'
keycode TEXT NOT NULL           -- ex: 'Z'
modifiers TEXT                  -- ex: 'Ctrl'

## Tables — Boutique cosmétique

### gem_shop_item
id TEXT PK
name TEXT NOT NULL
cost_gem INTEGER NOT NULL
fashion_item_id TEXT REFERENCES item_template(id)

## Vues / indexes recommandés

- `idx_inventory_slot ON inventory_item(slot_index)`
- `idx_mob_kill_player ON mob_kill_count(player_id)`
- `idx_quest_progress_player ON quest_progress(player_id, state)`

## Stratégie de migration

DB version stockée dans `meta(key='schema_version')`. Au boot:
1. `Database.Open()` ouvre/initialise la DB.
2. Lit `schema_version`.
3. Apply migrations `001_init.sql`, `002_xxx.sql` ... dans l'ordre.
4. Sans version : crée toutes les tables (latest).