using UnityEngine;

public enum ScreenPosition
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Center,
    Custom
}

public class IconFollowScreenCorner : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera cam;
    public float distanceFromCamera = 5f;
    
    [Header("Position Settings")]
    public ScreenPosition screenPosition = ScreenPosition.TopRight;
    [Range(0f, 1f)] public float customX = 0.9f; // For custom position
    [Range(0f, 1f)] public float customY = 0.9f; // For custom position
    public Vector2 offset = Vector2.zero; // Fine-tuning offset
    
    [Header("Behavior Settings")]
    public bool faceCamera = true;
    public bool rotateY180 = true; // Face camera properly
    
    [Header("Performance Settings")]
    public bool forceUpdateEveryFrame = false;
    public float updateInterval = 0.1f;
    public float movementThreshold = 0.01f;
    public float rotationThreshold = 0.1f;
    
    // Performance tracking
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;
    private float lastUpdateTime;
    private bool isInitialized = false;

    void Start()
    {
        InitializeComponent();
    }

    void Update()
    {
        if (!isInitialized) return;
        
        if (forceUpdateEveryFrame || ShouldUpdate())
        {
            UpdateIconPosition();
            UpdatePerformanceTracking();
        }
    }

    private void InitializeComponent()
    {
        // Auto-assign camera if not set
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
            {
                Debug.LogWarning($"No camera found for {gameObject.name}!");
                return;
            }
        }
        
        UpdateIconPosition();
        UpdatePerformanceTracking();
        isInitialized = true;
        
        Debug.Log($"IconFollowScreenCorner initialized for {gameObject.name} at {screenPosition}");
    }

    private bool ShouldUpdate()
    {
        // Time-based update
        if (Time.time - lastUpdateTime > updateInterval)
            return true;
            
        // Movement-based update
        if (HasCameraMoved())
            return true;
            
        return false;
    }

    private bool HasCameraMoved()
    {
        if (cam == null) return false;
        
        bool positionChanged = Vector3.Distance(cam.transform.position, lastCameraPosition) > movementThreshold;
        bool rotationChanged = Quaternion.Angle(cam.transform.rotation, lastCameraRotation) > rotationThreshold;
        
        return positionChanged || rotationChanged;
    }

    private void UpdateIconPosition()
    {
        if (cam == null) return;
        
        // Get viewport position based on screen position setting
        Vector3 viewportPos = GetViewportPosition();
        viewportPos.z = distanceFromCamera;
        
        // Convert to world space
        Vector3 worldPos = cam.ViewportToWorldPoint(viewportPos);
        transform.position = worldPos;
        
        // Face camera if enabled
        if (faceCamera)
        {
            transform.LookAt(cam.transform);
            if (rotateY180)
            {
                transform.Rotate(0, 180, 0);
            }
        }
    }

    private Vector3 GetViewportPosition()
    {
        Vector3 viewportPos;
        
        switch (screenPosition)
        {
            case ScreenPosition.TopLeft:
                viewportPos = new Vector3(0.1f + offset.x, 0.9f + offset.y, 0);
                break;
            case ScreenPosition.TopRight:
                viewportPos = new Vector3(0.9f + offset.x, 0.9f + offset.y, 0);
                break;
            case ScreenPosition.BottomLeft:
                viewportPos = new Vector3(0.1f + offset.x, 0.1f + offset.y, 0);
                break;
            case ScreenPosition.BottomRight:
                viewportPos = new Vector3(0.9f + offset.x, 0.1f + offset.y, 0);
                break;
            case ScreenPosition.Center:
                viewportPos = new Vector3(0.5f + offset.x, 0.5f + offset.y, 0);
                break;
            case ScreenPosition.Custom:
                viewportPos = new Vector3(customX + offset.x, customY + offset.y, 0);
                break;
            default:
                viewportPos = new Vector3(0.9f, 0.9f, 0);
                break;
        }
        
        // Clamp to valid viewport range
        viewportPos.x = Mathf.Clamp01(viewportPos.x);
        viewportPos.y = Mathf.Clamp01(viewportPos.y);
        
        return viewportPos;
    }

    private void UpdatePerformanceTracking()
    {
        if (cam != null)
        {
            lastCameraPosition = cam.transform.position;
            lastCameraRotation = cam.transform.rotation;
            lastUpdateTime = Time.time;
        }
    }

    // Public methods for runtime control
    public void SetScreenPosition(ScreenPosition newPosition)
    {
        screenPosition = newPosition;
        UpdateIconPosition();
    }

    public void SetCustomPosition(float x, float y)
    {
        screenPosition = ScreenPosition.Custom;
        customX = Mathf.Clamp01(x);
        customY = Mathf.Clamp01(y);
        UpdateIconPosition();
    }

    public void SetCamera(Camera newCamera)
    {
        cam = newCamera;
        if (isInitialized)
        {
            UpdateIconPosition();
            UpdatePerformanceTracking();
        }
    }

    // Debug methods
    [ContextMenu("Update Position Now")]
    private void ForceUpdatePosition()
    {
        UpdateIconPosition();
    }

    [ContextMenu("Reset to Default")]
    private void ResetToDefault()
    {
        screenPosition = ScreenPosition.TopRight;
        distanceFromCamera = 5f;
        offset = Vector2.zero;
        UpdateIconPosition();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-update in editor when values change
        if (Application.isPlaying && isInitialized)
        {
            UpdateIconPosition();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Show camera reference in editor
        if (cam != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
            Gizmos.DrawLine(transform.position, cam.transform.position);
        }
    }
#endif
}