

namespace Guru.Network
{
    using Guru;
    using System;
    using UnityEngine;
    using System.Collections;
    
    public class NetworkStatusMonitor
    {
        private const string Tag = "[NET]";
        
        private const string NETWORK_STATUS_NONE = "none";
        private const string NETWORK_STATUS_MOBILE = "mobile";
        private const string NETWORK_STATUS_WIFI = "wifi";
        private const string NETWORK_STATUS_ETHERNET = "ethernet";
        private const string NETWORK_STATUS_VPN = "vpn";
        private const string NETWORK_STATUS_TETHER = "tether";
        private const string NETWORK_STATUS_BLUETOOTH = "bluetooth";
        private const string NETWORK_STATUS_OTHER = "other";

        private const int CHECK_STATUS_INTERVAL_SECOND = 10;

        private DateTime LastTriggerDate
        {
            get => _saveData.LastReportDate;
            set => _saveData.LastReportDate = value;
        }

        private string LastNetworkStatus
        {
            get => _saveData.LastReportStatus;
            set => _saveData.LastReportStatus = value;
        }

        private readonly Action<string> _onNetworkStatusChanged;
        private readonly Action<string> _onFirstOfflineToday;
        private readonly NetworkMonitorData _saveData;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="onNetworkStatusChanged"></param>
        /// <param name="onFirstOfflineToday"></param>
        public NetworkStatusMonitor(Action<string> onNetworkStatusChanged, Action<string> onFirstOfflineToday)
        {
            _onNetworkStatusChanged = onNetworkStatusChanged;
            _onFirstOfflineToday = onFirstOfflineToday;

            _saveData = new NetworkMonitorData(); // 读取数据
            
            //TODO: 此处不应使用协程实现， 协程只用在 UI 上。 
            CoroutineHelper.Instance.StartCoroutine(OnCheckingNetworkStatus(CHECK_STATUS_INTERVAL_SECOND));
        }
        
        /// <summary>
        /// 获取网络状态
        /// </summary>
        /// <returns></returns>
        public string GetNetworkStatus()
        {
            var internetReachability = Application.internetReachability;
            switch (internetReachability)
            {
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    return NETWORK_STATUS_MOBILE;
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    return NETWORK_STATUS_WIFI;
            }
            return NETWORK_STATUS_NONE;
        }

        /// <summary>
        /// 用户是否已经失去了链接
        /// </summary>
        /// <returns></returns>
        private bool IsUserOffline() => Application.internetReachability == NetworkReachability.NotReachable;

        /// <summary>
        /// 当前是可以打点上报
        /// </summary>
        /// <returns></returns>
        private bool IsSameDay(DateTime date) => DateTime.UtcNow.DayOfYear != date.DayOfYear && DateTime.UtcNow.Year != date.Year;

        /// <summary>
        /// 设置打点时间
        /// </summary>
        private void SetTriggerFirstOfflineDate() => LastTriggerDate = DateTime.UtcNow;
        
        private bool ShouldTriggerFirstOffline()
        {
            return IsUserOffline() && IsSameDay(LastTriggerDate);
        }
        
        private void UpdateNetworkStatus()
        {
            var status = GetNetworkStatus();
#if DEBUG
            Debug.Log($"{Tag} --- Update network status:{status}");
#endif
            if (status != LastNetworkStatus)
            {
                _onNetworkStatusChanged(status);
            }
            
            if (ShouldTriggerFirstOffline())
            {
                Debug.Log($"{Tag} --- Report Offline: {LastNetworkStatus}");
                _onFirstOfflineToday?.Invoke(LastNetworkStatus);
                SetTriggerFirstOfflineDate();
            }
            
            LastNetworkStatus = status;
        }
        
        /// <summary>
        /// 每隔一段时间检测一次网络状态
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        private IEnumerator OnCheckingNetworkStatus(float interval)
        {
            while (true)
            {
                UpdateNetworkStatus();
                yield return new WaitForSeconds(interval);
            }
        }


        [Serializable]
        class NetworkMonitorData
        {
            private const string K_NETWORK_MONITOR_DATA = "guru_network_monitor_data";
            private const string NETWORK_STATUS_NOT_SET = "not_set";
            
            private DateTime _lastReportDate;
            public DateTime LastReportDate
            {
                get => _lastReportDate;
                set
                {
                    _lastReportDate = value;
                    Save(true);
                }
            }

            private string _lastReportStatus;
            public string LastReportStatus
            {
                get => _lastReportStatus;
                set
                {
                    _lastReportStatus = value;
                    Save();
                }
            }
            
            public NetworkMonitorData()
            {
                Load(); // 立即加载数据
            }
            
            private void Load()
            {
                var raw = PlayerPrefs.GetString(K_NETWORK_MONITOR_DATA, "");

                _lastReportStatus = NETWORK_STATUS_NOT_SET;
                _lastReportDate = new DateTime(1970, 1, 1);
                if (!string.IsNullOrEmpty(raw))
                {
                    try
                    {
                        var arr = raw.Split('|');
                        if (arr.Length > 0) _lastReportStatus = arr[0];
                        if (arr.Length > 1) _lastReportDate = DateTime.Parse(arr[1]);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                }
            }
            
            public void Save(bool force = false)
            {
                var buffer = $"{_lastReportStatus}|{_lastReportDate:g}";
                PlayerPrefs.SetString(K_NETWORK_MONITOR_DATA, buffer);
                if(force) PlayerPrefs.Save();
            }

        }
    }
}