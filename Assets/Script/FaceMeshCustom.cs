using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Unity.CoordinateSystem;
using UnityEngine.Android;

using Stopwatch = System.Diagnostics.Stopwatch;
using System.Resources;
using System;
using TMPro;

namespace Mediapipe.Unity
{
    public class FaceMeshCustom : MonoBehaviour
    {
        [SerializeField] private TextAsset _configAsset;
        [SerializeField] private RawImage _screen;
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private int _fps;
        [SerializeField] private MultiFaceLandmarkListAnnotationController _multiFaceLandmarksAnnotationController;

        [SerializeField] private VRMFaceContoller _vrmFaceContoller;

        [SerializeField] private TMP_Dropdown _dropdown; // カメラ選択用のドロップダウン

        [SerializeField] private SettingManager settingManager; // 設定ファイルの内容

        private CalculatorGraph _graph;
        private OutputStream<List<NormalizedLandmarkList>> _multiFaceLandmarksStream;
        private IResourceManager _resourceManager;

        private WebCamTexture _webCamTexture;
        private Texture2D _inputTexture;
        private Color32[] _inputPixelData;

        private List<string> _cameraNames = new List<string>();

        private IEnumerator Start()
        {
            CheckCameraPermission();
            InitCamDropdown();

            if (WebCamTexture.devices.Length == 0)
            {
                throw new System.Exception("Web Camera devices are not found");
            }
            //bool AOS = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
            
            //var webCamDevice = WebCamTexture.devices[0];


            //_webCamTexture = new WebCamTexture(webCamDevice.name, _width, _height, _fps);
            //_webCamTexture.Play();

            //// WebCamTextureの初期化が終わるまで待機
            yield return new WaitUntil(() => _webCamTexture.width > 16 && _webCamTexture.height > 16);

            //// 実際の解像度に基づいて処理を行う
            //_width = _webCamTexture.width;
            //_height = _webCamTexture.height;

            //_screen.rectTransform.sizeDelta = new Vector2(_width, _height);

            //_inputTexture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
            //_inputPixelData = new Color32[_width * _height];

            //_screen.texture = _webCamTexture;

            //_resourceManager = new StreamingAssetsResourceManager();
            //yield return _resourceManager.PrepareAssetAsync("face_detection_short_range.bytes");
            //yield return _resourceManager.PrepareAssetAsync("face_landmark_with_attention.bytes");

            //var stopwatch = new Stopwatch();

            //_graph = new CalculatorGraph(_configAsset.text);
            //_multiFaceLandmarksStream = new OutputStream<List<NormalizedLandmarkList>>(_graph, "multi_face_landmarks");
            //_multiFaceLandmarksStream.StartPolling();
            //_graph.StartRun();
            //stopwatch.Start();

            var screenRect = _screen.GetComponent<RectTransform>().rect;

            //while (true)
            //{
            //    _inputTexture.SetPixels32(_webCamTexture.GetPixels32(_inputPixelData));
            //    _inputTexture.Apply();

            //    var imageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, _width, _height, _width * 4, _inputTexture.GetRawTextureData<byte>());
            //    var currentTimestamp = stopwatch.ElapsedTicks / (System.TimeSpan.TicksPerMillisecond / 1000);
            //    _graph.AddPacketToInputStream("input_video", Packet.CreateImageFrameAt(imageFrame, currentTimestamp));

            //    var task = _multiFaceLandmarksStream.WaitNextAsync();

            //    yield return new WaitUntil(() => task.IsCompleted);
            //    var result = task.Result;

            //    if (!result.ok)
            //    {
            //        throw new Exception("Something went wrong");
            //    }

            //    if (result.ok && result.packet != null)
            //    {
            //        var multiFaceLandmarks = result.packet.Get(NormalizedLandmarkList.Parser);

            //        if (multiFaceLandmarks.Count > 0)
            //        {
            //            var faceLandmarks = multiFaceLandmarks[0];
            //            Debug.Log($"Landmark Count: {faceLandmarks.Landmark.Count}");

            //            foreach (var landmark in faceLandmarks.Landmark)
            //            {
            //                Debug.Log($"Landmark: {landmark.X}, {landmark.Y}, {landmark.Z}");
            //            }
            //        }
            //        else
            //        {
            //            Debug.LogWarning("No face landmark is detected");
            //        }

            //        if (multiFaceLandmarks.Count > 0)
            //        {
            //            _vrmFaceContoller.UpdateVRMFace(multiFaceLandmarks[0]);
            //        }

            //    }

            //    var multiFaceLandmarksPacket = result.packet;
            //    if (multiFaceLandmarksPacket != null)
            //    {
            //        var multiFaceLandmarks = multiFaceLandmarksPacket.Get(NormalizedLandmarkList.Parser);
            //        _multiFaceLandmarksAnnotationController.DrawNow(multiFaceLandmarks);
            //    }
            //    else
            //    {
            //        _multiFaceLandmarksAnnotationController.DrawNow(null);
            //    }
            //}
        }

