

using System.Threading.Tasks;
using Firebase.Extensions;

namespace Guru
{
	using System;
	using System.Collections;
	using Firebase.Messaging;
	using UnityEngine;
	
	public static partial class FirebaseUtil
	{
		private static int _retryDeviceCount = 1;
		private const int _retryTokenDelay = 10;
		// public static bool? IsInitMessage;
		private static bool _isAutoFetchFcmToken = true;
		private static bool _isFetchOnce = false;
		// private static bool _isOnFetching = false;

		public static void SetAutoFetchFcmToken(bool value)
		{
			_isAutoFetchFcmToken = value;
		}

		private static void InitializeMessage()
		{
			// 初始化回调挂载
			FirebaseMessaging.TokenReceived -= OnTokenReceived;
			FirebaseMessaging.MessageReceived -= OnMessageReceived;
			
			FirebaseMessaging.TokenReceived += OnTokenReceived;
			FirebaseMessaging.MessageReceived += OnMessageReceived;
			
			if (_isAutoFetchFcmToken)
			{
				StartFetchFcmToken();
			}
		}
		
		public static void StartFetchFcmToken()
		{
			if (_isFetchOnce) return;
			_isFetchOnce = true;
			
			DelayGetFCMToken(0);
		}

		/// <summary>
		/// 异步获取 FCM Token
		/// </summary>
		private static void GetFCMTokenAsync()
		{
			if (!NetworkUtil.IsNetAvailable)
			{
				// 无网络直接重新获取
				DelayGetFCMToken(_retryTokenDelay);
				return;
			}

			Debug.Log($"{LOG_TAG}[SDK]--- Start GetTokenAsync ---");
			FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(task =>
			{
				if (task.IsCanceled || task.IsFaulted)
				{
					// task 获取失败
					DelayGetFCMToken(_retryTokenDelay);
					return;
				}

				var token = task.Result;

				// 取到的值不为空
				if (!string.IsNullOrEmpty(token))
				{
					IPMConfig.FIREBASE_PUSH_TOKEN = token;
					UploadDeviceInfo();
					return;
				}
				
				DelayGetFCMToken(_retryTokenDelay);

				// 缓存值不为空
				if (!string.IsNullOrEmpty(IPMConfig.FIREBASE_PUSH_TOKEN))
				{
					UploadDeviceInfo();
				}
		
				
				// if (string.IsNullOrEmpty(token) 
				//     && string.IsNullOrEmpty(IPMConfig.FIREBASE_PUSH_TOKEN))
				// {
				// 	// 拉取到了空值， 且无缓存值
				// 	DelayGetFCMToken(_retryTokenDelay);
				// 	return;
				// }
				//
				//
				// Debug.Log($"{LOG_TAG}[SDK] --- GetPushToken:{token}");
				// if (!string.IsNullOrEmpty(token))
				// {
				// 	IPMConfig.FIREBASE_PUSH_TOKEN = token;
				// }
				// UploadDeviceInfo();
			});
		}
		
		private static async void DelayGetFCMToken(int seconds = 2)
		{
			await Task.Delay(seconds * 1000); // 等待 10s
			GetFCMTokenAsync();
		}

		
		//-------- Upload DeviceInfo -----------
		

		/*
		 private static void GetFCMToken()
		{
			CoroutineHelper.Instance.StartCoroutine(CoroutineGetFCMToken());
		}

		private static IEnumerator CoroutineGetFCMToken()
		{
			Debug.Log($"[Firebase] --- Start GetTokenAsync ---");
			
			var task = FirebaseMessaging.GetTokenAsync();
			while (!task.IsCompleted)
				yield return new WaitForEndOfFrame();

			if (task.IsFaulted || task.IsCanceled)
			{
				Log.I(LOG_TAG, $"--- GetTokenAsync Token Failed! {task.Status}");
				CoroutineHelper.Instance.StartDelayed(10, GetFCMToken);
			}
			else
			{
				Log.I(LOG_TAG, "--- GetTokenAsync Token: " + task.Result);
				if (IPMConfig.FIREBASE_PUSH_TOKEN != task.Result || !IPMConfig.IS_UPLOAD_DEVICE_SUCCESS)
				{
					IPMConfig.FIREBASE_PUSH_TOKEN = task.Result;
					UploadDeviceInfo();
				}
			}
		}
		*/

		private static void UploadDeviceInfo()
		{
			if (!NetworkUtil.IsNetAvailable)
			{
				double retryDelay = Math.Pow(2, _retryDeviceCount);
				_retryDeviceCount++;
				CoroutineHelper.Instance.StartDelayed((float) retryDelay, UploadDeviceInfo);
			}
			else
			{
				Debug.Log($"{LOG_TAG} --- UploadDeviceInfo ---");
				//延时重试
				new DeviceInfoUploadRequest()
					.SetRetryTimes(1)
					.SetSuccessCallBack(() =>
					{
						Debug.Log($"{LOG_TAG} --- UploadDeviceInfo:Success");
					})
					.SetFailCallBack(() =>
					{
						Debug.Log($"{LOG_TAG} --- UploadDeviceInfo:failed");
						double retryDelay = Math.Pow(2, _retryDeviceCount);
						_retryDeviceCount++;
						CoroutineHelper.Instance.StartDelayed((float) retryDelay, UploadDeviceInfo);
					}).Send();
			}
		}
		
		private static void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
	        Debug.Log($"{LOG_TAG} --- OnTokenReceived:{token.Token}");
#if UNITY_IOS
	        DeviceUtil.SetiOSBadge();
#endif
        }

		public static void OnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
	        Debug.Log($"{LOG_TAG} --- OnMessageReceived:{args.Message}");
#if UNITY_IOS
	        DeviceUtil.SetiOSBadge();
#endif
        }
	}
}