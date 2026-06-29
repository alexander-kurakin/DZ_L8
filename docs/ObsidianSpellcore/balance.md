# Balance — Spellcore Princess Demo

**Status:** синхронизировано с `LevelConfig` / `*Config.asset` (Epic 8)  
**Правила и flow:** [[00-one-pager]] — без цифр, только качественные описания  
**Код / конфиги:** значения вносятся сюда после playtest → перенос в `LevelConfig` / `*Config.asset` (не дублировать в плане, one-pager, word doc)

> Demo doc stack = **`00-one-pager` + `balance.md`**. Большой GDD `params/` — post-demo. Ищешь цифру — только этот файл.

---

## 1. Tower & run

| Параметр          | Значение | Примечание |
| ----------------- | -------- | ---------- |
| Tower max HP      | 2000     | `LevelConfig 1` |
| Tower start HP    | 2000     |            |
| Refund price      | 35%      | `EssenceConfig.PlantSellRefundFraction` |
| Starter free Mine | 2        | after W1; `SpellcoreProgressionConfig` |
| LMB cooldown      | 5с       | `ExplodeAtPointAbilityConfig` |

---

## 2. Economy (Essence)

| Параметр                       | Значение | Примечание |
| ------------------------------ | -------- | ---------- |
| Start Essence (run)            | **0**    |            |
| Essence drop per kill (Cat)    | **25**   |            |
| Essence drop per kill (Tank)   | **60**   |            |
| Essence drop per kill (Dragon) | **125**  |            |
| Tower eat fraction             | **1.0**  | весь дроп в кошелёк |
| Vacuum hover radius            | **2.5**  | `PickupHoverColliderRadius` |

### Plant costs (Essence)

| Plant | Cost | Belt |
| ----- | ---- | ---- |
| Mine | **10** | O / M / I |
| Toxic | **25** | O / M |
| Turret | **100** | O / M / I |

**Экономика W1→W3 (ориентир):** W1 ≈ 75 Essence; после W1 — 2 free mines; к W3 при полном сборе ≈ 270+ Essence → хватает на 1–2 платных растения без паники.

---

## 3. Plants — base numbers

| Plant | Параметр | Значение | Примечание |
| ----- | -------- | -------- | ---------- |
| **Mine** | Damage | **250** | 2× на танка (500 HP); sector-based |
| | Proc delay | **0.25с** | «short delay» |
| | Radius / sector | **sector** | `ExplosionRadius` 15 — визуал/legacy; урон по сектору |
| **Toxic** | DoT per tick | **35** | |
| | Tick interval | **1с** | 35 DPS на Cat |
| | Slow | **−33%** move speed | `SlowMoveSpeedFraction` 0.33 → `speed × (1 − 0.33)` |
| | Radius / sector | **sector** | O/M only |
| **Turret** | Damage per shot | **100** | inner belt vs dragon |
| | Fire interval | **~0.9с** | 0.5 process + 0.3 delay + 0.1 cooldown |
| | Target arc | N±1 same belt | канон |
| **LMB** | Cat damage | **50% max HP** | канон; не от current — иначе не убить |
| | Tank / Dragon | 0 | flavor toasts |

---

## 4. Enemies — base stats

| Enemy | Belt | Max HP | Move speed | Другое |
| ----- | ---- | ------ | ---------- | ------ |
| Cat | Inner | **300** | **4** | explosion dmg **500** к башне |
| Tank | Middle | **500** | **9** | ranged dmg 100; stop at **Middle belt anchor** (~35) via `StopAtMiddleBeltAnchor` |
| Dragon | Inner | **400** | **9** | beam 40/tick; fly over toxic; 2 mines or inner turret |

---

## 5. Counter matrix (множители урона)

Заполни только если отличается от канона. `1.0` = полный урон, `0` = нет эффекта.

| Source → / Enemy ↓ | Cat | Tank | Dragon |
| ------------------ | --- | ---- | ------ |
| **Toxic DoT** | 1.0 + slow | 0.5 + slow | 0 (fly over) |
| **Turret** | **1.0** | **0.5** (shield) | 1.0 |
| **Mine** | 1.0 | 1.0 | 1.0 + enrage |
| **LMB** | 50% max HP | 0 | 0 |

### Особые правила (не множители)

| Правило | Значение |
| ------- | -------- |
| Dragon enrage per mine hit | **+50%** outgoing damage (stack) |
| Dragon enrage cap (demo) | **2 stacks** | max outgoing ×2; `DragonEnrageConfig.MaxEnrageStacks` |
| Tank shield depletion | out of scope | post-demo |

---

## 6. Waves 1–5 — composition

Скопируй структуру из [[00-one-pager#5. Waves 1–5]]; здесь — **числа** (count, timing, HP scale).

| Wave | Paths total | Cat count | Tank count | Dragon count | Spawn interval | Group pause | Notes |
| ---- | ----------- | --------- | ---------- | ------------ | -------------- | ----------- | ----- |
| 1 | 1 | **3** | 0 | 0 | 4–5с | — | teach LMB; `Intro1` |
| 2 | 2 | **3** | **2** | 0 | 1–1.5с / 1.5–2с | 2с | 2 mines vs tank; `Intro2` |
| 3 | 4 | **3** | **2** | **1** | 0.8–1.2 / 1–1.5с | 1.5–2с | inner turret/mine vs dragon; `Intro3` |
| 4 | 6 | **5** | **3** | **1** | 0.6–1 / 0.8–1.2с | 1.5–2с | intro toxic; `Intro4` |
| 5 | 6 | **6** | **5** | **2** | 0.4–1.5с (ускорение) | 1–1.5с | full factory; `Intro5` |

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
| Essence bailout | Tower HP < 20% на входе в prep | **+100 Essence** (1× за prep-фазу); `EssenceConfig` |
