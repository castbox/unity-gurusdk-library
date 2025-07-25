#nullable enable
using Cysharp.Threading.Tasks;
using UnityEngine;
using AdjustSdk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Analytics;
using Newtonsoft.Json;

namespace Guru
{
	
	public enum DelayMinutesSource
	{
		Default = 0,
		Local,
		RemoteConfig
	}
	
	/// <summary>
	/// Adjust 服务组件
	/// Unity 插件下载地址：https://github.com/adjust/unity_sdk
	/// Adjust V4 to V5 升级事项， 诸多接口进行了调整： https://dev.adjust.com/en/sdk/migration/unity/v4-to-v5/
	/// </summary>
	public class AdjustService
	{
		public const string VERSION = "2.0.0";
		public const string LOG_TAG = "[Adjust]";
		public const string REMOTE_DELAY_TIME_KEY = "adjust_adrev_delay_minutes"; // DelayMinutes 运控值 默认 30分钟， 云控配置 1440 （24 小时）
		private const float START_DELAY_SECONDS = 5.0f; // 延迟启动时间(s)
		private const int DEFAULT_ATT_WAIT_SECONDS = 120; // 延迟等待 ATT 用户操作结果(s)
		
		public const float DEFAULT_DELAY_MINUTES = 30;
		private const string REVENUE_CURRENCY_USD = "USD";

		private const string AD_REVENUE_SOURCE_APPLOVIN_MAX = "applovin_max_sdk";
		private const string AD_REVENUE_SOURCE_IRONSOURCE = "ironsource_sdk";
		
		
		// private const string K_IAP_PURCHASE = "iap_purchase"; // 固定点位事件
		// private const string K_SUB_PURCHASE = "sub_purchase"; // 固定点位事件
		
		private string _googleAdId = "";

		
		private string _sdkVersion = "5.0.5";
		public string SdkVersion => _sdkVersion;
		private bool _isReady = false;
		public bool IsReady => _isReady;

		private string _adSourceName;

		// ---------------- AdRevenue 打点属性 -------------------- 
		/// <summary>
		/// 应用安装的日志
		/// </summary>
		private readonly DateTime _firstOpenTime;
		/// <summary>
		/// 收益相关数据
		/// </summary>
		private readonly AdjustAdRevenueModel _revenueModel;

		/// <summary>
		/// 暂缓打点策略开关
		/// </summary>
		private bool _deferredReportAdRevenueEnabled = false;
		/// <summary>
		/// 暂缓向Adjust上报adRevenue数据的时间区间，为一个浮点数，单位为分钟；
		/// 当 app_age 小于等于本时间区间时，视为用户处于暂缓上报时间窗口内，暂缓向Adjust上报该用户带来广告收入的数据。
		/// </summary>
		private float _delayMinutes;
		/// <summary>
		/// DelayMinutes 配置来源
		/// </summary>
		private DelayMinutesSource _delayMinutesSource;
		private Coroutine _delayCoroutine;
		
		
		private IGuruSDKApiProxy _guruApiProxy;
		

		#region 构造函数

		/// <summary>
		/// 构造函数
		/// </summary>
		internal AdjustService(IGuruSDKApiProxy guruSDKApiProxy)
		{
			_guruApiProxy = guruSDKApiProxy;
			_firstOpenTime = _guruApiProxy.FirstOpenDate;
			_revenueModel = AdjustAdRevenueModel.LoadOrCreate();
			Adjust.GetSdkVersion(v => _sdkVersion = v);
		}

		#endregion

		#region Consent 设置

		/// <summary>
		/// 更新 ConsentData
		/// </summary>
		/// <param name="consentData"></param>
		public void SetConsentData(ConsentData consentData)
		{
			LogI(LOG_TAG, $"Adjust SetConsent {consentData.IsEea}");
			var adjustThirdPartySharing = new AdjustThirdPartySharing(null);
			if (consentData.IsEea)
			{
				adjustThirdPartySharing.AddGranularOption("google_dma", "eea", "1");
				adjustThirdPartySharing.AddGranularOption(
					"google_dma",
					"ad_personalization",
					consentData.Consents[ConsentType.AdPersonalization] == ConsentStatus.Granted ? "1" : "0");
			}
			else
			{
				adjustThirdPartySharing.AddGranularOption("google_dma", "eea", "0");
				adjustThirdPartySharing.AddGranularOption("google_dma", "ad_personalization", "1");
				adjustThirdPartySharing.AddGranularOption("google_dma", "ad_user_data", "1");
			}

			Adjust.TrackThirdPartySharing(adjustThirdPartySharing);
			LogI(LOG_TAG, "setAdjust complete!");
		}
		

