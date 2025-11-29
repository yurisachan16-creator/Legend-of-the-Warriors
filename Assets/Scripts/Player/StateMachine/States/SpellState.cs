using UnityEngine;

/// <summary>
/// 技能状态 - 玩家释放技能时的状态
/// </summary>
public class SpellState : PlayerStateBase
{
    private float _spellDuration = 0.8f; // 技能动画持续时间

    public SpellState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        base.Enter();

        StateData.StateTimer = 0f;

        // 触发技能动画
        SetTrigger(StateMachine.HashSpell);

        // 施法时停止移动
        Rigidbody.velocity = new Vector2(0, Rigidbody.velocity.y);

        Debug.Log("释放技能！");
    }

    public override void Update()
    {
        base.Update();

        StateData.StateTimer += Time.deltaTime;

        // 技能动画结束
        if (StateData.StateTimer >= _spellDuration)
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

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // 施法时保持静止
        Rigidbody.velocity = new Vector2(0, Rigidbody.velocity.y);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override bool CanTransitionTo(IPlayerState newState)
    {
        // 技能状态可以被受伤和死亡打断
        if (newState is HurtState || newState is DeathState)
        {
            return true;
        }

        // 技能动画播放完成后才能转换到其他状态
        return StateData.StateTimer >= _spellDuration;
    }
}
