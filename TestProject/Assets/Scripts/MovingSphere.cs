using System.Runtime.CompilerServices;
using UnityEngine;

public class MovingSphere : MonoBehaviour
{
    #region 函数

    private void Awake()
    {
        m_body = GetComponent<Rigidbody>();
        OnValidate();
    }

    private void Update()
    {
        // 1. 初始化玩家输入
        Vector2 playerInput;
        playerInput.x = 0f;
        playerInput.y = 0f;

        // 2. 获取玩家水平和垂直的输入
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        // 3. 如果以物体为准的局部坐标系的玩家输入不为空则获取局部坐标系的玩家输入
        if (m_playerInputSpace)
        {
            m_rightAxis = ProjectDirectionOnPlane(m_playerInputSpace.right, m_upAxis);
            m_forwardAxis = ProjectDirectionOnPlane(m_playerInputSpace.forward, m_upAxis);
        }
        else
        {
            m_rightAxis = ProjectDirectionOnPlane(Vector3.right, m_upAxis);
            m_forwardAxis = ProjectDirectionOnPlane(Vector3.forward, m_upAxis);
        }

        // 4. 计算Sphere的速度
        m_desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * m_maxSpeed;
        Debug.Log(nameof(m_desiredVelocity) + ":" + m_desiredVelocity);

        // 5. 获取玩家输入Sphere是否需要跳跃
        m_desiredJump |= Input.GetButtonDown("Jump");
    }


    private void FixedUpdate()
    {
        m_upAxis = -Physics.gravity.normalized;

        // 1. 更新Sphere的状态
        UpdateState();
        AdjustVelocity();

        // 2. Sphere跳跃
        if (m_desiredJump)
        {
            m_desiredJump = false;
            Jump();
        }

        m_body.velocity = m_velocity;
        CleanState();
    }

    /// <summary>
    /// Sphere跳跃
    /// </summary>
    private void Jump()
    {
        Vector3 jumpDirection;
        // 1. Sphere在地面上，则跳跃方向为接触点法线
        if (OnGround)
        {
            jumpDirection = m_contactNormal;
        }
        // 2. Sphere在斜坡上，则跳跃方向为斜坡法线
        else if (OnSteep)
        {
            jumpDirection = m_steepNormal;
            m_jumpPhase = 0;
        }
        // 3. Sphere在空中且未达到最大空中跳跃次数，则跳跃方向为接触点法线
        else if (m_maxAirJumps > 0 && m_jumpPhase <= m_maxAirJumps)
        {
            if (m_jumpPhase == 0)
            {
                m_jumpPhase = 1;
            }

            jumpDirection = m_contactNormal;
        }
        // 4. 否则不跳跃
        else
        {
            return;
        }

        // 5. 通过添加向上的向量来改变跳跃方向以便达到跳墙的目的
        jumpDirection = (jumpDirection + m_upAxis).normalized;

        m_stepsSinceLastJump = 0;
        // 6. 更新当前跳跃次数
        m_jumpPhase += 1;
        // 7. 计算跳跃速度
        float jumpSpeed = Mathf.Sqrt(2f * Physics.gravity.magnitude * m_jumpHeight);
        float alignedSpeed = Vector3.Dot(m_velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }

        // 8. 添加跳跃的速度到Sphere的当前速度
        m_velocity += jumpDirection * jumpSpeed;
    }

