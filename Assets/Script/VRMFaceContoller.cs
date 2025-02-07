using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

using Mediapipe;
using UniVRM10;
using VRM;
using UnityEngine.UI;


public class VRMFaceContoller : MonoBehaviour
{
    [SerializeField] private VRMLoder vrmLoder;
    private VRMBlendShapeProxy blendShapeProxy;

    public Slider eyeSensitivtySlider; // 目の感度調整用
    public Slider mouthSensitivtySlider; // 口の感度調整用

    private float eyeMultiplier = 50.0f; // 目の感度のデフォルト値
    private float mouthMultiplier = 30.0f; // 口の感度のデフォルト値

    public float eyeSliderMaxValue = 50.0f; // 目の感度の最大値
    public float mouthSliderMaxValue = 100.0f; // 口の感度の最大値


    // 目・口の基準値(初回計測用)
    private float baseMouthOpen = 0.01f;
    private float baseLeftEyeOpen = 0.01f;
    private float baseRightEyeOpen = 0.01f;

    // 目と口の開閉のスムージング用
    private float smoothMouthOpen = 0.0f;
    private float smoothLeftEyeOpen = 0.0f;
    private float smoothRightEyeOpen = 0.0f;

    private const float SMOOTHFACTOR = 0.1f; // スムージング係数

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var vrmModel = vrmLoder.VRMModel;

        if (vrmModel != null)
        {
            blendShapeProxy = vrmModel.GetComponent<VRMBlendShapeProxy>(); 
        }

        // スライダーの値を初期化
        if (eyeSensitivtySlider != null)
        {
            eyeSensitivtySlider.value = eyeMultiplier;
            eyeSensitivtySlider.maxValue = eyeSliderMaxValue;
        }
        if (mouthSensitivtySlider != null)
        {
            mouthSensitivtySlider.value = mouthMultiplier;
            mouthSensitivtySlider.maxValue = mouthSliderMaxValue;
        }

        // スライダーの変更イベントを登録
        if (eyeSensitivtySlider != null)
        {
            eyeSensitivtySlider.onValueChanged.AddListener(SetEyeSensitivity);
        }
        if (mouthSensitivtySlider != null)
        {
            mouthSensitivtySlider.onValueChanged.AddListener(SetMouthSensitivity);
        }
    }

    public void SetEyeSensitivity(float value)
    {
        eyeMultiplier = Mathf.Clamp(value, 0.02f, eyeSliderMaxValue);
    }

    public void SetMouthSensitivity(float value)
    {
        mouthMultiplier = Mathf.Clamp(value, 0.02f, mouthSliderMaxValue);
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

        var mouthWidth = Vector3.Distance(
            new Vector3(landmarks[78].X, landmarks[78].Y, landmarks[78].Z), //
            new Vector3(landmarks[308].X, landmarks[308].Y, landmarks[308].Z)  //
            );

        smoothMouthOpen = Mathf.Lerp(smoothMouthOpen, mouthOpen, SMOOTHFACTOR);
        float openValue = Mathf.Clamp01(smoothMouthOpen * mouthMultiplier);
        float widthValue = Mathf.Clamp01(mouthWidth * mouthMultiplier);

#pragma warning disable CS0618 // 型またはメンバーが旧型式です
        //if (mouthOpen > baseMouthOpen)
        //{
        //    float normalizedMouth = Mathf.Clamp01((mouthOpen * mouthMultiplier)); // 係数調整
        //    //smoothMouthOpen = Mathf.Lerp(smoothMouthOpen, normalizedMouth, SMOOTHFACTOR);
        //    blendShapeProxy.ImmediatelySetValue(BlendShapePreset.A, normalizedMouth);
        //}
        //else
        //{
        //    smoothMouthOpen = Mathf.Lerp(smoothMouthOpen, 0.0f, SMOOTHFACTOR);
        //    blendShapeProxy.ImmediatelySetValue(BlendShapePreset.A, 0.0f);
        //}

        // 各母音に対応するBlendShapeに値を設定
        float aVal = openValue * (1.0f - widthValue); // あ 口が開きつつ幅が狭い
        float iVal = widthValue * 0.9f; // い 幅が広い
        float uVal = (1.0f - widthValue) * 0.8f; // う すぼまる
        float eVal = widthValue * openValue * 0.7f; // え 口が開いて横に広がる
        float oVal = openValue * (1.0f - widthValue) * 0.9f; // お

#pragma warning restore CS0618 // 型またはメンバーが旧型式です

        // 目の開閉
        var leftEyeOpen = Vector3.Distance(
            new Vector3(landmarks[159].X, landmarks[159].Y, landmarks[159].Z), //
            new Vector3(landmarks[145].X, landmarks[145].Y, landmarks[145].Z)  //
        );

        var rightEyeOpen = Vector3.Distance(
            new Vector3(landmarks[386].X, landmarks[386].Y, landmarks[386].Z), //
            new Vector3(landmarks[374].X, landmarks[374].Y, landmarks[374].Z)  //
        );

        float normalizedLeftEye = Mathf.Clamp01(leftEyeOpen * eyeMultiplier);
        float normalizedRightEye = Mathf.Clamp01(rightEyeOpen * eyeMultiplier);


        //smoothLeftEyeOpen = Mathf.Lerp(smoothLeftEyeOpen, 1.0f - normalizedLeftEye, SMOOTHFACTOR);
        //smoothRightEyeOpen = Mathf.Lerp(smoothRightEyeOpen, 1.0f - normalizedRightEye, SMOOTHFACTOR);

#pragma warning disable CS0618 // 型またはメンバーが旧型式です
        blendShapeProxy.ImmediatelySetValue(BlendShapePreset.Blink_L, 1.0f - normalizedLeftEye);
        blendShapeProxy.ImmediatelySetValue(BlendShapePreset.Blink_R, 1.0f - normalizedRightEye);
#pragma warning restore CS0618 // 型またはメンバーが旧型式です


        Debug.Log($"leftEyeOpen: {leftEyeOpen}, rightEyeOpen: {rightEyeOpen}");
        Debug.Log($"eyeMultiplier: {eyeMultiplier}");
    }
}
