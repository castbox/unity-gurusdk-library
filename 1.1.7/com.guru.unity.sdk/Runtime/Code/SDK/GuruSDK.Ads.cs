
namespace Guru
{
    using UnityEngine;
    using System;
    using Guru.Notification;
    
    public partial class GuruSDK
    {
        private const float CONSENT_FLOW_TIMEOUT = 10; // Consent Flow 超时时间（秒）   
        private static AdsInitSpec _adInitSpec;
        
        /// <summary>
        /// 启动广告服务
        /// </summary>
        public static void StartAds(AdsInitSpec spec = null)
        {
            _adInitSpec = spec;
            if (InitConfig.UseCustomConsent)
            {
                Debug.Log($"{Tag} --- Call <color=orange>StartAdsWithCustomConsent</color> when you use custom consent, and pass the result (boolean) to the method.");
                return;
            }
            
            // 默认的启动顺序是先启动Consent后, 根据用户回调的结果来启动广告
            Instance.StartConsentFlow();
        }

        /// <summary>
        /// 是否已经购买了去广告
        /// </summary>
        /// <param name="buyNoAds"></param>
        public static void StartAds(bool buyNoAds = false)
        {
            StartAds(AdsInitSpec.BuildWithNoAds());  // 按照默认的去广告来生成广告启动配置
        }


        /// <summary>
        /// 使用自定义的Consent, 获取用户授权后, 调用此方法
        /// </summary>
        /// <param name="userAllow"></param>
        /// <param name="consentName">Consent 引导的类型, 如果使用了 MAX 的 consent 请填写 max </param>
        /// <param name="spec">广告启动配置</param>
        public static void StartAdsWithCustomConsent(bool userAllow = true, 
            string consentName = "custom", AdsInitSpec spec = null)
        {
#if UNITY_IOS
            _attType = consentName;
            InitAttStatus();
#endif
            if (userAllow)
            {
#if UNITY_IOS
                Instance.CheckAttStatus();
#else
                StartAdService(spec);
#endif
            }
            else
            {
                Debug.Log($"{Tag} --- User refuse to provide ads Id, Ads Service will be cancelled");
            }
        }

        /// <summary>
        /// 使用自定义的Consent, 获取用户授权后, 调用此方法
        /// </summary>
        /// <param name="userAllow">自定义 Consent 的用户授权结果</param>
        /// <param name="consentName">Consent引导名称</param>
        /// <param name="buyNoAds">是否已经购买了去广告</param>
        public static void StartAdsWithCustomConsent(bool userAllow = true, string consentName = "custom",
            bool buyNoAds = false)
        {
            StartAdsWithCustomConsent(userAllow, consentName, AdsInitSpec.BuildWithNoAds());
        }


        #region Guru Consent

        private bool _hasConsentCalled = false;
        private bool _adServiceHasStarted = false;
        private string _notiStatue = "";
        private bool _hasNotiGranted = false;

        /// <summary>
        /// 启动Consent流程
        /// 因为之后规划广告流程会放在 Consent 初始化之后, 因此请求广告的时候会需要先请求 Consent
        /// </summary>
        private void StartConsentFlow()
        {
            LogI($"#4.5 ---  StartConsentFlow ---");
            
            float time = 1;
            if (!_adServiceHasStarted && _appServicesConfig != null)
            {
                time = _appServicesConfig.IsAdsCompliance() ? CONSENT_FLOW_TIMEOUT : 1f; // 启动合规判定后, 延迟最多 10 秒后启动广告
            }
            Delay(time, AdServiceHandler); // 广告延迟启动
            
            if (_hasConsentCalled) return;
            _hasConsentCalled = true;

            bool enableCountryCheck = false;
            string dmaMapRule = "";
            
            if (_appServicesConfig != null && _appServicesConfig.parameters != null)
            {
                enableCountryCheck = _appServicesConfig.DMACountryCheck();
                dmaMapRule = _appServicesConfig.DMAMapRule();
            }

#if UNITY_IOS
            InitAttStatus(); // Consent 启动前记录 ATT 初始值
#endif
            UnityEngine.Debug.Log($"{Tag}  --- Call:StartConsentFlow ---");
            GuruConsent.StartConsent(OnGuruConsentOver, dmaMapRule:dmaMapRule, enableCountryCheck:enableCountryCheck);
        }

