#if UNITY_IOS

namespace Guru.Editor
{
	using System.Collections.Generic;
	using System.IO;
	using UnityEditor;
	using UnityEditor.Callbacks;
	using UnityEditor.iOS.Xcode;
	using UnityEditor.iOS.Xcode.Extensions;
	using UnityEngine;
	
	public static class IOSPostProcessBuildIPM
	{
		public static readonly string DEFAULT_PROJECT_TARGET_NAME = "Unity-iPhone";
		public static readonly string NOTIFICATION_SERVICE_EXTENSION_TARGET_NAME = "U3D2FCM-iOS";
		public static readonly string NOTIFICATION_SERVICE_EXTENSION_OBJECTIVEC_FILENAME = "NotificationService";
		
		private static readonly char DIR_CHAR = Path.DirectorySeparatorChar;
		public static readonly string OS_PLATFORM_LOCATION = $"Assets/Guru/GuruBuildTool/Editor/IOS_POST_IPM/";
		private static readonly string SKADNetworkIdentifier = "SKAdNetworkIdentifier";
		private static List<string> NETWORK_IDENTIFIER_ARRAY = new List<string>();
		
		private enum EntitlementOptions {
			AppGroups,
		}
		
		private static readonly string[] FRAMEWORKS_MAIN_TO_ADD = {
		};
		
		private static readonly string[] FRAMEWORKS_UNITY_FRAMEWORK_TO_ADD = {
			"GameKit.framework",
		};
		
		private static readonly string[] FRAMEWORKS_FCM_TO_ADD = {
			"UserNotifications.framework",
			"UIKit.framework",
		};

