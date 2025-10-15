using System.Collections.Generic;

namespace Guru
{
    using Guru.Ads;
    public partial class GuruSDK
    {
        // UID
        public static string UID
        {
            get
            {
                if(Model != null && !string.IsNullOrEmpty(Model.UserId)) 
                    return Model.UserId;
                return IPMConfig.IPM_UID;
            }
        }
        public static string UUID => IPMConfig.IPM_UUID;
        public static string DeviceId => IPMConfig.IPM_DEVICE_ID;  // TODO: change it to _model member later.
        public static string PushToken => IPMConfig.FIREBASE_PUSH_TOKEN ?? ""; // TODO: change it to _model member later.
        public static string AuthToken => IPMConfig.FIREBASE_AUTH_TOKEN ?? ""; // TODO: change it to _model member later.
        public static string SupportEmail => GuruSettings.SupportEmail ?? "";

        public static string StoreUrl
        {
            get
            {
                string url = "";
#if UNITY_EDITOR
                url = "https://test@com.guru.ai";
#elif UNITY_ANDROID
                url = GuruSettings?.AndroidStoreUrl ?? "";
#elif UNITY_IOS
                url = GuruSettings?.IOSStoreUrl ?? "";
#endif
                return url;
            }
        }
        
        public static string PrivacyUrl => GuruSettings.PriacyUrl ?? "";
        
        public static string TermsUrl => GuruSettings.TermsUrl ?? "";

        public static string AppBuildVersion => GuruAppVersion.buildVersion;

        public static string AppBuildNumber => GuruAppVersion.buildNumber;
        
        public static string AppVersionString => GuruAppVersion.ToString();

        public static bool IsNewUser => IPMConfig.IPM_NEW_USER;
        
        public static string FirebaseId => IPMConfig.FIREBASE_ID;
        public static string IDFA => IPMConfig.IDFA;
        public static string IDFV => IPMConfig.IDFV;
        public static string AdjustId => IPMConfig.ADJUST_DEVICE_ID;
        public static string GSADID => IPMConfig.GOOGLE_ADID;
        public static string CdnHost => _appServicesConfig?.GetCdnHost() ?? "";

        private static GuruAppVersion _appVersion;
        private static GuruAppVersion GuruAppVersion
        {
            get
            {
                if(_appVersion == null) _appVersion = GuruAppVersion.Load();
                return _appVersion;
            }
        }


        private static string _appBundleId;
        public static string AppBundleId => _appBundleId;

        
        /// <summary>
        /// 设置购买去广告道具的标志位
        /// </summary>
        /// <param name="value"></param>
        public static void SetBuyNoAds(bool value = true)
        {
            Model.IsNoAds = value; // GuruSDK 的 Model
            if (value)
            {
                Analytics.SetIsIapUser(true);
                AdService.Instance.EnableNoAds();
            }
        }

        /// <summary>
        /// 所有成功的主线关卡数量 (b_level)
        /// </summary>
        public static int BLevel
        {
            get => Model.BLevel;
            set => Model.BLevel = value;
        }
        
        /// <summary>
        /// 成功关卡总计数量 (b_play)
        /// </summary>
        public static int BPlay
        {
            get => Model.BPlay;
            set => Model.BPlay = value;
        }
        
        /// <summary>
        /// 获取原生平台国家Code
        /// </summary>
        /// <returns></returns>
        public static string GetCountryCode()
        {
            DeviceUtil.GetDeviceInfo();
            return IPMConfig.IPM_COUNTRY_CODE;

        }
        
        /// <summary>
        /// EEA国家代码列表（大写和小写）
        /// </summary>
        private static readonly HashSet<string> EeaCountryCodes = new()
        {
            // 大写
            "BE", "BG", "CZ", "DK", "CY", "LV", "LT", "LU", "ES", "FR",
            "HR", "IT", "PL", "PT", "RO", "SI", "HU", "MT", "NL", "AT",
            "IS", "LI", "NO", "SK", "FI", "SE", "DE", "EE", "IE", "EL", "GB",
        };
        
        /// <summary>
        /// 是否是EEA国家
        /// </summary>
        /// <returns></returns>
        public static bool IsEea()
        {
            try
            {
                var countryCode = GetCountryCode().ToUpperInvariant();
                return !string.IsNullOrEmpty(countryCode) && EeaCountryCodes.Contains(countryCode);
            }
            catch(System.Exception ex)
            {
                LogE(ex.Message);
                return false;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hasUserConsent"></param>
        public static void SetHasUserConsent(bool hasUserConsent)
        {
            try
            {
                Guru.Ads.Max.MaxAdManager.SetHasUserConsent(hasUserConsent);
            }
            catch(System.Exception ex)
            {
                LogE(ex.Message);
            }
        }
    }


    

}