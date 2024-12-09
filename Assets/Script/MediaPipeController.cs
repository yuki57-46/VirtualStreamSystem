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

    IResourceManager resourceManager;

    // 設定ファイルのパス
    private string configAsset;


    private IEnumerator Start()
    {
        if (WebCamTexture.devices.Length == 0)
        {
        //    Debug.LogError("カメラが見つかりません");
        //    yield break;

            throw new System.Exception("カメラが見つかりません");
        }
        var webCamDevice = WebCamTexture.devices[0];

        webCamTexture = new WebCamTexture(webCamDevice.name, width, height, fps);
        webCamTexture.Play();
        yield return new WaitUntil(() => webCamTexture.width > 16);
        screen.rectTransform.sizeDelta = new Vector2(width, height);
        inputTexture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);
        inputPixelData = new Color32[inputTexture.width * inputTexture.height];
        var outputTexture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);
        var outputPixelData = new Color32[outputTexture.width * outputTexture.height];
        screen.texture = outputTexture;

        // MediaPipe
        resourceManager = new StreamingAssetsResourceManager();
        yield return resourceManager.PrepareAssetAsync("pose_landmark.bytes");
        yield return resourceManager.PrepareAssetAsync("pose_detection.bytes");
        var stopwatch = new System.Diagnostics.Stopwatch();
        graph = new CalculatorGraph(configAsset);
        
    }


    void Update()
    {
        
    }
}
