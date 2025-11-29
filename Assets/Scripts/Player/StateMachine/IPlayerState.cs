using UnityEngine;

/// <summary>
/// 玩家状态接口 - 定义所有状态的基本行为
/// </summary>
public interface IPlayerState
{
    /// <summary>
    /// 进入状态时调用
    /// </summary>
    void Enter();

    /// <summary>
    /// 每帧更新（Update）
    /// </summary>
    void Update();

    /// <summary>
    /// 物理更新（FixedUpdate）
    /// </summary>
    void FixedUpdate();

    /// <summary>
    /// 退出状态时调用
    /// </summary>
    void Exit();

    /// <summary>
    /// 检查是否可以切换到其他状态
    /// </summary>
    bool CanTransitionTo(IPlayerState newState);
}
