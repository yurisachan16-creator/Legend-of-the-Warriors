using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 玩家战斗系统 - 处理攻击、技能等
/// </summary>
public class PlayerCombat : MonoBehaviour
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
    [SerializeField] private bool _isBlocking = false;
    #endregion

    void Awake()
    {
        _playerAnimation = GetComponent<PlayerAnimation>();
        _inputControl = new PlayerInputControl();

        // 绑定输入事件
        // _inputControl.Gameplay.Attack.performed += OnAttack;
        // _inputControl.Gameplay.HeavyAttack.performed += OnHeavyAttack;
        // _inputControl.Gameplay.Spell.performed += OnSpell;
        // _inputControl.Gameplay.Slide.performed += OnSlide;
        // _inputControl.Gameplay.Block.performed += OnBlockPressed;
        // _inputControl.Gameplay.Block.canceled += OnBlockReleased;
    }

    void OnEnable()
    {
        _inputControl?.Enable();
    }

    void OnDisable()
    {
        _inputControl?.Disable();
    }

    #region 输入处理

    /// <summary>
    /// 普通攻击输入
    /// </summary>
    private void OnAttack(InputAction.CallbackContext context)
    {
        if (Time.time - _lastAttackTime < _attackCooldown) return;

        _playerAnimation?.TriggerAttack();
        _lastAttackTime = Time.time;
    }

    /// <summary>
    /// 重击输入
    /// </summary>
    private void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (Time.time - _lastHeavyAttackTime < _heavyAttackCooldown) return;

        _playerAnimation?.TriggerHeavyAttack();
        _lastHeavyAttackTime = Time.time;
    }

    /// <summary>
    /// 施法输入
    /// </summary>
    private void OnSpell(InputAction.CallbackContext context)
    {
        if (Time.time - _lastSpellTime < _spellCooldown) return;

        _playerAnimation?.TriggerSpell();
        _lastSpellTime = Time.time;
    }

    /// <summary>
    /// 滑铲输入
    /// </summary>
    private void OnSlide(InputAction.CallbackContext context)
    {
        if (Time.time - _lastSlideTime < _slideCooldown) return;

        _playerAnimation?.TriggerSlide();
        _lastSlideTime = Time.time;
    }

    /// <summary>
    /// 格挡按下
    /// </summary>
    private void OnBlockPressed(InputAction.CallbackContext context)
    {
        _isBlocking = true;
        _playerAnimation?.SetBlock(true);
    }

    /// <summary>
    /// 格挡释放
    /// </summary>
    private void OnBlockReleased(InputAction.CallbackContext context)
    {
        _isBlocking = false;
        _playerAnimation?.SetBlock(false);
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
            Debug.Log("格挡成功！");
            return;
        }

        _playerAnimation?.TriggerHurt();
        Debug.Log($"受到 {damage} 点伤害！");
    }

    /// <summary>
    /// 死亡
    /// </summary>
    public void Die()
    {
        _playerAnimation?.SetDead(true);
        Debug.Log("玩家死亡！");
    }

    #endregion
}
