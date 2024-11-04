

namespace Guru
{
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;
    using Const=AdStatusConsts;
    
    public partial class AdStatusPresenter
    {
        private Queue<AdStatusInfo> _bannerInfos;
        private Queue<AdStatusInfo> _interInfos;
        private Queue<AdStatusInfo> _rewardInfos;

        private AdStatusInfo _curBadsInfo;
        private AdStatusInfo _curIadsInfo;
        private AdStatusInfo _curRadsInfo;
        
        #region InfoContainer

        /// <summary>
        /// 添加对应的 Info
        /// </summary>
        /// <param name="info"></param>
        private void AddBannerInfo(AdStatusInfo info)
        {
            if (_bannerInfos.Count >= AdStatusConsts.MaxInfoCount)
            {
                _bannerInfos.Dequeue();
            }
            Debug.Log(info.ToLogString());
            _bannerInfos.Enqueue(info);
            _curBadsInfo = info;
            OnStatueChanged(info);
        }
        
        private void AddInterInfo(AdStatusInfo info)
        {
            if (_interInfos.Count >= AdStatusConsts.MaxInfoCount)
            {
                _interInfos.Dequeue();
            }
            _interInfos.Enqueue(info);
            _curIadsInfo = info;
            OnStatueChanged(info);
        }
        
        private void AddRewardInfo(AdStatusInfo info)
        {
            if (_rewardInfos.Count >= AdStatusConsts.MaxInfoCount)
            {
                _rewardInfos.Dequeue();
            }
            _rewardInfos.Enqueue(info);
            _curRadsInfo = info;
            OnStatueChanged(info);
        }




        private void OnStatueChanged(AdStatusInfo info)
        {
            if (_model == null) return;
            _model.monitorInfo = CreateMonitorInfo();
            
            if (info != null)
            {
                int code = 0;
                if (info.status == AdStatusType.LoadFailed || info.status == AdStatusType.DisplayFailed)
                {
                    code = -1;
                }
                else if (info.status == AdStatusType.Loaded)
                {
                    code = 1;
                }
                
                switch (info.adType)
                {
                    case AdType.Banner:
                        if(code == 1) _model.AddBannerCount(true);
                        else if(code == -1) _model.AddBannerCount(false);
                        break;
                    case AdType.Interstitial:
                        if(code == 1) _model.AddInterCount(true);
                        else if(code == -1) _model.AddInterCount(false);
                        break;
                    case AdType.Rewarded:
                        if(code == 1) _model.AddRewardCount(true);
                        else if(code == -1) _model.AddRewardCount(false);
                        break;
                }
            }

            UpdateView(); // 刷新视图
        }

