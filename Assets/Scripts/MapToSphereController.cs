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
    
    [Header("Multiple Sphere Objects")]
    public GameObject[] sphereObjects;
    private int currentSphereIndex = 0;

    [Header("Camera Settings")]
    public Camera mainCamera;
    public CameraControl cameraControl;

    [Header("Floating Buttons")]
    public GameObject homeButton;
    public GameObject playPauseButton;

    [Header("Audio")]
    public AudioSource backgroundMusic;
    private bool isPlaying = false;

    [Header("Play/Pause Icons")]
    public Sprite playIcon;
    public Sprite pauseIcon;

    [Header("UI Positioning")]
    public float uiDistanceFromCamera = 2f;
    public float uiScale = 0.8f;
    public float floatingButtonDistance = 3f;
    
    private Image homeButtonImage;
    private Button homeButtonComponent;
    private TextMeshProUGUI homeButtonText;
    
    private Image playPauseImage;
    private Button playPauseButtonComponent;
    private TextMeshProUGUI playPauseButtonText;
    
    private Renderer sphereRenderer;
    private int currentMaterialIndex = 0;
    private bool isMapOverlayVisible = true;

    private Vector3 lastCameraPos;
    private Quaternion lastCameraRot;

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        CacheComponents();
        SetupButtonEvents();
    }

    void Start()
    {
        try
        {
            if (sphereObjects == null || sphereObjects.Length == 0)
                SetupTestSpheres();
            
            InitializeApp();
            Invoke("VerifyButtonsSetup", 0.5f);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Scene setup error: " + e.Message);
            SetupFallbackMode();
        }
    }
    
    private void VerifyButtonsSetup()
    {
        if (homeButton != null && homeButtonComponent == null)
        {
            homeButtonComponent = homeButton.GetComponent<Button>();
            if (homeButtonComponent != null)
            {
                homeButtonComponent.onClick.RemoveAllListeners();
                homeButtonComponent.onClick.AddListener(OnHomeButtonClick);
            }
        }
        
        if (buttonPanel != null && buttonPanel.activeInHierarchy)
        {
            Button[] panelButtons = buttonPanel.GetComponentsInChildren<Button>(true);
            
            foreach (Button btn in panelButtons)
            {
                btn.gameObject.SetActive(true);
                btn.interactable = true;
            }
        }
    }

    private void CacheComponents()
    {
        if (homeButton != null)
        {
            homeButtonImage = homeButton.GetComponent<Image>();
            homeButtonComponent = homeButton.GetComponent<Button>();
            homeButtonText = homeButton.GetComponentInChildren<TextMeshProUGUI>();
            
            if (homeButtonText != null)
            {
                homeButtonText.text = "Explore";
            }
        }

        if (playPauseButton != null)
        {
            playPauseImage = playPauseButton.GetComponent<Image>();
            playPauseButtonComponent = playPauseButton.GetComponent<Button>();
            playPauseButtonText = playPauseButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (sphere != null)
        {
            sphereRenderer = sphere.GetComponent<Renderer>();
        }
    }

    private void SetupButtonEvents()
    {
        if (homeButtonComponent != null)
        {
            homeButtonComponent.onClick.RemoveAllListeners();
            homeButtonComponent.onClick.AddListener(OnHomeButtonClick);
        }
        else if (homeButton != null)
        {
            homeButtonComponent = homeButton.GetComponent<Button>();
            if (homeButtonComponent != null)
            {
                homeButtonComponent.onClick.RemoveAllListeners();
                homeButtonComponent.onClick.AddListener(OnHomeButtonClick);
            }
        }

        if (playPauseButtonComponent != null)
        {
            playPauseButtonComponent.onClick.RemoveAllListeners();
            playPauseButtonComponent.onClick.AddListener(OnPlayPauseClick);
        }
        else if (playPauseButton != null)
        {
            playPauseButtonComponent = playPauseButton.GetComponent<Button>();
            if (playPauseButtonComponent != null)
            {
                playPauseButtonComponent.onClick.RemoveAllListeners();
                playPauseButtonComponent.onClick.AddListener(OnPlayPauseClick);
            }
        }
    }

    private void InitializeApp()
    {
        if (sphereObjects != null && sphereObjects.Length > 0)
        {
            for (int i = 0; i < sphereObjects.Length; i++)
            {
                if (sphereObjects[i] != null)
                {
                    SetActive(sphereObjects[i], i == 0);
                }
            }
            currentSphereIndex = 0;
        }
        else if (sphereRenderer != null && sphereMaterials != null && sphereMaterials.Length > 0)
        {
            sphereRenderer.material = sphereMaterials[0];
            currentMaterialIndex = 0;
        }

        ShowMapOverlay();
        UpdatePlayPauseButton();
        SetupFloatingButtons();

        if (cameraControl != null)
            cameraControl.enabled = true;
    }

    private void SetupFloatingButtons()
    {
        SetActive(homeButton, true);
        SetActive(playPauseButton, true);
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
    }

    public void OnButtonClick(int sphereIndex)
    {
        if (sphereObjects == null || sphereObjects.Length == 0)
        {
            SetupTestSpheres();
            
            if (sphereObjects == null || sphereObjects.Length == 0)
            {
                return;
            }
        }
        
        if (sphereIndex < 0 || sphereIndex >= sphereObjects.Length)
        {
            return;
        }
        
        currentSphereIndex = sphereIndex;
        
        UpdateAllSphereVisibility();
        HideMapOverlay();
        
        if (cameraControl != null)
        {
            cameraControl.enabled = true;
        }
    }

    private void UpdateAllSphereVisibility()
    {
        if (sphereObjects == null || sphereObjects.Length == 0)
            return;
        
        if (currentSphereIndex < 0 || currentSphereIndex >= sphereObjects.Length)
        {
            currentSphereIndex = 0;
        }
        
        for (int i = 0; i < sphereObjects.Length; i++)
        {
            if (sphereObjects[i] != null && i != currentSphereIndex)
            {
                sphereObjects[i].SetActive(false);
            }
        }
        
        if (sphereObjects[currentSphereIndex] != null)
        {
            sphereObjects[currentSphereIndex].SetActive(true);
        }
        
        if (sphere != null)
        {
            sphere.SetActive(false);
        }
    }

    public void OnHomeButtonClick()
    {
        isMapOverlayVisible = true;
        
        if (mapPanel != null)
        {
            mapPanel.SetActive(true);
        }
        
        if (buttonPanel != null)
        {
            buttonPanel.SetActive(true);
            
            Button[] buttons = buttonPanel.GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttons)
            {
                btn.gameObject.SetActive(true);
                btn.interactable = true;
            }
        }
        
        if (sphereUI != null)
        {
            sphereUI.SetActive(false);
        }
        
        if (sphereObjects != null && sphereObjects.Length > 0)
        {
            bool anySphereVisible = false;
            
            for (int i = 0; i < sphereObjects.Length; i++)
            {
                if (sphereObjects[i] != null)
                {
                    bool shouldBeActive = (i == currentSphereIndex);
                    sphereObjects[i].SetActive(shouldBeActive);
                    if (shouldBeActive) anySphereVisible = true;
                }
            }
            
            if (!anySphereVisible && sphereObjects.Length > 0 && sphereObjects[0] != null)
            {
                sphereObjects[0].SetActive(true);
                currentSphereIndex = 0;
            }
        }
        else if (sphere != null)
        {
            sphere.SetActive(true);
        }
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

    private bool IsValidMaterialIndex(int index)
    {
        if (sphereMaterials == null || index < 0 || index >= sphereMaterials.Length)
        {
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
        }
    }

    private void PauseAudio()
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.Pause();
            isPlaying = false;
            UpdatePlayPauseButton();
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
        isMapOverlayVisible = true;
        
        SetActive(mapPanel, true);
        SetActive(buttonPanel, true);
        SetActive(sphereUI, false);

        if (sphereObjects != null && currentSphereIndex >= 0 && currentSphereIndex < sphereObjects.Length)
        {
            for (int i = 0; i < sphereObjects.Length; i++)
            {
                if (sphereObjects[i] != null)
                {
                    SetActive(sphereObjects[i], i == currentSphereIndex);
                }
            }
        }
        else if (sphere != null)
        {
            SetActive(sphere, true);
        }
    }

    private void HideMapOverlay()
    {
        isMapOverlayVisible = false;
        
        SetActive(mapPanel, false);
        SetActive(buttonPanel, false);
        SetActive(sphereUI, true);
        
        UpdateAllSphereVisibility();
    }

    private void SetActive(GameObject obj, bool active)
    {
        if (obj != null) 
        {
            obj.SetActive(active);
        }
    }

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
        OnButtonClick(1);
    }

    [ContextMenu("Debug Sphere Toggle System")]
    private void DebugSphereToggleSystem()
    {
        Debug.Log("=== SPHERE TOGGLE SYSTEM DEBUG ===");
        Debug.Log($"Sphere Objects array: {(sphereObjects == null ? "NULL" : $"Length: {sphereObjects.Length}")}");
        
        if (sphereObjects != null)
        {
            Debug.Log($"Current active sphere index: {currentSphereIndex}");
            
            for (int i = 0; i < sphereObjects.Length; i++)
            {
                GameObject obj = sphereObjects[i];
                string status = obj == null ? "NULL" : (obj.activeInHierarchy ? "ACTIVE" : "inactive");
                Debug.Log($"Sphere [{i}]: {(obj == null ? "NULL" : obj.name)} - {status}");
                
                if (obj != null)
                {
                    Renderer r = obj.GetComponent<Renderer>();
                    if (r != null)
                    {
                        Debug.Log($"  Material: {r.material?.name ?? "NULL"}");
                    }
                }
            }
        }
        
        Debug.Log("============================");
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
    
    [ContextMenu("Setup Test Spheres")]
    private void SetupTestSpheres()
    {
        if (sphere == null || sphereMaterials == null || sphereMaterials.Length == 0)
        {
            return;
        }
        
        sphereObjects = new GameObject[sphereMaterials.Length];
        
        for (int i = 0; i < sphereMaterials.Length; i++)
        {
            if (sphereObjects[i] != null)
            {
                DestroyImmediate(sphereObjects[i]);
            }
            
            sphereObjects[i] = Instantiate(sphere, sphere.transform.position, sphere.transform.rotation);
            sphereObjects[i].name = $"Sphere_{i}";
            
            sphereObjects[i].transform.parent = sphere.transform.parent;
            
            Renderer r = sphereObjects[i].GetComponent<Renderer>();
            if (r != null && sphereMaterials[i] != null)
            {
                r.sharedMaterial = sphereMaterials[i];
            }
            
            sphereObjects[i].SetActive(i == 0);
        }
        
        currentSphereIndex = 0;
        sphere.SetActive(false);
    }

    [ContextMenu("Emergency Setup")]
    private void EmergencySetup()
    {
        SetupTestSpheres();
        
        if (homeButton != null)
        {
            homeButtonComponent = homeButton.GetComponent<Button>();
            if (homeButtonComponent != null)
            {
                homeButtonComponent.onClick.RemoveAllListeners();
                homeButtonComponent.onClick.AddListener(OnHomeButtonClick);
            }
        }
        
        if (mapPanel != null) mapPanel.SetActive(true);
        if (buttonPanel != null) buttonPanel.SetActive(true);
        if (sphereUI != null) sphereUI.SetActive(false);
        
        if (buttonPanel != null)
        {
            Button[] buttons = buttonPanel.GetComponentsInChildren<Button>(true);
            
            for (int i = 0; i < buttons.Length; i++)
            {
                Button btn = buttons[i];
                if (btn != null)
                {
                    btn.gameObject.SetActive(true);
                    btn.interactable = true;
                    
                    btn.name = $"Button_{i}";
                    
                    btn.onClick.RemoveAllListeners();
                    
                    int index = i;
                    btn.onClick.AddListener(() => OnButtonClick(index));
                }
            }
        }
        
        if (sphereObjects != null && sphereObjects.Length > 0)
        {
            for (int i = 0; i < sphereObjects.Length; i++)
            {
                if (sphereObjects[i] != null)
                {
                    sphereObjects[i].SetActive(i == 0);
                }
            }
            currentSphereIndex = 0;
        }
    }
}