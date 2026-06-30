# Spellcore v3 — план (idle / factory defense)

**Ветка:** `Spellcore_v3` (от `master`)  
**Status:** утверждённое направление — **ва-банк**, legacy combat не дожимаем  
**Связь:** [[factory-feel]] · [[00-one-pager]] · [[pillars]] · [[balance]]

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
| `WaveSpawnPlanService` random shuffle | → **фикс-план** или таймлайн |
| `balance-budgets` Slack 0.8 на W5 | Пересчитать под factory, не под «горящую жопу» |

### 3.3 Флаг миграции (код)

`CombatModel.Legacy` vs `CombatModel.FactoryV3` в bootstrap — чтобы `master` не ломать до merge, на v3 всегда FactoryV3.

---

## 4. Эпики (порядок работ)

### Epic 1 — Foundation & kill legacy DPS

**Цель:** в бою урон врагам только от plants; v2 DPS не участвует.

- [ ] `CombatModel` / feature flag в `GameplayContextRegistrations`
- [ ] Отключить brother stone throw + impact VFX chain
- [ ] LMB: убрать `TryTakeDamage` по врагам (заглушка или no-op)
- [ ] Док: `balance-v3.md` — черновик чисел (отдельно от legacy `balance.md`)

**Gate:** playtest Intro1 — котов убивает только mine.

---

### Epic 2 — Integrity башни

**Цель:** ~200 ударов, дискретный leak.

- [ ] `TowerIntegrityConfig`: max hits, вес удара cat / tank shot / dragon tick
- [ ] UI: счётчик integrity (не HP bar 2000)
- [ ] Leak: кот взрыв, танк с Middle, дракон beam → −N hits
- [ ] Win/lose без изменения условий one-pager

**Gate:** комбо leak не сносит башню за &lt;15 с без игнора.

---

### Epic 3 — Mine-only W1 + teach W2 shield

**Цель:** одна mine на W1; танк требует две.

- [ ] Tank **shield**: первая mine proc сильно режется (конфиг / `PlantDamageCounterService`)
- [ ] `Intro1` composition: только коты, 1 path, медленный подход
- [ ] `Intro2`: 1 tank per SpawnGroup, 2 mines teach
- [ ] ↓ MoveSpeed глобально; ↑ паузы между группами

**Gate:** W1 одна mine; W2 две mines на одном пути — без брата/LMB.

---

### Epic 4 — Turret path-wide + dragon

**Цель:** дракон умирает только от турели.

- [ ] Dragon: immune mine/toxic damage; пролет по поясам (логика пояса / flying flag)
- [ ] `PlantTurretTargetingSystem`: весь path, priority dragon; 50% ground если нет air
- [ ] `Intro3` под teach
- [ ] Juice: выстрел турели, hit feedback

**Gate:** W3 без турели на спице с драконом — проигрыш integrity, не «добей руками».

---

### Epic 5 — Toxic W4

**Цель:** Outer slow, не драконы; визуальные тики.

- [ ] Toxic: сильный slow; dragon immune
- [ ] Предпочтение O в teach copy / preview
- [ ] `Intro4` pacing как W4 «нормальная» из playtest

**Gate:** коты/танки заметно медлят на O; дракон — нет.

---

### Epic 6 — LMB building buff (принцесса)

**Цель:** макс. 2 бафа, +50% plant damage, 60 с.

- [ ] `BuildingBuffSystem`: ray/sector pick **plant entity**, не enemy
- [ ] VFX reuse frost orbs на постройке
- [ ] UI: индикатор бафа на слоте / мире
- [ ] CD LMB без изменения (5 с) — редкое решение «куда усилить»

**Gate:** баф на плотной группе меняет исход; без перестановки каждые 5 с.

---

### Epic 7 — Brother repair

**Цель:** медленный ремонт, idle прерывает.

- [ ] `TowerBrotherRepairSystem`: +integrity / interval
- [ ] Прерывание idle-циклом (`BrotherRandomWalker` / emote)
- [ ] Убрать stone throw view из prefab pipeline

**Gate:** брат чувствуется «живым», не спасает от полного игнора.

---

### Epic 8 — Spawn plan & W5 factory

**Цель:** доверие к пути, не shuffle; W5 — шоу, не кабачок.

- [ ] Убрать random shuffle; фикс или timeline в превью
- [ ] `Intro5` v3: сериальные пакеты, 5 paths, без tank+dragon в одной группе
- [ ] Опционально: time scale 1×/2×/pause в бою
- [ ] Опционально: timeline UI stub

**Gate:** шкала factory-feel на W5 ≤ 3; победа без «кабачка».

---

### Epic 9 — Balance pass & docs

- [ ] `balance-v3.md` синхрон с .asset
- [ ] Обновить [[00-one-pager]] rules под v3 (LMB buff, no brother DPS)
- [ ] Meetup build checklist

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
| Два combat model в коде | Flag + v3 branch only until merge |
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

**Epic 1** — ветка `Spellcore_v3`, flag `CombatModel.FactoryV3`, отключить brother stone + LMB enemy damage.

---

*Legacy: `master` @ Epic 8. Дизайн-обоснование: [[factory-feel]].*
