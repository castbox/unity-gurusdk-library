


namespace Guru.Ads.Max
{
    using System;
    using UnityEngine;
    using Guru.Ads;
    using System.Collections.Generic;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    
    /// <summary>
    /// Max Banner 广告代理
    /// </summary>
    public class MaxBannerLoader
    {
        
        private const int BANNER_RELOAD_SECONDS = 30; // 自动重试加载间隔
        
        private readonly Color _backColor;
        private readonly float _width;
        private readonly MaxCustomLoaderAmazon _customLoaderAmazon;
        private readonly IAdEventObserver _eventObserver; // 广告事件监听器

        private string _maxAdUnitId;
        private bool _isBannerVisible; // Banner 是否可见
        private bool _autoRefresh;
        private bool _hasBannerCreated = false; // Banner 是否被创建
        private int _loadedTimes;
        private int _failedTimes;
        private DateTime _adStartLoadTime;
        private string _adPlacement;
        private readonly string _tag;
        private bool _shouldReportImpEvent;
        private CancellationTokenSource _retryLoadCts;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="width"></param>
        /// <param name="colorHexStr"></param>
        /// <param name="customLoaderAmazon"></param>
        /// <param name="observer"></param>
        /// <param name="adUnitId"></param>
        public MaxBannerLoader(string adUnitId, float width, string colorHexStr, MaxCustomLoaderAmazon customLoaderAmazon, IAdEventObserver observer)
        {
            _hasBannerCreated = false;
            _maxAdUnitId = adUnitId;
            _backColor = MaxAdHelper.HexToColor(colorHexStr);
            _width = width;
            _customLoaderAmazon = customLoaderAmazon;
            _eventObserver = observer;
            _tag = AdConst.LOG_TAG_MAX;
            
            // --- Add Callbacks ---
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnAdsLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdPaidEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnAdsCollapsedEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnAdsExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdReviewCreativeIdGeneratedEvent += OnAdsReviewCreativeIdGeneratedEvent;
        }


        public bool IsBannerVisible
        {
            get => _isBannerVisible;
            set
            {
                _isBannerVisible = value;
                SetAutoRefresh(value);
            }
        }

        /// <summary>
        /// 设置已购买去广告
        /// </summary>
        public void SetBuyNoAds()
        {
            // 取消加载重试
            Hide();
        }

        public Rect GetBannerLayout()
        {
            return MaxSdk.GetBannerLayout(_maxAdUnitId);
        }
        
        /// <summary>
        /// 设置 BANNER AdUnitID
        /// </summary>
        /// <param name="adUnitId"></param>
        public void SetAdUnitId(string adUnitId) => _maxAdUnitId = adUnitId;

        public void SetAutoRefresh(bool value)
        {
            _autoRefresh = value;
            if (value)
            {
                MaxSdk.StartBannerAutoRefresh(_maxAdUnitId); // 开启 Banner 的自动刷新
            }
            else
            {
                MaxSdk.StopBannerAutoRefresh(_maxAdUnitId);
            }
        }
        
        public bool Enabled
        {
            set
            {
                if (value)
                {
                    Load();
                }
                else
                {
                    Disable();
                }
            }
        }

        private void CreateBannerIfNotExists()
        {
            if(_hasBannerCreated) return;
            _customLoaderAmazon.RequestAPSBanner(CreateMaxBanner);
        }

