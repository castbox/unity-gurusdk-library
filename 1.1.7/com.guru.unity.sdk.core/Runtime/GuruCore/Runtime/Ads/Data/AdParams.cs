
namespace Guru
{
	using System.Collections.Generic;

	
    /// <summary>
    /// 广告打点上报参数
    /// </summary>
    public class AdParams
    {
	    private const int MAX_VALUE_LENGTH = 96;
	    private const string VALUE_NOT_SET = "not_set";
	    
	    
	    public double value;
	    public string currency;
	    public string category;
	    public string networkPlacement;
	    public string adPlatform;
	    public string adSource;
	    public string adFormat;
	    public string adUnitId;
	    public int errorCode = 0; 
	    public int duration = 0;
	    public string creativeId;
	    public string reviewCreativeId;
	    
	    /// <summary>
	    /// 构造AD参数
	    /// </summary>
	    /// <param name="adInfo"></param>
	    /// <param name="adUnitId"></param>
	    /// <param name="category"></param>
	    /// <param name="duration"></param>
	    /// <param name="errorCode"></param>
	    /// <param name="reviewCreativeId"></param>
	    /// <param name="adPlatform"></param>
		/// <param name="currency"></param>
	    /// <returns></returns>
	    public static AdParams Build(MaxSdkBase.AdInfo adInfo = null, string adUnitId = "", string category = "",
		    int duration = 0, int errorCode = 0, string reviewCreativeId = "", string adPlatform = "", string currency = "")
	    {
		    if (string.IsNullOrEmpty(adUnitId) && adInfo != null) adUnitId = adInfo.AdUnitIdentifier;
		    var networkPlacement = "";
		    var creativeId = "";
		    var adSource = "";
		    var adFormat = "";
		    double value = 0;
		    // string waterfallName = "";
		    
		    if (adInfo != null)
		    {
			    value = adInfo.Revenue;
			    networkPlacement = adInfo.NetworkPlacement;
			    creativeId = adInfo.CreativeIdentifier;
			    adSource = adInfo.NetworkName;
			    adFormat = adInfo.AdFormat;
		    }
		    
		    if (string.IsNullOrEmpty(adPlatform)) adPlatform = Analytics.AdMAX;
		    if (string.IsNullOrEmpty(category)) category = VALUE_NOT_SET;
		    if (string.IsNullOrEmpty(currency)) currency = Analytics.USD;
		    if (string.IsNullOrEmpty(creativeId))
		    {
			    creativeId = VALUE_NOT_SET;
		    }
		    else if(creativeId.Length > MAX_VALUE_LENGTH)
		    {
			    creativeId = creativeId.Substring(0, MAX_VALUE_LENGTH);
		    }
		    
		    if (!string.IsNullOrEmpty(reviewCreativeId) 
		        && reviewCreativeId.Length > MAX_VALUE_LENGTH)
		    {
			    reviewCreativeId = reviewCreativeId.Substring(0, MAX_VALUE_LENGTH);
		    }

		    var p = new AdParams()
		    {
			    value = value,
			    currency = currency,
			    adUnitId = adUnitId,
			    adPlatform = adPlatform,
			    adSource = adSource,
			    adFormat = adFormat,
			    duration = duration,
			    networkPlacement = networkPlacement,
			    category = category,
			    errorCode = errorCode,
			    creativeId = creativeId,
			    reviewCreativeId = reviewCreativeId,
		    };
		    return p;
	    }


	    /// <summary>
	    /// 转化为 AdImpression 事件数据
	    /// </summary>
	    /// <returns></returns>
	    public Dictionary<string, object> ToAdImpressionData()
	    {
		    var data = new Dictionary<string, object>()
		    {
				[Analytics.ParameterValue] = value,
				[Analytics.ParameterCurrency] = currency,
				[Analytics.ParameterAdPlatform] = adPlatform,
				[Analytics.ParameterAdSource] = adSource,
				[Analytics.ParameterAdFormat] = adFormat,
				[Analytics.ParameterAdUnitName] = adUnitId,
				[Analytics.ParameterAdPlacement] = networkPlacement,
				[Analytics.ParameterAdCreativeId] = creativeId,
		    };

		    if (!string.IsNullOrEmpty(reviewCreativeId))
			    data[Analytics.ParameterReviewCreativeId] = reviewCreativeId;
		    return data;
	    }
	    
	    
	    /// <summary>
	    /// 转化为 ads_imp 事件数据
	    /// </summary>
	    /// <returns></returns>
	    public Dictionary<string, object> ToAdImpData()
	    {
		    var data = new Dictionary<string, object>()
		    {
			    [Analytics.ParameterAdPlatform] = adPlatform,
			    [Analytics.ParameterAdSource] = adSource,
			    [Analytics.ParameterAdUnitName] = adUnitId,
			    [Analytics.ParameterAdPlacement] = networkPlacement,
			    [Analytics.ParameterAdCreativeId] = creativeId,
			    [Analytics.ParameterItemCategory] = category,
		    };

		    if (!string.IsNullOrEmpty(reviewCreativeId))
			    data[Analytics.ParameterReviewCreativeId] = reviewCreativeId;
		    return data;
	    }
	    
