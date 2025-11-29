using UnityEngine;

/// <summary>
/// 跳跃状态 - 玩家跳跃/空中时的状态
/// </summary>
public class JumpState : PlayerStateBase
{
    private bool _hasJumped = false;

    public JumpState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        
        // 如果是主动跳跃（从地面起跳）
        if (IsGrounded())
        {
            // 触发跳跃动画
            SetTrigger(StateMachine.HashJump);
            
            // 施加跳跃力
            Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, 0);
            Rigidbody.AddForce(Vector2.up * StateData.JumpForce, ForceMode2D.Impulse);
            _hasJumped = true;
        }
        else
        {
            // 从平台掉落，不施加跳跃力
            _hasJumped = false;
        }
    }

    public override void Update()
    {
        base.Update();

        // 落地检测
        if (IsGrounded() && Rigidbody.velocity.y <= 0)
        {
            // 触发落地事件
            PlayerEvents.Instance.TriggerLanded();

            // 根据是否有移动输入切换状态
            if (HasMoveInput())
            {
                StateMachine.ChangeState<MoveState>();
            }
            else
            {
                StateMachine.ChangeState<IdleState>();
            }
            return;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 空中移动控制（可以左右调整）
        float targetVelocityX = GetHorizontalInput() * StateData.MoveSpeed * StateData.AirControl;
        Rigidbody.velocity = new Vector2(targetVelocityX, Rigidbody.velocity.y);
    }

    public override void Exit()
    {
        base.Exit();
        _hasJumped = false;
    }

    public override bool CanTransitionTo(IPlayerState newState)
    {
        // 跳跃状态不能直接转换到移动或待机（除非落地）
        if (newState is MoveState || newState is IdleState)
        {
            return IsGrounded() && Rigidbody.velocity.y <= 0;
        }
        return base.CanTransitionTo(newState);
    }
}
