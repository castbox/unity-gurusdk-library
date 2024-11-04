/**********************************************
 * Amazon 广告渠道
 **********************************************/
namespace Guru
{
    using AmazonAds;
    using UnityEngine;
    using System;

    internal struct AdSize
    {
        public int width;
        public int height;
    } 

    /// <summary>
    /// 广告渠道: Amazon
    /// </summary>
    public class AdChanelAmazon: IAdChannel, IAsyncRequestChannel
    {
        #region 广告尺寸
        
        // Banner 尺寸参数
        private static AdSize BannerSize = new AdSize() { width = 320, height = 50 };
        // Video 尺寸参数
        private static AdSize VideoSize = new AdSize() { width = 320, height = 480 };
        
        #endregion
        
        #region SLOT IDS

        // ---------------  获取各种SlotID --------------------
        private string AmazonBannerSlotID => GuruSettings.Instance.AmazonSetting.BannerSlotID;
        private string AmazonInterVideoSlotID => GuruSettings.Instance.AmazonSetting.InterSlotID;
        private string AmazonRewardSlotID => GuruSettings.Instance.AmazonSetting.RewardSlotID;
        private string AmazonAppID => GuruSettings.Instance.AmazonSetting.AppID;
        
        // ---------------- Max 广告位ID -------------------- 
        private string MaxBannerSlotID => GuruSettings.Instance.ADSetting.GetBannerID();
        private string MaxIVSlotID => GuruSettings.Instance.ADSetting.GetInterstitialID();
        private string MaxRVSlotID => GuruSettings.Instance.ADSetting.GetRewardedVideoID();
        
        #endregion

        #region Async 回调

        public Action<bool, bool> OnBannerRequestOver { get; set; }
        public Action<bool, bool> OnInterstitialRequestOver { get; set; }
        public Action<bool, bool> OnRewardRequestOver { get; set; }
        
        #endregion

        #region 属性
        
        public static readonly string ChanelName = "Amazon";
        public string Name => ChanelName;
        
