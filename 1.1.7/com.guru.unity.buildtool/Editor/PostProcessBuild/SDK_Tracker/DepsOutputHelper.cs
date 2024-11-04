namespace Guru.Editor
{
    using System;
    using UnityEditor;
    using System.Diagnostics;
    using System.IO;
    using UnityEngine;
    using Debug=UnityEngine.Debug;
    using System.Collections.Generic;
    
    public static class DepsOutputHelper
    {
        private static readonly string DepsScriptName = "deps.sh";
        private static readonly string EnvScriptName = ".deps_env";
        private static string _scriptFilePath = String.Empty;

        private static readonly List<string> UnityEmbeddedGradleVersions = new List<string>()
        {
            "2021.3.41f1",
            "2021.3.42f1",
            "2021.3.43f1",
        };

        private static string ScriptFilePath
        {
            get
            {
                if(string.IsNullOrEmpty(_scriptFilePath))
                    _scriptFilePath = GetScriptFilePath();
                return _scriptFilePath;
            }
        }
        /// <summary>
        /// 获取脚本路径
        /// </summary>
        /// <returns></returns>
        private static string GetScriptFilePath()
        {
            string sc = string.Empty;
            var guids = AssetDatabase.FindAssets($"{nameof(DepsOutputHelper)} t:script");
            if (guids.Length <= 0) return string.Empty;
            
            sc = AssetDatabase.GUIDToAssetPath(guids[0]);
            var fpath = $"{Directory.GetParent(sc).FullName}/files/{DepsScriptName}";
            if(File.Exists(fpath)) return fpath;
            return string.Empty;
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="projPath"></param>
        /// <param name="cmd"></param>
        public static void CallDepsScript(string workpath, string cmd = "")
        {
            if (string.IsNullOrEmpty(cmd)) cmd = DepsScriptName;
            RunShellCmd(workpath, cmd);
            Debug.Log($"---- running command: {cmd} is over -----");
        }

        // 运行命令
        public static void RunShellCmd(string workpath, string cmd)
        {
            //------ 启动命令 --------
            Process p = new Process();
            p.StartInfo.WorkingDirectory = workpath;
            p.StartInfo.FileName = "/bin/bash";
            p.StartInfo.Arguments = cmd;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            var log = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            Debug.Log(log);
        }

        // 设置ENV文件
        private static void SetupEnvScript(string projPath, string depauditPath = "")
        {
            string buildName = $"1.0.0-00000000";
            string platform = $"editor";
            string dir = projPath;
            
#if UNITY_ANDROID
            buildName = $"{Application.version}-{PlayerSettings.Android.bundleVersionCode}";
            platform = "android";
#elif UNITY_IOS
            buildName = $"{Application.version}-{PlayerSettings.iOS.buildNumber}";
            platform = "ios";
#endif
            List<string> lines = new List<string>()
            {
                $"export BUILD_NAME={buildName}",
                $"export APP_NAME=\"{PlayerSettings.productName}\"",
                $"export APP_ID={Application.identifier}",
                $"export PLATFORM={platform}",
                $"export DIR={dir}",
#if UNITY_ANDROID
                $"export GRADLE_HOME={UnityEditor.Android.AndroidExternalToolsSettings.gradlePath}", // 使用 UNITY 内置的 GRADLE 版本
#endif
            };
            
            if (!string.IsNullOrEmpty(depauditPath))
            {
                // 本地调试, 需要工具路径
                lines.Add($"export depaudit={depauditPath}");
            }
            
            File.WriteAllLines($"{projPath}/{EnvScriptName}", lines.ToArray());
        }

        /// <summary>
        /// 安装和运行依赖输出器
        /// </summary>
        /// <param name="buildPath"></param>
        public static void InstallAndRun(string buildPath)
        {
            if (string.IsNullOrEmpty(ScriptFilePath) || !File.Exists(ScriptFilePath))
            {
                Debug.LogError($"--- deps script file not found, skip output deps...");
                return;
            }
            
            string projPath = buildPath;
#if UNITY_ANDROID
            projPath = Directory.GetParent(buildPath).FullName;
#elif UNITY_IOS
            //TBD
#endif
            //---- Setup Env ----
            SetupEnvScript(projPath);
            
            //---- Setup Deps ----
            string to = $"{projPath}/{DepsScriptName}";
            if (File.Exists(to)) File.Delete(to);
            FileUtil.CopyFileOrDirectory(ScriptFilePath, to); //拷贝脚本
            
            try
            {
                Debug.Log($"=== Output build deps data ===");
                CallDepsScript(projPath);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Debug.Log($"=== Output pods deps failed: {ex}");
            }
            
        }
        
    }
}