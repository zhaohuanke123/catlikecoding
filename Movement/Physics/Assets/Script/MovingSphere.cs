﻿using UnityEngine;


public class MovingSphere : MonoBehaviour
{
    #region Unity 生命周期

    private void Awake()
    {
        m_body = GetComponent<Rigidbody>();
        m_renderer = GetComponent<Renderer>();
        OnValidate();
    }

    private void Update()
    {
        // 1. 获取输入并限制到单位长度
        var playerInput = default(Vector2);
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        //  2. 获取当前输入控制的速度
        m_desiredVelocity =
            new Vector3(playerInput.x, 0f, playerInput.y) * m_maxSpeed;

        // 3. 获取跳跃输入
        m_desiredJump |= Input.GetButtonDown("Jump");

        if (m_propBlock == null)
        {
            m_propBlock = new MaterialPropertyBlock();
        }

        // 4. 根据接触点数量设置颜色
        m_propBlock.SetColor(ColorId, Color.white * (m_groundContactCount * 0.25f));
        m_renderer.SetPropertyBlock(m_propBlock);
    }

    private void FixedUpdate()
    {
        // 1. 更新状态，计算获取速度
        UpdateState();
        AdjustVelocity();

        // 2. 处理跳跃
        if (m_desiredJump)
        {
            m_desiredJump = false;
            Jump();
        }

        // 3. RigiBody更新速度
        m_body.velocity = m_velocity;

        // 4. 清除状态
        ClearState();
    }

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnValidate()
    {
        m_minGroundDotProduct = Mathf.Cos(m_maxGroundAngle * Mathf.Deg2Rad);
    }

    #endregion

    #region 方法

    /// <summary>
    ///  跳跃逻辑
    /// </summary>
    private void Jump()
    {
        if (OnGround || m_jumpPhase < m_maxAirJumps)
        {
            m_jumpPhase += 1;

            // 1. 根据高度计算跳跃速度
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * m_jumpHeight);

            // 2. 根据接触面法线调整速度 
            float alignedSpeed = Vector3.Dot(m_velocity, m_contactNormal);
            if (alignedSpeed > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }

            // 3. 根据法线和速度计算更新跳跃速度
            m_velocity += m_contactNormal * jumpSpeed;
        }
    }

    /// <summary>
    ///  碰撞检测
    /// </summary>
    /// <param name="collision"> 接触碰撞的信息 </param>
    private void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            // 1. 获取和接触物体的法线
            var normal = collision.GetContact(i).normal;
            // 2. 根据法线和最小地面角度点积判断是否在地面上 
            if (normal.y >= m_minGroundDotProduct)
            {
                // onGround = true;
                m_groundContactCount += 1;
                // 累加法向量
                m_contactNormal += normal;
            }
        }
    }

    /// <summary>
    ///  更新状态
    /// </summary>
    private void UpdateState()
    {
        m_velocity = m_body.velocity;
        if (OnGround)
        {
            m_jumpPhase = 0;
            if (m_groundContactCount > 1)
            {
                m_contactNormal.Normalize();
            }
        }
        else
        {
            m_contactNormal = Vector3.up;
        }
    }

    /// <summary>
    ///  每次FixedUpdate结束时调用，用于清除状态
    /// </summary>
    private void ClearState()
    {
        // 每个物理步骤都以调用所有 FixedUpdate 方法开始，之后 PhysX 执行其操作，最后调用碰撞方法。
        // 因此，当 FixedUpdate 被调用时，如果存在任何活动碰撞， onGround 会在上一步被设置为 true 。
        // 为了保持 onGround 有效，我们只需在 FixedUpdate 结束时将其设置回 false 即可。
        // onGround = false;

        m_groundContactCount = 0;
        m_contactNormal = Vector3.zero;
    }

    /// <summary>
    ///  投影到接触平面
    /// </summary>
    /// <param name="vector"> 需要投影的向量 </param>
    /// <returns> 投影后的向量 </returns>
    private Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        // Vector3.ProjectOnPlane() 不使用是因为，contactNormal 都是单位向量，不需要额外的计算
        return vector - m_contactNormal * Vector3.Dot(vector, m_contactNormal);
    }

    /// <summary>
    /// 根据所在平面方向调整速度
    /// </summary>
    private void AdjustVelocity()
    {
        // 1. 获取平面的 x 和 z 轴 
        var xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        var zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        // 2. 获取当前速度在 x 和 z 轴的分量
        float currentX = Vector3.Dot(m_velocity, xAxis);
        float currentZ = Vector3.Dot(m_velocity, zAxis);
        float acceleration = OnGround ? m_maxAcceleration : m_maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        // 3. 限制速度变化
        float newX =
            Mathf.MoveTowards(currentX, m_desiredVelocity.x, maxSpeedChange);
        float newZ =
            Mathf.MoveTowards(currentZ, m_desiredVelocity.z, maxSpeedChange);
        m_velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    #endregion

    #region 属性

    /// <summary>
    ///  是否在地面上
    /// </summary>
    private bool OnGround => m_groundContactCount > 0;

    #endregion

    #region 字段

    /// <summary>
    ///  物理刚体
    /// </summary>
    private Rigidbody m_body;

    #region 移动逻辑

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
    private float m_maxAirAcceleration = 1f;

    /// <summary>
    ///  当前速度
    /// </summary>
    private Vector3 m_velocity;

    /// <summary>
    ///  输入的期望速度
    /// </summary>
    private Vector3 m_desiredVelocity;

    #endregion

    #region 跳跃逻辑字段

    /// <summary>
    ///  是否跳跃
    /// </summary>
    private bool m_desiredJump;

    /// <summary>
    ///  跳跃高度
    /// </summary>
    [SerializeField]
    [Range(0f, 10f)]
    private float m_jumpHeight = 2f;

    /// <summary>
    ///  最大空中跳跃次数
    /// </summary>
    [SerializeField]
    [Range(0, 5)]
    private int m_maxAirJumps = 0;

    /// <summary>
    ///  最大地面角度
    /// </summary>
    [SerializeField]
    [Range(0f, 90f)]
    private float m_maxGroundAngle = 25f;

    /// <summary>
    ///  最小地面角度点积
    /// </summary>
    private float m_minGroundDotProduct;

    /// <summary>
    ///  当前空中跳跃次数
    /// </summary>
    private int m_jumpPhase;

    /// <summary>
    ///  接触法线
    /// </summary>
    private Vector3 m_contactNormal;

    /// <summary>
    ///  接触面数量
    /// </summary>
    private int m_groundContactCount;

    #endregion

    /// <summary>
    ///  渲染器
    /// </summary>
    private Renderer m_renderer;

    /// <summary>
    ///  材质属性块
    /// </summary>
    private MaterialPropertyBlock m_propBlock;

    /// <summary>
    ///  颜色属性ID
    /// </summary>
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    #endregion
}