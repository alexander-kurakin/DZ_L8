# HANDOFF — сессия Gnome + Projectile (2026-07-09)

Документ для следующего агента/разработчика. Состояние на конец сессии.

---

## Контекст сессии

Реализована **Gnome Arena** фича: гномы прячутся за укрытиями, выглядывают, получают урон от снарядов (2 попадания = смерть), gaze-скан мышью провоцирует peek. Параллельно перестроен **пайплайн урона снарядов** через ECS (`TakeDamage`, `ContactTakeDamage`, `Sensors`).

Пользователь несколько раз откатывал правки **peek-анимации** и **физики снарядов** из-за регрессий. К концу сессии peek возвращён к **стандартной** версии; физика снарядов починена **вручную в Project Settings** пользователем.

---

## Что работает / задумано

### Gnome gameplay

| Область | Файлы | Поведение |
|--------|-------|-----------|
| Оркестратор | `GnomeOrchestratorService.cs` | Пул гномов, respawn, gaze scan, taunt по радиусу на плоскости укрытия |
| AI | `GnomeHiddenState`, `GnomePeekingState` | Цикл hide → peek → hide; force peek по gaze |
| Спавн | `EnemiesFactory.cs` | Гном на `GnomePeekPoint`, peek direction/offset с точки |
| Урон | `GnomeTakeDamageSystem.cs` | Счётчик попаданий → смерть при `RequiredHits` (2) |
| View | `GnomeView.cs` | Коллайдер вкл. только в peek; DOTween выезд |
| Win | `GnomeWinConditionService`, `GnomeArenaStage` | Условие победы арены |

**Gaze / taunt:**
- Луч мыши бьёт в коллайдер с `GnomeCoverAnchor`
- Позиция peek point **проецируется на плоскость укрытия** — taunt срабатывает даже если гном «за» cover
- `_gazeActivatedGnomes` — gaze активирует гнома **один раз** за жизнь (не бесконечный re-trigger)

**Peek (текущая логика в `GnomeView`):**
```csharp
hiddenWorld = entityTransform.TransformPoint(hiddenLocalPosition)
peekWorld = hiddenWorld + peekDirection * peekOffset
_visualRoot.DOMove(peekWorld, ...)
```
- Коллайдер выкл. в hidden, вкл. в peek
- `GnomePeekPoint`: gizmo-линия = `forward * _peekOffset`, `HiddenRotation = transform.rotation`
- **Известная нерешённая проблема:** у **вертикальных** гномов едет **центр капсулы**, а не **голова** по линии gizmo. Горизонтальные после отката работают нормально.
- **Не делать снова:** `GnomeUseVerticalPeekLayout`, `GetUprightRotation`, смещение через `InverseTransformDirection`, runtime-detect оси капсулы — всё это ломало ориентацию/горизонтальные точки.

### Projectile

| Слой | Поведение |
|------|-----------|
| Unity Physics | Снаряд **сталкивается** со стенами/полом/объектами (отскок/остановка, гравитация после удара) |
| ECS `MustDie` | Только `IsTouchAnotherTeam` — **исчезает только при валидном попадании в гнома** |
| ECS overlap mask | `Layers.ProjectileContactsMask = CharactersMask` — контакты для урона только по персонажам |
| View | `ProjectileCollisionView` — `OnCollisionEnter` на **любое** столкновение (VFX, `HasCollided`, gravity) |

**`HasCollided` — вводящее в заблуждение имя:**
- При **запуске** (`ProjectileLaunchSystem`) сразу `HasCollided = true` → «передан физике», `RigidbodyMovementSystem` перестаёт перезаписывать velocity
- При **столкновении** (`ProjectileCollisionView`) тоже `true` + `useGravity = true`
- Не путать с «уже ударился об стену»

**Префаб:** `Assets/_Project/Resources/Entities/Projectile.prefab` — layer `Projectiles` (7), `ContinuousDynamic`, `SphereBodyColliderRegistrator`.

### TakeDamage / LifeCycle / Sensors

Новые фичи (часть в git как added):
- `TakeDamage` — упрощённый `TakeDamageInfo` (только `Damage` + `Source`)
- `LifeCycle` — `DeathSystem`, dissolve, `SelfReleaseSystem`
- `Sensors` — `BodyContactsDetectingSystem` (OverlapSphere + SphereCast при non-kinematic), `AnotherTeamTouchDetectorSystem`
- `ContactTakeDamage` — `DealDamageOnContactSystem`, `ProjectileContactRules`

### EntityAPI

- Генератор: `Tools/GenerateEntityAPI.ps1`
- **Фикс сессии:** strip суффикса `Component` при резолве типов (`Transform` vs `TransformComponent`)
- После удаления `GnomeUseVerticalPeekLayout` — **70 компонентов**. При добавлении компонентов — перегенерировать:
  ```powershell
  .\Tools\GenerateEntityAPI.ps1
  ```

