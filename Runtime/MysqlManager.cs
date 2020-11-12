using System;
using System.Collections.Generic;
using System.Text;
using MEC;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Soap.Internet
{
    public static class MysqlManager
    {
        public static Action OnConnectFail;

        //Url Settings
        private static List<string> domainList = new List<string>();

        [RuntimeInitializeOnLoadMethod]
        static void InitializeSetting()
        {
            MysqlManagerScriptableObject mysqlManagerSO = Resources.Load<MysqlManagerScriptableObject>("MysqlManagerSetting");

            if (mysqlManagerSO != null)
            {
                domainList = Resources.Load<MysqlManagerScriptableObject>("MysqlManagerSetting").domainList;
            }
        }

        #region GET

        public static void RunRequestAPIByGet(Action<string> _action, int _domainIndex, string _api,params string[] _key)
        {
            Timing.RunCoroutine(RequestAPIByGet(_action, _domainIndex, _api, _key));
        }

        public static void RunRequestAPIByGet(Action<string> _action, int _domainIndex, string _api, string _token, params string[] _key)
        {
            Timing.RunCoroutine(RequestAPIByGet(_action, _domainIndex, _api, _token, _key));
        }

        private static IEnumerator<float> RequestAPIByGet(Action<string> _action, int _domainIndex, string _api, params string[] _key)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(GetAPIUrl(_domainIndex, _api, _key)))
            {
                yield return Timing.WaitUntilDone(req.SendWebRequest());
        
                CallbackMessage(_action, req);
            }
        }
        
        private static IEnumerator<float> RequestAPIByGet(Action<string> _action, int _domainIndex, string _api, string _token, params string[] _key)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(GetAPIUrl(_domainIndex, _api, _key)))
            {
                req.SetRequestHeader("Authorization", _token);
        
                yield return Timing.WaitUntilDone(req.SendWebRequest());
        
                CallbackMessage(_action, req);
            }
        }

        #endregion

        #region POST

        public static void RunRequestAPIByPost(Action<string> _action, int _domainIndex, string _api, object _data,params string[] _key)
        {
            Timing.RunCoroutine(RequestAPIByPost(_action, _domainIndex, _api, _data, _key));
        }

        public static void RunRequestAPIByPost(Action<string> _action, int _domainIndex, string _api, string _token, object _data,params string[] _key)
        {
            Timing.RunCoroutine(RequestAPIByPost(_action, _domainIndex, _api, _token, _data, _key));
        }

        public static void RunMultiFormRequestAPIByPost(Action<string> _action, int _domainIndex, string _api, List<IMultipartFormSection> _data)
        {
            Timing.RunCoroutine(MultiFormRequestAPIByPost(_action, _domainIndex, _api, _data));
        }

        private static IEnumerator<float> RequestAPIByPost(Action<string> _action, int _domainIndex, string _api, object _data, params string[] _key)
        {
            using (UnityWebRequest req = new UnityWebRequest(GetAPIUrl(_domainIndex, _api, _key), UnityWebRequest.kHttpVerbPOST))
            {
                req.uploadHandler = CreateJsonUploadHandler(_data);
        
                req.downloadHandler = new DownloadHandlerBuffer();
        
                req.SetRequestHeader("Content-Type", "application/json");
        
                yield return Timing.WaitUntilDone(req.SendWebRequest());
        
                CallbackMessage(_action, req);
            }
        }
        
        private static IEnumerator<float> RequestAPIByPost(Action<string> _action, int _domainIndex, string _api, string _token, object _data, params string[] _key)
        {
            using (UnityWebRequest req = new UnityWebRequest(GetAPIUrl(_domainIndex, _api, _key), UnityWebRequest.kHttpVerbPOST))
            {
                req.uploadHandler = CreateJsonUploadHandler(_data);
        
                req.downloadHandler = new DownloadHandlerBuffer();
        
                req.SetRequestHeader("Authorization", _token);
                req.SetRequestHeader("Content-Type", "application/json");
        
                yield return Timing.WaitUntilDone(req.SendWebRequest());
        
                CallbackMessage(_action, req);
            }
        }
        
        private static IEnumerator<float> MultiFormRequestAPIByPost(Action<string> _action, int _domainIndex, string _api, List<IMultipartFormSection> _data)
        {
            using (UnityWebRequest req = UnityWebRequest.Post(GetAPIUrl(_domainIndex, _api), _data))
            {
                req.SetRequestHeader("Content-Type", "multipart/form-data");
                
                yield return Timing.WaitUntilDone(req.SendWebRequest());
        
                CallbackMessage(_action, req);
            }
        }

        #endregion

        private static string GetAPIUrl( int _domainIndex, string _api, params string[] _key)
        {
            string _finalUrl = domainList[_domainIndex] + _api + "?";

            for (int i = 0; i < _key.Length; i++)
            {
                _finalUrl += (i == 0) ? _key[i] : "&" + _key[i];
            }

            return _finalUrl;
        }

        private static void CallbackMessage(Action<string> _action, UnityWebRequest _req)
        {
#if UNITY_EDITOR
            Debug.Log("Url: " +_req.uri +"\nCode: " + _req.responseCode + "\nContext: " + _req.downloadHandler.text);
#endif
            
            switch (_req.responseCode)
            {
                case 200:
                case 204:
                    _action?.Invoke(_req.downloadHandler.text);
                    break;
                default:
                    OnConnectFail?.Invoke();
                    break;
            }
        }

        private static UploadHandler CreateJsonUploadHandler(object _data, params string[] _key)
        {
            if ((_data == null && _key.Length == 0) || (_data != null && _key.Length != 0)) return null;

            byte[] _jsonRaw = null;

            if (_data != null)
            {
                _jsonRaw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_data));
            }
            else if (_key.Length > 0)
            {
                string _combineData = "";

                for (int i = 0; i < _key.Length; i++)
                {
                    _combineData += (i == 0) ? _key[i] : "&" + _key[i];
                }

                _jsonRaw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_combineData));
            }

            UploadHandler _uploadHandler = new UploadHandlerRaw(_jsonRaw);
            _uploadHandler.contentType = "application/json";

            return _uploadHandler;
        }

        public static string GetDomainName(int _index)
        {
            if (_index > domainList.Count)
            {
#if UNITY_EDITOR
                Debug.LogError("Over domain list's length");
#endif
                return "";
            }
            
            return domainList[_index];
        }
    }
}
