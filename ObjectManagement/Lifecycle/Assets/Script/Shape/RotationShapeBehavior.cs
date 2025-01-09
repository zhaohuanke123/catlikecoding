using UnityEngine;

public sealed class RotationShapeBehavior : ShapeBehavior
{
    #region 方法

    public override bool GameUpdate(Shape shape)
    {
        shape.transform.Rotate(AngularVelocity * Time.deltaTime);
        return true;
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


    #region 属性

    /// <summary>
    ///  旋转速度
    /// </summary>
    public Vector3 AngularVelocity { get; set; }

    public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Rotation;

    #endregion
}