        /// <summary>
        /// Guru Consent flow is Over
        /// </summary>
        /// <param name="code"></param>
        private void OnGuruConsentOver(int code)
        {
            
            // 无论状态如何, 都在回调内启动广告初始化
            AdServiceHandler();

            // 调用回调
            Callbacks.ConsentFlow.InvokeOnConsentResult(code);
            
#if UNITY_IOS
            CheckAttStatus();  // [iOS] Consent 启动后检查 ATT 初始值
#elif UNITY_ANDROID
            CheckNotiPermission(); // Consent 回调后检查 Notification 权限
#endif
            
            // 内部处理后继逻辑
            switch(code)
            {
                case GuruConsent.StatusCode.OBTAINED:
                case GuruConsent.StatusCode.NOT_AVAILABLE:
                    // 已获取授权, 或者地区不可用, ATT 尚未启动
                    // TODO: 添加后继处理逻辑
                    break;
            }
        }

        /// <summary>
        /// 启动广告服务
        /// </summary>
        private void AdServiceHandler()
        {
            if (_adServiceHasStarted) return;
            _adServiceHasStarted = true;
            StartAdService(_adInitSpec);
        }



        #endregion
        
        #region IOS ATT 广告授权流程
        
#if UNITY_IOS
        
        private static string _initialAttStatus;
        private static String _attType = "admob";
        private static bool _autoReCallAtt = false;
        
        /// <summary>
        /// 显示系统的 ATT 弹窗
        /// </summary>
        public void RequestAttDialog()
        {
            LogI($"RequestATTDialog");
            ATTManager.RequestATTDailog(ReportAttStatus);
        }
        
        /// <summary>
        /// 初始化 ATT 状态
        /// </summary>
        public static void InitAttStatus()
        {
            _attType = InitConfig.UseCustomConsent ? ATTManager.GUIDE_TYPE_CUSTOM : ATTManager.GUIDE_TYPE_ADMOB; // 点位属性确定
            _initialAttStatus = ATTManager.GetStatus();
            SetATTStatus(_initialAttStatus); // #1 初始化的时候上报 ATT 状态
        }
        
        /// <summary>
        /// iOS 平台检查 ATT 状态
        /// </summary>
        private void CheckAttStatus(bool autoReCall = false)
        {
            _autoReCallAtt = autoReCall;
            
            // Delay 1s to get the user choice
            Delay(1, () => ATTManager.CheckStatus(ReportAttStatus));
        }
        
        /// <summary>
        /// 上报 Att 状态
        /// </summary>
        /// <param name="status"></param>
        private void ReportAttStatus(string status)
        {
            LogI($"{Tag} --- Check Att status:{status}  att Type:{_attType}  recall:{_autoReCallAtt}");
            
            switch(status)
            {
                case ATTManager.ATT_STATUS_NOT_DETERMINED:
                    // ATT 状态未知, 请求弹窗
                    if(_autoReCallAtt) RequestAttDialog();
                    break;
                case ATTManager.ATT_STATUS_RESTRICTED:
                case ATTManager.ATT_STATUS_DENIED:
                    // ATT 状态受限, 或者被拒绝
                    // TODO: 可以引导用户打开授权页
                    break;
                case ATTManager.ATT_STATUS_AUTHORIZED:
                    // ATT 状态已授权
                    break;
            }
            
            // #2 获取用户变化后上报 ATT 状态:
            SetATTStatus(status);
            LogI($"{Tag} --- Report New Att status:{status}");
            
            CheckNotiPermission(); // Consent 回调后检查 Notification 权限
        } 
#endif
        
        #endregion

        #region Notification Permission Check
        
        /// <summary>
        /// 初始化 Noti Service
        /// </summary>
        private void InitNotiPermission()
        {
            // bool hasNotiGranted = false;
            _notiStatue = "no_determined";
            NotificationService.Initialize(); // 初始化 Noti 服务
            Analytics.SetNotiPerm(NotificationService.GetStatus());
        }
        
        /// <summary>
        /// 检查 Noti 状态
        /// </summary>
        private void CheckNotiPermission()
        {
            var status = NotificationService.GetStatus();
            
            // 如果未启用自动 Noti 授权，则直接上报状态
            if (!_initConfig.AutoNotificationPermission)
            {
                Debug.LogWarning($"[SDK] ---- AutoNotificationPermission is OFF, Project should request permission own.");
                return;
            }

            bool isGranted = NotificationService.IsPermissionGranted();
            Debug.Log($"[SDK] ---- Check Noti Permission: {isGranted}");
            if (isGranted)
            {
                Debug.Log($"[SDK] ---- Set Notification Permission: {status}");
                Analytics.SetNotiPerm(status);
                return;
            }

            RequestNotificationPermission(); // 请求授权
        }
        
