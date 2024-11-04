

namespace Guru
{
    using System.Collections.Generic;
    using Facebook.Unity;
    
    // item_category: reward/iap_buy/igc/props/bonus/behavior/others
    // item_name: 
    
    /// <summary>
    /// 经济统计接口
    /// </summary>
    public partial class Analytics
    {
        //----------- 货币类型常用名称 ------------------
        
        public const string CurrencyNameCoin = "coin";
        public const string CurrencyNameCash = "cash";
        public const string CurrencyNameDiamond = "diamond";
        public const string CurrencyNameGem = "gem";
        public const string CurrencyNameCrystal = "crystal";
        public const string CurrencyNameWood = "wood";
        public const string CurrencyNameStone = "stone";
        public const string CurrencyNameIron = "iron";
        public const string CurrencyNameOre = "ore";
        public const string CurrencyNameGas = "gas";
        public const string CurrencyNameGold = "gold";
        public const string CurrencyNameSilver = "silver";
        public const string CurrencyNameCopper = "copper";
        
        //----------- 货币类型常用名称 ------------------
        
        /// <summary>
        /// 货币获取的类别
        /// </summary>
        public enum CurrencyCategory
        {
            Reward,
            IapBuy,
            Igc,
            Props,
            Bonus,
            Behavior,
            Others,
        }

        /// <summary>
        /// 枚举转字符串
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        private static string GetCategoryString(CurrencyCategory category)
        {
            switch (category)
            {
                case CurrencyCategory.Reward:
                    return "reward";
                case CurrencyCategory.IapBuy:
                    return "iap_buy";
                case CurrencyCategory.Igc:
                    return "igc";
                case CurrencyCategory.Props:
                    return "props";
                case CurrencyCategory.Bonus:
                    return "bonus";
                case CurrencyCategory.Behavior:
                    return "behavior";
            }
            return "others";
        }


        /// <summary>
        /// 获取虚拟货币/道具
        /// </summary>
        /// <param name="currencyName"></param>
        /// <param name="value"></param>
        /// <param name="balance"></param>
        /// <param name="category"></param>
        /// <param name="levelName"></param>
        /// <param name="isIap"></param>
        /// <param name="itemName"></param>
        /// <param name="scene"></param>
        public static void EarnVirtualCurrency(string currencyName, 
            int value = 1, int balance = 0, 
            string category = "", 
            string itemName = "",
            string levelName = "0",
            string scene = "", Dictionary<string, object> extra = null)
        {
            var data = new Dictionary<string, object>()
            {
                { ParameterVirtualCurrencyName, currencyName },
                { ParameterValue, value },
                { ParameterBalance, balance },
                { ParameterLevelName, levelName },
                { ParameterItemName, itemName }, 
                { ParameterItemCategory, category },
                { ParameterScene, scene },
            };
            
            if(!string.IsNullOrEmpty(scene)) data[ParameterScene] = scene; // 获取的虚拟货币或者道具的场景
            if (extra != null) data.AddRange(extra, isOverride: true);
            
            TrackEvent(EventEarnVirtualCurrency, data, new EventSetting()
            {
                EnableFirebaseAnalytics = true,
                EnableGuruAnalytics = true
            });
            
            // FB 上报收入点
            FBService.LogEvent(EventEarnVirtualCurrency, value, data);
        }
        
        
        public static void SpendVirtualCurrency(string currencyName, 
            int value = 1, int balance = 0, 
            string category = "", 
            string itemName = "",
            string levelName = "0",
            string scene = ""
            , Dictionary<string, object> extra = null)
        {
            var data = new Dictionary<string, object>()
            {
                { ParameterVirtualCurrencyName, currencyName },
                { ParameterValue, value },
                { ParameterBalance, balance },
                { ParameterLevelName, levelName },
                { ParameterItemCategory, category },
                { ParameterItemName, itemName },
            };
            if (extra != null) data.AddRange(extra, isOverride: true);
            
            if(!string.IsNullOrEmpty(scene)) data[ParameterScene] = scene; // 获取的虚拟货币或者道具的场景
            
            TrackEvent(EventSpendVirtualCurrency, data, new EventSetting()
            {
                EnableFirebaseAnalytics = true,
                EnableGuruAnalytics = true
            });
            
            // FB 上报消费点
            FBService.LogEvent(EventSpendVirtualCurrency, value, data);
            // FB 上报消耗事件买量点
            FBSpentCredits(value, itemName, category);  // 点位信息有变化
        }


        /// <summary>
        /// FB 消耗点位上报
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="contentId"></param>
        /// <param name="contentType"></param>
        private static void FBSpentCredits(int amount, string contentId, string contentType)
        {
            FBService.LogEvent(AppEventName.SpentCredits, amount, 
                new Dictionary<string, object>()
                {
                    { AppEventParameterName.ContentID, contentId },
                    { AppEventParameterName.ContentType, contentType }, 
                });
        }



    }
}