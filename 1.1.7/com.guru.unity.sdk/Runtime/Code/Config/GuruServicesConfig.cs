

namespace Guru
{
    using System;
    using UnityEngine.Serialization;
    
    [Serializable]
    public class GuruServicesConfig
    {
        public long version = 0;
        public GuruAppSettings app_settings;
        public GuruParameters parameters;
        public GuruAdjustSettings adjust_settings;
        public GuruFbSettings fb_settings;
        public GuruAdSettings ad_settings;
        public string[] products;
        
        //-------------------------------- 配置检测 --------------------------------
        public bool IsAmazonAndroidEnabled() => ad_settings != null && 
                                                ad_settings.amazon_ids_android != null &&
                                                ad_settings.amazon_ids_android.Length > 0;
        public bool IsAmazonIOSEnabled() => ad_settings != null && 
                                                ad_settings.amazon_ids_ios != null &&
                                                ad_settings.amazon_ids_ios.Length > 0;
        public bool IsPubmaticAndroidEnabled() => ad_settings != null && 
                                            ad_settings.pubmatic_ids_android != null &&
                                            ad_settings.pubmatic_ids_android.Length > 0;
        public bool IsPubmaticIOSEnabled() => ad_settings != null && 
                                                  ad_settings.pubmatic_ids_ios != null &&
                                                  ad_settings.pubmatic_ids_ios.Length > 0;
        public bool IsMolocoAndroidEnabled() => ad_settings != null && 
                                                  ad_settings.moloco_ids_android != null &&
                                                  ad_settings.moloco_ids_android.Length > 0;
        public bool IsMolocoIOSEnabled() => ad_settings != null && 
                                                ad_settings.moloco_ids_ios != null &&
                                                ad_settings.moloco_ids_ios.Length > 0;
        public bool IsTradplusAndroidEnabled() => ad_settings != null && 
                                            ad_settings.tradplus_ids_android != null &&
                                            ad_settings.tradplus_ids_android.Length > 0;
        public bool IsTradplusIOSEnabled() => ad_settings != null && 
                                              ad_settings.tradplus_ids_ios != null &&
                                              ad_settings.tradplus_ids_ios.Length > 0;
        public bool IsIAPEnabled() => app_settings != null && app_settings.enable_iap 
                                                           && products != null && products.Length > 0;
        public bool UseCustomKeystore() => app_settings?.custom_keystore ?? false;
        
        public bool IsFirebaseEnabled() => app_settings?.enable_firebase ?? true;
        public bool IsFacebookEnabled() => app_settings?.enable_facebook ?? true;
        public bool IsAdjustEnabled() => app_settings?.enable_adjust ?? true;
        
        //-------------------------------- 配置检测 -------------------------------

        
        
        //-------------------------------- Parameters --------------------------------
        public double Tch02Value() => parameters?.tch_020 ?? 0;
        public bool IsAppReview() => parameters?.apple_review ?? false;
        public bool EnableAnaErrorLog() => parameters?.enable_errorlog ?? false;
        public bool IsAdsCompliance() => parameters?.ads_compliance ?? false;
        public bool DMACountryCheck() => parameters?.dma_country_check ?? false;
        public string DMAMapRule() => parameters?.dma_map_rule ?? "";
        public bool UseUUID() => parameters?.using_uuid ?? false;
        public bool KeywordsEnabled() => parameters?.enable_keywords ?? false; 
        public int TokenValidTime() => parameters?.token_valid_time ?? 604800;
        public int LevelEndSuccessNum() => parameters?.level_end_success_num ?? 50;
        public string CdnHost() => parameters?.cdn_host ?? "";
        public bool UsingUUID() => parameters?.using_uuid ?? true;
        //-------------------------------- Parameters --------------------------------


    }
    
    [Serializable]    
    public class GuruAppSettings
    {
        public string app_id;
        public string product_name;
        public string bundle_id;
        public string support_email;
        public string privacy_url;
        public string terms_url;
        public string android_store;
        public string ios_store;
        public bool enable_firebase = true;
        public bool enable_facebook = true;
        public bool enable_adjust = true;
        public bool enable_iap = false;
        public bool custom_keystore = false;
    }

    [Serializable]
    public class GuruParameters
    {
        public int token_valid_time = 604800;
        public int level_end_success_num = 50;
        public bool enable_keywords = false;
        public double tch_020 = 0;
        public bool using_uuid = false;
        public string dma_map_rule = "";
        public bool dma_country_check = false;
        public bool apple_review = false; // 苹果审核标志位
        public bool enable_errorlog = false;
        public bool ads_compliance = false;
        public string cdn_host = "";
    }

    [Serializable]
    public class GuruAdjustSettings
    {
        public string[] app_token;
        public string[] events;

        public string AndroidToken() => app_token != null && app_token.Length > 0 ? app_token[0] : "";
        public string iOSToken() => app_token != null && app_token.Length > 1 ? app_token[1] : "";
    }
    
    [Serializable]
    public class GuruFbSettings
    {
        public string fb_app_id;
        public string fb_client_token;
    }

    [Serializable]
    public class GuruAdSettings
    {
        public string sdk_key;
        public string[] admob_app_id;
        public string[] max_ids_android;
        public string[] max_ids_ios;
        public string[] amazon_ids_android;
        public string[] amazon_ids_ios;
        public string[] pubmatic_ids_android;
        public string[] pubmatic_ids_ios;
        public string[] moloco_ids_android;
        public string[] moloco_ids_ios;
        public string[] tradplus_ids_android;
        public string[] tradplus_ids_ios;
    }
    
}