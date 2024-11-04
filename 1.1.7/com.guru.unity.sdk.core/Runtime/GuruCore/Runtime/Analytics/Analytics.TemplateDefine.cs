

using System.Linq;

namespace Guru
{
	using System;
	using System.Collections.Generic;
	using Facebook.Unity;
	using UnityEngine;
	
	//游戏通用模版打点定义
    public partial class Analytics
    {
	    #region 游戏通用打点

	    /// <summary>
	    /// 当玩家在游戏中升级时触发
	    /// </summary>
	    /// <param name="level">level （等级）从1开始 标准点</param>
	    /// <param name="character">升级的角色，如果没有可不选</param>
	    /// <param name="extra"></param>
	    public static void LevelUp(int level, string character, Dictionary<string, object> extra = null)
        {
	        var dict = new Dictionary<string, object>()
	        {
		        { ParameterLevel, level },
		        { ParameterCharacter, character }
	        };
	        if (extra != null) dict.AddRange(extra, isOverride:true);
	        
            TrackEvent(EventLevelUp, dict);
        }
        
        /// <summary>
        /// 玩家已完成解锁成就时触发。
        /// </summary>
        /// <param name="achievementID">这里的成就ID值项目方自行定义</param>
        public static void UnlockAchievement(string achievementID, Dictionary<string, object> extra = null)
        {
	        var dict = new Dictionary<string, object>()
	        {
		        { ParameterAchievementId, achievementID },
	        };
	        if (extra != null) dict.AddRange(extra, isOverride:true);
	        
            TrackEvent(EventUnlockAchievement, dict);
        }
        
        /// <summary>
		/// 玩家已开始挑战某个关卡时触发。
        /// </summary>
		/// <param name="levelName">关卡名称</param>
		/// <param name="level">关卡数</param>
		[Obsolete("Obsolete method, please use <LogLevelStart> instead. will be discard in next version.")]
		public static void LevelStart(int level)
		{
			TrackEvent(EventLevelStart, new Dictionary<string, object>()
			{
				{ ParameterLevel, level },
				{ ParameterItemCategory, "main" },
			});
		}

        /// <summary>
        /// 玩家已开始挑战某个关卡时触发。
        /// </summary>
        /// <param name="level">关卡数</param>
        /// <param name="levelName">关卡名称</param>
        /// <param name="levelType">关卡类型</param>
        /// <param name="itemId">关卡配置表 ID</param>
        /// <param name="startType">启动方式</param>
        /// <param name="isReplay">是否是重玩</param>
        /// <param name="extra">额外数据</param>
        public static void LogLevelStart(int level, string levelName, 
	        string levelType = "main", string itemId = "", string startType = "play", bool isReplay = false, 
	        Dictionary<string, object> extra = null)
        {
	        Dictionary<string, object> dict = new Dictionary<string, object>()
	        {
		        { ParameterLevel, level },
		        { ParameterLevelName, levelName },
		        { ParameterItemCategory, levelType },
		        { ParameterStartType, startType },
		        { ParameterReplay, isReplay ? "true" : "false" },
	        };
	        
	        if(!string.IsNullOrEmpty(itemId))
		        dict[ParameterItemId] = itemId;
            
	        if (extra != null)
	        {
		        dict.AddRange(extra, isOverride:true);
	        }
	        
	        TrackEvent(EventLevelStart, dict);
        }
        
		/// <summary>
		/// 关卡结束(Firebase标准事件)
		/// </summary>
		/// <param name="level"></param>
		/// <param name="result"></param>
		/// <param name="levelName"></param>
		/// <param name="levelType"></param>
		/// <param name="itemId"></param>
		/// <param name="duration"></param>
		/// <param name="step"></param>
		/// <param name="score"></param>
		/// <param name="extra"></param>
		public static void LogLevelEnd(int level, string result, 
			string levelName = "", string levelType = "main", string itemId = "", 
			int duration = 0, int? step = null, int? score = null, Dictionary<string, object> extra = null)
		{
			bool isSuccess = result.Equals("success");

			var dict = new Dictionary<string, object>()
			{
				[ParameterLevel] = level,
				[ParameterLevelName] = levelName,
				[ParameterItemCategory] = levelType,
				[ParameterSuccess] = isSuccess ? "true" : "false",
				[ParameterResult] = result,
				[ParameterDuration] = duration
			};

			if(!string.IsNullOrEmpty(itemId))
				dict[ParameterItemId] = itemId;
			if(step != null)
				dict[ParameterStep] = step.Value;
			if(score != null)
				dict[ParameterScore] = score.Value;

			if(extra != null) dict.AddRange(extra, isOverride:true);
			
			TrackEvent(EventLevelEnd, dict);

			// if (isSuccess)
			// {
			// 	int lv = BPlay;
			// 	if (lv == 0) lv = level;
			// 	LevelEndSuccess(lv, levelType, itemId);
			// }
		}


