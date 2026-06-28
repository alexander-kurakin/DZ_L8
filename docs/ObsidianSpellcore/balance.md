# Balance — Spellcore Princess Demo

**Status:** единственный источник **всех чисел** для demo-среза  
**Правила и flow:** [[00-one-pager]] — без цифр, только качественные описания  
**Код / конфиги:** значения вносятся сюда после playtest → перенос в `LevelConfig` / `*Config.asset` (не дублировать в плане, one-pager, word doc)

> Demo doc stack = **`00-one-pager` + `balance.md`**. Большой GDD `params/` — post-demo. Ищешь цифру — только этот файл.

---

## 1. Tower & run

| Параметр          | Значение | Примечание |
| ----------------- | -------- | ---------- |
| Tower max HP      | 2000     |            |
| Tower start HP    | 2000     |            |
| Refund price      | 35%      |            |
| Starter free Mine | 2        | after W1   |
| LMB cooldown      | 5с       |            |

---

## 2. Economy (Essence)

| Параметр                       | Значение | Примечание |
| ------------------------------ | -------- | ---------- |
| Start Essence (run)            |          |            |
| Essence drop per kill (Cat)    |          |            |
| Essence drop per kill (Tank)   |          |            |
| Essence drop per kill (Dragon) |          |            |
| Tower eat fraction             |          |            |
| Vacuum hover radius            |          |            |

### Plant costs (Essence)

| Plant | Cost | Belt |
| ----- | ---- | ---- |
| Mine | TBD | O / M / I |
| Toxic | TBD | O / M |
| Turret | TBD | O / M / I |

---

## 3. Plants — base numbers

| Plant | Параметр | Значение | Примечание |
| ----- | -------- | -------- | ---------- |
| **Mine** | Damage | **250** | 2× на танка (500 HP); sector-based |
| | Proc delay | **0.25с** | «short delay» |
| | Radius / sector | TBD | sector-based в demo |
| **Toxic** | DoT per tick | TBD | |
| | Tick interval | TBD | |
| | Slow | **−33%** move speed | канон |
| | Radius / sector | TBD | O/M only |
| **Turret** | Damage per shot | **100** | inner belt vs dragon |
| | Fire interval | TBD | |
| | Target arc | N±1 same belt | канон |
| **LMB** | Cat damage | **50% max HP** | канон; не от current — иначе не убить |
| | Tank / Dragon | 0 | flavor toasts |

---

## 4. Enemies — base stats

| Enemy | Belt | Max HP | Move speed | Другое |
| ----- | ---- | ------ | ---------- | ------ |
| Cat | Inner | **300** | **4** | explosion dmg TBD |
| Tank | Middle | **500** | **9** | ranged dmg 100, range 20 |
| Dragon | Inner | **400** | **9** | beam 40/tick; fly over toxic; 2 mines or inner turret |

---

## 5. Counter matrix (множители урона)

Заполни только если отличается от канона. `1.0` = полный урон, `0` = нет эффекта.

| Source → / Enemy ↓ | Cat | Tank | Dragon |
| ------------------ | --- | ---- | ------ |
| **Toxic DoT** | 1.0 + slow | 0.5 + slow | 0 (fly over) |
| **Turret** | TBD | **0.5** (shield) | 1.0 |
| **Mine** | 1.0 | 1.0 | 1.0 + enrage |
| **LMB** | 50% max HP | 0 | 0 |

### Особые правила (не множители)

| Правило | Значение |
| ------- | -------- |
| Dragon enrage per mine hit | **+50%** outgoing damage (stack) |
| Dragon enrage cap (demo) | TBD | max stacks или once per dragon |
| Tank shield depletion | out of scope | post-demo |

---

## 6. Waves 1–5 — composition

Скопируй структуру из [[00-one-pager#5. Waves 1–5]]; здесь — **числа** (count, timing, HP scale).

| Wave | Paths total | Cat count | Tank count | Dragon count | Spawn interval | Group pause | Notes |
| ---- | ----------- | --------- | ---------- | ------------ | -------------- | ----------- | ----- |
| 1 | 1 | **3** | 0 | 0 | 4–5с | — | teach LMB |
| 2 | 2 | **3** | **2** | 0 | 1–1.5с / 1.5–2с | 2с | 2 mines vs tank |
| 3 | 4 | **3** | **2** | **1** | см. Intro3 | 1.5–2с | inner turret/mine vs dragon |
| 4 | 6 | **5** | **3** | **1** | см. Intro4 | 1.5–2с | intro toxic |
| 5 | 6 | **6** | **5** | **2** | см. Intro5 | 1–1.5с | full factory |

**Difficulty scaling:** open Paths ↑ → больше направлений / микс (без per-sector % eff).

---

## 7. Survival (post-W5)

| Параметр | Значение | Примечание |
| -------- | -------- | ---------- |
| Paths unlocked | 6 (fixed) | не открываем новые |
| Difficulty ramp | TBD | per wave index |
| Tower hunger / upkeep | TBD | optional Epic 4.3 |
| Reward scaling | TBD | после W4 polish |

---

## 8. Playtest targets

| Метрика | Target |
| ------- | ------ |
| Newbie full run | 5–15 min |
| Skilled 5 waves | ~5 min |
| Skilled + survival | +~10 min optional |
| Wave 3 fun check | проходимо 1–2 plant без react |

---

## 9. Cheats (стенд)

| Cheat | Условие | Действие |
| ----- | ------- | -------- |
| Essence bailout | Tower HP < 20% | +TBD Essence |
