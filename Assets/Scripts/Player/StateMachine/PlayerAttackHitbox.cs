using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家攻击判定区域 - 管理攻击的伤害检测
/// 
/// 使用方法：
/// 1. 在玩家角色的子对象上创建空物体，添加此组件
/// 2. 添加BoxCollider2D或CircleCollider2D作为攻击范围
/// 3. 设置Collider为Trigger模式
/// 4. 在动画中通过Animation Event调用EnableHitbox/DisableHitbox
/// 5. 或者在AttackState中通过代码控制启用/禁用
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("攻击参数")]
    [SerializeField] private int _baseDamage = 10;
    [SerializeField] private float _knockbackForce = 5f;
    [SerializeField] private Vector2 _knockbackDirection = new Vector2(1f, 0.5f);

    [Header("连击伤害加成")]
    [SerializeField] private float[] _comboDamageMultipliers = { 1f, 1.2f, 1.5f };

    [Header("攻击层级")]
    [SerializeField] private LayerMask _targetLayers;

    [Header("调试")]
    [SerializeField] private bool _showDebugInfo = false;

    private Collider2D _collider;
    private PlayerStateMachine _stateMachine;
    private HashSet<Collider2D> _hitTargets = new HashSet<Collider2D>();
    private bool _isActive = false;

    #region Unity 生命周期

    void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _collider.isTrigger = true;
        _collider.enabled = false;

        // 获取父物体上的状态机
        _stateMachine = GetComponentInParent<PlayerStateMachine>();

        if (_stateMachine == null)
        {
            Debug.LogError("PlayerAttackHitbox: 找不到父物体上的PlayerStateMachine！");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isActive) return;

        // 检查是否为有效目标
        if (!IsValidTarget(other.gameObject)) return;

        // 防止同一次攻击多次命中同一目标
        if (_hitTargets.Contains(other)) return;

        // 记录已命中的目标
        _hitTargets.Add(other);

        // 尝试获取Character组件并造成伤害
        if (other.TryGetComponent<Character>(out Character targetCharacter))
        {
            // 计算实际伤害（考虑连击加成）
            int actualDamage = CalculateDamage();
            
            // 创建攻击数据
            AttackData attackData = new AttackData
            {
                Damage = actualDamage,
                Attacker = _stateMachine.transform,
                KnockbackForce = _knockbackForce,
                KnockbackDirection = GetKnockbackDirection()
            };

            // 造成伤害
            targetCharacter.TakeDamage(attackData);

            // 触发攻击命中事件
            PlayerEvents.Instance.TriggerAttackHit(actualDamage);

            if (_showDebugInfo)
            {
                Debug.Log($"攻击命中 {other.name}! 伤害: {actualDamage}, 连击数: {GetCurrentComboCount()}");
            }
        }
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 启用攻击判定（由动画事件或代码调用）
    /// </summary>
    public void EnableHitbox()
    {
        _isActive = true;
        _collider.enabled = true;
        _hitTargets.Clear();

        if (_showDebugInfo)
        {
            Debug.Log("攻击判定已启用");
        }
    }

    /// <summary>
    /// 禁用攻击判定（由动画事件或代码调用）
    /// </summary>
    public void DisableHitbox()
    {
        _isActive = false;
        _collider.enabled = false;
        _hitTargets.Clear();

        if (_showDebugInfo)
        {
            Debug.Log("攻击判定已禁用");
        }
    }

    /// <summary>
    /// 设置基础伤害
    /// </summary>
    public void SetBaseDamage(int damage)
    {
        _baseDamage = damage;
    }

    /// <summary>
    /// 设置击退力度
    /// </summary>
    public void SetKnockbackForce(float force)
    {
        _knockbackForce = force;
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 检查目标是否有效（在目标层级中）
    /// </summary>
    private bool IsValidTarget(GameObject target)
    {
        return (_targetLayers.value & (1 << target.layer)) != 0;
    }

    /// <summary>
    /// 计算实际伤害（考虑连击加成）
    /// </summary>
    private int CalculateDamage()
    {
        int comboCount = GetCurrentComboCount();
        float multiplier = 1f;

        if (comboCount > 0 && comboCount <= _comboDamageMultipliers.Length)
        {
            multiplier = _comboDamageMultipliers[comboCount - 1];
        }

        return Mathf.RoundToInt(_baseDamage * multiplier);
    }

    /// <summary>
    /// 获取当前连击数
    /// </summary>
    private int GetCurrentComboCount()
    {
        if (_stateMachine != null)
        {
            return _stateMachine.StateData.ComboCount;
        }
        return 1;
    }

    /// <summary>
    /// 获取击退方向（考虑角色朝向）
    /// </summary>
    private Vector2 GetKnockbackDirection()
    {
        float facingDirection = _stateMachine != null ? _stateMachine.GetFacingDirection() : 1f;
        return new Vector2(_knockbackDirection.x * facingDirection, _knockbackDirection.y).normalized;
    }

    #endregion

    #region 调试绘制

    void OnDrawGizmosSelected()
    {
        if (_collider == null) _collider = GetComponent<Collider2D>();
        if (_collider == null) return;

        Gizmos.color = _isActive ? Color.red : Color.yellow;

        if (_collider is BoxCollider2D box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.offset, box.size);
        }
        else if (_collider is CircleCollider2D circle)
        {
            Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
        }
    }

    #endregion
}

/// <summary>
/// 攻击数据结构 - 用于传递攻击信息
/// </summary>
[System.Serializable]
public struct AttackData
{
    public int Damage;
    public Transform Attacker;
    public float KnockbackForce;
    public Vector2 KnockbackDirection;
}