		/// <summary>
		/// 新用户通过第几关（仅记录前n关,根据项目自行确定，不区分关卡类型）[买量用]
		/// </summary>
		/// <param name="level">总计完成的管卡数 (b_play)</param>
		/// <param name="data"></param>
		public static void TrackLevelEndSuccessEvent(LevelEndSuccessEventData eventData)
		{
			if (eventData.level > GuruSettings.Instance.AnalyticsSetting.LevelEndSuccessNum)
				return;
			TrackEvent(eventData.eventName, eventData.eventData, eventData.eventSetting, eventData.priority);
		}

		/// <summary>
		/// 第一次通关打点
		/// </summary>
		public static void LevelFirstEnd(string levelType, string levelName, int level, 
			string result, int duration = 0, Dictionary<string, object> extra = null)
		{
			var dict = new Dictionary<string, object>()
			{
				{ ParameterItemCategory, levelType },
				{ ParameterLevelName, levelName },
				{ ParameterLevel, level },
				{ ParameterSuccess, result == "success" ? 1 : 0 },
				{ ParameterResult, result },
			};

			if (duration > 0)
				dict[ParameterDuration] = duration;
			if (extra != null)
				dict.AddRange(extra, isOverride: true);
			
			TrackEvent(EventLevelFirstEnd, dict);
		}
		
	    #endregion

	    #region Coins
	    /// <summary>
		/// 当用户获取了虚拟货币（金币、宝石、代币等）时触发
		/// </summary>
		/// <param name="virtual_currency_name">虚拟货币的名称</param>
		/// <param name="value">虚拟货币的数量</param>
		/// <param name="item_category">金币获取的方式，通过IAP购买的方式，固定使用<iap_buy>参数值，其余场景自行定义。</param>
		/// <param name="balance">玩家当前剩余的虚拟货币数量</param>
		/// <param name="sku">购买商品的product_id(购买时传参)</param>
		public static void EarnVirtualCurrency(string virtual_currency_name, int value, string item_category, 
		    int balance, string sku, Dictionary<string, object> extra = null)
		{
			var dict = new Dictionary<string, object>()
			{
				[ParameterVirtualCurrencyName] = virtual_currency_name,
				[ParameterValue] = value,
				[ParameterItemCategory] = item_category,
				["balance"] = balance,
				["sku"] = sku,
			};
			if(extra != null) dict.AddRange(extra, isOverride: true);
			
			TrackEvent(EventEarnVirtualCurrency, dict);
		}

		/// <summary>
		/// 当用户支出了虚拟货币（金币、宝石、代币等）时触发
		/// </summary>
		/// <param name="virtual_currency_name">虚拟货币的名称</param>
		/// <param name="value">虚拟货币的数量</param>
		/// <param name="item_category">虚拟货币花费场景</param>
		/// <param name="balance">玩家当前剩余的虚拟货币数量</param>
		public static void SpendVirtualCurrency(string virtual_currency_name, int value, string item_category, 
			int balance, Dictionary<string, object> extra = null)
		{
			var dict = new Dictionary<string, object>()
			{
				[ParameterVirtualCurrencyName] = virtual_currency_name,
				[ParameterValue] = value,
				[ParameterItemCategory] = item_category,
				["balance"] = balance,
			};
			if(extra != null) dict.AddRange(extra, isOverride: true);
			
			TrackEvent(EventSpendVirtualCurrency, dict);
		}
	    #endregion

