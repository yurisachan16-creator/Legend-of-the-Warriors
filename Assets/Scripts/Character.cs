using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

public class Character : MonoBehaviour
{
    [Header("角色属性")]
    public float MaxHealth = 100;
    public float CurrentHealth;
    [Header("受伤无敌")]
    public float InvincibleDuration = 1f;
    private float _invincibleTimer;
    public bool IsInvincible;

    public UnityEvent<Transform> OnTakeDamage;

    // 缓存组件引用
    private Rigidbody2D _rigidbody;

    void Awake()
    {
        CurrentHealth = MaxHealth;
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        CheckInvincibilityTimer();
    }

    public void TakeDamage(Attack attack)
    {
        if(IsInvincible) return;

        if(CurrentHealth-attack.Damage > 0)
        {
            CurrentHealth -= attack.Damage;
            Debug.Log($"{gameObject.name} 受到 {attack.Damage} 点伤害，当前生命值：{CurrentHealth}/{MaxHealth}");
            TriggerInvulnerable();
            //执行受伤事件
            OnTakeDamage?.Invoke(attack.transform);
        }
        else
        {
            CurrentHealth = 0;
            Debug.Log($"{gameObject.name} 受到 {attack.Damage} 点伤害，当前生命值：{CurrentHealth}/{MaxHealth}");
            Die();
        }

        
        
    }

    /// <summary>
    /// 使用AttackData结构接收伤害（支持状态机系统）
    /// </summary>
    public void TakeDamage(AttackData attackData)
    {
        if(IsInvincible) return;

        if(CurrentHealth - attackData.Damage > 0)
        {
            CurrentHealth -= attackData.Damage;
            Debug.Log($"{gameObject.name} 受到 {attackData.Damage} 点伤害，当前生命值：{CurrentHealth}/{MaxHealth}");
            TriggerInvulnerable();
            
            // 应用击退效果
            ApplyKnockback(attackData.KnockbackDirection, attackData.KnockbackForce);
            
            // 执行受伤事件
            OnTakeDamage?.Invoke(attackData.Attacker);
        }
        else
        {
            CurrentHealth = 0;
            Debug.Log($"{gameObject.name} 受到 {attackData.Damage} 点伤害，当前生命值：{CurrentHealth}/{MaxHealth}");
            Die();
        }
    }

    /// <summary>
    /// 应用击退效果
    /// </summary>
    private void ApplyKnockback(Vector2 direction, float force)
    {
        if(_rigidbody != null)
        {
            _rigidbody.velocity = Vector2.zero;
            _rigidbody.AddForce(direction * force, ForceMode2D.Impulse);
        }
    }

    private void Die()
    {
        if(CurrentHealth <= 0)
        {
            Debug.Log($"{gameObject.name} 死亡");
            // 在这里添加死亡处理逻辑，例如播放动画、禁用角色等
        }
    }

    #region  计时器
    private void TriggerInvulnerable()
    {
        if(!IsInvincible)
        {
            IsInvincible = true;
            _invincibleTimer = InvincibleDuration;
            
            
        }
    }

    private void CheckInvincibilityTimer()
    {
        if(IsInvincible)
        {
            _invincibleTimer -= Time.deltaTime;
            if(_invincibleTimer <= 0)
            {
                IsInvincible = false;
                Debug.Log($"{gameObject.name} 结束无敌状态");
            }
        }
    }

    #endregion
}
