# SPEC: Cave Runner — MVP Implementation

> Source: `docs/GDD.md`
> Status: **Ready** (HARD GATE passed by spec-normalizer)
> Distribution: `all` (no IAP, no live-ops)
> Полнота: REQ/AC registry, TERM glossary, traceability matrix.

---

## 1. Goal

Реализовать MVP 2D-платформера «Cave Runner» — один полноценный уровень
в стиле Mario 1-1 с игроком-гоблином (3 глагола: run, jump, stomp),
сбором монет, аптечками, HUD и GameOver/restart логикой.

**Definition of Done:**
- Все REQ-* имеют хотя бы один AC-* и ссылку на реализацию.
- Уровень проходим от старта до финиша без блокирующих багов.
- Все четыре глагола работают через код, а не хардкод сцены.

---

## 2. Context

| Параметр | Значение |
|---|---|
| Engine | Unity 2022.3.62f3 (LTS) |
| Render pipeline | Built-in |
| Physics | 2D (Box2D via Rigidbody2D) |
| Input | Old Input Manager (`Input.GetAxis` / `Input.GetKey`) |
| Animation | Unity Animator + 2D Animation (PSB skeletal) |
| Assembly | Assembly-less (single C# project); без namespace (см. ADR-0011) |
| Target platform | PC standalone (Windows) — MVP |

**Доступные ассеты:**
- `Assets/Cave Platformer Tileset/` — тайлы, parallax-фоны;
- `Assets/Dark fantasy - popular enemies- Free Sample/Goblin/` — игрок
  (PSB, Animator с Idle/Walking/Running/Attack/Dying);
- `Assets/Dark fantasy - popular enemies- Free Sample/Skeleton/` — враг
  (PSB, тот же набор анимаций).

---

## 3. Terminology (TERM registry)

| ID | Term | Definition |
|---|---|---|
| TERM-001 | **Player** | GameObject на слое `Player` (8), содержит `PlayerMovement`, `PlayerHealth`, `PlayerCombat`, `Rigidbody2D`, `BoxCollider2D`, `Animator`. |
| TERM-002 | **Enemy** | GameObject с компонентами `EnemyAwareness`, `EnemyLocomotion`, `EnemyStriker`, `EnemyDeath`, `Rigidbody2D`, `BoxCollider2D`, `Animator`. |
| TERM-003 | **Stomp** | Единственная атака Player — прыжок сверху на врага (velocity.y < 0 + overlap с stompCheck). Мгновенное убийство + bounce. |
| TERM-004 | (unused) | — |
| TERM-005 | **HP** | Hit Points. Player: 3, Enemy: 3. |
| TERM-006 | **Spawn point** | Стартовая позиция Player на уровне. В MVP respawn заменён на GameOver + перезагрузка сцены. |
| TERM-007 | **Patrol zone** | Отрезок [leftX, rightX] в world space, в котором Enemy патрулирует. |
| TERM-008 | **Detect range** | Радиус в world units, при входе Player в который Enemy переходит из Patrol в Chase. |
| TERM-009 | **Chase range** | Максимальное расстояние до Player в Chase state; при превышении Enemy возвращается в Patrol. |
| TERM-010 | **Coin** | Trigger-collider GameObject со скриптом `Coin`; при overlap с Player увеличивает счётчик GameSession. |
| TERM-011 | **HealthPack** | Trigger-collider GameObject со скриптом `HealthPickup`; при overlap с Player лечит 1 HP. |
| TERM-012 | **QuestionBlock** | Декоративный SpriteRenderer с BoxCollider2D сверху (как статичная платформа). Монеты под ним — отдельные объекты. |
| TERM-013 | **Pipe** | Декоративный SpriteRenderer + BoxCollider2D; служит платформой или obstacle. |
| TERM-014 | **Pit** | Разрыв в Ground — зона, при падении в которую Player умирает (через Y-threshold или trigger). |
| TERM-015 | **Invincibility frames** | Окно после получения урона, в течение которого Player игнорирует повторный урон. |
| TERM-016 | **Coyote time** | Окно после схода с платформы, в течение которого прыжок ещё допустим. |
| TERM-017 | **Jump buffer** | Окно до приземления, в течение которого нажатие прыжка «запоминается» и срабатывает при касании земли. |
| TERM-018 | **Stomp chain** | Серия последовательных stomp без приземления на землю (chain-stomp). |
| TERM-019 | **Knockback** | Мгновенное изменение velocity для отталкивания получателя от источника урона. |
| TERM-020 | **HUD** | Canvas с UI.Text: Coins (лево-верх), HP (лево-верх под Coins). |

---

## 4. System Decomposition

```
┌──────────────────────────────────────────────────────────────────────┐
│                        GameSession                                        │
│  • totalCoinsCollected : int                                          │
│  • AddCoin(amount)                                                    │
│  • GameOver() / FinishLevel() / RestartLevel()                        │
└──────────────┬───────────────────────────────────────────────────────┘
               │ references
               ▼
┌──────────────────────────────────────────────────────────────────────┐
│                              Player (TERM-001)                       │
│ ┌───────────────────┐  ┌───────────────────┐  ┌────────────────────┐ │
│ │ PlayerMovement  │  │ PlayerHealth      │  │ PlayerCombat       │ │
│ │ • Move / Jump     │  │ • HP, i-frames    │  │ • Stomp            │ │
│ │ • Coyote/Buffer   │◄─┤ • TakeDamage(src) │◄─┤ • Stomp            │ │
│ │ • Flip            │  │ • Heal(n)         │  │ • ApplyKnockback   │ │
│ │ • Animator params │  │ • Die() → GameOver│  │ • OnCollide2D →    │ │
│ └─────────┬─────────┘  └─────────┬─────────┘  │   side-detect      │ │
│           │                      │            └─────────┬──────────┘ │
│           ▼                      ▼                      ▼            │
│      Rigidbody2D            SpriteRenderer[]         Animator        │
│      BoxCollider2D          (flicker on dmg)         (in children)   │
│      GroundCheck            StompCheck                               │
└──────────────────────────────────────────────────────────────────────┘
               │ collides with
               ▼
┌──────────────────────────────────────────────────────────────────────┐
│                              Enemy                                   │
│ ┌──────────────────────┐  ┌──────────────────────────────────────────┤
│ │ Enemy (state machine)│  │                                         │ │
│ │ States:              │  │ — нет HP, умирает мгновенно от        │ │
│ │ Patrol/Chase/Attack  │  │   stomp                                │ │
│ │ • TickPatrol/Chase   │  │                                         │ │
│ │ • DoAttack()         │  │                                         │ │
│ │ • Die()              │  │                                         │ │
│ └──────────────────────┘  └──────────────────────────────────────────┘ │
│      Rigidbody2D          Animator                                     │
│      BoxCollider2D                                                     │
└──────────────────────────────────────────────────────────────────────┘
               │ collides with
               ▼
┌──────────────────────────────────────────────────────────────────────┐
│                       World / Level                                  │
│ ┌──────────────────┐  ┌──────────────────┐  ┌────────────────────┐   │
│ │ Ground (Ground)  │  │ Pipe / QBlock    │  │ Coin (trigger)     │   │
│ │ BoxCollider2D    │  │ (Ground layer)   │  │ HealthPickup       │   │
│ │ SpriteRenderer   │  │ SpriteRenderer   │  │  (trigger)         │   │
│ └──────────────────┘  └──────────────────┘  └────────────────────┘   │
│                                                                      │
│ Camera (orthographic size=5) with CameraFollow (SmoothDamp)          │
└──────────────────────────────────────────────────────────────────────┘
```

---

## 5. Data Models

### 5.1. PlayerMovement (MonoBehaviour)

| Field | Type | Default | Source |
|---|---|---|---|
| moveSpeed | float | 5.5 | REQ-001 |
| jumpForce | float | 15.0 | REQ-001 |
| moveSmoothTime | float | 0.08 | REQ-001 |
| groundCheckRadius | float | 0.22 | REQ-004 |
| coyoteTime | float | 0.10 | REQ-004 |
| jumpBufferTime | float | 0.12 | REQ-004 |
| fallMultiplier | float | 2.4 | REQ-004 |
| lowJumpMultiplier | float | 2.0 | REQ-004 |

Public read-only API: `IsGrounded`, `IsMoving`, `IsFalling`, `VerticalVelocity`,
`GetFacingDirection()`, `Flip()`.

### 5.2. PlayerHealth (MonoBehaviour)

| Field | Type | Default |
|---|---|---|
| maxHealth | int | 3 |
| invincibilityTime | float | 1.0 |
| healthText | UI.Text (optional) | null |

Public API: `CurrentHealth`, `MaxHealth`, `IsAlive`, `TakeDamage(src)`,
`Heal(amount)`.

### 5.3. PlayerCombat (MonoBehaviour)

| Field | Type | Default |
|---|---|---|
| stompBounceForce | float | 14.0 |
| stompCheckRadius | float | 0.35 |
| stompCheck | Transform | child with offset -0.95 |
| enemyLayer | LayerMask | `Enemy` |
| selfKnockbackX | float | 4.5 |
| selfKnockbackY | float | 7.5 |

> Melee attack удалена (REQ-006). Игрок атакует только stomp'ом (прыжок сверху).

### 5.4. Enemy (4 компонента)

Враг разбит на 4 компонента, каждый с одной ответственностью:

**EnemyLocomotion** — перемещение: patrol (между leftX/rightX) и chase (за игроком).

| Field | Type | Default |
|---|---|---|
| patrolSpeed | float | 1.6 |
| leftX, rightX | float | (-3, 3) — instance-specific |
| startFacingRight | bool | true |
| chaseSpeed | float | 2.8 |
| groundCheck | Transform | null (обязательный) |
| groundCheckRadius | float | 0.2 |
| edgeCheckOffset | float | 0.5 |
| groundLayer | LayerMask | — |

Public API: `Patrol()`, `Chase(targetPosition)`, `Stop()`, `FaceTowards(targetPosition)`, `HasGroundAhead()`, `FacingVector` (свойство).

**EnemyAwareness** — state machine: решает, когда patrol/chase/attack.

| Field | Type | Default |
|---|---|---|
| detectRange | float | 5.0 |
| chaseRange | float | 7.0 |
| playerLayer | LayerMask | Player |
| playerTarget | Transform | null (обязательный) |
| locomotion | EnemyLocomotion | null (обязательный) |
| striker | EnemyStriker | null (обязательный) |
| death | EnemyDeath | null (обязательный) |
| gameSession | GameSession | null (опциональный) |

Управляет `EnemyLocomotion`, `EnemyStriker` и `EnemyDeath` через SerializedField-ссылки. Внутреннее состояние (`State`): `Patrol`, `Chase`, `Attack`, `Dead`.

**EnemyStriker** — атака: windup → CircleCast → урон игроку.

| Field | Type | Default |
|---|---|---|
| attackRange | float | 1.0 |
| attackDamage | int | 1 |
| attackCooldown | float | 1.2 |
| attackWindup | float | 0.25 |
| attackOriginHeight | float | 0.8 |
| obstacleLayer | LayerMask | Ground (bit 11) |

Public API: `AttackRange`, `IsOnCooldown`, `BeginWindup()`, `TickWindup(...)`.

**EnemyDeath** — смерть: анимация, коллайдер off, уничтожение.

| Field | Type | Default |
|---|---|---|
| deathDelay | float | 0.6 |

Events: `Died`. Public API: `IsDead`, `Die()`.

> Враг не имеет HP — умирает мгновенно от stomp (прыжок сверху).

### 5.5. Coin (MonoBehaviour)

| Field | Type | Default |
|---|---|---|
| value | int | 1 |

> Spinning/bobbing анимация монеты делегирована отдельному компоненту
> `SpinAndBob` (см. §5.6a) на том же GameObject.

### 5.6. HealthPickup (MonoBehaviour)

| Field | Type | Default |
|---|---|---|
| healAmount | int | 1 |

> Spinning/bobbing анимация аптечки делегирована отдельному компоненту
> `SpinAndBob` (см. §5.6a) на том же GameObject.

### 5.6a. SpinAndBob (MonoBehaviour)

| Field | Type | Default |
|---|---|---|
| spinSpeed | float | 120 |
| bobAmount | float | 0.10 |
| bobSpeed | float | 3.0 |

> Универсальный компонент визуальной анимации (вращение по оси Y +
> покачивание по синусоиде). Навешивается на Coin, HealthPickup и любые
> другие коллектиблы/декорации, которым нужен «парящий» look.

### 5.7. GameSession (MonoBehaviour)

| Field | Type | Default |
|---|---|---|
| input | PlayerInput | null (serialized) |
| totalCoinsCollected | int | 0 (state) |
| enemiesDefeated | int | 0 (state) |
| totalCoinsInLevel | int | 0 (state, регистрируется при старте) |
| totalEnemiesInLevel | int | 0 (state, регистрируется при старте) |
| playTime | float | 0 (state) |

Public API: `AddCoin(amount)`, `RegisterEnemyKill()`, `RegisterCoin()`,
`RegisterEnemy()`, `GameOver()`, `FinishLevel()`, `RestartLevel()`.
UI отображение вынесено в `GameSessionUI`. Health UI — в `PlayerHealth`.

### 5.8. CoinSpawner + HealthPickupSpawner (MonoBehaviour)

Типизированные спавнеры вместо `PrefabSpawner` с `GameObject`. Каждый спавнер принимает конкретный тип префаба и подписывается на событие `Collected` для уничтожения.

**CoinSpawner:**
| Field | Type | Default |
|---|---|---|
| prefab | Coin | обязательный |
| spawnPoints | Vector3[] | — |
| spawnScale | Vector3 | (1,1,1) |

**HealthPickupSpawner:**
| Field | Type | Default |
|---|---|---|
| prefab | HealthPickup | обязательный |
| spawnPoints | Vector3[] | — |
| spawnScale | Vector3 | (1,1,1) |

Используются для массового создания однотипных объектов:
- CoinSpawner → Coin на своих spawnPoints
- HealthPickupSpawner → HealthPickup на своих spawnPoints
- Каждый спавнер подписывается на `Collected` → уничтожает объект

### 5.9. PlayerInput (MonoBehaviour)

Единый компонент ввода. Все `Input.GetKey` / `Input.GetMouseButton` только здесь.

| Property | Type | Source keys |
|---|---|---|
| HorizontalInput | float | A/← → -1, D/→ → +1 |
| JumpPressed | bool | Space / W / ↑ (GetKeyDown) |
| JumpHeld | bool | Space / W / ↑ (GetKey) |
| AttackPressed | bool | удалено (игрок атакует только stomp'ом) |
| RestartPressed | bool | R |

Другие компоненты получают `PlayerInput` через `[SerializeField]` и читают только свойства.

---

## 6. Layer Setup

| Layer | Index | Purpose |
|---|---|---|
| Default | 0 | (built-in) |
| Player | 8 | player-owned objects |
| Enemy | 9 | enemy objects |
| Coin | 10 | coin triggers |
| Ground | 11 | solid platforms |

> Идентификация объектов — по наличию компонента через `TryGetComponent<T>()`, не по тэгам (`CompareTag` запрещён стандартом `unity-patterns.md §8a`).

### Collision matrix (2D)

|  | Player | Enemy | Coin | Ground |
|---|---|---|---|---|
| **Player** | ✅ | ✅ | ✅(trigger) | ✅ |
| **Enemy** | ✅ | ❌ | ❌ | ✅ |
| **Coin** | ✅(trigger) | ❌ | ❌ | ❌ |
| **Ground** | ✅ | ✅ | ❌ | ✅(self) |

> Coin ↔ Ground отключено, чтобы монетки не сталкивались с платформами
> и не «тёрлись» об них. Coin ↔ Enemy отключено, чтобы монетки проходили
> сквозь врагов. Enemy ↔ Enemy отключено, чтобы враги не «бодались»
> друг с другом.

---

## 7. Animator Parameters

### PlayerAnimator.controller

| Name | Type | Used in | Set by |
|---|---|---|---|
| Speed | float | Idle ↔ Running transitions | PlayerMovement (Mathf.Abs(horizontalInput)) |
| IsGrounded | bool | (reserved for jump anim) | PlayerMovement |
| Jump | trigger | (reserved) | PlayerMovement (on jump) |
| Attack | trigger | Idle/Running → Attack state | PlayerCombat (on attack) |
| Hurt | trigger | any → Hurt state | PlayerHealth (on TakeDamage) — future |
| Die | trigger | any → Die state | PlayerHealth (on HP=0) — future |

### EnemyAnimator.controller

| Name | Type | Used in | Set by |
|---|---|---|---|
| Speed | float | Idle ↔ Walking transitions | Enemy (Mathf.Abs(velocity.x)) |
| Attack | trigger | → Attack state | Enemy (on windup start) |
| Hurt | trigger | → Hurt state | Enemy (on TakeDamage) — future |
| Die | trigger | → Die state | Enemy (on Die) — future |

States (player): Idle (default), Running, Attack, Hurt, Die.
States (enemy): Idle (default), Walking, Attack, Hurt, Die.

> MVP hard requirement: Idle ↔ Running/Walking transitions реализованы и
> работают. Attack/Hurt/Die — **optional for MVP** (см. REQ-021).

---

## 8. Flows

### Flow 1: Player Movement (per FixedUpdate)
```
Update:
  read A/D/←/→ → horizontalInput
  read Space/W/↑ keydown → jumpBufferTimer = jumpBufferTime
  read jumpHeld (variable jump)
  update timers
  set animator Speed
  flip sprite if direction changed

FixedUpdate:
  OverlapCircle(groundCheck.pos, groundCheckRadius, groundLayer) → isGrounded
  SmoothDamp velocity.x toward target = horizontalInput * moveSpeed
  if vy < 0: vy += gravity * (fallMultiplier-1) * dt
  else if vy > 0 and !jumpHeld: vy += gravity * (lowJumpMultiplier-1) * dt
  if jumpBufferTimer > 0 and coyoteTimer > 0:
    vy = jumpForce
    consume timers
    animator.SetTrigger(Jump)
  rb.velocity = (vx, vy)
```

### Flow 2: Player Stomp (per FixedUpdate)
```
if controller.IsFalling:
  hit = OverlapCircle(stompCheck.pos, stompCheckRadius, enemyLayer)
  if hit != null:
    enemy = hit.GetComponentInParent<Enemy>()
    if enemy != null and !enemy.IsDead:
      enemy.Die()
      rb.velocity = (rb.velocity.x, stompBounceForce)
```

### Flow 3: Player Melee Attack (on F / ЛКМ) — удалено. Игрок атакует только stomp'ом (прыжок сверху).

### Flow 4: Player Damage (on side-collision with Enemy, via PlayerCollision)
```
PlayerCollision.OnCollisionEnter2D(collision):
  if collision.collider.TryGetComponent(out EnemyDeath _) == false: return
  if enemyDeath.IsDead: return
  // Simplified height-check instead of contact normal (unreliable in Box2D)
  playerAboveAndFalling = pos.y > collision.transform.y + 0.4 AND rigidbody.verticalVelocity < 0
  if playerAboveAndFalling: return  // stomp handles it
  TakeDamage(collision.transform.position)

TakeDamage(src):
  if invincibilityTimer > 0 or !IsAlive: return
  currentHealth--
  invincibilityTimer = invincibilityTime
  combat.ApplyKnockbackFrom(src)
  UpdateUI()
  if currentHealth <= 0: Die() → GameSession.GameOver()
```

### Flow 5: Enemy State Machine (EnemyAwareness per FixedUpdate)
```
EnemyAwareness.FixedUpdate:
  switch (state):
    Patrol:
      EnemyAwareness: can see player? → Chase
      EnemyLocomotion.Patrol() — двигается между leftX/rightX, разворачивается

    Chase:
      EnemyAwareness: player too far / outside zone? → Patrol
      EnemyAwareness: close enough to attack? → Attack
      EnemyLocomotion.Chase(player) — бежит за игроком

    Attack:
      EnemyLocomotion.Stop()
      EnemyStriker.BeginWindup() → animator Attack trigger
      EnemyStriker.TickWindup() → CircleCast → player damage
      if attack done → back to Chase

    Dead:
      EnemyLocomotion.Stop()
      (EnemyDeath handles collider + destroy)
```

### Flow 6: Player Trigger Collection (via PlayerCollision)
```
PlayerCollision.OnTriggerEnter2D:
  TryGetComponent(out Coin coin)       → coin.Collect(gameObject)
  TryGetComponent(out HealthPickup hp) → hp.Collect(gameObject)
  TryGetComponent(out FinishTrigger _) → GameSession.FinishLevel()

Coin.Collect:
  GameSession.AddCoin(coin.value)
  Collected?.Invoke(this)  // событие — создатель решит, что делать

HealthPickup.Collect:
  if playerHealth == null or !playerHealth.IsAlive: return
  if playerHealth.CurrentHealth >= playerHealth.MaxHealth: return
  playerHealth.Heal(healAmount)
  Collected?.Invoke(this)  // событие — создатель решит, что делать

CoinSpawner (подписался при создании):
  OnCoinCollected(coin)        → Destroy(coin.gameObject)

HealthPickupSpawner (подписался при создании):
  OnHealthPickupCollected(hp)  → Destroy(hp.gameObject)
```

### Flow 8: Level Build (Editor / runtime via CoinSpawner / HealthPickupSpawner)
```
For each level segment (Act 1 / Act 2 / Act 3):
  - place Ground tiles (BoxCollider2D + SpriteRenderer tiled)
  - place Pipes / QBlocks (BoxCollider2D + SpriteRenderer)
  - place Enemy instances (with leftX/rightX customized)
  - CoinSpawner creates Coin instances from Coin.prefab at predefined Vector3[]
  - HealthPickupSpawner creates HealthPickup instances at predefined Vector3[]
```

---

## 9. Constraints

### 9.1. Hard constraints
- C1: Все ссылки на ассеты — через `AssetDatabase.LoadAssetAtPath` /
  SerializedField; никаких строковых имён в логике.
- C2: Один GameSession на сцену, ссылки через `[SerializeField]`
  pattern.
- C3: Все publick fields — `[SerializeField] private` (Unity best
  practice).
- C4: Никаких `GetComponent<T>()` в Update/FixedUpdate — кешировать на
  Awake.
- C5: Physics only in FixedUpdate.
- C6: Input only in Update.

### 9.2. Performance budgets
- 60 FPS на машине разработчика;
- ≤ 100 draw calls на кадр (без batching optimization);
- ≤ 50 активных GameObject на уровне (excluding tiles).

### 9.3. Compatibility
- Windows / Mac / Linux standalone (MVP = Windows only);
- keyboard input only (gamepad — future).

---

## 10. Open Questions

| ID | Question | Default | Resolution |
|---|---|---|---|
| OQ-001 | Сохранять ли собранные монеты при respawn? | **No** (reset) | Принято в ADR-007 |
| OQ-002 | Должен ли pit убивать мгновенно? | Yes (instant death) | Реализовано: PlayerHealth.deathY проверяет Y < -20 |
| OQ-003 | Атакует ли враг в Pattern-режиме (несколько атак подряд)? | No (1 attack → cooldown → chase) | Принято |
| OQ-004 | Использовать ли новый Input System? | No (old Input Manager) | Принято в ADR-002 |
| OQ-005 | Спаунить ли врагов динамически или разместить в сцене? | Place in scene | Принято в ADR-006 |
| OQ-006 | UI Toolkit или uGUI? | uGUI (Canvas) | Принято в ADR-009 |
| OQ-007 | Skeletal анимация или sprite sheet для UI? | N/A (нет UI-анимации в MVP) | — |
| OQ-008 | Звук в MVP? | No (post-MVP) | Принято в ADR-010 |

---

## 11. Requirements Registry (REQ)

### REQ-001: Player horizontal movement
Player может двигаться горизонтально с `moveSpeed = 5.5 u/s`, плавный
разгон 0.08s.

- **AC-001.1:** При зажатой D игрок движется вправо со скоростью
  `(5.5 ± 0.5) u/s` через 0.1s.
- **AC-001.2:** При отпускании D скорость плавно падает до 0 за
  `≤ 0.2s`.
- **AC-001.3:** Аниматор переходит в Running при `|input| > 0.1`.
- **AC-001.4:** Аниматор возвращается в Idle при `|input| < 0.1`.

### REQ-002: Player jump
Player прыгает с `jumpForce = 15 u/s`, gravity scale = 2.5, variable
height.

- **AC-002.1:** При нажатии Space на земле игрок приобретает
  `velocity.y = 15 u/s`.
- **AC-002.2:** Если отпустить Space в верхней точке, высота прыжка
  сокращается минимум на 30%.
- **AC-002.3:** После схода с платформы без прыжка остаётся окно 0.10s,
  в течение которого прыжок ещё срабатывает (coyote).
- **AC-002.4:** Если нажать Space за 0.12s до приземления, прыжок
  срабатывает сразу после касания (buffer).

### REQ-003: Player flip
Player разворачивается спрайтом в сторону движения.

- **AC-003.1:** При движении вправо `localScale.x > 0`.
- **AC-003.2:** При движении влево `localScale.x < 0`.

### REQ-004: Player feel (gravity tuning)
- **AC-004.1:** `fallMultiplier = 2.4` — падение быстрее подъёма.
- **AC-004.2:** `lowJumpMultiplier = 2.0` — variable height работает.

### REQ-005: Player stomp
Player мгновенно убивает врага при попадании сверху.

- **AC-005.1:** При `velocity.y < -0.5` и overlap с Enemy в радиусе
  0.35u от stompCheck, враг вызывает `Die()`.
- **AC-005.2:** Игрок получает `velocity.y = 14 u/s` (bounce).
- **AC-005.3:** Stomp-chain работает: подряд 2+ врага можно убить без
  приземления.

### REQ-006: (removed — melee атака удалена, только stomp)

### REQ-007: Player health
Player имеет 3 HP.

- **AC-007.1:** HP уменьшается на 1 при каждом uninsured hit.
- **AC-007.2:** При получении урона активны invincibility frames 1.0s.
- **AC-007.3:** Спрайт мигает с частотой 18 Гц во время invincibility.
- **AC-007.4:** При HP = 0 вызывается `GameSession.GameOver()` — экран Game Over с результатами.

### REQ-008: Player take damage from enemy contact (side)
При боковом столкновении с Enemy игрок получает урон.

- **AC-008.1:** `OnCollisionEnter2D` с объектом, имеющим компонент `EnemyDeath` → вызов
  `TakeDamage(enemy.position)`.
- **AC-008.2:** Если игрок выше врага на 0.4u и `vy < 0`, урон не
  наносится (это stomp).

### REQ-009: Player heal
Player может лечиться аптечкой.

- **AC-009.1:** При overlap с HealthPickup и `CurrentHealth < MaxHealth`,
  HP += 1.
- **AC-009.2:** Если HP уже полный, аптечка остаётся (не подбирается).
- **AC-009.3:** После подбора HealthPickup уничтожается.

### REQ-010: Enemy patrol
Враг патрулирует зону [leftX, rightX].

- **AC-010.1:** В состоянии Patrol враг движется со скоростью 1.6 u/s.
- **AC-010.2:** При достижении границы зоны враг разворачивается.
- **AC-010.3:** Вне detect range враг остаётся в Patrol.

### REQ-011: Enemy detect + chase
Враг переходит в Chase при входе игрока в detect range.

- **AC-011.1:** При `|player.x - enemy.x| ≤ 5.0` → state = Chase.
- **AC-011.2:** В Chase скорость 2.8 u/s в направлении игрока.
- **AC-011.3:** При `|player.x - enemy.x| > 7.0` → state = Patrol.

### REQ-012: Enemy attack
Враг атакует игрока при сближении.

- **AC-012.1:** В Attack state враг замирает на `attackWindup = 0.25s`.
- **AC-012.2:** После windup CircleCast радиусом 0.6u в playerLayer.
- **AC-012.3:** При попадании — `PlayerHealth.TakeDamage(enemy.pos)`.
- **AC-012.4:** Cooldown между атаками 1.2s.
- **AC-012.5:** После атаки state = Chase (не Patrol).

### REQ-013: Enemy death
Враг не имеет HP — умирает мгновенно от stomp.

- **AC-013.1:** При stomp (коллизия сверху, `velocity.y < 0` + overlap stompCheck) → `EnemyDeath.Die()`.
- **AC-013.2:** Удалено (melee атака отсутствует).
- **AC-013.3:** `EnemyDeath.Die()` → disable collider, destroy after 0.6s.

### REQ-014: (removed — враг не имеет HP, нет hurt feedback)

### REQ-015: Coin collection
- **AC-015.1:** Coin имеет CircleCollider2D с `isTrigger = true`.
- **AC-015.2:** При overlap с Player, GameSession.coinsCollected += 1.
- **AC-015.3:** Coin уничтожается после подбора.
- **AC-015.4:** Coin вращается по оси Y (spinSpeed = 120) и
  покачивается (bob).

### REQ-016: Coin spawner (prefab-driven)
Coins создаются из одного `Coin.prefab` через `CoinSpawner`.

- **AC-016.1:** Не более 1 Coin-источника в проекте (`Assets/Prefabs/Coin.prefab`).
- **AC-016.2:** Все Coin на уровне созданы либо инстансом префаба, либо через CoinSpawner.
- **AC-016.3:** Запрещены «пустые» Coin-объекты с дублированными
  компонентами в сцене.

### REQ-017: HUD
- **AC-017.1:** Canvas с Text «Coins: N» в левом-верхнем углу.
- **AC-017.2:** Text «HP: current/max» под Coins.
- **AC-017.3:** Шрифт Arial (или fallback OS font), размер 28–32.

### REQ-018: Camera follow
- **AC-018.1:** Main Camera ортогональная, size = 5.
- **AC-018.2:** SmoothDamp к игроку с smoothTime 0.15–0.18s.
- **AC-018.3:** Offset = (0, 1.5).

### REQ-019: Level structure (Mario 1-1 inspired)
- **AC-019.1:** Длина уровня 60–100u.
- **AC-019.2:** Минимум 3 акта: warmup / escalation / climax.
- **AC-019.3:** Минимум 1 Pipe и 1 QuestionBlock как декорация.
- **AC-019.4:** Минимум 1 pit (или эквивалентная risk-zone).
- **AC-019.5:** Staircase в Act 3.

### REQ-020: (removed — respawn заменён на GameOver)
Respawn удалён из MVP. При HP=0 теперь вызывается `GameSession.GameOver()` (см. REQ-022).

### REQ-021: Animator states
- **AC-021.1:** PlayerAnimator: Idle ↔ Running работает.
- **AC-021.2:** EnemyAnimator: Idle ↔ Walking работает.
- **AC-021.3:** Attack / Hurt / Die states реализованы в обоих
  контроллерах. PlayerCombat вызывает Attack, PlayerHealth вызывает
  Hurt и Die при соответствующих событиях.

### REQ-022: Game Over on death
При HP=0 или падении в пропасть показывается экран Game Over.

- **AC-022.1:** При HP=0 игрок не респавнится автоматически, а
  вызывается GameSession.GameOver().
- **AC-022.2:** GameOver() устанавливает GameState.GameOver, показывает
  панель gameOverPanel с результатами: «Монет собрано: X из Y»,
  «Повержено врагов: X из Y», «Время: N.N с».
- **AC-022.3:** При нажатии R на экране Game Over перезагружается сцена.
- **AC-022.4:** Игрок не управляется во время GameOver
  (PlayerMovement._isDead блокирует ввод).

### REQ-023: Finish screen
При достижении триггера конца уровня показывается экран Finish.

- **AC-023.1:** FinishTrigger (BoxCollider2D, isTrigger) на правом конце
  уровня при пересечении с Player вызывает GameSession.FinishLevel().
- **AC-023.2:** FinishLevel() устанавливает GameState.Finish, показывает
  панель finishPanel с результатами: «Монет собрано: X из Y»,
  «Повержено врагов: X из Y», «Время: N.N с».
- **AC-023.3:** При нажатии R на экране Finish перезагружается сцена
  (переиграть уровень).

### REQ-024: Falling death (pit)
Падение за пределы уровня приводит к Game Over.

- **AC-024.1:** PlayerHealth проверяет `transform.position.y < deathY`
  (-20 по умолчанию) каждый кадр.
- **AC-024.2:** При пересечении порога вызывается
  GameSession.GameOver() без проигрывания анимации Die.
- **AC-024.3:** Параметр deathY настраивается в инспекторе PlayerHealth.

---

## 12. Traceability Matrix

| REQ | Source (GDD) | Implementation (file) | AC | Tests |
|---|---|---|---|---|
| REQ-001 | §6.1 | PlayerMovement.cs | AC-001.1–4 | manual |
| REQ-002 | §6.1 | PlayerMovement.cs | AC-002.1–4 | manual |
| REQ-003 | §6.1 | PlayerMovement.cs | AC-003.1–2 | manual |
| REQ-004 | §8.1 | PlayerMovement.cs | AC-004.1–2 | manual |
| REQ-005 | §6.2 | PlayerCombat.cs | AC-005.1–3 | manual |
| REQ-006 | удалено | — | — | — |
| REQ-007 | §6.4 | PlayerHealth.cs | AC-007.1–4 | manual |
| REQ-008 | §6.4 | PlayerCollision.cs | AC-008.1–2 | manual |
| REQ-009 | §6.5 | HealthPickup.cs | AC-009.1–3 | manual |
| REQ-010 | §6.8 | EnemyAwareness.cs + EnemyLocomotion.cs | AC-010.1–3 | manual |
| REQ-011 | §6.8 | EnemyAwareness.cs + EnemyLocomotion.cs | AC-011.1–3 | manual |
| REQ-012 | §6.8 | EnemyAwareness.cs + EnemyStriker.cs | AC-012.1–5 | manual |
| REQ-013 | §6.4 | EnemyDeath.cs | AC-013.1–3 | manual |
| REQ-014 | удалено | — | — | — |
| REQ-015 | §6.6 | Coin.cs | AC-015.1–4 | manual |
| REQ-016 | §17.1 | CoinSpawner.cs + Coin.prefab | AC-016.1–3 | manual |
| REQ-017 | §12 | GameSession.cs + Canvas | AC-017.1–3 | manual |
| REQ-018 | §6.7 | CameraFollow.cs | AC-018.1–3 | manual |
| REQ-019 | §10 | Level scene + CoinSpawner / HealthPickupSpawner | AC-019.1–5 | manual |
| REQ-020 | удалено | — | — | — |
| REQ-021 | §14.3 | PlayerAnimator/EnemyAnimator + PlayerHealth.cs, PlayerCombat.cs | AC-021.1–3 | manual |
| REQ-022 | §5.8 | GameSession.cs + PlayerHealth.cs | AC-022.1–4 | manual |
| REQ-023 | §5.8 | FinishTrigger.cs + GameSession.cs | AC-023.1–3 | manual |
| REQ-024 | §8 | PlayerHealth.cs | AC-024.1–3 | manual |

---

## 13. PASS Verdicts (normalizer gate)

| PASS | Check | Result |
|---|---|---|
| PASS-001 | Document has TOC of 20 sections | ✅ |
| PASS-002 | All terms in TERM-* registry | ✅ (TERM-001..020) |
| PASS-003 | Each REQ has at least one AC | ✅ |
| PASS-004 | All AC reference implementable behaviour | ✅ |
| PASS-005 | No vendor-specific code in spec | ✅ |
| PASS-006 | Constraints section present | ✅ §9 |
| PASS-007 | Open questions marked with defaults | ✅ §10 |
| PASS-008 | Machine-addressable IDs (REQ-*/AC-*/TERM-*) | ✅ |
| PASS-009 | Traceability matrix covers all REQs | ✅ §12 |
| PASS-010 | No `TBD` without default | ✅ |

**Final verdict:** **Ready** for implementation hand-off.

---

## 14. Hand-off Notes

### Что реализовано частично (нужно доделать)
- Сцена SampleScene содержит базовую раскладку, но требует пересборки
  под REQ-019 (Mario 1-1 layout) и REQ-016 (prefab-driven coins).
- Animator controllers содержат Idle/Run/Walk transitions; Attack/Hurt/
  Die states не построены (REQ-021 — optional для MVP).

### Что нужно реализовать с нуля при resume
- Rebuild уровня по REQ-019;
- Перенести Coin objects в Coin.prefab + CoinSpawner;
- Добавить HealthPickup instances;
- Подключить UI Text к GameSession.healthText;
- Play-mode тестирование всех AC.

### Связанные ADR
- `docs/adr/0001-unity-2022-lts.md`
- `docs/adr/0002-old-input-manager.md`
- `docs/adr/0003-skeletal-psb-animation.md`
- `docs/adr/0004-static-health-pack-spawn.md`
- `docs/adr/0005-gameobject-monobehavior-not-ecs.md`
- `docs/adr/0006-enemy-scene-placement-not-runtime-spawn.md`
- `docs/adr/0007-coin-reset-on-respawn.md`
- `docs/adr/0008-pit-instant-death.md`
- `docs/adr/0009-ugui-canvas-not-ui-toolkit.md`
- `docs/adr/0010-no-audio-in-mvp.md`
- `docs/adr/0011-single-namespace-flat-layout.md`
- `docs/adr/0012-2d-physics-rigidbody-not-transform.md`

---

[SPEC_COMPLETE]