		#endregion

		#region 启动服务

		/// <summary>
		/// Adjust启动服务
		/// </summary>
		/// <param name="profile"></param>
		/// <param name="guruSDKApiProxy"></param>
		/// <param name="onInitComplete">初始化完成的时候会返回 AdjustId </param>
		/// <param name="onGetGoogleAdIdCallback"></param>
		public async UniTask Start(AdjustProfile profile,
			Action<string>? onInitComplete = null, 
			Action<string>? onGetGoogleAdIdCallback = null)
		{
			if (string.IsNullOrEmpty(profile.AppToken))
			{
				LogE(LOG_TAG, "Adjust没有设置token，无法进行初始化");
				return;
			}
			
			// ----------- SetupAdjustSessionCall --------------------
			SetupAdjustSessionCallbackParameters(_guruApiProxy.FirebaseId, _guruApiProxy.DeviceId);
			var config = BuildAdjustConfig(profile.AppToken, _guruApiProxy.FbAppId , profile.ShowLogs, profile.IOSAttWaitingTime, profile.OnDeepLinkCallback);
			
			// SetupInstance(); // 初始化场景示例
			_deferredReportAdRevenueEnabled = profile.DelayStrategyEnabled; // 外部输入的延迟打点策略
			// 设置延迟上报的初始值
			SetAdRevDelayMinutes(DEFAULT_DELAY_MINUTES, DelayMinutesSource.Default);
			
			Adjust.InitSdk(config);  // 启动服务
			
			// 异步加载AdId
			FetchGoogleAdIdAsync(onGetGoogleAdIdCallback);
			LogI(LOG_TAG, $"--- Start AdjustService:{VERSION}    SDK:{SdkVersion}    deferred_report_ad_revenue:{_deferredReportAdRevenueEnabled}");
			
			await UniTask.Delay(TimeSpan.FromSeconds(START_DELAY_SECONDS + 0.1f));
			_isReady = true;
			// 更新归因数据
			CheckAndFetchAdjustAttribution();
			
			// 异步等待延时初始化执行成功
			Adjust.GetAdid(adid =>
			{
				onInitComplete?.Invoke(adid);
			});
		}

		
		private void SetupAdjustSessionCallbackParameters(string firebaseId, string deviceId)
		{
			try
			{
				// 需要在 Adjust.start 前设置 <安装归因参数>
				if (!string.IsNullOrEmpty(firebaseId))
				{
					Adjust.AddGlobalCallbackParameter("user_pseudo_id", firebaseId);
				}

				if (!string.IsNullOrEmpty(deviceId))
				{
					Adjust.AddGlobalCallbackParameter("device_id", deviceId);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"setupAdjustSessionCall error: {ex}");
			}
		}

		private AdjustConfig BuildAdjustConfig(string appToken, string fbAppId, 
			bool showLogs = false, int iOSAttWaitingTime = 0,
			Action<string>? onDeeplinkCallback = null)
		{
			// 初始化启动 Config
			AdjustEnvironment environment = GetAdjustEnvironment();
			AdjustConfig config = new AdjustConfig(appToken, environment);
			config.LogLevel = GetLogLevel(showLogs);
			config.IsPreinstallTrackingEnabled = true; // Adjust Preinstall
			if (!string.IsNullOrEmpty(fbAppId)) config.FbAppId = fbAppId;
			
			// Deeplink Callback
			if (onDeeplinkCallback != null)
			{
				// config.setDeferredDeeplinkDelegate(onDeeplinkCallback);
				config.DeferredDeeplinkDelegate = onDeeplinkCallback;
			}
			
#if UNITY_IOS
			if(iOSAttWaitingTime <= 0) 
				iOSAttWaitingTime = DEFAULT_ATT_WAIT_SECONDS;

			// config.setAttConsentWaitingInterval(iOSAttWaitingTime);
			// In SDK v5, you need to assign your delay interval to the AttConsentWaitingInterval property of your AdjustConfig instance.
			config.AttConsentWaitingInterval = iOSAttWaitingTime;
#endif	
			
			return config;
		}


