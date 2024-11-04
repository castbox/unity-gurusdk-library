/**********************************************
 * Pubmatic 广告渠道
 **********************************************/
namespace Guru
{
    using OpenWrapSDK;
    using System;
    using OpenWrapSDK.Mediation.AppLovinMAX;
    using UnityEngine;
    
    public class AdChannelPubMatic: IAdChannel
    {
        #region 属性定义
        
        public static readonly string ChanelName = "PubMatic";
        public string Name => ChanelName;
        
        public Action<string> OnRequestOver { get; set; }


        // ---------------  获取各种SlotID --------------------
        private static string PMBannerUnitID => GuruSettings.Instance.PubmaticSetting.BannerUnitID;
        private static string PMInterUnitID => GuruSettings.Instance.PubmaticSetting.InterUnitID;
        private static string PMRewardUnitID => GuruSettings.Instance.PubmaticSetting.RewardUnitID;
        private static string PMStoreUrl => GuruSettings.Instance.PubmaticSetting.StoreUrl;

        public static readonly int BidRequestTimeout = 5; // 请求超时 (秒)

        /// <summary>
        /// 当前平台是否可用
        /// </summary>
        public bool IsEnabled
        {
            get
            {
#if UNITY_EDITOR
                return false; 
#endif
                // return GuruSettings.Instance.PubmaticSetting.Enable;
                return true; // 常驻开启
            }
        }

        #endregion
        
        #region 初始化

        /*
         * You must set the App Store/Google Play Store storeURL of your app
         * before it can request an ad using OpenWrap SDK.
         * The storeURL is the URL where users can download your app from the App Store/Google Play Store.
         */
        public void Initialize(bool isDebug = false)
        {
#if UNITY_EDITOR
            Debug.Log($"<color=orange>=== PubMatic will not init on Editor ===</color>");
#endif
            if (!IsEnabled)
            {
                Debug.Log($"[Ads] --- PubMatic is not enabled");
                return;
            }
            
            if (string.IsNullOrEmpty(PMStoreUrl))
            {
                Debug.Log($"[Ads] --- PubMatic with empty store url. skip initialize...");
                return;
            }

            var appInfo = new POBApplicationInfo();
            appInfo.StoreURL = new Uri(PMStoreUrl);
            POBOpenWrapSDK.SetApplicationInfo(appInfo);
        }
     
        #endregion
        
        #region 基础参数设置

        /// <summary>
        /// 设置Banner参数
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private static void SetBannerParams(string adUnitId, string key, object value)
        {
#if UNITY_IOS
            MaxSdk.SetBannerLocalExtraParameter(adUnitId, 
                key, 
                POBMAXUtil.GetIntPtr(adUnitId, value));
#elif UNITY_ANDROID
            MaxSdk.SetBannerLocalExtraParameter(adUnitId, 
                key, 
                POBMAXUtil.GetAndroidJavaObject(value));
#endif
        }
        
        /// <summary>
        /// 设置IV参数
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private static void SetIVParams(string adUnitId, string key, object value)
        {
#if UNITY_IOS
            MaxSdk.SetInterstitialLocalExtraParameter(adUnitId, 
                key, 
                POBMAXUtil.GetIntPtr(adUnitId, value));
#elif UNITY_ANDROID
            MaxSdk.SetInterstitialLocalExtraParameter(adUnitId, 
                key, 
                POBMAXUtil.GetAndroidJavaObject(value));
#endif
        }
        
        /// <summary>
        /// 设置RV参数
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private static void SetRVParams(string adUnitId, string key, object value)
        {
#if UNITY_IOS
            MaxSdk.SetRewardedAdLocalExtraParameter(adUnitId, 
                key, 
                POBMAXUtil.GetIntPtr(adUnitId, value));
#elif UNITY_ANDROID
            MaxSdk.SetRewardedAdLocalExtraParameter(adUnitId, 
                key, 
                POBMAXUtil.GetAndroidJavaObject(value));
#endif
        }

        
        //---------------- 设置广告超时 -------------------------

        private void SetBannerTimeout(string adUnitId, int timeout = 5)
        {
            SetBannerParams(adUnitId, POBMAXConstants.NetworkTimeoutKey, timeout);
        }
        private void SetIVTimeout(string adUnitId, int timeout = 5)
        {
            SetIVParams(adUnitId, POBMAXConstants.NetworkTimeoutKey, timeout);
        }
        
        private void SetRVTimeout(string adUnitId, int timeout = 5)
        {
            SetRVParams(adUnitId, POBMAXConstants.NetworkTimeoutKey, timeout);
        }
        
        //------------------ 打开测试广告 ----------------------------

        private void EnableBannerTestAds(string adUnitId)
        {
            SetBannerParams(adUnitId, POBMAXConstants.EnableTestModeKey, true);
        }
        private void EnableIVTestAds(string adUnitId)
        {
            SetIVParams(adUnitId, POBMAXConstants.EnableTestModeKey, true);
        }
        private void EnableRVTestAds(string adUnitId)
        {
            SetRVParams(adUnitId, POBMAXConstants.EnableTestModeKey, true);
        }

        //------------------- 打开调试模式 ---------------------------
        private static void EnableBannerDebugMode(string adUnitId)
        {
            SetBannerParams(adUnitId, POBMAXConstants.EnableDebugModeKey, true);
        }
        private static void EnableIVDebugMode(string adUnitId)
        {
            SetIVParams(adUnitId, POBMAXConstants.EnableDebugModeKey, true);
        }
        private static void EnableRVDebugMode(string adUnitId)
        {
            SetRVParams(adUnitId, POBMAXConstants.EnableDebugModeKey, true);
        }



