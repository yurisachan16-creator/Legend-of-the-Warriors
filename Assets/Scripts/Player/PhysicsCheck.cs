using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PhysicsCheck : MonoBehaviour
{
    public Vector2 GroundCheckOffset;
    [SerializeField] private float _checkRadius = 0.1f;
    [SerializeField] private LayerMask _groundLayer;
    [Header("状态")]
    public bool IsGrounded;
    void Update()
    {
        Check();
    }

    void Check()
    {
        //检测角色是否在地面上
        IsGrounded = Physics2D.OverlapCircle((Vector2)transform.position + GroundCheckOffset, _checkRadius, _groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + GroundCheckOffset, _checkRadius);
    }
}
