using UnityEngine;

/// <summary>
/// 攻击状态 - 玩家攻击时的状态（支持三连击）
/// </summary>
public class AttackState : PlayerStateBase
{
    private float _attackDuration = 0.4f; // 攻击动画持续时间
    private bool _canCombo = false;
    private bool _comboRequested = false;

    public AttackState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();

        StateData.IsAttacking = true;
        StateData.StateTimer = 0f;
        _canCombo = false;
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

        // 在攻击动画中段允许接收下一次攻击输入
        float comboWindowStart = _attackDuration * StateData.AttackComboWindowStart;
        float comboWindowEnd = _attackDuration * StateData.AttackComboWindowEnd;
        if (StateData.StateTimer > comboWindowStart && StateData.StateTimer < comboWindowEnd)
        {
            _canCombo = true;
        }

        // 攻击动画结束
        if (StateData.StateTimer >= _attackDuration)
        {
            // 检查是否请求了连击
            if (_comboRequested && StateData.ComboCount < StateData.MaxCombo)
            {
                // 继续攻击（重新进入攻击状态）
                StateData.StateTimer = 0f;
                _canCombo = false;
                _comboRequested = false;
                ExecuteAttack();
            }
            else
            {
                // 攻击结束，返回待机或移动
                if (HasMoveInput())
                {
                    StateMachine.ChangeState<MoveState>();
                }
                else
                {
                    StateMachine.ChangeState<IdleState>();
                }
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
        _canCombo = false;
        _comboRequested = false;
    }

    public override bool CanTransitionTo(IPlayerState newState)
    {
        // 攻击状态可以被受伤和死亡打断
        if (newState is HurtState || newState is DeathState)
        {
            return true;
        }

        // 攻击状态不能直接被其他状态打断
        return false;
    }

    /// <summary>
    /// 执行攻击（增加连击计数并触发动画）
    /// </summary>
    private void ExecuteAttack()
    {
        StateData.IncrementCombo();
        SetTrigger(StateMachine.HashAttack);
        Debug.Log($"攻击！连击数: {StateData.ComboCount}");
    }

    /// <summary>
    /// 请求连击（由攻击输入触发）
    /// </summary>
    public void RequestCombo()
    {
        if (_canCombo && StateData.ComboCount < StateData.MaxCombo)
        {
            _comboRequested = true;
        }
    }
}
