using UnityEngine;

/// <summary>
/// 死亡状态 - 玩家死亡时的状态
/// </summary>
public class DeathState : PlayerStateBase
{
    private bool _allowRespawn = false;

    public DeathState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();

        StateData.IsDead = true;
        _allowRespawn = false;

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
        _allowRespawn = false;
        SetBool(StateMachine.HashIsDead, false);
        StateMachine.EnableInput();
    }

    public override bool CanTransitionTo(IPlayerState newState)
    {
        // 死亡状态只有在允许重生时才能转换到待机状态
        if (_allowRespawn && newState is IdleState)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 允许重生（由重生系统调用）
    /// </summary>
    public void AllowRespawn()
    {
        _allowRespawn = true;
    }

    /// <summary>
    /// 执行重生
    /// </summary>
    public void Respawn()
    {
        _allowRespawn = true;
        StateMachine.ChangeState<IdleState>();
    }
}
