using System.IO;
using UnityEngine;

namespace Guru
{
    public class GuruAppVersion
    {
        public const string BuildInfoName = "build_info";

        public string raw;
        public string version;
        public string code;

        public override string ToString()
        {
            return $"{version}-{code}";
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public GuruAppVersion()
        {
            version = Application.version;
            code = "unknown";
            raw = "";
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="version"></param>
        /// <param name="code"></param>
        public GuruAppVersion(string version, string code)
        {
            this.version = version;
            this.code = code;
        }

        public static GuruAppVersion Load()
        {
            var raw = Resources.Load<TextAsset>(BuildInfoName)?.text??"";
            return GuruAppVersion.Parse(raw);
        }

        
        protected static GuruAppVersion Parse(string raw)
        {
            var a = new GuruAppVersion();
            if (!string.IsNullOrEmpty(raw))
            {
                a.raw = raw;
                var arr = raw.Split('-');
                if (arr.Length > 0) a.version = arr[0];
                if (arr.Length > 1) a.code = arr[1];
            }
            return a;
        }


        /// <summary>
        /// 保存至磁盘
        /// </summary>
        /// <param name="version"></param>
        /// <param name="code"></param>
        public static void SaveToDisk(string version, string code)
        {
            var dir = $"{Application.dataPath}/Guru/Resources";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var path = $"{dir}/{BuildInfoName}.txt";
            File.WriteAllText(path,  $"{version}-{code}");
        }

    }


 
}