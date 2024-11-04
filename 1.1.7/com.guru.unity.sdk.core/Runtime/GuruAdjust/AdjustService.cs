



namespace Guru
{
	using UnityEngine;
	using com.adjust.sdk;
	using System;
	using System.Threading.Tasks;

	public class AdjustService
	{
		private const string Version = "1.6.1";
		private const string AdjustVersion = "4.38.0"; // Adjust SDK Version
		private const string LOG_TAG = "[ADJUST]";
		private const double delayTime = 5; // 延迟启动时间(s)

		private const string K_IAP_PURCHASE = "iap_purchase"; // 固定点位事件
		private const string K_SUB_PURCHASE = "sub_purchase"; // 固定点位事件
		
		private string _googleAdId = "";
		public string GoogleAdId // GPS = Google Play Service
		{
			get
			{
				if(string.IsNullOrEmpty(_googleAdId)) FetchGoogleAdIdAsync();
				return _googleAdId; // Google AdId
			}
		}

		public string IDFA => Adjust.getIdfa();
		public string IDFV => Adjust.getIdfv();

		private string _adjustId = "";
		public string AdjustId
		{
			get
			{
				if (string.IsNullOrEmpty(_adjustId)) _adjustId = Adjust.getAdid();
				return _adjustId; // Adjust AdId;
			}
		}
		
		private bool _isReady = false;
		public bool IsReady => _isReady;

		private static AdjustService _instance;

		public static AdjustService Instance
		{
			get
			{
				if (_instance == null) _instance = new AdjustService();
				return _instance;
			}
		}

		#region 启动服务

		/// <summary>
		/// Adjust启动服务
		/// </summary>
		/// <param name="appToken"></param>
		/// <param name="fbAppId">MIR 追踪 AppID</param>
		/// <param name="firebaseId"></param>
		/// <param name="deviceId"></param>
		/// <param name="onInitComplete">初始化完成的时候会返回 AdjustId </param>
		/// <param name="onDeeplinkCallback"></param>
		/// <param name="onGetGoogleAdIdCallback"></param>
		/// <param name="showLogs"></param>
		public async void Start(string appToken, string fbAppId = "", string firebaseId = "", string deviceId = "", 
			Action<string> onInitComplete = null, 
			Action<string> onDeeplinkCallback = null, 
			Action<string> onGetGoogleAdIdCallback = null, 
			bool showLogs  = false)
		{
			if (string.IsNullOrEmpty(appToken))
			{
				LogE(LOG_TAG, "Adjust没有设置token，无法进行初始化");
				return;
			}

			// 需要在 Adjust.start 前设置 <安装归因参数>
			if (!string.IsNullOrEmpty(firebaseId))
			{
				Adjust.addSessionCallbackParameter("user_pseudo_id", firebaseId);
			}

			if (!string.IsNullOrEmpty(deviceId))
			{
				Adjust.addSessionCallbackParameter("device_id", deviceId);
			}
			
			// 初始化启动 Config
			AdjustEnvironment environment = GetAdjustEnvironment();
			AdjustConfig config = new AdjustConfig(appToken, environment);
			config.setPreinstallTrackingEnabled(true); // Adjust Preinstall
			config.setLogLevel(GetLogLevel(showLogs));
			config.setDelayStart(delayTime);  // 延迟 1s 启动 Adjust，保证 <安装归因参数> 成功注入
			
#if UNITY_ANDROID
			if (!string.IsNullOrEmpty(fbAppId)) config.setFbAppId(fbAppId); // 注入 MIR ID
#endif
			// Deeplink Callback
			if(onDeeplinkCallback != null)
				config.setDeferredDeeplinkDelegate(onDeeplinkCallback);
/*
#if UNITY_EDITOR || DEBUG
			config.setSessionSuccessDelegate(OnSessionSuccessCallback); // SessionSuccess
			config.setSessionFailureDelegate(OnSessionFailureCallback); // SessionFailed
			config.setLogDelegate(log => LogI(LOG_TAG, log));
			config.setEventSuccessDelegate(OnEventSuccessCallback);
			config.setEventFailureDelegate(OnEventFailureCallback);
			config.setAttributionChangedDelegate(OnAttributionChangedCallback);
#endif
*/
			// SetupInstance(); // 初始化场景示例

			Adjust.start(config);  // 启动服务
			
			// 异步加载AdId
			FetchGoogleAdIdAsync(onGetGoogleAdIdCallback);
			LogI(LOG_TAG, $"----- Start AdjustService[{Version}]  AdjustVer:{AdjustVersion} -----");
			
			// 异步等待延时初始化执行成功
			// TODO: 应该在此处类似 Firebase 的continueWithMainThread 的能力的一个函数
			// TODO: 应该让以下的任务推迟 Ns 在主线程执行
			await Task.Delay(TimeSpan.FromMilliseconds((delayTime + 0.1) * 1000));
			_isReady = true;
			onInitComplete?.Invoke(Adjust.getAdid());
		}
		
		
		/// <summary>
		/// 异步拉取 Google Ad Id
		/// </summary>
		private void FetchGoogleAdIdAsync(Action<string> onGetGoogleAdIdCallback = null)
		{
			Adjust.getGoogleAdId(gid =>
			{
				if (!string.IsNullOrEmpty(gid))
				{
					_googleAdId = gid; // 获取Google AD ID 
					onGetGoogleAdIdCallback?.Invoke(_googleAdId); // 返回 GoogleAdid
				}
			});
		}

