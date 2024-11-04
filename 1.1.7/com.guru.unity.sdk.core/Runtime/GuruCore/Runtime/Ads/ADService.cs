
using System;
using UnityEngine;

namespace Guru
{
	using System.Collections.Generic;
    
    /// <summary>
    /// 通用的ADService
    /// </summary>
    public class ADService: ADServiceBase<ADService>
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public static readonly string Version = "1.6.0";
        
        #region 初始化
        
        /// <summary>
		/// 初始化服务
		/// </summary>
        protected override void InitService()
        {
	        base.InitService();
	        InitChannels(_isDebug);  // 启动各广告渠道代理
        }
		
		#endregion
		
        #region 渠道管理
        
        private HashSet<IAdChannel> _adChannels;
        private AdChanelMax _chanelMax;
        private IAsyncRequestChannel _asyncLoader;
        
        /// <summary>
        /// 各渠道初始化
        /// </summary>
        private void InitChannels(bool isDebug)
        {
        	_adChannels = new HashSet<IAdChannel>();
        	IAdChannel channel = null;
            _asyncLoader = null;
        	
        	_chanelMax = new AdChanelMax(); // 默认持有MAXChannel
            _chanelMax.Initialize(isDebug);
            if(_initSpec != null) _chanelMax.SetBannerBackColor(_initSpec.bannerColorHex);
            
        	//------------ 以下为扩展的广告渠道 ------------------
            // 请根据项目需求实现各渠道接入的逻辑
            // 开启渠道需要添加对应的宏
            
        	channel = new AdChanelAmazon();
        	channel.Initialize(isDebug);
        	_adChannels.Add(channel); // Amazon
            _asyncLoader = channel as IAsyncRequestChannel;
            if (_asyncLoader != null)
            {
	            _asyncLoader.OnBannerRequestOver = (success, firstLoad) =>
	            {
		            Debug.Log($"--- [Amazon] Async banner response, success:{success} -> OnLoadMaxBanner");
		            // if (success && firstLoad) OnLoadMaxBanner();  // 广告频率足够快, 不需要再次调用 
	            };
	            _asyncLoader.OnInterstitialRequestOver = (success, firstLoad) =>
	            {
		            Debug.Log($"--- [Amazon] Async IV response, success:{success} -> OnLoadMaxIV");
		            // if (success && firstLoad) OnLoadMaxIV();
	            };
	            _asyncLoader.OnRewardRequestOver = (success, firstLoad) =>
	            {
		            Debug.Log($"--- [Amazon] Async RV response, success:{success} > OnLoadMaxRV");
		            // if (success && firstLoad) OnLoadMaxRV();
	            };
            }

        	channel = new AdChannelPubMatic();
        	channel.Initialize();
        	_adChannels.Add(channel);	// PubMatic

#if UNITY_EDITOR
	        Debug.Log($"---- Editor Ads Init Over ----");
	        _onSdkInitReady?.Invoke();
#endif
        }
        #endregion

        #region Banner 广告

        
        /// <summary>
        /// 加载Banner广告
        /// </summary>
        public override void RequestBannerAD()
        {
	        if (!IsInitialized) return;
	        OnChannelLoadBannerAD(); // 调用各渠道请求Banner
        }
        
        
        /// <summary>
        /// 请求Banner广告
        /// </summary>
        private void OnChannelLoadBannerAD()
        {
	        IAsyncRequestChannel loader = null;
        	foreach (var channel in _adChannels)
        	{
        		channel?.LoadBannerAD();
            }

            // if (null == _asyncLoader) 
	        OnLoadMaxBanner();
        }
        
        private void OnLoadMaxBanner()
        {
	        _badsLoadStartTime = DateTime.UtcNow;
	        _chanelMax.LoadBannerAD();
	        OnBannerStartLoad?.Invoke(_chanelMax.MaxBADSSlotID);
        }

        #endregion

        #region Interstitial 广告


        public override void RequestInterstitialAD()
        {
	        if (!IsInitialized) return;
	        if (IsIadsLoading) return;
	        OnChannelLoadInterstitialAD();
        }


        /// <summary>
        /// 请求插屏广告
        /// </summary>
        private void OnChannelLoadInterstitialAD()
        {
        	foreach (var channel in _adChannels)
        	{
        		channel?.LoadInterstitialAD();
        	}
            // if (_asyncLoader == null) 
	        OnLoadMaxIV();
        }

        private void OnLoadMaxIV() {
	        _iadsLoadStartTime = DateTime.UtcNow; // 更新计时器
	        _chanelMax.LoadInterstitialAD();
	        OnInterstitialStartLoad?.Invoke(_chanelMax.MaxIADSSlotID);
        }
        
        
        #endregion

        #region Reward 广告

        public override void RequestRewardedAD()
        {
	        if (!IsInitialized) return;
	        if (IsRadsLoading) return;
	        OnChannelLoadRewardAD();
        }
        
        /// <summary>
        /// 请求激励视频
        /// </summary>
        private void OnChannelLoadRewardAD()
        {
	        // 各 Channel 都去加载激励视频
        	foreach (var channel in _adChannels)
        	{
        		channel?.LoadRewardAD();
        	}
            // 最后 Max 渠道进行加载 
	        OnLoadMaxRV();
        }
        
        private void OnLoadMaxRV()
        {
	        _radsLoadStartTime = DateTime.UtcNow; // 更新计时器
	        _chanelMax.LoadRewardAD();
	        OnRewardedStartLoad?.Invoke(_chanelMax.MaxRADSSlotID);
        }
    
        

        #endregion
        
    }
}