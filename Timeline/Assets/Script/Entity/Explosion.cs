using UnityEngine;

public class Explosion : WarEntity
{
    #region Unity 生命周期

    private void Awake()
    {
        m_meshRenderer = GetComponent<MeshRenderer>();

        Debug.Assert(m_meshRenderer != null, "Explosion without renderer!");
    }

    #endregion

    #region 方法

    /// <summary>
    /// 初始化爆炸效果, 使其在指定位置产生爆炸效果。
    /// </summary>
    /// <param name="position">爆炸发生的位置。</param>
    /// <param name="blastRadius">爆炸的半径大小。</param>
    /// <param name="damage">爆炸对敌人造成的伤害值。</param>
    public void Initialize(Vector3 position, float blastRadius, float damage = 0f)
    {
        if (damage > 0)
        {
            // 1. 获取爆炸范围内的所有敌人并造成伤害
            TargetPoint.FillBuffer(position, blastRadius);
            for (int i = 0; i < TargetPoint.BufferedCount; i++)
            {
                TargetPoint.GetBuffered(i).Enemy.ApplyDamage(damage);
            }
        }

        // 2. 初始化各个属性
        transform.localPosition = position;
        transform.localScale = Vector3.one * (2f * blastRadius);
        m_scale = 2f * blastRadius;
    }

    public override bool GameUpdate()
    {
        // 1. 更新爆炸的存在时间
        m_age += Time.deltaTime;
        if (m_age >= m_duration)
        {
            // 2. 爆炸效果消失回收
            OriginFactory.Reclaim(this);
            return false;
        }

        s_propertyBlock ??= new MaterialPropertyBlock();

        // 3. 更新爆炸效果的不透明度和缩放
        float t = m_age / m_duration;
        Color c = Color.clear;
        c.a = m_opacityCurve.Evaluate(t);
        s_propertyBlock.SetColor(ColorPropertyID, c);
        m_meshRenderer.SetPropertyBlock(s_propertyBlock);
        transform.localScale = Vector3.one * (m_scale * m_scaleCurve.Evaluate(t));

        return true;
    }

    #endregion

    #region 字段

    /// <summary>
    /// 爆炸持续时间，单位为秒。
    /// </summary>
    [SerializeField]
    [Range(0f, 1f)]
    private float m_duration = 0.5f;

    /// <summary>
    /// 表示爆炸经过的时间。
    /// </summary>
    /// <remarks>
    /// 此属性用于跟踪爆炸从发生到消失的生命周期，当其值达到 <see cref="m_duration"/> 时，爆炸实体将被回收。
    /// </remarks>
    private float m_age;

    /// <summary>
    /// 控制爆炸的不透明度随时间变化的动画曲线。
    /// </summary>
    [SerializeField]
    private AnimationCurve m_opacityCurve = default;

    /// <summary>
    /// 爆炸效果随时间变化的缩放曲线。
    /// </summary>
    [SerializeField]
    private AnimationCurve m_scaleCurve = default;

    /// <summary>
    /// 爆炸的缩放尺寸，用于控制爆炸效果的视觉大小。
    /// </summary>
    private float m_scale;

    /// <summary>
    /// 网格渲染组件，用于修改爆炸效果的显示颜色。
    /// </summary>
    private MeshRenderer m_meshRenderer;

    /// <summary>
    /// 颜色属性ID，由Shader.PropertyToID转换得到，用于访问特定着色器中的_Color属性。
    /// </summary>
    private static readonly int ColorPropertyID = Shader.PropertyToID("_Color");

    /// <summary>
    /// 存储材质属性块，用于统一修改场景中多个物体的渲染属性。
    /// </summary>
    private static MaterialPropertyBlock s_propertyBlock;

    #endregion
}