        #endregion
        
        #region Banner

        public void LoadBannerAD()
        {
        }


        private static bool _isLoadingBanner = false;
        /// <summary>
        /// 请求测试广告Ba
        /// </summary>
        public static void RequestDebugBanner()
        {
            if (_isLoadingBanner) return;
            _isLoadingBanner = true;
            EnableBannerDebugMode(PMBannerUnitID);
            AddBannerCallBacks();
            MaxSdk.CreateBanner(PMBannerUnitID, MaxSdkBase.BannerPosition.BottomCenter);
            ShowToast($"Load Banner: {PMBannerUnitID}");
        }
        private static void AddBannerCallBacks()
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnDebugBannerLoadSuccess;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnDebugBannerLoadFailed;
        }
        
        private static void RemoveBannerCallBacks()
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent -= OnDebugBannerLoadSuccess;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent -= OnDebugBannerLoadFailed;
        }

        private static void OnDebugBannerLoadSuccess(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (adUnitId == PMBannerUnitID)
            {
                RemoveBannerCallBacks();
                Debug.Log($"[PM] Load Banner success => Revenue: {adInfo.Revenue}");
                MaxSdk.ShowBanner(adUnitId);
                _isLoadingBanner = false;
                
                ShowToast($"Banner Loaded: {PMBannerUnitID}");
            }
        }

        private static void OnDebugBannerLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            if (adUnitId == PMBannerUnitID)
            {
                RemoveBannerCallBacks();
                Debug.Log($"[PM] Load Banner fail => Waterfall:{errorInfo.WaterfallInfo.Name}");
                _isLoadingBanner = false;
                
                ShowToast($"Banner Load fail: {PMBannerUnitID}");
            }
        }

        #endregion

        #region Interstitial

        public void LoadInterstitialAD()
        {
        }
        
        
        private static bool _isLoadingIV = false;
        /// <summary>
        /// 请求测试广告IV
        /// </summary>
        public static void LoadDebugIV()
        {
            if (_isLoadingIV) return;
            _isLoadingIV = true;
            EnableIVDebugMode(PMInterUnitID);
            AddIvCallBacks();
            MaxSdk.LoadInterstitial(PMInterUnitID);
            
            ShowToast($"Load Interstitial: {PMInterUnitID}");
        }
        
        private static void AddIvCallBacks()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnDebugIVLoadSuccess;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnDebugIVLoadFailed;
        }
        
        private static void RemoveIvCallBacks()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnDebugIVLoadSuccess;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnDebugIVLoadFailed;
        }

        private static void OnDebugIVLoadSuccess(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (adUnitId == PMInterUnitID)
            {
                RemoveIvCallBacks();
                _isLoadingIV = false;
                Debug.Log($"[PM] Load IV success => Revenue: {adInfo.Revenue}");
                string placement = "pm_test_iv";
                MaxSdk.ShowInterstitial(adUnitId, placement);
                ShowToast($"Load IV Success: {PMInterUnitID}");
            }
        }

        private static void OnDebugIVLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            if (adUnitId == PMInterUnitID)
            {
                RemoveIvCallBacks();
                _isLoadingIV = false;
                Debug.Log($"[PM] Load IV fail => Waterfall:{errorInfo.WaterfallInfo.Name}");
                ShowToast($"Load IV Fail: {PMInterUnitID}");
            }
        }
        
        #endregion

        #region Reward

        public void LoadRewardAD()
        {
        }


        private static bool _isLoadingRV = false;
        /// <summary>
        /// 请求测试广告RV
        /// </summary>
        public static void RequestDebugRV()
        {
            if (_isLoadingRV) return;
            _isLoadingRV = true;
            EnableIVDebugMode(PMRewardUnitID);
            AddRvCallBacks();
            MaxSdk.LoadRewardedAd(PMRewardUnitID);
            
            ShowToast($"Load RV: {PMInterUnitID}");
        }

        private static void AddRvCallBacks()
        {
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnDebugRVLoadSuccess;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnDebugRVLoadFailed;
        }
        
        private static void RemoveRvCallBacks()
        {
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnDebugRVLoadSuccess;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnDebugRVLoadFailed;
        }
        
        private static void OnDebugRVLoadSuccess(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (adUnitId == PMRewardUnitID)
            {
                RemoveRvCallBacks();
                _isLoadingRV = false;
                Debug.Log($"[PM] Load RV success => Revenue: {adInfo.Revenue}");
                string placement = "pm_test_rv";
                MaxSdk.ShowRewardedAd(adUnitId, placement);
                ShowToast($"Load RV Success: {PMRewardUnitID}");
            }
        }

        private static void OnDebugRVLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            if (adUnitId == PMRewardUnitID)
            {
                RemoveRvCallBacks();
                _isLoadingRV = false;
                Debug.Log($"[PM] Load RV fail => Waterfall:{errorInfo.WaterfallInfo.Name}");
                ShowToast($"Load RV Fail: {PMRewardUnitID}");
            }
        }
        #endregion
        
        #region Debug
        
        private static void ShowToast(string msg)
        {
#if UNITY_ANDROID   
            DeviceUtil.ShowToast(msg);
#endif
        }
        
        #endregion
     
    }
}