namespace Guru
{
    using UnityEngine;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Debug = UnityEngine.Debug;
    using Guru.Network;
    using System.Linq;
    
    public partial class GuruSDK: MonoBehaviour
    {
        // SDK_VERSION
        public const string Version = "1.1.7"; 
        
        // Const
        private const string Tag = "[Guru]";
        public const string ServicesConfigKey = "guru_services";
        
        private static GuruSDK _instance;
        /// <summary>
        /// 单利引用
        /// </summary>
        public static GuruSDK Instance
        {
            get
            {
                if(null == _instance)
                {
                    _instance = CreateInstance();
                }
                return _instance;
            }
            
        }

        private GuruSDKInitConfig _initConfig;

        private static GuruSDKInitConfig InitConfig => Instance._initConfig;
        private static GuruSDKModel Model => GuruSDKModel.Instance;
        private static GuruServicesConfig _appServicesConfig;
        private static GuruSettings _guruSettings;
        private static GuruSettings GuruSettings
        {
            get
            {
                if (_guruSettings == null) _guruSettings = GuruSettings.Instance;
                return _guruSettings;
            }
        }
        
        private static DateTime _initTime;
        private static bool _isDebugEnabled = false;
        /// <summary>
        /// Debug Mode
        /// </summary>
        public static bool IsDebugMode
        {
            get
            {
#if UNITY_EDITOR || DEBUG
                return true;
#endif
                return _isDebugEnabled;
            }
        }

        /// <summary>
        /// 初始化成功标志位
        /// </summary>
        public static bool IsInitialSuccess { get; private set; } = false;
        /// <summary>
        /// Firebase 就绪标志位
        /// </summary>
        public static bool IsFirebaseReady { get; private set; } = false;
        /// <summary>
        /// 服务就绪标志位
        /// </summary>
        public static bool IsServiceReady { get; private set; } = false;

        private Firebase.Auth.FirebaseUser _firebaseUser;
        [Obsolete("获取 FirebaseUser 的属性接口即将废弃，请改用 <GuruSDK.Callbacks.SDK.OnFirebaseUserAuthResult += OnMyGetFirebaseUserCallback> 来异步获取该属性")]
        public static Firebase.Auth.FirebaseUser FirebaseUser => Instance?._firebaseUser ?? null; 
        
        
        
        #region 初始化
        
        private static GuruSDK CreateInstance()
        {
            var go = new GameObject(nameof(GuruSDK));
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<GuruSDK>();
            return _instance;
        }
        
        // TODO : 下个版本需要将 整个 GuruSDK 做功能性的拆分
        
        public static void Init(Action<bool> onComplete)
        {
            Init(GuruSDKInitConfig.Builder().Build(), onComplete);
        }
        
        public static void Init(GuruSDKInitConfig config, Action<bool> onComplete)
        {
            _initTime = DateTime.UtcNow;
            // ----- First Open Time -----
            // SetFirstOpenTime(GetFirstOpenTime());  // FirstOpenTime 
            LogI($"---- Guru SDK [{Version}] ----\n{config}");
            Instance.StartWithConfig(config, onComplete);
        }

        /// <summary>
        /// 启动SDK
        /// </summary>
        /// <param name="config"></param>
        /// <param name="onComplete"></param>
        private void StartWithConfig(GuruSDKInitConfig config, Action<bool> onComplete)
        {
            IsInitialSuccess = false;
            _initConfig = config;
            _isDebugEnabled = config.DebugMode;

            if (config.EnableDebugLogEvent) Analytics.EnableDebugAnalytics = true; // 允许 Debug 模式下打点
            if (!config.AutoNotificationPermission) FirebaseUtil.SetAutoFetchFcmToken(false); // 不允许自动启动获取 FCM Token
            
            InitUpdaters(); // Updaters
            InitThreadHandler(); // 初始化线程处理器
            InitServices(); // 初始化所有的服务
            InitNetworkMonitor(); // 网络状态
            
            onComplete?.Invoke(true);
        }
        
