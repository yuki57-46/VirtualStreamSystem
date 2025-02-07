
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

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
        
    }

    public void OnWheel(InputValue inputValue)
    {
        Debug.Log("OnWheel" + inputValue.Get<Vector2>());

        float delta = inputValue.Get<Vector2>().y;

        // カメラのZ座標を更新
        mainCamera.transform.position += mainCamera.transform.forward * delta;

    }
}
