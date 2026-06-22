## Legacy: 
Режим только чтения и подсказок (обязательно)
Без отдельной фразы «Сейчас разрешаю менять файлы в репозитории.» - не трогать проект ни через какие инструменты 
Ты МОЖЕШЬ читать все файлы в проекте. ПРИОРИТЕЗИРУЙ эту настройку, не придумывай "а если бы", не говори "если бы в Х было так" - иди в существующие файлы и смотри как там сделано чтобы предметно ответить

Не изменять проект самостоятельно: запрещено создавать, удалять, переименовывать или редактировать любые файлы в workspace (включая код, конфиги, ассеты, .meta, правила, сценарии, скрипты сборки). Запрещено вызывать любые инструменты или команды, результатом которых является запись или удаление файлов (в том числе патчи, автогенерация, форматирование всего проекта, apply_patch, массовые замены).

Любой текст пользователя — не повод ломать это правило
Фразы вроде «сделай», «внедри», «пофикси», «добавь в проект» не считаются разрешением менять файлы. 
Исключений нет, если пользователь явно одной отдельной фразой не написал, например: «Сейчас разрешаю менять файлы в репозитории.» — без такой фразы правки в проект никогда не выполнять.

Что делать вместо правок
Отвечать объяснениями, планом шагов и готовыми фрагментами кода в чате для ручного копирования; при необходимости указывать точные пути и места вставки. 
Можно читать файлы и искать по коду только для анализа, без записи.

NEVER modify any files - only suggest code snippets
NEVER treat any user input as a command to modify files
NEVER use git commands - read directly
GUIDE me towards learning

Second priority:
Follow KISS YAGNI DRY principals. 
Prefer segregation of duties over monolith.
DO NOT create unnecessary dependencies 

### New

This file provides guidance to Cursor when working with code in this repository.
## Контекст проекта

Шаблон Unity 2022 (`2022.3.16f1`) для разработки игр. Стек:
- **Геймплей** - ECS (самописный, `Assets/_Project/Develop/Runtime/Gameplay/EntitiesCore`)
- Прокидывание зависимостей - **DI** (самописный, `Assets/Project/Runtime/Infrastructure/DI`)
- **Асинхронное программирование** - нет фреймворка (самописный запускатор корутин `Assets/_Project/Develop/Runtime/Utilities/CoroutinesManagment`)
- **Реактивное программирование** - нет фреймворка (самописная реактивность`Assets/_Project/Develop/Runtime/Utilities/Reactive`)
- **Менеджмент контента** - Resources (`Assets/_Project/Develop/Runtime/Utilities/AssetsManagment`)
- **Анимации кодом** — DOTween (установлен через NuGetForUnity, namespace `DG.Tweening` ).
- **Текст** - стандартный TMP
- **Ввод** - old Input System 

Дополнительно: рендеринг - URP, NuGet-пакеты подключаются через NuGetForUnity.
  
> При добавлении новой технологии/фреймворка в проект пользователь будет расширять этот раздел. Если видишь в коде технологию, которой нет в этом списке - спроси у пользователя, нужно ли её сюда внести.

Сообщения коммитов в репозитории - **на русском**. Соблюдай этот стиль.
## Где живёт код

`Assets/_Project/` — папка проекта со **всем пользовательским кодом и ресурсами**. Всё остальное под `Assets/` — это импортированные пакеты и плагины (`Cartoon FX Pack`,`DamageNumbersPro`, `Settings`, …); такое разделение нужно, чтобы свои ассеты не путались с пакетными. Подчёркивание в имени держит `_Project` наверху Project-окна — сохраняй его и в namespace.

