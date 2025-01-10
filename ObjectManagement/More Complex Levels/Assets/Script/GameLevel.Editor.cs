#if UNITY_EDITOR
using System;
using UnityEngine;

/// <summary>
/// 游戏关卡类，负责管理关卡中的游戏对象实例。 编辑器下的功能
/// </summary>
public partial class GameLevel
{
    #region 方法

    /// <summary>
    /// 移除数组中缺失的Level对象实例，通常在编辑模式下用于清理未正确引用的游戏对象。
    /// 检查<see cref="levelObjects"/>数组中的null元素，并从数组中移除它们，同时紧凑数组以避免空洞。
    /// 注意：此操作不应在Play模式下执行。
    /// </summary>
    public void RemoveMissingLevelObjects()
    {
        // 1. 检查是否在运行模式下
        if (Application.isPlaying)
        {
            Debug.LogError("Do not invoke in play mode!");
            return;
        }

        // 2. 移除数组中的null元素
        int holes = 0;
        for (int i = 0; i < levelObjects.Length - holes; i++)
        {
            if (levelObjects[i] == null)
            {
                holes += 1;
                Array.Copy(levelObjects, i + 1, levelObjects, i, levelObjects.Length - i - holes);
                i -= 1;
            }
        }

        // 3. 缩减数组
        Array.Resize(ref levelObjects, levelObjects.Length - holes);
    }

    /// <summary>
    /// 检查指定的游戏对象是否已存在于当前关卡中。
    /// </summary>
    /// <param name="o">需要检查的游戏对象实例。</param>
    /// <returns>如果游戏对象存在于当前关卡的levelObjects数组中，则返回true；否则返回false。</returns>
    public bool HasLevelObject(GameLevelObject o)
    {
        if (levelObjects != null)
        {
            for (int i = 0; i < levelObjects.Length; i++)
            {
                if (levelObjects[i] == o)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 向当前关卡注册一个级别的游戏对象。
    /// 此方法应仅在编辑模式下调用，以避免在播放模式下对关卡对象进行修改。
    /// 将提供的GameLevelObject实例添加到当前关卡的levelObjects数组中，确保跟踪关卡中所有相关的可持久化对象。
    /// </summary>
    /// <param name="o">要注册到当前关卡的GameLevelObject实例。</param>
    public void RegisterLevelObject(GameLevelObject o)
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Do not invoke in play mode!");
            return;
        }

        if (HasLevelObject(o))
        {
            return;
        }

        if (levelObjects == null)
        {
            levelObjects = new GameLevelObject[] { o };
        }
        else
        {
            Array.Resize(ref levelObjects, levelObjects.Length + 1);
            levelObjects[levelObjects.Length - 1] = o;
        }
    }

    #endregion

    #region 事件

    #endregion

    #region 属性

    /// <summary>
    /// 指示当前关卡的编辑器序列化数组中是否缺少Level对象。
    /// </summary>
    /// <value>
    /// 如果关卡中的任何Level对象为null，则返回true；否则返回false。
    /// </value>
    /// <remarks>
    /// 此属性遍历<see cref="levelObjects"/>数组，检查其中是否有未实例化的对象。
    /// </remarks>
    public bool HasMissingLevelObjects
    {
        get
        {
            if (levelObjects != null)
            {
                for (int i = 0; i < levelObjects.Length; i++)
                {
                    if (levelObjects[i] == null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    #endregion
}
#endif