		private float GetRemainDelaySeconds()
		{
			// 起始时间 + 延迟时间 - 当前时间
			float delayTime = (float)(_firstOpenTime.AddMinutes(_delayMinutes) - DateTime.UtcNow).TotalSeconds;
			if (delayTime < 0) delayTime = 0;
			return delayTime;
		}

		

		/// <summary>
		/// 异步拉取 Google Ad Id
		/// </summary>
		private void FetchGoogleAdIdAsync(Action<string> onGetGoogleAdIdCallback = null)
		{
			Adjust.GetGoogleAdId(gid =>
			{
				if (!string.IsNullOrEmpty(gid))
				{
					_googleAdId = gid; // 获取Google AD ID 
					onGetGoogleAdIdCallback?.Invoke(_googleAdId); // 返回 GoogleAdid
				}
			});
		}
		
		
		#endregion

		#region 属性变化


		private const string KAdjustAttribution = "adjust_attribution";
		public static AdjustAttribution? AdjustAttribution { get; private set; }
		
		public void SetAdjustAttribution(AdjustAttribution? attribution)
		{
			if (attribution == null) return;

			var dict = new Dictionary<string, object>()
			{
				["trackerToken"] = attribution.TrackerToken,
				["trackerName"] = attribution.TrackerName,
				["network"] = attribution.Network,
				["campaign"] = attribution.Campaign,
				["adgroup"] = attribution.Adgroup,
				["creative"] = attribution.Creative,
				["clickLabel"] = attribution.ClickLabel,
				["costType"] = attribution.CostType,
				["costAmount"] = attribution.CostAmount?.ToString() ?? "",
				["costCurrency"] = attribution.CostCurrency,
				["fbInstallReferrer"] = attribution.FbInstallReferrer
			};

			var filtered = dict
				.Where(kv => kv.Value != null)
				.ToDictionary(kv => kv.Key, kv => kv.Value);

			var json = JsonConvert.SerializeObject(filtered);
			// property.SetString(PropertyKeys.AdjustAttributionData, json).Forget();
			PlayerPrefs.SetString(KAdjustAttribution, json);
		}

		public AdjustAttribution? GetAdjustAttribution()
		{
			try
			{
				var result = PlayerPrefs.GetString(KAdjustAttribution, "");
				return string.IsNullOrEmpty(result) ? null : JsonConvert.DeserializeObject<AdjustAttribution>(result);
			}
			catch (Exception e)
			{
				Log.W($"getAdjustAttribution error! {e}");
				return null;
			}
		}
		
		
		private void CheckAndFetchAdjustAttribution()
		{
			try
			{
				var attribution = GetAdjustAttribution();
				if (attribution == null)
				{
					Log.I("first fetch adjust attribution");
					FetchAdjustAttribution(); // fire and forget
				}
				else
				{
					AdjustAttribution = attribution;
					ProcessAdjustAttributionChanged(attribution);
				}
			}
			catch (Exception)
			{
				// Ignored
			}
		}
		
		private bool CheckAdjustAttributionChanged(AdjustAttribution a, AdjustAttribution b)
		{
			return !(ReferenceEquals(a, b) ||
			         (a.TrackerToken == b.TrackerToken &&
			          a.TrackerName == b.TrackerName &&
			          a.Network == b.Network &&
			          a.Campaign == b.Campaign &&
			          a.Adgroup == b.Adgroup &&
			          a.Creative == b.Creative &&
			          a.ClickLabel == b.ClickLabel &&
			          a.CostType == b.CostType &&
			          a.CostAmount == b.CostAmount &&
			          a.CostCurrency == b.CostCurrency &&
			          a.FbInstallReferrer == b.FbInstallReferrer));
		}
		
