using UnityEngine;

/// <summary>
/// 待机状态 - 玩家静止不动时的状态
/// </summary>
public class IdleState : PlayerStateBase
{
    public IdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        // 确保速度为零
        Rigidbody.velocity = new Vector2(0, Rigidbody.velocity.y);
    }

    public override void Update()
    {
        base.Update();

        // 检查是否有移动输入，切换到移动状态
        if (HasMoveInput() && IsGrounded())
        {
            StateMachine.ChangeState<MoveState>();
            return;
        }

        // 检查是否在空中（从平台掉落）
        if (!IsGrounded())
        {
            StateMachine.ChangeState<JumpState>();
            return;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // 确保水平速度为零
        if (IsGrounded())
        {
            Rigidbody.velocity = new Vector2(0, Rigidbody.velocity.y);
        }
    }
}
