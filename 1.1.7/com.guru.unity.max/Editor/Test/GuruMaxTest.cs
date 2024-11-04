using System.Collections;
using System.Collections.Generic;
using Guru.Editor.Max;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Guru.Editor
{
    public class GuruMaxTest
    {
        [Test]
        public void TestSettingsPath()
        {
            var sttings = AppLovinSettings.Instance;

            var path = AssetDatabase.GetAssetPath(sttings.GetInstanceID());
            Debug.Log($"path: {path}");
            Debug.Log($"SdkKey: {sttings.SdkKey}");
            Debug.Log($"AdMobAndroidAppId: {sttings.AdMobAndroidAppId}");
            Debug.Log($"AdMobIosAppId: {sttings.AdMobIosAppId}");

        }

#if GURU_SDK_DEV
        // 测试API检测地址
        [Test]
        public void Test_GetAPIPath()
        {
            var aipPath = GuruMaxSdkAPI.DevPackageRoot;
            Debug.Log($"--- aipPath: {aipPath}");
        }
#endif
    }
}
