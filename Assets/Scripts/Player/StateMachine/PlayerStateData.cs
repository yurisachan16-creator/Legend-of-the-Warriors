using UnityEngine;

/// <summary>
/// 玩家状态数据 - 存储状态机共享的数据
/// </summary>
[System.Serializable]
public class PlayerStateData
{
    [Header("输入")]
    public Vector2 InputDirection;
    public float InputThreshold = 0.01f;

    [Header("移动参数")]
    public float MoveSpeed = 5f;

    [Header("跳跃参数")]
    public float JumpForce = 15f;

    [Header("滑铲参数")]
    public float SlideSpeed = 15f;
    public float SlideDuration = 0.5f;
    public float SlideEndSpeedMultiplier = 0.5f;

    [Header("攀爬参数")]
    public float ClimbSpeed = 3f;

    [Header("空中控制参数")]
    public float AirControl = 0.8f;

    [Header("受伤参数")]
    public float HurtForce = 10f;
    public float HurtDuration = 0.3f;

    [Header("攻击参数")]
    public int ComboCount = 0;
    public int MaxCombo = 3;
    public float ComboResetTime = 1f;
    public float LastAttackTime;
    public float AttackComboWindowStart = 0.5f;
    public float AttackComboWindowEnd = 0.9f;

    [Header("状态标志")]
    public bool IsFacingRight = true;
    public bool IsHurting = false;
    public bool IsDead = false;
    public bool IsSliding = false;
    public bool IsClimbing = false;
    public bool IsAttacking = false;

    [Header("计时器")]
    public float StateTimer = 0f;

    /// <summary>
    /// 重置攻击连击
    /// </summary>
    public void ResetCombo()
    {
        ComboCount = 0;
    }

    /// <summary>
    /// 增加连击计数
    /// </summary>
    public void IncrementCombo()
    {
        ComboCount++;
        if (ComboCount > MaxCombo)
        {
            ComboCount = 1;
        }
        LastAttackTime = Time.time;
    }

    /// <summary>
    /// 检查连击是否超时
    /// </summary>
    public bool IsComboTimeout()
    {
        return Time.time - LastAttackTime > ComboResetTime;
    }
}
