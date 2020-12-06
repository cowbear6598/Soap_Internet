using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Soap.Internet
{
    public class DownloadManager : MonoBehaviour
    {
        public static readonly DownloadManager Instance;

        static DownloadManager()
        {
            if (Instance == null)
            {
                GameObject g = new GameObject("DownloadManager");

                Instance = g.AddComponent<DownloadManager>();

                DontDestroyOnLoad(g);
            }
        }
        
        public void DownloadTexture(Action<Texture> _callback,string _url)
        {
            StartCoroutine(StartDownloadTexture(_callback, _url));
        }

        public void StopDownload()
        {
            StopAllCoroutines();
        }
    
        private IEnumerator StartDownloadTexture(Action<Texture> _callback,string _url)
        {
            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(_url))
            {
                yield return req.SendWebRequest();
        
                if (req.isNetworkError || req.isHttpError)
                {
                    Debug.Log("Cannot get texture");
                    
                    _callback?.Invoke(null);
                }
                else
                {
                    _callback?.Invoke(DownloadHandlerTexture.GetContent(req));
                }
            }
        }
    }
}

