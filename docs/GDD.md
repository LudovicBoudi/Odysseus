# GDD — Voxel-Odysseus

Document canonique de design. Source de vérité pour toutes les implémentations.
Toute dérive doit être validée puis reportée ici.

## 1. Pitch

RPG 3D solo, vue troisième personne, isekai gréco-romain inspiré de l'hypermythe
de Kurumada (Saint Seiya). Le joueur incarne un **Perpétuel** ( référence Warhammer 40k) :
un être réapparu dans ce monde, non natif, capable d'évoluer jusqu'au statut de divinité
inférieure en atteignant le niveau 100. Au-delà, il progresse par **prestige**
(1 → 9999) à raison de 12 pts/niveau prestige.

Pas de classe, pas de trinité tank/heal/dps. Les skills dépendent de l'arme équipée
et de l'affiliation divine choisie à la création du personnage.

## 2. Affiliations divines (8) — type de magie

| Dieu    | Élément         | Orientation                                   |
|---------|-----------------|----------------------------------------------|
| Zeus    | Foudre          | Offensif — dégâts directs                    |
| Poseidon| Eau / Glace     | Offensif — dégâts + contrôle                 |
| Hades   | Mort            | DoT + vampirisme (10% soins)                 |
| Nyx     | Ténèbres        | DoT                                          |
| Ares    | Feu             | Mixte — dégâts + DoT                         |
| Era     | Terre           | Buff défensif                                |
| Hermes  | Air             | Buff offensif                                |
| Athena  | Lumière         | Mixte — buff                                 |

## 3. Attributs

**Base** (1 pt libre = 1 pt dans un attribut base) :
- Constitution → HP + défense
- Force → dégâts physiques
- Dextérité → précision, évasion, critique physique
- Intelligence → dégâts magiques, mana, critique magique
- Esprit → régénération HP/Mana

**Complémentaires** (dérivés ou sur équipements) :
Attaque, Défense, Chance critique, Protection critique, Bonus critique.

## 4. Progression

- XP courbe : `next = floor(100 × 1.12^(level-1))`. Niveau 1→100, puis prestige 1→9999.
- Par niveau : **+5 pts libres** + **+1 pt complémentaire** (auto).
  - Bonus de pts libres par paliers : 20-40: +2, 41-60: +4, 61-80: +6, 81-100: +8, prestige: +12.
- HP/Mana base : 200 HP / 100 Mana au lvl 1, +20 HP / +10 Mana par niveau
  + bonus Constitution/Intelligence.
- Pas de respec précisée — défaut : respec libre hors combat.

## 5. Équipement

**Slots** (13) : Weapon, Offhand(Shield), Head, Chest, Legs, Boots, Gloves,
Ring1, Ring2, Earring1, Earring2, Necklace, Cape.

**Types d'armes** (8) :
Sword1H, Axe1H, Mace1H, Sword2H, Axe2H, Mace2H, Staff, Spear.

- 1H : attack speed 1.5s, peut équiper bouclier en Offhand.
- 2H : attack speed 3.8s, plus de dégâts de base. Plus lent = plus fort.

**Qualités** (8) : Blanc → Vert → Bleu → Violet → Orange → Jaune → Rose → Rouge.
Chaque qualité a 15 rangs. Le starter kit est **Blanc Rang 1**.

**Capacité de runes** (Inscription) par qualité :
4 (Blanc), 8, 12, 16, 20, 24, 28, 32 (Rouge). Une rune = une gravure unique par item.

**Évolution** : via Fioles de Sang (pas or ni matériaux hors sang) :
- Sang Mineur  → Rangs 1-5, drop zones 1-25
- Sang Moyen   → Rangs 6-10, zones 26-50
- Sang Majeur  → Rangs 11-15, zones 51-80
- Sang Démoniaque → passage de qualité (ex: Blanc R15 → Vert R1), zones 81-100
- Sang Divin   → Jaune → Rose → Rouge et Rangs 13-15 Rouge, zones prestige

**Pas de loot d'équipement** : starter kit complet à la création, évolution via sang.
Équipement received Non vendable, Incassable (pas de durabilité).

## 6. Combat

- Tab-target : Tab ou clic souris pour lock cible. Pas de visée libre.
- Auto-attack selon attack speed de l'arme (1.5s 1H / 3.8s 2H).
- Skills dirigés automatiquement vers cible verrouillée.
- Loot auto à la mort du mob → inventaire.

## 7. Inventaire & containers