        /// <summary>
        /// 请求推送授权
        /// </summary>
        /// <param name="callback"></param>
        public static void RequestNotificationPermission(Action<string> callback = null)
        {
            FirebaseUtil.StartFetchFcmToken();
            
            Debug.Log($"[SDK] ---- RequestNotificationPermission");
            NotificationService.RequestPermission(status =>
            {
                Debug.Log($"[SDK] ---- Set Notification Permission: {status}");
                if(!string.IsNullOrEmpty(status)) Analytics.SetNotiPerm(status);
                
                callback?.Invoke(status);
            });
        }
        
        /// <summary>
        /// 获取 Notification 状态值
        /// </summary>
        /// <returns></returns>
        public static string GetNotificationStatus()
        {
            return NotificationService.GetStatus();
        }
        
        /// <summary>
        /// 用户是否已经获取了 Notification 授权了
        /// </summary>
        /// <returns></returns>
        public static bool IsNotificationPermissionGranted()
        {
            return NotificationService.IsPermissionGranted();
        }

        #endregion
        
        #region Ad Services

        private static bool _initAdsCompleted = false;
        public static bool IsAdsReady => _initAdsCompleted;
        private static int _preBannerAction = 0;
        
        public static AdsInitSpec GetDefaultAdsSpec()
        {
            return AdsInitSpec.BuildDefault(InitConfig.AutoLoadWhenAdsReady, IsDebugMode);
        }


        /// <summary>
        /// 启动广告服务
        /// </summary>
        private static void StartAdService(AdsInitSpec spec = null)
        {
            
            //---------- Using InitConfig ----------
            if (InitConfig is { IsBuyNoAds: true }) SetBuyNoAds(true);

            LogI($"StartAdService");
            if (spec == null)
            {
                spec = AdsInitSpec.BuildDefault(InitConfig.AutoLoadWhenAdsReady, IsDebugMode);
                if (ADService.Instance.IsBuyNoAds)
                {
                    spec = AdsInitSpec.BuildWithNoAds(InitConfig.AutoLoadWhenAdsReady, IsDebugMode);
                }
            }
            
            if(InitConfig != null && !string.IsNullOrEmpty(InitConfig.BannerBackgroundColor)) 
                spec.bannerColorHex = InitConfig.BannerBackgroundColor;
            
            //--------- Add Callbacks -----------
            // BADS
            ADService.Instance.OnBannerStartLoad = OnBannerStartLoad;
            ADService.Instance.OnBannerLoaded = OnBannerLoaded;
            // IADS
            ADService.Instance.OnInterstitialStartLoad = OnInterstitialStartLoad;
            ADService.Instance.OnInterstitialLoaded = OnInterstitialLoaded;
            ADService.Instance.OnInterstitialFailed = OnInterstitialFailed;
            ADService.Instance.OnInterstitialClosed = OnInterstitialClosed;
            // RADS
            ADService.Instance.OnRewardedStartLoad = OnRewardStartLoad;
            ADService.Instance.OnRewardLoaded = OnRewardLoaded;
            ADService.Instance.OnRewardFailed = OnRewardFailed;
            ADService.Instance.OnRewardClosed = OnRewardClosed;
            
            // ---------- Start Services ----------
            ADService.Instance.StartService(OnAdsInitComplete, spec);
            
            // ---------- Life Cycle ----------
            Callbacks.App.OnAppPaused += OnAppPaused;
        }
        
        /// <summary>
        /// 生命周期回调
        /// </summary>
        /// <param name="paused"></param>
        private static void OnAppPaused(bool paused)
        {
            if(ADService.Instance.IsInitialized)
                ADService.Instance.OnAppPaused(paused);
        }

        private static void OnBannerStartLoad(string adUnitId)
            => Callbacks.Ads.InvokeOnBannerADStartLoad(adUnitId);
        private static void OnBannerLoaded() 
            => Callbacks.Ads.InvokeOnBannerADLoaded();
        private static void OnInterstitialStartLoad(string adUnitId) 
            => Callbacks.Ads.InvokeOnInterstitialADStartLoad(adUnitId);
        private static void OnInterstitialLoaded() 
            => Callbacks.Ads.InvokeOnInterstitialADLoaded();
        private static void OnInterstitialFailed()
            => Callbacks.Ads.InvokeOnInterstitialADFailed();
        private static void OnInterstitialClosed()
            => Callbacks.Ads.InvokeOnInterstitialADClosed();
        private static void OnRewardStartLoad(string adUnitId)
            => Callbacks.Ads.InvokeOnRewardedADStartLoad(adUnitId); 
        private static void OnRewardLoaded()
            => Callbacks.Ads.InvokeOnRewardedADLoaded(); 
        private static void OnRewardFailed()
            => Callbacks.Ads.InvokeOnRewardADFailed();
        private static void OnRewardClosed()
            => Callbacks.Ads.InvokeOnRewardADClosed();

