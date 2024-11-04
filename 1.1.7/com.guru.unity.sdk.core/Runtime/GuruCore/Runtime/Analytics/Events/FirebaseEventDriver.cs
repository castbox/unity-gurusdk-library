


namespace Guru
{
    using Firebase.Analytics;
    using System.Collections.Generic;
    using UnityEngine;
    
    /// <summary>
    /// Firebase 专用
    /// </summary>
    internal class FirebaseEventDriver : AbstractEventDriver
    {
        
        protected override void FlushTrackingEvent(TrackingEvent trackingEvent)
        {
            var eventName = trackingEvent.eventName;
            var data = trackingEvent.data;
            
            Debug.Log($"[SDK] --- Firebase logEvent: {trackingEvent}");
            
            if (data != null)
            {
                List<Parameter> parameters = new List<Parameter>();
                foreach (var kv in data)
                {
                    if(kv.Value is string strValue)
                        parameters.Add(new Parameter(kv.Key, strValue));
                    else if (kv.Value is bool boolValue)
                        parameters.Add(new Parameter(kv.Key, boolValue ? "true" : "false"));
                    else if (kv.Value is int intValue)
                        parameters.Add(new Parameter(kv.Key, intValue));
                    else if (kv.Value is long longValue)
                        parameters.Add(new Parameter(kv.Key, longValue));
                    else if (kv.Value is float floatValue)
                        parameters.Add(new Parameter(kv.Key, floatValue));
                    else if (kv.Value is double doubleValue)
                        parameters.Add(new Parameter(kv.Key, doubleValue));
                    else if (kv.Value is decimal decimalValue)
                        parameters.Add(new Parameter(kv.Key, decimal.ToDouble(decimalValue)));
                    else
                        parameters.Add(new Parameter(kv.Key, kv.Value.ToString()));
                }
					
                FirebaseAnalytics.LogEvent(eventName, parameters.ToArray());
            }
            else
            {
                FirebaseAnalytics.LogEvent(eventName);
            }
        }
        
        /// <summary>
        /// 输出属性
        /// </summary>
        protected override void SetUserProperty(string key, string value)
        {
            FirebaseAnalytics.SetUserProperty(key, value);
        }

        protected override void ReportUid(string uid)
        {
            FirebaseAnalytics.SetUserId(uid);
        }
        
        protected override void ReportDeviceId(string deviceId)
        {
        }
        
        protected override void ReportAdjustId(string adjustId)
        {
        }

        protected override void ReportGoogleAdId(string adId)
        {
        }

        protected override void ReportAndroidId(string adId)
        {
        }

        protected override void ReportIDFV(string idfv)
        {
        }

        protected override void ReportIDFA(string idfa)
        {
        }
    }
}