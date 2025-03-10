
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class MainCameraController : MonoBehaviour
{
    public Camera mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = GetComponent<Camera>();    
    }

    // Update is called once per frame
    void Update()
    {

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            // 再起動
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            //System.Diagnostics.Process.Start(Application.dataPath + "/../" + Application.productName + ".exe");
            //Application.Quit();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
#endif
        }

    }

    public void OnWheel(InputValue inputValue)
    {
        //Debug.Log("OnWheel" + inputValue.Get<Vector2>());

        float delta = inputValue.Get<Vector2>().y;

        // カメラのZ座標を更新
        mainCamera.transform.position += mainCamera.transform.forward * delta;

    }
}
