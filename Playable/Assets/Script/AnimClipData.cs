using System;
using UnityEngine;

/// <summary>
/// 单个动画数据
/// </summary>
[Serializable]
public class AnimClipData
{
    #region 字段

    /// <summary>
    ///  动画名称
    /// </summary>
    public string m_animName;

    /// <summary>
    /// 是否循环 
    /// </summary>
    public bool m_loop;

    /// <summary>
    /// AnimationClip引用 
    /// </summary>
    public AnimationClip m_clip;

    #endregion
}