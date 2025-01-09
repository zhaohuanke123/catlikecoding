using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 形状工厂类，用于按需创建和管理形状实例。客户端无需了解这些形状是如何创建的，可以直接通过此工厂获取形状。
/// 支持形状实例池化，从而优化性能。
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
        Shape instance;
        // 1. 根据是否回收销毁的形状实例，选择不同的实例化方式 
        if (m_recycle)
        {
            // 1 检查对象池初始化 
            if (m_pools == null)
            {
                CreatePools();
            }

            List<Shape> pool = m_pools[shapeId];
            int lastIndex = pool.Count - 1;

            // 2. 如果池中有可复用的形状实例，则从池中取出
            if (lastIndex >= 0)
            {
                instance = pool[lastIndex];
                instance.gameObject.SetActive(true);
                pool.RemoveAt(lastIndex);
            }
            else
            {
                // 否则实例化一个新的形状
                instance = Instantiate(m_prefabs[shapeId]);
                instance.OriginFactory = this;
                instance.ShapeId = shapeId;
                SceneManager.MoveGameObjectToScene(instance.gameObject, m_poolScene);
            }
        }
        else
        {
            // 否则直接实例化新的形状
            instance = Instantiate(m_prefabs[shapeId]);
            instance.ShapeId = shapeId;
        }

        // 2. 为实例设置材质
        instance.SetMaterial(m_materials[materialId], materialId);

        Game.Instance.AddShape(instance);
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

    /// <summary>
    /// 将一个形状实例返回到池中，以便下次复用。如果没有启用回收机制，则销毁该实例。
    /// </summary>
    /// <param name="shapeToRecycle">需要回收的Shape实例。</param>
    public void Reclaim(Shape shapeToRecycle)
    {
        if (shapeToRecycle.OriginFactory != this)
        {
            Debug.LogError("Tried to reclaim shape with wrong factory.");
            return;
        }

        if (m_recycle)
        {
            // 1 检查对象池初始化
            if (m_pools == null)
            {
                CreatePools();
            }

            // 2. 将形状实例加入到对应的池中
            m_pools[shapeToRecycle.ShapeId].Add(shapeToRecycle);
            shapeToRecycle.gameObject.SetActive(false);
        }
        else
        {
            // 如果没有启用回收机制，直接销毁该实例
            Destroy(shapeToRecycle.gameObject);
        }
    }

    /// <summary>
    /// 初始化形状池。每个形状ID对应一个池，用于存储销毁的形状实例。
    /// 在编辑器模式下，如果已加载场景，尝试从场景中的根对象获取已销毁的形状实例并将其加入池中。
    /// 如果没有加载场景，则创建一个新的场景用于存放这些销毁的形状实例。
    /// </summary>
    private void CreatePools()
    {
        // 1. 初始化对象池，池的数量与形状预设数量相同，每个池用于存储不同形状ID对应的销毁实例。
        m_pools = new List<Shape>[m_prefabs.Length];
        for (int i = 0; i < m_pools.Length; i++)
        {
            m_pools[i] = new List<Shape>();
        }

        if (Application.isEditor)
        {
            m_poolScene = SceneManager.GetSceneByName(name);
            if (m_poolScene.isLoaded)
            {
                GameObject[] rootObjects = m_poolScene.GetRootGameObjects();

                // 遍历所有根对象，检查是否包含已销毁但未激活的Shape实例
                for (int i = 0; i < rootObjects.Length; i++)
                {
                    Shape pooledShape = rootObjects[i].GetComponent<Shape>();
                    if (pooledShape != null && !pooledShape.gameObject.activeSelf)
                    {
                        m_pools[pooledShape.ShapeId].Add(pooledShape);
                    }
                }

                return;
            }
        }

        // 2. 如果场景未加载或不在编辑器模式下，则创建一个新的场景用于存放销毁的形状实例
        m_poolScene = SceneManager.CreateScene(name);
    }

    #endregion

    #region 属性

    public int FactoryId
    {
        get => m_factoryId;
        set
        {
            if (m_factoryId == int.MinValue && value != int.MinValue)
            {
                m_factoryId = value;
            }
            else
            {
                Debug.Log("Not allowed to change factoryId.");
            }
        }
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

    private List<Shape>[] m_pools;

    /// <summary>
    ///  是否回收销毁的形状实例。
    /// </summary>
    [SerializeField]
    private bool m_recycle;

    /// <summary>
    /// 池场景来容纳所有可以回收的形状实例。工厂的所有形状都进入这个池，并且永远不应该从中移除。
    /// </summary>
    private Scene m_poolScene;

    /// <summary>
    ///  工厂ID
    /// </summary>
    [System.NonSerialized]
    private int m_factoryId = int.MinValue;

    #endregion
}