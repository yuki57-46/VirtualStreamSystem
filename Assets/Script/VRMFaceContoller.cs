using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using Mediapipe;
using UniVRM10;
using VRM;

public class VRMFaceContoller : MonoBehaviour
{
    [SerializeField] private VRMLoder vrmLoder;
    private VRMBlendShapeProxy blendShapeProxy;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var vrmModel = vrmLoder.VRMModel;

        if (vrmModel != null)
        {
            blendShapeProxy = vrmModel.GetComponent<VRMBlendShapeProxy>(); 
        }

    }

    public void UpdateVRMFace(NormalizedLandmarkList landmarkList)
    {

        if (blendShapeProxy == null)
        {
            var vrmModel = vrmLoder.VRMModel;

            if (vrmModel != null)
            {
                blendShapeProxy = vrmModel.GetComponent<VRMBlendShapeProxy>();
            }
        }


        if (blendShapeProxy == null || landmarkList == null)
        {
            return;
        }

        // フェイストラッキングの結果を使って、VRMのBlendShapeを操作
        var landmarks = landmarkList.Landmark;

        // 口の開閉
        var mouthOpen = Vector3.Distance(
            new Vector3(landmarks[13].X, landmarks[13].Y, landmarks[13].Z), //
            new Vector3(landmarks[14].X, landmarks[14].Y, landmarks[14].Z)  //
        );
        blendShapeProxy.ImmediatelySetValue(BlendShapePreset.A, mouthOpen * 50);

        // 目の開閉
        var leftEyeOpen = Vector3.Distance(
            new Vector3(landmarks[159].X, landmarks[159].Y, landmarks[159].Z), //
            new Vector3(landmarks[145].X, landmarks[145].Y, landmarks[145].Z)  //
        );

        var rightEyeOpen = Vector3.Distance(
            new Vector3(landmarks[386].X, landmarks[386].Y, landmarks[386].Z), //
            new Vector3(landmarks[374].X, landmarks[374].Y, landmarks[374].Z)  //
        );

        float normalizedLeftEye = Mathf.Clamp01(leftEyeOpen * 50);
        float normalizedRightEye = Mathf.Clamp01(rightEyeOpen * 50);

        blendShapeProxy.ImmediatelySetValue(BlendShapePreset.Blink_L, 1.0f - normalizedLeftEye);
        blendShapeProxy.ImmediatelySetValue(BlendShapePreset.Blink_R, 1.0f - normalizedRightEye);

        Debug.Log($"leftEyeOpen: {leftEyeOpen}, rightEyeOpen: {rightEyeOpen}");

    }
}
