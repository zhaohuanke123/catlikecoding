using UnityEngine;


public class MovingSphere : MonoBehaviour
{
    #region Unity 生命周期

    void Update()
    {
        // 获取输入并限制到单位长度
        var playerInput = default(Vector2);
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        // Vector3 acceleration =
        //     new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        Vector3 desiredVelocity =
            new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        // if (velocity.x < desiredVelocity.x)
        // {
        //     // velocity.x += maxSpeedChange;
        //     velocity.x =
        //         Mathf.Min(velocity.x + maxSpeedChange, desiredVelocity.x);
        // }
        // else if (velocity.x > desiredVelocity.x)
        // {
        //     velocity.x =
        //         Mathf.Max(velocity.x - maxSpeedChange, desiredVelocity.x);
        // }

        velocity.x =
            Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z =
            Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        // velocity += acceleration * Time.deltaTime;
        Vector3 displacement = velocity * Time.deltaTime;

        transform.localPosition += displacement;
    }

    #endregion

    #region 方法

    #endregion

    #region 事件

    #endregion

    #region 属性

    #endregion

    #region 字段

    /// <summary>
    ///  最大速度
    /// </summary>
    [SerializeField]
    [Range(0f, 100f)]
    private float maxSpeed = 10f;

    /// <summary>
    /// 最大加速度
    /// </summary>
    [SerializeField, Range(0f, 100f)]
    private float maxAcceleration = 10f;

    /// <summary>
    ///  当前速度
    /// </summary>
    private Vector3 velocity;

    #endregion
}