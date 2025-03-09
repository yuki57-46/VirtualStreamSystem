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
    public VRMLoder vrmLoder;
    private VRMBlendShapeProxy blendShapeProxy;
    private Vrm10Instance vrmInstance;
 

    public Slider eyeSensitivtySlider; // 目の感度調整用
    public Slider mouthSensitivtySlider; // 口の感度調整用

    private float eyeMultiplier = 50.0f; // 目の感度のデフォルト値
    private float mouthMultiplier = 30.0f; // 口の感度のデフォルト値

    public float eyeSliderMaxValue = 50.0f; // 目の感度の最大値
    public float mouthSliderMaxValue = 100.0f; // 口の感度の最大値

    public SettingManager settingManager; // 設定ファイルの内容


    // 目・口の基準値(初回計測用)
    private float baseMouthOpen = 0.01f;
    private float baseLeftEyeOpen = 0.01f;
    private float baseRightEyeOpen = 0.01f;

    // 目と口の開閉のスムージング用
    private float smoothMouthOpen = 0.0f;
    private float smoothLeftEyeOpen = 0.0f;
    private float smoothRightEyeOpen = 0.0f;

    private const float SMOOTHFACTOR = 0.2f; // スムージング係数

    private Transform headBone; // 頭のボーン
    private Transform NeckBone; // 首のボーン

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
        vrmLoder = GetComponent<VRMLoder>();
        var vrmModel = vrmLoder.VRMModel;
        

        // 設定ファイルから感度の設定を読み込む
        if (settingManager != null)
        {
            eyeMultiplier = settingManager.settings.eyeSensitivtySlider;
            mouthMultiplier = settingManager.settings.mouthSensitivtySlider;
#if UNITY_EDITOR
            Debug.Log("設定ファイルを読み込みました");
#endif
        }

        if (vrmModel != null)
        {
            blendShapeProxy = vrmModel.GetComponent<VRMBlendShapeProxy>();
            if (blendShapeProxy == null)
            {
                vrmInstance = vrmModel.GetComponent<Vrm10Instance>();
                if (vrmInstance == null)
                {
                    Debug.LogError("VRMBlendShapeProxyが見つかりません");
                    Debug.LogError("Vrm10Instanceが見つかりません");
                }
            }
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
            else
            {
                vrmInstance = vrmModel.GetComponent<Vrm10Instance>();
                if (vrmInstance == null)
                {
                    Debug.LogError("VRMBlendShapeProxyが見つかりません");
                    Debug.LogError("Vrm10Instanceが見つかりません");
                }
            }
        }



        if (blendShapeProxy == null && vrmInstance == null  || landmarkList == null)
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


        float mouthMin = 0.05f;
        float mouthMax = 1.2f;
        mouthWidth = Mathf.InverseLerp(mouthMin, mouthMax, mouthWidth);


        smoothMouthOpen = Mathf.Lerp(smoothMouthOpen, mouthOpen, SMOOTHFACTOR);
        float openValue = Mathf.Clamp(smoothMouthOpen * mouthMultiplier, 0.1f, 3.0f);
        float widthValue = Mathf.Clamp(mouthWidth * mouthMultiplier * 0.5f, 0.1f, 3.0f);

        Debug.Log($"Processed openValue: {openValue}, Processed widthValue: {widthValue}");

