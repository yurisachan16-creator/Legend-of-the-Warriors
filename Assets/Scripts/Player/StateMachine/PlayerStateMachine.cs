using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 玩家状态机 - 管理所有玩家状态的切换
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PhysicsCheck))]
public class PlayerStateMachine : MonoBehaviour
{
    #region 组件引用
    public Animator Animator { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public PhysicsCheck PhysicsCheck { get; private set; }
    private PlayerInputControl _inputControl;
    #endregion

    #region 状态数据
    [SerializeField] private PlayerStateData _stateData = new PlayerStateData();
    public PlayerStateData StateData => _stateData;
    #endregion

    #region 状态管理
    private Dictionary<Type, IPlayerState> _states = new Dictionary<Type, IPlayerState>();
    private IPlayerState _currentState;
    public IPlayerState CurrentState => _currentState;
    public Type CurrentStateType => _currentState?.GetType();
    #endregion

    #region Animator 参数哈希值
    public readonly int HashIsGround = Animator.StringToHash("IsGround");
    public readonly int HashYVelocity = Animator.StringToHash("yVelocity");
    public readonly int HashXVelocity = Animator.StringToHash("xVelocity");
    public readonly int HashJump = Animator.StringToHash("Jump");
    public readonly int HashAttack = Animator.StringToHash("Attack");
    public readonly int HashHurt = Animator.StringToHash("Hurt");
    public readonly int HashSpell = Animator.StringToHash("Spell");
    public readonly int HashSlide = Animator.StringToHash("Slide");
    public readonly int HashIsSliding = Animator.StringToHash("IsSliding");
    public readonly int HashIsClimbing = Animator.StringToHash("IsClimbing");
    public readonly int HashIsDead = Animator.StringToHash("IsDead");
    #endregion

    #region Unity 生命周期

    void Awake()
    {
        // 获取组件引用
        Animator = GetComponent<Animator>();
        Rigidbody = GetComponent<Rigidbody2D>();
        PhysicsCheck = GetComponent<PhysicsCheck>();
        _inputControl = new PlayerInputControl();

        // 注册所有状态
        RegisterStates();

        // 绑定输入事件
        BindInputEvents();
    }

    void OnEnable()
    {
        _inputControl?.Enable();
        SubscribeToEvents();
    }

    void OnDisable()
    {
        _inputControl?.Disable();
        UnsubscribeFromEvents();
    }

    void Start()
    {
        // 设置初始状态为待机
        ChangeState<IdleState>();
    }

    void Update()
    {
        // 读取输入
        _stateData.InputDirection = _inputControl.Gameplay.Move.ReadValue<Vector2>();

        // 更新动画参数
        UpdateAnimatorParameters();

        // 处理角色翻转
        CheckFlip();

        // 更新当前状态
        _currentState?.Update();
    }

    void FixedUpdate()
    {
        _currentState?.FixedUpdate();
    }

    #endregion

    #region 状态注册

    /// <summary>
    /// 注册所有玩家状态
    /// </summary>
    private void RegisterStates()
    {
        _states.Add(typeof(IdleState), new IdleState(this));
        _states.Add(typeof(MoveState), new MoveState(this));
        _states.Add(typeof(JumpState), new JumpState(this));
        _states.Add(typeof(AttackState), new AttackState(this));
        _states.Add(typeof(HurtState), new HurtState(this));
        _states.Add(typeof(DeathState), new DeathState(this));
        _states.Add(typeof(ClimbState), new ClimbState(this));
        _states.Add(typeof(SlideState), new SlideState(this));
        _states.Add(typeof(SpellState), new SpellState(this));
    }

    #endregion

    #region 状态切换

    /// <summary>
    /// 切换到指定状态
    /// </summary>
    public void ChangeState<T>() where T : IPlayerState
    {
        var newState = GetState<T>();
        if (newState == null)
        {
            Debug.LogError($"状态 {typeof(T).Name} 未注册！");
            return;
        }

        // 检查是否可以从当前状态转换
        if (_currentState != null && !_currentState.CanTransitionTo(newState))
        {
            return;
        }

        // 获取旧状态类型用于事件
        Type oldStateType = _currentState?.GetType();

        // 退出旧状态
        _currentState?.Exit();

        // 切换到新状态
        _currentState = newState;

        // 进入新状态
        _currentState.Enter();

        // 触发状态改变事件
        PlayerEvents.Instance.TriggerStateChanged(oldStateType, typeof(T));

        Debug.Log($"状态切换: {oldStateType?.Name ?? "None"} -> {typeof(T).Name}");
    }

    /// <summary>
    /// 获取指定类型的状态
    /// </summary>
    public T GetState<T>() where T : IPlayerState
    {
        if (_states.TryGetValue(typeof(T), out IPlayerState state))
        {
            return (T)state;
        }
        return default;
    }

    /// <summary>
    /// 检查当前是否为指定状态
    /// </summary>
    public bool IsInState<T>() where T : IPlayerState
    {
        return _currentState is T;
    }

    #endregion

    #region 输入处理

    private void BindInputEvents()
    {
        _inputControl.Gameplay.Jump.started += OnJumpInput;
        _inputControl.Gameplay.Attack.started += OnAttackInput;
        _inputControl.Gameplay.Spell.started += OnSpellInput;
        _inputControl.Gameplay.Slide.started += OnSlideInput;
    }

    private void OnJumpInput(InputAction.CallbackContext context)
    {
        PlayerEvents.Instance.TriggerJumpRequested();
    }

    private void OnAttackInput(InputAction.CallbackContext context)
    {
        PlayerEvents.Instance.TriggerAttackRequested();
    }

    private void OnSpellInput(InputAction.CallbackContext context)
    {
        PlayerEvents.Instance.TriggerSpellRequested();
    }

    private void OnSlideInput(InputAction.CallbackContext context)
    {
        PlayerEvents.Instance.TriggerSlideRequested();
    }

    #endregion

    #region 事件订阅

    private void SubscribeToEvents()
    {
        PlayerEvents.Instance.OnJumpRequested += HandleJumpRequest;
        PlayerEvents.Instance.OnAttackRequested += HandleAttackRequest;
        PlayerEvents.Instance.OnSpellRequested += HandleSpellRequest;
        PlayerEvents.Instance.OnSlideRequested += HandleSlideRequest;
        PlayerEvents.Instance.OnPlayerHurt += HandleHurt;
        PlayerEvents.Instance.OnPlayerDeath += HandleDeath;
    }

    private void UnsubscribeFromEvents()
    {
        if (PlayerEvents.Instance != null)
        {
            PlayerEvents.Instance.OnJumpRequested -= HandleJumpRequest;
            PlayerEvents.Instance.OnAttackRequested -= HandleAttackRequest;
            PlayerEvents.Instance.OnSpellRequested -= HandleSpellRequest;
            PlayerEvents.Instance.OnSlideRequested -= HandleSlideRequest;
            PlayerEvents.Instance.OnPlayerHurt -= HandleHurt;
            PlayerEvents.Instance.OnPlayerDeath -= HandleDeath;
        }
    }

    private void HandleJumpRequest()
    {
        if (!_stateData.IsDead && PhysicsCheck.IsGrounded && !_stateData.IsHurting)
        {
            ChangeState<JumpState>();
        }
    }

    private void HandleAttackRequest()
    {
        if (!_stateData.IsDead && PhysicsCheck.IsGrounded && !_stateData.IsHurting)
        {
            // 如果已经在攻击状态，请求连击
            if (_stateData.IsAttacking)
            {
                var attackState = GetState<AttackState>();
                attackState?.RequestCombo();
            }
            else
            {
                ChangeState<AttackState>();
            }
        }
    }

    private void HandleSpellRequest()
    {
        if (!_stateData.IsDead && PhysicsCheck.IsGrounded && !_stateData.IsHurting)
        {
            ChangeState<SpellState>();
        }
    }

    private void HandleSlideRequest()
    {
        if (!_stateData.IsDead && PhysicsCheck.IsGrounded && !_stateData.IsHurting && !_stateData.IsSliding)
        {
            ChangeState<SlideState>();
        }
    }

    private void HandleHurt(Transform attacker)
    {
        if (!_stateData.IsDead && !_stateData.IsHurting)
        {
            // 计算击退方向
            Vector2 dir = new Vector2(transform.position.x - attacker.position.x, 0).normalized;
            Rigidbody.velocity = Vector2.zero;
            Rigidbody.AddForce(dir * _stateData.HurtForce, ForceMode2D.Impulse);

            ChangeState<HurtState>();
        }
    }

    private void HandleDeath()
    {
        if (!_stateData.IsDead)
        {
            ChangeState<DeathState>();
        }
    }

    #endregion

    #region 动画更新

    private void UpdateAnimatorParameters()
    {
        if (Animator == null) return;

        Animator.SetBool(HashIsGround, PhysicsCheck.IsGrounded);
        Animator.SetFloat(HashXVelocity, Mathf.Abs(Rigidbody.velocity.x));
        Animator.SetFloat(HashYVelocity, Rigidbody.velocity.y);
        Animator.SetBool(HashIsSliding, _stateData.IsSliding);
        Animator.SetBool(HashIsClimbing, _stateData.IsClimbing);
        Animator.SetBool(HashIsDead, _stateData.IsDead);
    }

    #endregion

    #region 角色翻转

    private void CheckFlip()
    {
        if (_stateData.IsDead || _stateData.IsHurting) return;

        float xInput = _stateData.InputDirection.x;
        if (Mathf.Abs(xInput) > _stateData.InputThreshold)
        {
            if (xInput > 0 && !_stateData.IsFacingRight)
            {
                Flip();
            }
            else if (xInput < 0 && _stateData.IsFacingRight)
            {
                Flip();
            }
        }
    }

    private void Flip()
    {
        _stateData.IsFacingRight = !_stateData.IsFacingRight;
        float yRotation = _stateData.IsFacingRight ? 0f : 180f;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 禁用玩家输入
    /// </summary>
    public void DisableInput()
    {
        _inputControl?.Gameplay.Disable();
    }

    /// <summary>
    /// 启用玩家输入
    /// </summary>
    public void EnableInput()
    {
        _inputControl?.Gameplay.Enable();
    }

    /// <summary>
    /// 获取面朝方向
    /// </summary>
    public float GetFacingDirection()
    {
        return _stateData.IsFacingRight ? 1f : -1f;
    }

    #endregion
}
