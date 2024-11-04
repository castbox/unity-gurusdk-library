#if UNITY_IOS
namespace Guru.Editor
{
	using System.Collections.Generic;
	using System.IO;
	using UnityEditor;
	using UnityEditor.Callbacks;
	using UnityEditor.iOS.Xcode;
	using UnityEngine;

	// ----------- SKAdNetwork 更新版本日志： 2024-06-27 --------------------	

	/// <summary>
	/// SKAdNetwork 注入逻辑
	/// </summary>
	public static class IOSPostBuild_SKAdNetwork
	{
		private static List<string> NETWORK_IDENTIFIER_ARRAY = new List<string>();
		public static readonly string SKADNetworkIdentifier = "SKAdNetworkIdentifier";
		
		private static readonly char DIR_CHAR = Path.DirectorySeparatorChar;
		public static readonly string OS_PLATFORM_LOCATION = $"Assets/Guru/GuruBuildTool/Editor/IOS_POST_AD/";
		public static readonly string SKADNetworkName = "SKADNetwork.plist";
		
		[PostProcessBuild(10)]
		private static void OnPostProcessBuild(BuildTarget buildTarget, string path)
		{
			if (buildTarget != BuildTarget.iOS)
			{
				return;
			}
			
			var plistPath = Path.Combine(path, "Info.plist");
			var plist = new PlistDocument();
			plist.ReadFromFile(plistPath);
			
			//设置SKAdNetworkItems
			ReadSKADNetworkPlistFile();
			var plistElementArray = plist.root.CreateArray("SKAdNetworkItems");
			AddPlatformADNetworkIdentifier(plistElementArray, NETWORK_IDENTIFIER_ARRAY);
			plist.WriteToFile(plistPath);
		}
		
		public static void ReadSKADNetworkPlistFile()
		{
			string plistPath = Path.Combine(GetToolRootDir(), SKADNetworkName);
			if (File.Exists(plistPath))
			{
				var plist = new PlistDocument();
				plist.ReadFromFile(plistPath);
				var skADNetworksArr = plist.root["SKAdNetworkItems"].AsArray();
				if (skADNetworksArr != null)
				{
					foreach (var plistElement in skADNetworksArr.values)
					{
						var adNetworkValue = plistElement.AsDict()[SKADNetworkIdentifier].AsString();
						if(!NETWORK_IDENTIFIER_ARRAY.Contains(adNetworkValue))
							NETWORK_IDENTIFIER_ARRAY.Add(adNetworkValue);
					}
				}
			}
			else
			{
				Debug.Log($"[POST] --- Inject SKADNetwork Failed: {plistPath}");
			}
			
		}
		
		private static void AddPlatformADNetworkIdentifier(PlistElementArray plistElementArray, List<string> arrays)
		{
			foreach (var value in arrays)
			{
				PlistArrayAddDict(plistElementArray, value);
			}
		}
		
		private static void PlistArrayAddDict(PlistElementArray plistElementArray, string value)
		{
			plistElementArray.AddDict().SetString(SKADNetworkIdentifier, value);
		}


		private static string GetToolRootDir()
		{
			var guids = AssetDatabase.FindAssets($"{nameof(IOSPostBuild_SKAdNetwork)}");
			if (guids.Length > 0)
			{
				var path = Directory.GetParent(AssetDatabase.GUIDToAssetPath(guids[0])).FullName;
				return path;
			}
			return $"{Application.dataPath.Replace("Assets", "Packages")}/com.guru.unity.sdk.core/Editor/GuruBuildSuit/IOS_POST_AD";
		}

	}
}


#endif