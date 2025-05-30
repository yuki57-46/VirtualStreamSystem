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
using UniVRM10;
using VRM;
using UnityEngine.InputSystem;

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

        [SerializeField] private Button _calibrateButton;

        private CalculatorGraph _graph;
        private OutputStream<List<NormalizedLandmarkList>> _multiFaceLandmarksStream;
        private IResourceManager _resourceManager;

        private WebCamTexture _webCamTexture;
        private Texture2D _inputTexture;
        private Color32[] _inputPixelData;

        private List<string> _cameraNames = new List<string>();

        private Quaternion _initHeadRotation = Quaternion.identity;

        private Vector3 _initHeadRotation_Vec3 = Vector3.zero;

        private bool _hasSetInitHeadRotation = false;

        private NormalizedLandmarkList _latestLandmarks = null;

        private bool _isCalibrating = false;

        private long _baseTimestamp = 0; // ベースタイムスタンプ
        private long _prevTimestamp = -1; // 前回のタイムスタンプ

        public bool IsFirst { get; set; } = true; // 初回のみの処理用

        private IEnumerator Start()
        {
            CheckCameraPermission();
            InitCamDropdown();

            StartCoroutine(DelayInit());

            if (WebCamTexture.devices.Length == 0)
            {
                throw new System.Exception("Web Camera devices are not found");
            }

            yield return new WaitUntil(() => _webCamTexture.width > 16 && _webCamTexture.height > 16);

            var screenRect = _screen.GetComponent<RectTransform>().rect;


            if (_calibrateButton == null)
            {
                throw new System.Exception("Calibrate Button is not set");
            }

            _calibrateButton.onClick.AddListener(() =>
            {
                CalibrateHeadRotation(_latestLandmarks);
            });

        }

        private void OnDestroy()
        {
            if (_graph != null)
            {
                try
                {
                    //_graph.CloseInputStream("input_video");
                    _graph.CloseAllPacketSources();
                    _graph.WaitUntilDone();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Graph shutdown failed: {e.Message}");
                }
                finally
                {
                    _graph.Dispose();
                    _graph = null;
                }
            }

            // WebCam停止
            if (_webCamTexture != null)
            {
                settingManager.SetCameraSettings(_dropdown.value, _cameraNames[_dropdown.value]);
                _webCamTexture.Stop();
            }

            // ストリームやその他のリソースの破棄
            _multiFaceLandmarksStream?.Dispose();
            _multiFaceLandmarksStream = null;
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
            _dropdown.onValueChanged.AddListener(index => StartCoroutine(CamaraChanged(index))); // ドロップダウンの選択肢が変更されたときの処理を登録

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

        private IEnumerator CamaraChanged(int index)
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

            // 少し待ってから次の処理を実行
            yield return new WaitForSeconds(0.5f);



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
                    _hasSetInitHeadRotation = false;
                    _graph.Dispose();
                    _graph = null;
                }
            }
        }

        IEnumerator DelayInit()
        {
            yield return null;
            ResetGraph();
        }

        private IEnumerator RunFaceTracking()
        {

            while (_webCamTexture == null || !_webCamTexture.isPlaying)
            {
                yield return null;
            }

            yield return new WaitUntil(() => _webCamTexture.width > 16 && _webCamTexture.height > 16);

            _baseTimestamp = Stopwatch.GetTimestamp(); // 基準タイムスタンプを取得

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

            //_resourceManager = new EmptyResourceManager();

            _multiFaceLandmarksStream = new OutputStream<List<NormalizedLandmarkList>>(_graph, "multi_face_landmarks");
            _multiFaceLandmarksStream.StartPolling();

            //yield return new WaitUntil(() => _width > 100);

            _graph.StartRun();
            //stopwatch.Start();
            stopwatch = Stopwatch.StartNew();
            stopwatch.Restart();

            int frameCount = 0;

            while (true)
            {
                _inputTexture.SetPixels32(_webCamTexture.GetPixels32(_inputPixelData));
                _inputTexture.Apply();

                //var imageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, _width, _height, _width * 4, _inputTexture.GetRawTextureData<byte>());
                //var currentTimestamp = stopwatch.ElapsedTicks / (System.TimeSpan.TicksPerMillisecond / 1000);
                //long currentTimestamp = stopwatch.ElapsedMilliseconds * 1000; // マイクロ秒に変換
                //long currentTimestamp = (Stopwatch.GetTimestamp() - _baseTimestamp) * 1000000 / Stopwatch.Frequency; // マイクロ秒に変換
                //long currentTimestamp = frameCount++ * 333333; // 30fpsでのマイクロ秒に変換
                 //long currentTimestamp = stopwatch.ElapsedMilliseconds * 1000; // マイクロ秒に変換

                //if (currentTimestamp <= _prevTimestamp)
                //{
                //    currentTimestamp = _prevTimestamp + 1; // タイムスタンプを1msずらす
                //}
                //_prevTimestamp = currentTimestamp;

                ////_graph.AddPacketToInputStream("input_video", Packet.CreateImageFrameAt(imageFrame, currentTimestamp));

                //try
                //{
                //    _graph.AddPacketToInputStream("input_video", Packet.CreateImageFrameAt(imageFrame, currentTimestamp));
                //}
                //catch (Exception e)
                //{
                //    Debug.LogError($"Packet error: {e}");
                //    yield break; // または continue
                //}
                var rawPixels = _inputTexture.GetRawTextureData<byte>();
                using (var imageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, _width, _height, _width * 4, rawPixels))
                {
                    long currentTimestamp = frameCount++ * 33333; // 30fps
                    _graph.AddPacketToInputStream("input_video", Packet.CreateImageFrameAt(imageFrame, currentTimestamp));
                }



                var task = _multiFaceLandmarksStream.WaitNextAsync();
                float startTime = Time.time;
                yield return new WaitUntil(() => task.IsCompleted || Time.time - startTime > 2.0f);
                if (task.IsFaulted)
                {
                    Debug.LogError($"Task faulted: {task.Exception}");
                    yield break; // または continue
                }
                
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

                    _latestLandmarks = multiFaceLandmarks[0];

                    // 頭の向きを計算して VRMFaseController に反映
                    var headRotationBase = GetHeadRotationEuler(multiFaceLandmarks[0]);
                    _vrmFaceContoller.UpdateHeadRotation(headRotationBase);

                }

                _multiFaceLandmarksAnnotationController.DrawNow(multiFaceLandmarks.Count > 0 ? multiFaceLandmarks : null);
            }
        }


        public Vector3 GetHeadRotationEuler(NormalizedLandmarkList landmarks, bool Calibration = false)
        {
            if (landmarks == null || landmarks.Landmark.Count == 0)
            {
                return Vector3.zero;
            }


            Vector3 chin = new Vector3(landmarks.Landmark[152].X, landmarks.Landmark[152].Y, landmarks.Landmark[152].Z);
            Vector3 leftCheek = new Vector3(landmarks.Landmark[234].X, landmarks.Landmark[234].Y, landmarks.Landmark[234].Z);
            Vector3 rightCheek = new Vector3(landmarks.Landmark[454].X, landmarks.Landmark[454].Y, landmarks.Landmark[454].Z);
            Vector3 nose = new Vector3(landmarks.Landmark[1].X, landmarks.Landmark[1].Y, landmarks.Landmark[1].Z);

            Vector3 horizontal = (rightCheek - leftCheek).normalized; // 顔の横方向 (左右の頬の中心)
            float yaw = Mathf.Atan2(horizontal.x, horizontal.z) * Mathf.Rad2Deg; // 顔の横方向の角度

            Vector3 vertical = (nose - chin).normalized; // 顔の縦方向 (鼻先から顎の中心)

            float pitch = Mathf.Atan2(vertical.y, vertical.z) * Mathf.Rad2Deg; // 顔の縦方向の角度

            // 顔の傾き
            float roll = Mathf.Atan2(rightCheek.y - leftCheek.y, Vector3.Distance(rightCheek, leftCheek)) * Mathf.Rad2Deg;

            //roll *= 0.2f; // 顔の傾きを調整

            if (!_hasSetInitHeadRotation)
            {
                _initHeadRotation_Vec3 = new Vector3(pitch, yaw, roll);
                _hasSetInitHeadRotation = true;
            }

            //return _initHeadRotation_Vec3 + new Vector3(-pitch, -yaw, -roll);

            Vector3 currentRotation = new Vector3(-pitch, -yaw, -roll);

            if (IsFirst)
            {
                currentRotation = new Vector3(-pitch, -yaw, -roll);
            }


            if (!Calibration)
            {
                return _initHeadRotation_Vec3 + currentRotation;
            }
            else
            {
                return -currentRotation;
            }


        }

        public void CalibrateHeadRotation(NormalizedLandmarkList landmarks)
        {
            if (_multiFaceLandmarksStream == null)
            {
                Debug.LogError("Face landmark stream is not running");
                return;
            }

            //var task = _multiFaceLandmarksStream.WaitNextAsync();
            //new WaitUntil(() => task.IsCompleted);
            //var result = task.Result;
            //if (!result.ok || result.packet == null)
            //{
            //    Debug.LogWarning("No face landmark data available for calibration");
            //    return;
            //}

            //var landmarks = result.packet.Get(NormalizedLandmarkList.Parser);
            //var resultLandmarks = result.packet.Get(NormalizedLandmarkList.Parser);
            if (landmarks == null || landmarks.Landmark.Count == 0)
            {
                return;
            }

            //_hasSetInitHeadRotation = false;
            _isCalibrating = true;

            //if (_vrmFaceContoller.vrmLoder.VRMModel != null)
            //{
            //    var vrmModel = _vrmFaceContoller.vrmLoder.VRMModel;


            //    vrmModel.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).localEulerAngles = new Vector3(0, 0, 0);
            //}
            // 現在の顔の向きを基準として保存
            _initHeadRotation_Vec3 = GetHeadRotationEuler(landmarks, _isCalibrating);

            //_hasSetInitHeadRotation = true;
            _isCalibrating = false;

            Debug.Log("Calibrated Head Rotation");

#if UNITY_EDITOR
            Debug.Log($"Head Rotation Calibrated: {_initHeadRotation_Vec3}");
#endif
        }

        public void ResetFaceTracking()
        {
            //StopCoroutine(RunFaceTracking());
            ResetGraph();
            StartCoroutine(RunFaceTracking());
        }
    }
}