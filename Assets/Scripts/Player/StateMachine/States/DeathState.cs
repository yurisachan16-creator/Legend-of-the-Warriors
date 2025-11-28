using UnityEngine;

/// <summary>
/// 死亡状态 - 玩家死亡时的状态
/// </summary>
public class DeathState : PlayerStateBase
{
    public DeathState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();

        StateData.IsDead = true;

        // 设置死亡动画
        SetBool(StateMachine.HashIsDead, true);

        // 禁用输入
        StateMachine.DisableInput();

        // 停止移动
        Rigidbody.velocity = Vector2.zero;

        Debug.Log("玩家已死亡！");
    }

    public override void Update()
    {
        base.Update();
        // 死亡状态不做任何更新
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // 确保玩家不会移动
    }

    public override void Exit()
    {
        base.Exit();
        StateData.IsDead = false;
        SetBool(StateMachine.HashIsDead, false);
        StateMachine.EnableInput();
    }

    public override bool CanTransitionTo(IPlayerState newState)
    {
        // 死亡状态不能转换到任何其他状态（除非重生逻辑）
        return false;
    }
}
