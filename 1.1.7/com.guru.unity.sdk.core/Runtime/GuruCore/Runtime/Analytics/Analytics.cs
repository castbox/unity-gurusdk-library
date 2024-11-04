

using System.Collections;
using UnityEngine;

namespace Guru
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using com.adjust.sdk;
	using Facebook.Unity;
	using UnityEngine;
	using Firebase.Analytics;
	using Firebase.Crashlytics;
	
	//打点模块初始化和基础接口封装
	public partial class Analytics
	{
		public class EventSetting
		{
			public bool EnableFirebaseAnalytics;
			public bool EnableAdjustAnalytics;
			public bool EnableFacebookAnalytics;
			public bool EnableGuruAnalytics = true; // 默认开启自打点

			public override string ToString()
			{
				return $"EvenSetting: firebase:{EnableFirebaseAnalytics}, adjust:{EnableAdjustAnalytics}, facebook:{EnableFacebookAnalytics}, guru:{EnableGuruAnalytics}";
			}

			public static EventSetting GetDefaultSetting()
			{
				return new EventSetting()
				{
					EnableFirebaseAnalytics = true,
					EnableGuruAnalytics = true,
					EnableFacebookAnalytics = true,
					EnableAdjustAnalytics = true,
				};
			}
		}
		private static EventSetting DefaultEventSetting => EventSetting.GetDefaultSetting();

		private static bool _isInitOnce;				//Analytics是否初始化完成
		public static bool EnableDebugAnalytics;	//允许Debug包上报打点

		private static bool IsDebug => PlatformUtil.IsDebug();
		private static bool IsFirebaseReady => FirebaseUtil.IsFirebaseInitialized;
		private static bool IsGuruAnalyticsReady => GuruAnalytics.IsReady;
		
		private static AdjustEventDriver _adjustEventDriver;
		private static FBEventDriver _fbEventDriver;
		private static FirebaseEventDriver _firebaseEventDriver;
		private static GuruEventDriver _guruEventDriver;
		private static MidWarePropertiesManager _propertiesManager;
		

		#region 初始化

		/// <summary>
		/// 初始化打点模块
		/// </summary>
		public static void Init()
		{
			if (_isInitOnce) return;
			_isInitOnce = true;
			_adjustEventDriver = new AdjustEventDriver();
			_fbEventDriver = new FBEventDriver();
			_firebaseEventDriver = new FirebaseEventDriver();
			_guruEventDriver = new GuruEventDriver();
			
			_propertiesManager = new MidWarePropertiesManager(_guruEventDriver, _firebaseEventDriver);
		}
		
		/// <summary>
		/// 外部拉起 Firebase 初始化完成回调
		/// </summary>
		public static void OnFirebaseInitCompleted()
		{
			Debug.Log($"[SDK] --- Analytics Init After FirebaseReady:{IsFirebaseReady}");
			
			// --- 初始化 Crashlytics ---
			CrashlyticsAgent.Init();
			FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
			FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));
			
			// SetUserProperty(PropertyFirstOpenTime, FirstOpenTime);
			_firebaseEventDriver.TriggerFlush();
		}
		
		/// <summary>
		/// 上报事件成功率
		/// </summary>
		private static void ReportEventSuccessRate()
		{
			var interval = (DateTime.Now - _lastReportRateDate).TotalSeconds;
			if (interval > _reportSuccessInterval)
			{
				GuruAnalytics.Instance.ReportEventSuccessRate();
				_lastReportRateDate = DateTime.Now;
			}
		}
		
		public static void OnFBInitComplete()
		{
			_fbEventDriver.TriggerFlush();
		}

		public static void OnAdjustInitComplete()
		{
			_adjustEventDriver.TriggerFlush();
		}
		
		
		private static void OnGuruAnalyticsInitComplete()
		{
			// ShouldFlushGuruEvents();
			CoroutineHelper.Instance.StartDelayed(new WaitForSeconds(0.1f), ShouldFlushGuruEvents);
		}

		/// <summary>
		/// 是否可以发送自打点事件
		/// </summary>
		public static void ShouldFlushGuruEvents()
		{
			if (!_guruEventDriver.IsReady  // Driver 的 Ready 标志位没有打开
			    && IsGuruAnalyticsReady
			    && !string.IsNullOrEmpty(IPMConfig.IPM_UID) // UID 不为空
				) // 自打点库初始化完毕
			{
				Debug.Log($"[ANU][GA] --- ShouldFlushGuruEvents -> _guruEventDriver.TriggerFlush");
				_guruEventDriver.TriggerFlush();
			}
		}


		#endregion

		#region 屏幕(场景)名称

		public static void SetCurrentScreen(string screenName, string className)
		{
			if (!_isInitOnce)
			{
				return;
			}

			Log.I(TAG,$"SetCurrentScreen -> screenName:{screenName}, className:{className}");
			if(GuruAnalytics.IsReady) 
				GuruAnalytics.Instance.SetScreen(screenName);

			TrackEvent(EventScreenView, new Dictionary<string, dynamic>()
			{
				[ParameterScreenName] = screenName,
				[ParameterScreenClass] = className,
			});
		}

		#endregion

		#region 用户属性上报

		/// <summary>
		/// Firebase上报用户ID
		/// </summary>
		/// <param name="uid">通过Auth认证地用户ID</param>
		public static void SetFirebaseUserId(string uid)
		{
			if (!IsFirebaseReady) return;
			Log.I(TAG,$"SetUserIDProperty -> userID:{uid}");
			FirebaseAnalytics.SetUserId(uid);			
			Crashlytics.SetUserId(uid);
		}


		/// <summary>
		/// 设置用户属性
		/// </summary>
		public static void SetUserProperty(string key, string value)
		{
			if (!_isInitOnce)
			{
				throw new Exception($"[{TAG}][SDK] Analytics did not initialized, Call <Analytics.{nameof(Init)}()> first!");
			}
			
			if (IsDebug && !EnableDebugAnalytics)
			{
				Debug.LogWarning($"[{TAG}][SDK] --- SetProperty {key}:{value} can not send int Debug mode. Set <InitConfig.EnableDebugAnalytics> with `true`");
				return;
			}
			
			try
			{
				// 填充相关的追踪事件
				_guruEventDriver.AddProperty(key, value);
				_firebaseEventDriver.AddProperty(key, value);
				ReportEventSuccessRate();
				Debug.Log($"{TAG} --- SetUserProperty -> propertyName:{key}, propertyValue:{value}");
			}
			catch (Exception ex)
			{
				if (FirebaseUtil.IsReady)
				{
					Crashlytics.LogException(ex);
				}
				else
				{
					Debug.Log($"Catch Error: {ex}");
				}
			}
		}
		
	


		/// <summary>
		/// Firebase 上报用户属性
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="propertyValue"></param>
		private static void FirebaseSetUserProperty(string propertyName, string propertyValue)
		{
			if (IsFirebaseReady)
			{
				FirebaseAnalytics.SetUserProperty(propertyName, propertyValue);
			}
			else
			{
				Debug.Log($"{TAG} --- Firebase not ready, call Firebase Init first!");
			}
		}


		#endregion

		#region 打点上报
		

		/// <summary>
		/// 打点上报 (带参数)
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="data"></param>
		/// <param name="eventSetting"></param>
		/// <param name="priority"></param>
		internal static void TrackEvent(string eventName, Dictionary<string, dynamic> data,
			EventSetting eventSetting = null, int priority = -1)
		{
			if (!_isInitOnce)
			{
				throw new Exception($"[{TAG}][SDK] Analytics did not initialized, Call <Analytics.{nameof(Init)}()> first!");
			}
			
			if (IsDebug && !EnableDebugAnalytics)
			{
				Debug.LogWarning($"[{TAG}][SDK] --- LogEvent [{eventName}] can not send int Debug mode. Set <InitConfig.EnableDebugAnalytics> with `true`");
				return;
			}
			
			if (eventSetting == null) eventSetting = DefaultEventSetting;

			var dataStr = "";
			if (data != null) dataStr = JsonParser.ToJson(data);
			Debug.Log($"{TAG} --- [SDK] TrackEvent: {eventName} | priority: {priority} | data:{dataStr} | eventSetting: {eventSetting}");
			
			try
			{
				// 填充相关的追踪事件
				if (eventSetting.EnableGuruAnalytics)
				{
					_guruEventDriver.AddEvent(eventName, data, eventSetting, priority);
				}
				if (eventSetting.EnableFirebaseAnalytics)
				{
					_firebaseEventDriver.AddEvent(eventName, data, eventSetting, priority);
				}
				if (eventSetting.EnableAdjustAnalytics)
				{
					_adjustEventDriver.AddEvent(eventName, data, eventSetting, priority);
				}
				if (eventSetting.EnableFacebookAnalytics)
				{
					_fbEventDriver.AddEvent(eventName, data, eventSetting, priority);
				}
			}
			catch (Exception ex)
			{
				if (FirebaseUtil.IsReady)
				{
					Crashlytics.LogException(ex);
				}
				else
				{
					Debug.Log($"Catch Error: {ex}");
				}
			}
		}
		

		/// <summary>
		/// 上报 Adjust 事件
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="productId"></param>
		/// <param name="receipt"></param>
		/// <param name="data"></param>
		/// <param name="usdPrice"></param>
		/// <param name="transactionId"></param>
		/// <param name="purchaseToken"></param>
		/// <returns></returns>
		internal static bool LogAdjustRevenueEvent(string eventName, double usdPrice, 
			string productId = "", string transactionId = "", string purchaseToken = "", string receipt = "", 
			Dictionary<string, object> data = null )
		{
			AdjustEvent adjustEvent = Analytics.CreateAdjustEvent(eventName);
			if (adjustEvent != null)
			{ 
				adjustEvent.setRevenue(usdPrice, USD);
				if (!string.IsNullOrEmpty(productId)) adjustEvent.setProductId(productId);
				if (!string.IsNullOrEmpty(transactionId)) adjustEvent.setTransactionId(transactionId);
				if (!string.IsNullOrEmpty(purchaseToken)) adjustEvent.setPurchaseToken(purchaseToken);
				if (!string.IsNullOrEmpty(receipt)) adjustEvent.setReceipt(receipt);

				if (data != null && data.Count > 0)
				{
					foreach (var kv in data)
					{
						adjustEvent.AddEventParameter(kv.Key, kv.Value.ToString());
					}
				}
				
				Adjust.trackEvent(adjustEvent);
				return true;
			}
			return false;
		}

		#endregion
		
		#region 通用打点

		/// <summary>
		/// 一般的事件上报通用接口
		/// </summary>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <param name="setting"></param>
		/// <param name="priority"></param>
		public static void Track(string key, Dictionary<string, dynamic> data = null, EventSetting setting = null, int priority = -1)
		{
			TrackEvent(key, data, setting, priority);
		}

		
		/// <summary>
		/// Crashlytics 上报 
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="isException"></param>
		public static void LogCrashlytics(string msg, bool isException = true)
		{
			if (!_isInitOnce) return;
			if (isException)
			{
				LogCrashlytics(new Exception(msg));
			}
			else
			{
				CrashlyticsAgent.Log(msg);
			}
		}
		
		
		public static void LogCrashlytics(Exception ex)
		{
			if (!_isInitOnce) return;
			CrashlyticsAgent.LogException(ex);
		}

		#endregion


	}



}