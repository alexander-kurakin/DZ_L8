# Balance v3 — factory / idle defense

**Status:** черновик · **ветка** `Spellcore_v3`  
**Контракт:** [[factory-feel#12. Направление v2 — урон **только** от построек]] · [[plan-v3]]  
**Legacy:** [[balance]] — не трогаем при v3-итерациях, только сверка «что выключили»

> Цифры ниже — **целевые** для эпиков 2–9. До реализации эпика помечено *(TBD / Epic N)*.  
> Код на Epic 1: `CombatModel.FactoryV3` — брат и LMB **не наносят** урон врагам.

---

## 1. Combat model (Epic 1 + v3 feel pass)

| Параметр | Legacy (`master`) | Factory v3 |
| -------- | ----------------- | ---------- |
| Brother stone throw | 135 dmg / 2.2с | **Off** |
| LMB enemy damage | Cat 50% / Tank 20% max HP | **Off** (Epic 6: баф постройки) |
| Урон врагам в бою | plants + family DPS | **только plants** |
| Mine W1 | 1 proc / враг, 150 dmg | **Пульсы** пока враг в секторе |
| Move speed | asset as-is | **×0.7** (было ×0.35) |
| Mine пульс | таймер | см. **Mine pulse rules** ниже |

### Mine v3 (`FactoryV3CombatConfig`)

| Параметр | Значение |
| -------- | -------- |
| Урон за пульс | **115** |
| Пульсов на проход сектора | **3** |
| Tank 1-й mine pulse | **×0.15** (`TankFirstMinePulseDamageMultiplier`) — одна mine ≈247 dmg, нужна вторая |
| Toxic slow (FactoryV3) | **−50%** move speed (`ToxicSlowMoveSpeedFraction`) |
| Toxic placement (FactoryV3) | **Outer only** |
| LMB building buff | **×1.5** dmg, **60 с**, max **2** active, **10** essence |
| Кошка стоп | **Inner anchor** (`StopAtInnerBeltAnchor`, r=12.6) |
| Танк стоп | **Middle anchor** (`StopAtMiddleBeltAnchor`, r=35.1) |
| Выход / смерть в секторе | недоставленные пульсы **добиваются** |
| Трекинг | **per-enemy**; клин: `CollectEnemiesInMineSector` |

### Mine pulse rules

| Враг | Пояс | Тики |
| ---- | ---- | ---- |
| Танк | Outer | 3 × **прогресс** (33 / 66 / 92%) |
| Танк | Middle (финал) | **вход** → **стоп** → **3-й таймер** |
| Кошка | Outer, Middle | 3 × прогресс |
| Кошка | Inner (финал) | вход → стоп → 3-й таймер |

### Прогрессия v3

| Когда | Unlock |
| ----- | ------ |
| Prep W1 | Mine + **1 free mine** |
| После W1 | +2 free mines (legacy starter) |
| После W2 | Turret |
| После W3 | Toxic |

---

## 2. Tower integrity (Epic 2)

| Параметр | Значение | Примечание |
| -------- | -------- | ---------- |
| Max integrity hits | **200** | `TowerIntegrityConfig._maxHits`; вместо HP 2000 |
| Cat leak (Inner взрыв) | **−1** hit | `CatExplosionHits`; source `ExplosionDamage` |
| Tank shot (Middle) | **−2** hits | `TankShotHits`; source projectile `BodyContactDamage` |
| Dragon beam tick (Inner) | **−1** hit | `DragonBeamTickHits`; source `DamagePerTick` / `DragonEnrageStackCount` |
| Fallback leak | **−1** hit | неизвестный source |

UI: счётчик **current/max** (напр. `180/200`), bar = ratio. Код: `EntityHealthPresenter` + `TowerIntegrityTakeDamageSystem`.

HUD kill counter: один `IconTextView` — убитые враги с начала забега (`RunEnemyKillCounterService`); вместо Win/Loss в gameplay HUD. Main menu — Win/Loss без изменений.

---

## 3. Plants — v3 роли

| Plant | Параметр | Старт (из legacy .asset) | Цель v3 | Эпик |
| ----- | -------- | ------------------------- | ------- | ---- |
| **Mine** | Damage | 150 | W1: **1 mine = все коты пути** | 3 |
| | Proc delay | 0.25с | без изменений | — |
| | Tank shield | — | 1-й pulse **×0.15** (Epic 3) | 3 |
| **Toxic** | DoT / slow | 35/tick, −33% speed | **−50%** slow, **Outer only**, дракон immune | 5 ✓ |
| **Turret** | Damage / interval | 100 / ~0.9с | path-wide **O/M/I** на своей спице, приоритет air; **×0.5** cat/tank | 4 ✓ |
| **LMB** | — | cooldown 5с | баф +50% plant (`BuildingBuffDamageMultiplier` 1.5), 60 с, max 2 | 6 ✓ |

### Plant costs (Essence)

Без изменений до playtest v3: Mine **28**, Toxic **25**, Turret **100**.

---

## 4. Enemies — v3 роли

| Enemy | Belt | HP (legacy) | Move speed | v3 заметка |
| ----- | ---- | ----------- | ---------- | ---------- |
| Cat | Inner | 325 | **↓ глобально** (Epic 3) | конвейер, не мини-босс |
| Tank | Middle | 500 | **↓** | щит mine: нужна **2-я** mine на пути (Epic 3) |
| Dragon | Inner | 550 | 6 | **FlyingEnemy**: sector O/M → Inner; mine/toxic **×0**; только турель (Epic 4 ✓) |

---

## 5. Counter matrix (v3 target)

| Source → / Enemy ↓ | Cat | Tank | Dragon |
| ------------------ | --- | ---- | ------ |
| **Mine** | 1.0 | 1.0; 1-й pulse **×0.15** | **0** (Epic 4 ✓) |
| **Toxic** | 1.0 + slow | 0.5 + slow | **0** |
| **Turret** | 0.5 | 0.5 | **1.0** (единственный kill) |
| **LMB** | — | — | — (баф plant, Epic 6) |
| **Brother** | — | — | — (ремонт, Epic 7) |

---

## 6. Waves — teach ladder (v3)

| Wave | Unlock | Gate |
| ---- | ------ | ---- |
| W1 | Mine | 1 mine на пути, без LMB/брата |
| W2 | Tank + 2nd mine | две mines на одном пути |
| W3 | Turret + Dragon | без турели на спице с драконом — leak integrity |
| W4 | Toxic | Outer slow-шоу |
| W5 | Full factory | фикс-план путей, LMB-баф на плотные пакеты |

Composition/timing — переписываются в эпиках 3, 5, 8. **Intro3 (Epic 4):** 3 cats (5–7 с) → pause 5 → **1 dragon** (teach: mine бесполезна, нужна турель на спице).

---

## 7. Family (v3)

| Роль | Параметр | Цель |
| ---- | -------- | ---- |
| Принцесса (LMB) | Баф plant | +50% damage, 60 с, max 2 active — `FactoryV3CombatConfig` |
| Брат | Ремонт integrity | **+1 hit / 6 с** при движении, в радиусе **18** от башни; idle **прерывает** (Epic 7) |
| Брат stone DPS | — | **выключен** (Epic 1) |

---

## 8. Playtest шкала

См. [[factory-feel#6. Анти-паттерны]]: **≤ 2.5** на W4, **≤ 3** на W5 (30 с боя, 1 = смотреть · 5 = жопа).

---

## Changelog

| Дата | Эпик | Изменение |
| ---- | ---- | --------- |
| 2026-06-30 | 1 | Файл создан; legacy DPS off в коде (`CombatModel.FactoryV3`) |
| 2026-06-30 | 1+ | Mine pulse v3, move ×0.7, mine W1 + free mine, Intro1 pacing |
| 2026-07-01 | 2 | Tower integrity: `TowerIntegrityConfig`, leak resolver, UI current/max |
| 2026-07-01 | 4+ | Turret same-cell dmg, dragon priority refresh, mine wedge collect |
| 2026-07-01 | 1+ | Mine stop-belt vs full-crossing rules; cat `StopAtInnerBeltAnchor` |
| 2026-07-01 | 6+ | LMB essence cost, buff timer UI, building buff juice |
| 2026-07-01 | 7 | Brother repair: config + `TowerBrotherRepairSystem` (draft) |
