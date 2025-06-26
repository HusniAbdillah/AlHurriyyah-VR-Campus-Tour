using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapToSphereController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mapPanel;
    public GameObject buttonPanel;
    public GameObject sphereUI;

    [Header("Sphere Settings")]
    public GameObject sphere;
    public Material[] sphereMaterials;

    [Header("Camera Settings")]
    public Camera mainCamera;
    public CameraControl cameraControl;

    [Header("Floating Buttons")]
    public GameObject homeButton;        // GameObject containing Image + Button + TMP
    public GameObject playPauseButton;   // GameObject containing Image + Button + TMP

    [Header("Audio")]
    public AudioSource backgroundMusic;
    private bool isPlaying = false;

    [Header("Play/Pause Icons")]
    public Sprite playIcon;
    public Sprite pauseIcon;

    [Header("UI Positioning")]
    public float uiDistanceFromCamera = 2f;
    public float uiScale = 0.8f; // 80% screen coverage
    public float floatingButtonDistance = 3f; // Distance for floating buttons
    
    // Cached components untuk performance
    private Image homeButtonImage;
    private Button homeButtonComponent;
    private TextMeshProUGUI homeButtonText;
    
    private Image playPauseImage;
    private Button playPauseButtonComponent;
    private TextMeshProUGUI playPauseButtonText;
    
    private Renderer sphereRenderer;
    private int currentMaterialIndex = 0;
    private bool isMapOverlayVisible = true; // Track overlay visibility

    // Add performance tracking
    private Vector3 lastCameraPos;
    private Quaternion lastCameraRot;

    void Awake()
    {
        // Auto-assign camera if null
        if (mainCamera == null)
            mainCamera = Camera.main;

        CacheComponents();
        SetupButtonEvents();
    }

    void Start()
    {
        try
        {
            InitializeApp();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Scene setup error: " + e.Message);
            SetupFallbackMode();
        }
    }

    void Update()
    {
        // Tidak perlu update posisi UI, biarkan diatur oleh RectTransform di Canvas
    }

    private void CacheComponents()
    {
        Debug.Log("=== Caching Components ===");
        
        // Cache home button components
        if (homeButton != null)
        {
            homeButtonImage = homeButton.GetComponent<Image>();
            homeButtonComponent = homeButton.GetComponent<Button>();
            homeButtonText = homeButton.GetComponentInChildren<TextMeshProUGUI>();
            
            // Set home button text
            if (homeButtonText != null)
            {
                homeButtonText.text = "Explore";
            }
            Debug.Log($"Home button cached: Image={homeButtonImage != null}, Button={homeButtonComponent != null}, Text={homeButtonText != null}");
        }
        else
        {
            Debug.LogError("HomeButton is NULL!");
        }

        // Cache play/pause button components
        if (playPauseButton != null)
        {
            playPauseImage = playPauseButton.GetComponent<Image>();
            playPauseButtonComponent = playPauseButton.GetComponent<Button>();
            playPauseButtonText = playPauseButton.GetComponentInChildren<TextMeshProUGUI>();
            Debug.Log($"PlayPause button cached: Image={playPauseImage != null}, Button={playPauseButtonComponent != null}, Text={playPauseButtonText != null}");
        }
        else
        {
            Debug.LogError("PlayPauseButton is NULL!");
        }

        // Cache sphere renderer - CRITICAL
        if (sphere != null)
        {
            sphereRenderer = sphere.GetComponent<Renderer>();
            Debug.Log($"Sphere renderer cached: {sphereRenderer != null}");
            
            if (sphereRenderer != null)
            {
                Debug.Log($"Initial sphere material: {sphereRenderer.material?.name ?? "NULL"}");
            }
            else
            {
                Debug.LogError("Sphere object doesn't have Renderer component!");
            }
        }
        else
        {
            Debug.LogError("Sphere GameObject is NULL!");
        }
        
        Debug.Log("========================");
    }

    private void SetupButtonEvents()
    {
        // Setup home button click event
        if (homeButtonComponent != null)
        {
            homeButtonComponent.onClick.RemoveAllListeners();
            homeButtonComponent.onClick.AddListener(OnHomeButtonClick);
        }

        // Setup play/pause button click event
        if (playPauseButtonComponent != null)
        {
            playPauseButtonComponent.onClick.RemoveAllListeners();
            playPauseButtonComponent.onClick.AddListener(OnPlayPauseClick);
        }
    }

    private void InitializeApp()
    {
        Debug.Log("=== Initializing App ===");
        
        // Set initial material (material 0)
        if (sphereRenderer != null && sphereMaterials != null && sphereMaterials.Length > 0)
        {
            Debug.Log($"Setting initial material: {sphereMaterials[0]?.name ?? "NULL"}");
            sphereRenderer.material = sphereMaterials[0];
            currentMaterialIndex = 0;
            Debug.Log($"Initial material set to: {sphereRenderer.material?.name}");
        }
        else
        {
            Debug.LogError("Cannot set initial material - missing references!");
        }

        // Show map overlay initially
        ShowMapOverlay();
        
        // Initialize audio button state
        UpdatePlayPauseButton();
        
        // Setup floating buttons to always be visible
        SetupFloatingButtons();

        // Enable camera control
        if (cameraControl != null)
            cameraControl.enabled = true;
        
        Debug.Log("=====================");
    }

    private void SetupFloatingButtons()
    {
        // Both floating buttons always visible
        SetActive(homeButton, true);
        SetActive(playPauseButton, true);
        // Tidak perlu atur posisi, biarkan diatur RectTransform di Canvas
    }



    private bool HasCameraMoved()
    {
        bool moved = Vector3.Distance(mainCamera.transform.position, lastCameraPos) > 0.01f ||
                     Quaternion.Angle(mainCamera.transform.rotation, lastCameraRot) > 0.1f;
        
        if (moved)
        {
            lastCameraPos = mainCamera.transform.position;
            lastCameraRot = mainCamera.transform.rotation;
        }
        
        return moved;
    }



    private void SetupFallbackMode()
    {
        if (mapPanel != null) mapPanel.SetActive(true);
        Debug.Log("Running in fallback mode - some features may not work");
    }

    // Public methods untuk UI callbacks dari 11 button
    public void OnButtonClick(int materialIndex)
    {
        Debug.Log($"OnButtonClick called with index: {materialIndex}");
        
        if (!IsValidMaterialIndex(materialIndex)) 
        {
            Debug.LogError($"Invalid material index: {materialIndex}");
            return;
        }

        // Debug sebelum perubahan
        Debug.Log($"Before change - Current material: {sphereRenderer.material?.name}");

        if (sphereRenderer != null && sphereMaterials != null && materialIndex < sphereMaterials.Length && sphereMaterials[materialIndex] != null)
        {
            // Change sphere material
            sphereRenderer.material = sphereMaterials[materialIndex];
            currentMaterialIndex = materialIndex;
            
            // Verifikasi material berdasarkan nama, bukan referensi objek
            Debug.Log($"After change - Current material: {sphereRenderer.material?.name}");
            
            // Hide map overlay dan switch ke sphere mode
            HideMapOverlay();
            
            // Pastikan camera control tetap aktif
            if (cameraControl != null)
            {
                cameraControl.enabled = true;
                Debug.Log("Camera control re-enabled after material change");
            }
        }
        else
        {
            Debug.LogError($"Cannot change material - Missing references or null material");
        }
    }

    public void OnHomeButtonClick()
    {
        // Always show map overlay when home is clicked
        ShowMapOverlay();
        Debug.Log("Home button clicked - Map overlay shown");
    }

    public void OnPlayPauseClick()
    {
        if (backgroundMusic == null) return;

        if (isPlaying)
        {
            PauseAudio();
        }
        else
        {
            PlayAudio();
        }
    }

    // Helper methods
    private bool IsValidMaterialIndex(int index)
    {
        if (sphereMaterials == null || index < 0 || index >= sphereMaterials.Length)
        {
            Debug.LogWarning($"Material index {index} out of range! Available: 0-{sphereMaterials?.Length - 1 ?? 0}");
            return false;
        }
        return true;
    }

    private void PlayAudio()
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.Play();
            isPlaying = true;
            UpdatePlayPauseButton();
            Debug.Log("Audio started playing");
        }
    }

    private void PauseAudio()
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.Pause();
            isPlaying = false;
            UpdatePlayPauseButton();
            Debug.Log("Audio paused");
        }
    }

    private void UpdatePlayPauseButton()
    {
        if (playPauseImage != null)
        {
            playPauseImage.sprite = isPlaying ? pauseIcon : playIcon;
        }
        
        if (playPauseButtonText != null)
        {
            playPauseButtonText.text = isPlaying ? "Pause" : "Play";
        }
    }

    private void ShowMapOverlay()
    {
        Debug.Log("ShowMapOverlay called");
        
        isMapOverlayVisible = true;
        
        // Show map UI overlay (80% screen coverage, always in front of camera)
        SetActive(mapPanel, true);
        SetActive(buttonPanel, true);
        SetActive(sphereUI, false);

        // Keep sphere visible with current material as background
        SetActive(sphere, true);

        // Tidak perlu update posisi, biarkan diatur RectTransform di Canvas
        
        Debug.Log($"Map overlay shown - mapPanel: {mapPanel?.activeInHierarchy}, buttonPanel: {buttonPanel?.activeInHierarchy}");
    }

    private void HideMapOverlay()
    {
        Debug.Log("HideMapOverlay called");
        
        isMapOverlayVisible = false;
        
        // Hide map UI overlay
        SetActive(mapPanel, false);
        SetActive(buttonPanel, false);
        SetActive(sphereUI, true);

        // Show sphere with current material (full view)
        SetActive(sphere, true);
        
        Debug.Log("Map overlay hidden - full sphere view");
    }

    private void SetActive(GameObject obj, bool active)
    {
        if (obj != null) 
        {
            bool wasActive = obj.activeInHierarchy;
            obj.SetActive(active);
            Debug.Log($"SetActive: {obj.name} changed from {wasActive} to {active}");
        }
        else
        {
            Debug.LogWarning($"SetActive called on NULL object - trying to set active = {active}");
        }
    }

    // Debug methods
    [ContextMenu("Toggle Map Overlay")]
    private void DebugToggleMapOverlay()
    {
        OnHomeButtonClick();
    }

    [ContextMenu("Test Material Switching")]
    private void TestMaterialSwitching()
    {
        if (sphereMaterials != null && sphereMaterials.Length > 0)
        {
            int nextIndex = (currentMaterialIndex + 1) % sphereMaterials.Length;
            OnButtonClick(nextIndex);
            Debug.Log($"Switched to material {nextIndex}");
        }
    }

    [ContextMenu("Show Current State")]
    private void ShowCurrentState()
    {
        Debug.Log($"Current Material Index: {currentMaterialIndex}");
        Debug.Log($"Map Overlay Visible: {isMapOverlayVisible}");
        Debug.Log($"Audio Playing: {isPlaying}");
    }

    [ContextMenu("Debug Material System")]
    private void DebugMaterialSystem()
    {
        Debug.Log("=== MATERIAL SYSTEM DEBUG ===");
        Debug.Log($"Sphere object: {(sphere == null ? "NULL" : sphere.name)}");
        Debug.Log($"Sphere active: {(sphere == null ? "NULL" : sphere.activeInHierarchy.ToString())}");
        Debug.Log($"SphereRenderer: {(sphereRenderer == null ? "NULL" : "Found")}");
        
        if (sphereRenderer != null)
        {
            Debug.Log($"Current material: {(sphereRenderer.material == null ? "NULL" : sphereRenderer.material.name)}");
            Debug.Log($"Renderer enabled: {sphereRenderer.enabled}");
        }
        
        Debug.Log($"Materials array: {(sphereMaterials == null ? "NULL" : $"Length {sphereMaterials.Length}")}");
        
        if (sphereMaterials != null)
        {
            for (int i = 0; i < sphereMaterials.Length; i++)
            {
                Debug.Log($"Material {i}: {(sphereMaterials[i] == null ? "NULL" : sphereMaterials[i].name)}");
            }
        }
        
        Debug.Log($"Current Material Index: {currentMaterialIndex}");
        Debug.Log("========================");
    }

    [ContextMenu("Force Material Change")]
    private void ForceMaterialChange()
    {
        Debug.Log("Force changing material to index 1...");
        OnButtonClick(1);
    }

    [ContextMenu("Debug UI State")]
    private void DebugUIState()
    {
        Debug.Log("=== UI STATE DEBUG ===");
        Debug.Log($"isMapOverlayVisible: {isMapOverlayVisible}");
        Debug.Log($"mapPanel: {(mapPanel == null ? "NULL" : $"Active: {mapPanel.activeInHierarchy}")}");
        Debug.Log($"buttonPanel: {(buttonPanel == null ? "NULL" : $"Active: {buttonPanel.activeInHierarchy}")}");
        Debug.Log($"sphere: {(sphere == null ? "NULL" : $"Active: {sphere.activeInHierarchy}")}");
        Debug.Log("=====================");
    }
}