using System;
using UnityEngine;

/// <summary>
///  轨道相机
/// </summary>
[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    #region Unity 生命周期

    private void OnValidate()
    {
        // 控制最大值不低于最小值
        if (m_maxVerticalAngle < m_minVerticalAngle)
        {
            m_maxVerticalAngle = m_minVerticalAngle;
        }
    }

    private void Awake()
    {
        m_regularCamera = GetComponent<Camera>();
        m_focusPoint = m_focus.position;
        transform.localRotation = m_orbitRotation = Quaternion.Euler(m_orbitAngles);
    }

    private void LateUpdate()
    {
        UpdateGravityAlignment();

        // 1. 更新焦点位置。
        UpdateFocusPoint();

        // 2. 检查是否需要手动或自动旋转。
        if (ManualRotation() || AutomaticRotation())
        {
            // 3. 限制旋转角度在设定范围内。
            ConstrainAngles();
            m_orbitRotation = Quaternion.Euler(m_orbitAngles);
        }

        var lookRotation = m_gravityAlignment * m_orbitRotation;
        // 4. 计算相机的朝向和位置。
        var lookDirection = lookRotation * Vector3.forward;
        var lookPosition = m_focusPoint - lookDirection * m_distance;

        // 5. 根据相机近平面的偏移量调整碰撞检测。
        var rectOffset = lookDirection * m_regularCamera.nearClipPlane;
        var rectPosition = lookPosition + rectOffset;
        var castFrom = m_focus.position;
        var castLine = rectPosition - castFrom;
        float castDistance = castLine.magnitude;
        var castDirection = castLine / castDistance;

        // 6. 执行盒体投射，避免相机穿透障碍物。
        if (Physics.BoxCast(
                castFrom, CameraHalfExtends, castDirection, out var hit,
                lookRotation, castDistance, m_obstructionMask
            ))
        {
            // 调整位置以确保相机不会穿透物体。
            rectPosition = castFrom + castDirection * hit.distance;
            lookPosition = rectPosition - rectOffset;
        }

        // 7. 更新相机的位置和旋转。
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    #endregion

    #region 方法

    /// <summary>
    /// 更新焦点位置。
    /// </summary>
    private void UpdateFocusPoint()
    {
        // 1. 记录上一帧的焦点位置。
        m_previousFocusPoint = m_focusPoint;

        // 2. 获取目标位置。
        var targetPoint = m_focus.position;

        // 3. 如果定义了焦点半径，应用平滑过渡。
        if (m_focusRadius > 0f)
        {
            // 计算当前距离。
            float distance = Vector3.Distance(targetPoint, m_focusPoint);

            // 4. 如果距离较大且有居中因子，逐步减小距离。
            float t = 1f;
            if (distance > 0.01f && m_focusCentering > 0f)
            {
                t = Mathf.Pow(1f - m_focusCentering, Time.unscaledDeltaTime);
            }

            // 5. 如果超过焦点半径，则限制移动范围。
            if (distance > m_focusRadius)
            {
                t = Mathf.Min(t, m_focusRadius / distance);
            }

            // 6. 使用插值平滑移动到新位置。
            m_focusPoint = Vector3.Lerp(targetPoint, m_focusPoint, t);
        }
        else
        {
            // 如果没有半径约束，直接设置为目标位置。
            m_focusPoint = targetPoint;
        }
    }

    /// <summary>
    ///  是否输入控制旋转
    /// </summary>
    /// <returns> 有则返回 true，否则返回 false </returns>
    private bool ManualRotation()
    {
        // 1. 获取相机旋转输入。
        var input = new Vector2(
            Input.GetAxis("Vertical Camera"),
            Input.GetAxis("Horizontal Camera")
        );

        // 2. 检查输入是否超过阈值。
        const float e = 0.001f;
        if (input.x < -e || input.x > e || input.y < -e || input.y > e)
        {
            // 3. 更新旋转角度。
            m_orbitAngles += m_rotationSpeed * Time.unscaledDeltaTime * input;
            m_lastManualRotationTime = Time.unscaledTime;
            return true;
        }

        return false;
    }

    /// <summary>
    ///  是否需要自动旋转
    /// </summary>
    /// <returns> 有则返回 true，否则返回 false </returns>
    private bool AutomaticRotation()
    {
        // 1. 如果手动旋转时间较短，不自动旋转。
        if (Time.unscaledTime - m_lastManualRotationTime < m_alignDelay)
        {
            return false;
        }

        // 2. 根据移动方向计算目标角度。
        var alignedDelta = Quaternion.Inverse(m_gravityAlignment) * (m_focusPoint - m_previousFocusPoint);
        var movement = new Vector2(alignedDelta.x, alignedDelta.z);
        float movementDeltaSqr = movement.sqrMagnitude;

        // 3. 如果移动距离很小，不自动旋转。
        if (movementDeltaSqr < 0.0001f)
        {
            return false;
        }

        // 3. 平滑调整角度。
        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(m_orbitAngles.y, headingAngle));
        float rotationChange =
            m_rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);

        // 4. 根据角度差距调整旋转速度。
        if (deltaAbs < m_alignSmoothRange)
        {
            rotationChange *= deltaAbs / m_alignSmoothRange;
        }
        else if (180f - deltaAbs < m_alignSmoothRange)
        {
            rotationChange *= (180f - deltaAbs) / m_alignSmoothRange;
        }

        // 5. 应用旋转变化。
        m_orbitAngles.y =
            Mathf.MoveTowardsAngle(m_orbitAngles.y, headingAngle, rotationChange);
        return true;
    }

    /// <summary>
    /// 限制旋转角度。
    /// </summary>
    private void ConstrainAngles()
    {
        // 1. 限制垂直角度。
        m_orbitAngles.x =
            Mathf.Clamp(m_orbitAngles.x, m_minVerticalAngle, m_maxVerticalAngle);

        // 2. 保持水平角度在 0-360 度范围内。
        if (m_orbitAngles.y < 0f)
        {
            m_orbitAngles.y += 360f;
        }
        else if (m_orbitAngles.y >= 360f)
        {
            m_orbitAngles.y -= 360f;
        }
    }

    void UpdateGravityAlignment()
    {
        Vector3 fromUp = m_gravityAlignment * Vector3.up;
        Vector3 toUp = CustomGravity.GetUpAxis(m_focusPoint);
        float dot = Mathf.Clamp(Vector3.Dot(fromUp, toUp), -1f, 1f);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
        float maxAngle = upAlignmentSpeed * Time.deltaTime;

        Quaternion newAlignment =
            Quaternion.FromToRotation(fromUp, toUp) * m_gravityAlignment;
        if (angle <= maxAngle)
        {
            m_gravityAlignment = newAlignment;
        }
        else
        {
            m_gravityAlignment = Quaternion.SlerpUnclamped(
                m_gravityAlignment, newAlignment, maxAngle / angle
            );
        }
    }

    #endregion

    #region 静态工具方法

    /// <summary>
    ///  获取方向的角度
    /// </summary>
    ///  <param name="direction"> 方向 </param>
    /// <returns> 计算得到的角度 </returns>
    private static float GetAngle(Vector2 direction)
    {
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle;
    }

    #endregion

    #region 属性

    /// <summary>
    ///  相机的半扩展面
    /// </summary>
    private Vector3 CameraHalfExtends
    {
        get
        {
            // 盒体投射需要一个 3D 向量，其中包含盒体的半扩展，这意味着其宽度、高度和深度的二分之一。
            // 高度的一半可以通过取相机视野角（以弧度为单位）的正切值，并按其近裁剪平面距离缩放来求得。宽度的一半是通过将其按相机的纵横比缩放来得到的。盒子的深度为零。
            Vector3 halfExtends;
            halfExtends.y = m_regularCamera.nearClipPlane *
                            Mathf.Tan(0.5f * Mathf.Deg2Rad * m_regularCamera.fieldOfView);
            halfExtends.x = halfExtends.y * m_regularCamera.aspect;
            halfExtends.z = 0f;

            return halfExtends;
        }
    }

    #endregion

    #region 字段

    #region 相机碰撞

    private Camera m_regularCamera;

    [SerializeField]
    private LayerMask m_obstructionMask = -1;

    #endregion

    #region 距离跟随

    /// <summary>
    ///  跟随的目标
    /// </summary>
    [SerializeField]
    private Transform m_focus = default;

    /// <summary>
    ///  跟随保持的距离
    /// </summary>
    [SerializeField]
    [Range(1f, 20f)]
    private float m_distance = 5f;

    /// <summary>
    ///  记录跟随点
    /// </summary>
    private Vector3 m_focusPoint;

    /// <summary>
    ///  记录前一帧的跟随点
    /// </summary>
    private Vector3 m_previousFocusPoint;

    /// <summary>
    ///  宽松的跟随半径，防止轻微移动导致相机一起移动 
    /// </summary>
    [SerializeField]
    [Min(0f)]
    private float m_focusRadius = 1f;

    /// <summary>
    /// 居中目标的变化速度, 0 表示不居中， 1 表示立即居中  
    /// </summary>
    [SerializeField]
    [Range(0f, 1f)]
    private float m_focusCentering = 0.5f;

    #endregion

    #region 旋转

    /// <summary>
    /// 表示相机方向 
    /// </summary>
    private Vector2 m_orbitAngles = new Vector2(45f, 0f);

    /// <summary>
    ///  旋转速度
    /// </summary>
    [SerializeField]
    [Range(1f, 360f)]
    private float m_rotationSpeed = 90f;

    /// <summary>
    ///  最小垂直角度
    /// </summary>
    [SerializeField]
    [Range(-89f, 89f)]
    private float m_minVerticalAngle = -30f;

    /// <summary>
    ///  最大垂直角度
    /// </summary>
    [SerializeField]
    [Range(-89f, 89f)]
    private float m_maxVerticalAngle = 60f;

    /// <summary>
    ///  对齐延迟
    /// </summary>
    [SerializeField]
    [Min(0f)]
    private float m_alignDelay = 5f;

    /// <summary>
    ///  手动旋转的时间
    /// </summary>
    private float m_lastManualRotationTime;

    /// <summary>
    ///  对齐平滑范围, 使旋转速度与当前角度和所需角度之间的差值成比例。我们将使其线性缩放至某个角度，在此角度下我们将以全速旋转。
    /// </summary>
    [SerializeField]
    [Range(0f, 90f)]
    private float m_alignSmoothRange = 45f;

    #endregion

    #region 重力修正相关字段

    /// <summary>
    /// 重力对物体的旋转校正，用于调整物体的旋转，使其与重力方向对齐。
    /// </summary>
    private Quaternion m_gravityAlignment = Quaternion.identity;

    /// <summary>
    /// 轨道旋转四元数
    /// </summary>
    private Quaternion m_orbitRotation;

    /// <summary>
    /// 向上对齐速度，限制相机如何快速调整其向上矢量，以每秒度数为单位表示
    /// </summary>
    [SerializeField]
    [Min(0f)]
    float upAlignmentSpeed = 360f;

    #endregion

    #endregion
}