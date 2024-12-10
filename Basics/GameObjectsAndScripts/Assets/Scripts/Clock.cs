using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZhaoHuanKe
{
    public class Clock : MonoBehaviour
    {
        const int HoursToDegrees = -30;
        const int MinutesToDegrees = -6;
        const int SecondsToDegrees = -6;
        [SerializeField] Transform m_hoursPivot;
        [SerializeField] Transform m_minutesPivot;

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
            m_hoursPivot.localRotation =
                Quaternion.Euler(0f, 0f, HoursToDegrees * (float)time.TotalHours);
            m_minutesPivot.localRotation =
                Quaternion.Euler(0f, 0f, MinutesToDegrees * (float)time.TotalMinutes);
            m_secondsPivot.localRotation =
                Quaternion.Euler(0f, 0f, SecondsToDegrees * (float)time.TotalSeconds);
        }
    }
}