        public Action<string> OnRequestOver { get; set; }

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
                // return GuruSettings.Instance.AmazonSetting.Enable;
                return true; // 常驻开启
            }
        }
        
        #endregion
        
        #region 初始化

        /// <summary>
        /// 初始化平台
        /// </summary>
        public void Initialize(bool isDebug = false)
        {
#if UNITY_EDITOR
            Debug.Log($"<color=orange>=== Amazon will not init on Editor ===</color>");
#endif
            if (!IsEnabled)
            {
                 Debug.Log($"[Ads] --- Amazon is not enabled");
                 return;
            }
            
            // 初始化Amazon
            Amazon.Initialize (AmazonAppID);
            Amazon.SetAdNetworkInfo(new AdNetworkInfo(DTBAdNetwork.MAX));
            Debug.Log($"[Ads] --- Amazon init start isDebug:{isDebug},    AmazonID:{AmazonAppID}");
            Amazon.EnableTesting (isDebug); // Make sure to take this off when going live.
            Amazon.EnableLogging (isDebug);
		
#if UNITY_IOS
            Amazon.SetAPSPublisherExtendedIdFeatureEnabled(true);
#endif
        }

        #endregion
        
        #region Banner
        
        private APSBannerAdRequest bannerAdRequest;
        private bool _firstLoadBanner = true;
        public void LoadBannerAD()
        {
            if (!IsEnabled) return;
            
            Debug.Log($"--- Amazon banner start load ---");
            if (bannerAdRequest != null) bannerAdRequest.DestroyFetchManager();
            bannerAdRequest = new APSBannerAdRequest(BannerSize.width, BannerSize.height, AmazonBannerSlotID);
            bannerAdRequest.onSuccess += (adResponse) =>
            {
                Debug.Log($"--- Amazon Banner Load Success ---");
                MaxSdk.SetBannerLocalExtraParameter(MaxBannerSlotID, 
                    "amazon_ad_response", 
                    adResponse.GetResponse());
                OnBannerRequestOver?.Invoke(true, _firstLoadBanner);
                _firstLoadBanner = false;
            };
            
            bannerAdRequest.onFailedWithError += (adError) =>
            {
                Debug.Log($"--- Amazon Banner Load Fail: [{adError.GetCode()}] {adError.GetMessage()}");
                MaxSdk.SetBannerLocalExtraParameter(MaxBannerSlotID, 
                    "amazon_ad_error", 
                    adError.GetAdError());
                OnBannerRequestOver?.Invoke(false, _firstLoadBanner);
            };
            
            bannerAdRequest.LoadAd();

#if UNITY_EDITOR
            OnBannerRequestOver?.Invoke(true, false);
#endif
            
        }
        #endregion

        #region Intersitial

        private bool _isFirstLoadInter = true;
        private APSVideoAdRequest interstitialAdRequest;
        
        public void LoadInterstitialAD()
        {
            if (!IsEnabled) return;
            
            // 首次启动注入渠道参数
            if (_isFirstLoadInter)
            {
                Debug.Log($"--- Amazon INTER start load ---");
                interstitialAdRequest = new APSVideoAdRequest(VideoSize.width, VideoSize.height, AmazonInterVideoSlotID);
                interstitialAdRequest.onSuccess += (adResponse) =>
                {
                    Debug.Log($"--- Amazon INTER Load Success ---");
                    MaxSdk.SetInterstitialLocalExtraParameter(MaxIVSlotID, 
                        "amazon_ad_response",
                        adResponse.GetResponse());
                    OnInterstitialRequestOver?.Invoke(true, true);
                    _isFirstLoadInter = false;
                };
                interstitialAdRequest.onFailedWithError += (adError) =>
                {
                    Debug.Log($"--- Amazon INTER Load Fail: [{adError.GetCode()}] {adError.GetMessage()}");
                    MaxSdk.SetInterstitialLocalExtraParameter(MaxIVSlotID, 
                        "amazon_ad_error", 
                        adError.GetAdError());
                    OnInterstitialRequestOver?.Invoke(false, true); // 不成功则一直请求Amazon广告
                };
                interstitialAdRequest.LoadAd();
            }
            else
            {
                OnInterstitialRequestOver?.Invoke(true, false); // 走默认的广告加载逻辑
            }
            
#if UNITY_EDITOR
            OnInterstitialRequestOver?.Invoke(true, false);
            _isFirstLoadInter = false;
#endif
        }
        
        

        #endregion
        
        #region Reward
        
        private bool _firstLoadRewward = true;
        private APSVideoAdRequest rewardedVideoAdRequest;
        
        public void LoadRewardAD()
        {
            if (!IsEnabled) return;
            
            if (_firstLoadRewward)
            {
                Debug.Log($"--- Amazon Reward start load ---");
                rewardedVideoAdRequest = new APSVideoAdRequest(VideoSize.width, VideoSize.height, AmazonRewardSlotID);
                rewardedVideoAdRequest.onSuccess += (adResponse) =>
                {
                    Debug.Log($"--- Amazon Reward Load Success ---");
                    MaxSdk.SetRewardedAdLocalExtraParameter(MaxRVSlotID, 
                        "amazon_ad_response",
                        adResponse.GetResponse());
                    OnRewardRequestOver?.Invoke(true, true);
                    _firstLoadRewward = false;
                };
                rewardedVideoAdRequest.onFailedWithError += (adError) =>
                {
                    Debug.Log($"--- Amazon Reward Load Fail: [{adError.GetCode()}] {adError.GetMessage()}");
                    MaxSdk.SetRewardedAdLocalExtraParameter(MaxRVSlotID, 
                        "amazon_ad_error", 
                        adError.GetAdError());
                    OnRewardRequestOver?.Invoke(false, true);  // 不成功则一直请求Amazon广告
                };
                rewardedVideoAdRequest.LoadAd();
            }
            else
            {
                OnRewardRequestOver?.Invoke(true, false); // 走默认的广告加载逻辑
            }
#if UNITY_EDITOR
            OnRewardRequestOver?.Invoke(true, false);
#endif
        }
        

        #endregion

    }
    
}