        // 字段缓冲
        private StringBuilder _infoBuff;
        private string CreateMonitorInfo()
        {
            string msg;
            if (ADService.Instance == null || !ADService.Instance.IsInitialized)
            {
                msg = ColoredText("AdService not initialized...", Const.ColorRed);
                return msg;
            }
            
            if (_infoBuff == null) _infoBuff = new StringBuilder();
            _infoBuff.Clear();
            
            if (_curBadsInfo == null)
            {
                msg = $"BADS: {ColoredText("not ready", Const.ColorRed)}\n";
            }
            else
            {

                switch (_curBadsInfo.status)
                {
                    case AdStatusType.Loaded:
                        msg = $"BADS: {ColoredText("loaded", Const.ColorGreen)}\n\tnetwork: {_curBadsInfo.network}\n\twaterfall: {_curBadsInfo.waterfall}\n";
                        break;
                    case AdStatusType.LoadFailed:
                        msg = $"BADS: {ColoredText("loading failed", Const.ColorRed)}\n\tmessage: {_curBadsInfo.info}\n";
                        break;
                    case AdStatusType.DisplayFailed:
                        msg = $"BADS: {ColoredText("display failed", Const.ColorRed)}\n\tmessage: {_curBadsInfo.info}\n";
                        break;  
                    case AdStatusType.Loading:
                        msg = $"BADS: {ColoredText("loading...", Const.ColorYellow)}\n\tformat: {_curBadsInfo.format}\n";
                        break;
                    case AdStatusType.Paid:
                        msg = $"BADS: {ColoredText("display", Const.ColorGreen)}\n\tnetwork: {_curBadsInfo.network}\n\trevenue: {_curBadsInfo.revenue}\n";
                        break;
                    case AdStatusType.NotReady:
                        msg = $"BADS: {ColoredText("not ready", Const.ColorGray)}\n\t{ColoredText("---", Const.ColorGray)}\n";
                        break;
                    default:
                        msg = $"BADS: {ColoredText("other", Const.ColorGray)}\n\tstatus: {ColoredText($"{_curBadsInfo.status}", Const.ColorYellow)}\n";
                        break;
                }
            }
            _infoBuff.Append(msg);


            if (_curIadsInfo == null)
            {
                msg = $"IADS: {ColoredText("not ready", Const.ColorRed)}\n";
            }
            else
            {
                switch (_curIadsInfo.status)
                {
                    case AdStatusType.Loaded:
                        msg = $"IADS: {ColoredText("loaded", Const.ColorGreen)}\n\tnetwork: {_curIadsInfo.network}\n\twaterfall: {_curIadsInfo.waterfall}\n";
                        break;
                    case AdStatusType.LoadFailed:
                        msg = $"IADS: {ColoredText("loading failed", Const.ColorRed)}\n\tmessage: {_curIadsInfo.info}\n";
                        break;
                    case AdStatusType.DisplayFailed:
                        msg = $"IADS: {ColoredText("display failed", Const.ColorRed)}\n\tmessage: {_curIadsInfo.info}\n";
                        break;  
                    case AdStatusType.Loading:
                        msg = $"IADS: {ColoredText("loading...", Const.ColorYellow)}\n\tformat: {_curIadsInfo.format}\n";
                        break;
                    case AdStatusType.Paid:
                        msg = $"IADS: {ColoredText("get revenue", Const.ColorGreen)}\n\trevenue: {_curIadsInfo.revenue}\n";
                        break;
                    case AdStatusType.NotReady:
                        msg = $"IADS: {ColoredText("not ready", Const.ColorGray)}\n\t{ColoredText("---", Const.ColorGray)}\n";
                        break;
                    default:
                        msg = $"IADS: {ColoredText("other", Const.ColorGray)}\n\tstatus: {ColoredText($"{_curIadsInfo.status}", Const.ColorYellow)}\n";
                        break;
                }
            }
            _infoBuff.Append(msg);
            

            if (_curRadsInfo == null)
            {
                msg = $"RADS: {ColoredText("not ready", Const.ColorRed)}\n";
            }
            else
            {
                switch (_curRadsInfo.status)
                {
                    case AdStatusType.Loaded:
                        msg = $"RADS: {ColoredText("loaded", Const.ColorGreen)}\n\tnetwork: {_curRadsInfo.network}\n\twaterfall: {_curRadsInfo.waterfall}\n";
                        break;
                    case AdStatusType.LoadFailed:
                        msg = $"RADS: {ColoredText("loading failed", Const.ColorRed)}\n\tmessage: {_curRadsInfo.info}\n";
                        break;
                    case AdStatusType.DisplayFailed:
                        msg = $"RADS: {ColoredText("display failed", Const.ColorRed)}\n\tmessage: {_curRadsInfo.info}\n";
                        break;  
                    case AdStatusType.Loading:
                        msg = $"RADS: {ColoredText("loading...", Const.ColorYellow)}\n\tformat: {_curRadsInfo.format}\n";
                        break;
                    case AdStatusType.Paid:
                        msg = $"RADS: {ColoredText("get revenue", Const.ColorGreen)}\n\trevenue: {_curRadsInfo.revenue}\n";
                        break;
                    case AdStatusType.NotReady:
                        msg = $"RADS: {ColoredText("not ready", Const.ColorGray)}\n\t{ColoredText("---", Const.ColorGray)}\n";
                        break;
                    default:
                        msg = $"RADS: {ColoredText("other", Const.ColorGray)}\n\tstatus: {ColoredText($"{_curRadsInfo.status}", Const.ColorYellow)}\n";
                        break;
                }
            }
            _infoBuff.Append(msg);
            
            
            return _infoBuff.ToString();

        }