	    #region HP
	    /// <summary>
		/// 体力变化时触发
		/// </summary>
		/// <param name="item_category">体力变化的场景</param>
		/// <param name="hp_before">本次行为变化前体力</param>
		/// <param name="hp">本次行为带来的体力</param>
		/// <param name="hp_after">本次行为变化后体力</param>
		public static void HitPoints(string item_category, int hp_before, int hp, int hp_after, Dictionary<string, object> extra = null)
		{
			var dict = new Dictionary<string, object>()
			{
				[ParameterItemCategory] = item_category,
				["hp_before"] = hp_before,
				["hp"] = hp,
				["hp_after"] = hp_after,
			};
			if(extra != null) dict.AddRange(extra, isOverride: true);
			
			TrackEvent("hit_points", dict);
		}
	    #endregion
	    
	    #region Tch 太极打点逻辑

	    public const double TCH_02_VALUE = 0.2; // tch_02 上限值
	    private static double _tch001MaxValue = 5.0d; // 预设保护值, 如果大于这个值, 算作异常上报
	    private static double _tch001TargetValue = 0.01d;
	    public static double Tch001TargetValue => _tch001TargetValue; // 太极 001 设定值
	    
	    
	    private static double _tch02TargetValue = 0.20d;
	    public static double Tch02TargetValue => _tch02TargetValue; // 太极 02 设定值

	    public static string IAPPlatform
	    {
		    get
		    {
#if UNITY_IOS
			    return "appstore";
#endif
			    return "google_play";
		    }
	    }
	    public static bool EnableTch02Event { get; set; } = false; // 是否使用太极02事件(请手动开启)

	    /// <summary>
	    /// 太极001 IAP收入
	    /// 每发生一次iap收入，触发一次该事件，value值为本次iap的收入值；
	    /// </summary>
	    /// <param name="value">中台返回地收入值</param>
	    /// <param name="productId"></param>
	    /// <param name="orderId"></param>
	    /// <param name="orderType"></param>
	    /// <param name="timestamp"></param>
	    /// <param name="isTest"></param>
	    public static void Tch001IAPRev(double value, string productId, string orderId, string orderType, string timestamp, bool isTest = false)
	    {
		    string sandbox = isTest ? "true" : "false";
		    TchRevEvent(EventTchAdRev001Impression, IAPPlatform, value, orderType, productId, orderId, timestamp, sandbox);
	    }

	    /// <summary>
	    /// 太极02 IAP 收入打点
	    /// 发生一次iap收入，触发一次该事件，value值为本次iap的收入值；
	    /// </summary>
	    /// <param name="value"></param>
	    /// <param name="productId"></param>
	    /// <param name="orderId"></param>
	    /// <param name="orderType"></param>
	    /// <param name="timestamp"></param>
	    /// <param name="isTest"></param>
	    // public static void Tch02IAPRev(double value, string productId, string orderId, string orderType, string timestamp)
	    public static void Tch02IAPRev(double value, string productId, string orderId, string orderType, string timestamp, bool isTest = false)
	    {
		    if (!EnableTch02Event) return;
		    string sandbox = isTest ? "true" : "false";
		    TchRevEvent(EventTchAdRev02Impression, IAPPlatform, value, orderType, productId, orderId, timestamp, sandbox);
	    }
	    
	    /// <summary>
	    /// "1.广告收入累计超过0.01美元，触发一次该事件，重新清零后，开始下一次累计计算；
	    /// </summary>
	    /// <param name="value"></param>
	    public static void Tch001ADRev(double value)
	    {
		    if (value > _tch001MaxValue)
		    {
			    TchAdAbnormalEvent(value); // 上报异常值
			    return;
		    }
		    
		    // if (value < Tch001TargetValue) value = Tch001TargetValue;  // TCH广告添加0值校验修复, 不得小于0.01
		    TchRevEvent(EventTchAdRev001Impression, AdMAX, value);
        
		    if(EnableTch02Event) return; // 如果使用了太极02 则不做FB上报

		    //FB标准购买事件
		    FBPurchase(value, USD, "ads", AdMAX);
	    }
	    
	    /// <summary>
	    /// "1.5 广告收入累计超过0.2美元，触发一次该事件，重新清零后，开始下一次累计计算；
	    /// </summary>
	    /// <param name="value"></param>
	    public static void Tch02ADRev(double value)
	    {
		    if (!EnableTch02Event) return;
		    
		    // if (value < Tch02TargetValue) value = Tch02TargetValue; // TCH广告添加0值校验修复
		    TchRevEvent(EventTchAdRev02Impression, AdMAX, value);
		    
		    //FB标准购买事件
		    FBPurchase(value, USD, "ads", AdMAX);
	    }

