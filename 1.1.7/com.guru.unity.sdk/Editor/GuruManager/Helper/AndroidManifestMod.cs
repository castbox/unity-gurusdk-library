
namespace Guru.Editor
{
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;
    using System;
    using System.IO;
    using System.Xml;
    
    public static class AndroidManifestMod
    {
        private const string TargetPath = "Plugins/Android/AndroidManifest.xml";
        private const string ValOptimizeInitialization = "com.google.android.gms.ads.flag.OPTIMIZE_INITIALIZATION";
        private const string ValOptimizeAdLoading = "com.google.android.gms.ads.flag.OPTIMIZE_AD_LOADING";

        private const string PermissionReadPostNotifications = "android.permission.POST_NOTIFICATIONS";
        private const string PermissionReadPhoneState = "android.permission.READ_PHONE_STATE";
        private const string PermissionAccessCoarseLocation = "android.permission.ACCESS_COARSE_LOCATION";
        private const string PermissionAccessFineLocation = "android.permission.ACCESS_FINE_LOCATION";
        private const string PermissionReadExternalStorage = "android.permission.READ_EXTERNAL_STORAGE";
        private const string PermissionReadLogs = "android.permission.READ_LOGS";
        private const string NetworkSecurityConfig = "networkSecurityConfig";
        private const string NetworkSecurityConfigValue = "@xml/network_security_config";
        private const string PermissionAdjustReadPermission = "com.adjust.preinstall.READ_PERMISSION"; // Adjust permission
        private const string AdjustQueriesActionValue = "com.attribution.REFERRAL_PROVIDER"; // Adjust action
        
        // Add Permissions
        private static string[] addPermissions = new[]
        {
            PermissionReadPostNotifications,
            PermissionAdjustReadPermission,
        };
        
        // Remove Permissions
        private static string[] removePermissions = new[]
        {
            PermissionReadPhoneState,
            PermissionAccessCoarseLocation,
            PermissionAccessFineLocation,
            PermissionReadExternalStorage,
            PermissionReadLogs,
        };
        

        private static string TargetFullPath = Path.Combine(Application.dataPath, TargetPath);
        
        public static bool IsManifestExist() => File.Exists(TargetFullPath);

        public static void Apply()
        {
            if (!IsManifestExist())
            {
                CopyManifest();
            }
            
            FixAndroidManifest();
        }

        
        /// <summary>
        /// Fix Android Manifest
        /// </summary>
        private static void FixAndroidManifest()
        {
            var doc = AndroidManifestDoc.Load(TargetFullPath);
            
            // --- network_security_config ---
            doc.SetApplicationAttribute(NetworkSecurityConfig, NetworkSecurityConfigValue);
            doc.AddApplicationReplaceItem($"android:{NetworkSecurityConfig}");
            // ---- Metadata ---
            doc.SetMetadata(ValOptimizeInitialization, "true");
            doc.SetMetadata(ValOptimizeAdLoading, "true");
            // ---- Permission ---
            // Add needed permissions
            foreach (var p in addPermissions)
            {
                doc.AddPermission(p);
            }
            // Remove sensitive permissions
            foreach (var p in removePermissions)
            {
                doc.RemovePermission(p);
            }

            // --- Bundle Id ---
            doc.SetPackageName(PlayerSettings.applicationIdentifier);
            
            // --- Adjust Preinstall (Content provider) ---
            doc.AddQueriesIntent(AdjustQueriesActionValue);
            
            doc.Save();
        }
        

        /// <summary>
        /// 拷贝 AndroidManifest
        /// </summary>
        private static void CopyManifest()
        {
            if (File.Exists(TargetFullPath)) return;
            
            var path = GuruEditorHelper.GetAssetPath(nameof(AndroidManifestMod), "Script", true);
            if (!string.IsNullOrEmpty(path))
            {
                var from = Path.GetFullPath($"{path}/../../Files/AndroidManifest.txt");
                if (File.Exists(from))
                {
                    File.Copy(from, TargetFullPath);
                }
            }
        }
        
        #region Testing

        [Test]
        public static void Test_Injection()
        {
            Apply();
        }

        #endregion
        
    }
}