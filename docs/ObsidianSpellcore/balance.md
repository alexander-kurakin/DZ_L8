# Balance — Spellcore Princess Demo

**Status:** синхронизировано с `LevelConfig` / `*Config.asset` (Epic 8, **balance iter.3.5**)
**Правила и flow:** [[00-one-pager]] — без цифр, только качественные описания  
**Код / конфиги:** значения вносятся сюда после playtest → перенос в `LevelConfig` / `*Config.asset` (не дублировать в плане, one-pager, word doc)

> Demo doc stack = **`00-one-pager` + `balance.md` + `balance-budgets.md`**. Большой GDD `params/` — post-demo. Ищешь цифру — только этот файл. Бюджеты Slack — [[balance-budgets]].

---

## 1. Tower & run

| Параметр          | Значение | Примечание |
| ----------------- | -------- | ---------- |
| Tower max HP      | 2000     | `LevelConfig 1` |
| Tower start HP    | 2000     |            |
| Refund price      | 35%      | `EssenceConfig.PlantSellRefundFraction` |
| Starter free Mine | 2        | after W1; teach W2: O+M на двух путях |
| LMB cooldown      | 5с       | `ExplodeAtPointAbilityConfig` |
| Brother stone throw | **135** dmg / **2.2с** | **Middle + Inner** only |

---

## 2. Economy (Essence)

| Параметр                       | Значение | Примечание |
| ------------------------------ | -------- | ---------- |
| Start Essence (run)            | **0**    |            |
| Essence drop per kill (Cat)    | **24**   |            |
| Essence drop per kill (Tank)   | **52**   |            |
| Essence drop per kill (Dragon) | **100**  |            |
| Tower eat fraction             | **1.0**  | весь дроп в кошелёк |
| Vacuum hover radius            | **2.5**  | `PickupHoverColliderRadius` |

### Plant costs (Essence)

| Plant | Cost | Belt |
| ----- | ---- | ---- |
| Mine | **28** | O / M / I |
| Toxic | **25** | O / M |
| Turret | **100** | O / M / I |

**Экономика W1→W3 (iter.3):** W1 ≈ **60** Essence; после W1 — **1** free mine; к W3 при полном сборе ≈ **220** Essence. Полная доска W5 (18 слотов) только минами ≈ **504** Essence. См. [[balance-budgets]].

---

## 3. Plants — base numbers

| Plant | Параметр | Значение | Примечание |
| ----- | -------- | -------- | ---------- |
| **Mine** | Damage | **150** | 2 proc = 300 на танк 500; LMB/брат добивают |
| | Proc delay | **0.25с** | «short delay» |
| | Radius / sector | **sector** | `ExplosionRadius` 15 — визуал/legacy; урон по сектору |
| **Toxic** | DoT per tick | **35** | |
| | Tick interval | **1с** | 35 DPS на Cat |
| | Slow | **−33%** move speed | `SlowMoveSpeedFraction` 0.33 → `speed × (1 − 0.33)` |
| | Radius / sector | **sector** | O/M only |
| **Turret** | Damage per shot | **100** | inner belt vs dragon |
| | Fire interval | **~0.9с** | 0.5 process + 0.3 delay + 0.1 cooldown |
| | Target arc | N±1 same belt | канон |
| **LMB** | Cat damage | **50% max HP** | ≈ **162** на Cat 325 HP |
| | Tank damage | **20% max HP** | ≈ **100** на Tank 500 HP; flavor toast «броня» |
| | Dragon | 0 | flavor toast |

---

## 4. Enemies — base stats

| Enemy | Belt | Max HP | Move speed | Другое |
| ----- | ---- | ------ | ---------- | ------ |
| Cat | Inner | **325** | **4** | explosion **150** (`ExplodeyCatC` в Intro) |
| Tank | Middle | **500** | **9** | ranged **70** dmg; CD **0.35**; 2×mine(150)+LMB/брат |
| Dragon | Inner | **550** | **6** | beam **30**/tick; ~6 выстрелов турели |

---

## 5. Counter matrix (множители урона)

Заполни только если отличается от канона. `1.0` = полный урон, `0` = нет эффекта.

| Source → / Enemy ↓ | Cat | Tank | Dragon |
| ------------------ | --- | ---- | ------ |
| **Toxic DoT** | 1.0 + slow | 0.5 + slow | 0 (fly over) |
| **Turret** | **1.0** | **0.5** (shield) | 1.0 |
| **Mine** | 1.0 | 1.0 | 1.0 + enrage |
| **LMB** | 50% max HP | **20% max HP** | 0 |

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
| 2 | 2 | **3** | **2** | 0 | 1–1.5с / **пауза 10с** | 2с / 0с | 2×1 tank; окно на минирование 2-го пути |
| 3 | **3** | **3** | **2** | **1** | … | паузы 2–2.5с | **2 группы × 1 танк**, дракон отдельно; `Intro3` |
| 4 | **5** | **5** | **3** | **1** | 0.25–0.55с, mixed groups | 0.4–0.6с | toxic; `Intro4` |
| 5 | **5** | **5** | **5** | **2** | 0.3–0.5с | **0.55–0.65с** | как W4 по путям; без tank+dragon в одной группе; `Intro5` |

**Paths unlock (iter.3):** `[1, 2, 3, 5, 5]` — W4 и W5 на **5 путях** (нет 6-го на финале). Slack-цели: [[balance-budgets#1-определения]].

**Playtest iter.3.2:** снижен leak-DPS танка/дракона; mine **200**, брат **135/2.2с**, cat drop **22**, `Intro3` паузы ↑.

**Playtest iter.3.5:** mine **150**; dragon HP **425→550**.

**Playtest iter.3.6:** dragon move **9→6** — не спринтит как танк; больше окна на Outer/Middle до beam на Inner (playtest: W5 80 HP, комбо tank+dragon).

**Playtest iter.3.7:** W5 **5 путей** (как W4, без 6-го); Intro5: **−1 кот**, tank+dragon **никогда в одной SpawnGroup**, паузы **~0.55–0.65с** (как W4). Итого 5 кот / 5 танк / 2 дракон.

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