	    /// <summary>
	    /// 太极事件点位上报
	    /// </summary>
	    /// <param name="evtName"></param>
	    /// <param name="platform"></param>
	    /// <param name="value"></param>
	    /// <param name="orderType"></param>
	    /// <param name="productId"></param>
	    /// <param name="orderId"></param>
	    /// <param name="timestamp"></param>
	    /// <param name="sandbox"></param>
	    private static void TchRevEvent(string evtName, string platform, double value, 
		    string orderType = "", string productId = "", string orderId = "", string timestamp = "", string sandbox = "")
	    {
		    var data = new Dictionary<string, dynamic>()
		    {
			    { ParameterAdPlatform, platform },
			    { ParameterValue, value },
			    { ParameterCurrency, USD },
		    };
		    
		    //--------- Extra data for IAP receipt ---------------
		    
		    if(!string.IsNullOrEmpty(orderType)) data["order_type"] = orderType;
		    if(!string.IsNullOrEmpty(productId)) data["product_id"] = productId;
		    if(!string.IsNullOrEmpty(orderId)) data["order_id"] = orderId;
		    if(!string.IsNullOrEmpty(timestamp)) data["trans_ts"] = timestamp;
		    if(!string.IsNullOrEmpty(sandbox)) data["sandbox"] = sandbox;
		    
		    //--------- Extra data for IAP receipt ---------------
		    
		    TrackEvent(evtName, data, new EventSetting()
		    {
			    EnableFirebaseAnalytics = true,
			    EnableGuruAnalytics = true,
			    EnableFacebookAnalytics = true,
		    });
	    }
	    
	    
	    /// <summary>
	    /// Facebook 支付上报
	    /// </summary>
	    /// <param name="revenue"></param>
	    /// <param name="currency"></param>
	    /// <param name="type"></param>
	    /// <param name="platfrom"></param>
	    public static void FBPurchase(float revenue, string currency, string type, string platfrom)
	    {
		    var data = new Dictionary<string, object>()
		    {
			    { AppEventParameterName.Currency, USD },
			    { AppEventParameterName.ContentType, type },
			    { ParameterAdPlatform, platfrom },
		    };
		    FBService.LogPurchase(revenue, currency, data);
	    }

	    public static void FBPurchase(double revenue, string currency, string type, string platfrom)
		    => FBPurchase((float)revenue, currency, type, platfrom);

	    /// <summary>
	    /// Google ARO买量点
	    /// </summary>
	    /// <param name="impressionData">广告收入数据</param>
	    /// <param name="value"></param>
	    /// <param name="currency"></param>
	    /// <param name="adPlatform"></param>
	    /// <param name="adUnitId"></param>
	    /// <param name="adPlacement"></param>
	    /// <param name="adSource"></param>
	    /// <param name="adFormat"></param>
	    /// <param name="creativeId"></param>
	    /// <param name="reviewCreativeId"></param>
	    /// <a href="https://docs.google.com/spreadsheets/d/1lFWLeOGJgq34QDBTfl6OpNh7MoI37ehGrhdbxlOrJgs/edit#gid=983654222"></a>
	    /// <li>
	    /// value			double	eg：0.002
	    /// currency			string	USD(只传美元)
	    /// ad_platform			string	"MAX | ADMOB | FACEBOOK"
	    /// ad_source			string	广告来源
	    /// ad_format			string	广告格式
	    /// ad_unit_name		string	广告位名称
	    /// ad_creative_id		string	广告素材id
	    /// </li>
	    public static void ADImpression(double value, string currency = "USD", string adPlatform = "",
		    string adSource = "", string adFormat = "", string adUnitId = "", string adPlacement= "",
		    string creativeId = "", string reviewCreativeId = "")
	    {
		    ADImpression(new Dictionary<string, dynamic>()
		    {
			    [ParameterValue] = value,
			    [ParameterCurrency] = currency,
			    [ParameterAdPlatform] = adPlatform,
			    [ParameterAdSource] = adSource,
			    [ParameterAdFormat] = adFormat,
			    [ParameterAdUnitName] = adUnitId,
			    [ParameterAdPlacement] = adPlacement,
			    [ParameterAdCreativeId] = creativeId,
			    [ParameterReviewCreativeId] = reviewCreativeId,
 		    });
	    }
	    
