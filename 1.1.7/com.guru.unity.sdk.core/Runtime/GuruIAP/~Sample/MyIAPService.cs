using UnityEngine.Purchasing;

namespace Guru.Sample
{
    public class MyIAPService: IAPServiceBase<MyIAPService>
    {
        /// <summary>
        /// 必须实现 BLevel 反回值, 用于支付打点上报 
        /// </summary>
        /// <returns></returns>
        protected override int GetBLevel()
        {
            // return UserData.Instance.GetLastFinishedLevel(); // 游戏目前进度完成的最后一关
            return 1;
        }

        public override void Initialize(string uid, bool showLog = false)
        {
            base.Initialize(uid,true);
            InitGameProducts();
        }

        private void InitGameProducts()
        {
            // 插入项目专用的初始化逻辑
            foreach (var key in Products.Keys)
            {
                var info = Products[key];
                if (info.Setting.Type == ProductType.Subscription)
                {
                    // TODO: 针对订阅行道具进行处理逻辑
                }
            }
        }

        protected override void OnPurchaseOver(bool success, string productName)
        {
            switch (productName)
            {
                // TODO: 请在此处处理购买
            }
        }
    }
}