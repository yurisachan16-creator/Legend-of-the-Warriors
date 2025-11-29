using UnityEngine;

/// <summary>
/// 受伤状态 - 玩家受到伤害时的状态
/// </summary>
public class HurtState : PlayerStateBase
{
    public HurtState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();

        StateData.IsHurting = true;
        StateData.StateTimer = 0f;

        // 触发受伤动画
        SetTrigger(StateMachine.HashHurt);

        Debug.Log("玩家受伤！");
    }

    public override void Update()
    {
        base.Update();

        StateData.StateTimer += Time.deltaTime;

        // 受伤状态持续时间结束
        if (StateData.StateTimer >= StateData.HurtDuration)
        {
            // 检查是否死亡
            if (StateData.IsDead)
            {
                StateMachine.ChangeState<DeathState>();
            }
            // 根据是否在地面决定下一个状态
            else if (IsGrounded())
            {
                if (HasMoveInput())
                {
                    StateMachine.ChangeState<MoveState>();
                }
                else
                {
                    StateMachine.ChangeState<IdleState>();
                }
            }
            else
            {
                StateMachine.ChangeState<JumpState>();
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // 受伤时不控制移动，让击退力自然衰减
    }

    public override void Exit()
    {
        base.Exit();
        StateData.IsHurting = false;
    }

    public override bool CanTransitionTo(IPlayerState newState)
    {
        // 受伤状态可以被死亡状态打断
        if (newState is DeathState)
        {
            return true;
        }

        // 受伤动画播放完成后才能转换到其他状态
        return StateData.StateTimer >= StateData.HurtDuration;
    }
}
