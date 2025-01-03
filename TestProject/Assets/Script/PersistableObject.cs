using UnityEngine;


/// <summary>
/// 代表一个可持久化的游戏对象，它能够保存和加载自己的变换（位置、旋转和缩放）。
/// 此类作为基类，可以用于那些需要持久化存储变换信息的对象。子类可以重写Save和Load方法以处理额外的数据。
/// </summary>
[DisallowMultipleComponent]
public class PersistableObject : MonoBehaviour
{
    #region 方法

    /// <summary>
    /// 将游戏对象的变换数据（位置、旋转、缩放）保存到指定的GameDataWriter中。
    /// 可被子类重写以保存额外的数据。
    /// </summary>
    /// <param name="writer">用于写入数据的GameDataWriter。</param>
    public virtual void Save(GameDataWriter writer)
    {
        writer.Write(transform.localPosition);
        writer.Write(transform.localRotation);
        writer.Write(transform.localScale);
    }

    /// <summary>
    /// 从指定的GameDataReader中加载游戏对象的变换数据（位置、旋转、缩放）。
    /// 可被子类重写以加载额外的数据。
    /// </summary>
    /// <param name="reader">用于读取数据的GameDataReader。</param>
    public virtual void Load(GameDataReader reader)
    {
        transform.localPosition = reader.ReadVector3();
        transform.localRotation = reader.ReadQuaternion();
        transform.localScale = reader.ReadVector3();
    }

    #endregion
}