        private void OnDestroy()
        {
            if (_webCamTexture != null)
            {
                settingManager.SetCameraSettings(_dropdown.value, _cameraNames[_dropdown.value]);
                _webCamTexture.Stop();
            }

            _multiFaceLandmarksStream?.Dispose();
            _multiFaceLandmarksStream = null;

            if (_graph != null)
            {
                try
                {
                    _graph.CloseInputStream("input_video");
                    _graph.WaitUntilDone();
                }
                finally
                {
                    _graph.Dispose();
                    _graph = null;
                }
            }
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

        private void InitCamDropdown()
        {
            _dropdown.ClearOptions(); // ドロップダウンの選択肢をクリア
            _cameraNames.Clear(); // カメラ名のリストをクリア

            // カメラデバイスの名前を取得
            var devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Debug.Log("カメラデバイスが見つかりません");
                return;
            }

            // カメラリストを取得
            foreach (var device in devices)
            {
                _cameraNames.Add(device.name);
            }

            _dropdown.AddOptions(_cameraNames); // ドロップダウンにカメラ名を追加
            _dropdown.onValueChanged.AddListener(CamaraChanged); // ドロップダウンの選択肢が変更されたときの処理を登録

            // 最初のカメラを選択 / 設定ファイルに保存されたカメラを選択
            if (settingManager != null)
            {
                _dropdown.value = settingManager.settings.cameraIndex;
                _dropdown.RefreshShownValue();
            }
            else
            {
                _dropdown.value = 0;
            }

            CamaraChanged(_dropdown.value);

        }

        private void CamaraChanged(int index)
        {
            if (_webCamTexture != null)
            {
                _webCamTexture.Stop();
                _webCamTexture = null;
            }

            // MediaPipeのグラフをリセット
            ResetGraph();


            // 選択されたカメラデバイスを取得
            string cameraName = _cameraNames[index];
            _webCamTexture = new WebCamTexture(cameraName, _width, _height, _fps);
            _webCamTexture.Play();
            _screen.texture = _webCamTexture;

            // トラッキングを再開
            StartCoroutine(RunFaceTracking());
        }

        private void ResetGraph()
        {
            if (_graph != null)
            {
                try
                {
                    _graph.CloseInputStream("input_video");
                    _graph.WaitUntilDone();
                }
                finally
                {
                    _graph.Dispose();
                    _graph = null;
                }
            }
        }

        private IEnumerator RunFaceTracking()
        {
            yield return new WaitUntil(() => _webCamTexture.width > 16 && _webCamTexture.height > 16);

            _width = _webCamTexture.width;
            _height = _webCamTexture.height;
            _inputTexture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
            _inputPixelData = new Color32[_width * _height];

            var stopwatch = new Stopwatch();

            // MediaPipeのリソースマネージャーを再初期化
            _resourceManager = new StreamingAssetsResourceManager();
            yield return _resourceManager.PrepareAssetAsync("face_detection_short_range.bytes");
            Debug.Log("face_detection_short_range.bytes loaded");
            yield return _resourceManager.PrepareAssetAsync("face_landmark_with_attention.bytes");
            Debug.Log("face_landmark_with_attention.bytes loaded");


            // MediaPipeのグラフを作成
            _graph = new CalculatorGraph(_configAsset.text);
            _multiFaceLandmarksStream = new OutputStream<List<NormalizedLandmarkList>>(_graph, "multi_face_landmarks");
            _multiFaceLandmarksStream.StartPolling();
            _graph.StartRun();
            stopwatch.Start();

            while (true)
            {
                _inputTexture.SetPixels32(_webCamTexture.GetPixels32(_inputPixelData));
                _inputTexture.Apply();

                var imageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, _width, _height, _width * 4, _inputTexture.GetRawTextureData<byte>());
                var currentTimestamp = stopwatch.ElapsedTicks / (System.TimeSpan.TicksPerMillisecond / 1000);
                _graph.AddPacketToInputStream("input_video", Packet.CreateImageFrameAt(imageFrame, currentTimestamp));

                var task = _multiFaceLandmarksStream.WaitNextAsync();
                yield return new WaitUntil(() => task.IsCompleted);
                
                var result = task.Result;
                if (!result.ok || result.packet == null)
                {
                    Debug.LogWarning("No face landmark is detected");
                    continue;
                }

                var multiFaceLandmarks = result.packet.Get(NormalizedLandmarkList.Parser);
                if (multiFaceLandmarks.Count > 0)
                {
                    _vrmFaceContoller.UpdateVRMFace(multiFaceLandmarks[0]);
                }

                _multiFaceLandmarksAnnotationController.DrawNow(multiFaceLandmarks.Count > 0 ? multiFaceLandmarks : null);
            }
        }
    }
}