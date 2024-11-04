#if UNITY_IOS

namespace Guru
{
    using UnityEngine;
    using System;
    using Unity.Advertisement.IosSupport;
    public class ATTManager
    {
        public const string Version = "1.0.0";

        public const string ATT_STATUS_AUTHORIZED = "authorized";
        public const string ATT_STATUS_DENIED = "denied";
        public const string ATT_STATUS_RESTRICTED = "restricted";
        public const string ATT_STATUS_NOT_DETERMINED = "notDetermined";
        public const string ATT_STATUS_NOT_APPLICABLE = "notApplicable";
        public const int ATT_REQUIRED_MIN_OS = 14;
        
        //----------  引导类型 ------------
        public const string GUIDE_TYPE_ADMOB = "admob";
        public const string GUIDE_TYPE_CUSTOM = "custom";
        public const string GUIDE_TYPE_MAX = "max";
        
        /// <summary>
        /// 获取状态
        /// </summary>
        /// <returns></returns>
        public static string GetStatus()
        {
            if (!IsATTSupported()) return ATT_STATUS_NOT_APPLICABLE;
            var status = GetStatusString(ATTrackingStatusBinding.GetAuthorizationTrackingStatus());
            if(!string.IsNullOrEmpty(status)) return status;
            return ATT_STATUS_NOT_APPLICABLE;
        }

        /// <summary>
        /// 转字符串
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static string GetStatusString(ATTrackingStatusBinding.AuthorizationTrackingStatus status)
        {
            switch (status)
            {
                case ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED:
                    return ATT_STATUS_NOT_DETERMINED;
                case ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED:
                    return ATT_STATUS_AUTHORIZED;
                case ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED:
                    return ATT_STATUS_DENIED;
                case ATTrackingStatusBinding.AuthorizationTrackingStatus.RESTRICTED:
                    return ATT_STATUS_RESTRICTED;
            }
            return "";
        }

        /// <summary>
        /// 状态码转字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetStatusString(int value) 
            => GetStatusString((ATTrackingStatusBinding.AuthorizationTrackingStatus)value);
        
        /// <summary>
        /// 是否支持ATT
        /// </summary>
        /// <returns></returns>
        private static bool IsATTSupported()
        {
            string version = UnityEngine.iOS.Device.systemVersion;
            
            // Debug.Log($"[ATT] --- Get iOS system version: {version}");

            string tmp = version;
            if (version.Contains(" "))
            {
                var a1 = version.Split(' ');
                tmp = a1[a1.Length - 1];
            }

            string num = tmp;
            if (tmp.Contains("."))
            {
                num = tmp.Split('.')[0];
            }
            
            if (int.TryParse(num, out var ver))
            {
                if (ver >= ATT_REQUIRED_MIN_OS) return true;
            }

            return false;
        }
        
        /// <summary>
        /// 请求系统弹窗
        /// </summary>
        public static void RequestATTDailog(Action<string> callback = null)
        {
            if (!IsATTSupported())
            {
                callback?.Invoke(ATT_STATUS_NOT_APPLICABLE); //  不支持
                return;
            }
            
            ATTrackingStatusBinding.RequestAuthorizationTracking(status =>{
                callback?.Invoke(GetStatusString(status));
            });
        }
        
        /// <summary>
        /// 启动时检查状态
        /// </summary>
        /// <param name="callback"></param>
        public static void CheckStatus(Action<string> callback = null)
        {
            if (!IsATTSupported())
            {
                callback?.Invoke(ATT_STATUS_NOT_APPLICABLE); //  不支持
                return;
            }
            
            callback?.Invoke(GetStatus());
        }

    }


}

#endif