namespace Guru
{
	using System;
	using System.Collections.Generic;
	using Facebook.Unity;
	using UnityEngine;
	
	[MonoSingleton(EMonoSingletonType.CreateOnNewGameObject, false)]
	public class FBService : MonoSingleton<FBService>
	{
		private const string Tag = "[FB]";
		private bool _isInitOnce;
		private Action _onInitComplete;
		
		public void StartService(Action onInitComplete = null)
		{
			if(_isInitOnce) return;
			_isInitOnce = true;

			_onInitComplete = onInitComplete;
			// Initialize the Facebook SDK
			FB.Init(InitCallback, OnHideUnity);
		}

		private void InitCallback()
		{

			// Signal an app activation App Event
			FB.ActivateApp();
			FB.Mobile.SetAdvertiserIDCollectionEnabled(true);
			FB.Mobile.SetAutoLogAppEventsEnabled(false); // 关闭自动打点上报
#if UNITY_IOS
			FB.Mobile.SetAdvertiserTrackingEnabled(true);
#endif
			_onInitComplete?.Invoke();
		}

		private void OnHideUnity(bool isGameShown)
		{
			if (!isGameShown)
			{
				// Pause the game - we will need to hide
				// Time.timeScale = 0;
			}
			else
			{
				// Resume the game - we're getting focus again
				// Time.timeScale = 1;
			}
		}

		/// <summary>
		/// 事件上报
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="valueToSum"></param>
		/// <param name="data"></param>
		public static void LogEvent(string eventName, float? valueToSum = null, Dictionary<string, object> data  = null)
		{
			if(!IsAvailable) return;
			FB.LogAppEvent(eventName, valueToSum, data);
		}

		/// <summary>
		/// 支付上报
		/// </summary>
		/// <param name="valueToSum"></param>
		/// <param name="currency"></param>
		/// <param name="data"></param>
		public static void LogPurchase(float valueToSum, string currency = "USD",
			Dictionary<string, object> data = null)
		{
			if(!IsAvailable) return;
			FB.LogPurchase(valueToSum, currency, data);
		}


		public static bool IsAvailable
		{
			get
			{
				if (!FB.IsInitialized)
				{
					Debug.LogError($"{Tag} FB is not initialized, please call <FBService.StartService> first.");
					return false;
				}
				return true;
			}
		}


	}
}