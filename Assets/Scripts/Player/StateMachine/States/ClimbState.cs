using UnityEngine;

/// <summary>
/// 攀爬状态 - 玩家攀爬梯子时的状态
/// </summary>
public class ClimbState : PlayerStateBase
{
    public ClimbState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();

        StateData.IsClimbing = true;

        // 设置攀爬动画
        SetBool(StateMachine.HashIsClimbing, true);

        // 禁用重力
        Rigidbody.gravityScale = 0f;

        // 停止当前速度
        Rigidbody.velocity = Vector2.zero;

        Debug.Log("开始攀爬！");
    }

    public override void Update()
    {
        base.Update();

        // 检查是否松开攀爬（例如离开梯子区域）
        // 这个逻辑应该由触发器检测处理
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 垂直攀爬移动
        float climbInput = GetVerticalInput();
        Rigidbody.velocity = new Vector2(0, climbInput * StateData.ClimbSpeed);
    }

    public override void Exit()
    {
        base.Exit();

        StateData.IsClimbing = false;

        // 恢复重力
        Rigidbody.gravityScale = 1f;

        // 恢复攀爬动画
        SetBool(StateMachine.HashIsClimbing, false);

        // 触发攀爬结束事件
        PlayerEvents.Instance.TriggerClimbEnded();

        Debug.Log("结束攀爬！");
    }

    public override bool CanTransitionTo(IPlayerState newState)
    {
        // 攀爬状态可以被受伤、死亡打断，也可以主动退出
        if (newState is HurtState || newState is DeathState)
        {
            return true;
        }

        // 可以从攀爬转换到跳跃（从梯子上跳下）
        if (newState is JumpState)
        {
            return true;
        }

        // 可以转换到待机或移动（离开梯子）
        if (newState is IdleState || newState is MoveState)
        {
            return true;
        }

        return base.CanTransitionTo(newState);
    }

    /// <summary>
    /// 停止攀爬并跳下
    /// </summary>
    public void JumpOffLadder()
    {
        StateMachine.ChangeState<JumpState>();
    }

    /// <summary>
    /// 停止攀爬并恢复正常状态
    /// </summary>
    public void StopClimbing()
    {
        if (IsGrounded())
        {
            StateMachine.ChangeState<IdleState>();
        }
        else
        {
            StateMachine.ChangeState<JumpState>();
        }
    }
}
