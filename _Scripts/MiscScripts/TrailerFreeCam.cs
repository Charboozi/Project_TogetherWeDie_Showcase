using UnityEngine;

public class TrailerFreeCam : MonoBehaviour
{
    [Header("Main Gameplay References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject gameUICanvas;

    [Header("Freecam Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float speedStep = 2f;
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float maxSpeed = 50f;

    private Camera freeCam;
    private AudioListener freeCamListener;
    private AudioListener mainCamListener;

    private bool isActive = false;
    private float yaw, pitch;

    void Start()
    {
        freeCam = GetComponent<Camera>();
        freeCamListener = GetComponent<AudioListener>();

        if (freeCam == null)
        {
            Debug.LogError("TrailerFreeCam: Attach this script to a Camera.");
            enabled = false;
            return;
        }

        if (mainCamera != null)
            mainCamListener = mainCamera.GetComponent<AudioListener>();

        freeCam.enabled = false;
        if (freeCamListener != null)
            freeCamListener.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (!isActive && Input.GetKeyDown(KeyCode.C))
        {
            ActivateFreeCam();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filename = $"Screenshot_{timestamp}.png";
            ScreenCapture.CaptureScreenshot(filename);
            Debug.Log($"📸 Screenshot taken: {filename}");
        }

        if (!isActive) return;

        HandleMovement();
        HandleLook();
        HandleSpeedChange();
    }

    private void ActivateFreeCam()
    {
        isActive = true;

        if (mainCamera != null)
        {
            mainCamera.enabled = false;
            if (mainCamListener != null) mainCamListener.enabled = false;
        }

        if (gameUICanvas != null)
            gameUICanvas.SetActive(false);

        freeCam.enabled = true;
        if (freeCamListener != null) freeCamListener.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float upDown = 0f;

        if (Input.GetKey(KeyCode.E)) upDown += 1f;
        if (Input.GetKey(KeyCode.Q)) upDown -= 1f;

        Vector3 direction = transform.right * h + transform.forward * v + transform.up * upDown;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandleSpeedChange()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            moveSpeed = Mathf.Min(moveSpeed + speedStep, maxSpeed);
            Debug.Log($"🎥 Freecam speed increased: {moveSpeed}");
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            moveSpeed = Mathf.Max(moveSpeed - speedStep, minSpeed);
            Debug.Log($"🎥 Freecam speed decreased: {moveSpeed}");
        }
    }
}
