using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 玩家战斗系统 - 使用 Input System
/// </summary>
public class PlayerCombatSimple : MonoBehaviour
{
    #region 组件引用
    private PlayerAnimation _playerAnimation;
    private PlayerInputControl _inputControl;
    #endregion

    #region 战斗参数
    [Header("攻击设置")]
    [SerializeField] private float _attackCooldown = 0.5f;
    private float _lastAttackTime;

    [Header("重击设置")]
    [SerializeField] private float _heavyAttackCooldown = 1f;
    private float _lastHeavyAttackTime;

    [Header("技能设置")]
    [SerializeField] private float _spellCooldown = 2f;
    private float _lastSpellTime;

    [Header("滑铲设置")]
    [SerializeField] private float _slideCooldown = 1.5f;
    private float _lastSlideTime;

    [Header("格挡设置")]
    private bool _isBlocking = false;
    #endregion

    #region 输入状态
    private Keyboard _keyboard;
    private Mouse _mouse;
    #endregion

    void Awake()
    {
        _playerAnimation = GetComponent<PlayerAnimation>();
        _keyboard = Keyboard.current;
        _mouse = Mouse.current;
    }

    void Update()
    {
        if (_keyboard == null) return;
        HandleInput();
    }

    #region 输入处理

    private void HandleInput()
    {
        // 普通攻击 - J 键
        if (_keyboard.jKey.wasPressedThisFrame)
        {
            TryAttack();
        }

        

        // 施法 - L 键
        if (_keyboard.lKey.wasPressedThisFrame)
        {
            TrySpell();
        }

        // 滑铲 - Left Shift 键
        if (_keyboard.ctrlKey.wasPressedThisFrame)
        {
            TrySlide();
        }

        // 格挡 - 鼠标右键（按住）
        if (_mouse != null)
        {
            if (_mouse.rightButton.wasPressedThisFrame)
            {
                SetBlock(true);
            }
            else if (_mouse.rightButton.wasReleasedThisFrame)
            {
                SetBlock(false);
            }
        }

        // 测试受伤 - H 键
        if (_keyboard.hKey.wasPressedThisFrame)
        {
            TakeDamage(10);
        }

        // 测试死亡 - G 键
        if (_keyboard.gKey.wasPressedThisFrame)
        {
            Die();
        }
    }

    private void TryAttack()
    {
        if (Time.time - _lastAttackTime < _attackCooldown) return;

        _playerAnimation?.TriggerAttack();
        _lastAttackTime = Time.time;
        Debug.Log("🗡️ 普通攻击！");
    }

    

    private void TrySpell()
    {
        if (Time.time - _lastSpellTime < _spellCooldown) return;

        _playerAnimation?.TriggerSpell();
        _lastSpellTime = Time.time;
        Debug.Log("✨ 施法！");
    }

    private void TrySlide()
    {
        if (Time.time - _lastSlideTime < _slideCooldown) return;

        _playerAnimation?.TriggerSlide();
        _lastSlideTime = Time.time;
        Debug.Log("💨 滑铲！");
    }

    private void SetBlock(bool isBlocking)
    {
        if (_isBlocking == isBlocking) return;

        _isBlocking = isBlocking;
        _playerAnimation?.SetBlock(isBlocking);
        Debug.Log(isBlocking ? "🛡️ 格挡开始！" : "🛡️ 格挡结束！");
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (_isBlocking)
        {
            Debug.Log("🛡️ 格挡成功！");
            return;
        }

        _playerAnimation?.TriggerHurt();
        Debug.Log($"💔 受到 {damage} 点伤害！");
    }

    /// <summary>
    /// 死亡
    /// </summary>
    public void Die()
    {
        _playerAnimation?.SetDead(true);
        Debug.Log("💀 玩家死亡！");
    }

    /// <summary>
    /// 设置爬梯状态
    /// </summary>
    public void SetClimbing(bool isClimbing)
    {
        _playerAnimation?.SetClimbing(isClimbing);
        Debug.Log(isClimbing ? "🪜 开始爬梯！" : "🪜 停止爬梯！");
    }

    #endregion
}
