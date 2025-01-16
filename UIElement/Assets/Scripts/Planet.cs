using UnityEngine;
using UnityEngine.Serialization;

public class Planet : ScriptableObject
{
    /// <summary>
    /// 星球sprite
    /// </summary>
    public Sprite m_sprite;

    /// <summary>
    /// 星球大小
    /// </summary>
    public float m_scale;

    /// <summary>
    /// 速度
    /// </summary>
    public float m_speed;

    /// <summary>
    /// 距离
    /// </summary>
    public float m_distance;
}