		private void FetchAdjustAttribution()
		{
			try
			{
				Adjust.GetAttribution(ProcessAdjustAttributionChanged);
			}
			catch (Exception error)
			{
				Log.W($"Failed to fetch adjust attribution: {error}");
				ProcessAdjustAttributionChanged(new AdjustAttribution());
			}
		}
		
		
		private void ProcessAdjustAttributionChanged(AdjustAttribution changedAttribution)
		{
			Log.I("[Adjust]: Attribution changed!");

			UniTask.SwitchToMainThread();
			var channel = !string.IsNullOrEmpty(changedAttribution.Network) ? changedAttribution.Network : "unknown";
			_guruApiProxy.ReportUserProperty("channel", channel);

			var campaign = !string.IsNullOrEmpty(changedAttribution.Campaign) ? changedAttribution.Campaign : "unknown";
			_guruApiProxy.ReportUserProperty("campaign", campaign);

			var creative = !string.IsNullOrEmpty(changedAttribution.Creative) ? changedAttribution.Creative : "unknown";
			_guruApiProxy.ReportUserProperty("creative", creative);

			var currentAdjustAttribution = AdjustAttribution;
			if (currentAdjustAttribution == null ||
			    CheckAdjustAttributionChanged(currentAdjustAttribution, changedAttribution))
			{
				SetAdjustAttribution(changedAttribution);
				AdjustAttribution = changedAttribution;
				Log.I("adjust attribution changed!");
			}

			Log.I($"changedAttr: network:{channel} campaign:{campaign} creative: {creative}");
		}
		

		#endregion
		
		#region 工具接口

		private static AdjustEnvironment GetAdjustEnvironment()
		{
#if UNITY_EDITOR || DEBUG || DEVELOPMENT_BUILD
			return AdjustEnvironment.Sandbox;
#else
			return AdjustEnvironment.Production;
#endif
		}

		private static AdjustLogLevel GetLogLevel(bool showLogs)
		{
#if UNITY_EDITOR || DEBUG || DEVELOPMENT_BUILD
			return AdjustLogLevel.Verbose;
#endif
			return showLogs? AdjustLogLevel.Verbose : AdjustLogLevel.Suppress;
		}


		private static void LogI(string tag, object content)
		{
			Debug.Log($"{tag} {content}");
		}
		
		private static void LogE(string tag, object content)
		{
			Debug.LogError($"{tag} {content}");
		}
		private static void LogW(string tag, object content)
		{
			Debug.LogWarning($"{tag} {content}");
		}


		#endregion

		#region 广告收入打点


		private bool IsInDelayTimeWindow()
		{
			if (!_deferredReportAdRevenueEnabled) return false;
			
			var appAge = DateTime.UtcNow - _firstOpenTime;
			Debug.Log($"{LOG_TAG} --- AppAge:{appAge.TotalMinutes}  delayMins:{_delayMinutes}  ::  firstOpenTime:{_firstOpenTime:g}    now:{DateTime.UtcNow:g}    dataSource:{_delayMinutesSource}");
			return appAge.TotalMinutes <= _delayMinutes;
		}


		/// <summary>
		/// 设置广告延迟窗口参数
		/// </summary>
		/// <param name="delayMinutes">延迟分钟数</param>
		/// <param name="delayMinutesSource">延迟配置来源</param>
		public void SetAdRevDelayMinutes(float delayMinutes, DelayMinutesSource delayMinutesSource)
		{
			_delayMinutes = delayMinutes;
			_delayMinutesSource = delayMinutesSource;
			Debug.Log($"{LOG_TAG} --- Set delayMinutes:{_delayMinutes}, source:{GetDelayMinutesSourceString(_delayMinutesSource)}");
			
			if (_delayCoroutine != null)
			{
				Debug.Log($"{LOG_TAG} --- Set new delayMinutes:{_delayMinutes}. Stop old Coroutine.");
				CoroutineHelper.Instance.StopCoroutine(_delayCoroutine);
			}
			
			if (IsInDelayTimeWindow())
			{
				float delayTime = GetRemainDelaySeconds();
				Debug.Log($"{LOG_TAG} <color=cyan>--- Will report <AccumulatedAdRevenue> in [{delayTime}] sec </color>");
				_delayCoroutine = CoroutineHelper.Instance.StartDelayed(new WaitForSecondsRealtime(delayTime), TrackAccumulatedAdRevenue);
			}
			else
			{
				// 如果此时已含有累计数据， 则需立即上报
				TrackAccumulatedAdRevenue();
			}

		}

