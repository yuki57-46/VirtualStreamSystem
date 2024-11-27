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

    private float currentCWNeeded = 0f;
    private WebCamDevice targetDevice;

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
        StartCoroutine(_startCamera(cameraName));

        Rect uvRectForVideoVerticallyMirrored = new(1f, 0f, -1f, 1f);
        Rect uvRectForVideoNotVerticallyMirrored = new(0f, 0f, 1f, 1f);
        Vector3 currentLocalEulerAngle = Vector3.zero;

        if (webCam && webCam.width >= 100f)
        {
            currentCWNeeded = targetDevice.isFrontFacing ? webCam.videoRotationAngle : -webCam.videoRotationAngle;

            if (webCam.videoVerticallyMirrored)
            {
                currentCWNeeded += 180f;
            }

            currentLocalEulerAngle.z = currentCWNeeded;
            rawImage.rectTransform.localEulerAngles = currentLocalEulerAngle;

            if ((webCam.videoVerticallyMirrored && !targetDevice.isFrontFacing) ||
                (!webCam.videoVerticallyMirrored && targetDevice.isFrontFacing))
            {
                rawImage.uvRect = uvRectForVideoVerticallyMirrored;
            }
            else
            {
                rawImage.uvRect = uvRectForVideoNotVerticallyMirrored;
            }
        }

    }

    IEnumerator _startCamera(string cameraName )
    {
        // 指定カメラを起動させる
        webCam = new WebCamTexture(cameraName, Screen.width, Screen.height);

        // RawImageのテクスチャにWebCamTextureのインスタンスを設定
        rawImage.texture = webCam;
        // カメラ起動
        webCam.Play();

        Debug.Log("WebCamTextureのwidth: " + webCam.width);
        Debug.Log("WebCamTextureのrequestedWidth: " + webCam.requestedWidth);

        webCam.width = webCam.requestedWidth;

        while (webCam.width != webCam.requestedWidth)
        {
            // widthが指定したものになっていない場合は処理を抜けて次のフレームで再開
            Debug.Log("widthが指定したものになっていない");
            yield return null;
        }
        Rect uvRectForVideoVerticallyMirrored = new(1f, 0f, -1f, 1f);
        Rect uvRectForVideoNotVerticallyMirrored = new(0f, 0f, 1f, 1f);
        Vector3 currentLocalEulerAngle = Vector3.zero;

        if (webCam && webCam.width >= 100f)
        {
            currentCWNeeded = targetDevice.isFrontFacing ? webCam.videoRotationAngle : -webCam.videoRotationAngle;

            if (webCam.videoVerticallyMirrored)
            {
                currentCWNeeded += 180f;
            }

            currentLocalEulerAngle.z = currentCWNeeded;
            rawImage.rectTransform.localEulerAngles = currentLocalEulerAngle;

            if ((webCam.videoVerticallyMirrored && !targetDevice.isFrontFacing) ||
                (!webCam.videoVerticallyMirrored && targetDevice.isFrontFacing))
            {
                rawImage.uvRect = uvRectForVideoVerticallyMirrored;
            }
            else
            {
                rawImage.uvRect = uvRectForVideoNotVerticallyMirrored;
            }
        }

//        yield break;
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
