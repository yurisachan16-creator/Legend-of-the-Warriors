using System;
using UnityEngine;

/// <summary>
/// 玩家事件系统 - 使用委托解耦模块通信
/// </summary>
public class PlayerEvents : MonoBehaviour
{
    // 单例实例
    private static PlayerEvents _instance;
    private static readonly object _lock = new object();
    private static bool _isApplicationQuitting = false;

    public static PlayerEvents Instance
    {
        get
        {
            if (_isApplicationQuitting)
            {
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // 先尝试在场景中找到已存在的实例
                    _instance = FindAnyObjectByType<PlayerEvents>();
                    
                    if (_instance == null)
                    {
                        var go = new GameObject("PlayerEvents");
                        _instance = go.AddComponent<PlayerEvents>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
    }

    #region 状态事件
    /// <summary>
    /// 状态改变事件 - 参数：旧状态类型，新状态类型
    /// </summary>
    public event Action<Type, Type> OnStateChanged;
    public void TriggerStateChanged(Type oldState, Type newState)
    {
        OnStateChanged?.Invoke(oldState, newState);
    }
    #endregion

    #region 移动事件
    /// <summary>
    /// 移动输入事件
    /// </summary>
    public event Action<Vector2> OnMoveInput;
    public void TriggerMoveInput(Vector2 input)
    {
        OnMoveInput?.Invoke(input);
    }
    #endregion

    #region 战斗事件
    /// <summary>
    /// 攻击请求事件
    /// </summary>
    public event Action OnAttackRequested;
    public void TriggerAttackRequested()
    {
        OnAttackRequested?.Invoke();
    }

    /// <summary>
    /// 攻击命中事件 - 参数：伤害值
    /// </summary>
    public event Action<int> OnAttackHit;
    public void TriggerAttackHit(int damage)
    {
        OnAttackHit?.Invoke(damage);
    }

    /// <summary>
    /// 受伤事件 - 参数：攻击者Transform
    /// </summary>
    public event Action<Transform> OnPlayerHurt;
    public void TriggerPlayerHurt(Transform attacker)
    {
        OnPlayerHurt?.Invoke(attacker);
    }

    /// <summary>
    /// 死亡事件
    /// </summary>
    public event Action OnPlayerDeath;
    public void TriggerPlayerDeath()
    {
        OnPlayerDeath?.Invoke();
    }
    #endregion

    #region 动作事件
    /// <summary>
    /// 跳跃请求事件
    /// </summary>
    public event Action OnJumpRequested;
    public void TriggerJumpRequested()
    {
        OnJumpRequested?.Invoke();
    }

    /// <summary>
    /// 滑铲请求事件
    /// </summary>
    public event Action OnSlideRequested;
    public void TriggerSlideRequested()
    {
        OnSlideRequested?.Invoke();
    }

    /// <summary>
    /// 技能请求事件
    /// </summary>
    public event Action OnSpellRequested;
    public void TriggerSpellRequested()
    {
        OnSpellRequested?.Invoke();
    }

    /// <summary>
    /// 攀爬开始事件
    /// </summary>
    public event Action OnClimbStarted;
    public void TriggerClimbStarted()
    {
        OnClimbStarted?.Invoke();
    }

    /// <summary>
    /// 攀爬结束事件
    /// </summary>
    public event Action OnClimbEnded;
    public void TriggerClimbEnded()
    {
        OnClimbEnded?.Invoke();
    }

    /// <summary>
    /// 落地事件
    /// </summary>
    public event Action OnLanded;
    public void TriggerLanded()
    {
        OnLanded?.Invoke();
    }
    #endregion

    #region 动画事件
    /// <summary>
    /// 动画完成事件 - 参数：动画名称
    /// </summary>
    public event Action<string> OnAnimationComplete;
    public void TriggerAnimationComplete(string animationName)
    {
        OnAnimationComplete?.Invoke(animationName);
    }
    #endregion

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
