using UnityEngine;

/// <summary>
/// 滑铲状态 - 玩家滑铲时的状态
/// </summary>
public class SlideState : PlayerStateBase
{
    private float _slideDirection;

    public SlideState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();

        StateData.IsSliding = true;
        StateData.StateTimer = 0f;

        // 获取滑铲方向（使用当前朝向）
        _slideDirection = StateMachine.GetFacingDirection();

        // 触发滑铲动画
        SetTrigger(StateMachine.HashSlide);
        SetBool(StateMachine.HashIsSliding, true);

        // 施加滑铲速度
        Rigidbody.velocity = new Vector2(_slideDirection * StateData.SlideSpeed, Rigidbody.velocity.y);

        Debug.Log("滑铲！");
    }

    public override void Update()
    {
        base.Update();

        StateData.StateTimer += Time.deltaTime;

        // 滑铲持续时间结束
        if (StateData.StateTimer >= StateData.SlideDuration)
        {
            EndSlide();
            return;
        }

        // 如果离开地面，取消滑铲
        if (!IsGrounded())
        {
            StateMachine.ChangeState<JumpState>();
            return;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 滑铲时保持滑铲速度（随时间衰减）
        float progress = StateData.StateTimer / StateData.SlideDuration;
        float currentSpeed = Mathf.Lerp(StateData.SlideSpeed, StateData.MoveSpeed * 0.5f, progress);
        Rigidbody.velocity = new Vector2(_slideDirection * currentSpeed, Rigidbody.velocity.y);
    }

    public override void Exit()
    {
        base.Exit();

        StateData.IsSliding = false;
        SetBool(StateMachine.HashIsSliding, false);
    }

    public override bool CanTransitionTo(IPlayerState newState)
    {
        // 滑铲状态可以被受伤和死亡打断
        if (newState is HurtState || newState is DeathState)
        {
            return true;
        }

        // 滑铲结束后可以转换到其他状态
        if (StateData.StateTimer >= StateData.SlideDuration)
        {
            return true;
        }

        // 离开地面可以转换到跳跃
        if (newState is JumpState && !IsGrounded())
        {
            return true;
        }

        return false;
    }

    private void EndSlide()
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
}
