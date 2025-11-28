using UnityEngine;

/// <summary>
/// 玩家动画控制器 - 控制 Animator 状态机参数
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    #region 组件引用
    private Animator _animator;
    private Rigidbody2D _rb;
    private PhysicsCheck _physicsCheck;
    #endregion

    #region Animator 参数哈希值（性能优化）
    private readonly int _hashIsGround = Animator.StringToHash("IsGround");
    private readonly int _hashYVelocity = Animator.StringToHash("yVelocity");
    private readonly int _hashXVelocity = Animator.StringToHash("xVelocity");
    private readonly int _hashJump = Animator.StringToHash("Jump");
    private readonly int _hashAttack = Animator.StringToHash("Attack");
    private readonly int _hashHurt = Animator.StringToHash("Hurt");
    private readonly int _hashHeavyAttack = Animator.StringToHash("HeavyAttack");
    private readonly int _hashSpell = Animator.StringToHash("Spell");
    private readonly int _hashSlide = Animator.StringToHash("Slide");
    private readonly int _hashIsSliding = Animator.StringToHash("IsSliding");
    private readonly int _hashIsClimbing = Animator.StringToHash("IsClimbing");
    private readonly int _hashIsDead = Animator.StringToHash("IsDead");
    private readonly int _hashBlock = Animator.StringToHash("Block");
    #endregion

    #region 状态标志
    [Header("状态标志")]
    [SerializeField] private bool _isSliding = false;
    [SerializeField] private bool _isClimbing = false;
    [SerializeField] private bool _isDead = false;
    [SerializeField] private bool _isBlocking = false;
    #endregion

    #region 参数调试
    [Header("调试信息")]
    [SerializeField] private float _currentXVelocity;
    [SerializeField] private float _currentYVelocity;
    [SerializeField] private bool _currentIsGround;
    #endregion

    #region Unity 生命周期

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _physicsCheck = GetComponent<PhysicsCheck>();

        if (_animator == null)
        {
            Debug.LogError("PlayerAnimation: Animator 组件未找到！");
        }
        if (_rb == null)
        {
            Debug.LogError("PlayerAnimation: Rigidbody2D 组件未找到！");
        }
        if (_physicsCheck == null)
        {
            Debug.LogError("PlayerAnimation: PhysicsCheck 组件未找到！");
        }
    }

    void Update()
    {
        if (_animator == null || _rb == null || _physicsCheck == null) return;

        UpdateAnimatorParameters();
    }

    #endregion

    #region 动画参数更新

    /// <summary>
    /// 每帧更新 Animator 参数
    /// </summary>
    private void UpdateAnimatorParameters()
    {
        // 更新地面检测状态
        _currentIsGround = _physicsCheck.IsGrounded;
        _animator.SetBool(_hashIsGround, _currentIsGround);

        // 更新速度参数（用于 Blend Tree）
        _currentXVelocity = Mathf.Abs(_rb.velocity.x);
        _currentYVelocity = _rb.velocity.y;
        
        _animator.SetFloat(_hashXVelocity, _currentXVelocity);
        _animator.SetFloat(_hashYVelocity, _currentYVelocity);

        // 更新状态标志
        _animator.SetBool(_hashIsSliding, _isSliding);
        _animator.SetBool(_hashIsClimbing, _isClimbing);
        _animator.SetBool(_hashIsDead, _isDead);
        _animator.SetBool(_hashBlock, _isBlocking);
    }

    #endregion

    #region 公共方法 - 触发动画

    /// <summary>
    /// 触发跳跃动画
    /// </summary>
    public void TriggerJump()
    {
        if (_animator != null && !_isDead)
        {
            _animator.SetTrigger(_hashJump);
        }
    }

    /// <summary>
    /// 触发普通攻击动画
    /// </summary>
    public void TriggerAttack()
    {
        if (_animator != null && !_isDead)
        {
            _animator.SetTrigger(_hashAttack);
        }
    }

    /// <summary>
    /// 触发重击动画
    /// </summary>
    public void TriggerHeavyAttack()
    {
        if (_animator != null && !_isDead)
        {
            _animator.SetTrigger(_hashHeavyAttack);
        }
    }

    /// <summary>
    /// 触发施法动画
    /// </summary>
    public void TriggerSpell()
    {
        if (_animator != null && !_isDead)
        {
            _animator.SetTrigger(_hashSpell);
        }
    }

    /// <summary>
    /// 触发滑铲动画
    /// </summary>
    public void TriggerSlide()
    {
        if (_animator != null && !_isDead && !_isSliding)
        {
            _animator.SetTrigger(_hashSlide);
            _isSliding = true;
        }
    }

    /// <summary>
    /// 触发受伤动画
    /// </summary>
    public void TriggerHurt()
    {
        if (_animator != null && !_isDead)
        {
            _animator.SetTrigger(_hashHurt);
        }
    }

    /// <summary>
    /// 设置死亡状态
    /// </summary>
    public void SetDead(bool isDead)
    {
        _isDead = isDead;
        if (_animator != null)
        {
            _animator.SetBool(_hashIsDead, _isDead);
        }
    }

    /// <summary>
    /// 设置格挡状态
    /// </summary>
    public void SetBlock(bool isBlocking)
    {
        _isBlocking = isBlocking;
        if (_animator != null && !_isDead)
        {
            _animator.SetBool(_hashBlock, _isBlocking);
        }
    }

    /// <summary>
    /// 设置爬梯状态
    /// </summary>
    public void SetClimbing(bool isClimbing)
    {
        _isClimbing = isClimbing;
        if (_animator != null && !_isDead)
        {
            _animator.SetBool(_hashIsClimbing, _isClimbing);
        }
    }

    /// <summary>
    /// 结束滑铲（由动画事件或代码调用）
    /// </summary>
    public void EndSlide()
    {
        _isSliding = false;
    }

    #endregion

    #region 动画事件回调（可选）

    // 这些方法可以在动画事件中调用

    /// <summary>
    /// 攻击动画中的伤害判定帧
    /// </summary>
    public void OnAttackHit()
    {
        // 在这里处理攻击判定逻辑
        Debug.Log("攻击判定触发！");
    }

    /// <summary>
    /// 滑铲动画结束回调
    /// </summary>
    public void OnSlideAnimationEnd()
    {
        EndSlide();
    }

    /// <summary>
    /// 落地动画结束回调
    /// </summary>
    public void OnLandAnimationEnd()
    {
        // 落地动画完成后的处理
    }

    #endregion

    #region 工具方法

    /// <summary>
    /// 获取当前动画状态信息（调试用）
    /// </summary>
    public AnimatorStateInfo GetCurrentStateInfo(int layerIndex = 0)
    {
        return _animator.GetCurrentAnimatorStateInfo(layerIndex);
    }

    /// <summary>
    /// 检查是否在播放指定动画
    /// </summary>
    public bool IsPlayingAnimation(string stateName, int layerIndex = 0)
    {
        return _animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(stateName);
    }

    #endregion
}
