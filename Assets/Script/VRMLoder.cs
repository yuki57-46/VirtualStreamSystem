using System.Collections.Generic;
using UnityEngine;

using System.IO;
using VRM;
using UniVRM10;
using UniGLTF;
using SFB;
using Mediapipe.Unity;

public class VRMLoder : MonoBehaviour
{
    // ファイルパスの指定
    public string vrmFilePath;

    [SerializeField]
    private GameObject defaultModel;
    [SerializeField] private VRMFaceContoller faceContoller; // FaceMeshCustom にモデル変更を通知するため

    public GameObject VRMModel { get; set; }

    RuntimeGltfInstance instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadDefaultModel();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenFileDialog()
    {
        var path = StandaloneFileBrowser.OpenFilePanel("Open VRM File", "", "vrm", false);
        if (path.Length > 0)
        {
            LoadVRMFile(path[0]);
        }
    }




    public void LoadVRMFile(string filePath)
    {

        if (File.Exists(filePath))
        {
            LoadVRM(filePath);
        }
        else
        {
            Debug.LogError($"VRMファイルが見つかりません: {filePath}");

        }
    }

    async void LoadVRM(string filePath)
    {

        // 既にモデルが読み込まれている場合は削除
        if (VRMModel != null)
        {
            Destroy(VRMModel);
        }

        Debug.Log(filePath);
        
        try
        {
            var model = await Vrm10.LoadPathAsync(filePath, canLoadVrm0X: true,
                materialGenerator: new UrpVrm10MaterialDescriptorGenerator());

            model.transform.Rotate(0, 180, 0);
            VRMModel = /*(GameObject)*/model.gameObject;
            AlignModelToCamera(VRMModel);

            // FaceMeshCustom にモデル変更を通知
            faceContoller = GetComponent<VRMFaceContoller>();
            if (faceContoller != null)
            {
                faceContoller.SetNewVRMModel(VRMModel);
            }

            var FaceTracker = GetComponent<FaceMeshCustom>();
            if (FaceTracker != null)
            {
                FaceTracker.ResetFaceTracking();
            }
            var VRMBodyController = GetComponent<VRMBodyController>();
            if (VRMBodyController != null)
            {
                VRMBodyController.SetAPose();
            }

        }
        catch (System.NotImplementedException e)
        {
            // このエラーは無視しても問題ないと思われる
        }
        catch (System.Exception e)
        {
            Debug.LogError($"VRM 読み込み失敗: {e.Message}");
        }

    }

    private void LoadDefaultModel()
    {
        // デフォルトのモデルを読み込む
        VRMModel = Instantiate(defaultModel);
        VRMModel.transform.Rotate(0, 180, 0);
        AlignModelToCamera(VRMModel);

    }


    // モデルをカメラの方向に向ける
    private void AlignModelToCamera(GameObject model)
    {
        if (model == null) return;

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        // カメラの位置と向き
        Vector3 cameraPos = mainCam.transform.position;
        Vector3 modelPos = model.transform.position;

        // カメラに向けてモデルを回転
        Vector3 directionToCamera = (cameraPos - modelPos).normalized;
        directionToCamera.y = 0;
        model.transform.rotation = Quaternion.LookRotation(directionToCamera);

    }

}
