using UnityEngine;

/// <summary>
/// 移动状态 - 玩家行走/奔跑时的状态
/// </summary>
public class MoveState : PlayerStateBase
{
    public MoveState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        // 检查是否停止移动输入，切换到待机状态
        if (!HasMoveInput() && IsGrounded())
        {
            StateMachine.ChangeState<IdleState>();
            return;
        }

        // 检查是否在空中
        if (!IsGrounded())
        {
            StateMachine.ChangeState<JumpState>();
            return;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        // 应用水平移动
        float moveSpeed = StateData.MoveSpeed;
        Rigidbody.velocity = new Vector2(GetHorizontalInput() * moveSpeed, Rigidbody.velocity.y);
    }
}
