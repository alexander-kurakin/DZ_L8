# Balance — Spellcore Princess Demo

**Status:** синхронизировано с `LevelConfig` / `*Config.asset` (Epic 8, **balance iter.3**)  
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
| Brother stone throw | **95** dmg / **3с** | **Middle + Inner** belt; nearest enemy |

---

## 2. Economy (Essence)

| Параметр                       | Значение | Примечание |
| ------------------------------ | -------- | ---------- |
| Start Essence (run)            | **0**    |            |
| Essence drop per kill (Cat)    | **20**   |            |
| Essence drop per kill (Tank)   | **50**   |            |
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
| **Mine** | Damage | **190** | 2 proc = 380; танк 500 HP → добивает брат |
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
| | Tank / Dragon | 0 | flavor toasts |

---

## 4. Enemies — base stats

| Enemy | Belt | Max HP | Move speed | Другое |
| ----- | ---- | ------ | ---------- | ------ |
| Cat | Inner | **325** | **4** | explosion dmg **500** к башне |
| Tank | Middle | **500** | **9** | ranged dmg 100; **2 mine + брат**; stop at Middle belt anchor |
| Dragon | Inner | **425** | **9** | beam 40/tick; fly over toxic; 2 mines + добивание |

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
| 2 | 2 | **3** | **2** | 0 | 1–1.5с / **пауза 3с** | 2с / 0с | **2 группы × 1 танк** = разные пути; teach O+M; `Intro2` |
| 3 | **3** | **3** | **2** | **1** | … | паузы 2–2.5с | **2 группы × 1 танк**, дракон отдельно; `Intro3` |
| 4 | **5** | **5** | **3** | **1** | 0.25–0.55с, mixed groups | 0.4–0.6с | toxic; `Intro4` |
| 5 | 6 | **6** | **5** | **2** | 0.15–0.45с, 5 groups | 0.25–0.45с | `Intro5` |

**Paths unlock (iter.3):** `[1, 2, 3, 5, 6]` — 6 paths только на W5. Slack-цели: [[balance-budgets#1-определения]].

**Playtest iter.3 → iter.3.1:** композиция `Intro2`/`Intro3` (1 танк = 1 spawn group = 1 путь); mine **190**, танк **500 HP**, брат **95**, free mines **2**. См. [[balance-budgets#13-композиция-волн-spawn-groups]].

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