        private void CreateMaxBanner()
        {
            MaxSdk.CreateBanner(_maxAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
            MaxSdk.SetBannerExtraParameter(_maxAdUnitId, "adaptive_banner", "false");
            // Set background or background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(_maxAdUnitId, _backColor);
            // MaxSdk.StartBannerAutoRefresh(_maxAdUnitId);
            if (_width > 0)
            {
                // 可由外部设置 Banner 宽度
                MaxSdk.SetBannerWidth(_maxAdUnitId, _width);
            }
            _hasBannerCreated = true;
            Debug.Log($"{_tag} --- BADS created: {_maxAdUnitId}");
        }


        /// <summary>
        /// 加载 Banner
        /// </summary>
        public void Load()
        {
            CreateBannerIfNotExists();
            _adStartLoadTime = DateTime.UtcNow;

            // 加载广告
            MaxSdk.LoadBanner(_maxAdUnitId);
            
            // 广告加载
            var e = MaxAdEventBundleFactory.BuildBadsLoad(_maxAdUnitId, _adPlacement);
            _eventObserver.OnEventBadsLoad(e);
            Debug.Log($"{_tag} --- BADS Load: {_maxAdUnitId}");
        }

        /// <summary>
        /// 显示 Banner
        /// </summary>
        /// <param name="placement"></param>
        public void Show(string placement = "")
        {
            _adPlacement = placement;

            if (IsBannerVisible) return;
            
            // 显示广告
            MaxSdk.ShowBanner(_maxAdUnitId);
            MaxSdk.SetBannerPlacement(_maxAdUnitId, _adPlacement);
            SetAutoRefresh(true); // 开启 Banner 的自动刷新
            
            // 数据清零
            _loadedTimes = 0;
            _failedTimes = 0;
            _shouldReportImpEvent = true;
            IsBannerVisible = true;
            
            Load();
            Debug.Log($"{_tag} --- BADS Show: {_maxAdUnitId}");
        }
        
        private void ReportBadsImpEvent()
        {
            var e = MaxAdEventBundleFactory.BuildBadsImp(_maxAdUnitId, _adPlacement);
            _eventObserver.OnEventBadsImp(e);
        }
        
        /// <summary>
        /// 隐藏 Banner
        /// </summary>
        public void Hide()
        {
            // 停止广告刷新
            SetAutoRefresh(false);
            MaxSdk.HideBanner(_maxAdUnitId); // 关闭 Banner 的自动刷新

            if (!IsBannerVisible) return;
            
            CancelRetryLoadCts(); // 取消重试加载
            
            IsBannerVisible = false;
            // 广告隐藏
            var e = MaxAdEventBundleFactory.BuildBadsHide(_loadedTimes, _failedTimes);
            _eventObserver.OnEventBadsHide(e);
            Debug.Log($"{_tag} --- BADS Hide: {_maxAdUnitId}");
        }
        

        /// <summary>
        /// Banner 加载成功
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="adInfo"></param>
        private void OnAdsLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // 加载成功
            var e = MaxAdEventBundleFactory.BuildBadsLoaded(adUnitId, _adPlacement, _adStartLoadTime, adInfo);
            _eventObserver.OnEventBadsLoaded(e);
            // 数据更新
            _loadedTimes++;
            
            _adStartLoadTime = DateTime.UtcNow;
            Debug.Log($"{_tag} --- BADS loaded {adUnitId} -> WaterfallName: {(adInfo.WaterfallInfo?.Name ?? "NULL")}  TestName: {(adInfo.WaterfallInfo?.TestName ?? "NULL")}");

            // 如果加载成功后，发现 Banner 未展示，则尝试展示
            if (_shouldReportImpEvent)
            {
                _shouldReportImpEvent = false;
                ReportBadsImpEvent();
            }

            // 取消正在进行的重试加载任务
            CancelRetryLoadCts();
        }
        
        /// <summary>
        /// Banner 加载失败
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="errorInfo"></param>
        private void OnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _failedTimes++;
            // 加载失败
            var e= MaxAdEventBundleFactory.BuildBadsFailed(adUnitId, _adPlacement, errorInfo, _adStartLoadTime);
            _eventObserver.OnEventBadsFailed(e);
            // 刷新时间
            _adStartLoadTime = DateTime.UtcNow;
            
            Debug.Log($"{_tag} --- BADS load failed -> Failed count:{_failedTimes}  Info:{errorInfo.AdLoadFailureInfo}  autoRefresh:{_autoRefresh}");
            ReloadBannerAsync();
        }