---

## Критические ошибки сессии (НЕ ПОВТОРЯТЬ)

### 1. `Physics.IgnoreLayerCollision` в коде

В какой-то момент в `Layers` добавили static ctor с `Physics.IgnoreLayerCollision(Projectiles, Enviroment/Floor/...)` чтобы снаряды **пролетали сквозь** окружение.

**Проблема:** в Unity Editor вызовы `IgnoreLayerCollision` **сохраняются в** `ProjectSettings/DynamicsManager.asset`. Удаление кода **не откатывает** матрицу слоёв.

**Симптом:** снаряды проходят сквозь `Default` (0) и `Enviroment` (8) — большая часть геометрии `Gameplay.unity` на layer 0.

**Исправление:** пользователь **вручную** восстановил матрицу в Project Settings → Physics.

**Правило:** **НЕ** вызывать `Physics.IgnoreLayerCollision` из runtime-кода. Менять коллизии слоёв только через **Project Settings → Physics** (или осознанный коммит `DynamicsManager.asset`). **НЕ** городить static ctor в `Layers.cs`.

### 2. Peek — многократные переписывания

4+ итерации ломали горизонтальные точки (ныряние в землю, боковая ориентация). Пользователь явно просил: **стандартный DOMove**, для вертикальных — только минус полвысоты капсулы **без** ломания остального. Откат к стандарту принят.

### 3. Пустые строки между каждой строкой

`GnomeOrchestratorService.cs` и `GnomeCoverAnchor.cs` были с двойным spacing — исправлено (только форматирование).

---

## Слои (TagManager)

| Index | Имя |
|-------|-----|
| 0 | Default |
| 6 | Characters |
| 7 | Projectiles |
| 8 | Enviroment (опечатка в проекте) |
| 9 | ContactTrigger |
| 10 | Floor |
| 11 | Fence |

`Layers.cs` — **только константы**, без static ctor.

`DynamicsManager.asset` (на конец сессии, после ручного фикса пользователя):
```
m_LayerCollisionMatrix: ffff...7fff...  (почти все пары включены)
```

---

## Конфиги

- `GnomeConfig` — `RequiredHits: 2`, тайминги hide/peek, `DeathDissolveSeconds`
- `GnomeArenaConfig` — `TotalGnomesInRun`, `MaxActiveGnomes`, respawn, gaze radius/interval, cover layer mask
- Ресурсы: `Assets/_Project/Resources/Configs/Gameplay/Gnome/`

---

## Сцена / Editor (незавершено у пользователя)

- Расставить `GnomePeekPoint` + `GnomeCoverAnchor` на арене
- `GnomeArenaStage` / anchor в `Gameplay.unity`
- Горизонтальные peek points: rotation вручную в Inspector
- Вертикальные: peek line gizmo должна совпадать с траекторией **головы** (пока не совпадает — см. выше)

---

## Ключевые пути

```
Assets/_Project/Develop/Runtime/Gameplay/Features/GnomeFeature/
Assets/_Project/Develop/Runtime/Gameplay/Features/ContactTakeDamage/
Assets/_Project/Develop/Runtime/Gameplay/Features/Sensors/
Assets/_Project/Develop/Runtime/Gameplay/Features/TakeDamage/
Assets/_Project/Develop/Runtime/Gameplay/Features/LifeCycle/
Assets/_Project/Develop/Runtime/Gameplay/EntitiesCore/EntitiesFactory.cs
Assets/_Project/Develop/Runtime/Utilities/Layers.cs
Assets/_Project/Resources/Entities/Projectile.prefab
Tools/GenerateEntityAPI.ps1
ProjectSettings/DynamicsManager.asset
ProjectSettings/TagManager.asset
```

---

## Открытые задачи

1. **Вертикальный peek** — голова по gizmo-линии, не ломая горизонтальные (минимальный diff).
2. **Сцена** — peek points, arena anchor (пользователь в Editor).
3. **Playtest** — gaze → peek → 2 hits → death → respawn → win condition.

---

## Стиль / правила проекта

- `.cursor/rules/` — GUID в `.meta`, DOTween `.Play()`, `using Random = UnityEngine.Random`, расширять эталоны (`EntitiesFactory`, `TakeDamage`, `SpawnFeature`)
- Коммиты на **русском**
- Не коммитить без явной просьбы
- Перед правками **перечитывать файлы** — пользователь часто правит сам

---

## Git

Много untracked/modified файлов по GnomeFeature, Sensors, TakeDamage, LifeCycle, EntityAPI. Ветка не закоммичена в рамках этой сессии (уточнить у пользователя).

Transcript сессии: `agent-transcripts/92567295-de56-4a19-af04-a984a72929e9/92567295-de56-4a19-af04-a984a72929e9.jsonl`
