using System;

public static class AmazonConstants
{
    public const string VERSION = "2.0.0";
    public const string RELEASE_NOTES_URL = "https://ams.amazon.com/webpublisher/uam/docs/aps-mobile/resources"; //TODO : add Unity Release Notes link

    public const string titleAboutDialog = "About Amazon SDK";
    public const string labelSdkVersion = "Amazon SDK version " + AmazonConstants.VERSION;
    public const string buttonReleaseNotes = "Release Notes";
    public const string labelReportIssues = "Report Issues: " + "Mobile-aps-support@amazon.com";

    public const string aboutDialogOk = "OK";

    public const string manifestURL = "https://mdtb-sdk-packages.s3-us-west-2.amazonaws.com/Unity/aps_unity.json";
    public const string helpLink = "https://ams.amazon.com/webpublisher/uam/docs/aps-mobile/resources";
    public const string docUrl = "https://ams.amazon.com/webpublisher/uam/docs/aps-mobile";


    internal const string unityPlayerClass = "com.unity3d.player.UnityPlayer";

    // Android constant names
    internal const string sdkUtilitiesClass = "com.amazon.device.ads.SDKUtilities";
    internal const string dtbAdViewClass = "com.amazon.device.ads.DTBAdView";
    internal const string dtbAdInterstitialClass = "com.amazon.device.ads.DTBAdInterstitial";
    internal const string apsAdNetworkClass = "com.amazon.aps.ads.model.ApsAdNetwork";
    internal const string apsAdNetworkInfoClass = "com.amazon.aps.ads.ApsAdNetworkInfo";

}

