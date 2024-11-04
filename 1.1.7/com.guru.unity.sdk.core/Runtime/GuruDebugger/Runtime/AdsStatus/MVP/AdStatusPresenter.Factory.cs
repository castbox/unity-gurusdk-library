namespace Guru
{
    using System;
    public partial class AdStatusPresenter
    {

        internal static AdStatusInfo CreateLoadingInfo(string adUnitId, AdType adType)
        {
            return new AdStatusInfo
            {
                adUnitId = adUnitId,
                adType = adType,
                status = AdStatusType.Loading,
                date = DateTime.Now,
            };
        }
        internal static AdStatusInfo CreateLoadedInfo(string adUnitId, AdType adType, string network, string format, string waterfall)
        {
            return new AdStatusInfo
            {
                adUnitId = adUnitId,
                adType = adType,
                status = AdStatusType.Loaded,
                date = DateTime.Now,
                network = network,
                format = format,
                waterfall = waterfall
            };
        }

        internal static AdStatusInfo CreateFailInfo(string adUnitId, AdType adType, string network, string format, string waterfall, string message = "", bool disPlayError = false)
        {
            if (string.IsNullOrEmpty(network)) network = "unknown";
            return new AdStatusInfo
            {
                adUnitId = adUnitId,
                adType = adType,
                status = disPlayError ? AdStatusType.LoadFailed : AdStatusType.DisplayFailed,
                date = DateTime.Now,
                info = message,
                network = network,
                format = format,
                waterfall = waterfall
            };
        }
        
        
        internal static AdStatusInfo CreateClosedInfo(string adUnitId, AdType adType, string network, string format, string waterfall)
        {
            if (string.IsNullOrEmpty(network)) network = "unknown";
            return new AdStatusInfo
            {
                adUnitId = adUnitId,
                adType = adType,
                status = AdStatusType.Closed,
                date = DateTime.Now,
                network = network,
                format = format,
                waterfall = waterfall
            };
        }
        
   
        
        internal static AdStatusInfo CreatePaidInfo(string adUnitId, AdType adType, double revenue, string network, string format, string waterfall)
        {
            if (string.IsNullOrEmpty(network)) network = "unknown";
            return new AdStatusInfo
            {
                adUnitId = adUnitId,
                adType = adType,
                status = AdStatusType.Paid,
                date = DateTime.Now,
                revenue = revenue,
                network = network,
                format = format,
                waterfall = waterfall
            };
        }
        
        
        internal static AdStatusInfo CreateClickedInfo(string adUnitId, AdType adType, string network, string format, string waterfall)
        {
            if (string.IsNullOrEmpty(network)) network = "unknown";
            return new AdStatusInfo
            {
                adUnitId = adUnitId,
                adType = adType,
                status = AdStatusType.Clicked,
                date = DateTime.Now,
                network = network,
                format = format,
                waterfall = waterfall
            };
        }
        
    }
}