		/// <summary>
		/// 确保 Adjust 实例在场景中
		/// </summary>
		private void SetupInstance()
		{
			var go = UnityEngine.GameObject.Find(nameof(Adjust));
			if (go == null)
			{
				go = new GameObject(nameof(Adjust));
				var ins = go.AddComponent<Adjust>();
				ins.startManually = true;
				ins.launchDeferredDeeplink = true;
				ins.sendInBackground = true;
			}
		}
		
		#endregion
		
		#region 事件回调函数
		/*
		/// <summary>
		/// Session 启动后回调
		/// 回调中可以获取实际的 AdjustID
		/// </summary>
		/// <param name="sessionSuccessData"></param>
		private void OnSessionSuccessCallback(AdjustSessionSuccess sessionSuccessData)
		{
			var adid = sessionSuccessData.Adid;
			LogI(LOG_TAG,$"{LOG_TAG} --- Session tracked successfully! Get Adid: {adid}");
		}
		
		private void OnAttributionChangedCallback(AdjustAttribution attributionData)
		{
			LogI(LOG_TAG, "Attribution changed!");

			if (attributionData.trackerName != null)
			{
				LogI(LOG_TAG, "Tracker name: " + attributionData.trackerName);
			}

			if (attributionData.trackerToken != null)
			{
				LogI(LOG_TAG, "Tracker token: " + attributionData.trackerToken);
			}

			if (attributionData.network != null)
			{
				LogI(LOG_TAG, "Network: " + attributionData.network);
			}

			if (attributionData.campaign != null)
			{
				LogI(LOG_TAG, "Campaign: " + attributionData.campaign);
			}

			if (attributionData.adgroup != null)
			{
				LogI(LOG_TAG, "Adgroup: " + attributionData.adgroup);
			}

			if (attributionData.creative != null)
			{
				LogI(LOG_TAG, "Creative: " + attributionData.creative);
			}

			if (attributionData.clickLabel != null)
			{
				LogI(LOG_TAG , "Click label: " + attributionData.clickLabel);
			}

			if (attributionData.adid != null)
			{
				LogI(LOG_TAG, "ADID: " + attributionData.adid);
			}
		}

		private void OnEventSuccessCallback(AdjustEventSuccess eventSuccessData)
		{
			LogI(LOG_TAG, "Event tracked successfully!");

			if (eventSuccessData.Message != null)
			{
				LogI(LOG_TAG, "Message: " + eventSuccessData.Message);
			}

			if (eventSuccessData.Timestamp != null)
			{
				LogI(LOG_TAG, "Timestamp: " + eventSuccessData.Timestamp);
			}

			if (eventSuccessData.Adid != null)
			{
				LogI(LOG_TAG, "Adid: " + eventSuccessData.Adid);
			}

			if (eventSuccessData.EventToken != null)
			{
				LogI(LOG_TAG, "EventToken: " + eventSuccessData.EventToken);
			}

			if (eventSuccessData.CallbackId != null)
			{
				LogI(LOG_TAG, "CallbackId: " + eventSuccessData.CallbackId);
			}

			if (eventSuccessData.JsonResponse != null)
			{
				LogI(LOG_TAG, "JsonResponse: " + eventSuccessData.GetJsonResponse());
			}
		}

		private void OnEventFailureCallback(AdjustEventFailure eventFailureData)
		{
			LogI(LOG_TAG, "Event tracking failed!");

			if (eventFailureData.Message != null)
			{
				LogI(LOG_TAG, "Message: " + eventFailureData.Message);
			}

			if (eventFailureData.Timestamp != null)
			{
				LogI(LOG_TAG, "Timestamp: " + eventFailureData.Timestamp);
			}

			if (eventFailureData.Adid != null)
			{
				LogI(LOG_TAG, "Adid: " + eventFailureData.Adid);
			}

			if (eventFailureData.EventToken != null)
			{
				LogI(LOG_TAG, "EventToken: " + eventFailureData.EventToken);
			}

			if (eventFailureData.CallbackId != null)
			{
				LogI(LOG_TAG, "CallbackId: " + eventFailureData.CallbackId);
			}

			if (eventFailureData.JsonResponse != null)
			{
				LogI(LOG_TAG, "JsonResponse: " + eventFailureData.GetJsonResponse());
			}

			LogI(LOG_TAG, "WillRetry: " + eventFailureData.WillRetry.ToString());
		}

		private void OnSessionFailureCallback(AdjustSessionFailure sessionFailureData)
		{
			LogE(LOG_TAG,"Session tracking failed!");

			if (sessionFailureData.Message != null)
			{
				LogI(LOG_TAG,"Message: " + sessionFailureData.Message);
			}

			if (sessionFailureData.Timestamp != null)
			{
				LogI(LOG_TAG,"Timestamp: " + sessionFailureData.Timestamp);
			}

			if (sessionFailureData.Adid != null)
			{
				LogI(LOG_TAG,"Adid: " + sessionFailureData.Adid);
			}

			if (sessionFailureData.JsonResponse != null)
			{
				LogI(LOG_TAG,"JsonResponse: " + sessionFailureData.GetJsonResponse());
			}

			LogI(LOG_TAG,"WillRetry: " + sessionFailureData.WillRetry.ToString());
		}
		*/
		#endregion
		
