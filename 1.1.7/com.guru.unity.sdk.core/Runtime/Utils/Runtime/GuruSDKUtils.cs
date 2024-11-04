

namespace Guru
{
    using UnityEngine;
    using System.Collections.Generic;
    
    public static class GuruSDKUtils
    {
        public static Color HexToColor(string hexString)
        {
            if(string.IsNullOrEmpty(hexString)) return Color.clear;
            
            var hex = hexString.Replace("#", "");
            if(hex.Length < 6) return Color.clear;
            
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte a = 255;
            if (hex.Length >= 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return new Color(r, g, b, a);
        }
        
        public static Dictionary<string, object> MergeDictionary(Dictionary<string, object> source, Dictionary<string, object> other)
        {
            int len = source?.Count ?? 0 + other?.Count ?? 0;
            if (len == 0) len = 10;
            var newDict = new Dictionary<string, object>(len);
            if (source != null)
            {
                foreach (var k in source.Keys)
                {
                    newDict[k] = source[k];
                }
            }
            
            if (other != null)
            {
                foreach (var k in other.Keys)
                {
                    newDict[k] = other[k];
                }
            }
            return newDict;
        }
    }
}