using UnityEngine;
using UnityEngine.UI;

public class WebCamSet : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WebCamTexture webCam = new WebCamTexture();
        RawImage rawImage = GetComponent<RawImage>();
        rawImage.texture = webCam;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
