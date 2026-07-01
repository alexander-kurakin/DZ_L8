# Spellcore v3 — план (idle / factory defense)

**Ветка:** `Spellcore_v3` (от `master`)  
**Status:** утверждённое направление — **ва-банк**, legacy combat не дожимаем  
**Связь:** [[factory-feel]] · [[00-one-pager]] · [[pillars]] · [[balance]] · **[[balance-v3]]**

---

## 0. Зачем v3

| Версия | Что было | Почему не то |
| ------ | -------- | ------------ |
| **v1** | Клик по полю → враги умирают | Активный кликер, нет Spellcore / построек |
| **v2** (текущий `master`) | Spellcore + mine/turret/toxic + **LMB урон** + **брат камни** | Микро-добив, параллельный хаос, не factory; 100+ итераций баланса не дали «смотреть и чиллить» |
| **v3** | Урон **только постройки**; семья пассивна; LMB = баф; integrity башни | Целевой контракт: [[factory-feel#12. Направление v2 — урон **только** от построек]] |

**Решение:** не чинить v2-хаос. На ветке `Spellcore_v3` строим **второй прототип fun'а** на том же техническом каркасе (сетка, ECS, Essence, волны).

---

## 1. North star (одна фраза)

> **Настроил спицы → смотришь шоу, как три типа построек перемалывают толпы → изредка бафнешь узкое место. Башня держит ~200 ударов, брат чинит плохо и отвлекается.**

Референс по ощущению: King is Watching — долгий подход, таймлайн, сочность без спешки, не кликер.

---

## 2. Дизайн-контракт (не обсуждается в рамках v3)

### 2.1 Урон и роли

| Источник | Роль |
| -------- | ---- |
| **Mine** | Основной урон наземным; W1: одной хватает на всех котов пути |
| **Toxic** | Сильный slow (кроме драконов); Outer; тики яда — шоу |
| **Turret** | Единственный гарант по **дракону**; стрельба по **всему пути**, приоритет воздуху |
| **LMB (принцесса)** | **Баф** постройки +50% урона, 60 с, **макс. 2** активных; тот же VFX |
| **Брат** | **Ремонт** integrity, медленно; idle отвлекает |
| **Враги → башня** | Integrity **~200 ударов**, не HP 2000 |

### 2.2 Teach ladder (волны)

| Wave | Unlock | Урок |
| ---- | ------ | ---- |
| W1 | Mine | 1 mine на пути = все коты |
| W2 | Tank | Щит: нужна **2-я** mine на пути |
| W3 | Turret, Dragon | Дракон пролетает mine/toxic; убивает турель |
| W4 | Toxic | Лужа на Outer, slow всем кроме драконов |
| W5 | Full factory | Несколько спиц, LMB-баф на плотные пакеты |

### 2.3 Анти-цели (playtest стоп-кран)

См. [[factory-feel#6. Анти-паттерны (чеклист playtest)]]. Если снова **a–f** на W3+ — не балансим цифры, режем scope волны.

**Шкала 30 с боя:** 1 = смотреть · 5 = жопа. Gate: **≤ 2.5** W4, **≤ 3** W5.

---

## 3. Каркас: оставляем / вырезаем

### 3.1 Оставляем (инфраструктура)

- Радиальная сетка, `SectorRegistry`, path unlock, преп ↔ бой
- Essence, vacuum, plant placement, plant bar
- `Intro1`–`Intro5` как **каркас** волн (перепишем composition/timing)
- ECS, фабрики сущностей, UI волн, wave preview / spawn arrows
- Family: брат + принцесса на сцене

### 3.2 Deprecate / отключить (v2 combat)

| Система | Действие |
| ------- | -------- |
| `TowerBrotherStoneThrowSystem` + arc projectile | **Off** / удалить из v3 bootstrap |
| `ExplodeAtPointSystem` direct damage | Заменить на **BuildingBuffSystem** |
| Brother как DPS в балансе | Убрать из [[balance]] v3 |
| Tower `MaxHealth` 2000 leak-DPS | → **Integrity hits** |
| Turret N±1 arc only | → path-wide + air priority |
| `WaveSpawnPlanService` random shuffle | **Done** — `PathIndex` + round-robin |
| `balance-budgets` Slack 0.8 на W5 | Пересчитать под factory, не под «горящую жопу» |

---

## 4. Эпики (порядок работ)

### Epic 1 — Foundation & kill legacy DPS

**Цель:** в бою урон врагам только от plants; v2 DPS не участвует.

- [x] `CombatModel` / feature flag в `GameplayContextRegistrations`
- [x] Отключить brother stone throw + impact VFX chain
- [x] LMB: убрать `TryTakeDamage` по врагам (заглушка или no-op)
- [x] Док: `balance-v3.md` — черновик чисел (отдельно от legacy `balance.md`)

**Done (сверх чеклиста):**

- [x] `FactoryV3CombatConfig` + `.asset`, регистрация в `ResourcesConfigsLoader`
- [x] Mine v3: `MineFactoryPulseDetonationSystem`, `MineFactoryPulseTimingUtility`, `MineFactoryPulseBehaviorUtility` — см. **Mine pulse rules** ниже
- [x] `CollectEnemiesInMineSector` — клин по геометрии + proximity на M/I (не outer)
- [x] Кошка: `StopAtInnerBeltAnchor` → стоп на inner-якоре (как танк на middle)
- [x] Enemy move speed scale **×0.7** в `EntitiesFactory.ResolveEnemyMoveSpeed`
- [x] `SpellcoreProgressionService` v3: mine W1 + **1 free mine**, turret W2+, toxic W3+
- [x] `StageProcessState` / prep: default ability **mine**, не LMB в FactoryV3
- [x] `Intro1` — медленнее spawns
- [x] `TurretPathTargetSelector` — plantable path O/M/I only (без Spawn), приоритет dragon; legacy `TurretSectorArcTargetSelector` только для Legacy
- [x] `CombatModelService` + FactoryV3 flag в bootstrap

**Mine pulse rules (FactoryV3, playtest iter):**

| Враг | Пояс мины | Тики |
| ---- | --------- | ---- |
| Танк | **Outer** | 3 × **прогресс** прохода клина (33 / 66 / 92%) |
| Танк | **Middle** (финальный) | **вход** → **стоп на якоре** → **3-й по таймеру** после стопа |
| Кошка | **Outer / Middle** | 3 × прогресс (полный проход) |
| Кошка | **Inner** (финальный) | вход → стоп на inner-якоре → 3-й по таймеру |
| Любой | выход / смерть в секторе | недоставленные пульсы **добиваются** (flush) |

Код: `UsesStopBeltMinePattern` (tank+middle, cat+inner); `IsFullCrossingPulseReady` — только progress; `IsStopBeltPulseReady` — pulse0 entry, pulse1 `DistanceToTargetReached`, pulse2 timer.

**Gate:** playtest Intro1 — котов убивает только mine.

---

### Epic 2 — Integrity башни

**Цель:** ~200 ударов, дискретный leak.

- [x] `TowerIntegrityConfig`: max hits, вес удара cat / tank shot / dragon tick
- [x] UI: счётчик integrity (не HP bar 2000)
- [x] HUD: вместо Win/Loss — один `IconTextView` со счётчиком убитых врагов за забег
- [x] Leak: кот взрыв, танк с Middle, дракон beam → −N hits
- [x] Win/lose без изменения условий one-pager (`MainHero.IsDead` при integrity 0)

**Done (сверх чеклиста):**

- [x] `TowerIntegrityTakeDamageSystem` + `TowerIntegrityLeakResolver`; `TakeDamageInfo.Source` для маршрутизации leak
- [x] Tower в FactoryV3: integrity вместо HP 2000 (`MainHeroFactory`)
- [x] `EntityHealthPresenter` — `current/max` hits для MainHero в FactoryV3
- [x] `RunEnemyKillCounterService` + `GameplayStatsPresenter` → `StatsListView.prefab` (`StatsView`, `ChildAlignment: 5` справа)
- [x] Main menu Win/Loss через `GameStatsPresenter` — без изменений

**Gate:** комбо leak не сносит башню за &lt;15 с без игнора.

---

### Epic 3 — Mine-only W1 + teach W2 shield

**Цель:** одна mine на W1; танк требует две.

- [x] Tank **shield**: 1-й mine pulse на танка ×0.15 (`TankMineShieldService` + `FactoryV3CombatConfig._tankFirstMinePulseDamageMultiplier`)
- [x] `Intro1` composition: только коты (5), 1 path (progression), spawn 6–8 с, pause 4
- [x] `Intro2`: 3 кота → пауза → **1 tank** per wave (teach 2 mines)
- [x] Move speed ×0.7 (Epic 1 `FactoryV3CombatConfig`); Intro spawn/pause замедлены в `.asset`

**Gate:** W1 одна mine; W2 две mines на одном пути — без брата/LMB.

---

### Epic 4 — Turret path-wide + dragon

**Цель:** дракон умирает только от турели.

- [x] Dragon: immune mine/toxic damage; пролет по поясам (`FlyingEnemy` + sector belt → Inner)
- [x] `TurretPathTargetSelector` (PlantTurret targeting): весь path O/M/I, priority dragon; 50% ground dmg (cat/tank ×0.5)
- [x] `Intro3` под teach (3 cats → 1 dragon)
- [x] Juice: выстрел турели (screen shake), hit feedback (scale punch + `TakeDamageVisualKind.Turret`)

**Done (сверх чеклиста):**

- [x] `PlantDamageCounterService` — mine/dragon ×0; turret dragon ×1, ground ×0.5
- [x] `WorldToSector.ResolveForFlyingEnemy` + `SectorMembershipSystem` — O/M → Inner для летающих
- [x] `PlantDamageApplicationService` — mine enrage только Legacy (не FactoryV3)
- [x] `DealDamageOnContactSystem` + `GameplayJuiceService.PlayTurretHit`
- [x] **Same-cell / dragon priority (playtest):** `TurretCombatTargetRefreshSystem` (цель каждый кадр); `PlantTurretInstantShootSystem` — direct dmg на клетке, projectile в дракона если танк на клетке; `TurretTargetPriority`
- [x] ECS update **до** AI brains (`GameplayBootstrap`) — турель успевает развернуться на дракона

**Gate:** W3 без турели на спице с драконом — проигрыш integrity, не «добей руками».

---

### Epic 5 — Toxic W4

**Цель:** Outer slow, не драконы; визуальные тики.

- [x] Toxic: slow **−50%** speed в FactoryV3 (`FactoryV3CombatConfig.ToxicSlowMoveSpeedFraction`); dragon immune (×0 dmg, no slow, `FlyingEnemy`)
- [x] FactoryV3: toxic только **Outer** (`PlantPlacementService`); preview — ярче/крупнее маркер на O
- [x] `Intro4` pacing v3: spawn 5–8 с, паузы 4–6, без dragon (ground teach)
- [x] Juice: `TakeDamageVisualKind.Toxic`, `PlayToxicTick` scale punch на тик

**Gate:** коты/танки заметно медлят на O; дракон — нет.

---

### Epic 6 — LMB building buff (принцесса)

**Цель:** макс. 2 бафа, +50% plant damage, 60 с.

- [x] `PlantBuildingBuffService` + `BuildingBuffSystem`: клик LMB → plant в секторе (`TryGetPlantAtSector`), не enemy
- [x] VFX: frost orbs на постройке на время бафа (reuse `FrostTargetOrbsPrefab`)
- [x] Урон: `PlantDamageApplicationService` + `PlantTurretInstantShootSystem` × `BuildingBuffDamageMultiplier` (1.5)
- [x] Лимит **2** активных; повторный клик по той же постройке — refresh 60 с; CD LMB **5 с** без изменений
- [x] Essence **10** за apply/refresh (`BuildingBuffEssenceCost`); без essence баф не накладывается
- [x] Таймер бафа над постройкой (`PlantBuildingBuffTimersDisplayPresenter` + prefab)
- [x] Juice: punch + delayed crunch mine/turret/toxic; LMB landing без enemy impact
- [x] UI ability bar: отдельный индикатор бафа — **stub** (мир = orbs + таймер над plant; bar — post-v3)

**Gate:** баф на плотной группе меняет исход; без перестановки каждые 5 с.

---

### Epic 7 — Brother repair

**Цель:** медленный ремонт, idle прерывает.

- [x] `TowerBrotherRepairSystem`: +integrity / interval (`FactoryV3CombatConfig`: 1 hit / 6 с, радиус 18)
- [x] Прерывание idle-циклом (`IsCurrentlyIdle` → таймер сбрасывается, ремонт не тикает)
- [x] Stone throw **off** в FactoryV3 (`AllowsPlayerPassiveEnemyDamage`); prefab без throw view

**Gate:** брат чувствуется «живым», не спасает от полного игнора.

---

### Epic 8 — Spawn plan & W5 factory

**Цель:** доверие к пути, не shuffle; W5 — шоу, не кабачок.

- [x] Убрать random shuffle; `PathIndex` на `SpawnGroupConfig` + round-robin fallback
- [x] `Intro5` v3: 10 сериальных пакетов, 5 paths, factory pacing 5–8 с, без tank+dragon в одной группе
- [x] Time scale UI: Pause / 1× / 3× между wave preview и kill counter (`GameplayTimeScaleService`, `CombatTimeScaleView`)
- [x] `GameplayBootstrap.OnDestroy` → `Time.timeScale = 1`

**Done (сверх чеклиста):**

- [x] `CombatTimeScalePresenter` + DI + `GameplayScreenView.prefab` wiring
- [x] Wave preview и spawn используют один детерминированный план
- [x] Пауза: геймплейные DOTween без `SetUpdate(true)`; `DOTween.PauseAll`/`PlayAll` в `GameplayTimeScaleService` — отложенные колбэки не «протекают» на паузе

**Gate:** шкала factory-feel на W5 ≤ 3; победа без «кабачка».

---

### Epic 9 — Balance pass & docs

- [ ] `balance-v3.md` синхрон с .asset
- [ ] Обновить [[00-one-pager]] rules под v3 (LMB buff, no brother DPS)
- [ ] Meetup build checklist
- [ ] **End-of-run beat:** победа/поражение → сразу `WinState`/`DefeatState` → попап → главное меню; нет паузы/момента «завершения» на сцене. Целевое: задержка, камера, fade или короткий beat до попапа — **ближе к balance playtests**, не блокер эпиков 3–8.

---

## 5. Вне scope v3 (явно)

- Survival post-W5 (после полировки W4–W5)
- Meta progression / coins
- Новые типы врагов
- 6-й path на W5 (держим 5 как в iter 3.7 обсуждении, если не переиграем)
- Полный таймлайн UI как King is Watching (stub достаточно для demo)

---

## 6. Риски

| Риск | Митигация |
| ---- | --------- |
| Снова 40 итераций без fun | Gate после **каждого** epic, не копить |
| Meetup сырой | Epic 1–3 = минимальный демо «factory exists» |
| «Садистское шоу» без juice | Epic 4–5 parallel juice tasks |

---

## 7. Критерий merge в master

1. W1–W5 проходимы с **factory** шкалой ≤ целевой.
2. Нет обязательного brother/LMB damage по врагам.
3. Playtest сторонним человеком: описывает бой словами **«смотрел»**, не **«тушил»**.
4. `balance-v3.md` + one-pager согласованы.

До merge **master** = legacy archive; не балансим v2 параллельно.

---

## 8. Следующий шаг

**Epic 9** — balance pass, one-pager, meetup checklist, end-of-run beat.

---

*`Spellcore_v3` @ Epic 8 done. Дизайн-обоснование: [[factory-feel]].*
