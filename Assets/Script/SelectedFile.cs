using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Android;

/// <summary>
/// ファイルを選択し、そのファイルパスを取得する
/// </summary>
public class SelectedFile : MonoBehaviour
{
    // ボタンのインスタンス
    public GameObject button;

    // ファイルパス
    public string filePath;

    private Button btn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // ボタンが押されたら
        if (btn.onClick != null)
        {
            // ファイルを選択
            FileBrowser();


            //filePath = Application.dataPath;
            //UnityEngine.Debug.Log(filePath);
        }
    }

    public static void FileBrowser()
    {
#if UNITY_ANDROID
        if (System.Environment.OSVersion.Platform == System.PlatformID.Unix)
        {
            // Androidの場合
            Process.Start("am start -a android.intent.action.GET_CONTENT -t */* -c android.intent.category.OPENABLE");
        }


#endif
    }

}
