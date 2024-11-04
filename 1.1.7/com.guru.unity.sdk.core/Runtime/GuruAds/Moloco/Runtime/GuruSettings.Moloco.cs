using System;
using UnityEngine;

namespace Guru
{
    public partial class GuruSettings
    {
        [Header("[ Moloco ] 测试广告配置")] [Tooltip("此配置只在测试阶段验证广告时使用")]
        public MolocoSetting MolocoSetting;
    }

    /// <summary>
    /// Moloco 广告配置
    /// </summary>
    [Serializable]
    public class MolocoSetting
    {
        [SerializeField] public bool Enable;
        [SerializeField] private MolocoPlatformSetting Android;
        [SerializeField] private MolocoPlatformSetting iOS;

        /// <summary>
        /// 获取AppID
        /// </summary>
        /// <returns></returns>
        public MolocoPlatformSetting GetPlatform()
        {
#if UNITY_IOS
			return iOS;
#else
            return Android;
#endif
        }

        public string BannerTestUnitID => GetPlatform().bannerTestUnitID;
        public string InterTestUnitID => GetPlatform().interTestUnitID;
        public string RewardTestUnitID => GetPlatform().rewardTestUnitID;



        /// <summary>
        /// Moloco 平台专属配置
        /// </summary>
        [Serializable]
        public class MolocoPlatformSetting
        {
            // public string name;		// 平台名称
            public string bannerTestUnitID; // Banner ID
            public string interTestUnitID; // Inter ID
            public string rewardTestUnitID; // Reward ID
        }
    }
}