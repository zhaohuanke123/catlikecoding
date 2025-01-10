using UnityEngine;

public sealed class MovementShapeBehavior : ShapeBehavior
{
    #region 方法

    public override bool GameUpdate(Shape shape)
    {
        shape.transform.localPosition += Velocity * Time.deltaTime;
        return true;
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(Velocity);
    }

    public override void Load(GameDataReader reader)
    {
        Velocity = reader.ReadVector3();
    }

    public override void Recycle()
    {
        ShapeBehaviorPool<MovementShapeBehavior>.Reclaim(this);
    }

    #endregion

    #region 属性

    /// <summary>
    ///  移动速度
    /// </summary>
    public Vector3 Velocity { get; set; }

    public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Movement;

    #endregion
}