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

    private const float SMOOTHFACTOR = 0.2f; // スムージング係数

#if UNITY_EDITOR
    // Editor上でのデバッグ用
    private float openValueMax = 0.0f;  
    private float widthValueMax = 0.0f;
    private float openValueMin = 100.0f;
    private float widthValueMin = 100.0f;
#endif

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
            new Vector3(landmarks[61].X, landmarks[61].Y, landmarks[61].Z), //
            new Vector3(landmarks[291].X, landmarks[291].Y, landmarks[291].Z)  //
            );

        //mouthWidth = Mathf.Clamp01(mouthWidth * 0.01f); // 係数調整
        //float beforeMouthWidth = mouthWidth;
        float mouthMin = 0.05f;
        float mouthMax = 1.2f;
        mouthWidth = Mathf.InverseLerp(mouthMin, mouthMax, mouthWidth);
        //mouthWidth = Mathf.Clamp(mouthWidth * 2.0f, 0.1f, 1.2f);
        //Debug.Log($"Before Scaling: {beforeMouthWidth}, After Scaling: {mouthWidth}");
        //mouthOpen = Mathf.Clamp(mouthOpen * 2.0f, 0.01f, 1.0f);

        smoothMouthOpen = Mathf.Lerp(smoothMouthOpen, mouthOpen, SMOOTHFACTOR);
        float openValue = Mathf.Clamp(smoothMouthOpen * mouthMultiplier, 0.1f, 3.0f);
        float widthValue = Mathf.Clamp(mouthWidth * mouthMultiplier * 0.5f, 0.1f, 3.0f);

        Debug.Log($"Processed openValue: {openValue}, Processed widthValue: {widthValue}");

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

        //各母音に対応するBlendShapeに値を設定
        //float aVal = openValue * (1.0f - widthValue); // あ 口が開きつつ幅が狭い
        //float iVal = widthValue * 0.9f; // い 幅が広い
        //float uVal = (1.0f - widthValue) * 0.8f; // う すぼまる
        //float eVal = widthValue * openValue * 0.7f; // え 口が開いて横に広がる
        //float oVal = openValue * (1.0f - widthValue) * 0.9f; // お
        //blendShapeProxy.ImmediatelySetValue(BlendShapePreset.A, aVal);
        //blendShapeProxy.ImmediatelySetValue(BlendShapePreset.I, iVal);
        //blendShapeProxy.ImmediatelySetValue(BlendShapePreset.U, uVal);
        //blendShapeProxy.ImmediatelySetValue(BlendShapePreset.E, eVal);
        //blendShapeProxy.ImmediatelySetValue(BlendShapePreset.O, oVal);
        //Debug.Log($"aVal: {aVal}, iVal: {iVal}, uVal: {uVal}, eVal: {eVal}, oVal: {oVal}");

        string detectedVowel = "None";
        if (openValue <= 0.1 && widthValue <= 0.13f) detectedVowel = "None"; // 口が開いていない
        else if (openValue < 0.2f && widthValue < 0.2f && widthValue >= 0.12f) detectedVowel = "U";  // う：口の開きも幅も小さい
        else if (openValue < 1.0f && widthValue > 0.3f) detectedVowel = "I";  // い：口の開きが小さく、幅が広い
        else if (openValue > 1.5f && widthValue > 0.4f) detectedVowel = "E";  // え：中間的な形
        else if (openValue > 1.2f && widthValue < 0.2f /*&& widthValue < 1.1f*/) detectedVowel = "O";  // お：あより幅が狭め
        else if (openValue < 2.0f && widthValue < 0.3f) detectedVowel = "A";  // あ：口が大きく開いていて幅も広い
        Debug.Log($"Detected Vowel: {detectedVowel} (openValue: {openValue}, widthValue: {widthValue})");

#if UNITY_EDITOR

        if (openValue > openValueMax)
        {
            openValueMax = openValue;
        }
        if (widthValue > widthValueMax)
        {
            widthValueMax = widthValue;
        }
        if (openValue < openValueMin)
        {
            openValueMin = openValue;
        }
        if (widthValue < widthValueMin)
        {
            widthValueMin = widthValue;
        }

        Debug.Log($"openValueMax: {openValueMax}, widthValueMax: {widthValueMax}");
        Debug.Log($"openValueMin: {openValueMin}, widthValueMin: {widthValueMin}");
#endif


        blendShapeProxy.ImmediatelySetValue(BlendShapePreset.A, detectedVowel == "A" ? openValue : 0.0f);
        blendShapeProxy.ImmediatelySetValue(BlendShapePreset.I, detectedVowel == "I" ? openValue : 0.0f);
        blendShapeProxy.ImmediatelySetValue(BlendShapePreset.U, detectedVowel == "U" ? openValue : 0.0f);
        blendShapeProxy.ImmediatelySetValue(BlendShapePreset.E, detectedVowel == "E" ? openValue : 0.0f);
        blendShapeProxy.ImmediatelySetValue(BlendShapePreset.O, detectedVowel == "O" ? openValue : 0.0f);

        //Debug.Log($"detectedVowel: {detectedVowel}");
        //Debug.Log($"openValue: {openValue}, widthValue: {widthValue}");


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


       // Debug.Log($"leftEyeOpen: {leftEyeOpen}, rightEyeOpen: {rightEyeOpen}");
       // Debug.Log($"eyeMultiplier: {eyeMultiplier}");
    }
}
