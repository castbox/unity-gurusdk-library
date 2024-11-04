namespace Guru
{
    using System;
    /// <summary>
    /// 广告渠道
    /// </summary>
    public interface IAdChannel
    {
        // Action<string> OnRequestOver { get; set; }

        void Initialize(bool isDebug = false);
        
        string Name { get;}

        bool IsEnabled { get; }

        void LoadBannerAD();

        void LoadInterstitialAD();

        void LoadRewardAD();
    }

    /// <summary>
    /// 异步请求逻辑
    /// </summary>
    public interface IAsyncRequestChannel
    {
        Action<bool, bool> OnBannerRequestOver { get; set; }
        Action<bool, bool> OnInterstitialRequestOver { get; set; }
        Action<bool, bool> OnRewardRequestOver { get; set; }
    }

}