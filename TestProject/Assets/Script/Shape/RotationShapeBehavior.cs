using UnityEngine;

public sealed class RotationShapeBehavior
    : ShapeBehavior
{
    #region Unity 生命周期

    #endregion

    #region 方法

    public override void GameUpdate(Shape shape)
    {
        shape.transform.Rotate(AngularVelocity * Time.deltaTime);
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(AngularVelocity);
    }

    public override void Load(GameDataReader reader)
    {
        AngularVelocity = reader.ReadVector3();
    }

    public override void Recycle()
    {
        ShapeBehaviorPool<RotationShapeBehavior>.Reclaim(this);
    }

    #endregion

    #region 事件

    #endregion

    #region 属性

    public Vector3 AngularVelocity { get; set; }

    public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Rotation;

    #endregion

    #region 字段

    #endregion
}