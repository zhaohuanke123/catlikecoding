using UnityEngine;

public class MovingSphere : MonoBehaviour
{
    #region Unity 生命周期

    private void Awake()
    {
        m_body = GetComponent<Rigidbody>();
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

        // 3. RigidBody更新速度
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
        m_minStairsDotProduct = Mathf.Cos(m_maxStairsAngle * Mathf.Deg2Rad);
    }

    #endregion

    #region 方法

    /// <summary>
    ///  跳跃逻辑
    /// </summary>
    private void Jump()
    {
        // 1. 获取跳跃方向
        var jumpDirection = default(Vector3);
        if (OnGround)
        {
            jumpDirection = m_contactNormal;
        }
        else if (OnSteep)
        {
            jumpDirection = m_steepNormal;
            m_jumpPhase = 0;
        }
        else if (m_maxAirJumps > 0 && m_jumpPhase <= m_maxAirJumps)
        {
            if (m_jumpPhase == 0)
            {
                m_jumpPhase = 1;
            }

            jumpDirection = m_contactNormal;
        }
        else
        {
            return;
        }

        m_jumpPhase += 1;

        // 2. 根据高度计算跳跃速度
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * m_jumpHeight);

        // 3. 根据接触面法线调整速度 
        float alignedSpeed = Vector3.Dot(m_velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }

        // 4. 根据法线和速度计算更新跳跃速度
        m_velocity += jumpDirection * jumpSpeed;
    }

    /// <summary>
    ///  碰撞检测
    /// </summary>
    /// <param name="collision"> 接触碰撞的信息 </param>
    private void EvaluateCollision(Collision collision)
    {
        // 1. 根据不同的层级获取接触面处理的最小点积
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++)
        {
            // 2. 获取和接触物体的法线
            var normal = collision.GetContact(i).normal;
            // 3. 根据法线和最小地面角度点积判断是否在地面上 
            if (normal.y >= minDot)
            {
                // onGround = true;
                m_groundContactCount += 1;
                // 累加法向量
                m_contactNormal += normal;
            }
            // 4. 检查一下它是否是一个陡峭的接触。一个完美垂直墙壁的点积应该为零，但让我们宽松一些，接受所有大于−0.01 的值
            else if (normal.y > -0.01f)
            {
                m_steepContactCount += 1;
                m_steepNormal += normal;
            }
        }
    }

    /// <summary>
    ///  更新状态
    /// </summary>
    private void UpdateState()
    {
        // 1. 累加距离上次FixUpdate 步数
        m_stepsSinceLastGrounded += 1;
        m_stepsSinceLastJump += 1;

        // 2. 获取rigidbody的速度
        m_velocity = m_body.velocity;

        // 3. 更新地面或空中状态
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            m_stepsSinceLastGrounded = 0;
            if (m_stepsSinceLastJump > 1)
            {
                m_stepsSinceLastJump = 0;
            }

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

        m_groundContactCount = m_steepContactCount = 0;
        m_contactNormal = m_steepNormal = Vector3.zero;
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

    /// <summary>
    /// 尝试将物体的当前位置“吸附”到地面上。通过射线检测地面并调整物体的速度和位置，以便物体在接触到地面时与地面平行。
    /// </summary>
    /// <returns>如果物体成功吸附到地面，返回 true；否则返回 false。</returns>
    private bool SnapToGround()
    {
        if (m_stepsSinceLastGrounded > 1 || m_stepsSinceLastJump <= 2)
        {
            return false;
        }

        float speed = m_velocity.magnitude;
        if (speed > m_maxSnapSpeed)
        {
            return false;
        }

        // 1. 使用射线检测从当前物体位置向下方是否有碰撞
        if (!Physics.Raycast(m_body.position, Vector3.down, out var hit, m_probeDistance, m_probeMask))
        {
            return false;
        }

        // 2. 如果碰撞表面的法线与世界坐标系的y轴的点积小于最小允许值，则表示该表面不可作为地面
        if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }

        // 3. 如果射线检测到有效地面，更新地面接触计数器和法线
        m_groundContactCount = 1;
        m_contactNormal = hit.normal;

        // 4. 获取当前速度的大小（速率）
        // float speed = m_velocity.magnitude;

        // 5. 计算速度与地面法线的点积，如果速度朝着地面法线方向，则需要调整速度
        float dot = Vector3.Dot(m_velocity, hit.normal);
        if (dot > 0f)
        {
            // 将速度沿着地面法线方向的分量减去，保持物体与地面平行
            m_velocity = (m_velocity - hit.normal * dot).normalized * speed;
        }

        return true;
    }

    private float GetMinDot(int layer)
    {
        return m_stairsMask != layer ? m_minGroundDotProduct : m_minStairsDotProduct;
    }

    /// <summary>
    ///  检查陡峭的接触
    /// </summary>
    /// <returns>  如果检测到陡峭的接触返回true，否则返回false </returns>
    private bool CheckSteepContacts()
    {
        if (m_steepContactCount > 1)
        {
            m_steepNormal.Normalize();
            if (m_steepNormal.y >= m_minGroundDotProduct)
            {
                m_groundContactCount = 1;
                m_contactNormal = m_steepNormal;
                return true;
            }
        }

        return false;
    }

    #endregion

    #region 属性

    /// <summary>
    ///  是否在地面上
    /// </summary>
    private bool OnGround => m_groundContactCount > 0;

    private bool OnSteep => m_steepContactCount > 0;

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
    ///  陡峭面法线 
    /// </summary>
    private Vector3 m_steepNormal;

    /// <summary>
    ///  接触面数量
    /// </summary>
    private int m_groundContactCount;

    /// <summary>
    ///  陡峭面接触数量 
    /// </summary>
    private int m_steepContactCount;

    #endregion

    /// <summary>
    ///  材质属性块
    /// </summary>
    private MaterialPropertyBlock m_propBlock;

    #region 地面吸附逻辑字段

    /// <summary>
    ///  距离上次接触地面的FixUpdate 步数
    /// </summary>
    private int m_stepsSinceLastGrounded;

    /// <summary>
    ///  最大吸附速度, 当前速度超过该速度则不吸附
    /// </summary>
    [SerializeField]
    [Range(0f, 100f)]
    private float m_maxSnapSpeed = 100f;

    /// <summary>
    ///  射线探测距离
    /// </summary>
    [SerializeField]
    [Min(0f)]
    private float m_probeDistance = 1f;

    /// <summary>
    ///  射线探测层
    /// </summary>
    [SerializeField]
    private LayerMask m_probeMask = -1;

    /// <summary>
    ///  距离上次跳跃的FixUpdate步数
    /// </summary>
    private int m_stepsSinceLastJump;

    #endregion

    /// <summary>
    ///  最大楼梯角度 
    /// </summary>
    [SerializeField]
    [Range(0f, 90f)]
    private float m_maxStairsAngle = 50f;

    /// <summary>
    /// 最小楼梯角度点积 
    /// </summary>
    private float m_minStairsDotProduct;

    /// <summary>
    ///  楼梯Mask
    /// </summary>
    [SerializeField]
    private LayerMask m_stairsMask = -1;

    #endregion
}