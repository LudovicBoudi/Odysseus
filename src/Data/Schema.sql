-- Schema SQLite — Voxel-Odysseus (migration initiale 001)
-- DB path: user://save.sqlite (singleton perso)

PRAGMA foreign_keys = ON;
PRAGMA journal_mode = WAL;

CREATE TABLE IF NOT EXISTS meta (
  key   TEXT PRIMARY KEY,
  value TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS player (
  id                      INTEGER PRIMARY KEY CHECK (id = 1),
  name                    TEXT    NOT NULL,
  gender                  TEXT    NOT NULL CHECK (gender IN ('Male', 'Female')),
  divine_affiliation      TEXT    NOT NULL,
  creation_seed           TEXT    NOT NULL DEFAULT '',
  level                   INTEGER NOT NULL DEFAULT 1 CHECK (level >= 1),
  prestige               INTEGER NOT NULL DEFAULT 0 CHECK (prestige >= 0),
  xp                      INTEGER NOT NULL DEFAULT 0 CHECK (xp >= 0),
  gold                    INTEGER NOT NULL DEFAULT 0 CHECK (gold >= 0),
  gem                     INTEGER NOT NULL DEFAULT 0 CHECK (gem  >= 0),
  base_attributes         TEXT    NOT NULL DEFAULT '{}',
  unspent_base_pts        INTEGER NOT NULL DEFAULT 0,
  unspent_secondary_pts   INTEGER NOT NULL DEFAULT 0,
  current_map_id          TEXT,
  created_at              TEXT    NOT NULL,
  updated_at              TEXT    NOT NULL
);

CREATE TABLE IF NOT EXISTS player_vitals (
  player_id    INTEGER PRIMARY KEY REFERENCES player(id) ON DELETE CASCADE,
  hp_current   INTEGER NOT NULL DEFAULT 200,
  mana_current INTEGER NOT NULL DEFAULT 100
);

CREATE TABLE IF NOT EXISTS keybind (
  action    TEXT PRIMARY KEY,
  keycode   TEXT NOT NULL,
  modifiers TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS item_template (
  id            TEXT PRIMARY KEY,
  name          TEXT    NOT NULL,
  kind          TEXT    NOT NULL,
  equip_slot    TEXT,
  weapon_type   TEXT,
  attack_speed  REAL,
  quality       INTEGER NOT NULL DEFAULT 0,
  rank          INTEGER NOT NULL DEFAULT 1,
  base_stats    TEXT    NOT NULL DEFAULT '{}',
  attack_bonus  INTEGER NOT NULL DEFAULT 0,
  defense_bonus INTEGER NOT NULL DEFAULT 0,
  sellable      INTEGER NOT NULL DEFAULT 1,
  stackable     INTEGER NOT NULL DEFAULT 1,
  icon_path     TEXT,
  description   TEXT    NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS inventory_item (
  id        INTEGER PRIMARY KEY AUTOINCREMENT,
  item_id   TEXT    NOT NULL REFERENCES item_template(id),
  quality   INTEGER NOT NULL DEFAULT 0,
  rank      INTEGER NOT NULL DEFAULT 1,
  slot_index INTEGER NOT NULL DEFAULT 0,
  qty       INTEGER NOT NULL DEFAULT 1,
  runes     TEXT    NOT NULL DEFAULT '[]',
  container TEXT    NOT NULL DEFAULT 'Inventory' CHECK (container IN ('Inventory', 'Bank', 'Fashion'))
);

CREATE INDEX IF NOT EXISTS idx_inventory_container_slot ON inventory_item(container, slot_index);

CREATE TABLE IF NOT EXISTS equipment_slot (
  slot              TEXT PRIMARY KEY,
  inventory_item_id INTEGER REFERENCES inventory_item(id) ON DELETE SET NULL,
  fashion_item_id   INTEGER REFERENCES inventory_item(id) ON DELETE SET NULL
);