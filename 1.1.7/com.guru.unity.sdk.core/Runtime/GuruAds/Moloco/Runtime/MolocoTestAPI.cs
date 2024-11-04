using UnityEngine;

namespace Guru
{
    /// <summary>
    /// Moloco 测试接口
    /// </summary>
    public class MolocoTestAPI
    {
        

        #region 初始化


       


        #endregion
        
        #region Banner

        private static string _testBannerId;
        private static bool _isLoadingBanner = false;
        /// <summary>
        /// 请求测试广告Ba
        /// </summary>
        public static void LoadDebugBanner(string bannerId)
        {
            if (_isLoadingBanner) return;
            _isLoadingBanner = true;
            _testBannerId = bannerId;
            AddBannerCallBacks();
            MaxSdk.CreateBanner(_testBannerId, MaxSdkBase.BannerPosition.BottomCenter);
            ShowToast($"Load Banner: {_testBannerId}");
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
            if (adUnitId == _testBannerId)
            {
                RemoveBannerCallBacks();
                Debug.Log($"[PM] Load Banner success => Revenue: {adInfo.Revenue}");
                MaxSdk.ShowBanner(adUnitId);
                _isLoadingBanner = false;
                
                ShowToast($"Banner Loaded: {_testBannerId}");
            }
        }

        private static void OnDebugBannerLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            if (adUnitId == _testBannerId)
            {
                RemoveBannerCallBacks();
                Debug.Log($"[PM] Load Banner fail => Waterfall:{errorInfo.WaterfallInfo.Name}");
                _isLoadingBanner = false;
                
                ShowToast($"Banner Load fail: {_testBannerId}");
            }
        }


        #endregion
        
        #region Interstitial


        private static bool _isLoadingIV = false;
        private static string _testIVId;
        
        /// <summary>
        /// 请求测试广告IV
        /// </summary>
        public static void LoadDebugIV(string unitId)
        {
            if (_isLoadingIV) return;
            _isLoadingIV = true;
            _testIVId = unitId;
            AddIvCallBacks();
            MaxSdk.LoadInterstitial(_testIVId);
            
            ShowToast($"Load Interstitial: {_testIVId}");
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
            if (adUnitId == _testIVId)
            {
                RemoveIvCallBacks();
                _isLoadingIV = false;
                Debug.Log($"[PM] Load IV success => Revenue: {adInfo.Revenue}");
                string placement = "pm_test_iv";
                MaxSdk.ShowInterstitial(adUnitId, placement);
                ShowToast($"Load IV Success: {_testIVId}");
            }
        }

        private static void OnDebugIVLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            if (adUnitId == _testIVId)
            {
                RemoveIvCallBacks();
                _isLoadingIV = false;
                Debug.Log($"[PM] Load IV fail => Waterfall:{errorInfo.WaterfallInfo.Name}");
                ShowToast($"Load IV Fail: {_testIVId}");
            }
        }
        
        #endregion

        #region Reward



        private static string _testRVId;
        private static bool _isLoadingRV = false;
        /// <summary>
        /// 请求测试广告RV
        /// </summary>
        public static void LoadDebugRV(string unitId)
        {
            if (_isLoadingRV) return;
            _isLoadingRV = true;
            _testRVId = unitId;
            AddRvCallBacks();
            MaxSdk.LoadRewardedAd(_testRVId);
            
            ShowToast($"Load RV: {_testRVId}");
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
            if (adUnitId == _testRVId)
            {
                RemoveRvCallBacks();
                _isLoadingRV = false;
                Debug.Log($"[PM] Load RV success => Revenue: {adInfo.Revenue}");
                string placement = "pm_test_rv";
                MaxSdk.ShowRewardedAd(adUnitId, placement);
                ShowToast($"Load RV Success: {_testRVId}");
            }
        }

        private static void OnDebugRVLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            if (adUnitId == _testRVId)
            {
                RemoveRvCallBacks();
                _isLoadingRV = false;
                Debug.Log($"[PM] Load RV fail => Waterfall:{errorInfo.WaterfallInfo.Name}");
                ShowToast($"Load RV Fail: {_testRVId}");
            }
        }
        #endregion
        
        #region Debug


        /// <summary>
        /// 显示Toast信息
        /// </summary>
        /// <param name="msg"></param>
        public static void ShowToast(string msg)
        {
#if UNITY_ANDROID   
            // U3D2Android.ShowToast(msg);
#else
            Debug.Log(msg);
#endif
        }



        #endregion
    }
}