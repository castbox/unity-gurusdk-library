//================================================
// Company Name: Castbox
// Project: Ball Sort Puzzle
// Author: HYZ
// CreateTime: 2023-04-12 21:06:02
// Version: 1.0.0
// Desc: 
//================================================

using UnityEngine.Serialization;

namespace Guru
{
    using System;
    using UnityEngine;
    
    public partial class GuruSettings
    {
        [Header("Pubmatic 广告配置")]
        public PubmaticSetting PubmaticSetting;
    }


    /// <summary>
    /// Amazon广告配置
    /// </summary>
    [Serializable]
    public class PubmaticSetting
    {
        [SerializeField] public bool Enable;
        [SerializeField] private PubmaticPlatformSetting Android;
        [SerializeField] private PubmaticPlatformSetting iOS;
	
        /// <summary>
        /// 获取AppID
        /// </summary>
        /// <returns></returns>
        public PubmaticPlatformSetting GetPlatform()
        {
#if UNITY_IOS
			return iOS; 
#else
            return Android;
#endif 
        }
        
        public string BannerUnitID => GetPlatform().bannerUnitID;
        public string InterUnitID => GetPlatform().interUnitID;
        public string RewardUnitID => GetPlatform().rewardUnitID;
        public string StoreUrl => GetPlatform().storeUrl;

    }
    
    /// <summary>
    /// Amazon平台专属配置
    /// </summary>
    [Serializable]
    public class PubmaticPlatformSetting
    {
        // public string name;		// 平台名称
        public string storeUrl; // 平台商店地址
        public string bannerUnitID;	// Banner ID
        public string interUnitID;	// Inter ID
        public string rewardUnitID;	// Reward ID
    }
    
}