using System;
using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Networking;

namespace Soap.Internet
{
    public static class DownloadManager
    {
        public static void DownloadTexture(Action<Texture> _callback,string _url)
        {
            Timing.RunCoroutine(StartDownloadTexture(_callback, _url));
        }
    
        private static IEnumerator<float> StartDownloadTexture(Action<Texture> _callback,string _url)
        {
            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(_url))
            {
                yield return Timing.WaitUntilDone(req.SendWebRequest());
        
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