	    public static void ADImpression(Dictionary<string, object> data)
	    {
		    TrackEvent(EventAdImpression, data);
	    }

	    public static void TchAdAbnormalEvent(double value)
	    {
		    TrackEvent(EventTchAdRevAbnormal, new Dictionary<string, dynamic>()
		    {
			    { ParameterAdPlatform, AdMAX },
			    { ParameterCurrency, USD }, 
			    { ParameterValue, value },
		    }, new EventSetting()
		    {
			    EnableFirebaseAnalytics = true,
			    EnableGuruAnalytics = true,
		    });
	    }

	    #endregion

	    #region Analytics Game IAP 游戏内购打点
	    
	    /// <summary>
	    /// 当付费页面打开时调用（iap_imp）
	    /// </summary>
	    /// <param name="scene">界面跳转的来源</param>
	    /// <param name="extra">扩展参数</param>
	    public static void IAPImp(string scene, Dictionary<string, object> extra = null)
	    {
		    var dict = new Dictionary<string, object>()
		    {
			    { ParameterItemCategory, scene },
		    };
		    if(extra != null) dict = GuruSDKUtils.MergeDictionary(dict, extra);
		    
		    TrackEvent(EventIAPImp, dict);
	    }
	    
	    /// <summary>
	    /// 当付费页面关闭时调用（iap_close）
	    /// </summary>
	    /// <param name="scene"></param>
	    /// <param name="extra"></param>
	    public static void IAPClose(string scene, Dictionary<string, object> extra = null)
	    {
		    var dict = new Dictionary<string, object>()
		    {
			    { ParameterItemCategory, scene },
		    };
		    if(extra != null) dict = GuruSDKUtils.MergeDictionary(dict, extra);
		    
		    TrackEvent(EventIAPClose, dict);
	    }

	    /// <summary>
	    /// 点击付费按钮时调用 (iap_clk)
	    /// </summary>
	    /// <param name="scene">支付场景</param>
	    /// <param name="productId">道具的 sku</param>
	    /// <param name="basePlanId">offer 的 basePlanId</param>
	    /// <param name="offerId">offer 的 offerId</param>
	    /// <param name="extra">扩展参数</param>
	    public static void IAPClick(string scene, string productId, string basePlanId = "", string offerId = "", Dictionary<string, object> extra = null)
	    {
		    string sku = productId;
		    if (!string.IsNullOrEmpty(offerId) && !string.IsNullOrEmpty(basePlanId))
		    {
			    sku = $"{productId}:{basePlanId}:{offerId}"; // 上报三连 ID
		    }
		    var dict = new Dictionary<string, object>()
		    {
			    { ParameterItemCategory, scene },
			    { ParameterItemName, sku },
			    { ParameterProductId, sku },
		    };
		    if(extra != null) dict = GuruSDKUtils.MergeDictionary(dict, extra);

		    TrackEvent(EventIAPClick, dict);
	    }

	    /// <summary>
	    /// "app 内弹出的付费引导IAP付费或试用成功打点"
	    /// </summary>
	    /// <param name="scene">界面跳转的来源</param>
	    /// <param name="productId">product id,多个产品用逗号分隔，第一个商品id放主推商品id</param>
	    /// <param name="value">产品的价格</param>
	    /// <param name="currency">用户的付费币种</param>
	    /// <param name="orderId">订单 ID</param>
	    /// <param name="type">付费类型订阅/产品（subscription/product）</param>
	    /// <param name="isFree">是否为试用（1：试用，0：付费）</param>
	    /// <param name="offerId">若存在 Offer 的话需要上报 OfferID</param>
	    internal static void IAPRetTrue(string scene, string productId, double value, string currency, string orderId, string type, bool isFree = false, string offerId = "")
	    {
		    var dict = new Dictionary<string, object>()
		    {
			    { ParameterItemCategory, scene },
			    { ParameterItemName, productId },
			    { ParameterProductId, productId }, // new parameter, will replace with item_name
			    { ParameterValue, value },
			    { ParameterCurrency, currency },
			    { "order_id", orderId },
			    { "type", type },
			    { "isfree", isFree ? "1" : "0" },
		    };
		
		    if(!string.IsNullOrEmpty(offerId))
			    dict["basePlan"] = offerId;
		    
		    TrackEvent(EventIAPReturnTrue, dict, new EventSetting()
		    {
			    EnableFirebaseAnalytics = true,
			    EnableGuruAnalytics  = true,
			    EnableAdjustAnalytics = true,
		    });
		    
	    }

