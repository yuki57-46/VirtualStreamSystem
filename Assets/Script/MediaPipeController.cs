using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Mediapipe;
using Mediapipe.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRM;
using Mediapipe.Unity.Sample;
using System.Resources;

public class MediaPipeController : MonoBehaviour
{
    [SerializeField] private TextMeshPro gpuSetting;
    [SerializeField] private TextMeshPro cpuSetting;
    [SerializeField] private RawImage screen;
    [SerializeField] private int width = 640;
    [SerializeField] private int height = 480;
    [SerializeField] private int fps = 60;

    // VRMモデルの表情を変更するためのコンポーネント
    [SerializeField] private VRMBlendShapeProxy vrmBlendShapeProxy;
    // 顔のランドマークを描画するためのコンポーネント
    [SerializeField] private FaceLandmarkListAnnotationController faceLandmarkListAnnotationController;

    private WebCamTexture webCamTexture;
    
    private Texture2D inputTexture;
    private Color32[] inputPixelData;

    private CalculatorGraph graph;

    ResourceManager resourceManager;


    private async void Start()
    {
        // Androidか判定
        bool isAndroid = Application.platform == RuntimePlatform.Android && !Application.isEditor;

        // Androidの場合はカメラのデバイスを切り替える
        var index = !isAndroid ? 0 : 1;
        var webCamDevices = WebCamTexture.devices[index];

        // widthとheightは設定値よりカメラの上限のwidthとheightが低いと書き換えられる
        // ここで書き換えられた場合、InputTextureの設定が変わるので注意
        webCamTexture = new WebCamTexture(webCamDevices.name, width, height, fps);

        yield return new WaitUntil(() => webCamTexture.width > 16);

        // 初期化が早いとMediaPipeの初期化が間に合わないので少し待つ
        //await WaitForSecondsRealtime(0.5f);
        await WaitForSeconds(0.5f);

        // Androidの場合はカメラの向きを変更する 縦向きなため
        if (isAndroid)
        {
            screen.rectTransform.sizeDelta = new Vector2(width / 3, height / 3);
            Vector3 angle = screen.rectTransform.eulerAngles;
            angle.z = 90;
            screen.rectTransform.eulerAngles = angle;
        }

        // カメラの映像をテクスチャに設定
        inputTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        inputPixelData = new Color32[width * height];
        screen.texture = inputTexture;

        // モデルの読み込み
        resourceManager = new StreamingAssetsResourceManager();
        



        async Task WaitForSeconds(float seconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));
        }

        async Task WaitUntilWebCamTexIsReady()
        {
            while (webCamTexture.width <= 16)
            {
                await Task.Yield();
            }
        }



    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
