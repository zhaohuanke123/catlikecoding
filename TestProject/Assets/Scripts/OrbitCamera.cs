using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    #region Methods

    private void Awake()
    {
        // 1.获取相机组件
        m_regularCamera = GetComponent<Camera>();
        
        // 2.初始化聚焦点即跟踪目标的位置
        m_focusPoint = m_focus.position;
        transform.localRotation = m_orbitRotation = Quaternion.Euler(m_orbitAngles);
    }

    private void LateUpdate()
    {
        m_gravityAlignment = Quaternion.FromToRotation(m_gravityAlignment * Vector3.up, -Physics.gravity.normalized) *
                             m_gravityAlignment;
        
        // 1. 更新聚焦点
        UpdateFocusPoint();
        
        // 2. 判断是否需要手动旋转或者自动旋转
        // Quaternion lookRotation;
        if (ManualRotation() || AutomaticRotation())
        {
            ConstrainAngles();
            m_orbitRotation = Quaternion.Euler(m_orbitAngles);
        }
        // else
        // {
        //     lookRotation = transform.localRotation;
        // }
        
        Quaternion lookRotation = m_gravityAlignment * m_orbitRotation;
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = m_focusPoint - lookDirection * m_distance;
        
        Vector3 rectOffset = lookDirection * m_regularCamera.nearClipPlane;
        Vector3 rectPosition = lookPosition + rectOffset;
        Vector3 castFrom = m_focus.position;
        Vector3 castLine = rectPosition - castFrom;
        float castDistance = castLine.magnitude;
        Vector3 castDirection = castLine / castDistance;

        // 2. 检测是否有遮挡物然后调整相机位置和朝向
        if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out RaycastHit hit, lookRotation, castDistance, m_obstructionMask))
        {
            rectPosition = castFrom + castDirection * hit.distance;
            lookPosition = rectPosition - rectOffset;
        }

        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    /// <summary>
    /// 更新聚焦点
    /// </summary>
    private void UpdateFocusPoint()
    {
        // 1. 记录上一个聚焦点
        m_previousFocusPoint = m_focusPoint;
        
        // 2. 更新聚焦点
        Vector3 targetPoint = m_focus.position;
        if (m_focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, m_focusPoint);
            float t = 1f;
            if (distance > 0.01f && m_focusCentering > 0f)
            {
                t = Mathf.Pow(1f - m_focusCentering, Time.deltaTime);
            }

            if (distance > m_focusRadius)
            {
                t = Mathf.Min(t, m_focusRadius / distance);
            }

            m_focusPoint = Vector3.Lerp(targetPoint, m_focusPoint, t);
        }
        else
        {
            m_focusPoint = targetPoint;
        }
    }

    /// <summary>
    /// 检测是否发生手动旋转
    /// </summary>
    /// <returns></returns>
    private bool ManualRotation()
    {
        Vector2 input = new Vector2(Input.GetAxis("Vertical Camera"), Input.GetAxis("Horizontal Camera"));
        const float e = 0.001f;
        if (input.x < -e || input.x > e || input.y < -e || input.y > e)
        {
            m_orbitAngles += m_rotationSpeed * Time.unscaledDeltaTime * input;
            m_lastManualRotationTime = Time.unscaledTime;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 检测是否需要自动旋转
    /// </summary>
    /// <returns></returns>
    private bool AutomaticRotation()
    {
        // 1. 如果距离上一次手动旋转时间太短则不旋转
        if (Time.unscaledTime - m_lastManualRotationTime < m_alignDelay)
        {
            return false;
        }

        Vector3 alignedDelta = Quaternion.Inverse(m_gravityAlignment) * (m_focusPoint - m_previousFocusPoint);
        Vector2 movement = new Vector2(alignedDelta.x, alignedDelta.z);
        
        // 2. 如果偏移距离太小则不旋转
        float movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.0001f)
        {
            return false;
        }

        // 3. 计算偏航角度
        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(m_orbitAngles.y, headingAngle));
        float rotationChange = m_rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
        if (deltaAbs < m_alignSmoothRange)
        {
            rotationChange *= deltaAbs / m_alignSmoothRange;
        }
        else if (180f - deltaAbs < m_alignSmoothRange)
        {
            rotationChange *= (180f - deltaAbs) / m_alignSmoothRange;
        }

        m_orbitAngles.y = Mathf.MoveTowardsAngle(m_orbitAngles.y, headingAngle, rotationChange);

        return true;
    }

    /// <summary>
    /// 验证数据正确性
    /// </summary>
    private void OnValidate()
    {
        // 1. 强制相机最大俯仰角度不小于最小俯仰角度
        if (m_maxVerticalAngle < m_minVerticalAngle)
        {
            m_maxVerticalAngle = m_minVerticalAngle;
        }
    }
    
    /// <summary>
    /// 限制俯仰角度范围
    /// </summary>
    private void ConstrainAngles()
    {
        // 1. 限制俯仰角度范围
        m_orbitAngles.x = Mathf.Clamp(m_orbitAngles.x, m_minVerticalAngle, m_maxVerticalAngle);
        // 2. 检查偏航角度是否超出范围[0, 360]，如果超出则修正
        if (m_orbitAngles.y < 0f)
        {
            m_orbitAngles.y += 360f;
        }
        else if (m_orbitAngles.y >= 360f)
        {
            m_orbitAngles.y -= 360f;
        }
    }

    /// <summary>
    /// 获取一个向量的角度
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private static float GetAngle(Vector2 direction)
    {
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle;
    }

    #endregion

    #region Properties

    /// <summary>
    /// 计算相机近裁剪面的一半的大小，用向量表示
    /// </summary>
    private Vector3 CameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;
            halfExtends.y = m_regularCamera.nearClipPlane *
                            Mathf.Tan(0.5f * Mathf.Deg2Rad * m_regularCamera.fieldOfView);
            halfExtends.x = halfExtends.y * m_regularCamera.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }

    #endregion

    #region Fields

    /// <summary>
    /// 需要跟踪的目标
    /// </summary>
    [SerializeField]
    private Transform m_focus = default;

    /// <summary>
    /// 相机离目标的距离
    /// </summary>
    [SerializeField]
    [Range(1f, 20f)]
    private float m_distance = 5f;

    /// <summary>
    /// 聚焦半径
    /// </summary>
    [SerializeField]
    [Min(0f)]
    private float m_focusRadius = 1f;

    /// <summary>
    /// 当前聚焦点
    /// </summary>
    private Vector3 m_focusPoint;

    /// <summary>
    /// 上一个聚焦点
    /// </summary>
    private Vector3 m_previousFocusPoint;

    [SerializeField]
    [Range(0f, 1f)]
    private float m_focusCentering = 0.5f;

    /// <summary>
    /// 相机的俯仰角度和偏航角度
    /// </summary>
    private Vector2 m_orbitAngles = new Vector2(45f, 0f);

    /// <summary>
    /// 相机偏转速度
    /// </summary>
    [SerializeField]
    [Range(1f, 360f)]
    private float m_rotationSpeed = 90f;

    /// <summary>
    /// 相机最小俯仰角度
    /// </summary>
    [SerializeField]
    [Range(-89f, 89f)]
    private float m_minVerticalAngle = -30f;

    /// <summary>
    /// 相机最大俯仰角度
    /// </summary>
    [SerializeField]
    [Range(-89f, 89f)]
    private float m_maxVerticalAngle = 60f;

    /// <summary>
    /// 相机自动对准延迟
    /// </summary>
    [SerializeField]
    [Min(0f)]
    private float m_alignDelay = 5f;

    /// <summary>
    /// 上一次手动旋转的时间
    /// </summary>
    private float m_lastManualRotationTime;

    /// <summary>
    /// 相机自动对准平滑范围
    /// </summary>
    [SerializeField]
    [Range(0f, 90f)]
    private float m_alignSmoothRange = 45f;

    /// <summary>
    /// 相机组件
    /// </summary>
    private Camera m_regularCamera;

    /// <summary>
    /// 遮挡物层掩码
    /// </summary>
    [SerializeField]
    private LayerMask m_obstructionMask = -1;

    private Quaternion m_gravityAlignment = Quaternion.identity;

    private Quaternion m_orbitRotation;

    #endregion
}