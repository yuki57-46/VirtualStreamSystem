using System.Collections.Generic;
using UnityEngine;

using System.IO;
using VRM;
using UniVRM10;
using UniGLTF;


public class VRMLoder : MonoBehaviour
{
    // ファイルパスの指定
    public string vrmFilePath;

    [SerializeField]
    private GameObject defaultModel;

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
            VRMModel = (GameObject)model;
            AlignModelToCamera(VRMModel);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"VRM 読み込み失敗: {e.Message}");
        }
        //if (string.IsNullOrEmpty(vrmFilePath))
        //{

        //    // 座標系を(+X, +Y, -Z)に変換
        //    defaultModel.transform.Rotate(0, 180, 0);

        //    // ファイルパスが指定されていない場合はデフォルトのvrmファイルを読み込む
        //    var model = Instantiate(defaultModel);

        //    VRMModel = model;

        //}
        //else
        //{
        //    GltfData data = new AutoGltfFileParser(vrmFilePath).Parse();

        //    var model = await Vrm10.LoadPathAsync(vrmFilePath, canLoadVrm0X: true, materialGenerator: new UrpVrm10MaterialDescriptorGenerator());
        //    // 座標系を(+X, +Y, -Z)に変換
        //    model.transform.Rotate(0, 180, 0);

        //    VRMModel = (GameObject)model;

        //}

        //AlignModelToCamera(VRMModel);

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
