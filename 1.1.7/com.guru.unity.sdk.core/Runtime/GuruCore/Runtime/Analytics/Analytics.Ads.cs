

namespace Guru
{
	using System.Collections.Generic;
	using UnityEngine;
	
    public partial class Analytics
    {
	    #region 广告属性
	    //------ 默认值 -------
	    public const string DefaultCategory = "not_set";
	    public const string DefaultWaterfall = "unknown";
	    
	    
	    #endregion
	    
	    #region Ads
	    
		//---------------------- BANNER -------------------------
	    public static void TrackAdBadsLoad(Dictionary<string, object> data)
	    {
		    TrackEvent(EventBadsLoad, data);
	    }
	    public static void TrackAdBadsLoaded(Dictionary<string, object> data)
	    {
		    TrackEvent(EventBadsLoaded, data);
	    }
	    public static void TrackAdBadsFailed(Dictionary<string, object> data)
	    {
		    TrackEvent(EventBadsFailed, data);
	    }
	    /// <summary>
	    /// 广告点击
	    /// </summary>
		public static void TrackAdBadsClick(Dictionary<string, object> data)
	    {
		    TrackEvent(EventBadsClick, data);
	    }
	    public static void TrackAdBadsImp(Dictionary<string, object> data)
	    {
		    TrackEvent(EventBadsImp, data);
	    }
	    
	    public static void TrackAdBadsHide( int loadedTimes, int failTimes)
	    {
		    var dict = new Dictionary<string, object>()
		    {
			    ["loaded_times"] = loadedTimes,
			    ["fail_times"] = failTimes
		    };
		    TrackEvent(EventBadsHide, dict);
	    }
		//---------------------- INTERSTITIAL -------------------------
		/// <summary>
		/// 广告拉取
		/// </summary>
		public static void TrackAdIadsLoad(Dictionary<string, object> data)
		{
			TrackEvent(EventIadsLoad, data);
		}

		/// <summary>
		/// 广告拉取成功
		/// </summary>
		public static void TrackAdIadsLoaded(Dictionary<string, object> data)
		{
			TrackEvent(EventIadsLoaded, data);
		}

		/// <summary>
		/// 广告拉取失败
		/// </summary>
		public static void TrackAdIadsFailed(Dictionary<string, object> data)
		{
			TrackEvent(EventIadsFailed, data);
		}

		/// <summary>
		/// 广告展示
		/// </summary>
		public static void TrackAdIadsImp(Dictionary<string, dynamic> data)
		{
			TrackEvent(EventIadsImp, data);
		}

		/// <summary>
		/// 广告点击
		/// </summary>
		public static void TrackAdIadsClick(Dictionary<string, object> data)
		{
			TrackEvent(EventIadsClick, data);
		}

		/// <summary>
		/// 用户关闭广告
		/// </summary>
		public static void TrackAdIadsClose(Dictionary<string, object> data)
		{
			TrackEvent(EventIadsClose, data);
		}
		
		/// <summary>
		/// 插屏广告收到奖励
		/// </summary>
		/// <param name="data"></param>
		public static void TrackAdIadsPaid(Dictionary<string, object> data)
		{
			TrackEvent(EventIadsPaid, data);
		}
		
	    //---------------------- REWARDS -------------------------
		/// <summary>
		/// 广告开始加载
		/// </summary>
		public static void TrackAdRadsLoad(Dictionary<string, object> data)
		{
			TrackEvent(EventRadsLoad, data);
		}
		/// <summary>
		/// 广告拉取成功
		/// </summary>
		public static void TrackAdRadsLoaded(Dictionary<string, object> data)
		{
			TrackEvent(EventRadsLoaded, data);
		}
		/// <summary>
		/// 广告拉取失败
		/// </summary>
		public static void TrackAdRadsFailed(Dictionary<string, object> data)
		{
			TrackEvent(EventRadsFailed, data);
		}
		/// <summary>
		/// 广告展示
		/// </summary>
		public static void TrackAdRadsImp(Dictionary<string, dynamic> data)
		{
			TrackEvent(EventRadsImp, data);
		}
		
		
		
		/// <summary>
		/// 广告完成观看发奖励
		/// </summary>
		public static void TrackAdRadsRewarded(Dictionary<string, object> data)
		{
			TrackEvent(EventRadsRewarded, data);
			
			if (RadsRewardCount == 0)
			{
				RadsRewardCount = 1;
				TrackAdRadsFirstRewarded(data);
			}
		}
		
		/// <summary>
		/// 插屏广告收到奖励
		/// </summary>
		/// <param name="data"></param>
		public static void TrackAdRadsPaid(Dictionary<string, object> data)
		{
			TrackEvent(EventRadsPaid, data);
		}
		
		private static int RadsRewardCount
		{
			get => PlayerPrefs.GetInt(nameof(RadsRewardCount), 0);
			set => PlayerPrefs.SetInt(nameof(RadsRewardCount), value);
		}
		
		/// <summary>
		/// 用户首次完成观看
		/// </summary>
		private static void TrackAdRadsFirstRewarded(Dictionary<string, object> data)
		{
			TrackEvent(EventFirstRadsRewarded, data);
		}

		/// <summary>
		/// 广告点击
		/// </summary>
		public static void TrackAdRadsClick(Dictionary<string, object> data)
		{
			TrackEvent(EventRadsClick, data);
		}

		/// <summary>
		/// 用户关闭广告
		/// </summary>
		public static void TrackAdRadsClose(Dictionary<string, dynamic> data)
		{
			TrackEvent(EventRadsClose, data);
		}
	    #endregion
        	
	    #region Ads-ATT

#if UNITY_IOS

        /// <summary>
        /// ATT 结果打点
        /// </summary>
        /// <param name="status"></param>
        /// <param name="channel"></param>
        /// <param name="scene"></param>
        public static void AttResult(string status, string type = "custom", string scene = "")
        {
	        SetAttProperty(status);
	        Debug.Log($"{TAG} AttResult: {status}    type:{type}    others:{scene}");
	        var dict = new Dictionary<string, dynamic>()
	        {
		        { ParameterItemCategory, status },
		        { "type", type }
	        };
	        if(!string.IsNullOrEmpty(scene)) dict[ParameterItemName] = scene;
        }

        /// <summary>
        /// 上报 ATT 当前的属性
        /// </summary>
        /// <param name="status"></param>
        public static void SetAttProperty(string status)
        {
	        Debug.Log($"{TAG} SetAttProperty: {status}");
            SetUserProperty(PropertyAttStatus, status);
        }
        
#endif

	    #endregion
    }
    
    
    
}