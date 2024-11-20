using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnPlane : MonoBehaviour
{
    [Tooltip("AR空間に召喚するオブジェクト")]
    [SerializeField] private GameObject obj;
    [SerializeField] private InputActionAsset input;

    private GameObject spawnedObj;
    private ARRaycastManager raycastManager;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        EnhancedTouchSupport.Enable();
    }

    // Update is called once per frame
    void Update()
    {

        if (Touch.activeTouches.Count == 0) return;

        Vector2 touchPosition = Touch.activeTouches[0].screenPosition;
        if (raycastManager.Raycast(touchPosition, hits, TrackableType.Planes))
        {
            // Raycastの衝突情報は距離によってソートされるため、0番目が最も近い場所でヒットした情報
            var hitPose = hits[0].pose;

            if (spawnedObj)
            {
                spawnedObj.transform.position = hitPose.position;
            }
            else
            {
                spawnedObj = Instantiate(obj, hitPose.position, Quaternion.identity);
            }
        }
    }

}
