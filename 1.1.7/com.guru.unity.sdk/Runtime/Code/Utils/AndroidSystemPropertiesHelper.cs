

namespace Guru
{
    using System;
    using UnityEngine;
    
    /// <summary>
    /// Android 系统属性获取器
    /// </summary>
    public class AndroidSystemPropertiesHelper
    {
        
        private static string _appBundleId;
        public static string AppBundleId
        {
            get => _appBundleId;
            set => _appBundleId = value;
        }

        /// <summary>
        /// Get the system property value by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Get(string key)
        {
#if UNITY_ANDROID
            return GetPropValue(key);
#endif
            return "";
        }
        
#if UNITY_ANDROID

        private static AndroidJavaClass _systemPropsCls;
        private const string SYSTEM_PROPS_CLASS = "android.os.SystemProperties";

        private static string GetPropValue(string key)
        {
            try
            {
                if (_systemPropsCls == null)
                {
                    _systemPropsCls = new AndroidJavaClass(SYSTEM_PROPS_CLASS);
                }

                if (_systemPropsCls != null)
                {
                    return _systemPropsCls.CallStatic<string>("get", key);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return "";
        }
#endif
        
        
        
        

    }
}