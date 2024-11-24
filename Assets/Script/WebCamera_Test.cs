using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WebCamera_Test : MonoBehaviour
{
    public RawImage rawImage;
    WebCamTexture webCam;
    public AndroidCameraHelper androidCameraHelper; // カメラの回転角度を取得するためのAndroidCameraHelper
    public TMP_Dropdown cameraDropdown; // ユーザがカメラを選択するためのDropdown
    public List<WebCamDevice> availableCameras;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // カメラデバイスの取得
        WebCamDevice[] devices = WebCamTexture.devices;


        // 使用可能なカメラデバイスの名前を表示
        availableCameras = new List<WebCamDevice>(devices);
        if (availableCameras.Count == 0)
        {
            Debug.Log("カメラが見つかりません");
            return;
        }

        // Dropdownにカメラの名前を追加
        cameraDropdown.ClearOptions();
        List<string> cameraNames = new List<string>();
        foreach (var device in availableCameras)
        {
            cameraNames.Add(device.name);
        }
        cameraDropdown.AddOptions(cameraNames);

        StartCamera(availableCameras[0].name);

        // Dropdownの変更イベントを設定
        cameraDropdown.onValueChanged.AddListener(OnCameraSelectionChanged);

        

    }


    void AdjustRotation(int angle)
    {
        // RawImageの回転角度を設定
        rawImage.rectTransform.rotation = Quaternion.Euler(0, 0, -angle);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCameraSelectionChanged(int index)
    {
        if (webCam != null)
        {
            webCam.Stop();
        }
        StartCamera(availableCameras[index].name);
    }

    private void StartCamera(string cameraName = null)
    {
        webCam = new WebCamTexture(cameraName/*, webCam.width, webCam.height*/);

        // カメラのサイズを設定
        // availableCmaerasから選択されたカメラのサイズを取得
        webCam.requestedWidth = webCam.width;
        webCam.requestedHeight = webCam.height;

        rawImage.texture = webCam;
        
        AdjustRotation(androidCameraHelper.RotationAngle);

        webCam.Play(); 

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
