using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Star System", menuName = "Star System")]
public class StarSystem : ScriptableObject
{
    #region 方法

    private void OnEnable()
    {
        if (m_planets == null)
        {
            m_planets = new List<Planet>();
        }
    }

    #endregion

    #region 字段

    /// <summary>
    /// 星系Sprite
    /// </summary>
    public Sprite m_sprite;

    /// <summary>
    /// 星球列表
    /// </summary>
    public List<Planet> m_planets;

    /// <summary>
    /// 星系大小
    /// </summary>
    public float m_scale;

    #endregion
}