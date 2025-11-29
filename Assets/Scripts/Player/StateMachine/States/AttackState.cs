using UnityEngine;

/// <summary>
/// 攻击状态 - 玩家攻击时的状态（支持三连击）
/// 使用 IsAttack (Bool) 和 Attack (Trigger) 控制
/// 通过 AnimatorStateInfo 和 ExitTime 实现流畅连击
/// </summary>
public class AttackState : PlayerStateBase
{
    // 连击窗口期配置
    private float _comboWindowStart = 0.5f;  // 可以接受连击输入的开始时间点（normalizedTime）
    private float _comboWindowEnd = 0.95f;   // 可以接受连击输入的结束时间点（normalizedTime）
    
    // 状态追踪
    private bool _comboRequested = false;
    private bool _hasTriggeredNextCombo = false;
    private int _currentComboIndex = 0;  // 当前攻击段数（0=Attack01, 1=Attack02, 2=Attack03）
    
    // 动画层索引
    private int _animLayerIndex = 0;

    public AttackState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();

        StateData.IsAttacking = true;
        StateData.StateTimer = 0f;
        _comboRequested = false;
        _hasTriggeredNextCombo = false;

        // 检查连击超时，重置连击计数
        if (StateData.IsComboTimeout())
        {
            StateData.ResetCombo();
            _currentComboIndex = 0;
        }

        // 执行攻击
        ExecuteAttack();

        // 攻击时减速但不完全停止
        Rigidbody.velocity = new Vector2(Rigidbody.velocity.x * 0.3f, Rigidbody.velocity.y);
    }

    public override void Update()
    {
        base.Update();

        StateData.StateTimer += Time.deltaTime;
        
        // 获取当前攻击动画状态信息
        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(_animLayerIndex);
        
        // 检查是否在攻击动画中（通过标签或状态名）
        bool isInAttackAnimation = stateInfo.IsTag("Attack");
        
        if (isInAttackAnimation)
        {
            float normalizedTime = stateInfo.normalizedTime % 1.0f; // 获取归一化时间（0-1）
            
            // 在连击窗口期内，检查是否有连击请求
            if (_comboRequested && !_hasTriggeredNextCombo)
            {
                // 如果在可接受窗口内且还有剩余连击次数
                if (normalizedTime >= _comboWindowStart && StateData.ComboCount < StateData.MaxCombo)
                {
                    _hasTriggeredNextCombo = true;
                    _comboRequested = false;
                    _currentComboIndex++;
                    ExecuteAttack();
                    Debug.Log($"触发下一段连击！normalizedTime: {normalizedTime:F2}");
                }
            }
            
            // 动画播放完成（超过95%且没有连击请求）
            if (normalizedTime >= _comboWindowEnd && !_comboRequested)
            {
                EndAttack();
            }
        }
        else
        {
            // 如果不在攻击动画中，可能是动画转换完成，结束攻击
            if (StateData.StateTimer > 0.1f) // 给一点容错时间
            {
                EndAttack();
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // 攻击时轻微减速（保持少量惯性）
        Rigidbody.velocity = new Vector2(Rigidbody.velocity.x * 0.85f, Rigidbody.velocity.y);
    }

    public override void Exit()
    {
        base.Exit();
        StateData.IsAttacking = false;
        _comboRequested = false;
        _hasTriggeredNextCombo = false;
        
        // 重置Animator的IsAttack参数
        Animator.SetBool(StateMachine.HashIsAttack, false);
        
        // 注意：不立即重置连击计数，保留给超时检测
        // StateData.ResetCombo(); // 移除这行
    }

    public override bool CanTransitionTo(IPlayerState newState)
    {
        // 攻击状态可以被受伤和死亡打断
        if (newState is HurtState || newState is DeathState)
        {
            return true;
        }

        // 获取当前动画状态
        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(_animLayerIndex);
        bool isInAttackAnimation = stateInfo.IsTag("Attack");
        
        if (isInAttackAnimation)
        {
            float normalizedTime = stateInfo.normalizedTime % 1.0f;
            // 动画播放到一定程度后允许转换
            if (normalizedTime >= _comboWindowEnd)
            {
                return true;
            }
        }
        else
        {
            // 不在攻击动画中，可以转换
            return true;
        }

        // 攻击动画未结束时，不能直接被其他状态打断
        return false;
    }

    /// <summary>
    /// 执行攻击（增加连击计数并触发动画）
    /// </summary>
    private void ExecuteAttack()
    {
        StateData.IncrementCombo();
        _hasTriggeredNextCombo = false; // 重置标志
        
        // 设置 IsAttack = true 并触发 Attack Trigger
        Animator.SetBool(StateMachine.HashIsAttack, true);
        Animator.SetTrigger(StateMachine.HashAttack);
        
        Debug.Log($"执行攻击！连击段数: {StateData.ComboCount}/{StateData.MaxCombo}");
    }

    /// <summary>
    /// 结束攻击状态
    /// </summary>
    private void EndAttack()
    {
        // 设置 IsAttack = false
        Animator.SetBool(StateMachine.HashIsAttack, false);
        
        // 返回待机或移动
        if (HasMoveInput())
        {
            StateMachine.ChangeState<MoveState>();
        }
        else
        {
            StateMachine.ChangeState<IdleState>();
        }
    }

    /// <summary>
    /// 请求连击（由攻击输入触发）
    /// </summary>
    public void RequestCombo()
    {
        // 如果已完成三连击
        if (StateData.ComboCount >= StateData.MaxCombo)
        {
            // 获取当前动画状态
            AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(_animLayerIndex);
            float normalizedTime = stateInfo.normalizedTime % 1.0f;
            
            // 如果第三击动画播放超过一半，允许立即重新开始连击循环
            if (normalizedTime >= 0.5f)
            {
                Debug.Log("三连击完成，立即重新开始新的连击循环！");
                
                // 重置状态机中的连击计数
                StateData.ResetCombo();
                _currentComboIndex = 0;
                _comboRequested = true; // 标记需要重新开始
                _hasTriggeredNextCombo = false;
                
                // 立即结束当前攻击状态，让状态机重新进入攻击状态
                EndAttack();
                StateMachine.ChangeState<AttackState>();
            }
            else
            {
                Debug.Log("第三击动画尚未播放到一半，等待...");
            }
        }
        // 正常连击（第一击到第二击，第二击到第三击）
        else if (!_hasTriggeredNextCombo)
        {
            _comboRequested = true;
            Debug.Log($"连击请求！当前连击: {StateData.ComboCount}/{StateData.MaxCombo}");
        }
    }
}
