using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneMenuManager : MonoBehaviour
{
    private static SceneMenuManager instance;

    [SerializeField] private GameObject menuPanel;
    //[SerializeField] private Button sceneButton;
    [SerializeField] private Transform buttonCenter;


    private void Awake()
    {
        // インスタンスが存在する場合は削除
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // インスタンスが存在しない場合は自身をインスタンスとして設定
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // メニューの非表示
        menuPanel.SetActive(false);

        // BuildSettingsからシーンの数を取得
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; ++i)
        {
            // シーンのパスを取得して名前を抽出
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);

            // ボタンを生成
            //Button button = Instantiate(sceneButton, buttonCenter);
            //button.GetComponentInChildren<TextMeshPro>().text = sceneName;

            // ボタンにクリックイベントを追加
            int buildIndex = i;
            //button.onClick.AddListener(() => LoadSceneByIndexAsync(buildIndex));
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleMenu()
    {
        // メニューの表示切り替え
        menuPanel.SetActive(!menuPanel.activeSelf);
    }

    private void LoadSceneByIndexAsync(int buildIndex)
    {
        SceneManager.LoadSceneAsync(buildIndex);
    }
}