		[PostProcessBuild(1)]
		private static void OnPostProcessBuild(BuildTarget buildTarget, string path)
		{
			if (buildTarget != BuildTarget.iOS)
			{
				return;
			}
			
			var projectPath = PBXProject.GetPBXProjectPath(path);
			var project = new PBXProject();
			project.ReadFromString(File.ReadAllText(projectPath));
			
			var mainTargetName = GetPBXProjectTargetName(project);
			var mainTargetGUID = GetPBXProjectTargetGUID(project);
			var unityFrameworkGUID = GetPBXProjectUnityFrameworkGUID(project);
			
			foreach(var framework in FRAMEWORKS_MAIN_TO_ADD) {
				project.AddFrameworkToProject(mainTargetGUID, framework, false);
			}
        
			foreach(var framework in FRAMEWORKS_UNITY_FRAMEWORK_TO_ADD) {
				project.AddFrameworkToProject(unityFrameworkGUID, framework, false);
			}
			
			ModifyPlistFile(path);
			
			// 关闭Bitode
			project.SetBuildProperty(mainTargetGUID, "ENABLE_BITCODE", "NO");
			project.SetBuildProperty(unityFrameworkGUID, "ENABLE_BITCODE", "NO");

			// 添加 UnityFramework 版本号
			project.SetBuildProperty(unityFrameworkGUID, "CURRENT_PROJECT_VERSION", PlayerSettings.bundleVersion);
			project.SetBuildProperty(unityFrameworkGUID, "MARKETING_VERSION", PlayerSettings.iOS.buildNumber);

			AddOrUpdateEntitlements(path, project, mainTargetGUID, mainTargetName,
				new HashSet<EntitlementOptions>
				{
					EntitlementOptions.AppGroups
				});
			
			// AddNotificationServiceExtension(project ,path); // <--- 无需添加Extension了
			
			project.WriteToFile(projectPath);
			var contents = File.ReadAllText(projectPath);
			project.ReadFromString(contents);
			
			// Add push notifications as a capability on the main app target
			AddPushCapability(project, path, mainTargetGUID, mainTargetName);

			project.SetBuildProperty(unityFrameworkGUID, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
			project.SetBuildProperty(mainTargetGUID, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");
			File.WriteAllText(projectPath, project.WriteToString());
		}
		
		private static void ModifyPlistFile(string pathToBuildProject)
		{
			var plistPath = Path.Combine(pathToBuildProject, "Info.plist");
			var plist = new PlistDocument();
			plist.ReadFromFile(plistPath);
			//设置Google AD GADApplicationIdentifier
			plist.root.SetString("NSCalendarsUsageDescription", "Store calendar events from ads");
			// plist.root.SetString("GADApplicationIdentifier", "ca-app-pub-2436733915645843~7788635385");
			// plist.root.SetString("FacebookClientToken", "2414c9079473645856a5ef6b8ac95cf6");
			// plist.root.SetString("FacebookDisplayName", PlayerSettings.productName);
			//设置Xcode的Att弹窗配置
			plist.root.SetString("NSUserTrackingUsageDescription","By allowing tracking, we'll be able to better tailor ads served to you on this game.");
			//设置SKAdNetworkItems
			// ReadSKADNetworkPlistFile();
			// var plistElementArray = plist.root.CreateArray("SKAdNetworkItems");
			// AddPlatformADNetworkIdentifier(plistElementArray, NETWORK_IDENTIFIER_ARRAY);
			
			// 设置合规出口证明
			plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

			var root = plist.root.values;
			PlistElement atsRoot;
			root.TryGetValue("NSAppTransportSecurity", out atsRoot);

			if (atsRoot == null || atsRoot.GetType() != typeof(PlistElementDict))
			{
				atsRoot = plist.root.CreateDict("NSAppTransportSecurity");
				atsRoot.AsDict().SetBoolean("NSAllowsArbitraryLoads", true);
			}

			var atsRootDict = atsRoot.AsDict().values;
			if (atsRootDict.ContainsKey("NSAllowsArbitraryLoadsInWebContent"))
			{
				atsRootDict.Remove("NSAllowsArbitraryLoadsInWebContent");
			}
        
			plist.WriteToFile(plistPath);
		}
		
		#region 纯功能函数
		
		private static void AddOrUpdateEntitlements(string path, PBXProject project, string targetGUI,
			string targetName, HashSet<EntitlementOptions> options)
		{
			string entitlementPath = GetEntitlementsPath(path, project, targetGUI, targetName);
			var entitlements = new PlistDocument();

			// Check if the file already exisits and read it
			if (File.Exists(entitlementPath)) {
				entitlements.ReadFromFile(entitlementPath);
			}

			// TOOD: This can be updated to use project.AddCapability() in the future
			if (options.Contains(EntitlementOptions.AppGroups) && entitlements.root["com.apple.security.application-groups"] == null) {
				var groups = entitlements.root.CreateArray("com.apple.security.application-groups");
				groups.AddString("group." + PlayerSettings.applicationIdentifier);
			}

			entitlements.WriteToFile(entitlementPath);

			// Copy the entitlement file to the xcode project
			var entitlementFileName = Path.GetFileName(entitlementPath);
			var relativeDestination = targetName + "/" + entitlementFileName;

			// Add the pbx configs to include the entitlements files on the project
			project.AddFile(relativeDestination, entitlementFileName);
			project.AddBuildProperty(targetGUI, "CODE_SIGN_ENTITLEMENTS", relativeDestination);
		}
		
		private static void AddPushCapability(PBXProject project, string path, string targetGUID, string targetName)
		{
			var projectPath = PBXProject.GetPBXProjectPath(path);
			//project.AddCapability(targetGUID, PBXCapabilityType.PushNotifications);
			//project.AddCapability(targetGUID, PBXCapabilityType.BackgroundModes);

			var entitlementsPath = GetEntitlementsPath(path, project, targetGUID, targetName);
			// NOTE: ProjectCapabilityManager's 4th constructor param requires Unity 2019.3+
			var projCapability = new ProjectCapabilityManager(projectPath, entitlementsPath, targetName);
			//projCapability.AddBackgroundModes(BackgroundModesOptions.BackgroundFetch);
			//projCapability.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
			projCapability.AddPushNotifications(false);
			projCapability.WriteToFile();
		}

		
		private static string GetPBXProjectTargetName(PBXProject project)
		{
			// var projectUUID = project.GetUnityMainTargetGuid();
			// return project.GetBuildPhaseName(projectUUID);
			// The above always returns null, using a static value for now.
			return DEFAULT_PROJECT_TARGET_NAME;
		}
    
		private static string GetPBXProjectTargetGUID(PBXProject project)
		{
			return project.GetUnityMainTargetGuid();
		}

		private static string GetPBXProjectUnityFrameworkGUID(PBXProject project)
		{
			return project.GetUnityFrameworkTargetGuid();
		}
    
		private static string GetEntitlementsPath(string path, PBXProject project, string targetGUI, string targetName)
		{
			// Check if there is already an eltitlements file configured in the Xcode project
			var relativeEntitlementPath = project.GetBuildPropertyForConfig(targetGUI, "CODE_SIGN_ENTITLEMENTS");
			if (relativeEntitlementPath != null) {
				var entitlementPath = path + DIR_CHAR + relativeEntitlementPath;
				if (File.Exists(entitlementPath)) {
					return entitlementPath;
				}
			}

			// No existing file, use a new name
			return path + DIR_CHAR + targetName + DIR_CHAR + targetName + ".entitlements";
		}
		#endregion
	}
}

#endif