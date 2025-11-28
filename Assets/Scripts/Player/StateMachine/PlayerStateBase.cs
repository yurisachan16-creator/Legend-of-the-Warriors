using UnityEngine;

/// <summary>
/// 玩家状态基类 - 提供所有状态的共享功能
/// </summary>
public abstract class PlayerStateBase : IPlayerState
{
    protected PlayerStateMachine StateMachine { get; private set; }
    protected PlayerStateData StateData => StateMachine.StateData;
    protected Animator Animator => StateMachine.Animator;
    protected Rigidbody2D Rigidbody => StateMachine.Rigidbody;
    protected PhysicsCheck PhysicsCheck => StateMachine.PhysicsCheck;
    protected Transform Transform => StateMachine.transform;

    public PlayerStateBase(PlayerStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public virtual void Enter()
    {
        // 子类可以重写此方法进行进入状态的初始化
    }

    public virtual void Update()
    {
        // 子类可以重写此方法进行每帧更新
    }

    public virtual void FixedUpdate()
    {
        // 子类可以重写此方法进行物理更新
    }

    public virtual void Exit()
    {
        // 子类可以重写此方法进行退出状态的清理
    }

    public virtual bool CanTransitionTo(IPlayerState newState)
    {
        // 默认允许所有状态转换，子类可以重写添加限制
        return true;
    }

    #region 辅助方法

    /// <summary>
    /// 设置动画触发器
    /// </summary>
    protected void SetTrigger(int hash)
    {
        if (Animator != null)
        {
            Animator.SetTrigger(hash);
        }
    }

    /// <summary>
    /// 设置动画布尔值
    /// </summary>
    protected void SetBool(int hash, bool value)
    {
        if (Animator != null)
        {
            Animator.SetBool(hash, value);
        }
    }

    /// <summary>
    /// 设置动画浮点值
    /// </summary>
    protected void SetFloat(int hash, float value)
    {
        if (Animator != null)
        {
            Animator.SetFloat(hash, value);
        }
    }

    /// <summary>
    /// 检查是否在地面上
    /// </summary>
    protected bool IsGrounded()
    {
        return PhysicsCheck != null && PhysicsCheck.IsGrounded;
    }

    /// <summary>
    /// 获取水平输入
    /// </summary>
    protected float GetHorizontalInput()
    {
        return StateData.InputDirection.x;
    }

    /// <summary>
    /// 获取垂直输入
    /// </summary>
    protected float GetVerticalInput()
    {
        return StateData.InputDirection.y;
    }

    /// <summary>
    /// 检查是否有移动输入
    /// </summary>
    protected bool HasMoveInput()
    {
        return Mathf.Abs(StateData.InputDirection.x) > StateData.InputThreshold;
    }

    #endregion
}
