
namespace Guru
{
  
    public class GuruEventDriver: AbstractEventDriver
    {
        protected override void FlushTrackingEvent(TrackingEvent trackingEvent)
        {
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            GuruAnalytics.Instance.LogEvent(trackingEvent.eventName, trackingEvent.data, trackingEvent.priority);
        }
        
        /// <summary>
        /// 输出属性
        /// </summary>
        protected override void SetUserProperty(string key, string value)
        {
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            GuruAnalytics.Instance.SetUserProperty(key, value);
        }
        
        //---------------- 单独实现所有的独立属性打点 ------------------
        
        /// <summary>
        /// 设置用户ID
        /// </summary>
        protected override void ReportUid(string uid)
        {
            GuruAnalytics.Instance.SetUid(uid);
        }

        /// <summary>
        /// 设置设备ID
        /// (Firebase, Guru)
        /// </summary>
        protected override void ReportDeviceId(string deviceId)
        {
            GuruAnalytics.Instance.SetDeviceId(deviceId);
        }

        /// <summary>
        /// 设置 AdjustId
        /// (Firebase)
        /// </summary>
        protected override void ReportAdjustId(string adjustId)
        {
            GuruAnalytics.Instance.SetAdjustId(adjustId);
        }

        protected override void ReportAndroidId(string androidId)
        {
            GuruAnalytics.Instance.SetAndroidId(androidId);
        }

        /// <summary>
        /// 设置 AdId
        /// </summary>
        protected override void ReportGoogleAdId(string adId)
        {
            GuruAnalytics.Instance.SetAdId(adId);
        }

        /// <summary>
        /// 设置 IDFV
        /// </summary>
        protected override void ReportIDFV(string idfv)
        {
            GuruAnalytics.Instance.SetIDFV(idfv);
        }

        /// <summary>
        /// 设置 IDFA
        /// </summary>
        protected override void ReportIDFA(string idfa)
        {
            GuruAnalytics.Instance.SetIDFA(idfa);
        }
    }
}