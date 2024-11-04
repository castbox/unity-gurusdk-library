
namespace Guru
{
    using UnityEngine;
    using System;

    /// <summary>
    /// MAX 自有渠道初始化
    /// </summary>
    public class AdChanelMax: IAdChannel
    {
        #region 属性

        public static readonly string ChanelName = "AppLovinMax";
        public string Name => ChanelName;

        public bool IsEnabled => true;
        public Color BannerBackColor { get; set; }
        #endregion
        
        #region 广告位
        
        // ---------------- Max 广告位ID -------------------- 
        internal string MaxBADSSlotID => GuruSettings.Instance.ADSetting.GetBannerID();
        internal string MaxIADSSlotID => GuruSettings.Instance.ADSetting.GetInterstitialID();
        internal string MaxRADSSlotID => GuruSettings.Instance.ADSetting.GetRewardedVideoID();
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// MAX 渠道初始化, 启动服务
        /// </summary>
        public void Initialize(bool isDebug = false)
        {
            MaxSdk.SetSdkKey(GuruSettings.Instance.ADSetting.SDK_KEY);
            MaxSdk.SetUserId(IPMConfig.IPM_UID);  // 上报用户ID
            MaxSdk.SetVerboseLogging(isDebug); // 设置调试数据
            MaxSdk.InitializeSdk();
        }

        #endregion
        
        #region Banner

        private Color _backColor = Color.clear;
        public void LoadBannerAD()
        {
            // Banners are automatically sized to 320x50 on phones and 728x90 on tablets
            // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments
            MaxSdk.CreateBanner(MaxBADSSlotID, MaxSdkBase.BannerPosition.BottomCenter);
            MaxSdk.SetBannerExtraParameter(MaxBADSSlotID, "adaptive_banner", "false");
            // Set background or background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(MaxBADSSlotID, _backColor);
            Analytics.TrackAdBadsLoad(AdParams.Build(adUnitId:MaxBADSSlotID).ToAdLoadData());
        }
        
        public void SetBannerBackColor(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return;
            _backColor = GuruSDKUtils.HexToColor(hex);
        }
        
        #endregion
        
        #region Interstitial
        
        public bool IsInterstitialRequestOver => true;
        public void LoadInterstitialAD()
        {
            Analytics.TrackAdIadsLoad(AdParams.Build(adUnitId:MaxIADSSlotID).ToAdLoadData()); // 上报打点
            MaxSdk.LoadInterstitial(MaxIADSSlotID);
        }

        #endregion
        
        #region Reward

        public bool IsRewardRequestOver => true;
        public void LoadRewardAD()
        {
            Analytics.TrackAdRadsLoad(AdParams.Build(adUnitId:MaxRADSSlotID).ToAdLoadData()); // 上报打点
            MaxSdk.LoadRewardedAd(MaxRADSSlotID);
        }

        #endregion
       
    }
}