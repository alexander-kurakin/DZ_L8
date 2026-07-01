# Balance v3 — factory / idle defense

**Status:** синхрон с `.asset` · **ветка** `Spellcore_v3` · Epic 9  
**Контракт:** [[factory-feel#12. Направление v2 — урон **только** от построек]] · [[plan-v3]]  
**Legacy:** [[balance]] — не трогаем при v3-итерациях, только сверка «что выключили»

> Источник правды для чисел: `SpellcoreCombatConfig.asset`, plant `.asset`, `TowerIntegrityConfig.asset`, enemy character `.asset`.  
> Код: `CombatModel.FactoryV3` — брат и LMB **не наносят** урон врагам.

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

### Mine v3 (`SpellcoreCombatConfig.asset`)

| Параметр | Значение | Asset field |
| -------- | -------- | ----------- |
| Урон за пульс | **115** | `_mineDamagePerPulse` |
| Пульсов на проход сектора | **3** | `_minePulsesPerSectorCrossing` |
| Tank 1-й mine pulse | **×0.15** | `_tankFirstMinePulseDamageMultiplier` |
| Move speed scale | **×0.7** | `_enemyMoveSpeedScale` |
| Toxic slow (FactoryV3 runtime) | **−50%** (`×0.5`) | `_toxicSlowMoveSpeedFraction` |
| Toxic placement (FactoryV3) | **Outer only** | код `PlantPlacementService` |
| LMB building buff | **×1.5** dmg, **60 с**, max **2**, **10** essence | `_buildingBuff*` |
| Brother repair | **+1 hit** на старт фазы движения | `_brotherRepairHitsPerMovementPhase` |

Кошка стоп — **Inner anchor** (`StopAtInnerBeltAnchor`, r=12.6). Танк стоп — **Middle anchor** (`StopAtMiddleBeltAnchor`, r=35.1). Недоставленные пульсы **добиваются** при выходе/смерти в секторе. Трекинг per-enemy; клин: `CollectEnemiesInMineSector`.

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
| **Mine** | Damage (asset) | 150 | runtime pulse **115** (`SpellcoreCombatConfig`) | 3 ✓ |
| | Proc delay | 0.25с | `MineConfig` | — |
| | Tank shield | — | 1-й pulse **×0.15** | 3 ✓ |
| **Toxic** | DoT / interval | 35/tick, **2.5 с** | `ToxicAreaConfig`; slow в бою **−50%** из `SpellcoreCombatConfig` (asset slow **0.33** — legacy, не используется в FactoryV3) | 5 ✓ |
| **Turret** | Damage / cooldown | 100 / **2.5 с** | `TurretConfig`; hit juice §4.1 | 4 ✓ |
| **LMB** | — | cooldown 5с | баф +50% plant (`BuildingBuffDamageMultiplier` 1.5), 60 с, max 2 | 6 ✓ |

### Plant costs (Essence)

Без изменений до playtest v3: Mine **28**, Toxic **25**, Turret **100**.

---

## 4. Enemies — v3 роли

| Enemy | Belt | HP (legacy) | Move speed | v3 заметка |
| ----- | ---- | ----------- | ---------- | ---------- |
| Cat | Inner | 325 | **↓ глобально** (Epic 3) | конвейер, не мини-босс |
| Tank | Middle | 500 | **↓** | щит mine: нужна **2-я** mine на пути (Epic 3) |
| Dragon | Inner | 550 | 6 | **FlyingEnemy**; beam **30**/tick, interval **2 с**; mine/toxic **×0**; только турель (Epic 4 ✓) |

### 4.1 Enemy hit juice (impact hits only)

Mine / turret / projectile — **не** toxic DoT.

| Параметр | Значение | Код / asset |
| -------- | -------- | ----------- |
| Gameplay stun | **0.12–0.22 с** (от урона, ref **50**) | `EnemyHitReactionSystem`, `EnemyHitStunRemainingTime` (было `EnemyHitStunSkipFrames`) |
| Блокирует | `CanMove` + `CanRotate` → walk anim off | `EntitiesFactory` inline, без helper-методов |
| Отброс | к `EnemySpawnOrigin`, XZ **0.22–0.5**, Y **0.18–0.4** | `EnemyHitJuiceUtility` (был `EnemyHitKnockbackUtility` на корне) |
| Scale punch | **1.08–1.16×** на Animator child | `TakeDamageView` |
| Toxic DoT | hit stun **off**; только slow + flash + `PlayToxicTick` | `TakeDamageVisualKind.Toxic` |
| Turret shot shake | **0.12** strength, 0.12 с (выстрел, не попадание) | `GameplayVfxConfig` |

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

### W5 `Intro5.asset` (факт, survival template)

| Параметр | Значение |
| -------- | -------- |
| Групп | **10** сериальных |
| Пути | **0–4** (5 paths), `PathIndex` в каждой группе |
| Состав | **11** cats, **3** tanks, **2** dragons (tank+dragon **не** в одной группе) |
| Cat spawn interval | **5–7** или **5–8** с |
| Tank/dragon spawn | instant (`0–0` с) |
| Group pause | **5–6** с (последняя **0**) |
| `EnemySpawnRadius` | **90** |

---

## 7. Family (v3)

| Роль | Параметр | Цель |
| ---- | -------- | ---- |
| Принцесса (LMB) | Баф plant | +50% damage, 60 с, max 2 — `SpellcoreCombatConfig` |
| Брат | Ремонт integrity | **+1 hit** на каждый вход в фазу движения; idle **5 с** / move **3 с** (`BrainsFactory`); idle прерывает ремонт |
| Брат stone DPS | — | **выключен**; компонент снят с `TowerBrother.prefab` |

---

## 8. Playtest шкала

См. [[factory-feel#6. Анти-паттерны]]: **≤ 2.5** на W4, **≤ 3** на W5 (30 с боя, 1 = смотреть · 5 = жопа).

---

## 9. Survival playtest slice (Epic 8+)

| Параметр | Значение | Код / примечание |
| -------- | -------- | ---------------- |
| Offer после кампании | W5 complete → prep popup | `SurvivalFlowService.OnNormalCampaignCompleted` |
| Milestone волны | **10, 15, 20…** (каждые 5 `completedWaves` после W5) | `SurvivalWaveScalingService.IsSurvivalMilestoneCompletedWave` |
| Milestone gold | **+2000** | `SurvivalFlowService.SURVIVAL_MILESTONE_BONUS_GOLD` |
| Campaign completion gold | level reward (из `LevelConfig`) | `PreparationState` + `TryConsumeCampaignCompletionGoldGrant` |
| Gold persistence | wallet + save | survival: `AddGoldAndPersist`; финальный win: `AddGold` + `EndGameState.SaveAllData` |
| Survival wave template | **Intro5** runtime copy | `StageProviderService` → `SurvivalWaveScalingService` |
| Tier | `(waveNumber − stagesCount − 1) / 5` | waves 6–10 tier 0, 11–15 tier 1, … |
| Tier: enemy count | **+30%** / tier | `ENEMY_COUNT_BONUS_PER_TIER` |
| Tier: spawn interval | **×0.9** / tier (min 0.25 с) | `SPAWN_INTERVAL_SCALE_PER_TIER` |
| Tier: group pause | **×0.85** / tier (min 0.5 с) | `GROUP_PAUSE_SCALE_PER_TIER` |
| Survival paths | **+1** path / survival wave (до max 16) | `SpellcoreProgressionConfig._survivalPathsPerWave` |
| Spawn paths survival | `PathIndex = −1` → round-robin все открытые | `SurvivalWaveScalingService.CreateScaledWaveData` |
| Wave path rotation | слот сдвигается по `(waveNumber − 1) % pathCount` | `WaveSpawnPlanService.ResolvePathIndex` |

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
| 2026-06-30 | 4–5 | Enemy hit juice: stun + `EnemyHitJuiceUtility`; toxic/beam tick **2.5 / 2 с**; toxic без hitstop |
| 2026-06-30 | 8+ | Survival playtest slice: flow/popups, tier scaling, path rotation, `PersistedGoldRewardService`, sell-shovel hit radius; enemy hit juice iter (SkipFrames → RemainingTime → Animator child) |
| 2026-06-30 | 9 | Синхрон с `.asset`; `SpellcoreCombatConfig` naming; brother repair = movement phase; end-of-run beat **1.2 с**; one-pager v3; [[meetup-build-checklist]] |
