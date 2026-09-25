# Health Display

Экранная и мировая индикация здоровья для Unity (UGUI + TextMeshPro) —
эталонная реализация HP Bar из отдельного проекта, интегрированная
в платформер. Подписывается на события компонента `Health`
(см. `Assets/Scripts/Common/Health.cs`) и не содержит собственной
логики здоровья.

## Состав

| Компонент | Назначение |
|-----------|------------|
| `HealthView` | Базовый класс. Ссылка на `Health` назначается явно (валидация в `OnValidate`). Подписка на `Health.Changed` в `OnEnable`, отписка в `OnDisable`, первый рендер в `Start`. |
| `HealthText` | Текстовый индикатор в формате `Текущее здоровье/Максимальное здоровье` (например, `45/100`). |
| `HealthBar` | Бар здоровья на компоненте `Slider`: в `Awake` нормализует шкалу (`min = 0`, `max = 1`), в `Render` записывает долю HP (`CalculateRatio`). |
| `SmoothHealthBar` | Наследник `HealthBar`: корутина лерпит `Slider.value` к целевой доле за `_fillDuration` секунд (по умолчанию `0.5`); новое изменение HP перезапускает корутину с текущего значения — плавность не зависит от размера урона. |
| `HealthChangerButton` → `DamageButton` / `HealButton` | Демо-кнопки: слушают `Button.onClick` и вызывают `Health.TakeDamage` / `Health.Heal`. В платформере адаптированы к сигнатурам локального `Health` (`TakeDamage(amount, sourcePosition)`, `Heal`). |
| `WorldBillboard` | Разворачивает мировой UI к камере в `LateUpdate`; компенсирует поворот персонажа на 180° (SpriteFacing), чтобы бар не «переворачивался» вместе с ним. Компонент платформера — в эталоне его нет, демо экранное. |

## Персональные индикаторы над персонажами

Префаб `Assets/Prefabs/HealthBarWorld.prefab` (мировой Canvas + Slider +
`WorldBillboard`) висит над головой игрока и каждого врага в `SampleScene`.

Эталонный `HealthView` не ищет `Health` у родителя и валидирует ссылку в
`OnValidate`, поэтому компонент `SmoothHealthBar` не хранится в ассете
префаба (иначе ассет вечно сыпал бы ошибкой о пустом `_health` при каждой
загрузке сцены). Инсталлер
(`Tools → Health Display → Install World Bars Into SampleScene`) добавляет
`SmoothHealthBar` на каждый экземпляр бара и явно прописывает туда ссылку
`_slider` и `_health` персонажа, ставит бар над головой (верх
`BoxCollider2D` + 0.25) и компенсирует масштаб персонажа так, чтобы бар
всегда был 0.9 мировых единиц шириной. Старые бары инсталлер сносит
и ставит заново.

## Подключение в своём проекте

1. На объекте с состоянием здоровья должен быть компонент `Health` с событием
   `Changed(int current, int maximum)` и свойствами `Current`/`Maximum`.
2. Добавьте нужные индикаторы (`HealthText`, `HealthBar`, `SmoothHealthBar`) —
   можно все сразу на один экземпляр `Health`.
3. В инспекторе явно назначьте ссылку на `Health` и виджет
   (`TMP_Text` для текста, `Slider` для баров).
4. Индикаторы сами подписываются на `Health.Changed` — дополнительно
   связывать их с `Health` кодом не нужно.

## Демо-сцена

`Assets/Scenes/HealthDemo.unity` — один экземпляр `Health` (100 HP,
неуязвимость отключена), три индикатора (`HealthText`, мгновенный
`InstantHealthBar`, плавный `SmoothHealthBar`) и две деревянные кнопки
(«Урон −10», «Лечение +10») с ручным курсором (`HoverCursor`).

Сцена собирается и валидируется из редактора:
`Tools → Health Demo → Build Demo Scene` / `Validate Demo Scene`
(`HealthDemoSceneBuilder` / `HealthDemoSceneValidator`).

## Зависимости

- Компонент `Health` платформера (`Assets/Scripts/Common/Health.cs`).
- `com.unity.ugui` (Slider, Button) и `com.unity.textmeshpro` (TMP_Text).
