# PubMatic 广告插件

## Version 1.0.0

使用方法:
- 先解压缩Package内的zip文档
- 导入两个unitypackage文件
- 将 PubMatic 对应的的 AdChannelPubMatic.cs 文件导入项目
- 在 AdService 内注册AdChannelPubMatic. (需要升级 ADService )
- 在 PlayerSettings 内添加 `AD_PUBMATIC` 的宏
- 在 proguard-user.txt 内添加下面的代码:
    ```yaml
    # PubMatic 
    -keep class com.pubmatic.** { *; }
    ```

广告申请和展示时按照正常的 MAX 广告申请接口调用
 
测试时, 需要使用专用的Debug接口直接拉起 PubMatic 测试广告
```csharp

    Debug.Log($"--- 显示 PubMatic Banner ---");
    AdChannelPubMatic.RequestDebugBanner();

    Debug.Log($"--- 显示 PubMatic IV ---");
    AdChannelPubMatic.LoadDebugIV();

    Debug.Log($"--- 显示 PubMatic RV ---");
    AdChannelPubMatic.RequestDebugRV();

```