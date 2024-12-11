using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZhaoHuanKe
{
    public class Clock : MonoBehaviour
    {
        /// <summary>
        ///  时针每小时转动的角度
        /// </summary>
        const int HoursToDegrees = -30;

        /// <summary>
        ///  分针每分钟转动的角度
        /// </summary>
        const int MinutesToDegrees = -6;

        /// <summary>
        ///  秒针每秒转动的角度
        /// </summary>
        const int SecondsToDegrees = -6;

        /// <summary>
        ///  时针的旋转中心
        /// </summary>
        [SerializeField] Transform m_hoursPivot;

        /// <summary>
        ///  分针的旋转中心
        /// </summary>
        [SerializeField] Transform m_minutesPivot;

        /// <summary>
        ///  秒针的旋转中心
        /// </summary>
        [SerializeField] Transform m_secondsPivot;

        private void Awake()
        {
            // var time = DateTime.Now;
            // m_hoursPivot.localRotation =
            //     Quaternion.Euler(0f, 0f, HoursToDegrees * time.Hour);
            // m_minutesPivot.localRotation =
            //     Quaternion.Euler(0f, 0f, MinutesToDegrees * time.Minute);
            // m_secondsPivot.localRotation =
            //     Quaternion.Euler(0f, 0f, SecondsToDegrees * time.Second);
        }

        private void Update()
        {
            var time = DateTime.Now.TimeOfDay;
            // 根据当前时间，设置各个指针的旋转角度，绕Z轴旋转
            m_hoursPivot.localRotation =
                Quaternion.Euler(0f, 0f, HoursToDegrees * (float)time.TotalHours);
            m_minutesPivot.localRotation =
                Quaternion.Euler(0f, 0f, MinutesToDegrees * (float)time.TotalMinutes);
            m_secondsPivot.localRotation =
                Quaternion.Euler(0f, 0f, SecondsToDegrees * (float)time.TotalSeconds);
        }
    }
}