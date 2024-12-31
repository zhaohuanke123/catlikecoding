using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;


public class MovingSphere : MonoBehaviour
{
    #region Unity 生命周期

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // 1. 获取输入并限制到单位长度
        var playerInput = default(Vector2);
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        //  2. 获取当前输入控制的速度
        desiredVelocity =
            new Vector3(playerInput.x, 0f, playerInput.y) * m_maxSpeed;

        desiredJump |= Input.GetButtonDown("Jump");
    }

    private void FixedUpdate()
    {
        // 1. 获取当前物理刚体速度
        UpdateState();
        float acceleration = onGround ? m_maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;
        // 2. 限制速度
        m_velocity.x =
            Mathf.MoveTowards(m_velocity.x, desiredVelocity.x, maxSpeedChange);
        m_velocity.z =
            Mathf.MoveTowards(m_velocity.z, desiredVelocity.z, maxSpeedChange);

        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        body.velocity = m_velocity;

        // 每个物理步骤都以调用所有 FixedUpdate 方法开始，之后 PhysX 执行其操作，最后调用碰撞方法。
        // 因此，当 FixedUpdate 被调用时，如果存在任何活动碰撞， onGround 会在上一步被设置为 true 。
        // 为了保持 onGround 有效，我们只需在 FixedUpdate 结束时将其设置回 false 即可。
        onGround = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            onGround |= normal.y >= 0.9f;
        }
    }

    #endregion

    #region 方法

    private void Jump()
    {
        if (onGround || jumpPhase < maxAirJumps)
        {
            jumpPhase += 1;

            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            if (m_velocity.y > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - m_velocity.y, 0f);
            }

            m_velocity.y += jumpSpeed;
        }
    }


    void UpdateState()
    {
        m_velocity = body.velocity;
        if (onGround)
        {
            jumpPhase = 0;
        }
    }

    #endregion

    #region 字段

    /// <summary>
    ///  最大速度
    /// </summary>
    [SerializeField]
    [Range(0f, 100f)]
    private float m_maxSpeed = 10f;

    /// <summary>
    /// 最大加速度
    /// </summary>
    [SerializeField]
    [Range(0f, 100f)]
    private float m_maxAcceleration = 10f;

    /// <summary>
    /// 最大加速度
    /// </summary>
    [SerializeField]
    [Range(0f, 100f)]
    private float maxAirAcceleration = 1f;

    /// <summary>
    ///  跳跃高度
    /// </summary>
    [SerializeField]
    [Range(0f, 10f)]
    private float jumpHeight = 2f;

    /// <summary>
    ///  最大空中跳跃次数
    /// </summary>
    [SerializeField]
    [Range(0, 5)]
    int maxAirJumps = 0;

    /// <summary>
    ///  当前空中跳跃次数
    /// </summary>
    int jumpPhase;

    /// <summary>
    ///  当前速度
    /// </summary>
    private Vector3 m_velocity;

    /// <summary>
    ///  输入的期望速度
    /// </summary>
    private Vector3 desiredVelocity;

    /// <summary>
    ///  是否跳跃
    /// </summary>
    private bool desiredJump;

    /// <summary>
    ///  是否在地面上
    /// </summary>
    private bool onGround;

    /// <summary>
    ///  物理刚体
    /// </summary>
    private Rigidbody body;

    #endregion
}