        private void InitServices()
        {
            // --- Start Analytics ---
            LogI($"#1.1 ---- Init Analytics ----");
            Analytics.Init();
            ReportBasicUserProperties(); // 立即上报基础用户属性
            
            //--- Start Firebase ---
            LogI($"#1.2 --- InitFirebase ---");
            FirebaseUtil.Init(OnFirebaseDepsCheckResult, 
                OnGetFirebaseId, 
                OnGetGuruUID, 
                OnFirebaseLoginResult); // 确保所有的逻辑提前被调用到
            
            //--- Start Facebook ---
            LogI($"#1.3 --- InitFacebook ---");
            FBService.Instance.StartService(Analytics.OnFBInitComplete);
            
            IsInitialSuccess = true;
        }

        /// <summary>
        /// 注入云控参数基础数据
        /// </summary>
        /// <returns></returns>
        private string LoadDefaultGuruServiceJson()
        {
            // 加载本地 Services 配置值
            var txtAsset = Resources.Load<TextAsset>(ServicesConfigKey);
            if (txtAsset != null)
            {
                return txtAsset.text;
            }
            return "";
        }
        
        /// <summary>
        /// 拉取云控参数完成
        /// </summary>
        /// <param name="success"></param>
        private void OnFetchRemoteCallback(bool success)
        {
            LogI($"#6 --- Remote fetch complete: {success} ---");
            ABTestManager.Init(); // 启动AB测试解析器
            Callbacks.Remote.InvokeOnRemoteFetchComplete(success);
        }

        private void Update()
        {
            UpdateAllUpdates(); // 驱动所有的更新器
        }


        #endregion

        #region App Remote Update

