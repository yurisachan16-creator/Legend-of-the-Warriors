using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 简单版本的玩家控制脚本
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerInputControl _inputControl;
    [SerializeField] private Vector2 _inputDirection;
    private Rigidbody2D _rb;
    private PhysicsCheck _physicsCheck;
    private PlayerAnimation _playerAnimation;
    [Header("移动参数")]
    [SerializeField] private float _moveSpeed = 5f;
    [Header("跳跃参数")]
    [SerializeField] private float _jumpForce = 15f;
    [Header("冲刺参数")]
    [SerializeField] private float _dashSpeed = 20f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _dashCooldown = 1f;
    private const float _inputThreshold = 0.01f;    // 输入阈值，防止微小输入导致角色移动
    [SerializeField] private bool _isFacingRight = true;
    [SerializeField] public bool IsHurting = false;
    public float HurtFoce = 10f;
    public bool IsDead = false;

    void Awake()
    {
        _inputControl = new PlayerInputControl();
        _rb = GetComponent<Rigidbody2D>();
        _physicsCheck = GetComponent<PhysicsCheck>();
        _playerAnimation = GetComponent<PlayerAnimation>();

        _inputControl.Gameplay.Jump.started += Jump;
    }

    

    void OnEnable()
    {
        if(_inputControl != null)
        _inputControl.Enable();
    }

    void OnDisable()
    {
        if(_inputControl != null)
        _inputControl.Disable();
    }

    void Update()
    {
        _inputDirection = _inputControl.Gameplay.Move.ReadValue<Vector2>();
        CheckFlip(_inputDirection.x);
    }

    void FixedUpdate()
    {
        if(IsHurting) return;
        MovePlayer();
    }

    

    #region 角色动作

    private void MovePlayer()
    {
        _rb.velocity = new Vector2(_inputDirection.x * _moveSpeed, _rb.velocity.y);

    }

    /// <summary>
    /// 检查角色翻转
    /// </summary>
    private void CheckFlip(float xInput)
    {
        if(Mathf.Abs(xInput) > _inputThreshold)
        {
            if(xInput > 0 && !_isFacingRight)
            {
                Flip();
            }
            else if(xInput < 0 && _isFacingRight)
            {
                Flip();
            }
        }
    }

/// <summary>
/// 翻转角色的朝向
/// </summary>
    private void Flip()
    {
        _isFacingRight = !_isFacingRight;
        float yRotation = _isFacingRight ? 0f : 180f;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    private void Jump(InputAction.CallbackContext context)
    {
        //Debug.Log("Jumped!");
        if(!_physicsCheck.IsGrounded) return;
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        
        // 触发跳跃动画
        if(_playerAnimation != null)
        {
            _playerAnimation.TriggerJump();
        }
    }

    public void GetHurt(Transform attacker)
    {
        if(IsHurting) return;
        IsHurting = true;
        _rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(transform.position.x - attacker.position.x,0).normalized;

        _rb.AddForce(dir * HurtFoce, ForceMode2D.Impulse);

    }

    public void PlayerDead()
    {
        IsDead = true;
        _inputControl.Gameplay.Disable();
        Debug.Log("玩家已死亡，输入禁用。");
    }

    

    #endregion



}
