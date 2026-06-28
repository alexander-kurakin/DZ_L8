# Spellcore Princess — Demo One-Pager (Meetup)

**Status:** draft 
**Scope:** 5 waves + optional Survival 
**Meetup:** screenshot + **pitch:** Unveil Spellcore secrets by feeding your magic tower

**Links:** [[pillars]] · [[essence]] · [[world/spellcore]] · [[systems/spellcore-gameplay]]  · [[balance]]


---
## 1. Design pillars

Full text: [[pillars]]

| Pillar          | Demo means                                                                                           |
| --------------- | ---------------------------------------------------------------------------------------------------- |
| **Family**      | Brother run↔emote; Princess follows cursor; HP bar under platform                                    |
| **Less clicks** | Few plants per wave; watch combat; LMB on cooldown; sell refund is rare - [[balance#1. Tower & run]] |
| **Secrets**     | Spellcore reveals rules on the field; peak = 6 Paths working as a factory                            |

---

## 2. Core loop

**Diagram:**
![[Pasted image 20260622000949.png]]

Between iterations: spend Essence to fill unlocked Path slots, read wave preview. 
In combat: mostly watch; sometimes LMB or sell→buy to react

**Sub-loops (one line each):**
Economy
![[Pasted image 20260622003350.png]]

Idle LMB  
![[Pasted image 20260622003437.png]]

Wave details
![[Pasted image 20260622003654.png]]

React
![[Pasted image 20260622003716.png]]

## 3. Tutorial unlockable flow
### Secrets ladder (S1-S9)

| #   | Secret               | Reveal                                                    | Milestone  |
| --- | -------------------- | --------------------------------------------------------- | ---------- |
| S1  | Sectors + LMB        | 1 Path, spawn arrow                                       | pre Wave 1 |
| S2  | Plant layer (Mine)   | Path +1, free Mine (starter) - [[balance#1. Tower & run]] | After W1   |
| S3  | Essence / feed tower | hover vacuum, eat VFX                                     | W2         |
| S4  | Threat read          | wave preview                                              | W2 prep    |
| S5  | Belt rules           | toxic O/M only; etc.                                      | W2–3       |
| S6  | Layered factory      | outer → mid → inner                                       | W3–5       |
| S7  | Counters             | cat / tank / dragon                                       | W2–4       |
| S8  | Sell / react         | sell refund — [[balance#1. Tower & run]]                  | W3+        |
| S9  | Factory peak         | 6 Paths                                                   | Wave 5     |
### Abilities ladder

| When         | Unlocks                                               |
| ------------ | ----------------------------------------------------- |
| Wave 1       | LMB only (plant bar hidden)                           |
| After W1 win | Mine + free Mine starter - [[balance#1. Tower & run]] |
| After W2 win | Turret                                                |
| After W3 win | Toxic                                                 |
| W4-W5        | All available                                         |

---
## 4. Rules

**Grid:** 4 belts × 16 sectors (Spawn / Outer / Middle / Inner). 
Path N activates index N on all belts. Details: [[world/spellcore]]

**Plants:** 
Toxic O/M 
Turret+Mine O/M/I 
Turret hits N±1 same belt 
Mine = Hit every enemy exactly once after a short delay
Toxic = DoT

**LMB:** low effectiveness, cooldown - [[balance#1. Tower & run]]

**Enemies:** (множители и урон - [[balance#5. Counter matrix]], статы - [[balance#4. Enemies]])

**Cat** (Inner, emote + explosion)  
Countered by Mine, LMB, Toxic. Toxic slows and deals full DoT. LMB hurts cats significantly.

**Tank** (Middle, shoots from there)  
Countered by Turret, Mine, Toxic. 
Partial toxic/turret due to shield. 
LMB ineffective ("Aw no, spells do not penetrate armor").

**Dragon** (Inner, emote + continuous DoT)  
Countered by Turret, Mine. 
Flies over toxic. 
Mine detonations enrage dragon (stronger outgoing damage). 
LMB ineffective ("Aw no, magical defense is too high").

**Essence resource:** corpse drops loot→ hover over loot→ loot flies to tower -> tower eats essence

---
## 5. Waves 1–5

| Wave | Tools in fight              | Preview             | Spawn path(s) | Notes                                  |
| ---- | --------------------------- | ------------------- | ------------- | -------------------------------------- |
| 1    | LMB                         | cat                 | 1 random      | need to teach player                   |
| 2    | Mine + LMB                  | cat + tank          | + 1 random    | shows how 2 mines in a row kills tanks |
| 3    | Mine + Turret + LMB         | cat + tank + dragon | + 2 random    | shows turrets + enraged dragon         |
| 4    | Mine + Turret + LMB + Toxic | same                | +2 random     | intro of toxic                         |
| 5    | same                        | same                | 0 random      | full blown wave                        |

---
## 6. Demo bounds

**Conditions:**
Lose = Tower HP = 0.
Win = здоровье башни не должно опуститься до 0, мы должны пройти 5 волн.
После 5 волны - экран победы с кнопкой сурвайвал мода (продолжается с тем же количеством открытых путей, но с увеличивающейся сложностью и наградами) - survival режим настроить после W4 полировки.

**HUD:**

| Элемент                  | Demo значение                                                                                       |
| ------------------------ | --------------------------------------------------------------------------------------------------- |
| Валюта (геймплей и мета) | Essence - геймплей, Coins - мета (out of scope - enemy model parts e.g. cat/tank/dragon 0/10 parts) |
| Счетчик волн             | иконка замка + цифры 0/5                                                                            |
| Счетчик убийств          | общий счетчик убийств = иконка черепка                                                              |
| Plant bar                | bottom center, не слева (так было уже раньше в проекте)                                             |
| Wave preview             | Стрелка на активных путях. Иконки типов врагов                                                      |
**Митап:** - нужно приносить свой лэптоп - проверить билд на маке жены.

**Timing:** [[balance#8. Playtest targets]]

**Out of scope:**
* Champion monsters (blue / red) - extra toughness + extra loot
* Clothing meta-progression - нужны предметы, мета система, магазин
* Tank shield depletion
* Different levels / bioms / eenemy types
* Brother active role - animation or abilities (можно сделать ЛКМ = сестра бегает за мышкой - брат анимируется просто - и мы наносим урон по сектору, ПКМ = брат бегает за мышкой - сестра анимируется просто - и он например засасывает лут - или только он может продавать итд)

**Numbers:** [[balance]]


