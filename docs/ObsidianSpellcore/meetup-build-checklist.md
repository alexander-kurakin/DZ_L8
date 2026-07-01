# Meetup build checklist (Spellcore v3)

**Ветка:** `Spellcore_v3` · **План:** [[plan-v3]] · **Баланс:** [[balance-v3]] · **Питч:** [[00-one-pager]]

---

## Перед сборкой

- [ ] Unity **2022.3.16f1** (как в шаблоне проекта)
- [ ] `git status` чистый или осознанный diff; нет `Missing Script` в консоли на `TowerBrother`
- [ ] Шейдеры `Spellcore/Character` и `Spellcore/CharacterLit` компилируются без ошибок
- [ ] `Tools → GenerateEntityAPI` если добавлялись новые `IEntityComponent` вручную

---

## Сборка

- [ ] **File → Build Settings** → сцена `GameEntryPoint` в списке, **Platform: Windows / macOS** (целевой лэптоп митапа)
- [ ] **Development Build** выключен для демо (или включён только если нужен debug)
- [ ] Build folder: отдельная папка, не поверх старого билда
- [ ] После билда: запуск **без** Unity Editor, cold start < 15 с до главного меню

---

## Smoke test (15 мин)

| # | Сценарий | Ожидание |
| - | -------- | -------- |
| 1 | W1 prep | 1 free mine, LMB **не** бьёт врагов |
| 2 | W1 fight | mine pulses убивают котов на пути; integrity падает при leak |
| 3 | W3 | без турели на спице с драконом — leak / проигрыш |
| 4 | W4 toxic | slow на Outer; дракон не slow |
| 5 | W5 | детерминированный spawn plan; time scale Pause/1×/3× |
| 6 | Win W5 | **~1.2 с** beat → survival offer popup; gold сохраняется |
| 7 | Survival wave 6 | враги на 6+ путях; счётчик `N/5+` |
| 8 | Sell plant | клик только по иконке лопаты (рамка при hover) |
| 9 | Defeat | beat → defeat popup → main menu |
| 10 | Hit juice | mine/turret punch на враге; toxic **без** hitstop |

---

## Митап-day

- [ ] Свой лэптоп + зарядка
- [ ] Проверить билд на **Mac** (если демо на чужом железе)
- [ ] Разрешение экрана 1920×1080; UI readable
- [ ] Отключить уведомления ОС
- [ ] Запасной путь: Play Mode из Editor, если билд не взлетел

---

## Pitch hook (30 с)

«Корми башню essence, расставляй постройки как конвейер на 5 путях, смотри как factory сама разбирает волны — LMB бафает plants, брат чинит башню, после W5 можно уйти в survival.»