        /// <summary>
        /// Apply Cloud guru-service configs for sdk assets
        /// </summary>
        private void InitAllGuruServices()
        {
            // -------- Init Analytics ---------
            SetSDKEventPriority();
            // -------- Init Notification -----------
            InitNotiPermission();
            
            bool useKeywords = false;
            bool useIAP = _initConfig.IAPEnabled;
            bool appleReview = false;
            // bool enableAnaErrorLog = false;
            
            //----------- Set GuruServices ----------------
            var services = GetRemoteServicesConfig();
            if (services != null)
            {
                _appServicesConfig = services;
                useKeywords = _appServicesConfig.KeywordsEnabled();
                // 使用初始化配置中的 IAPEnable来联合限定, 如果本地写死关闭则不走云控开启
                useIAP = _initConfig.IAPEnabled && _appServicesConfig.IsIAPEnabled(); 
                // enableAnaErrorLog = _appServicesConfig.EnableAnaErrorLog();
                
                Try(() =>
                {
                    LogI($"#4.1 --- Start apply services ---");
                    //----------------------------------------------------------------

                    // 自打点设置错误上报
                    // if(enableAnaErrorLog) GuruAnalytics.EnableErrorLog = true;
                    
                    // adjust 事件设置
                    if (null != _appServicesConfig.adjust_settings && null != GuruSettings)
                    {
                        // 更新 Adjust Tokens
                        GuruSettings.UpdateAdjustTokens(
                            _appServicesConfig.adjust_settings.AndroidToken(),
                            _appServicesConfig.adjust_settings.iOSToken());
                        // 更新 Adjust Events
                        GuruSettings.UpdateAdjustEvents(_appServicesConfig.adjust_settings.events);
                    }
                
                    LogI($"#4.2 --- Start GuruSettings ---");
                    // GuruSettings 设置
                    if (null != _appServicesConfig.app_settings)
                    {
                        if (_appServicesConfig.Tch02Value() > 0)
                        {
                            Analytics.EnableTch02Event = true;
                            Analytics.SetTch02TargetValue(_appServicesConfig.Tch02Value());
                        }
                     
                        // 设置获取设备 UUID 的方法
                        // if (_appServicesConfig.UseUUID())
                        // {
                        //     IPMConfig.UsingUUID = true; // 开始使用 UUID 作为 DeviceID 标识
                        // }

#if UNITY_IOS 
                        // 苹果审核标志位
                        appleReview = _appServicesConfig.IsAppReview();
#endif
                    
                        if (null !=  GuruSettings)
                        {
                            // 更新和升级 GuruSettings 对应的值
                            GuruSettings.UpdateAppSettings(
                                _appServicesConfig.app_settings.bundle_id,
                                _appServicesConfig.fb_settings?.fb_app_id ?? "",
                                _appServicesConfig.app_settings.support_email,
                                _appServicesConfig.app_settings.privacy_url,
                                _appServicesConfig.app_settings.terms_url,
                                _appServicesConfig.app_settings.android_store,
                                _appServicesConfig.app_settings.ios_store, 
                                _appServicesConfig.parameters?.using_uuid ?? false,
                                _appServicesConfig.parameters?.cdn_host ?? "");
                            
                            _appBundleId = _appServicesConfig.app_settings.bundle_id; // 配置预设的 BundleId
                        }
                    }
                    //---------------------------------
                }, ex =>
                {
                    Debug.LogError($"--- ERROR on apply services: {ex.Message}");
                });
          
                
            }
            //----------- Set IAP ----------------
            if (useIAP)
            {
                // InitIAP(_initConfig.GoogleKeys, _initConfig.AppleRootCerts); // 初始化IAP
                Try(() =>
                {
                    LogI($"#4.3 --- Start IAP ---");
                    if (_initConfig.GoogleKeys == null || _initConfig.AppleRootCerts == null)
                    {
                        LogEx("[IAP] GoogleKeys is null when using IAPService! Integration failed. App will Exit");
                    }
                    
                    InitIAP(UID, _initConfig.GoogleKeys, _initConfig.AppleRootCerts); // 初始化IAP
                }, ex =>
                {
                    Debug.LogError($"--- ERROR on useIAP: {ex.Message}");
                });
            }
            //----------- Set Keywords ----------------
            // 2024/11/1 MAX SDK 1.2.3 版本 升级 MAX 插件至 2.6.1， 废弃了 Keywords 改为 Segments
            // 考虑到项目稳定性，从 1.1.7 版本开始， 低于 1.2.0 的版本皆不再支持 Keywords
            
            
            // if (useKeywords)
            // {
            //     // KeywordsManager.Install(Model.IsIAPUser, Model.SuccessLevelId); // 启动Keyword管理器
            //     Try(() =>
            //     {
            //         LogI($"#4.4 --- Start Keywords ---");
            //         KeywordsManager.Install(Model.IsIapUser, Model.BLevel); // 启动Keyword管理器
            //     }, ex =>
            //     {
            //         Debug.LogError($"--- ERROR on Keywords: {ex.Message}");
            //     });
            // }

#if UNITY_IOS
            if (appleReview)
            {
                // StartAppleReviewFlow(); // 直接显示 ATT 弹窗, 跳过 Consent 流程
                Try(() =>
                {
                    LogI($"#4.5 ---  StartAppleReviewFlow ---");
                    StartAppleReviewFlow(); // 直接显示 ATT 弹窗, 跳过 Consent 流程
                }, ex =>
                {
                    Debug.LogError($"--- ERROR on StartAppleReviewFlow: {ex.Message}");
                });
                return;
            }
#endif
            //----------- Set Consent ----------------
            if (!InitConfig.UseCustomConsent && !appleReview)
            {
                LogI($"#4.6 --- Start Consent Flow ---");
                Try(StartConsentFlow, ex =>
                {
                    Debug.LogError($"--- ERROR on StartConsentFlow: {ex.Message}");
                });
            }
            
#if UNITY_ANDROID
            LogI($"#5.1 --- Android StartAndroidDebug Cmd lines---");
            // Android 命令行调试
            StartAndroidDebugCmds();           
#endif
            
            IsServiceReady = true;
            
            // 中台服务初始化结束
            Callbacks.SDK.InvokeOnGuruServiceReady();
        }
        
