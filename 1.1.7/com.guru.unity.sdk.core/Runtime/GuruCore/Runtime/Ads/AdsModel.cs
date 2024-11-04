

namespace Guru
{
    using UnityEngine;
    using System;
    
    public class AdsModel
    {
        internal AdsModelStorage _storage;
        
        public int radsRewardCount = 0;
        public double tchAd001Value = 0;
        public double tchAd02Value = 0;
        public bool buyNoAds = false;
        
        
        public int RadsRewardCount
        {
            get => radsRewardCount;
            set
            {
                radsRewardCount = value;
                Save();
            }
        }

        public double TchAD001RevValue
        {
            get => tchAd001Value;
            set
            {
                tchAd001Value = value;
                Save();
            }
        }
        
        public double TchAD02RevValue
        {
            get => tchAd02Value;
            set
            {
                tchAd02Value = value;
                Save();
            }
        }
        
        public bool BuyNoAds
        {
            get => buyNoAds;
            set
            {
                buyNoAds = value;
                Save();
            }
        }
        
        
        private void Save() => _storage.Save();

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
        
        
        
        public static AdsModel Create()
        {
            AdsModel model;
            AdsModelStorage storage;
            AdsModelStorage.Create(out model, out storage);
            model._storage = storage;
            return model;
        } 
        

    }


    internal class AdsModelStorage : MonoBehaviour
    {
        private const string INSTANCE_NAME = "GuruSDK";
        private bool _needToSave = false;
        private float _lastSavedTime = 0;
        private float _saveInterval = 2;
        private AdsModel _model;

        public static void Create(out AdsModel model, out AdsModelStorage storage)
        {
            model = null;
            storage = null;
            
            var go = GameObject.Find(INSTANCE_NAME);
            if (go == null) go = new GameObject(INSTANCE_NAME);
            
            AdsModelStorage _ins = null;
            if (!go.TryGetComponent<AdsModelStorage>(out storage))
            {
                storage = go.AddComponent<AdsModelStorage>();
            }
            
            string json = PlayerPrefs.GetString(nameof(AdsModel), "");
            if (!string.IsNullOrEmpty(json))
            {
                model = JsonUtility.FromJson<AdsModel>(json);
            }
            else
            {
                model = new AdsModel();
            }
            
            model._storage = storage;
            storage._model  = model;
        }



        public void Save(bool forceSave = false)
        {
            _needToSave = forceSave || (Time.realtimeSinceStartup - _lastSavedTime > _saveInterval);
        }
        
        #region 生命周期

        
        // 主线程进行写入操作
        void Update()
        {
            if (_needToSave)
            {
                var json = _model?.ToJson() ?? "";
                if (!string.IsNullOrEmpty(json))
                {
                    PlayerPrefs.SetString(nameof(AdsModel), json);
                    _needToSave = false;
                    _lastSavedTime = Time.realtimeSinceStartup;
                }
            }
        }

        // 监听特殊事件
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Save( true);
            }
        }

        // 监听特殊事件
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Save( true);
            }
        }

        #endregion
    }


}