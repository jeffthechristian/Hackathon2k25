using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class UIToggleandfollow : MonoBehaviour
{
    [SerializeField] private InputActionReference bButtonAction;
    public Canvas uiCanvas; // Reference to the worldspace UI Canvas
    public Transform cameraTransform; // Reference to the CenterEyeAnchor (main camera)
    public float distanceFromCamera = 1.5f; // Distance to place UI in front of camera
    public float smoothSpeed = 5f; // Speed for smooth following

    private bool isUIVisible = false;

    void Start()
    {
        if (uiCanvas == null)
        {
            Debug.LogError("UI Canvas not assigned!");
            return;
        }

        if (cameraTransform == null)
        {
            // Find the CenterEyeAnchor under OVRCameraRig
            cameraTransform = GameObject.Find("OVRCameraRig/TrackingSpace/CenterEyeAnchor").transform;
            if (cameraTransform == null)
            {
                Debug.LogError("CenterEyeAnchor not found!");
            }
        }

        // Hide UI initially
        uiCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (bButtonAction != null && bButtonAction.action != null && bButtonAction.action.triggered)
        {
            ToggleUI();
        }

        // Make UI follow camera if visible
        if (isUIVisible)
        {
            UpdateUIPosition();
        }
    }

    public void ToggleUI()
    {
        isUIVisible = !isUIVisible;
        uiCanvas.gameObject.SetActive(isUIVisible);
        Debug.Log(isUIVisible ? "UI Shown" : "UI Hidden");
        // Position UI in front of camera when shown
        if (isUIVisible)
        {
            UpdateUIPosition();
        }
    }

    void UpdateUIPosition()
    {
        if (cameraTransform == null) return;

        // Calculate desired position in front of camera
        Vector3 targetPosition = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
        // Keep UI at the same height as the camera (optional, adjust as needed)
        targetPosition.y = cameraTransform.position.y;

        // Smoothly move UI to target position
        uiCanvas.transform.position = Vector3.Lerp(uiCanvas.transform.position, targetPosition, Time.deltaTime * smoothSpeed);

        // Make UI face the camera
        uiCanvas.transform.rotation = Quaternion.LookRotation(uiCanvas.transform.position - cameraTransform.position);
    }
}