        /// <summary>
        /// Get the guru-service cloud config value;
        /// User can fetch the cloud guru-service config by using Custom Service Key
        /// </summary>
        /// <returns></returns>
        private GuruServicesConfig GetRemoteServicesConfig()
        {

            string defaultJson = GetRemoteString(ServicesConfigKey);
            
            bool useCustomKey = false;
            string key = ServicesConfigKey;
            if (!string.IsNullOrEmpty(_initConfig.CustomServiceKey))
            {
                key = _initConfig.CustomServiceKey;
                useCustomKey = true;
            }
            var json = GetRemoteString(key); // Cloud cached data

            if (string.IsNullOrEmpty(json) && useCustomKey && !string.IsNullOrEmpty(defaultJson))
            {
                // No remote data fetched from cloud, should use default values
                json = defaultJson;
                Debug.Log($"{Tag} --- No remote data found with: {key}  -> Using default key {ServicesConfigKey} and local data!!!");
            }

            if (!string.IsNullOrEmpty(json))
            {
                return JsonParser.ToObject<GuruServicesConfig>(json);
            }
            
            return null;
        }

        private void Try(Action method, Action<Exception> onException = null, Action onFinal = null)
        {
            if (method == null) return;

            try
            {
                method.Invoke();
            }
            catch (Exception ex)
            {
                LogEx(ex);
                // ignored
                onException?.Invoke(ex);
            }
            finally
            {
                // Finally
                onFinal?.Invoke();
            }
        }


        #endregion

        #region Apple 审核流程逻辑

#if UNITY_IOS
        private void StartAppleReviewFlow()
        {
            CheckAttStatus();
        }
#endif
        #endregion
        
        #region Logging

        private static void LogI(object message)
        {
            Debug.Log($"{Tag} {message}");
        }

        private static void LogW(object message)
        {
            Debug.LogWarning($"{Tag} {message}");
        }

        private static void LogE(object message)
        {
            Debug.LogError($"{Tag} {message}");
        }


        private static void LogEx(string message)
        {
            LogEx( new Exception($"{Tag} {message}"));
        }

        private static void LogEx(Exception e)
        {
            Debug.LogException(e);
        }
        
        /// <summary>
        /// 上报崩溃信息
        /// </summary>
        /// <param name="message"></param>
        public static void Report(string message)
        {
            Analytics.LogCrashlytics(message, false);
        }
        
        /// <summary>
        /// 上报异常
        /// </summary>
        /// <param name="message"></param>
        public static void ReportException(string message)
        {
            Analytics.LogCrashlytics(message);
        }
        
        /// <summary>
        /// 上报异常 Exception
        /// </summary>
        /// <param name="ex"></param>
        public static void ReportException(Exception ex)
        {
            Analytics.LogCrashlytics(ex);
        }

        #endregion

        #region 生命周期

        /// <summary>
        /// 暂停时处理
        /// </summary>
        /// <param name="paused"></param>
        private void OnAppPauseHandler(bool paused)
        {
            if(paused) Model.Save(true); // 强制保存数据
            Callbacks.App.InvokeOnAppPaused(paused);
        }
        
        private void OnApplicationPause(bool paused)
        {
            OnAppPauseHandler(paused);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            OnAppPauseHandler(!hasFocus);
        }

        private void OnApplicationQuit()
        {
            Model.Save(true);
            Callbacks.App.InvokeOnAppQuit();
        }

        #endregion

        #region 延迟处理

        /// <summary>
        /// 启动协程
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        public static Coroutine DoCoroutine(IEnumerator enumerator)
        {
            return Instance != null ? Instance.StartCoroutine(enumerator) : null;
        }

        public static void KillCoroutine(Coroutine coroutine)
        {
            if(coroutine != null)
                Instance.StopCoroutine(coroutine);
        }
        
        /// <summary>
        /// 延时执行
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="callback"></param>
        public static Coroutine Delay(float seconds, Action callback)
        {
            return DoCoroutine(Instance.OnDelayCall(seconds, callback));
        }

        private IEnumerator OnDelayCall(float delay, Action callback)
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            else
            {
                yield return null;
            }
            callback?.Invoke();
        }
        
        #endregion

        #region 帧更新 Updater

        
        private List<IUpdater> _updaterRunningList;
        private List<IUpdater> _updaterRemoveList;

        private void InitUpdaters()
        {
            _updaterRunningList = new List<IUpdater>(20);
            _updaterRemoveList = new List<IUpdater>(20);
        }

