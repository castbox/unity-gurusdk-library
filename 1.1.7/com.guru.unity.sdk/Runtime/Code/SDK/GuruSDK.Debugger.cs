
namespace Guru
{
    using UnityEngine;
    using System;
    
    public partial class GuruSDK
    {
        private static readonly bool _useBaseOptions = true;

        private static GuruDebugger _debugger;

        public static GuruDebugger Debugger
        {
            get
            {
                if (_debugger == null)
                {
                    _debugger = GuruDebugger.Instance;
                    if (_useBaseOptions)
                    {
                        InitDebuggerLayout();
                    }
                }
                return _debugger;
            }
        }
        
        /// <summary>
        /// 显示广告状态
        /// </summary>
        public static bool ShowAdStatus()
        {
            if (!IsServiceReady) return false;
            
            Debugger.ShowAdStatus();
            return true;
        }
        
        /// <summary>
        /// 显示 Debugger
        /// </summary>
        /// <returns></returns>
        public static bool ShowDebugger()
        {
            if (!IsServiceReady) return false;
            
            Debugger.ShowPage(); // 显示 Debugger 界面
            return true;
        }

        private static void InitDebuggerLayout()
        {
            var settings = GuruSettings.Instance;
            var v = GuruAppVersion.Load();
            var app_version = (v == null ? $"{Application.version} (unknown)" : $"{v.version}  ({v.code})");
            var uid = (string.IsNullOrEmpty(UID) ? "NULL" : UID);
            var device_id = (string.IsNullOrEmpty(DeviceId) ? "NULL" : DeviceId);
            var push_token = (string.IsNullOrEmpty(PushToken) ? "NULL" : PushToken);
            var auth_token = (string.IsNullOrEmpty(AuthToken) ? "NULL" : AuthToken);
            var fid = (string.IsNullOrEmpty(FirebaseId) ? "NULL" : FirebaseId);
            var adjust_id = (string.IsNullOrEmpty(AdjustId) ? "NULL" : AdjustId);
            var idfa = (string.IsNullOrEmpty(IDFA) ? "NULL" : IDFA);
            var gsid = (string.IsNullOrEmpty(GSADID) ? "NULL" : GSADID);
            
            // ------------ Info Page --------------------
            Debugger.AddOption("Info/Guru SDK", GuruSDK.Version);
            Debugger.AddOption("Info/Unity Version", Application.unityVersion);
            Debugger.AddOption("Info/Name", settings.ProductName);
            Debugger.AddOption("Info/Bundle Id", settings.GameIdentifier);
            Debugger.AddOption("Info/Version", app_version);
            Debugger.AddOption("Info/Uid", uid).AddCopyButton();
            Debugger.AddOption("Info/Device ID", device_id).AddCopyButton();
            Debugger.AddOption("Info/Push Token", push_token).AddCopyButton();
            Debugger.AddOption("Info/Auth Token", auth_token).AddCopyButton();
            Debugger.AddOption("Info/Firebase Id", fid).AddCopyButton();
            Debugger.AddOption("Info/Adjust Id", adjust_id).AddCopyButton();
            Debugger.AddOption("Info/IDFA", idfa).AddCopyButton();
            Debugger.AddOption("Info/GSADID", gsid).AddCopyButton();
            Debugger.AddOption("Info/Debug Mode", GuruSDK.IsDebugMode? "true" : "false");
            Debugger.AddOption("Info/Screen size", $"{Screen.width} x {Screen.height}");
            
            
            // ------------ Ads Page --------------------
            Debugger.AddOption("Ads/Show Ads Debug Panel", "", ShowMaxDebugPanel);

            var badsId = settings.ADSetting.GetBannerID();
            var iadsId = settings.ADSetting.GetInterstitialID();
            var radsId = settings.ADSetting.GetRewardedVideoID();
            
            Debugger.AddOption("Ads/Banner Id", badsId);
            Debugger.AddOption("Ads/Interstitial Id", iadsId);
            Debugger.AddOption("Ads/Rewarded Id", radsId);
            
            GuruDebugger.OnClosed -= OnDebuggerClosed;
            GuruDebugger.OnClosed += OnDebuggerClosed;
            Callbacks.SDK.InvokeOnDebuggerDisplayed(true);
        }
        
        private static void OnDebuggerClosed()
        {
            GuruDebugger.OnClosed -= OnDebuggerClosed;
            Callbacks.SDK.InvokeOnDebuggerDisplayed(false);
        }
        
        /// <summary>
        /// 显示 Debugger
        /// </summary>
        /// <param name="debugger"></param>
        /// <returns></returns>
        public static bool ShowDebuggerWithData(out GuruDebugger debugger)
        {
            debugger = null;
            bool res = ShowDebugger();
            if (res)
            {
                debugger = GuruDebugger.Instance;
            }
            return res;
        }

        public static GuruDebugger.OptionLayout AddOption(string uri, string content = "", Action clickHandler = null)
        {
            return Debugger.AddOption(uri, content, clickHandler);
        }


    }
}