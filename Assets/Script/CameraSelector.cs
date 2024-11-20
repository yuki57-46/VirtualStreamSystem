using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraSelector : MonoBehaviour
{

    public RawImage cameraPreview; // カメラの映像を表示するためのRawImage
    public TMP_Dropdown cameraDropdown; // ユーザがカメラを選択するためのDropdown
    
    private WebCamTexture currentWebCam;
    private List<WebCamDevice> availableCameras;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 画面の向きを固定(端末の向きに応じて回転させない設定)
        Screen.orientation = ScreenOrientation.Portrait;

        // 利用可能なカメラを取得
        availableCameras = new List<WebCamDevice>(WebCamTexture.devices);
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

        // 初期カメラを開始
        StartCamera(availableCameras[0].name);

        // Dropdownの変更イベントを設定
        cameraDropdown.onValueChanged.AddListener(OnCameraSelectionChanged);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCameraSelectionChanged(int index)
    {
        if (currentWebCam != null)
        {
            currentWebCam.Stop();
        }

        StartCamera(availableCameras[index].name);
    }

    /// <summary>
    /// 指定したカメラ名でカメラを開始
    /// </summary>
    /// <param name="cameraName"></param>
    private void StartCamera(string cameraName = null)
    {
        currentWebCam = new WebCamTexture(cameraName);
        cameraPreview.texture = currentWebCam; // カメラの映像を表示
        currentWebCam.Play(); // カメラの再生
    }

    private void OnDestroy()
    {
        if (currentWebCam != null)
        {
            currentWebCam.Stop();
        }
    }
}
