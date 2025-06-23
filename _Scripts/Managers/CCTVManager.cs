using UnityEngine;

public class CCTVManager : MonoBehaviour
{
    private Camera mainCamera;

    [Header("Cameras")]
    [Tooltip("List of CCTV cameras to cycle through in CCTV mode.")]
    public Camera[] cctvCameras;

    [Header("UI")]
    [Tooltip("The Canvas (or UI GameObject) to show when in CCTV mode.")]
    [SerializeField] private GameObject cctvUI; 

    [Header("State")]
    [Tooltip("Index of the current active CCTV camera.")]
    public int currentCameraIndex = 0;
    [Tooltip("Is CCTV mode active?")]
    public bool isCCTVActive = false;

    void Start()
    {
        mainCamera = GetComponent<Camera>();

        // Disable all CCTV cameras
        foreach (Camera cam in cctvCameras)
            if (cam != null) cam.enabled = false;

        // Hide the CCTV UI at start
        if (cctvUI != null)
            cctvUI.SetActive(false);
    }

    void Update()
    {
        if (!isCCTVActive) return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            SwitchToNextCamera();
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            SwitchToPreviousCamera();
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            DeactivateCCTV();
    }

    /// <summary>
    /// Activates CCTV mode: disables main cam, enables first CCTV cam, and shows UI.
    /// </summary>
    public void ActivateCCTV()
    {
        if (isCCTVActive) return;
        isCCTVActive = true;

        if (mainCamera != null)
            mainCamera.enabled = false;

        if (cctvCameras.Length > 0 && cctvCameras[0] != null)
        {
            currentCameraIndex = 0;
            cctvCameras[0].enabled = true;
        }

        if (cctvUI != null)
            cctvUI.SetActive(true);
    }

    /// <summary>
    /// Deactivates CCTV mode: hides all CCTV cams, reenables main cam, hides UI.
    /// </summary>
    public void DeactivateCCTV()
    {
        if (!isCCTVActive) return;
        isCCTVActive = false;

        foreach (Camera cam in cctvCameras)
            if (cam != null) cam.enabled = false;

        if (mainCamera != null)
            mainCamera.enabled = true;

        if (cctvUI != null)
            cctvUI.SetActive(false);
    }

    private void SwitchToNextCamera()
    {
        if (cctvCameras.Length == 0) return;

        cctvCameras[currentCameraIndex].enabled = false;
        currentCameraIndex = (currentCameraIndex + 1) % cctvCameras.Length;
        cctvCameras[currentCameraIndex].enabled = true;
    }

    private void SwitchToPreviousCamera()
    {
        if (cctvCameras.Length == 0) return;

        cctvCameras[currentCameraIndex].enabled = false;
        currentCameraIndex = (currentCameraIndex - 1 + cctvCameras.Length) % cctvCameras.Length;
        cctvCameras[currentCameraIndex].enabled = true;
    }
}
