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
    public GameObject[] sphereObjects; // Assign 11 sphere objects di Inspector, masing-masing dengan material berbeda
    private int currentSphereIndex = 0;

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
            // Auto-setup sphere objects jika belum ada
            if (sphereObjects == null || sphereObjects.Length == 0)
            {
                Debug.Log("Auto-creating sphere objects from materials...");
                SetupTestSpheres();
            }
            
            InitializeApp();
            
            // Pastikan home button berfungsi
            Invoke("VerifyButtonsSetup", 0.5f);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Scene setup error: " + e.Message);
            SetupFallbackMode();
        }
    }
    
    // Fungsi untuk memastikan button events terhubung dengan benar
    private void VerifyButtonsSetup()
    {
        Debug.Log("Verifying button setup...");
        
        // Pastikan home button terhubung
        if (homeButton != null && homeButtonComponent == null)
        {
            homeButtonComponent = homeButton.GetComponent<Button>();
            if (homeButtonComponent != null)
            {
                homeButtonComponent.onClick.RemoveAllListeners();
                homeButtonComponent.onClick.AddListener(OnHomeButtonClick);
                Debug.Log("Home button connected via verification");
            }
        }
        
        // Pastikan juga button-button di button panel terhubung dengan benar
        if (buttonPanel != null && buttonPanel.activeInHierarchy)
        {
            Button[] panelButtons = buttonPanel.GetComponentsInChildren<Button>(true);
            Debug.Log($"Found {panelButtons.Length} buttons in panel");
            
            // Aktifkan semua button
            foreach (Button btn in panelButtons)
            {
                btn.gameObject.SetActive(true);
                btn.interactable = true;
            }
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
        Debug.Log("=== Setting up button events ===");
        
        // Setup home button click event
        if (homeButtonComponent != null)
        {
            homeButtonComponent.onClick.RemoveAllListeners();
            homeButtonComponent.onClick.AddListener(OnHomeButtonClick);
            Debug.Log("Home button click event registered successfully");
        }
        else if (homeButton != null)
        {
            // Fallback jika component belum di-cache
            homeButtonComponent = homeButton.GetComponent<Button>();
            if (homeButtonComponent != null)
            {
                homeButtonComponent.onClick.RemoveAllListeners();
                homeButtonComponent.onClick.AddListener(OnHomeButtonClick);
                Debug.Log("Home button click event registered via fallback");
            }
            else
            {
                Debug.LogError("CRITICAL: Home button doesn't have Button component!");
            }
        }
        else
        {
            Debug.LogError("CRITICAL: Home button GameObject is NULL!");
        }

        // Setup play/pause button click event
        if (playPauseButtonComponent != null)
        {
            playPauseButtonComponent.onClick.RemoveAllListeners();
            playPauseButtonComponent.onClick.AddListener(OnPlayPauseClick);
            Debug.Log("Play/Pause button click event registered");
        }
        else if (playPauseButton != null)
        {
            playPauseButtonComponent = playPauseButton.GetComponent<Button>();
            if (playPauseButtonComponent != null)
            {
                playPauseButtonComponent.onClick.RemoveAllListeners();
                playPauseButtonComponent.onClick.AddListener(OnPlayPauseClick);
                Debug.Log("Play/Pause button click event registered via fallback");
            }
        }
        
        Debug.Log("==============================");
    }

    private void InitializeApp()
    {
        Debug.Log("=== Initializing App ===");
        
        // Inisialisasi dengan pendekatan toggle sphere objects
        if (sphereObjects != null && sphereObjects.Length > 0)
        {
            // Sembunyikan semua sphere kecuali yang pertama
            for (int i = 0; i < sphereObjects.Length; i++)
            {
                if (sphereObjects[i] != null)
                {
                    SetActive(sphereObjects[i], i == 0); // Aktifkan hanya sphere pertama
                }
            }
            currentSphereIndex = 0;
            Debug.Log($"Initial sphere set to index 0");
            
            // Tetap support material legacy jika diperlukan
            if (sphereRenderer != null && sphereMaterials != null && sphereMaterials.Length > 0)
            {
                Debug.Log($"Legacy material system also initialized");
            }
        }
        else if (sphereRenderer != null && sphereMaterials != null && sphereMaterials.Length > 0)
        {
            // Fallback ke sistem material lama jika tidak ada sphereObjects
            Debug.Log($"Using legacy material system: {sphereMaterials[0]?.name ?? "NULL"}");
            sphereRenderer.material = sphereMaterials[0];
            currentMaterialIndex = 0;
            Debug.Log($"Initial material set to: {sphereRenderer.material?.name}");
        }
        else
        {
            Debug.LogError("Cannot initialize any sphere system - missing references!");
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
    public void OnButtonClick(int sphereIndex)
    {
        Debug.Log($"=== OnButtonClick called with index: {sphereIndex} ===");
        
        // Periksa array sphereObjects
        if (sphereObjects == null || sphereObjects.Length == 0)
        {
            Debug.LogError("Sphere objects array is empty. Setting up spheres now...");
            SetupTestSpheres();
            
            // Periksa lagi setelah setup
            if (sphereObjects == null || sphereObjects.Length == 0)
            {
                Debug.LogError("Failed to create sphere objects. Cannot continue.");
                return;
            }
        }
        
        // Validasi index
        if (sphereIndex < 0 || sphereIndex >= sphereObjects.Length)
        {
            Debug.LogError($"Invalid sphere index: {sphereIndex}. Valid range: 0-{sphereObjects.Length-1}");
            return;
        }
        
        // Cetak status sphere objects untuk debugging
        Debug.Log("Current state before switch:");
        for (int i = 0; i < Mathf.Min(sphereObjects.Length, 3); i++)
        {
            if (sphereObjects[i] != null)
            {
                Debug.Log($"Sphere[{i}]: {sphereObjects[i].name} - Active: {sphereObjects[i].activeInHierarchy}");
            }
        }
        
        // Sembunyikan SEMUA sphere
        for (int i = 0; i < sphereObjects.Length; i++)
        {
            if (sphereObjects[i] != null)
            {
                sphereObjects[i].SetActive(false);
            }
        }
        
        // Aktifkan sphere yang dipilih
        if (sphereObjects[sphereIndex] != null)
        {
            Debug.Log($"Activating sphere {sphereIndex}: {sphereObjects[sphereIndex].name}");
            sphereObjects[sphereIndex].SetActive(true);
            currentSphereIndex = sphereIndex;
            
            // Sembunyikan UI map
            HideMapOverlay();
            
            // Pastikan camera control aktif
            if (cameraControl != null)
            {
                cameraControl.enabled = true;
                Debug.Log("Camera control activated");
            }
            
            Debug.Log($"Successfully switched to sphere {sphereIndex}");
        }
        else
        {
            Debug.LogError($"ERROR: Sphere at index {sphereIndex} is null!");
        }
        
        Debug.Log("=======================================");
    }

    public void OnHomeButtonClick()
    {
        Debug.Log("=== Home button clicked - Showing map and navigation buttons ===");
        
        // Pastikan UI map dan button terlihat
        isMapOverlayVisible = true;
        
        // Aktifkan map panel
        if (mapPanel != null)
        {
            mapPanel.SetActive(true);
            Debug.Log("Map panel activated");
        }
        else
        {
            Debug.LogError("Map panel is NULL!");
        }
        
        // Aktifkan button panel dan semua button di dalamnya
        if (buttonPanel != null)
        {
            buttonPanel.SetActive(true);
            Debug.Log("Button panel activated");
            
            // Aktifkan semua button di panel
            Button[] buttons = buttonPanel.GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttons)
            {
                btn.gameObject.SetActive(true);
                btn.interactable = true;
            }
            Debug.Log($"Activated {buttons.Length} buttons in button panel");
        }
        else
        {
            Debug.LogError("Button panel is NULL! Cannot show navigation buttons.");
        }
        
        // Sembunyikan sphereUI
        if (sphereUI != null)
        {
            sphereUI.SetActive(false);
            Debug.Log("Sphere UI hidden");
        }
        
        // Pastikan sphere yang aktif tetap terlihat sebagai background
        if (sphereObjects != null && sphereObjects.Length > 0)
        {
            bool anySphereVisible = false;
            
            // Hanya tampilkan sphere yang sedang aktif
            for (int i = 0; i < sphereObjects.Length; i++)
            {
                if (sphereObjects[i] != null)
                {
                    bool shouldBeActive = (i == currentSphereIndex);
                    sphereObjects[i].SetActive(shouldBeActive);
                    if (shouldBeActive) anySphereVisible = true;
                }
            }
            
            // Jika tidak ada sphere yang aktif, aktifkan yang pertama
            if (!anySphereVisible && sphereObjects.Length > 0 && sphereObjects[0] != null)
            {
                sphereObjects[0].SetActive(true);
                currentSphereIndex = 0;
                Debug.Log("No active sphere found, activating first sphere");
            }
        }
        else if (sphere != null)
        {
            // Fallback ke sphere tunggal jika tidak ada array
            sphere.SetActive(true);
            Debug.Log("Using single sphere as fallback");
        }
        
        Debug.Log("Home button click handled successfully");
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

        // Keep sphere visible (old or new system)
        if (sphereObjects != null && currentSphereIndex >= 0 && currentSphereIndex < sphereObjects.Length)
        {
            // New toggle system - keep current sphere visible as background
            for (int i = 0; i < sphereObjects.Length; i++)
            {
                if (sphereObjects[i] != null)
                {
                    // Hanya aktifkan sphere yang sedang aktif
                    SetActive(sphereObjects[i], i == currentSphereIndex);
                }
            }
        }
        else if (sphere != null)
        {
            // Legacy system - keep single sphere visible
            SetActive(sphere, true);
        }
        
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
        // Fungsi untuk setup sphere objects dari sphere utama
        if (sphere == null)
        {
            Debug.LogError("Cannot setup test spheres - main sphere is NULL!");
            return;
        }
        
        if (sphereMaterials == null || sphereMaterials.Length == 0)
        {
            Debug.LogError("Cannot setup test spheres - materials array is empty or NULL!");
            return;
        }
        
        Debug.Log("=== Creating test spheres from main sphere... ===");
        
        // Buat array dengan ukuran yang sesuai
        sphereObjects = new GameObject[sphereMaterials.Length];
        
        // Buat duplicate sphere untuk setiap material
        for (int i = 0; i < sphereMaterials.Length; i++)
        {
            // Hapus sphere lama jika ada
            if (sphereObjects[i] != null)
            {
                DestroyImmediate(sphereObjects[i]);
            }
            
            // Buat clone baru
            sphereObjects[i] = Instantiate(sphere, sphere.transform.position, sphere.transform.rotation);
            sphereObjects[i].name = $"Sphere_{i}";
            
            // Atur parent sama dengan sphere utama
            sphereObjects[i].transform.parent = sphere.transform.parent;
            
            // Set material
            Renderer r = sphereObjects[i].GetComponent<Renderer>();
            if (r != null && sphereMaterials[i] != null)
            {
                r.sharedMaterial = sphereMaterials[i];
                Debug.Log($"Sphere_{i} created with material: {sphereMaterials[i].name}");
            }
            else
            {
                Debug.LogWarning($"Issues with Sphere_{i} - Renderer: {r != null}, Material: {sphereMaterials[i] != null}");
            }
            
            // Sembunyikan semua sphere kecuali yang pertama
            sphereObjects[i].SetActive(i == 0);
        }
        
        // Set sphere indeks pertama sebagai aktif
        currentSphereIndex = 0;
        
        // Sembunyikan sphere utama
        sphere.SetActive(false);
        
        Debug.Log($"Created {sphereObjects.Length} test spheres successfully");
        Debug.Log("========================================");
    }

    [ContextMenu("Emergency Setup")]
    private void EmergencySetup()
    {
        Debug.Log("=== EMERGENCY SETUP RUNNING ===");
        
        // 1. Pastikan sphere objects ada
        SetupTestSpheres();
        
        // 2. Pastikan button events terhubung
        if (homeButton != null)
        {
            homeButtonComponent = homeButton.GetComponent<Button>();
            if (homeButtonComponent != null)
            {
                homeButtonComponent.onClick.RemoveAllListeners();
                homeButtonComponent.onClick.AddListener(OnHomeButtonClick);
                Debug.Log("Home button connected via emergency setup");
            }
        }
        
        // 3. Aktifkan UI map dan button
        if (mapPanel != null) mapPanel.SetActive(true);
        if (buttonPanel != null) buttonPanel.SetActive(true);
        if (sphereUI != null) sphereUI.SetActive(false);
        
        // 4. Pastikan semua button di button panel terhubung ke OnButtonClick
        if (buttonPanel != null)
        {
            Button[] buttons = buttonPanel.GetComponentsInChildren<Button>(true);
            Debug.Log($"Found {buttons.Length} buttons in panel");
            
            // Aktifkan semua button
            for (int i = 0; i < buttons.Length; i++)
            {
                Button btn = buttons[i];
                if (btn != null)
                {
                    btn.gameObject.SetActive(true);
                    btn.interactable = true;
                    
                    // Tambahkan index ke nama button untuk identifikasi mudah
                    btn.name = $"Button_{i}";
                    
                    // Hapus listener lama dan tambahkan yang baru dengan index yang benar
                    btn.onClick.RemoveAllListeners();
                    
                    int index = i; // Capture index dalam closure
                    btn.onClick.AddListener(() => OnButtonClick(index));
                    
                    Debug.Log($"Button {i} setup with OnButtonClick({i})");
                }
            }
        }
        
        // 5. Aktifkan sphere pertama
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
        
        // Pastikan image icon home juga terhubung
        if (homeButtonImage != null && homeButtonImage.gameObject != homeButton)
        {
            // Dapatkan atau tambahkan Button component
            Button imgButton = homeButtonImage.gameObject.GetComponent<Button>();
            if (imgButton == null)
            {
                imgButton = homeButtonImage.gameObject.AddComponent<Button>();
            }
            
            // Setup onClick event
            imgButton.onClick.RemoveAllListeners();
            imgButton.onClick.AddListener(OnHomeButtonClick);
            
            Debug.Log("Home icon image connected via EmergencySetup");
        }
        
        Debug.Log("Emergency setup complete - UI should now be functional");
        Debug.Log("====================================");
    }
}