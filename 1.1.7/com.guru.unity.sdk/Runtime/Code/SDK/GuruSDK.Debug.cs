
namespace Guru
{
    public partial class GuruSDK
    {
        private const string K_CMD_NAME_DEBUGGER = "gurusdk.unity.dbg";
        private const string K_CMD_NAME_WATERMARK = "gurusdk.unity.wm";
        private const string K_CMD_NAME_CONSOLE = "gurusdk.unity.con";
        
        #region Android 测试入口
        
        /// <summary>
        /// 启动 Android 测试配置
        /// </summary>
        private void StartAndroidDebugCmds()
        {

            if (string.IsNullOrEmpty(AppBundleId))
            {
                UnityEngine.Debug.LogError("--- App Bundle Id is empty, please set it first. ---");
                return;
            }
            
            string val;
            string key;

            key = K_CMD_NAME_DEBUGGER;
            val = AndroidSystemPropertiesHelper.Get(key);
            if (val == AppBundleId)
            {
                // 显示应用调试状态栏
                Debugger.ShowAdStatus();
            }
            
            key = K_CMD_NAME_WATERMARK;
            val = AndroidSystemPropertiesHelper.Get(key);
            if (val == AppBundleId)
            {
                // 显示应用水印
                // TODO
            }
            
            key = K_CMD_NAME_CONSOLE;
            val = AndroidSystemPropertiesHelper.Get(key);
            if (val == AppBundleId)
            {
                // 显示控制台
                
            }
        }

        private bool IsCmdAvailable(string value)
        {
            return value == "1" || value == "true";
        }


        #endregion
    }
}