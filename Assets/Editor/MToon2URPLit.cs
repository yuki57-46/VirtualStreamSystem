using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class MToon2URPLit : EditorWindow
{
    private List<GameObject> targetGameObjects = new List<GameObject>();
    private bool showDetails = false;

    [MenuItem("Toshi/MToon2URPLit")]
    public static void ShowWindow()
    {
        GetWindow<MToon2URPLit>("MToon2URPLit");
    }

    private void OnGUI()
    {
        GUILayout.Label("MToonシェーダーをURP/litシェーダーに変換するため、GameObjectを選択して「Apply」ボタンをクリックしてください。");

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("対象GameObject", GUILayout.Width(90));
        GUILayout.Label("（複数選択可能）");
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel++;

        EditorGUILayout.BeginVertical(GUI.skin.box);

        int targetCount = targetGameObjects.Count;

        for (int i = 0; i < targetCount; i++)
        {
            EditorGUILayout.BeginHorizontal();

            targetGameObjects[i] = EditorGUILayout.ObjectField(targetGameObjects[i], typeof(GameObject), true) as GameObject;

            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                targetGameObjects.RemoveAt(i);
                targetCount--;
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("+", GUILayout.Width(20)))
        {
            targetGameObjects.Add(null);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        showDetails = EditorGUILayout.Foldout(showDetails, "詳細情報");

        if (showDetails)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("変換されるMaterial一覧：");

            foreach (GameObject targetGameObject in targetGameObjects)
            {
                if (targetGameObject != null)
                {
                    Renderer[] childRenderers = targetGameObject.GetComponentsInChildren<Renderer>();

                    EditorGUI.indentLevel++;

                    foreach (Renderer renderer in childRenderers)
                    {
                        Material[] materials = renderer.sharedMaterials;

                        foreach (Material material in materials)
                        {
                            if (material.shader.name.Contains("VRM/MToon"))
                            {
                                EditorGUILayout.LabelField(renderer.gameObject.name + " - " + material.name);
                            }
                        }
                    }

                    if (childRenderers.Length == 0)
                    {
                        EditorGUILayout.LabelField(targetGameObject.name + " - " + "Renderersが見つかりませんでした。");
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("対象GameObjectを選択してください。");
                }
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Apply"))
        {
            foreach (GameObject targetGameObject in targetGameObjects)
            {
                if (targetGameObject != null)
                {
                    Renderer[] childRenderers = targetGameObject.GetComponentsInChildren<Renderer>();

                    foreach (Renderer renderer in childRenderers)
                    {
                        Material[] materials = renderer.sharedMaterials;

                        for (int i = 0; i < materials.Length; i++)
                        {
                            Material material = materials[i];

                            if (material.shader.name.Contains("VRM/MToon"))
                            {
                                // テクスチャを取得する
                                Texture2D texture = material.GetTexture("_MainTex") as Texture2D;
                                                            // Universal Render Pipeline/litシェーダーに変換する
                                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                                material.shader = shader;

                                // テクスチャをBase Mapに登録する
                                material.SetTexture("_BaseMap", texture);
                            }
                        }
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("エラー", "対象GameObjectを選択してください。", "OK");
                }
            }
        }
    }   
}
