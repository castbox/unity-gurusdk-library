

namespace Guru
{
    using System;
    
    
    /// <summary>
    /// 广告类型
    /// </summary>
    public enum AdType
    {
        Banner,
        Interstitial,
        Rewarded,
    }

    /// <summary>
    /// 广告状态枚举
    /// </summary>
    public enum AdStatusType
    {
        NotReady,
        Loading,
        Loaded,
        LoadFailed,
        DisplayFailed,
        Closed,
        Paid,
        Clicked
    }


    /// <summary>
    /// 广告状态信息
    /// </summary>
    public class AdStatusInfo
    {
        public string adUnitId;
        public AdType adType;
        public AdStatusType status = AdStatusType.NotReady;
        public string info;
        public DateTime date;
        public string network;
        public string creativeId;
        public double revenue;
        public string waterfall;
        public string format;


        public string GetDate() => date.ToString("yy-MM-dd HH:mm:ss");

        public string ToLogString()
        {
            return $"[{GetDate()}] {adType}:{status}\tid:{adUnitId}\tname:{format}\tnetwork{network}\trevenue:{revenue}\twaterfall:{waterfall}\tinfo:{info}";
        }

    }
}