using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 按需创建形状，而无需其客户端知道这些形状是如何创建
/// </summary>
[CreateAssetMenu]
public class ShapeFactory : ScriptableObject
{
    #region 方法

    /// <summary>
    /// 根据给定的形状ID和材质ID创建一个新的形状实例，并为其设置相应的材质。
    /// </summary>
    /// <param name="shapeId">需要创建的形状ID。</param>
    /// <param name="materialId">需要为形状设置的材质ID。</param>
    /// <returns>一个创建的Shape实例，已设置材质和形状ID。</returns>
    public Shape Get(int shapeId = 0, int materialId = 0)
    {
        // 1. 根据形状ID实例化一个新的Shaoe实例
        Shape instance = Instantiate(m_prefabs[shapeId]);
        
        // 2. 为该实例设置材质
        instance.ShapeId = shapeId;
        instance.SetMaterial(m_materials[materialId], materialId);
        return instance;
    }


    /// <summary>
    /// 随机生成一个形状实例，并为其随机选择一个材质。
    /// </summary>
    /// <returns>一个随机生成的Shape实例，已设置随机材质和形状ID。</returns>
    public Shape GetRandom()
    {
        return Get(Random.Range(0, m_prefabs.Length), Random.Range(0, m_materials.Length));
    }

    #endregion

    #region 字段

    /// <summary>
    /// 存储所有可用的形状预设。
    /// </summary>
    [SerializeField]
    private Shape[] m_prefabs;

    /// <summary>
    /// 存储所有可用的材质。
    /// </summary>
    [SerializeField]
    private Material[] m_materials;

    #endregion
}