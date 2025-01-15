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
        if (Input.GetKeyDown(KeyCode.A))
        {
            m_playerAnimator.Play(m_animData[0]);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            m_playerAnimator.Play(m_animData[1]);
        }

        m_playerAnimator.GameUpdate();
    }

    private void OnDestroy()
    {
        m_playerAnimator.Destroy();
    }

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