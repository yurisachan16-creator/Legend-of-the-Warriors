# Player Animation System - 使用说明

## 📁 脚本说明

### 1. PlayerAnimation.cs
**核心动画控制器**，负责更新 Animator 参数和触发动画状态。

#### 主要功能：
- ✅ 自动更新速度参数（`xVelocity`, `yVelocity`）
- ✅ 自动更新地面检测（`IsGround`）
- ✅ 提供触发动画的公共方法

#### 公共方法：
```csharp
// 触发类（Trigger）
playerAnimation.TriggerJump();          // 触发跳跃
playerAnimation.TriggerAttack();        // 触发普攻
playerAnimation.TriggerHeavyAttack();   // 触发重击
playerAnimation.TriggerSpell();         // 触发施法
playerAnimation.TriggerSlide();         // 触发滑铲
playerAnimation.TriggerHurt();          // 触发受伤

// 状态类（Bool）
playerAnimation.SetDead(true);          // 设置死亡
playerAnimation.SetBlock(true);         // 设置格挡
playerAnimation.SetClimbing(true);      // 设置爬梯
playerAnimation.EndSlide();             // 结束滑铲
```

---

### 2. PlayerCombatSimple.cs
**简化版战斗系统**（临时使用键盘输入，后续可改为 Input System）

#### 按键绑定：
| 按键 | 功能 |
|------|------|
| `J` | 普通攻击 |
| `K` | 重击 |
| `L` | 施法 |
| `Left Shift` | 滑铲 |
| `鼠标右键` | 格挡（按住） |
| `H` | 测试受伤 |
| `G` | 测试死亡 |

---

## 🎯 Animator 参数配置

确保你的 Animator Controller 中有以下参数：

| 参数名 | 类型 | 说明 |
|--------|------|------|
| `IsGround` | Bool | 是否在地面 |
| `yVelocity` | Float | 垂直速度（用于跳跃Blend Tree）|
| `xVelocity` | Float | 水平速度（用于移动Blend Tree）|
| `Jump` | Trigger | 跳跃触发 |
| `Attack` | Trigger | 普攻触发 |
| `HeavyAttack` | Trigger | 重击触发 |
| `Spell` | Trigger | 施法触发 |
| `Slide` | Trigger | 滑铲触发 |
| `Hurt` | Trigger | 受伤触发 |
| `IsSliding` | Bool | 是否在滑铲中 |
| `IsClimbing` | Bool | 是否在爬梯 |
| `IsDead` | Bool | 是否死亡 |
| `Block` | Bool | 是否格挡 |

---

## 🔧 使用步骤

### 1. 添加组件
在 Player GameObject 上添加以下组件：
- ✅ `Animator` (已有)
- ✅ `Rigidbody2D` (已有)
- ✅ `PhysicsCheck` (已有)
- ✅ `PlayerAnimation` ⭐ **新增**
- ✅ `PlayerCombatSimple` ⭐ **新增**（可选）

### 2. 配置 Animator Controller
确保 Animator Controller 包含所有必需的参数和状态机结构。

### 3. 在其他脚本中调用
```csharp
// 示例：在 PlayerController 中触发跳跃动画
private PlayerAnimation _playerAnimation;

void Awake()
{
    _playerAnimation = GetComponent<PlayerAnimation>();
}

private void Jump()
{
    // 跳跃逻辑...
    _playerAnimation.TriggerJump();
}
```

---

## 📊 状态机结构

```
Base Layer
├── Locomotion (移动子状态机)
│   └── Blend Tree (Idle/Walk/Run)
├── Airborne (空中子状态机)
│   ├── Jump_Prep → Blend Tree → Jump_Land
│   └── Blend Tree (Jump_Up/Jump_Peak/Jump_Fall)
├── Combat (战斗子状态机)
│   ├── Attack01
│   ├── Attack_Heavy
│   └── SpellCast
├── Actions (特殊动作子状态机)
│   ├── Slide (Start → Loop → End)
│   ├── WallSlide
│   └── Climb
└── Global States (全局状态)
    ├── Death (Any State)
    ├── TakingDamage (Any State)
    └── ShieldDefence (Any State)
```

---

## ⚠️ 注意事项

### 1. Any State 转换设置
- `Any State → Death`: **关闭** Has Exit Time
- `Any State → TakingDamage`: **关闭** Has Exit Time
- `Any State → ShieldDefence`: **关闭** Has Exit Time（或设置为 0.1s）

### 2. 性能优化
- ✅ 使用 `Animator.StringToHash()` 缓存参数哈希值
- ✅ 避免每帧调用 `SetTrigger()`
- ✅ 只在状态改变时更新 Bool 参数

### 3. 后续改进建议
- 🔄 将 `PlayerCombatSimple` 改为使用 Input System
- 🔄 添加动画事件（Animation Events）处理攻击判定
- 🔄 添加音效触发点
- 🔄 添加特效生成点

---

## 🎮 测试说明

1. 运行游戏
2. 使用 `WASD` 移动（Idle/Walk/Run 自动切换）
3. 使用 `Space` 跳跃
4. 使用 `J/K/L` 测试攻击
5. 使用 `Shift` 测试滑铲
6. 使用 `鼠标右键` 测试格挡
7. 使用 `H` 测试受伤
8. 使用 `G` 测试死亡

---

## 📝 动画事件示例

在动画剪辑中可以添加事件，回调到 PlayerAnimation 的方法：

```csharp
// 在攻击动画的伤害判定帧添加事件
public void OnAttackHit()
{
    // 处理攻击判定
}

// 在滑铲动画结束时添加事件
public void OnSlideAnimationEnd()
{
    EndSlide();
}
```

---

## 🐛 常见问题

### Q: 动画不播放？
A: 检查 Animator Controller 是否正确配置参数和转换条件。

### Q: 跳跃动画不流畅？
A: 检查 Airborne Blend Tree 的 yVelocity 参数范围是否正确。

### Q: Any State 警告？
A: 关闭 Any State 转换的 "Has Exit Time" 选项。

---

**最后更新：2025年11月28日**