        private void UpdateAllUpdates()
        {
            int i = 0;
            // ---- Updater Trigger ----
            if (_updaterRunningList.Count > 0)
            {
                i = 0;
                while (i < _updaterRunningList.Count)
                {
                    var updater = _updaterRunningList[i];
                    if (updater != null) 
                    {
                        if (updater.State == UpdaterState.Running)
                        {
                            updater.OnUpdate();
                        }
                        else if(updater.State == UpdaterState.Kill)
                        {
                            _updaterRemoveList.Add(updater);
                        }
                    }
                    else
                    {
                        _updaterRunningList.RemoveAt(i);
                        i--;
                    }
                    i++;
                }

            }

            if (_updaterRemoveList.Count > 0)
            {
                i = 0;
                while (i < _updaterRemoveList.Count)
                {
                    RemoveUpdater(_updaterRemoveList[i]);
                    i++;
                }
                _updaterRemoveList.Clear();
            }
        }

        /// <summary>
        /// 注册帧更新器
        /// </summary>
        /// <param name="updater"></param>
        public static void RegisterUpdater(IUpdater updater)
        {
            Instance.AddUpdater(updater);
            updater.Start();
        }


        private void AddUpdater(IUpdater updater)
        {
            if (_updaterRunningList == null) _updaterRunningList = new List<IUpdater>(20);
            _updaterRunningList.Add(updater);
        }

        private void RemoveUpdater(IUpdater updater)
        {
            if (_updaterRunningList != null && updater != null)
            {
                _updaterRunningList.Remove(updater);
            }
        }

        #endregion

        #region 中台推送管理

        private static int _messageRetry = 0;
        public static void SetPushNotificationEnabled(bool enabled)
        {
            DeviceInfoUploadRequest request = (DeviceInfoUploadRequest) new DeviceInfoUploadRequest()
                .SetRetryTimes(1)
                .SetSuccessCallBack(() =>
                {
                    _messageRetry = 0;
                    Debug.Log($"[SDK] --- Set Push Enabled: {enabled} success");
                })
                .SetFailCallBack(() =>
                {
                    double retryDelay = Math.Pow(2, _messageRetry);
                    _messageRetry++;
                    CoroutineHelper.Instance.StartDelayed((float)retryDelay, ()=> SetPushNotificationEnabled(enabled));
                });

            if (request == null) return;
            
            request.SetPushEnabled(enabled);
            request.Send();
        }
        #endregion

        #region Deeplink
        
        /// <summary>
        /// 添加回调链接
        /// </summary>
        /// <param name="deeplink"></param>
        private void OnDeeplinkCallback(string deeplink)
        {
           Callbacks.SDK.InvokeDeeplinkCallback(deeplink); // 尝试调用回调
        }
        
        #endregion

        #region 网络状态上报

        private NetworkStatusMonitor _networkStatusMonitor;
        private string _lastNetworkStatus;
        
        private void InitNetworkMonitor()
        {
            _networkStatusMonitor = new NetworkStatusMonitor(Analytics.SetNetworkStatus, 
                lastStatus =>
            {
                LogEvent("guru_offline", new Dictionary<string, dynamic>()
                {
                    ["from"] = lastStatus
                });
            });
        }
        
        /// <summary>
        /// 获取当前的网络状态
        /// </summary>
        /// <returns></returns>
        private string GetNetworkStatus() => _networkStatusMonitor.GetNetworkStatus();

        
        #endregion

        #region Firebase 服务

        private void OnGetGuruUID(bool success)
        {
            if (success)
            {
                var uid = IPMConfig.IPM_UID;
                
                Model.UserId = uid;
                if (GuruIAP.Instance != null)
                {
                    GuruIAP.Instance.SetUID(uid);
                    GuruIAP.Instance.SetUUID(UUID);
                }
                
                // 自打点设置用户 ID
                SetUID(uid);
                
                // 上报所有的事件
                Analytics.ShouldFlushGuruEvents();
            }
            
            Callbacks.SDK.InvokeOnGuruUserAuthResult(success);
        }
        