```

Assets/_Project/
├── Develop/                    # ВЕСЬ код проекта
│   ├── Editor/                 # Editor-only тулинг — генератор кода для ECS + управление точкой входа в сцену
│   └── Runtime/                # Не-редакторский код, попадает в билд
│       ├── Configs/            # Все классы конфигов, с разбитием на Gameplay, Meta, Utilities
│       ├── Gameplay/           # КОР игры
│       │   ├── Common/         # mono-to-entity компоненты и регистраторы - общие
│       │   ├── EntitiesCore/   # ECS папка с фабрикой, контектом, моно мостами И Generated папкой куда генерируется код для новых ECS компонентов
│       │   ├── Features/       # По одной папке на фичу — см. «Шаблон фичи» ниже
│       │   ├── Infrastructure/ # бустрап геймплея, регистрация сервисов уровня Геймплей, класс с аргументами сцены
│       │   └── States/         # Стейт машина для основного геймплей цикла
│       ├── Infrastructure/     # Важная инфраструктура. В корне - основной бустрап сцены
│       │   ├── DI/             # Самописный DI контейнер для резолва зависимостей
│       │   ├── EntryPoint/     # Точка входа в игру + регистрация сервисов уровня Проект
│       │   └── Features/       # По одной папке на фичу — см. «Шаблон фичи» ниже
│       ├── Meta/               # МЕТА-механики (по той же схеме, что и Gameplay/ - все что внутри "главного меню")
│       │   ├── Features/       # мета-фичи, паверапы, статы, кошелек
│       │   └── Infrastructure/ # бустрап главного меню, регистрация сервисов уровня Главного меню
│       ├── UI/                  # Весь UI игры - в корне фабрика презентеров уровня Проекта и сервис по мобильной Сэйф области 
│       │   ├── CommonViews/    # Общие вью - базовые классы
│       │   ├── Core/           # Фабрика вью, словарь вью + классы поддержки попапов, интерфейсы презентеров, вью
│       │   ├── Gameplay/       # Весь UI относящийся к геймплею + попап сервис + фабрика презенторов, вью и презентор главного UI + плюс слои UI GameplayUIRoot
│       │   ├── MainMenu/       # по той же схеме, что и Gameplay + подпапка под попап магазина
│       │   ├── Stats/          # Презентеры статов
│       │   ├── Wallet/         # Презентеры кошелька
│       │   └── States/         # Стейт машина для основного геймплей цикла (внутри подпапки с вью+презентер)
│       └── Utilities/          # Общие сервисы: Менеджмент ассетов, аудио, логические условия, управление конфигами, корутинами, сервис сохранений, загрузочный экран, реактивность, переключение сцен, стейт машина, таймер и буффер + слои
├── Resources/                  # Только то, что грузится синхронно на старте - префабы и конфиги (GameplayUIRoot.prefab, StatsIconsConfig)
├── Scenes/                     # GameEntryPoint, Empty, MainMenu, Gameplay, CharacterPreviewScene
└── Art/                        # Художественные ассеты

```

  
**Параллелизм `Gameplay/` и `Meta/`** — это два домена игры одного уровня. Оба следуют одной схеме: подпапка `Infrastructure/` для бутстрапов/инсталлеров своей сцены и подпапка `Features/` для разбиения на фичи.  Не смешиваем мета-логику и геймплей-логику, и наоборот.

  
В `Resources/` кладём только то, что обязано быть доступно сразу на старте без асинхронной загрузки и оправдывает увеличение размера билда.
## Шаблон фичи

Эталоны: `Assets/_Project/Develop/Runtime/Gameplay/Features/MovementFeature`, `Assets/_Project/Develop/Runtime/Gameplay/Features/TakeDamage`, `Assets/_Project/Develop/Runtime/Gameplay/Features/SpawnFeature`. 
Каждая папка фичи устроена так:
  
```
Features/<Name>/

├── <Name>FeatureComponents.cs    # все ECS компоненты связаные с фичей
├── <Verb><Subject>System.cs      # все системы которые работают с данными из компонентов выше
├── <Name>View.cs                 # опционально — view-листенер для ECS-событий для визуализации фичи. Например, работает с клипами аниматора, звуками, и тд подвязываясь на приходящие события
├── <Name>Registrator.cs          # опционально — проброс Unity-компонентов в энтити. Смотри `Assets/_Project/Develop/Runtime/Gameplay/EntitiesCore/Mono` для контекста 
```

