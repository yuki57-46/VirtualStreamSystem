using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Android;
//using Unity.Android.Gradle.Manifest;


public class RuntimeAndroidSettingHelper : MonoBehaviour
{
    private RuntimeAndroidSettingHelper() { }

    private void Start()
    {
        // カメラ権限の確認
        CheckCameraPermission();
        // Androidの外部ストレージへのアクセス権限を取得
        //if (!HasUserAuthorizedPermission())
        //{
        //    // Androidの外部ストレージへのアクセス権限が許可されていない場合、設定画面を開く
        //    Request_SettingIntent();
        //}
    }

    private static AndroidJavaObject GetActivity()
    {
        using (var UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            return UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }

    public static void Request_SettingIntent()
    {
        using (var activity = GetActivity())
        {
            using (var intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.MANAGE_ALL_FILES_ACCESS_PERMISSION"))
            {
                activity.Call("startActivity", intentObject);
            }
        }
    }

    // https://developer.android.com/reference/android/os/Environment#isExternalStorageManager()
    public static bool HasUserAuthorizedPermission()
    {
        bool isExternalStorageManager = false;
        try
        {
            // Androidの外部ストレージへのアクセス権限を取得
            AndroidJavaClass environmentClass = new AndroidJavaClass("android.os.Environment");
            isExternalStorageManager = environmentClass.CallStatic<bool>("isExternalStorageManager");
        }
        catch (Exception e)
        {
            return false;
        }
        return isExternalStorageManager;
    }

    void CheckCameraPermission()
    {
        // カメラ権限が許可されているか確認
        if (Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            // カメラ権限が許可されている場合の処理
            return;
        }
        else
        {
            // カメラ権限が許可されていない場合の処理
            Permission.RequestUserPermission(Permission.Camera);
        }
    }

}
