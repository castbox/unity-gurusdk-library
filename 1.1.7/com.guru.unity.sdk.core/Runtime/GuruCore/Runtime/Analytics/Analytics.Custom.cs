namespace Guru
{
	using System;
	using Firebase.Crashlytics;
	using UnityEngine;
	
	
	/// <summary>
    /// 自打点逻辑
    /// </summary>
    public partial class Analytics
    {
	    
	    private static DateTime _lastReportRateDate; //上次上报信息的日期
	    private const double _reportSuccessInterval = 120; // 上报频率
#if UNITY_IOS
		private const string VALUE_NOT_FOR_IOS = "not_support_for_ios";
#endif
	    // private const string VALUE_ONLY_FOR_IOS = "idfa_only_for_ios";
	    
	    private static bool _isGuruAnalyticInitOnce = false;

	    public static void InitGuruAnalyticService(string firebaseId)
	    {
		    if (_isGuruAnalyticInitOnce) return;
		    _isGuruAnalyticInitOnce = true;

		    try
		    {
			    string appId = IPMConfig.IPM_X_APP_ID;
			    string deviceInfo = new DeviceInfoData().ToString();
			    
			    _lastReportRateDate = DateTime.Now;

			    Debug.Log($"{TAG} --- InitGuruAnalyticService: IsDebug:{IsDebug}  firebaseId:{firebaseId}");
			    
			    GuruAnalytics.Instance.Init(appId, deviceInfo, () =>
			    {
				    OnGuruAnalyticsInitComplete();
				    Debug.Log($"{TAG} --- Guru EXP: GroupId: {GuruAnalytics.Instance.ExperimentGroupId}");
				    SetAnalyticsExperimentGroup(GuruAnalytics.Instance.ExperimentGroupId);
			    }, IsDebug, firebaseId); // Android 初始化	
			    
		    }
		    catch (Exception ex)
		    {
			    LogCrashlytics(ex);
		    }
	    }
		
		#region 设置太极02 值
		
	    /// <summary>
	    /// 设置太极02阀值
	    /// </summary>
	    /// <param name="value"></param>
	    public static void SetTch02TargetValue(double value)
	    {
		    try
		    {
			    if (value > 0)
			    {
				    EnableTch02Event = true; // 自动开启太极02打点设置
				    if (Math.Abs(_tch02TargetValue - value) > 0.001d)
				    {
					    _tch02TargetValue = value;
					    GuruAnalytics.Instance.SetTch02Value(value);
				    }
			    }
		    }
		    catch (Exception e)
		    {
			    if (IsFirebaseReady)
			    {
				    Crashlytics.LogException(e);
			    }
			    else
			    {
				    Debug.LogWarning(e);
			    }
		    }
	    }

	    #endregion
    }
}