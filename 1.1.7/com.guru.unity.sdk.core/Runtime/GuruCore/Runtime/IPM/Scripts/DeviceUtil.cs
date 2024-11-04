
namespace Guru
{
	using System;
	using System.Runtime.InteropServices;
	using UnityEngine;
	
    public static class DeviceUtil
    {
        public static bool IsGetDeviceInfoSuccess;
#if UNITY_IOS
        [DllImport ("__Internal")]
        private static extern string iOSDeviceInfo();
        [DllImport ("__Internal")]
        private static extern void iOSSetBadge();
        [DllImport ("__Internal")]
        private static extern void savePlayerPrefs2AppGroup(string appGroupName);
        [DllImport ("__Internal")]
        private static extern void iOSClearBadge();
#endif
	    public static bool GetDeviceInfo()
        {
	        if (!IsGetDeviceInfoSuccess)
	        {
		        
#if UNITY_EDITOR
		        GetEditorDeviceInfo();
#elif UNITY_ANDROID
		        GetAndroidDeviceInfo();
#elif UNITY_IOS
		        GetIOSDeviceInfo();
#endif
	        }
	        return IsGetDeviceInfoSuccess;
        }

        #region IOS
        
        private static void GetIOSDeviceInfo()
        {
#if UNITY_IOS 
			try
			{
				Debug.Log($"[SDK] --- GetIOSDeviceInfo:: iOSDeviceInfo<string>");
				string content = iOSDeviceInfo();
				Debug.Log($"GetDeviceInfo:{content}");
				if(!string.IsNullOrEmpty(content))
				{
					string[] infos = content.Split('$');
					// IPMConfig.SetDeviceId(infos[0]);
					IPMConfig.IPM_APP_VERSION = infos[1];
					IPMConfig.IPM_TIMEZONE = infos[2];
					IPMConfig.IPM_MODEL = infos[3];
					IPMConfig.IPM_LANGUAGE = infos[4];
					IPMConfig.IPM_LOCALE = infos[5];
					IPMConfig.IPM_COUNTRY_CODE = infos[6];
					IsGetDeviceInfoSuccess = true;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
			}
#endif
        }
		
        public static void SetiOSBadge()
        {
#if UNITY_IOS && !UNITY_EDITOR
            iOSSetBadge();
#endif
        }

        public static void Save2AppGroup()
        {
#if UNITY_IOS && !UNITY_EDITOR
            savePlayerPrefs2AppGroup(IPMConfig.IPM_IOS_APP_GROUP);
#endif
        }
		
        public static void ClerBadge()
        {
#if UNITY_IOS && !UNITY_EDITOR
            iOSClearBadge();
#endif
        }

        #endregion
        
        #region Android
        
        private static AndroidJavaObject _androidJavaObject;
        private static void GetAndroidDeviceInfo()
        {
#if UNITY_ANDROID
	        try
	        {
		        _androidJavaObject ??= new AndroidJavaObject("com.guru.u3d2android.u3d2android");
		        if (_androidJavaObject != null)
		        {
			        Debug.Log($"[SDK] --- GetAndroidDeviceInfo:: com.guru.u3d2android.u3d2android: getDeviceInfo<string>");
			        string content = _androidJavaObject.Call<string>("getDeviceInfo");
			        Debug.Log($"GetDeviceInfo:{content}");
			        if(!string.IsNullOrEmpty(content))
			        {
				        string[] infos = content.Split('$');
				        IPMConfig.IPM_BRAND = infos[0];
				        IPMConfig.IPM_LANGUAGE = infos[1];
				        IPMConfig.IPM_MODEL = infos[2];
				        IPMConfig.IPM_TIMEZONE = infos[4];
				        IPMConfig.IPM_LOCALE = infos[5];
				        IPMConfig.IPM_COUNTRY_CODE = infos[6];
				        IsGetDeviceInfoSuccess = true;
			        }
		        }
	        }
	        catch (Exception ex)
	        {
		        Debug.LogError(ex);
	        }
#endif
        }
        
        
        #endregion

        #region Editor
        
        private static void GetEditorDeviceInfo()
        {
	        Debug.Log($"[SDK] --- GetEditorDeviceInfo");
	        IPMConfig.IPM_APP_VERSION = Application.version;
	        IPMConfig.IPM_TIMEZONE = TimeZone.CurrentTimeZone.StandardName;
	        IPMConfig.IPM_MODEL = SystemInfo.deviceModel;
	        IPMConfig.IPM_LANGUAGE = System.Globalization.CultureInfo.InstalledUICulture.Name;
	        IPMConfig.IPM_LOCALE = System.Globalization.RegionInfo.CurrentRegion.Name;
	        IPMConfig.IPM_COUNTRY_CODE = System.Globalization.RegionInfo.CurrentRegion.Name;
	        IsGetDeviceInfoSuccess = true;
        }

        #endregion

        #region 系统弹框
        public static void ShowToast(string content)
        {
#if UNITY_EDITOR
	        UnityEditor.EditorUtility.DisplayDialog("系统提示", content, "OK");
#elif UNITY_ANDROID
			if (_androidJavaObject == null) return;
			_androidJavaObject.Call<bool>("showToast", content);
#endif
	        Debug.Log($"--------- INFORMATION --------\n{content}\n--------- INFORMATION --------");
        }
        

        #endregion

        #region 系统版本
        
        /// <summary>
        /// 获取AndroidOS系统的版本号
        /// 如果获取失败则返回 0
        /// </summary>
        /// <returns></returns>
        public static int GetAndroidOSVersionInt()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
	        return _androidJavaObject?.CallStatic<int>("getSystemVersionSdkInt") ?? 0;
#endif
	        return 0;
        }
        

        

        #endregion
    }
}