        private string ColoredText(string content, string hexColor = "000000")
        {
            return $"<color=#{hexColor}>{content}</color>";
        }


        #endregion
        
        #region AppLovin

        private void InitAdsAssets()
        {
            RemoveCallbacks();
            AddCallbacks();
            _bannerInfos = new Queue<AdStatusInfo>(AdStatusConsts.MaxInfoCount);
            _interInfos = new Queue<AdStatusInfo>(AdStatusConsts.MaxInfoCount);
            _rewardInfos = new Queue<AdStatusInfo>(AdStatusConsts.MaxInfoCount);
        }


        private void AddCallbacks()
        {
            //----------------- Banner -----------------
            ADService.Instance.OnBannerStartLoad += OnBannerStartLoadEvent;
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            //----------------- Interstitials -----------------
            ADService.Instance.OnInterstitialStartLoad += OnInterStartLoadEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterAdLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterAdLoadFailEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterAdDisplayFailEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterAdRevenuePaidEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterAdClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterAdHiddenEvent;
            //----------------- Reward -----------------
            ADService.Instance.OnRewardedStartLoad += OnRewardedStartLoad;
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardAdLoadFailEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardAdDisplayFailEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardAdHiddenEvent;
        }
        
        private void RemoveCallbacks()
        {
            //----------------- Banner -----------------
            ADService.Instance.OnBannerStartLoad -= OnBannerStartLoadEvent;
            MaxSdkCallbacks.Banner.OnAdLoadedEvent -= OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent -= OnBannerAdLoadFailEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= OnBannerAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent -= OnBannerAdClickedEvent;
            //----------------- Interstitials -----------------
            ADService.Instance.OnInterstitialStartLoad -= OnInterStartLoadEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnInterAdLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnInterAdLoadFailEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= OnInterAdDisplayFailEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= OnInterAdRevenuePaidEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent -= OnInterAdClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= OnInterAdHiddenEvent;
            //----------------- Reward -----------------
            ADService.Instance.OnRewardedStartLoad -= OnRewardedStartLoad;
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnRewardAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnRewardAdLoadFailEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= OnRewardAdDisplayFailEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnRewardAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent -= OnRewardAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnRewardAdHiddenEvent;
        }
        
        //-------------- Banner ------------------
        private void OnBannerStartLoadEvent(string adUnitId)
        {
            AddBannerInfo(CreateLoadingInfo(adUnitId, AdType.Banner));
        }
        
        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var waterfall = adInfo.WaterfallInfo?.Name ?? "";
            AddBannerInfo(CreateLoadedInfo(adUnitId, AdType.Banner, adInfo.NetworkName, adInfo.AdFormat, waterfall));
        }
        
        private void OnBannerAdLoadFailEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            var waterfall = errorInfo.WaterfallInfo?.Name ?? "";
            var format = "BADS";
            var msg = $"[{errorInfo.MediatedNetworkErrorCode}] {errorInfo.MediatedNetworkErrorMessage}";
            var network = "error";
            AddBannerInfo(CreateFailInfo(adUnitId, AdType.Banner, network, format, waterfall, msg));
        }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var waterfall = adInfo.WaterfallInfo?.Name ?? "";
            AddBannerInfo(CreateClosedInfo(adUnitId, AdType.Banner, adInfo.NetworkName, adInfo.AdFormat, waterfall));
        }

        private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var waterfall = adInfo.WaterfallInfo?.Name ?? "";
            AddBannerInfo(CreatePaidInfo(adUnitId, AdType.Banner, adInfo.Revenue, adInfo.NetworkName, adInfo.AdFormat, waterfall));
        }

        
        //----------------- Interstitial -----------------
        private void OnInterStartLoadEvent(string adUnitId)
        {
            AddInterInfo(CreateLoadingInfo(adUnitId, AdType.Interstitial));
        }
        
        private void OnInterAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var waterfall = adInfo.WaterfallInfo?.Name ?? "";
            AddInterInfo(CreateClosedInfo(adUnitId, AdType.Interstitial, adInfo.NetworkName, adInfo.AdFormat, waterfall));
        }

        private void OnInterAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var waterfall = adInfo.WaterfallInfo?.Name ?? "";
            AddInterInfo(CreateClickedInfo(adUnitId, AdType.Interstitial, adInfo.NetworkName, adInfo.AdFormat, waterfall));
        }

        private void OnInterAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var waterfall = adInfo.WaterfallInfo?.Name ?? "";
            AddInterInfo(CreatePaidInfo(adUnitId, AdType.Interstitial, adInfo.Revenue, adInfo.NetworkName, adInfo.AdFormat, waterfall));
        }

        private void OnInterAdLoadFailEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            var waterfall = errorInfo.WaterfallInfo?.Name ?? "";
            var format = "IADS";
            var msg = $"[{errorInfo.MediatedNetworkErrorCode}] {errorInfo.MediatedNetworkErrorMessage}";
            var network = "error";
            AddInterInfo(CreateFailInfo(adUnitId, AdType.Interstitial, network, format, waterfall, msg));
        }
        
        private void OnInterAdDisplayFailEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            var waterfall = errorInfo.WaterfallInfo?.Name ?? "";
            var format = adInfo.AdFormat;
            var msg = $"[{errorInfo.MediatedNetworkErrorCode}] {errorInfo.MediatedNetworkErrorMessage}";
            var network = adInfo.NetworkName;
            AddInterInfo(CreateFailInfo(adUnitId, AdType.Interstitial, network, format, waterfall, msg, true));
        }
        
        private void OnInterAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var waterfall = adInfo.WaterfallInfo?.Name ?? "";
            AddInterInfo(CreateLoadedInfo(adUnitId, AdType.Interstitial, adInfo.NetworkName, adInfo.AdFormat, waterfall));
        }
        
        //----------------- Reward -----------------
        private void OnRewardedStartLoad(string adUnitId)
        {
            AddRewardInfo(CreateLoadingInfo(adUnitId, AdType.Rewarded));
        }
        
        private void OnRewardAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var waterfall = adInfo.WaterfallInfo?.Name ?? "";
            AddRewardInfo(CreateClosedInfo(adUnitId, AdType.Rewarded, adInfo.NetworkName, adInfo.AdFormat, waterfall));
        }

        private void OnRewardAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var waterfall = adInfo.WaterfallInfo?.Name ?? "";
            AddRewardInfo(CreateClickedInfo(adUnitId, AdType.Rewarded, adInfo.NetworkName, adInfo.AdFormat, waterfall));
        }

        private void OnRewardAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var waterfall = adInfo.WaterfallInfo?.Name ?? "";
            AddRewardInfo(CreatePaidInfo(adUnitId, AdType.Rewarded, adInfo.Revenue, adInfo.NetworkName, adInfo.AdFormat, waterfall));
        }

        private void OnRewardAdLoadFailEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            var waterfall = errorInfo.WaterfallInfo?.Name ?? "";
            var format = "IADS";
            var msg = $"[{errorInfo.MediatedNetworkErrorCode}] {errorInfo.MediatedNetworkErrorMessage}";
            var network = "error";
            AddRewardInfo(CreateFailInfo(adUnitId, AdType.Rewarded, network, format, waterfall, msg));
        }
        
        private void OnRewardAdDisplayFailEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            var waterfall = errorInfo.WaterfallInfo?.Name ?? "";
            var format = adInfo.AdFormat;
            var msg = $"[{errorInfo.MediatedNetworkErrorCode}] {errorInfo.MediatedNetworkErrorMessage}";
            var network = adInfo.NetworkName;
            AddRewardInfo(CreateFailInfo(adUnitId, AdType.Rewarded, network, format, waterfall, msg, true));
        }
        
        private void OnRewardAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            var waterfall = adInfo.WaterfallInfo?.Name ?? "";
            AddRewardInfo(CreateLoadedInfo(adUnitId, AdType.Rewarded, adInfo.NetworkName, adInfo.AdFormat, waterfall));
        }
        
        #endregion

        

    }
}