- Inventaire perso : 240 slots.
- Banque : 400 slots (accès via NPC en ville).
- Fashion pouch : 240 slots (séparé).
- Reliques : **interface dédiée** (pas d'inventaire).

## 8. Fashion / Cosmétiques

- Onglet "Fashion" dans la fiche perso (touche **C**) : surcharge visuelle des pièces équipées par les pièces fashion. Fashion = 0 stats.
- Source des fashions : quêtes, succès, événements saisonniers, boutique cosmétique (touche **J**).
- **Boutique (J)** : monnaie = **gems** gagnées en jeu uniquement (hauts faits + fin de donjon). Pas de monnaie réelle.

## 9. Métiers de production (craft)

Tous appris dès la création, level 0. Recettes auto-apprises au niveau up.
Ingrédients lootés sur mobs. Pas de récolte. Pas de zone dédiée — craft partout.

| Métier       | Produits                                                        |
|--------------|----------------------------------------------------------------|
| Alchimie     | Fioles de sang, potions soin/mana/buff                         |
| Inscription  | Runes (gravure), parchemins de buff                            |
| Cuisine      | Plats HoT/HoM + buffs (cumulables avec potions & parchemins)   |

## 10. Reliques

Artéfacts passifs (3 catégories) :
- A. **Constellations** (mythologie grecque, ~12+ majeures)
- B. **108 Étoiles Démoniaques** (Spectres d'Hadès)
- C. **7 Marinas de Poséidon**

Drop par les boss d'instances → interface dédiée (pas inventaire).
Toute relique possédée confère stats passives en permanence, sans limite de nombre.

**Qualité par duplicata** :
- 1 drop    → Bronze, niveau 1
- 2 drops   → Argent
- 3 drops   → Or
- 4 drops   → Platine
- 5+ drops  → converti en Animas

**Upgrade de niveau** : consommation d'Animas (XP exponentiel).
- Niveau 1→2 : 100 XP (~10 Animas Mineures)
- Niveau 50→51 : 5000 XP
- Niveau 99→100 : 50 000 XP

Stats : attributs base aux premiers niveaux, attributs complémentaires aux niveaux supérieurs.

## 11. Bestiaire

Carte de bestiaire : drop unique par type de mob (jamais 2 fois).
- Drop chance progressive : `base + kills_sans_drop × 0.5%` cap 80%.
  Boss base 15%, commun base 5%.
- Drop direct dans bestiaire (pas inventaire) + notification + popup illustré.
- Paliers de kills bonus : 50 (+1% dmg), 250 (+2% dmg +1% drop), 1000 (+3/+2), 5000 (+5/+3).

## 12. Collections de livres

Par map : 3 livres × 30 exemplaires lootés sur mobs. Completion = bonus de stats.
Voleurs (fleeing mobs) droppent des livres de collections mythologiques.

## 13. Map & Réputation

Maps instanciées (pas open world). Transfert par NPC de téléportation.

**Map de départ** : tutoriel, NPC + PNJ formation, mobs non agressifs.
**Autres maps** : NPC quêtes + mix agressifs/non-agressifs + donjons.

**Réputation par map** (paliers 1→100, panneau quotidien, 25 quêtes/j max/perso) :
1-4 Étranger ; 5-9 Connaissance ; 10-14 Ami ; 15-19 Allié ; 20-24 Respecté ; 25-29 Protecteur ;
30-39 Héros local ; 40-49 Champion ; 50-59 Légende régionale ; 60-79 Mythe vivant ;
80-99 Demi-dieu local ; 100 Divinité tutélaire.
Bonus map : +% XP, +% drop, +% dégâts. Récompenses : titres + hauts faits + fashions
(cape@20, arme@50, armure@75, aura@100).

## 14. Donjons, Boss & Challenges

- **Donjons** : scalent au niveau du joueur si > niveau du donjon. Loot adapté.
  Répétables 10 fois/semaine par perso.
- **Boss de map** : challenge pour joueur de même level, respawn 12h après kill.
- **Daily challenge** : menu dédié (pas une map). 30 minutes max, 1 fois/j.
  Très rémunérateur : XP, gem, gold, ressources.

## 15. Hauts faits & titres

Hauts faits : traquent accomplissements. Récompense : titre / item / fashion / or / animas.
Titres : confèrent stats passives permanentes dès l'obtention, indépendamment du titre affiché.

## 16. Mob voleur

Pop aléatoire maps & donjons. N'attaque pas, fuit si attaqué (vitesse raisonnable,
doit rester tuable).

## 17. Familier "mignon"

Familier aquérable, évolutif. Détails à spécifier.

## 18. Monnaies

- **Gold** : monnaie de base (drop mobs, quêtes, etc.).
- **Gem** : monnaie premium en jeu uniquement.

## 19. Contrôles par défaut

| Action              | Touche     |
|---------------------|------------|
| Déplacement          | ZQSD       |
| Ciblage tab-target  | Tab / clic souris |
| Caméra orbit         | clic gauche maintenu + déplacement souris |
| Fiche perso          | C          |
| Quêtes               | L          |
| Map                  | M          |
| Boutique cosmétique  | J          |

Tous rebindables in-game via interface de configuration.

## 20. Vue & caméra

Troisième personne. Par défaut, caméra dans le dos du perso.
Drag clic gauche = rotation de l'angle de vue autour du perso, pas déplacement caméra.

Minimap en haut à droite de l'écran.

HUD :
- 4 barres de compétences × 12 slots, alignement center, en bas écran.
- Skills placés depuis Skill Book. Chaque slot assignable à une combinaison clavier.
- Nameplate joueur : au-dessus des barres, décalé à gauche.
- Nameplate ennemi ciblé : au-dessus des barres, décalé à droite.