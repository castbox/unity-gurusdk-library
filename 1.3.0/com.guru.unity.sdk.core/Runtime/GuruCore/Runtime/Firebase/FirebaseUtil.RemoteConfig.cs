using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using UnityEngine;

namespace Guru
{
	public static partial class FirebaseUtil
	{
		private static FirebaseRemoteConfig _remoteConfigInstance => FirebaseRemoteConfig.DefaultInstance;
		private static Dictionary<string, object> _defaults = new Dictionary<string, object>();
		private static bool _isFetchSuccess;
		public static event Action<Dictionary<string, object>> OnSetDefaultParams;
		public static event Action OnFetchRemoteSuccess;
		
		public static bool GetRemoteBooleanValue(string key) => FirebaseRemoteConfig.DefaultInstance.GetValue(key).BooleanValue;
		public static long GetRemoteLongValue(string key) => FirebaseRemoteConfig.DefaultInstance.GetValue(key).LongValue;
		public static string GetRemoteStringValue(string key) => FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
		public static double GetRemoteDoubleValue(string key) => FirebaseRemoteConfig.DefaultInstance.GetValue(key).DoubleValue;
		public static int GetRemoteIntValue(string key)
		{
			try
			{
				if (int.TryParse(FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue, out int value))
					return value;
				else
					return FallBack();
			}
			catch (Exception e)
			{
				return FallBack();
			}

			int FallBack()
			{
				if (_defaults.ContainsKey(key))
					return (int) _defaults[key];
				else
					return -1;
			}
		}
		public static float GetRemoteFloatValue(string key)
		{
			try
			{
				if (float.TryParse(FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue, out float value))
					return value;
				else
					return FallBack();
			}
			catch (Exception e)
			{
				return FallBack();
			}

			float FallBack()
			{
				if (_defaults.ContainsKey(key))
					return (float) _defaults[key];
				else
					return -1f;
			}
		}
		public static T GetRemoteConfig<T>(string key) where T : class
		{
			string value = GetRemoteStringValue(key);
			if (value.IsNotNullAndEmpty())
				return JsonUtility.FromJson<T>(value);
			else if (_defaults.ContainsKey(key))
				return JsonUtility.FromJson<T>((string) _defaults[key]);
			else
				return null;
		}

		public static void InitRemoteConfig()
		{
			InitSetDefaultParams();
			FetchRemoteValue();
		}
		
		private static void InitSetDefaultParams()
		{
			OnSetDefaultParams?.Invoke(_defaults);
			_remoteConfigInstance.SetDefaultsAsync(_defaults);
		}
		
		private static void FetchRemoteValue()
		{
			TimeSpan timeSpan = PlatformUtil.IsDebug() ? TimeSpan.Zero : TimeSpan.FromHours(12);
			_remoteConfigInstance.FetchAsync(timeSpan).ContinueWithOnMainThread(task =>
			{
				if (task.IsCanceled || task.IsFaulted
				    || _remoteConfigInstance.Info.LastFetchStatus != LastFetchStatus.Success)
				{
					Log.E(LOG_TAG, "Firebase RemoteConfig Fetch Failure");
					CoroutineHelper.Instance.StartDelayed(10, FetchRemoteValue);
					return;
				}

				_isFetchSuccess = true;
				_remoteConfigInstance.ActivateAsync();
				OnFetchRemoteSuccess?.Invoke();
			});
		}

		public static void AppendDefaultValue(string key, object value)
		{
			_defaults[key] = value;
			_remoteConfigInstance.SetDefaultsAsync(_defaults);
		}


		public static bool IsFetchSuccess => _isFetchSuccess;

		
		/// <summary>
		/// 是否包含远程Key
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool HasRemoteKey(string key) => _remoteConfigInstance?.Keys.Contains(key) ?? false;

	}
}