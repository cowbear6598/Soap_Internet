﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Soap.Internet
{
    public class MysqlManager : MonoBehaviour
    {
        public static readonly MysqlManager Instance;

        static MysqlManager()
        {
            if (Instance == null)
            {
                GameObject g = new GameObject("MysqlManager");
                
                Instance = g.AddComponent<MysqlManager>();

                DontDestroyOnLoad(g);
            }
        }
        
        public Action OnConnectFail;

        //Url Settings
        private static List<string> domainList = new List<string>();

        private static int timeout = 30;
        
        [RuntimeInitializeOnLoadMethod]
        static void InitializeSetting()
        {
            MysqlManagerScriptableObject mysqlManagerSO = Resources.Load<MysqlManagerScriptableObject>("MysqlManagerSetting");

            if (mysqlManagerSO != null)
            {
                domainList = mysqlManagerSO.domainList;
                timeout = mysqlManagerSO.timeout;
            }
            else
            {
                Debug.Log("You didn't initialize mysql manager setting!!");
            }
        }

        #region GET

        public void RunGet(Action<string> _action, int _domainIndex, string _api,params string[] _key)
        {
            StartCoroutine(RequestAPIByGet(_action, _domainIndex, _api, _key));
        }

        public void RunGetWithToken(Action<string> _action, int _domainIndex, string _api, string _token, params string[] _key)
        {
            StartCoroutine(RequestAPIByGetWithToken(_action, _domainIndex, _api, _token, _key));
        }

        private IEnumerator RequestAPIByGet(Action<string> _action, int _domainIndex, string _api, params string[] _key)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(GetAPIUrl(_domainIndex, _api, _key)))
            {
                req.timeout = timeout;
                
                yield return req.SendWebRequest();
        
                CallbackMessage(_action, req);
            }
        }
        
        private IEnumerator RequestAPIByGetWithToken(Action<string> _action, int _domainIndex, string _api, string _token, params string[] _key)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(GetAPIUrl(_domainIndex, _api, _key)))
            {
                req.SetRequestHeader("Authorization", _token);
        
                req.timeout = timeout;
                
                yield return req.SendWebRequest();
        
                CallbackMessage(_action, req);
            }
        }

        #endregion

        #region POST

        public void RunPost(Action<string> _action, int _domainIndex, string _api, object _data,params string[] _key)
        {
            StartCoroutine(RequestAPIByPost(_action, _domainIndex, _api, _data, _key));
        }

        public void RunPostWithToken(Action<string> _action, int _domainIndex, string _api, string _token, object _data,params string[] _key)
        {
            StartCoroutine(RequestAPIByPostWithToken(_action, _domainIndex, _api, _token, _data, _key));
        }

        public void RunPostWithMultiPart(Action<string> _action, int _domainIndex, string _api, List<IMultipartFormSection> _data)
        {
            StartCoroutine(MultiFormRequestAPIByPost(_action, _domainIndex, _api, _data));
        }

        private IEnumerator RequestAPIByPost(Action<string> _action, int _domainIndex, string _api, object _data, params string[] _key)
        {
            using (UnityWebRequest req = new UnityWebRequest(GetAPIUrl(_domainIndex, _api, _key), UnityWebRequest.kHttpVerbPOST))
            {
                req.uploadHandler = CreateJsonUploadHandler(_data);
        
                req.downloadHandler = new DownloadHandlerBuffer();
        
                req.SetRequestHeader("Content-Type", "application/json");
        
                req.timeout = timeout;
                
                yield return req.SendWebRequest();
        
                CallbackMessage(_action, req);
            }
        }
        
        private IEnumerator RequestAPIByPostWithToken(Action<string> _action, int _domainIndex, string _api, string _token, object _data, params string[] _key)
        {
            using (UnityWebRequest req = new UnityWebRequest(GetAPIUrl(_domainIndex, _api, _key), UnityWebRequest.kHttpVerbPOST))
            {
                req.uploadHandler = CreateJsonUploadHandler(_data);
        
                req.downloadHandler = new DownloadHandlerBuffer();
        
                req.SetRequestHeader("Authorization", _token);
                req.SetRequestHeader("Content-Type", "application/json");
        
                req.timeout = timeout;
                
                yield return req.SendWebRequest();
        
                CallbackMessage(_action, req);
            }
        }
        
        private IEnumerator MultiFormRequestAPIByPost(Action<string> _action, int _domainIndex, string _api, List<IMultipartFormSection> _data)
        {
            using (UnityWebRequest req = UnityWebRequest.Post(GetAPIUrl(_domainIndex, _api), _data))
            {
                byte[] boundary = UnityWebRequest.GenerateBoundary();
                byte[] formSections = UnityWebRequest.SerializeFormSections(_data, boundary);

                byte[] terminate = Encoding.UTF8.GetBytes(String.Concat("\r\n--", Encoding.UTF8.GetString(boundary)));
                
                byte[] body = new byte[formSections.Length + terminate.Length];

                Buffer.BlockCopy(formSections, 0, body, 0, formSections.Length);
                Buffer.BlockCopy(terminate, 0, body, formSections.Length, terminate.Length);
                
                string contentType = String.Concat("multipart/form-data; boundary=", Encoding.UTF8.GetString(boundary));
                
                req.uploadHandler = new UploadHandlerRaw(body);
                req.uploadHandler.contentType = contentType;
                
                req.timeout = timeout;
                
                yield return req.SendWebRequest();
        
                CallbackMessage(_action, req);
            }
        }

        #endregion

        public void StopRequest()
        {
            StopAllCoroutines();
        }

        private string GetAPIUrl( int _domainIndex, string _api, params string[] _key)
        {
            string _finalUrl = domainList[_domainIndex] + _api + "?";

            for (int i = 0; i < _key.Length; i++)
            {
                _finalUrl += (i == 0) ? _key[i] : "&" + _key[i];
            }

            return _finalUrl;
        }

        private void CallbackMessage(Action<string> _action, UnityWebRequest _req)
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

        private UploadHandler CreateJsonUploadHandler(object _data, params string[] _key)
        {
            if ((_data == null && _key.Length == 0) || (_data != null && _key.Length != 0)) return null;

            byte[] _jsonRaw = null;

            if (_data != null)
            {
                _jsonRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(_data));
            }
            else if (_key.Length > 0)
            {
                string _combineData = "";

                for (int i = 0; i < _key.Length; i++)
                {
                    _combineData += (i == 0) ? _key[i] : "&" + _key[i];
                }

                _jsonRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(_combineData));
            }

            UploadHandler _uploadHandler = new UploadHandlerRaw(_jsonRaw);
            _uploadHandler.contentType = "application/json";

            return _uploadHandler;
        }

        public string GetDomainName(int _index)
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