		private string GetDelayMinutesSourceString(DelayMinutesSource source)
		{
			switch (source)
			{
				case DelayMinutesSource.Default:
					return "default";
				case DelayMinutesSource.Local:
					return "local";
				case DelayMinutesSource.RemoteConfig:
					return "remote_config";
			}
			return "default";
		}


		private string GetAppAgeStr(DateTime firstOpenTime)
		{
			var span = DateTime.UtcNow - firstOpenTime;
			return $"{(long)span.TotalMilliseconds}";
		}


		/// <summary>
		/// 上报广告事件
		/// </summary>
		/// <param name="impressionEvent"></param>
		public void TrackAdEvent(AdjustAdImpressionEvent impressionEvent)
		{
			TrackAdEvent(impressionEvent.value, 
				impressionEvent.adSource, 
				impressionEvent.adUnitId, 
				impressionEvent.adPlacement, 
				impressionEvent.adFormat,
				impressionEvent.adPlatform);
		}

		/// <summary>
		/// 广告收入上报 (Adjust 特有的接口)
		/// </summary>
		/// <param name="value">收入价值（USD）</param>
		/// <param name="adSource">广告源</param>
		/// <param name="adUnitId">广告单位 ID</param>
		/// <param name="adPlacement">广告播放场景：main_page</param>
		/// <param name="adFormat">广告类型： BANNER， INTERSTIAL， REWARDED</param>
		/// <param name="adPlatform">广告平台：MAX</param>
		public void TrackAdEvent(double value, string adSource, string adUnitId, string adPlacement, string adFormat, string adPlatform = "MAX")
		{

			// 使用
			_adSourceName = AD_REVENUE_SOURCE_APPLOVIN_MAX;
			if (adPlatform != "MAX")
			{
				_adSourceName = AD_REVENUE_SOURCE_IRONSOURCE;
			}

			if (IsInDelayTimeWindow())
			{
				// 累加本次广告收益
				_revenueModel.AddImpressionRevenue(value);
				// 于值更新后再打印数据
				LogI(LOG_TAG,$"<color=orange>--- Save AdRevenue to model: {_revenueModel}, skip report!</color>");
				return;
			}

			var appAge = GetAppAgeStr(_firstOpenTime);
			// 构建 Adjust 的 AdRevenue 事件
			var adEvent = new AdjustAdRevenue(_adSourceName);
			adEvent.SetRevenue(value, REVENUE_CURRENCY_USD);
			// adEvent.SetAdRevenueNetwork(adSource); // -- V4
			adEvent.AdRevenueNetwork = adSource;
			// adEvent.SetAdRevenueUnit(adUnitId); // -- V4
			adEvent.AdRevenueUnit = adUnitId;
			// adEvent.SetAdRevenuePlacement(adPlacement); // -- V4
			adEvent.AdRevenuePlacement = adPlacement;
			// 每次上报需增加回调参数 AddCallbackParameter
			adEvent.AddCallbackParameter("delay_minutes", $"{_delayMinutes}");
			adEvent.AddCallbackParameter("delay_minutes_source", GetDelayMinutesSourceString(_delayMinutesSource));
			adEvent.AddCallbackParameter("app_age_millis", appAge);
			// 添加合作伙伴参数上报（TikTok）
			adEvent.AddPartnerParameter("ad_unit_id", adUnitId);
			adEvent.AddPartnerParameter("ad_format", adFormat);
			
			// 上报广告收益
			LogI(LOG_TAG,$"<color=#88ff00>--- Report AdRevenue event -> value:{value}\n adSource:{adSource}\n adUnitId:{adUnitId}\n adPlacement:{adPlacement}\n delay_minutes:{_delayMinutes}\n delay_minutes_source:{_delayMinutesSource}\n app_age_millis:{appAge}\n</color>");
			Adjust.TrackAdRevenue(adEvent);

		}

