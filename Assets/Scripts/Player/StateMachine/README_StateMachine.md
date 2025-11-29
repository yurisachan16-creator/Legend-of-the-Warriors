# 玩家状态机使用指南

## 概述

状态机系统将玩家的各种行为（待机、移动、跳跃、攻击等）分离为独立的状态类，每个状态负责管理自己的逻辑，使代码更易于维护和扩展。

## 架构组件

| 组件 | 说明 |
|------|------|
| `PlayerStateMachine` | 核心状态机管理器，处理状态切换和输入 |
| `IPlayerState` | 状态接口，定义状态的基本行为 |
| `PlayerStateBase` | 状态基类，提供共享功能 |
| `PlayerStateData` | 可序列化的状态数据容器 |
| `PlayerEvents` | 单例事件总线，用于模块间通信 |

## 使用方法

### 1. 设置玩家角色

在玩家GameObject上添加以下组件：

```
Player (GameObject)
├── PlayerStateMachine (Script)  ← 添加此组件
├── Animator
├── Rigidbody2D
├── PhysicsCheck
└── AttackHitbox (Child GameObject)
    └── PlayerAttackHitbox (Script)
    └── BoxCollider2D (Trigger)
```

### 2. 配置 PlayerStateMachine

所有参数都可以在Inspector中配置：

```csharp
// PlayerStateData 包含以下可配置参数：
[Header("移动参数")]
public float MoveSpeed = 5f;

[Header("跳跃参数")]
public float JumpForce = 15f;

[Header("攻击参数")]
public int MaxCombo = 3;
public float ComboResetTime = 1f;
// ... 更多参数
```

### 3. 状态切换

状态机通过泛型方法进行状态切换：

```csharp
// 切换到待机状态
stateMachine.ChangeState<IdleState>();

// 切换到移动状态
stateMachine.ChangeState<MoveState>();

// 切换到攻击状态
stateMachine.ChangeState<AttackState>();
```

### 4. 事件系统

使用 `PlayerEvents` 进行模块间通信：

```csharp
// 订阅事件
PlayerEvents.Instance.OnPlayerHurt += HandleHurt;
PlayerEvents.Instance.OnAttackHit += HandleAttackHit;
PlayerEvents.Instance.OnStateChanged += HandleStateChanged;

// 触发事件
PlayerEvents.Instance.TriggerJumpRequested();
PlayerEvents.Instance.TriggerAttackRequested();
PlayerEvents.Instance.TriggerPlayerHurt(attackerTransform);
```

### 5. 攻击判定设置

#### 方法一：动画事件

在攻击动画中添加Animation Events：

1. 在攻击动画开始帧添加事件，调用 `EnableHitbox()`
2. 在攻击动画结束帧添加事件，调用 `DisableHitbox()`

#### 方法二：代码控制

```csharp
// 在 AttackState 中获取并控制 Hitbox
PlayerAttackHitbox hitbox = GetComponentInChildren<PlayerAttackHitbox>();
hitbox.EnableHitbox();  // 启用攻击判定
hitbox.DisableHitbox(); // 禁用攻击判定
```

### 6. 添加新状态

创建新状态只需继承 `PlayerStateBase`：

```csharp
public class MyNewState : PlayerStateBase
{
    public MyNewState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        // 进入状态时的初始化
    }

    public override void Update()
    {
        base.Update();
        // 每帧更新逻辑
        
        // 状态切换示例
        if (SomeCondition)
        {
            StateMachine.ChangeState<IdleState>();
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // 物理更新逻辑
    }

    public override void Exit()
    {
        base.Exit();
        // 退出状态时的清理
    }

    public override bool CanTransitionTo(IPlayerState newState)
    {
        // 定义此状态可以转换到哪些状态
        return true;
    }
}
```

然后在 `PlayerStateMachine.RegisterStates()` 中注册：

```csharp
_states.Add(typeof(MyNewState), new MyNewState(this));
```

## 状态列表

| 状态 | 按键 | 说明 |
|------|------|------|
| `IdleState` | - | 待机状态，无输入时 |
| `MoveState` | WASD/方向键 | 移动状态 |
| `JumpState` | Space | 跳跃状态 |
| `AttackState` | J | 攻击状态（支持三连击） |
| `SlideState` | Ctrl | 滑铲状态 |
| `SpellState` | L | 释放技能状态 |
| `HurtState` | - | 受伤状态（自动触发） |
| `DeathState` | - | 死亡状态（自动触发） |
| `ClimbState` | - | 攀爬状态 |

## 攻击系统详解

### 连击系统

攻击状态支持三连击：
1. 第一次攻击：基础伤害
2. 第二次攻击：1.2倍伤害
3. 第三次攻击：1.5倍伤害

玩家在0.9秒窗口期内按攻击键可以触发下一段连击。

### Animator 三连击设置

在Unity的Animator Controller中设置三连击需要：

1. **添加参数：**
   - `Attack` (Trigger) - 触发攻击
   - `IsAttack` (Bool) - 是否处于攻击状态

2. **创建攻击状态：**
   - `Char_Blue_Attack01` - 第一段攻击
   - `Char_Blue_Attack02` - 第二段攻击  
   - `Char_Blue_Attack03` - 第三段攻击

3. **设置转换条件：**
   ```
   Locomotion → Attack01: IsAttack == true, Attack (Trigger)
   Attack01 → Attack02: Attack (Trigger)
   Attack02 → Attack03: Attack (Trigger)
   Attack01 → Locomotion: IsAttack == false (Has Exit Time)
   Attack02 → Locomotion: IsAttack == false (Has Exit Time)
   Attack03 → Locomotion: IsAttack == false (Has Exit Time)
   ```

4. **添加 ResetTriggerBehaviour：**
   - 在每个攻击状态添加 `ResetTriggerBehaviour` 脚本
   - 设置 `triggerName = "Attack"`
   - 这会在进入状态时重置 Attack 触发器，玩家必须再次按键才能触发下一段

### 攻击流程说明

1. 按下攻击键 → `IsAttack = true`, `Attack` 触发 → 进入 `Attack01`
2. 在0.9秒内再按攻击键 → `Attack` 触发 → 进入 `Attack02`
3. 如果不按键 → 动画结束 → `IsAttack = false` → 返回 `Locomotion`
4. `Attack02` → `Attack03` 同理
5. `Attack03` 结束后 → `IsAttack = false` → 返回 `Locomotion`

### 攻击判定配置

`PlayerAttackHitbox` 组件参数：

```csharp
[Header("攻击参数")]
[SerializeField] private int _baseDamage = 10;        // 基础伤害
[SerializeField] private float _knockbackForce = 5f;  // 击退力度

[Header("连击伤害加成")]
[SerializeField] private float[] _comboDamageMultipliers = { 1f, 1.2f, 1.5f };

[Header("攻击层级")]
[SerializeField] private LayerMask _targetLayers;     // 可攻击的目标层级
```

## 调试

启用调试信息：
- `PlayerAttackHitbox` 上设置 `_showDebugInfo = true`
- 控制台会输出攻击命中信息

在Scene视图中：
- 选中攻击判定对象，Gizmos会显示攻击范围
- 黄色 = 未激活，红色 = 已激活

## 常见问题

### Q: 状态切换不生效？
A: 检查 `CanTransitionTo()` 方法，确保目标状态允许从当前状态转换。

### Q: 攻击不造成伤害？
A: 确保：
1. 目标有 `Character` 组件
2. 目标在 `_targetLayers` 层级中
3. Collider 设置为 Trigger 模式

### Q: 输入不响应？
A: 检查 `PlayerInputControl` 是否正确配置，输入操作是否绑定。
