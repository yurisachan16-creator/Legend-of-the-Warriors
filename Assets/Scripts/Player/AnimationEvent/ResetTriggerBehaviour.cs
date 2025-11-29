using UnityEngine;

public class ResetTriggerBehaviour : StateMachineBehaviour
{
    public string TriggerName = "Attack";

    // 当进入这个动画状态的第一帧，执行一次
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 【核心黑科技】
        // 一旦开始播放攻击动作，立刻把“Attack”触发器关掉。
        // 这样玩家必须再按一次键，重新 SetTrigger，才能满足下一段连击的条件。
        animator.ResetTrigger(TriggerName);
    }
}