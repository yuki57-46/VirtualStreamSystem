using UnityEngine;
using System.IO;


[System.Serializable] // シリアライズ可能にする
public class ParameterSettings
{
    public float eyeSensitivtySlider; // 目の感度調整用
    public float mouthSensitivtySlider; // 口の感度調整用
    public int cameraIndex; // カメラのインデックス
    public string cameraName; // カメラの名前
}
public class SettingManager : MonoBehaviour
{
    private string settingPath; // 設定ファイルのパス
    public ParameterSettings settings = new ParameterSettings(); // 設定ファイルの内容

    

    void Awake()
    {
        settingPath = Path.Combine(Application.persistentDataPath, "settings.json"); // 設定ファイルのパスを設定
        LoadSettings(); // 設定ファイルを読み込む
    }

    /// <summary>
    /// 設定ファイルの保存
    /// </summary>
    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(settings, true); // ParameterSettingsをJson形式に変換 (true: インデントをつける)
        File.WriteAllText(settingPath, json); // 設定ファイルに書き込み
        Debug.Log($"Save Settings: {json}, {settingPath}");
    }

    /// <summary>
    /// 設定ファイルの読み込み
    /// </summary>
    public void LoadSettings()
    {
        if (File.Exists(settingPath)) // 設定ファイルが存在する場合
        {
            string json = File.ReadAllText(settingPath); // 設定ファイルの読み込み
            settings = JsonUtility.FromJson<ParameterSettings>(json); // Json形式からParameterSettingsに変換
        }
        else
        {
            SaveSettings(); // 設定ファイルがない場合は新規作成
        }
    }

    /// <summary>
    /// Webカメラの設定を保存
    /// </summary>
    /// <param name="index">登録するカメラインデックス</param>
    /// <param name="name">登録するカメラの名前</param>
    public void SetCameraSettings(int index, string name)
    {
        settings.cameraIndex = index;
        settings.cameraName = name;
        SaveSettings();
    }

    /// <summary>
    /// 感度調整の設定を保存
    /// </summary>
    /// <param name="eyeSensitivity">目の感度設定</param>
    /// <param name="mouthSensitivity">口の感度設定</param>
    public void SetSensitivitySettings(float eyeSensitivity, float mouthSensitivity)
    {
        settings.eyeSensitivtySlider = eyeSensitivity;
        settings.mouthSensitivtySlider = mouthSensitivity;
        SaveSettings();
    }


}