#pragma warning disable CS0618 // 型またはメンバーが旧型式です
        
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

        if (blendShapeProxy != null)
        {

            blendShapeProxy.SetValues(new Dictionary<BlendShapeKey, float>
            {
                {BlendShapeKey.CreateFromPreset(BlendShapePreset.A), detectedVowel == "A" ? openValue : 0.0f},
                {BlendShapeKey.CreateFromPreset(BlendShapePreset.I), detectedVowel == "I" ? openValue : 0.0f},
                {BlendShapeKey.CreateFromPreset(BlendShapePreset.U), detectedVowel == "U" ? openValue : 0.0f},
                {BlendShapeKey.CreateFromPreset(BlendShapePreset.E), detectedVowel == "E" ? openValue : 0.0f},
                {BlendShapeKey.CreateFromPreset(BlendShapePreset.O), detectedVowel == "O" ? openValue : 0.0f}
            });
        }
        else if (vrmInstance != null)
        {
            var expression = vrmInstance.Runtime.Expression;
            expression.SetWeight(ExpressionKey.Aa, detectedVowel == "A" ? openValue : 0.0f);
            expression.SetWeight(ExpressionKey.Ih, detectedVowel == "I" ? openValue : 0.0f);
            expression.SetWeight(ExpressionKey.Ou, detectedVowel == "U" ? openValue : 0.0f);
            expression.SetWeight(ExpressionKey.Ee, detectedVowel == "E" ? openValue : 0.0f);
            expression.SetWeight(ExpressionKey.Oh, detectedVowel == "O" ? openValue : 0.0f);
        }




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


        if (blendShapeProxy != null)
        {
            blendShapeProxy.SetValues(new Dictionary<BlendShapeKey, float>
            {
                {BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_L), 1.0f - normalizedLeftEye},
                {BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink_R), 1.0f - normalizedRightEye}
            });
        }
        else if (vrmInstance != null)
        {
            var expression = vrmInstance.Runtime.Expression;
            expression.SetWeight(ExpressionKey.BlinkLeft, 1.0f - normalizedLeftEye);
            expression.SetWeight(ExpressionKey.BlinkRight, 1.0f - normalizedRightEye);
        }


        // Debug.Log($"leftEyeOpen: {leftEyeOpen}, rightEyeOpen: {rightEyeOpen}");
        // Debug.Log($"eyeMultiplier: {eyeMultiplier}");
    }

    public void UpdateHeadRotation(Quaternion headRotation)
    {
        Quaternion reverseValue = Quaternion.Euler(0.0f, 180.0f, 0.0f);

        if (vrmInstance != null)
        {
            headBone = vrmInstance.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
            
            if (headBone != null)
            {
                Vector3 adjustedEuler = new Vector3(headRotation.x, -headRotation.y, headRotation.z);
                headBone.localEulerAngles = adjustedEuler;

                //headBone.rotation = headRotation;
            }
            else
            {
                Debug.LogError("頭のボーンが見つかりません");
            }
        }
        else if (blendShapeProxy != null)
        {
            headBone = vrmLoder.VRMModel.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
            if (headBone != null)
            {
                Quaternion adjustedRotation = headRotation * reverseValue;
                headBone.rotation = adjustedRotation;

                //headBone.rotation = headRotation * reverseValue;
                //headBone.localRotation = headRotation;
            }
            else
            {
                Debug.LogError("頭のボーンが見つかりません");
            }
        }
        else
        {
            Debug.LogError("VRMBlendShapeProxyが見つかりません");
            Debug.LogError("Vrm10Instanceが見つかりません");
        }
    }

    public void UpdateHeadRotation(Vector3 headEulerAngle)
    {
        if (vrmInstance != null)
        {
            headBone = vrmInstance.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
            if (headBone != null)
            {
                headBone.localEulerAngles = headEulerAngle;
            }
            else
            {
                Debug.LogError("頭のボーンが見つかりません");
            }
        }
        else if (blendShapeProxy != null)
        {
            headBone = vrmLoder.VRMModel.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
            if (headBone != null)
            {
                headBone.localEulerAngles = headEulerAngle;
            }
            else
            {
                Debug.LogError("頭のボーンが見つかりません");
            }
        }
        else
        {
            Debug.LogError("VRMBlendShapeProxyが見つかりません");
            Debug.LogError("Vrm10Instanceが見つかりません");
        }
    }

    public void SetNewVRMModel(GameObject newVRM)
    {
        if (newVRM == null) return;

        vrmLoder.VRMModel = newVRM;
        if (newVRM.TryGetComponent(out blendShapeProxy))
        {
            blendShapeProxy = newVRM.GetComponent<VRMBlendShapeProxy>();
        }
        else if (newVRM.TryGetComponent(out vrmInstance))
        {
            vrmInstance = newVRM.GetComponent<Vrm10Instance>();
        }

        if (blendShapeProxy == null && vrmInstance == null)
        {
            Debug.LogError("VRMBlendShapeProxyが見つかりません");
            Debug.LogError("Vrm10Instanceが見つかりません");
        }
    }

    // アプリケーション終了時に感度の設定を保存
    private void OnDestroy()
    {
        settingManager.SetSensitivitySettings(eyeMultiplier, mouthMultiplier);
    }
}
