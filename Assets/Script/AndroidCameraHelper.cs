using UnityEngine;

public class AndroidCameraHelper : MonoBehaviour
{
    private AndroidJavaObject cameraHelper;

    // 回転角度を設定する変数
    public int RotationAngle { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // CameraHelperクラスのインスタンスを取得
        using (var pluginClass = new AndroidJavaClass("com.example.cameralibrary.CameraHelper"))
        {
            cameraHelper = pluginClass;
        }

        // カメラIDを取得
        string[] camraIds = cameraHelper.Call<string[]>("getAvailableCameraIds");
        Debug.Log("Available Camera:" + string.Join(", ", camraIds));

        // カメラの回転角度を取得
        if (camraIds.Length > 0)
        {
            RotationAngle = cameraHelper.CallStatic<int>("getCameraDisplayOrientation", GetAndroidContext());
            Debug.Log("Camera Rotation Angle:" + RotationAngle);
        }
    }

    // AndroidのContextを取得
    private AndroidJavaObject GetAndroidContext()
    {
        using(var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }
}
