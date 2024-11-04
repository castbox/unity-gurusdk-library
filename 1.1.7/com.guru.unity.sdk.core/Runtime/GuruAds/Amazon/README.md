# GuruAds 扩展 Amazon安装说明

## Version 1.0.0

## 更新内容：
- 添加 Amazon 广告适配兼容


## Amazon 使用说明

### 介绍
- 本扩展基于 GuruCore.Ads.ADService `1.2.0` 开发，
  > 低于该版本的Guru框架请先升级至[ Guru 1.8.0 ](https://raw.githubusercontent.com/castbox/unity_gurucore/main/gurucore_1.8.0.unitypackage)
- 导入本扩展后， 需要先手动安装 APS 插件
  > unitypackage 可从以下路径解压缩后自行安装：
  >   
  > Assets/Guru/GuruAds/Editor/Pcakge/unity_aps_1.4.3.zip
  > 
- 安装完毕后，请打开 GuruSettings 设置对应的 Amazon 广告配置参数 ，详见 `Amazon Setting`

### 代码调用

- 请直接使用 `ADServiceAPS` 来完成所有广告生命周期相关的功能， 它继承了 `IADService`
- 实际上 `ADServiceAPS` 继承自 `ADService`。 具备基础功能的同时实现了Amazon的广告请求逻辑。
- 所有的广告申请， 加载和播放结果的回调同之前是一致的。
  ```C#
    
    // 定义广告实例    
    var service = ADService.Instance;
    
    // 启动广告服务
    bool isDebugMode = true;
    service.StartService(() =>
    {
        //TODO 执行广告初始化成功后的逻辑
    }, isDebugMode);

    // 显示 Banner
    service.ShowBanner();
      
    // 显示 IV
    service.ShowInterstitialAD("level_end");
      
    // 显示 RV
    service.ShowRewardedAD("add_free_coin", () =>
    {
        UserData.Coin += 10;
    });



  ```