	    /// <summary>
	    /// "app 内弹出的付费引导IAP付费或试用失败打点"
	    /// </summary>
	    /// <param name="itemCategory">界面跳转的来源</param>
	    /// <param name="productId">product id,多个产品用逗号分隔，第一个商品id放主推商品id</param>
	    /// <param name="failReason"></param>
	    internal static void IAPRetFalse(string itemCategory, string productId, string failReason)
	    {
		    TrackEvent(EventIAPReturnFalse, new Dictionary<string, object>()
		    {
			    { ParameterItemCategory, itemCategory },
			    { ParameterItemName, productId },
			    { ParameterProductId, productId },
			    { ParameterResult, failReason }
		    });
	    }
	    
		/// <summary>
		/// 新用户首次 IAP 付费成功上报 （仅限应用内付费商品，不包含订阅等其它情况）【买量打点】
		/// </summary>
		/// <param name="itemName">productId 商品ID</param>
		/// <param name="value">付费总金额</param>
		/// <param name="currency">币种</param>
		public static void FirstIAP(string itemName, double value, string currency)
		{
			TrackEvent(EventIAPFirst, new Dictionary<string, object>()
			{
				{ ParameterItemName, itemName },
				{ ParameterValue, value },
				{ ParameterCurrency, currency },
			});
		}
		
		/// <summary>
		/// 商品购买成功上报【买量打点】
		/// </summary>
		/// <param name="productName">商品名称（商品ID一样）</param>
		/// <param name="itemName">productId 商品ID</param>
		/// <param name="value">付费总金额</param>
		/// <param name="currency">币种</param>
		public static void ProductIAP(string productName, string itemName, double value, string currency)
		{
			// 替换SKU中的 "." -> "_", 比如: "do.a.iapc.coin.100" 转换为 "do_a_iapc_coin_100"
			if (productName.Contains(".")) productName = productName.Replace(".", "_"); 
			
			string eventName = $"iap_{productName}";
			TrackEvent(eventName, new Dictionary<string, object>()
			{
				{ ParameterItemName, itemName },
				{ ParameterValue, value },
				{ ParameterCurrency, currency },
			});
		}


		/// <summary>
		/// 支付成功后统一上报所有点位数据
		/// </summary>
		/// <param name="productId"></param>
		/// <param name="usdPrice"></param>
		/// <param name="orderData"></param>
		/// <param name="isTest"></param>
		public static void ReportIAPSuccessEvent(BaseOrderData orderData, double usdPrice, bool isTest = false)
		{
			if (orderData == null) return;

			if (!isTest && usdPrice == 0)
			{
				Debug.Log($"[SDK] --- Pruchase value is 0, skip report orders");
				return;
			}

			string productId = orderData.GetProductId();
			string userCurrency = orderData.userCurrency;
			double payPrice = orderData.payPrice;
			string orderType = orderData.OrderType();
			string orderType2 = orderData.OrderTypeII();
			string orderId = orderData.orderId;
			string orderDate  = orderData.payedDate;
			string scene = orderData.scene;
			bool isFree = orderData.isFree;
			string offerId = orderData.offerId;
			
			string transactionId = "";
			string productToken = "";
			string receipt = "";

			if (orderData is GoogleOrderData gdata)
			{
				productToken = gdata.token;
			}
			else if (orderData is AppleOrderData adata)
			{
				receipt = adata.receipt;
			}
			
			//---------- 太极打点逻辑 -----------
			// tch_001 和 tch_02 都要上报
			// fb 的 purchase 事件只打一次
			// TCH 001
			Tch001IAPRev(usdPrice, productId, orderId, orderType, orderDate, isTest); 
			
			// TCH 020
			Tch02IAPRev(usdPrice, productId, orderId, orderType, orderDate, isTest);
			
			// Facebook Track IAP Purchase
			FBPurchase(usdPrice, USD, "iap", IAPPlatform);
			
			//---------- 太极打点逻辑 -----------

			if (orderData.orderType == 1)
			{
				// sub_pruchase : Firebase + Guru + Adjust
				SubPurchase(usdPrice, productId, orderId, orderDate, productToken, receipt, isTest);
			}
			else
			{
				// iap_purchase : Firebase + Guru + Adjust
				IAPPurchase(usdPrice, productId, orderId, orderDate, productToken, receipt, isTest);
			}
			
			// IAP Ret true : Firebase + Guru + Adjust
			IAPRetTrue(scene, productId, payPrice, userCurrency, orderId, orderType2, isFree, offerId);
		}
		

