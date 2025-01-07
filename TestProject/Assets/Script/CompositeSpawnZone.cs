using UnityEngine;

namespace Script
{
    public class CompositeSpawnZone : SpawnZone
    {
        #region 属性

        public override Vector3 SpawnPoint
        {
            get
            {
                // 从m_spawnZones中随机选择一个SpawnZone，然后返回它的SpawnPoint
                int index = Random.Range(0, m_spawnZones.Length);
                return m_spawnZones[index].SpawnPoint;
            }
        }

        #endregion

        #region 字段

        /// <summary>
        ///  生成区域数组
        /// </summary>
        [SerializeField]
        private SpawnZone[] m_spawnZones;

        #endregion
    }
}