using UnityEngine;

/// <summary>
/// 自定义重力应用于刚体的组件
/// 该组件负责通过自定义重力向物体施加加速度，并提供一种延迟休眠机制。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CustomGravityRigidbody : MonoBehaviour
{
    #region Unity 生命周期

    private void Awake()
    {
        m_body = GetComponent<Rigidbody>();
        m_body.useGravity = false;
    }

    private void FixedUpdate()
    {
        // 1. 如果启用了“浮动到休眠”模式
        if (m_floatToSleep)
        {
            if (m_body.IsSleeping())
            {
                m_floatDelay = 0f; 
                return;
            }

            // 如果刚体速度很小，认为它接近停止
            if (m_body.velocity.sqrMagnitude < 0.0001f)
            {
                m_floatDelay += Time.deltaTime;
                if (m_floatDelay >= 1f) 
                {
                    return; 
                }
            }
            else
            {
                m_floatDelay = 0f;
            }
        }

        // 2. 应用自定义重力
        m_body.AddForce(CustomGravity.GetGravity(m_body.position), ForceMode.Acceleration);
    }

    #endregion

    #region 字段

    /// <summary>
    /// 是否启用“浮动到休眠”模式
    /// 如果为true，刚体会在接近静止时延迟休眠。
    /// </summary>
    [SerializeField]
    private bool m_floatToSleep = false;

    /// <summary>
    /// 刚体组件
    /// </summary>
    private Rigidbody m_body;

    /// <summary>
    /// 用于延迟浮动到休眠的计时器
    /// </summary>
    private float m_floatDelay;

    #endregion
}