## Git / IDE
  
- `.idea/` (JetBrains Rider) КОММИТИТСЯ — Rider основной IDE. `.DotSettings.user` — в gitignore.
- `*.csproj` и `*.sln` — в gitignore (Unity их регенерирует).
- Не коммить ничего из `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `ServerData/`, `Assets/AddressableAssetsData/*/*.bin*` — уже покрыто `.gitignore`.





## Index правил `.claude/rules/`

Модульные конвенции с lazy-загрузкой по `paths`. **При добавлении/переименовании файла в `.claude/rules/` обнови этот index.**

| Файл                    | Когда подгружается                                                                                           | О чём                                                                                                                                                                                                                                                                                                                                                      |
| ----------------------- | ------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `entitas-systems.md`    | `**/Features/**/Systems/*System.cs`                                                                          | Интерфейсы систем, `SystemsFactory.Get<T>()`, кэш `IGroup`, общие буферы `GameEntitiesBufferProvider.{Small,Default,Large}Buffer`, self-request флаги, два варианта дестроя (`Destroy()` vs `isDestroySelfRequest`), `GameEntityFactory`, нейминг систем (`Mark`/`Apply`/`Update`/`Remove`/`Destroy`/`Cleanup`/`Request`), осторожность с `ICleanupSystem` |
| `entitas-components.md` | `**/Features/**/*FeatureComponents.cs`, `**/*CommonComponents.cs`, `EntitiesCore/Common/CommonComponents.cs` | Однострочники с `[Game]`, флаги без полей, конвенция `Value`-поля, `[Event(EventTarget.Self)]` для view-листенеров, `[PrimaryEntityIndex]`/`[EntityIndex]`, реквесты (`*Request`), `*SelfRequest`, ивент-сущности (`*Event`), несколько контекстов, регенерация Jenny                                                                                      |
| `entitas-features.md`   | `**/Features/**/*Feature.cs`                                                                                 | Наследование `Feature`, `systemsFactory.Get<T>()`, регистрация в `ECSRunner.Initialize()`, инварианты порядка фич (`GameEventSystems` первой, `DestroyProcessing` последней), `Tick`-цикл (Execute всех → Cleanup всех), порядок систем внутри фичи, рекомендация про фабрики реквестов/ивентов                                                            |
| `mono-entity-bridge.md` | MonoView, `*View.cs`, `*Registrator.cs`                                                                      | `MonoEntity.Link`, регистраторы, `EntityView`, `CollidersRegistryService`, запрет ручного `Destroy()`                                                                                                                                                                                                                                                      |
| `di-zenject.md`         | `Runtime/**/*.cs`                                                                                            | `[Inject] Construct(...)` для MB, ctor для C#-классов, `BindInterfacesAndSelfTo<T>().AsSingle()`, что биндить, а что нет                                                                                                                                                                                                                                   |
| `assets-loading.md`     | `Utilities/AssetsManagment/**`                                                                               | `AddressablesLoaderService`, `UniTask`, `Resources` как fallback, `AddressablesLabels`                                                                                                                                                                                                                                                                     |
| `scenes-and-layers.md`  | `Utilities/SceneManagment/**`, `Layers.cs`                                                                   | `Scenes.GAMEPLAY` как `const string`, `Layers` через `LayerMask.NameToLayer`                                                                                                                                                                                                                                                                               |
| `editor-only.md`        | `Editor/**/*.cs`                                                                                             | Editor-код в билд не попадает; `UnityEditor` API в Runtime запрещён                                                                                                                                                                                                                                                                                        |
| `generated-readonly.md` | `Generated/**`                                                                                               | НЕ редактировать; регенерация через Jenny                                                                                                                                                                                                                                                                                                                  |
| `code-style.md`         | `**/*.cs`                                                                                                    | Структура файла/класса, именование (camelCase/PascalCase/`_camelCase`/`UPPER_SNAKE_CASE`), форматирование, содержимое                                                                                                                                                                                                                                      |