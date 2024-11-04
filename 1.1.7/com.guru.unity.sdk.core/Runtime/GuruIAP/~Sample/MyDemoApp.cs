using System;
using UnityEngine;

namespace Guru.Sample
{
    public class MyDemoApp: MonoBehaviour
    {
        private void Awake()
        {
            // 初始化回调
            MyIAPService.Instance.OnInitResult += success =>
            {
                if (success)
                {
                    // UIManager.Instance.OpenStoreUI();
                }
            };
        }


        /// <summary>
        /// 点击支付按钮
        /// </summary>
        /// <param name="productId"></param>
        private void OnClickBuyItem(string productId)
        {
            MyIAPService.Instance
                .Buy(productId)
                .OnBuyEnd += (productName, success) =>
                {
                    if (success)
                    {
                        Debug.Log($"Product {productName} isSuccess!"); 
                    }
                    else
                    {
                        Debug.Log($"Product {productName} isFail!");
                    }
                };
        }
    }
}