        private static void OnAdsInitComplete()
        {
            _initAdsCompleted = true;

            if (_adInitSpec != null && _adInitSpec.loadBanner)
            {
                // 预制动作处理
                if (_preBannerAction == 1)
                {
                    _preBannerAction = 0;
                    ShowBanner();
                }
                else if (_preBannerAction == -1)
                {
                    _preBannerAction = 0;
                    HideBanner();
                }
            }
            Callbacks.Ads.InvokeOnAdsInitComplete();
        }

        private static bool CheckAdsReady()
        {
            if (!IsAdsReady)
            {
                LogW("[SDK] Ads is not ready. Call <GuruSDk.StartAdService> first.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 显示Banner广告
        /// </summary>
        /// <param name="placement"></param>
        public static void ShowBanner(string placement = "")
        {
            if (!CheckAdsReady())
            {
                _preBannerAction = 1;
                return;
            }
            ADService.Instance.ShowBanner(placement);
        }
        
        /// <summary>
        /// 设置 Banner 背景颜色
        /// </summary>
        /// <param name="color"></param>
        public static void SetBannerBackgroundColor(Color color)
        {
            // if (!CheckAdsReady()) return;
            ADService.Instance.SetBannerBackgroundColor(color);
        }
        
        public static void SetBannerAutoRefresh(bool value = true)
        {
            if (!CheckAdsReady()) return;
            ADService.Instance.SetBannerAutoRefresh(value);
        }
        
        /// <summary>
        /// 隐藏Banner广告
        /// </summary>
        public static void HideBanner()
        {
            if (!CheckAdsReady())
            {
                _preBannerAction = -1;
                return;
            }
            ADService.Instance.HideBanner();
        }

        public static void LoadInterstitialAd()
        {
            if (!CheckAdsReady()) return;
            ADService.Instance.RequestInterstitialAD();
        }

        public static bool IsInterstitialAdReady => ADService.Instance.IsInterstitialADReady();
        
        /// <summary>
        /// 显示插屏广告
        /// </summary>
        /// <param name="placement"></param>
        /// <param name="onDismissed"></param>
        public static void ShowInterstitialAd(string placement = "", Action onDismissed = null)
        {
            if (!CheckAdsReady()) return;
            if (!ADService.Instance.IsInterstitialADReady())
            {
                LogE("Interstitial is not ready. Call <GuruSDk.ShowInterstitialAd> again.");
                LoadInterstitialAd();
                return;
            }
            ADService.Instance.ShowInterstitialAD(placement, onDismissed);
        }

        public static void LoadRewardAd()
        {
            if (!CheckAdsReady()) return;
            ADService.Instance.RequestRewardedAD();
        }
        
        public static bool IsRewardAdReady => ADService.Instance.IsRewardedADReady();
        
        /// <summary>
        /// 显示激励视频广告
        /// </summary>
        /// <param name="placement"></param>
        /// <param name="onRewarded"></param>
        /// <param name="onFailed"></param>
        public static void ShowRewardAd(string placement = "", Action onRewarded = null, Action<string> onFailed = null)
        {
            if (!CheckAdsReady()) return;
            if (!ADService.Instance.IsRewardedADReady())
            {
                LogE("RewardAd is not ready. Call <GuruSDk.LoadRewardAd> again.");
                LoadRewardAd();
                return;
            }
            ADService.Instance.ShowRewardedAD(placement, onRewarded, onFailed);
        }


        #endregion

        #region MaxServices

        /// <summary>
        /// 显示Max调试菜单
        /// </summary>
        public static void ShowMaxDebugPanel()
        {
#if UNITY_EDITOR

            LogI($"Can not show Max Debug Panel in Editor, skipped.");
            return;
#endif
            if (!ADService.Instance.IsInitialized)
            {
                LogI($"ADService is not initialized, call <GuruSDK.StartAds> first.");
                return;
            }
            ADService.Instance.ShowMaxDebugPanel();
        }

        #endregion
        
    }


    

}