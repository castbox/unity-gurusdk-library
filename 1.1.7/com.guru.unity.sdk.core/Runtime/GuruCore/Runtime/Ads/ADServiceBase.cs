
namespace Guru
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    
    //TODO: 将 BADS，IADS，RADS 分别放到不同的类中，方便维护
    public abstract class ADServiceBase<T> : IADService where T : new()
    {
        // 单利定义
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (null == _instance) _instance = new T();
                return _instance;
            }
        }

        private const string Tag = "[SDK][ADS]";
        public bool IsInitialized => MaxSdk.IsInitialized() || _isServiceStarted;
        private static bool IsNetworkEnabled => Application.internetReachability != NetworkReachability.NotReachable;

        private const int MAX_ADS_RELOAD_INTERVAL = 6; // 广告加载最高时间为 2 的 6 次方 = 64秒

        private bool _isServiceStarted;

        protected Action _onSdkInitReady;
        
        public Action<string> OnBannerStartLoad;
        public Action OnBannerLoaded;

        public Action<string> OnInterstitialStartLoad;
        public Action OnInterstitialLoaded;
        public Action OnInterstitialFailed;
        public Action OnInterstitialClosed;

        public Action<string> OnRewardedStartLoad;
        public Action OnRewardLoaded;
        public Action OnRewardFailed;
        public Action OnRewardClosed;
        
        private AdsModel _model;
        protected AdsInitSpec _initSpec = null;

        private AdsModel Model
        {
            get
            {
                if (_model == null) _model = AdsModel.Create();
                return _model;
            }
        }

        internal bool _isDebug = false;

        /// <summary>
        /// 启动广告服务
        /// </summary>
        /// <param name="callback">广告初始化回调</param>
        /// <param name="initSpec">初始化配置参数</param>
        public virtual void StartService(Action callback = null, AdsInitSpec initSpec = null)
        {
            if (IsInitialized) return; // 已经初始化后, 无需再次初始化

            _initSpec = initSpec;
            if (_initSpec == null) _initSpec = AdsInitSpec.BuildDefault();
            
            _isDebug = _initSpec.isDebug;
            _isServiceStarted = true;
            _onSdkInitReady = callback;
            if(_model == null) _model = AdsModel.Create();
            this.Log("AD SDK Start Init");

            InitMaxCallbacks(); // 初始化 MAX 广告
            InitService(); // 内部继承接口
        }

        /// <summary>
        /// 初始化 MAX 广告组件
        /// </summary>
        private void InitMaxCallbacks()
        {
            //--------------  初始化回调 ------------------
            MaxSdkCallbacks.OnSdkInitializedEvent += OnMaxSdkInitializedCallBack;
            
            //--------------- MRec 回调 -----------------
            // MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            
            //--------------- Banner 回调 -----------------
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerRevenuePaidEvent;
            // MaxSdkCallbacks.Banner.OnAdReviewCreativeIdGeneratedEvent += OnAdReviewCreativeIdGeneratedEvent;
            
            //--------------- IV 回调 -----------------
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialPaidEvent;
            // MaxSdkCallbacks.Interstitial.OnAdReviewCreativeIdGeneratedEvent += OnAdReviewCreativeIdGeneratedEvent;
            
            //--------------- RV 回调 -----------------
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdPaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
            // MaxSdkCallbacks.Rewarded.OnAdReviewCreativeIdGeneratedEvent += OnAdReviewCreativeIdGeneratedEvent;
            
            
            //-------------- SDK 初始化 -------------------
            MaxSdk.SetExtraParameter("enable_black_screen_fixes", "true"); // 修复黑屏
        }

        protected virtual void InitService()
        {
        }


        private void OnMaxSdkInitializedCallBack(MaxSdkBase.SdkConfiguration sdkConfiguration)
        {
            this.Log("AD SDK Init Success");
            MaxSdk.SetMuted(false);
            if (_initSpec.autoLoad) OnMaxSdkReady();
            _onSdkInitReady?.Invoke();
        }

        protected virtual void OnMaxSdkReady()
        {
            Debug.Log($"[ADService] --- Start Max with spec: bads:{_initSpec.loadBanner}  iads:{_initSpec.loadInterstitial}  rads:{_initSpec.loadRewarded}");
            
            //应用启动策略
            if(_initSpec.loadBanner) RequestBannerAD();
            if(_initSpec.loadInterstitial) RequestInterstitialAD();
            if(_initSpec.loadRewarded) RequestRewardedAD();
        }

        /// <summary>
        /// 可加载广告
        /// </summary>
        /// <returns></returns>
        private bool CanLoadAds()
        {
            return IsInitialized && IsNetworkEnabled;
        }

        public bool IsBuyNoAds
        {
            get => Model.BuyNoAds;
            set => Model.BuyNoAds = value;
        }

        private float GetRetryDelaySeconds(int retryCount)
        {
            // 最低 2^retryCount 秒
            // 最高 2^6 = 64 秒
            return (float)Math.Pow(2, Math.Min(MAX_ADS_RELOAD_INTERVAL, retryCount));
        }

        #region Lifecycele
        
        public void OnAppPaused(bool paused)
        {
            if (paused)
            {
                if(IsBannerVisible) SetBannerAutoRefresh(false);
            }
            else
            {
                if(IsBannerVisible) SetBannerAutoRefresh(true);
            }
        }

        #endregion

        #region 收益打点

        private double TchAD001RevValue
        {
            get => _model.TchAD001RevValue;
            set => _model.TchAD001RevValue = value;
        }

        private double TchAD02RevValue
        {
            get => _model.TchAD02RevValue;
            set => _model.TchAD02RevValue = value;
        }

        /// <summary>
        /// 上报广告收益
        /// </summary>
        /// <param name="data"></param>
        private void ReportAdsRevenue(AdParams data)
        {
            try
            {
                // #1 ad_impression
                Analytics.ADImpression(data.ToAdImpressionData());
            }
            catch (Exception ex)
            {
                CrashlyticsAgent.LogException(ex);
            }
            
            
            try
            {
                // #2 adjust_ad_revenue
                AdjustService.Instance.TrackADRevenue(
                    data.value, 
                    data.currency, 
                    data.adSource, 
                    data.adUnitId, 
                    data.networkPlacement);
            }
            catch (Exception ex)
            {
                CrashlyticsAgent.LogException(ex);
            }
            
            
            try
            {
                // #3 tch_001 + tch_02 广告点内 tch_001 和 tch_02 都需要计算
                double revenue = data.value;
                AddAdsTch001Revenue(revenue);
                AddAdsTch02Revenue(revenue);
            }
            catch (Exception ex)
            {
                CrashlyticsAgent.LogException(ex);
            }

        }

        /// <summary>
        /// 累积计算太极001收益
        /// </summary>
        /// <param name="revenue"></param>
        private void AddAdsTch001Revenue(double revenue)
        {
            TchAD001RevValue += revenue;
            double revenueValue = TchAD001RevValue;
            
            if (revenueValue < Analytics.Tch001TargetValue) return;
            
            Debug.Log($"{Tag} --- [Tch] call <tch_ad_rev_roas_001> with value: {revenueValue}");
            Analytics.Tch001ADRev(revenueValue);
            TchAD001RevValue = 0.0;
        }

        /// <summary>
        /// 累积计算太极02收益
        /// </summary>
        /// <param name="revenue"></param>
        private void AddAdsTch02Revenue(double revenue)
        {
            if (!Analytics.EnableTch02Event) return;

            TchAD02RevValue += revenue;
            double revenueValue = TchAD02RevValue;
            if (revenueValue < Analytics.TCH_02_VALUE) return;
            
            Debug.Log($"{Tag} --- [Tch] call <tch_ad_rev_roas_02> with value: {revenueValue}");
            Analytics.Tch02ADRev(revenueValue);
            TchAD02RevValue = 0.0;
        }

        #endregion

        #region Banner Ads

        private Color _backColor = new Color(0, 0, 0, 0);
        private string _badsCategory;
        protected DateTime _badsLoadStartTime;
        private bool _bannerVisible = false;
        public bool IsBannerVisible => _bannerVisible;
        private int _badsLoadedNum = 0;
        private int _badsLoadFailNum = 0;
        
        
        /// <summary>
        /// 获取动作间隔之间的毫秒数
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        private int GetActionDuration(DateTime startTime)
        {
            var sp = DateTime.UtcNow.Subtract(startTime.ToUniversalTime()).Duration();
            return (int) sp.TotalMilliseconds;
        }


        public virtual void RequestBannerAD()
        {
            _backColor = Color.clear;
            if (_initSpec != null)
            {
                _backColor = GuruSDKUtils.HexToColor(_initSpec.bannerColorHex);
            }
            
            LoadMaxBannerAd();
        }

        /// <summary>
        /// Banner MAX 加载方式
        /// </summary>
        private void LoadMaxBannerAd()
        {
            OnLoadBads();
            // Banners are automatically sized to 320x50 on phones and 728x90 on tablets
            // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments
            var adUnitId = GetBannerID();
            MaxSdk.CreateBanner(adUnitId, MaxSdkBase.BannerPosition.BottomCenter);
            MaxSdk.SetBannerExtraParameter(adUnitId, "adaptive_banner", "false");
            // Set background or background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(adUnitId, _backColor);
            // Analytics.ADBadsLoad(GetBannerID());
            Analytics.TrackAdBadsLoad(AdParams.Build(adUnitId:adUnitId).ToAdLoadedData());
        }

        private void OnLoadBads()
        {
            _badsLoadStartTime = DateTime.UtcNow;
        }

        private void OnBadsLoaded()
        {
            _badsLoadStartTime = DateTime.UtcNow;
            OnBannerLoaded?.Invoke();
        }
        
        public void SetBannerAutoRefresh(bool value = true, string adUnitId = "")
        {
            if(string.IsNullOrEmpty(adUnitId)) adUnitId = GetBannerID();
            if (value)
            {
                MaxSdk.StartBannerAutoRefresh(adUnitId);
            }
            else
            {
                MaxSdk.StopBannerAutoRefresh(adUnitId);
            }
        }

        /// <summary>
        /// 显示 Banner 
        /// </summary>
        /// <param name="category"></param>
        public virtual void ShowBanner(string category = "")
        {
            _badsCategory = category;
            string adUnitId = GetBannerID();
            MaxSdk.ShowBanner(adUnitId);
            MaxSdk.SetBannerBackgroundColor(adUnitId, _backColor);
            SetBannerAutoRefresh(true, adUnitId);
            if (!_bannerVisible)
            {
                // 从隐藏状态到显示，累计的加载次数和失败次数清零
                _bannerVisible = true;
                OnBannerImpEvent(adUnitId);
                _badsLoadedNum = 0;
                _badsLoadFailNum = 0;
            }
        }

        /// <summary>
        /// 隐藏 Banner
        /// </summary>
        public void HideBanner()
        {
            string adUnitId = GetBannerID();
            MaxSdk.HideBanner(adUnitId);
            SetBannerAutoRefresh(false, adUnitId);
            if (_bannerVisible)
            {
                _bannerVisible = false;
                OnBannerHideEvent();
            }
        }

        /// <summary>
        /// 设置 Banner 背景颜色
        /// </summary>
        /// <param name="color"></param>
        public void SetBannerBackgroundColor(Color color)
        {
            _backColor = color;
        }

        private void OnBannerLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _badsLoadedNum++; // 记录加载次数
            Debug.Log( $"[SDK][Ads][Loaded] --- adUnitId:{adUnitId}    Revenue:{adInfo.Revenue}    Type:{adInfo.AdFormat}    CreativeId:{adInfo.CreativeIdentifier}");
            OnBadsLoaded();
            // BADSLoaded 事件已经不做上报处理了
            // --- fixed by YuFei 2024-5-29 为 don't report bads_loaded any more. ---
            // Analytics.ADBadsLoaded(AdParams.Build(adUnitId, adInfo,
            //     duration: GetAdsLoadDuration(ref _badsLoadStartTime), category: _badsCategory));
        }

        private void OnBannerFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _badsLoadFailNum++; // 记录加载失败次数 
            Analytics.TrackAdBadsFailed(BuildBadsFailedData(adUnitId, errorInfo));
        }

        private void OnBannerClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Analytics.ADBadsClick(adUnitId, _badsCategory);
            Analytics.TrackAdBadsClick(BuildBadsClickData(adInfo, adUnitId));
        }

        private void OnBannerImpEvent(string adUnitId)
        {
            // Analytics.ADBadsClick(adUnitId, _badsCategory);
            Analytics.TrackAdBadsImp(BuildBadsImpData(adUnitId));
        }
        
        
        private void OnBannerHideEvent()
        {
            Analytics.TrackAdBadsHide(_badsLoadedNum, _badsLoadFailNum);
        }
        
        /// <summary>
        /// Banner 收益打点
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="adInfo"></param>
        private void OnBannerRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (_bannerVisible)
            {
                ReportAdsRevenue(AdParams.Build(adInfo, adUnitId));
            }
        }

        #endregion

        #region Interstitial Ads

        private string _iadsCategory = "main";
        private int _interstitialRetryAttempt;
        private Action _interCloseAction;
        private bool _isIadsLoading = false;
        protected DateTime _iadsLoadStartTime;
        private DateTime _iadsDisplayStartTime;
        
        
        public bool IsIadsLoading => _isIadsLoading;

        public virtual void RequestInterstitialAD()
        {
            if (!CanLoadAds()) return;
            
            if(_isIadsLoading) return;
            _isIadsLoading = true;

            LoadMaxInterstitial();
        }


        private void LoadMaxInterstitial()
        {
            OnLoadIads();
            var adUnitId = GetInterstitialID();
            Analytics.TrackAdIadsLoad(AdParams.Build(adUnitId:adUnitId).ToAdLoadedData());
            MaxSdk.LoadInterstitial(adUnitId);
        }

        private void OnLoadIads()
        {
            _iadsLoadStartTime = DateTime.UtcNow;
        }


        public bool IsInterstitialADReady()
        {
            if (!IsInitialized) return false;
            return MaxSdk.IsInterstitialReady(GetInterstitialID());
        }

        /// <summary>
        /// 显示插屏广告
        /// </summary>
        /// <param name="category">广告奖励回调</param>
        /// <param name="dismissAction">广告界面关闭回调</param>
        public virtual void ShowInterstitialAD(string category, Action dismissAction = null)
        {
            if (!IsInitialized)
            {
                this.LogWarning("广告未初始化完成，无法显示插屏广告");
                return;
            }

            if (!IsInterstitialADReady())
            {
                this.LogWarning("插屏没有加载准备好，无法显示插屏广告");
                return;
            }

            _iadsCategory = category;
            _interCloseAction = dismissAction;
            MaxSdk.ShowInterstitial(GetInterstitialID());

            _iadsDisplayStartTime = DateTime.UtcNow;
        }

        protected virtual void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _isIadsLoading = false;
            // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'
            // Reset retry attempt
            // Analytics.ADIadsLoaded(adUnitId, GetAdsLoadDuration(ref _iadsLoadStartTime), _iadsCategory);
            Analytics.TrackAdIadsLoaded(BuildIadsLoadedData(adInfo, adUnitId));
            _interstitialRetryAttempt = 0;
            Debug.Log( $"[SDK][Ads][Loaded] --- adUnitId:{adUnitId}    Revenue:{adInfo.Revenue}    Type:{adInfo.AdFormat}    CreativeId:{adInfo.CreativeIdentifier}");
            OnInterstitialLoaded?.Invoke();
        }

        protected virtual void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _isIadsLoading = false;
            // Interstitial ad failed to load 
            // We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds)
            this.LogError(
                $"OnInterstitialFailedEvent AdLoadFailureInfo:{errorInfo.AdLoadFailureInfo}, Message: {errorInfo.Message}");
            _interstitialRetryAttempt++;
            float retryDelay = GetRetryDelaySeconds(_interstitialRetryAttempt);
            DelayCall(retryDelay, RequestInterstitialAD);
            // Analytics.ADIadsFailed(adUnitId, (int)errorInfo.Code, GetAdsLoadDuration(ref _iadsLoadStartTime), _iadsCategory);
            Analytics.TrackAdIadsFailed(BuildIadsFailedData(adUnitId, errorInfo));

            OnInterstitialFailed?.Invoke();
        }

        protected virtual void InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad failed to display. We recommend loading the next ad
            this.LogError(
                $"InterstitialFailedToDisplayEvent AdLoadFailureInfo:{errorInfo.AdLoadFailureInfo}, Message: {errorInfo.Message}");
            // Analytics.ADIadsFailed(adUnitId, (int)errorInfo.Code, GetAdsLoadDuration(ref _iadsLoadStartTime), _iadsCategory);
            Analytics.TrackAdIadsFailed(BuildIadsFailedData(adUnitId, errorInfo));
            DelayCall(2.0f, RequestInterstitialAD);
        }
        
        // iads_imp
        protected virtual void OnInterstitialDisplayEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Analytics.ADIadsImp(adUnitId, _iadsCategory);
            Analytics.TrackAdIadsImp(BuildIadsImpData(adInfo, adUnitId));
        }

        protected virtual void OnInterstitialClickEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Analytics.ADIadsClick(adUnitId, _iadsCategory);
            Analytics.TrackAdIadsClick(BuildIadsClickData(adInfo, adUnitId));
        }
        
        // Close 
        protected virtual void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad is hidden. Pre-load the next ad
            _interCloseAction?.Invoke();
            OnInterstitialClosed?.Invoke();
            // Analytics.ADIadsClose(adUnitId, _iadsCategory);
            Analytics.TrackAdIadsClose(BuildIadsCloseData(adInfo, adUnitId));
            
            //延时加载下一个广告
            DelayCall(2.0f, RequestInterstitialAD);
        }

        private void OnInterstitialPaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var p = AdParams.Build(adInfo, adUnitId, category:_iadsCategory);
            Analytics.TrackAdIadsPaid(p.ToAdPaidData());
            ReportAdsRevenue(p);
        }


        #endregion

        #region Rewarded Ads

        private string _radsCategory = "main";
        private int _rewardRetryAttempt;
        protected DateTime _radsLoadStartTime;
        private DateTime _radsDisplayStartTime;
        private Action _rvRewardAction;
        private Action<string> _rvFailAction;
        private Action _rvDismissAction;
        private bool _isRadsLoading = false;
        public bool IsRadsLoading => _isRadsLoading;

        public virtual void RequestRewardedAD()
        {
            if (!CanLoadAds()) return;

            if (_isRadsLoading) return;
            _isRadsLoading = true;
            
            LoadMaxRewardAd();
        }


        /// <summary>
        /// 默认加载 MAX 广告逻辑
        /// </summary>
        private void LoadMaxRewardAd()
        {
            if (IsRadsLoading) return;
            
            OnLoadRads();
            var adUnitId = GetRewardedID();
            Analytics.TrackAdRadsLoad(AdParams.Build(adUnitId:adUnitId).ToAdLoadData()); // 上报打点
            MaxSdk.LoadRewardedAd(adUnitId);
        }

        private void OnLoadRads()
        {
            _radsLoadStartTime = DateTime.UtcNow;
        }


        public virtual bool IsRewardedADReady()
        {
            if (!IsInitialized)
                return false;

            return MaxSdk.IsRewardedAdReady(GetRewardedID());
        }

        /// <summary>
        /// 显示激励视频广告
        /// </summary>
        /// <param name="category">广告奖励回调</param>
        /// <param name="rewardAction">广告失败回调</param>
        /// <param name="failAction">广告失败回调</param>
        /// <param name="dismissAction">广告界面关闭回调</param>
        public virtual void ShowRewardedAD(string category, Action rewardAction = null,
            Action<string> failAction = null, Action dismissAction = null)
        {
            if (!IsInitialized)
            {
                this.LogWarning("广告未初始化完成，无法显示视频广告");
                return;
            }

            if (!IsRewardedADReady())
            {
                this.LogWarning("广告没有准备好，无法显示视频广告");
                return;
            }

            _radsCategory = category;
            _rvRewardAction = rewardAction;
            _rvFailAction = failAction;
            _rvDismissAction = dismissAction;
            MaxSdk.ShowRewardedAd(GetRewardedID());

            _radsDisplayStartTime = DateTime.UtcNow;
            
            // RequestRewardedAD();
        }

        protected virtual void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _isRadsLoading = false;
            
            // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'
            // Reset retry attempt
            // this.Log("OnRewardedAdLoadedEvent");
            // Analytics.ADRadsLoaded(adUnitId, GetAdsLoadDuration(ref _radsLoadStartTime), _rewardCategory);
            Analytics.TrackAdRadsLoaded(BuildRadsLoadedData(adInfo, adUnitId));
            _rewardRetryAttempt = 0;
            
            Debug.Log( $"[SDK][Ads][Loaded] --- adUnitId:{adUnitId}    Revenue:{adInfo.Revenue}    Type:{adInfo.AdFormat}    CreativeId:{adInfo.CreativeIdentifier}");

            OnRewardLoaded?.Invoke();
        }

        protected virtual void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _isRadsLoading = false;
            
            // Rewarded ad failed to load 
            // We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds)
            this.LogError(
                $"OnRewardedAdFailedEvent AdLoadFailureInfo:{errorInfo.AdLoadFailureInfo}, Message: {errorInfo.Message}");
            // Analytics.ADRadsFailed(adUnitId, (int)errorInfo.Code, GetAdsLoadDuration(ref _radsLoadStartTime), _rewardCategory);
            Analytics.TrackAdRadsFailed(BuildRadsFailedData(adUnitId, errorInfo));
            _rewardRetryAttempt++;
            float retryDelay = GetRetryDelaySeconds(_rewardRetryAttempt);
            DelayCall(retryDelay, RequestRewardedAD);

            OnRewardFailed?.Invoke();
        }

        protected virtual void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo arg3)
        {
            // Rewarded ad failed to display. We recommend loading the next ad
            this.LogError(
                $"OnRewardedAdFailedToDisplayEvent AdLoadFailureInfo:{errorInfo.AdLoadFailureInfo}, Message: {errorInfo.Message}");
            // Analytics.ADRadsFailed(adUnitId, (int)errorInfo.Code, GetAdsLoadDuration(ref _radsLoadStartTime), _rewardCategory);
            Analytics.TrackAdRadsFailed(BuildRadsFailedData(adUnitId, errorInfo));
            _rvFailAction?.Invoke("OnRewardedAdFailedToDisplayEvent");
            DelayCall(2.0f, RequestRewardedAD);

            OnRewardFailed?.Invoke();
        }

        protected virtual void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.Log("OnRewardedAdDisplayedEvent");
            // Analytics.ADRadsImp(adUnitId, _rewardCategory);
            Analytics.TrackAdRadsImp(BuildRadsImpData(adInfo, adUnitId));
        }

        protected virtual void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.Log("OnRewardedAdClickedEvent");
            // Analytics.ADRadsClick(adUnitId, _rewardCategory);
            Analytics.TrackAdRadsClick(BuildRadsClickData(adInfo, adUnitId));
        }
        
        
        // rads_close
        protected virtual void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            this.Log("OnRewardedAdDismissedEvent");
            
            _rvDismissAction?.Invoke();
            OnRewardClosed?.Invoke();
            
            // Analytics.ADRadsClose(adUnitId, _rewardCategory);
            Analytics.TrackAdRadsClose(BuildRadsCloseData(adInfo, adUnitId));
            
            //延时加载下一个广告
            DelayCall(2.0f, RequestRewardedAD);
        }
        
        // rads_rewarded
        protected virtual void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward,
            MaxSdkBase.AdInfo adInfo)
        {
            this.Log("OnRewardedAdReceivedRewardEvent");
            Analytics.TrackAdRadsRewarded(BuildRadsRewardedData(adInfo, adUnitId));
            // Rewarded ad was displayed and user should receive the reward
            _rvRewardAction?.Invoke();
        }

        // rads_paid
        private void OnRewardedAdPaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var p = AdParams.Build(adInfo, adUnitId, category:_radsCategory);
            Analytics.TrackAdRadsPaid(p.ToAdPaidData());
            ReportAdsRevenue(p);
        }
        #endregion

        #region Ad Settings

        protected virtual string GetRewardedID()
        {
            return GuruSettings.Instance.ADSetting.GetRewardedVideoID();
        }

        protected virtual string GetInterstitialID()
        {
            return GuruSettings.Instance.ADSetting.GetInterstitialID();
        }

        protected virtual string GetBannerID()
        {
            return GuruSettings.Instance.ADSetting.GetBannerID();
        }

        #endregion

        #region MaxDebugView

        public void ShowMaxDebugPanel()
        {
#if !UNITY_EDITOR
            if (!IsInitialized) return;
	        MaxSdk.ShowMediationDebugger();
#endif
        }

        #endregion

        #region DelayCall

        private void DelayCall(float time, Action callback)
        {
            CoroutineHelper.Instance.StartDelayed(time, callback);
        }

        #endregion

        #region Build AdParams

        //---------- BADS Params --------------
        private Dictionary<string, object> BuildBadsImpData(string adUnitId)
        {
            var p = AdParams.Build(adUnitId:adUnitId, category:_badsCategory);
            return p.ToBadsImpData();
        }
        
        private Dictionary<string, object> BuildBadsFailedData(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            int duration = GetActionDuration(_badsLoadStartTime);
            int errorCode = (int)errorInfo.Code;
            var p = AdParams.Build(adUnitId:adUnitId, errorCode:errorCode, duration:duration);
            return p.ToAdFailedData();
        }
        
        private Dictionary<string, object> BuildBadsClickData(MaxSdk.AdInfo adInfo, string adUnitId)
        {
            var p = AdParams.Build(adInfo, adUnitId, _badsCategory);
            return p.ToAdClickData();
        }


        //---------- IADS Params --------------
        private Dictionary<string, object> BuildIadsLoadedData(MaxSdk.AdInfo adInfo, string adUnitId)
        {
            int duration = GetActionDuration(_iadsLoadStartTime);
            var p = AdParams.Build(adInfo, adUnitId, duration:duration);
            return p.ToAdLoadedData();
        }
        
        private Dictionary<string, object> BuildIadsFailedData(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            int duration = GetActionDuration(_iadsLoadStartTime);
            int errorCode = (int)errorInfo.Code;
            var p = AdParams.Build(adUnitId:adUnitId, errorCode:errorCode, duration:duration);
            return p.ToAdFailedData();
        }

        private Dictionary<string, object> BuildIadsImpData(MaxSdk.AdInfo adInfo, string adUnitId)
        {
            var p = AdParams.Build(adInfo, adUnitId:adUnitId, category:_iadsCategory);
            return p.ToAdImpData();
        }
        
        private Dictionary<string, object> BuildIadsClickData(MaxSdk.AdInfo adInfo, string adUnitId)
        {
            var p = AdParams.Build(adInfo, adUnitId, category:_iadsCategory);
            return p.ToAdClickData();
        }
        
        private Dictionary<string, object> BuildIadsCloseData(MaxSdk.AdInfo adInfo, string adUnitId)
        {
            int duration = GetActionDuration(_iadsDisplayStartTime);
            var p = AdParams.Build(adInfo, adUnitId, category:_iadsCategory, duration:duration);
            return p.ToAdCloseData();
        }
        
        //---------- RADS Params --------------
        private Dictionary<string, object> BuildRadsLoadedData(MaxSdk.AdInfo adInfo, string adUnitId)
        {
            int duration = GetActionDuration(_radsLoadStartTime);
            var p = AdParams.Build(adInfo, adUnitId, duration:duration);
            return p.ToAdLoadedData();
        }
        
        private Dictionary<string, object> BuildRadsFailedData(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            int duration = GetActionDuration(_radsLoadStartTime);
            int errorCode = (int)errorInfo.Code;
            var p = AdParams.Build(adUnitId:adUnitId, errorCode:errorCode, duration:duration);
            return p.ToAdFailedData();
        }

        private Dictionary<string, object> BuildRadsImpData(MaxSdk.AdInfo adInfo, string adUnitId)
        {
            var p = AdParams.Build(adInfo, adUnitId:adUnitId, category:_radsCategory);
            return p.ToAdImpData();
        }
        
        private Dictionary<string, object> BuildRadsClickData(MaxSdk.AdInfo adInfo, string adUnitId)
        {
            var p = AdParams.Build(adInfo, adUnitId, category:_radsCategory);
            return p.ToAdClickData();
        }
        
        private Dictionary<string, object> BuildRadsCloseData(MaxSdk.AdInfo adInfo, string adUnitId)
        {
            int duration = GetActionDuration(_radsDisplayStartTime);
            var p = AdParams.Build(adInfo, adUnitId, category:_radsCategory, duration:duration);
            return p.ToAdCloseData();
        }
        
        private Dictionary<string, object> BuildRadsRewardedData(MaxSdk.AdInfo adInfo, string adUnitId)
        {
            var p = AdParams.Build(adInfo, adUnitId, category:_radsCategory);
            return p.ToAdRewardedData();
        }


        #endregion
    }
}