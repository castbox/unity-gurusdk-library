

namespace Guru
{
    using UnityEngine;
    
    public class FBEventDriver: AbstractEventDriver
    {
        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="trackingEvent"></param>
        protected override void FlushTrackingEvent(TrackingEvent trackingEvent)
        {
            var eventName = trackingEvent.eventName;
            var data = trackingEvent.data;
            Debug.Log($"[SDK] --- FB logEvent: {trackingEvent}");
            
            FBService.LogEvent(eventName, null, data);
        }
        
        protected override void SetUserProperty(string key, string value)
        {
            
        }
        
        //---------------- 单独实现所有的独立属性打点 ------------------
        
        /// <summary>
        /// 设置用户ID
        /// </summary>
        protected override void ReportUid(string uid)
        {
            
        }
        protected override void ReportDeviceId(string deviceId)
        {
            
        }
        
        /// <summary>
        /// 设置 AdjustId
        /// (Firebase)
        /// </summary>
        protected override void ReportAdjustId(string adjustId)
        {
            
        }
        
        /// <summary>
        /// 设置 AdId
        /// </summary>
        protected override void ReportGoogleAdId(string adId)
        {
            
        }

        protected override void ReportAndroidId(string adId)
        {
            
        }

        /// <summary>
        /// 设置 IDFV
        /// </summary>
        protected override void ReportIDFV(string idfv)
        {
            
        }

        /// <summary>
        /// 设置 IDFA
        /// </summary>
        protected override void ReportIDFA(string idfa)
        {
            
        }
    }
}