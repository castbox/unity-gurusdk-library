#if UNITY_IOS
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Guru
{
    /// <summary>
    /// SWIFT语言支持
    /// </summary>
    public class IOSPostBuildSwift
    {
        [PostProcessBuild(2000)]
        public static void OnPostProcessBuild(BuildTarget target, string buildPath)
        {
            if (target != BuildTarget.iOS) return;
            
            Debug.Log($"--- Add Swift support to project: {buildPath}");
            
            // 更新Swift语言支持
            AddSwiftSupport(buildPath);
        }
        
        /// <summary>
        /// 添加Swift Support
        /// </summary>
        /// <param name="buildPath"></param>
        private static void AddSwiftSupport(string buildPath)
        {
            var projectPath = PBXProject.GetPBXProjectPath(buildPath);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);
            var mainTargetGuid = project.GetUnityMainTargetGuid();
            var frameworkTargetGuid = project.GetUnityFrameworkTargetGuid();
        
            // 关闭BitCode
            project.SetBuildProperty(mainTargetGuid, "ENABLE_BITCODE", "NO");
            project.SetBuildProperty(frameworkTargetGuid, "ENABLE_BITCODE", "NO");
            
            // 添加搜索路径
            project.AddBuildProperty(frameworkTargetGuid, "LD_RUNPATH_SEARCH_PATHS", "/usr/lib/swift");
            
            // 设置主项目的SWIFT构建支持
            project.SetBuildProperty(mainTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
            project.SetBuildProperty(frameworkTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            
            project.WriteToFile(projectPath);
        }
    }
}

#endif