    /// <summary>
    /// Sphere状态更新
    /// </summary>
    private void UpdateState()
    {
        m_stepsSinceLastGrounded += 1;
        m_stepsSinceLastJump += 1;
        // 1. 更新Sphere的速度
        m_velocity = m_body.velocity;
        // 2. 判断Sphere是否在地面上
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            // 2.1. 如果Sphere在地面上，则重置距离上一次接触地面的步数和空中跳跃次数
            m_stepsSinceLastGrounded = 0;
            if (m_stepsSinceLastJump > 1)
            {
                m_jumpPhase = 0;
            }

            // 2.2. 如果Sphere在地面上，则重置接触点法线
            if (m_groundContactCount > 1)
            {
                m_contactNormal.Normalize();
            }
        }
        else
        {
            // 2.3. 如果Sphere不在地面上，则更新接触点法线
            m_contactNormal = m_upAxis;
        }
    }

    /// <summary>
    /// Sphere状态清理
    /// </summary>
    private void CleanState()
    {
        m_groundContactCount = m_steepContactCount = 0;
        m_contactNormal = m_steepNormal = Vector3.zero;
    }

    /// <summary>
    /// Sphere与物体的碰撞
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    /// <summary>
    /// Sphere与物体的持续碰撞
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    /// <summary>
    /// Sphere与物体的碰撞处理
    /// </summary>
    /// <param name="collision"></param>
    private void EvaluateCollision(Collision collision)
    {
        float minDot = GetMinDot(collision.gameObject.layer);
        // 1. 计算接触点法线
        for (int i = 0; i < collision.contactCount; ++i)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(m_upAxis, normal);
            // 1.1. 判断是否接触地面
            if (upDot >= minDot)
            {
                // 1.1.1. 更新与地面的接触点数量和法线
                m_groundContactCount += 1;
                m_contactNormal += normal;
            }
            // 1.2. 判断是否接触斜坡
            else if (upDot > -0.01f)
            {
                // 1.2.1. 更新与斜墙的接触点数量和法线
                m_steepContactCount += 1;
                m_steepNormal += normal;
            }
        }
    }

    /// <summary>
    /// 计算可被记为接触面的地面角度的最小点积
    /// </summary>
    private void OnValidate()
    {
        m_minGroundDotProduct = Mathf.Cos(m_maxGroundAngle * Mathf.Deg2Rad); // 以弧度表示角度
        m_minStairsDotProduct = Mathf.Cos(m_maxStairsAngle * Mathf.Deg2Rad);
    }

    // /// <summary>
    // /// 将向量投影到接触点法线上
    // /// </summary>
    // /// <param name="vector"></param>
    // /// <returns></returns>
    // private Vector3 ProjectOnContactPlane(Vector3 vector)
    // {
    //     return vector - m_contactNormal * Vector3.Dot(vector, m_contactNormal);
    // }

    private Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    /// <summary>
    /// 调整Sphere的速度
    /// </summary>
    private void AdjustVelocity()
    {
        // 1. 获取坐标轴
        Vector3 xAxis = ProjectDirectionOnPlane(m_rightAxis, m_contactNormal);
        Vector3 zAxis = ProjectDirectionOnPlane(m_forwardAxis, m_contactNormal);

        // 2.计算Sphere在z轴和x轴上的速度
        float currentX = Vector3.Dot(m_velocity, xAxis);
        float currentZ = Vector3.Dot(m_velocity, zAxis);


        float acceleration = OnGround ? m_maxAcceleration : m_maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;
        // 3. 获取Sphere在x轴和z轴的目标速度
        float newX = Mathf.MoveTowards(currentX, m_desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, m_desiredVelocity.z, maxSpeedChange);
        // 4. 更新Sphere的速度
        m_velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        Debug.Log(xAxis + ":" + zAxis);
        // Debug.Log(nameof(m_velocity) + ":" + m_velocity);
    }

    /// <summary>
    /// 将Sphere黏在地面上
    /// </summary>
    /// <returns></returns>
    private bool SnapToGround()
    {
        // 1. 如果Sphere距离上一次接触地面的步数大于1或者Sphere距离上一次跳跃的步数小于等于2, 则不进行黏住
        if (m_stepsSinceLastGrounded > 1 || m_stepsSinceLastJump <= 2)
        {
            return false;
        }

        float speed = m_velocity.magnitude;
        // 2. 如果Sphere的速度标量大于最大黏住速度, 则不进行黏住
        if (speed > m_maxSnapSpeed)
        {
            return false;
        }

        // 3. 如果射线没检测到设定的层, 则不进行黏住
        if (!Physics.Raycast(m_body.position, -m_upAxis, out RaycastHit hit, m_probeDistance, m_probeMask))
        {
            return false;
        }

        // 4. 如果检测到的碰撞点的法线的y值小于最小地面角度的点积, 则不进行黏住
        float upDot = Vector3.Dot(m_upAxis, hit.normal);
        if (upDot < GetMinDot(hit.transform.gameObject.layer))
        {
            return false;
        }

        // 5. 否则进行黏住处理,更新与地面的接触点数量和法线
        m_groundContactCount = 1;
        m_contactNormal = hit.normal;
        float dot = Vector3.Dot(m_velocity, hit.normal);
        // 6. 如果Sphere的速度与接触点法线不是相反方向, 则进行速度修正
        if (dot > 0f)
        {
            m_velocity = (m_velocity - hit.normal * dot).normalized * speed;
        }

        return true;
    }

    /// <summary>
    /// 获取指定层的最小点积
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    private float GetMinDot(int layer)
    {
        return (m_stairsMask & (1 << layer)) == 0 ? m_minGroundDotProduct : m_minStairsDotProduct;
    }

    /// <summary>
    /// 检查是否与多个斜墙有接触, 如果有, 则将变为着地状态
    /// </summary>
    /// <returns></returns>
    private bool CheckSteepContacts()
    {
        if (m_steepContactCount > 1)
        {
            m_steepNormal.Normalize();
            float upDot = Vector3.Dot(m_upAxis, m_steepNormal);
            if (upDot >= m_minGroundDotProduct)
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
    /// 判断Sphere是否在地面上
    /// </summary>
    private bool OnGround => m_groundContactCount > 0;

    /// <summary>
    /// 判断Sphere是否与斜墙有接触
    /// </summary>
    private bool OnSteep => m_steepContactCount > 0;

    #endregion

    #region 字段

    /// <summary>
    /// Sphere允许的最大速度
    /// </summary>
    [SerializeField]
    [Range(0f, 100f)]
    private float m_maxSpeed = 10f;

    /// <summary>
    /// Sphere的当前速度
    /// </summary>
    private Vector3 m_velocity;

    /// <summary>
    /// 用户输入期望Sphere的速度
    /// </summary>
    private Vector3 m_desiredVelocity;

    /// <summary>
    /// Sphere的最大加速度
    /// </summary>
    [SerializeField]
    [Range(0f, 100f)]
    private float m_maxAcceleration = 10f;

    /// <summary>
    /// 用户输入期望Sphere是否跳跃
    /// </summary>
    private bool m_desiredJump;

    /// <summary>
    /// Sphere的最大空中跳跃次数
    /// </summary>
    [SerializeField]
    [Range(0, 5)]
    private int m_maxAirJumps = 0;

    /// <summary>
    /// Sphere的当前空中跳跃次数
    /// </summary>
    private int m_jumpPhase;

    /// <summary>
    /// Sphere的跳跃高度
    /// </summary>
    [SerializeField]
    [Range(0f, 10f)]
    private float m_jumpHeight = 2f;

    /// <summary>
    /// Sphere的最大空中加速度
    /// </summary>
    [SerializeField]
    [Range(0f, 100f)]
    private float m_maxAirAcceleration = 1f;

    /// <summary>
    /// 与地面的接触点数量
    /// </summary>
    private int m_groundContactCount;

    /// <summary>
    /// 与斜墙的接触点数量
    /// </summary>
    private int m_steepContactCount;

    /// <summary>
    /// Sphere的刚体组件
    /// </summary>
    private Rigidbody m_body;

    /// <summary>
    /// 最大地面角度
    /// </summary>
    [SerializeField]
    [Range(0f, 90f)]
    private float m_maxGroundAngle = 25f;

    /// <summary>
    /// 最大楼梯角度
    /// </summary>
    [SerializeField]
    [Range(0f, 90f)]
    private float m_maxStairsAngle = 50f;

    /// <summary>
    /// 算入接触面的地面角度的最小点积
    /// </summary>
    private float m_minGroundDotProduct;

    /// <summary>
    /// 算入接触面的楼梯角度的最小点积
    /// </summary>
    private float m_minStairsDotProduct;

    /// <summary>
    /// Sphere与地面的接触点法线
    /// </summary>
    private Vector3 m_contactNormal;

    /// <summary>
    /// Sphere与斜墙的接触点法线
    /// </summary>
    private Vector3 m_steepNormal;

    /// <summary>
    /// Sphere距离上一次接触地面的步数
    /// </summary>
    private int m_stepsSinceLastGrounded;

    /// <summary>
    /// Sphere距离上一次跳跃的步数
    /// </summary>
    private int m_stepsSinceLastJump;

    /// <summary>
    /// Sphere可被黏住的最大速度
    /// </summary>
    [SerializeField]
    [Range(0f, 100f)]
    private float m_maxSnapSpeed = 100f;

    /// <summary>
    /// Sphere的探测距离
    /// </summary>
    [SerializeField]
    [Min(0f)]
    private float m_probeDistance = 1f;

    /// <summary>
    /// Sphere需要探测的层
    /// </summary>
    [SerializeField]
    private LayerMask m_probeMask = -1;

    /// <summary>
    /// 楼梯层(斜坡层)
    /// </summary>
    [SerializeField]
    private LayerMask m_stairsMask = -1;

    /// <summary>
    /// 以物体的局部坐标系为准的玩家输入
    /// </summary>
    [SerializeField]
    private Transform m_playerInputSpace = default;

    /// <summary>
    /// 重力方向
    /// </summary>
    private Vector3 m_upAxis;

    private Vector3 m_rightAxis;

    private Vector3 m_forwardAxis;

    #endregion
}