		#endregion
		
		#region IAP_PURCHASE

		/// <summary>
		/// IAP 内购上报
		/// </summary>
		/// <param name="value"></param>
		/// <param name="productId"></param>
		/// <param name="orderId"></param>
		/// <param name="orderDate"></param>
		/// <param name="purchaseToken"></param>
		/// <param name="receipt"></param>
		/// <param name="isSandbox"></param>
		public static void IAPPurchase(double value, string productId, string orderId, string orderDate, 
			string purchaseToken = "", string receipt = "", bool isSandbox = false)
		{
			IAPPurchaseReport(EventIAPPurchase, value, productId, orderId, "IAP", orderDate, purchaseToken, receipt, isSandbox);
		}

		/// <summary>
		/// SUB 订阅上报
		/// </summary>
		/// <param name="value"></param>
		/// <param name="productId"></param>
		/// <param name="orderId"></param>
		/// <param name="orderDate"></param>
		/// <param name="purchaseToken"></param>
		/// <param name="receipt"></param>
		/// <param name="isSandbox"></param>
		public static void SubPurchase(double value, string productId, string orderId, string orderDate, 
			string purchaseToken = "", string receipt = "", bool isSandbox = false)
		{
			IAPPurchaseReport(EventSubPurchase, value, productId, orderId, "SUB", orderDate, purchaseToken, receipt, isSandbox);
		}
		
		private static void IAPPurchaseReport(string eventName, double value, string productId, 
			string orderId, string orderType, string orderDate, string purchaseToken = "", string receipt = "", bool isSandbox = false)	
		{
			
			var dict = new Dictionary<string, dynamic>()
			{
				[ParameterPlatform] = IAPPlatform,
				[ParameterValue] = value,
				[ParameterCurrency] = USD,
				[ParameterProductId] = productId,
				["order_id"] = orderId,
				["order_type"] = orderType,
				["trans_ts"] = orderDate,
				["sandbox"] = isSandbox? "true": "false"
			};
			
			// 上报Firebase + 自打点
			TrackEvent(eventName, dict, new EventSetting()
			{
				EnableFirebaseAnalytics = true,
				EnableGuruAnalytics = true,
				EnableAdjustAnalytics = true
			});
			
			// 上报 Adjust 支付事件
			LogAdjustRevenueEvent(eventName, value, productId, orderId, purchaseToken, receipt, dict);
		}
		
		#endregion

		#region 中台异常打点

		/// <summary>
		/// 中台异常打点
		/// </summary>
		/// <param name="data"></param>
		public static void LogDevAudit(Dictionary<string, dynamic> data)
		{
			if (data == null) return;
			data["country"] = IPMConfig.IPM_COUNTRY_CODE;
			data["network"] = Application.internetReachability.ToString();
			TrackEvent(EventDevAudit, data, new EventSetting() { EnableFirebaseAnalytics = true });
		}
		
		#endregion
		
		
		
		
    }


    public class LevelEndSuccessEventData
    {
	    public readonly int level;
	    public readonly string eventName;
	    public readonly Dictionary<string, object> eventData;
	    public readonly Analytics.EventSetting eventSetting;
	    public readonly int priority;

	    public LevelEndSuccessEventData()
	    {
            
	    }

	    public LevelEndSuccessEventData(int level, Dictionary<string, object> extra = null)
	    {
		    this.level = level;
		    eventName = $"level_end_success_{level}";
		    eventData = new Dictionary<string, object>();
		    if (extra != null)
		    {
			    foreach (var key in extra.Keys)
			    {
				    eventData[key] = extra[key];
			    }
		    }
		    eventData["level"] = level;
		    eventSetting = new Analytics.EventSetting()
		    {
			    EnableFirebaseAnalytics = true,
			    EnableGuruAnalytics = true,
			    EnableAdjustAnalytics = true,
			    EnableFacebookAnalytics = true,
		    };
		    priority = (int)EventPriority.Emergence;
	    }

    }
}

