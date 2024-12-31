using UnityEngine;
using UnityEngine.Serialization;


public class MovingSphere : MonoBehaviour
{
    #region Unity 生命周期

    void Update()
    {
        // 1. 获取输入并限制到单位长度
        var playerInput = default(Vector2);
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        //  2. 获取当前输入控制的速度
        var desiredVelocity =
            new Vector3(playerInput.x, 0f, playerInput.y) * m_maxSpeed;
        float maxSpeedChange = m_maxAcceleration * Time.deltaTime;

        // 3. 控制速度变化加速
        m_velocity.x =
            Mathf.MoveTowards(m_velocity.x, desiredVelocity.x, maxSpeedChange);
        m_velocity.z =
            Mathf.MoveTowards(m_velocity.z, desiredVelocity.z, maxSpeedChange);

        // 4. 计算位移变化量
        var displacement = m_velocity * Time.deltaTime;

        // 5. 限制移动访问, 各个方向分别判断，防止粘墙
        var newPosition = transform.localPosition + displacement;
        if (newPosition.x < m_allowedArea.xMin)
        {
            newPosition.x = m_allowedArea.xMin;
            m_velocity.x = -m_velocity.x * m_bounciness;
        }
        else if (newPosition.x > m_allowedArea.xMax)
        {
            newPosition.x = m_allowedArea.xMax;
            m_velocity.x = -m_velocity.x * m_bounciness;
        }

        if (newPosition.z < m_allowedArea.yMin)
        {
            newPosition.z = m_allowedArea.yMin;
            m_velocity.z = -m_velocity.z * m_bounciness;
        }
        else if (newPosition.z > m_allowedArea.yMax)
        {
            newPosition.z = m_allowedArea.yMax;
            m_velocity.z = -m_velocity.z * m_bounciness;
        }

        transform.localPosition = newPosition;
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
    [SerializeField, Range(0f, 100f)]
    private float m_maxAcceleration = 10f;

    /// <summary>
    ///  当前速度
    /// </summary>
    private Vector3 m_velocity;

    /// <summary>
    ///  限制移动的范围
    /// </summary>
    [SerializeField]
    private Rect m_allowedArea = new Rect(-5f, -5f, 10f, 10f);

    /// <summary>
    ///  弹力
    /// </summary>
    [SerializeField, Range(0f, 1f)]
    private float m_bounciness = 0.5f;

    #endregion
}