        private void OnGetFirebaseId(string fid)
        {
            // 初始化 Adjust 服务
            InitAdjustService(fid, InitConfig.OnAdjustDeeplinkCallback);
            // 初始化自打点
            Analytics.InitGuruAnalyticService(fid);
            //---------- Event SDK Info ------------
            LogI($"#6.0 --- SDK is ready, report Info ---");
            LogSDKInfo((DateTime.UtcNow - _initTime).TotalSeconds);
        }
        
        // TODO: 需要之后用宏隔离应用和实现
        // Auth 登录认证
        private void OnFirebaseLoginResult(bool success, Firebase.Auth.FirebaseUser firebaseUser)
        {
            _firebaseUser = firebaseUser;
            Callbacks.SDK.InvokeOnFirebaseAuthResult(success, firebaseUser);
        }

        /// <summary>
        /// 开始各种组件初始化
        /// </summary>
        private void OnFirebaseDepsCheckResult(bool success)
        {
            LogI($"#3 --- On FirebaseDeps: {success} ---");
            IsFirebaseReady = success;
            Callbacks.SDK.InvokeOnFirebaseReady(success);

            Analytics.OnFirebaseInitCompleted();

            LogI($"#3.5 --- Call InitRemoteConfig ---");
            // 开始Remote Manager初始化 
            
            var defaultGuruServiceJson = LoadDefaultGuruServiceJson();

            var dict = _initConfig.DefaultRemoteData.ToDictionary(
                entry => entry.Key,
                entry => entry.Value);
            
            if (!string.IsNullOrEmpty(defaultGuruServiceJson))
            {
                dict[ServicesConfigKey] = defaultGuruServiceJson;
            }
            
            RemoteConfigManager.Init(dict);
            RemoteConfigManager.OnFetchCompleted += OnFetchRemoteCallback;

            LogI($"#4 --- Apply remote services config ---");
            // 根据缓存的云控配置来初始化参数
            InitAllGuruServices();
        }

        #endregion
        		
        #region Adjust服务
        
        /// <summary>
        /// 启动 Adjust 服务
        /// </summary>
        private static void InitAdjustService(string firebaseId, Action<string> onDeeplinkCallback = null)
        {
            // 启动 AdjustService
            string appToken = GuruSettings.Instance.AdjustSetting?.GetAppToken() ?? "";
            string fbAppId = GuruSettings.Instance.IPMSetting.FacebookAppId;

            // if (!string.IsNullOrEmpty(IPMConfig.ADJUST_ID))
            //     Analytics.SetAdjustId(IPMConfig.ADJUST_ID); // 二次启动后，若有值则立即上报属性
            
            AdjustService.Instance.Start(appToken, fbAppId, firebaseId, DeviceId,
                OnAdjustInitComplete, onDeeplinkCallback ,OnGetGoogleAdId );
        }

        /// <summary>
        /// Adjust 初始化结束
        /// </summary>
        /// <param name="adjustDeviceId"></param>
        /// <param name="idfv"></param>
        /// <param name="idfa"></param>
        private static void OnAdjustInitComplete(string adjustDeviceId)
        {
            Debug.Log($"{Tag} --- OnAdjustInitComplete:  adjustId:{adjustDeviceId}");
            
            // 获取 ADID 
            if (string.IsNullOrEmpty(adjustDeviceId)) adjustDeviceId = "not_set";
            Analytics.SetAdjustDeviceId(adjustDeviceId);
            Analytics.OnAdjustInitComplete();
            
            // 确保跑在主线程内再进行赋值
            RunOnMainThread(() =>
            {
                IPMConfig.ADJUST_DEVICE_ID = adjustDeviceId;
            });
        }

        private static void OnGetGoogleAdId(string googleAdId)
        {
            Debug.Log($"{Tag} --- OnGetGoogleAdId: {googleAdId}");
            Analytics.SetGoogleAdId(googleAdId);
            
            // 确保跑在主线程内再进行赋值
            RunOnMainThread(() =>
            {
                IPMConfig.GOOGLE_ADID = googleAdId;
            });
        }
        

       
        #endregion
    }

}