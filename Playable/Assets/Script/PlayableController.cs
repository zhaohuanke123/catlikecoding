using System;
using UnityEngine;

public class PlayableController : MonoBehaviour
{
    #region Unity 生命周期

    private void Awake()
    {
        var animator = GetComponentInChildren<Animator>();
        m_playerAnimator.Init(animator);
    }

    private void Update()
    {
        for (int i = 0; i < m_animData.m_clipDatas.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                m_playerAnimator.Play(m_animData.m_clipDatas[i]);
            }
        }

        m_playerAnimator.GameUpdate();
    }

    private void OnDestroy()
    {
        m_playerAnimator.Destroy();
    }

    #endregion

    #region 方法

    #endregion

    #region 事件

    #endregion

    #region 属性

    #endregion

    #region 字段

    /// <summary>
    /// 玩家动画控制器
    /// </summary>
    private PlayerAnimator m_playerAnimator;

    /// <summary>
    /// 动画数据集合
    /// </summary>
    [SerializeField]
    private PlayerAnimData m_animData;

    #endregion
}