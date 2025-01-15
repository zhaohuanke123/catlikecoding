using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
///  玩家动画数据集合
/// </summary>
[CreateAssetMenu]
public class PlayerAnimData : ScriptableObject
{
    #region 属性

    /// <summary>
    /// 索引器，通过索引获取动画片段数据
    /// </summary>
    /// <param name="index">索引值</param>
    /// <returns>指定索引的AnimClipData对象</returns>
    public AnimClipData this[int index] => m_clipDatas[index];

    #endregion

    #region 字段

    /// <summary>
    /// 存储玩家动画数据的数组，每个元素代表一个动画片段的数据结构 <see cref="AnimClipData"/>。
    /// 这些数据用于驱动玩家角色的动画逻辑，如播放行走、跑步、跳跃等动画。
    /// </summary>
    public AnimClipData[] m_clipDatas;

    #endregion
}