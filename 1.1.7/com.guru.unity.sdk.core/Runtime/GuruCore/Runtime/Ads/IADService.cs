using System;

namespace Guru
{
    public interface IADService
    {
        
        #region Lifecycle

        void StartService(Action onSdkReady = null, AdsInitSpec spec = null);

        #endregion
        
        #region Banner

        void RequestBannerAD();
        void ShowBanner(string category = "");
        void HideBanner();
        
        #endregion
        
        #region Interstitial

        void RequestInterstitialAD();
        bool IsInterstitialADReady();
        void ShowInterstitialAD(string category, Action dismissAction = null);
        
        #endregion

        #region Rewarded Ads

        void RequestRewardedAD();
        bool IsRewardedADReady();
        void ShowRewardedAD(string category, Action rewardAction = null, Action<string> failAction = null, Action dismissAction = null);


        #endregion

    }
}