using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebCamera_Test : MonoBehaviour
{
    public RawImage rawImage;
    WebCamTexture webCam;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // カメラデバイスの取得
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length > 0)
        {
            // 最初のカメラデバイスを使用
            Debug.Log(devices[0].name);
            webCam = new WebCamTexture(devices[0].name);
            rawImage.texture = webCam;
            rawImage.material.mainTexture = webCam;

            // カメラデバイスの再生
            webCam.Play();
        }
        else
        {
            Debug.Log("カメラデバイスが見つかりません");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        // カメラデバイスの停止
        if (webCam != null)
        {
            webCam.Stop();
        }
    
    }
}
