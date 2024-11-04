

namespace Guru
{
    using System.Collections.Generic;
    
    /// <summary>
    /// 追踪事件
    /// </summary>
    public class TrackingEvent
    {
        public string eventName;
        public int priority;
        public Dictionary<string, dynamic> data;
        public Analytics.EventSetting setting;

        public TrackingEvent()
        {
        }
		
        /// <summary>
        /// 保存打点信息
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="_data"></param>
        /// <param name="_setting"></param>
        /// <param name="_priority"></param>
        public TrackingEvent(string eventName, Dictionary<string, dynamic> data = null, Analytics.EventSetting setting = null, int priority = -1)
        {
            this.eventName = eventName;
            this.data = data;
            this.setting = setting;
            this.priority = priority;
        }

        public override string ToString()
        {
            return $"eventName: {eventName}, data: {data}, setting: {setting}, priority: {priority}";
        }
    }

    
    
    
    

}