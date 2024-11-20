using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebCamera_Test : MonoBehaviour
{
    public RawImage rawImage;
    WebCamTexture webCam;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        webCam = new WebCamTexture();
        // RawImageにWebCamTextureを設定
        rawImage.texture = webCam;

        webCam.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
