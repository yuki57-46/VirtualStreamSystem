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
using UnityEngine.LowLevel;

public class MediaPipeController : MonoBehaviour
{
    [SerializeField] private TextAsset gpuSetting;
    [SerializeField] private TextAsset cpuSetting;
    [SerializeField] private RawImage screen;
    [SerializeField] private int width = 640;
    [SerializeField] private int height = 480;
    [SerializeField] private int fps = 60;

    // VRMモデルの表情を変更するためのコンポーネント
    [SerializeField] private VRMBlendShapeProxy vrmBlendShapeProxy;
    // 顔のランドマークを描画するためのコンポーネント
    [SerializeField] private FaceLandmarkListAnnotationController faceLandmarkListAnnotationController;

    private WebCamTexture webCamTexture;
    private OutputStream<List<NormalizedLandmarkList>> multiFaceLandmarksStream;

    private Texture2D inputTexture;
    private Color32[] inputPixelData;

    private CalculatorGraph graph;

    


    IResourceManager resourceManager;

    // 設定ファイルのパス
    private string configAsset;


    private async void Start()
    {

        configAsset = @"
input_stream: ""input_video""
output_stream: ""out""
node {
  calculator: ""PassThroughCalculator""
  input_stream: ""input_video""
  output_stream: ""out1""
}
node {
  calculator: ""PassThroughCalculator""
  input_stream: ""out1""
  output_stream: ""out""
}
";

        if (WebCamTexture.devices.Length == 0)
        {
            throw new System.Exception("カメラが見つかりません");
        }
        var webCamDevice = WebCamTexture.devices[0];

        webCamTexture = new WebCamTexture(webCamDevice.name, width, height, fps);
        webCamTexture.Play();
        await Task.Run(() => { while (webCamTexture.width <= 16) ; });
        screen.rectTransform.sizeDelta = new Vector2(width, height);
        inputTexture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);
        inputPixelData = new Color32[inputTexture.width * inputTexture.height];
        var outputTexture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);
        var outputPixelData = new Color32[outputTexture.width * outputTexture.height];
        screen.texture = outputTexture;

        // MediaPipe
        resourceManager = new StreamingAssetsResourceManager();
        await PrepareAssetAsyncWrapper("pose_landmark.bytes");
        await PrepareAssetAsyncWrapper("pose_detection.bytes");
        var stopwatch = new System.Diagnostics.Stopwatch();

        // 設定を読み込む
        var textConfig = Application.isEditor ? cpuSetting : gpuSetting;
        var baseConfig = textConfig == null ? null : CalculatorGraphConfig.Parser.ParseFromTextFormat(textConfig.text);

        // サイドパケットの設定
        var sidePacket = new PacketMap();
        sidePacket.Emplace("input_rotation", Packet.CreateInt(0));
        sidePacket.Emplace("input_horizontally_flipped", Packet.CreateBool(false));
        sidePacket.Emplace("input_vertically_flipped", Packet.CreateBool(true));

        // MediaPipeの初期化
        graph = new CalculatorGraph(configAsset);
        graph.Initialize(baseConfig, sidePacket);

        // 出力の設定 OutputStreamFaceはOutputStream<NormalizedLandmarkList>を非Generics化したクラス
        // Genericsクラスのメソッドに対して[AOT.MonoPInvokeCallback]を付与してもIL2CPPでは動作しないため、別で定義する
        multiFaceLandmarksStream = new OutputStream<List<NormalizedLandmarkList>>(graph, "multi_face_landmarks");

        // メインスレッドでの処理
        multiFaceLandmarksStream.StartPolling();
        graph.StartRun();
        stopwatch.Start();

        while (true)
        {
            if (webCamTexture == null || inputTexture == null || inputPixelData == null)
            {
                break;
            }

            inputTexture.SetPixels32(webCamTexture.GetPixels32(inputPixelData));
            var imageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, width, height, width * 4, inputTexture.GetRawTextureData<byte>());
            var currentTimestamp = stopwatch.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000);
            graph.AddPacketToInputStream("input_video", Packet.CreateImageFrameAt(imageFrame, currentTimestamp));

            var result = await multiFaceLandmarksStream.WaitNextAsync();

            if (!result.ok) throw new Exception("Something went wrong");

            var outputPacket = result.packet;
            if (outputPacket != null)
            {
                var landmarks = outputPacket.Get(NormalizedLandmarkList.Parser);
                faceLandmarkListAnnotationController.DrawNow((IReadOnlyList<NormalizedLandmark>)landmarks);
            }
            else
            {
                faceLandmarkListAnnotationController.DrawNow((IReadOnlyList<NormalizedLandmark>)null);
            }
        }
    }

    private async Task PrepareAssetAsyncWrapper(string assetName)
    {
        var enumerator = resourceManager.PrepareAssetAsync(assetName);
        while (enumerator.MoveNext())
        {
            await Task.Yield();
        }
        if (enumerator.Current != null)
        {
            Debug.LogError(enumerator.Current);
            throw (Exception)enumerator.Current;

        }

    }


    void Update()
    {
        
    }

    private void OnDestroy()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
        }

        multiFaceLandmarksStream?.Dispose();
        multiFaceLandmarksStream = null;

        if (graph != null)
        {
            try
            {
                graph.CloseInputStream("input_video");
                graph.WaitUntilDone();
            }
            finally
            {
                graph.Dispose();
                graph = null;
            }
        }
    }
}