		private void TrackAccumulatedAdRevenue()
		{

			if (!_revenueModel.HasData())
			{
				LogW(LOG_TAG,$"--- Report Accumulated AdRevenue but no data exists, Failed!");
				return;
			}
			
			// 上报累计收益
			var accumulatedRevenue = new AdjustAdRevenue(_adSourceName);
			// accumulatedRevenue.setAdImpressionsCount(_revenueModel.impressionCount); // -- V4
			accumulatedRevenue.AdImpressionsCount = _revenueModel.impressionCount;
			accumulatedRevenue.SetRevenue(_revenueModel.revenue, REVENUE_CURRENCY_USD);
			Adjust.TrackAdRevenue(accumulatedRevenue);
			
			// 记录上报时间, 清除累计数据
			LogI(LOG_TAG,$"<color=yellow>--- Report Accumulated AdRevenue -> {_revenueModel}</color>");
			_revenueModel.SetReportDateAndClear(DateTime.UtcNow);

		}

		#endregion
		
		#region IAP收入上报

		/// <summary>
		/// IAP订阅支付事件上报
		/// </summary>
		/// <param name="token"></param>
		/// <param name="iapEvent"></param>
		public void TrackIapEvent(string token, AdjustIapEvent iapEvent)
		{

			AdjustEvent adjustEvent = new AdjustEvent(token);
			
			// --- 属性赋值 ---
			adjustEvent.SetRevenue(iapEvent.value, iapEvent.currency);
			// adjustEvent.SetProductId(iapEvent.productId); // -- V4
			adjustEvent.ProductId = iapEvent.productId;
			// adjustEvent.setTransactionId(iapEvent.orderId); // -- V4
			adjustEvent.TransactionId = iapEvent.orderId;
			if (!string.IsNullOrEmpty(iapEvent.purchaseToken))
			{
				// adjustEvent.setPurchaseToken(iapEvent.purchaseToken); // -- V4
				adjustEvent.PurchaseToken = iapEvent.purchaseToken;
			}

			// if (!string.IsNullOrEmpty(iapEvent.receipt))
			// {
			// 	// adjustEvent.setReceipt(iapEvent.receipt); // -- V4
			// 	// adjustEvent.Receipt = iapEvent.receipt;   // Disabled in V5
			// }	
			// --- BI 属性对齐 ---
			adjustEvent.AddEventParameter("platform", iapEvent.platform);
			adjustEvent.AddEventParameter("value", $"{iapEvent.value}");
			adjustEvent.AddEventParameter("currency", iapEvent.currency);
			adjustEvent.AddEventParameter("product_id", iapEvent.productId);
			adjustEvent.AddEventParameter("order_id", iapEvent.orderId);
			adjustEvent.AddEventParameter("order_type", iapEvent.orderType);
			adjustEvent.AddEventParameter("trans_ts", $"{iapEvent.transTs}");
			adjustEvent.AddEventParameter("sandbox", iapEvent.sandbox);
			

			Adjust.TrackEvent(adjustEvent);
			LogI(LOG_TAG,$"<color=#88ff00>--- Report IAP event -> token:{token}  value:{iapEvent.value}  productId:{iapEvent.productId}  orderId:{iapEvent.orderId}</color>");

		}

		#endregion
		
		#region 普通事件上报

		/// <summary>
		/// 上报通常事件
		/// </summary>
		/// <param name="eventToken"></param>
		/// <param name="data"></param>
		public void TrackEvent(string eventToken, Dictionary<string, object> data)
		{

			var adjustEvent = new AdjustEvent(eventToken);
			if (data != null && data.Count > 0)
			{
				foreach (var kv in data)
				{
					adjustEvent.AddEventParameter(kv.Key, kv.Value.ToString());
				}
			}
			// 普通事件上报
			LogI(LOG_TAG, $"<color=#88ff00>--- Report Normal Event -> token:{eventToken}</color>");
			Adjust.TrackEvent(adjustEvent);

		}
		
		#endregion

		#region ATT 上报

		/// <summary>
		/// 检查新的 ATT 状态
		/// </summary>
		public void CheckNewAttStatus()
		{
			// In SDK v4, you can use the Adjust.checkForNewAttStatus() method to prompt the SDK to read a user’s ATT status and forward the information to Adjust’s servers.
			// This method has been removed in SDK v5.
			// Adjust.checkForNewAttStatus();
		}

		#endregion
	}
}