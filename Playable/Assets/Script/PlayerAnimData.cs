using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
///  玩家动画数据集合
/// </summary>
[CreateAssetMenu]
public class PlayerAnimData : ScriptableObject
{
    public AnimClipData[] m_clipDatas;
}