	    /// <summary>
	    /// 转化为 ads_paid 通用事件数据
	    /// </summary>
	    /// <returns></returns>
	    public Dictionary<string, object> ToAdPaidData()
	    {
		    var data = new Dictionary<string, object>()
		    {
			    [Analytics.ParameterValue] = value,
			    [Analytics.ParameterCurrency] = currency,
			    [Analytics.ParameterAdPlatform] = adPlatform,
			    [Analytics.ParameterAdSource] = adSource,
			    [Analytics.ParameterAdUnitName] = adUnitId,
			    [Analytics.ParameterAdPlacement] = networkPlacement,
			    [Analytics.ParameterAdCreativeId] = creativeId,
			    [Analytics.ParameterItemCategory] = category,
		    };

		    if (!string.IsNullOrEmpty(reviewCreativeId))
			    data[Analytics.ParameterReviewCreativeId] = reviewCreativeId;
		    return data;
	    }
	    
	    /// <summary>
	    /// 转化为 ads_paid 通用事件数据
	    /// </summary>
	    /// <returns></returns>
	    public Dictionary<string, object> ToAdRewardedData()
	    {
		    var data = new Dictionary<string, object>()
		    {
			    [Analytics.ParameterValue] = value,
			    [Analytics.ParameterCurrency] = currency,
			    [Analytics.ParameterAdPlatform] = adPlatform,
			    [Analytics.ParameterAdSource] = adSource,
			    [Analytics.ParameterAdUnitName] = adUnitId,
			    [Analytics.ParameterAdPlacement] = networkPlacement,
			    [Analytics.ParameterAdCreativeId] = creativeId,
			    [Analytics.ParameterItemCategory] = category,
		    };

		    if (!string.IsNullOrEmpty(reviewCreativeId))
			    data[Analytics.ParameterReviewCreativeId] = reviewCreativeId;
		    return data;
	    }
	    
	    public Dictionary<string, object> ToAdClickData()
	    {
		    var data = new Dictionary<string, object>()
		    {
			    [Analytics.ParameterValue] = value,
			    [Analytics.ParameterCurrency] = currency,
			    [Analytics.ParameterAdPlatform] = adPlatform,
			    [Analytics.ParameterAdSource] = adSource,
			    [Analytics.ParameterAdUnitName] = adUnitId,
			    [Analytics.ParameterAdPlacement] = networkPlacement,
			    [Analytics.ParameterAdCreativeId] = creativeId,
			    [Analytics.ParameterItemCategory] = category,
		    };

		    if (!string.IsNullOrEmpty(reviewCreativeId))
			    data[Analytics.ParameterReviewCreativeId] = reviewCreativeId;
		    return data;
	    }
	    
	    public Dictionary<string, object> ToAdCloseData()
	    {
		    var data = new Dictionary<string, object>()
		    {
			    [Analytics.ParameterItemCategory] = category,
			    [Analytics.ParameterDuration] = duration,
		    };
		    return data;
	    }
	    
	    /// <summary>
	    /// 广告失败事件数据
	    /// </summary>
	    /// <returns></returns>
	    public Dictionary<string, object> ToAdFailedData()
	    {
		    var data = new Dictionary<string, object>()
		    {
			    [Analytics.ParameterItemCategory] = category,
				[Analytics.ParameterItemName] = adUnitId,
				[Analytics.ParameterErrorCode] = errorCode,
				[Analytics.ParameterDuration] = duration,
		    };
		    return data;
	    }
	    
	    /// <summary>
	    /// 广告加载成功事件数据
	    /// </summary>
	    /// <returns></returns>
	    public Dictionary<string, object> ToAdLoadData()
	    {
		    var data = new Dictionary<string, object>()
		    {
			    [Analytics.ParameterItemCategory] = category,
			    [Analytics.ParameterItemName] = adUnitId,
		    };
		    return data;
	    }
	    
	    /// <summary>
	    /// 广告加载成功事件数据
	    /// </summary>
	    /// <returns></returns>
	    public Dictionary<string, object> ToAdLoadedData()
	    {
		    var data = new Dictionary<string, object>()
		    {
			    [Analytics.ParameterItemCategory] = category,
			    [Analytics.ParameterItemName] = adUnitId,
			    [Analytics.ParameterDuration] = duration,
		    };
		    return data;
	    }
	    
	    
	    public Dictionary<string, object> ToBadsImpData()
	    {
		    var data = new Dictionary<string, object>()
		    {
			    [Analytics.ParameterItemCategory] = category,
			    [Analytics.ParameterItemName] = adUnitId,
		    };
		    return data;
	    }
    }
}