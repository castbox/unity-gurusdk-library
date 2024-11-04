namespace Guru
{
    using System;
    using UnityEngine;

    public partial class GuruSettings
    {
        [Header("Amazon 广告配置")]
        public AmazonSetting AmazonSetting;
    }
    
    
    /// <summary>
    /// Amazon广告配置
    /// </summary>
    [Serializable]
    public class AmazonSetting
    {

        [SerializeField] public bool Enable;
        [SerializeField] private AmazonPlatformSetting Android;
        [SerializeField] private AmazonPlatformSetting iOS;
		
        /// <summary>
        /// 获取AppID
        /// </summary>
        /// <returns></returns>
        public AmazonPlatformSetting GetPlatform()
        {
#if UNITY_IOS
			return iOS; 
#else
            return Android;
#endif 
        }

        public string AppID => GetPlatform().appID;
        public string BannerSlotID => GetPlatform().bannerSlotID;
        public string InterSlotID => GetPlatform().interSlotID;
        public string RewardSlotID => GetPlatform().rewardSlotID;
            
    }
        
        
    /// <summary>
    /// Amazon平台专属配置
    /// </summary>
    [Serializable]
    public class AmazonPlatformSetting
    {
        // public string name;		// 平台名称
        public string appID;		// AppID
        public string bannerSlotID;	// Banner ID
        public string interSlotID;	// Inter ID
        public string rewardSlotID;	// Reward ID
    }
}