		#region IAP收入上报
		
		/// <summary>
		/// IAP支付事件上报
		/// </summary>
		/// <param name="revenue"></param>
		/// <param name="productID"></param>
		public void TrackIAPPurchase(double revenue, string productID)
		{
			string tokenID = Analytics.GetAdjustEventToken(K_IAP_PURCHASE);
			if (string.IsNullOrEmpty(tokenID))
				return;
			
			AdjustEvent adjustEvent = new AdjustEvent(tokenID);
			adjustEvent.setRevenue(revenue,"USD");
			adjustEvent.AddEventParameter("platform", Analytics.IAPPlatform);
			adjustEvent.AddEventParameter("product_id", productID);
			adjustEvent.AddEventParameter("value", $"{revenue}");
			Adjust.trackEvent(adjustEvent);
		}
		
		/// <summary>
		/// IAP订阅支付事件上报
		/// </summary>
		/// <param name="revenue"></param>
		/// <param name="productID"></param>
		public void TrackSubPurchase(double revenue, string productID)
		{
			string tokenID = Analytics.GetAdjustEventToken(K_SUB_PURCHASE);
			if (string.IsNullOrEmpty(tokenID))
				return;

			AdjustEvent adjustEvent = new AdjustEvent(tokenID);
			adjustEvent.setRevenue(revenue,"USD");
			adjustEvent.AddEventParameter("platform", Analytics.IAPPlatform);
			adjustEvent.AddEventParameter("product_id", productID);
			adjustEvent.AddEventParameter("value", $"{revenue}");
			Adjust.trackEvent(adjustEvent);
		}
		
		/// <summary>
		/// 广告收入上报 (Adjust 特有的接口)
		/// </summary>
		/// <param name="value"></param>
		/// <param name="currency"></param>
		/// <param name="adSource"></param>
		/// <param name="adUnitId"></param>
		/// <param name="adPlacement"></param>
		public void TrackADRevenue(double value, string currency, string adSource, string adUnitId, string adPlacement)
		{
			var adRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAppLovinMAX);
			if (string.IsNullOrEmpty(currency)) currency = "USD";
			adRevenue.setRevenue(value, currency);
			adRevenue.setAdRevenueNetwork(adSource);
			adRevenue.setAdRevenueUnit(adUnitId);
			adRevenue.setAdRevenuePlacement(adPlacement);
			Adjust.trackAdRevenue(adRevenue);
		}
		
		#endregion
		
		#region 工具接口

		private static AdjustEnvironment GetAdjustEnvironment()
		{
#if UNITY_EDITOR || DEBUG
			return AdjustEnvironment.Sandbox;
#else
			return AdjustEnvironment.Production;
#endif
		}

		private static AdjustLogLevel GetLogLevel(bool showLogs)
		{
#if UNITY_EDITOR || DEBUG
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
		
		#endregion
	}
}