        /// <summary>
        /// 异步重新加载 Banner
        /// </summary>
        private async void ReloadBannerAsync()
        {
            // 如果 Banner 没有显示则不做处理
            if (!IsBannerVisible) return;
            Debug.Log($"{_tag} --- BADS ReloadBannerAsync: {_maxAdUnitId}");
            
            try 
            {
                _retryLoadCts = new CancellationTokenSource();
                while (IsBannerVisible)
                {
                    // 没有网络
                    while (Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        // Debug.Log($"{_tag} --- Bads try Reload but no network: {Application.internetReachability}");
                        await UniTask.Delay(TimeSpan.FromSeconds(AdConst.NO_NETWORK_WAITING_TIME));
                    }
                    
                    // 等待自动重试加载
                    await UniTask.Delay(TimeSpan.FromSeconds(BANNER_RELOAD_SECONDS), cancellationToken: _retryLoadCts.Token);
                    
                    Load();
                    Debug.Log($"{_tag} --- BADS LoadFailHandler immediate with id: {_maxAdUnitId}");
                    break;
                }
                // Debug.Log($"{_tag} --- BADS ReloadBannerAsync over");
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    Debug.Log($"{_tag} --- BADS LoadFailHandler cancelled: {_maxAdUnitId}");
                }
                else
                {
                    Debug.LogError($"{_tag} --- BADS LoadFailHandler with Error: {ex.Message}");
                }
            }
            finally
            {
                // Debug.Log($"{_tag} --- BADS Reload Dispose: {_retryLoadCts.Token}");
                _retryLoadCts?.Dispose();
                _retryLoadCts = null;
            }
        }



        private void CancelRetryLoadCts()
        {
            if (_retryLoadCts == null || _retryLoadCts.IsCancellationRequested) return;
            _retryLoadCts.Cancel();
            _retryLoadCts.Dispose();
            _retryLoadCts = null;
        }


        private void OnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // No Callback so no Implemention
        }
        
        /// <summary>
        /// Banner 被点击
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="adInfo"></param>
        private void OnAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // 广告点击
            var e = MaxAdEventBundleFactory.BuildBadsClick(adUnitId, adInfo.Placement);
            _eventObserver.OnEventBadsClick(e);
            
            Debug.Log($"{_tag} --- BADS Click: {_maxAdUnitId}");
        }
        
        /// <summary>
        /// Banner 收益
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="adInfo"></param>
        private void OnAdPaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (!IsBannerVisible) return; // Banner 隐藏时不上报广告事件
            // 广告收益
            var e = MaxAdEventBundleFactory.BuildBadsPaid(adUnitId, adInfo, _adPlacement);
            _eventObserver.OnEventBadsPaid(e);
        }



        /// <summary>
        /// Banner 获取 reviewCreativeId
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="reviewCreativeId"></param>
        /// <param name="adInfo"></param>
        private void OnAdsReviewCreativeIdGeneratedEvent(string adUnitId, string reviewCreativeId, MaxSdkBase.AdInfo adInfo)
        {
            UnityEngine.Debug.Log($"{_tag} --- BADS get rcid::{reviewCreativeId}");
        }

        private void OnAdsCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            UnityEngine.Debug.Log($"{_tag} --- BADS Collapsed: {adUnitId}");
        }
        
        private void OnAdsExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            UnityEngine.Debug.Log($"{_tag} --- BADS Expanded: {adUnitId}");
        }

       

        /// <summary>
        /// 消除 Banner
        /// 去广告后请调用消除 Banner
        /// </summary>
        /// <param name="adUnitId"></param>
        private void Disable()
        {
            Hide();
            _isBannerVisible = false;
            MaxSdk.DestroyBanner(_maxAdUnitId);
            _hasBannerCreated = false;
        }


    }
}