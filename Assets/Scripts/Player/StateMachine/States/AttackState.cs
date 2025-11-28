using UnityEngine;

/// <summary>
/// 攻击状态 - 玩家攻击时的状态（支持三连击）
/// 使用 IsAttack (Bool) 和 Attack (Trigger) 控制
/// </summary>
public class AttackState : PlayerStateBase
{
    private float _attackDuration = 0.9f; // 攻击动画持续时间（0.9秒窗口期）
    private bool _comboRequested = false;

    public AttackState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();

        StateData.IsAttacking = true;
        StateData.StateTimer = 0f;
        _comboRequested = false;

        // 检查连击超时，重置连击计数
        if (StateData.IsComboTimeout())
        {
            StateData.ResetCombo();
        }

        // 执行攻击
        ExecuteAttack();

        // 攻击时停止移动
        Rigidbody.velocity = new Vector2(0, Rigidbody.velocity.y);
    }

    public override void Update()
    {
        base.Update();

        StateData.StateTimer += Time.deltaTime;

        // 攻击动画结束（0.9秒窗口期）
        if (StateData.StateTimer >= _attackDuration)
        {
            // 检查是否请求了连击
            if (_comboRequested && StateData.ComboCount < StateData.MaxCombo)
            {
                // 继续攻击（重新进入下一段攻击）
                StateData.StateTimer = 0f;
                _comboRequested = false;
                ExecuteAttack();
            }
            else
            {
                // 攻击结束，返回待机或移动
                EndAttack();
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // 攻击时保持静止
        Rigidbody.velocity = new Vector2(0, Rigidbody.velocity.y);
    }

    public override void Exit()
    {
        base.Exit();
        StateData.IsAttacking = false;
        _comboRequested = false;
        
        // 重置Animator的IsAttack参数
        Animator.SetBool(StateMachine.HashIsAttack, false);
        
        // 重置连击计数
        StateData.ResetCombo();
    }

    public override bool CanTransitionTo(IPlayerState newState)
    {
        // 攻击状态可以被受伤和死亡打断
        if (newState is HurtState || newState is DeathState)
        {
            return true;
        }

        // 攻击动画结束后允许转换到其他状态
        if (StateData.StateTimer >= _attackDuration)
        {
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
        
        // 设置 IsAttack = true 并触发 Attack
        Animator.SetBool(StateMachine.HashIsAttack, true);
        SetTrigger(StateMachine.HashAttack);
        
        Debug.Log($"攻击！连击数: {StateData.ComboCount}");
    }

    /// <summary>
    /// 结束攻击状态
    /// </summary>
    private void EndAttack()
    {
        // 设置 IsAttack = false
        Animator.SetBool(StateMachine.HashIsAttack, false);
        
        // 重置连击
        StateData.ResetCombo();
        
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
        // 在连击数限制内可以请求连击
        if (StateData.ComboCount < StateData.MaxCombo)
        {
            _comboRequested = true;
            Debug.Log($"连击请求！当前连击数: {StateData.ComboCount}");
        }
    }
}
