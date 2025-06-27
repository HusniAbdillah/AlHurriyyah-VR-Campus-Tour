using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapToSphereController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mapPanel;
    public GameObject buttonPanel;
    public GameObject sphereUI;
    
    [Header("Narration System")]
    public GameObject narationPanel;
    public Button narationButton;
    private bool isNarationVisible = false;

    [Header("Exit Button Settings")]
    public GameObject exitButton;
    public float exitButtonX = -20f;
    public float exitButtonY = -20f;
    public Vector2 exitButtonSize = new Vector2(80, 40);
    private Button exitButtonComponent;

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

        try
        {
            CacheComponents();
            SetupButtonEvents();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in Awake: " + e.Message);
            Invoke("EmergencySetup", 0.1f);
        }
    }

    void Start()
    {
        try
        {
            if (sphereObjects == null || sphereObjects.Length == 0)
                SetupTestSpheres();
            
            // Ubah alur inisialisasi
            SetupBasicComponents();      // Setup komponen dasar tanpa mengubah UI visibility
            Invoke("VerifyButtonsSetup", 0.5f);
            
            // Setup floating buttons (narration and exit)
            SetupFloatingButtons();
            
            // Update posisi tombol Exit setelah semua setup
            Invoke("UpdateExitButtonPosition", 0.2f);
            
            // Tampilkan narasi di AKHIR process start
            Invoke("ShowNaration", 0.3f); // Delay sedikit untuk memastikan semua setup selesai
        }
        catch (System.Exception e)
        {
            Debug.LogError("Scene setup error: " + e.Message);
            SetupFallbackMode();
        }
    }
    
    private void VerifyButtonsSetup()
    {
        bool allButtonsSetup = true;

        // Check if exit button is setup
        if (exitButtonComponent != null && exitButtonComponent.onClick.GetPersistentEventCount() == 0)
        {
            Debug.LogWarning("Exit button event is not set up correctly!");
            allButtonsSetup = false;
        }

        // Check if narration button is setup
        if (narationButton != null && narationButton.onClick.GetPersistentEventCount() == 0)
        {
            Debug.LogWarning("Narration button event is not set up correctly!");
            allButtonsSetup = false;
        }
        
        if (homeButton != null && homeButtonComponent == null)
        {
            homeButtonComponent = homeButton.GetComponent<Button>();
            if (homeButtonComponent != null)
            {
                homeButtonComponent.onClick.RemoveAllListeners();
                homeButtonComponent.onClick.AddListener(OnHomeButtonClick);
            }
        }
        
        // Verifikasi narasi button
        if (narationButton != null)
        {
            narationButton.onClick.RemoveAllListeners();
            narationButton.onClick.AddListener(ToggleNaration);
        }
        
        // Verifikasi exit button
        if (exitButton != null)
        {
            if (exitButton.GetComponent<Button>() == null)
            {
                exitButtonComponent = exitButton.AddComponent<Button>();
                Debug.Log("Added Button component to Exit Button in VerifyButtonsSetup");
            }
            else
            {
                exitButtonComponent = exitButton.GetComponent<Button>();
            }
            
            exitButtonComponent.onClick.RemoveAllListeners();
            exitButtonComponent.onClick.AddListener(OnExitButtonClick);
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
        
        if (!allButtonsSetup)
        {
            Debug.LogWarning("Some buttons are not set up correctly. Running emergency setup...");
            EmergencySetup();
        }
    }

    private void CacheComponents()
    {
        if (mapPanel == null)
        {
            Debug.LogError("Map Panel is not assigned!");
        }

        if (buttonPanel == null)
        {
            Debug.LogError("Button Panel is not assigned!");
        }

        if (sphereObjects == null || sphereObjects.Length == 0)
        {
            Debug.LogError("Sphere Objects are not assigned!");
        }

        if (narationPanel == null)
        {
            Debug.LogWarning("Naration Panel is not assigned!");
        }

        if (narationButton == null)
        {
            Debug.LogWarning("Naration Button is not assigned!");
        }

        if (exitButton == null)
        {
            Debug.LogWarning("Exit Button is not assigned!");
        }
        else
        {
            exitButtonComponent = exitButton.GetComponent<Button>();
            if (exitButtonComponent == null)
            {
                Debug.LogWarning("Exit Button does not have a Button component! Adding one automatically...");
                exitButtonComponent = exitButton.AddComponent<Button>();
                
                // Set default colors
                ColorBlock colors = exitButtonComponent.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1f, 0.5f, 0.5f, 1f);
                exitButtonComponent.colors = colors;
                
                // Setup onClick event
                exitButtonComponent.onClick.AddListener(OnExitButtonClick);
                
                Debug.Log("Button component added to Exit Button automatically");
            }
        }

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
        
        // Cache exit button component adalah duplikat dan perlu dihapus

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
        
        // Setup event untuk naration button (toggle)
        if (narationButton != null)
        {
            narationButton.onClick.RemoveAllListeners();
            narationButton.onClick.AddListener(ToggleNaration);
        }
        
        // Setup event untuk exit button
        if (exitButtonComponent != null)
        {
            exitButtonComponent.onClick.RemoveAllListeners();
            exitButtonComponent.onClick.AddListener(OnExitButtonClick);
        }
        else if (exitButton != null)
        {
            exitButtonComponent = exitButton.GetComponent<Button>();
            if (exitButtonComponent == null)
            {
                exitButtonComponent = exitButton.AddComponent<Button>();
                // Set default colors
                ColorBlock colors = exitButtonComponent.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1f, 0.5f, 0.5f, 1f);
                exitButtonComponent.colors = colors;
                Debug.Log("Added Button component to Exit Button in SetupButtonEvents");
            }
            
            exitButtonComponent.onClick.RemoveAllListeners();
            exitButtonComponent.onClick.AddListener(OnExitButtonClick);
        }
    }

    private void SetupBasicComponents()
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

        // Sembunyikan UI panels saat inisialisasi (TANPA mengaktifkan map overlay)
        if (mapPanel != null)
        {
            mapPanel.SetActive(false);
        }
        
        if (buttonPanel != null)
        {
            buttonPanel.SetActive(false);
        }
        
        if (sphereUI != null)
        {
            sphereUI.SetActive(false);
        }
        
        if (narationPanel != null)
        {
            // Jangan aktifkan/nonaktifkan di sini, karena akan dilakukan di ShowNaration()
        }

        UpdatePlayPauseButton();

        if (cameraControl != null)
            cameraControl.enabled = true;
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

        // Sembunyikan narasi saat inisialisasi (akan ditampilkan di Start())
        if (narationPanel != null)
        {
            narationPanel.SetActive(false);
        }
        
        ShowMapOverlay();
        UpdatePlayPauseButton();
        SetupFloatingButtons();

        if (cameraControl != null)
            cameraControl.enabled = true;
    }

    private void SetupFloatingButtons()
    {
        // Setup exit button position
        if (exitButton != null)
        {
            RectTransform exitRectTransform = exitButton.GetComponent<RectTransform>();
            if (exitRectTransform != null)
            {
                // Position exit button in the top right corner
                exitRectTransform.anchorMin = new Vector2(1, 1);
                exitRectTransform.anchorMax = new Vector2(1, 1);
                exitRectTransform.pivot = new Vector2(1, 1);
                exitRectTransform.anchoredPosition = new Vector2(exitButtonX, exitButtonY);
                exitRectTransform.sizeDelta = exitButtonSize;
            }
        }

        SetActive(homeButton, true);
        SetActive(playPauseButton, true);
        
        // Pastikan exit button selalu terlihat
        SetActive(exitButton, true);
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

    // Pastikan sphereUI tersembunyi saat narasi ditampilkan
    public void OnButtonClick(int sphereIndex)
    {
        // Sembunyikan narasi panel jika terbuka
        HideNaration();
        
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
        // Toggle map panel
        if (mapPanel != null)
        {
            bool isMapActive = !mapPanel.activeSelf;
            mapPanel.SetActive(isMapActive);
            
            // Toggle button panel with map panel
            if (buttonPanel != null)
            {
                buttonPanel.SetActive(isMapActive);
            }
            
            // Hide narration panel if map is shown
            if (isMapActive && isNarationVisible && narationPanel != null)
            {
                narationPanel.SetActive(false);
                isNarationVisible = false;
            }
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

    // Fungsi untuk mengelola panel narasi
    public void ToggleNaration()
    {
        if (isNarationVisible)
        {
            HideNaration();
        }
        else
        {
            ShowNaration();
        }
    }
    
    public void ShowNaration()
    {
        if (narationPanel != null)
        {
            // Hide map and button panels if they are visible
            if (mapPanel != null)
            {
                mapPanel.SetActive(false);
            }
            
            if (buttonPanel != null)
            {
                buttonPanel.SetActive(false);
            }
            
            if (sphereUI != null)
            {
                sphereUI.SetActive(false);
            }
            
            narationPanel.SetActive(true);
            isNarationVisible = true;
            
            Debug.Log("Naration panel shown");
        }
    }
    
    public void HideNaration()
    {
        if (narationPanel != null)
        {
            narationPanel.SetActive(false);
            isNarationVisible = false;
        }
    }

    // Function to handle exit button click
    public void OnExitButtonClick()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
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
        Debug.Log($"isNarationVisible: {isNarationVisible}");
        Debug.Log($"narationPanel: {(narationPanel == null ? "NULL" : $"Active: {narationPanel.activeInHierarchy}")}");
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
        
        // // Emergency setup for exit button
        // if (exitButton != null)
        // {
        //     // Hapus komponen lama jika ada
        //     Button oldButton = exitButton.GetComponent<Button>();
        //     if (oldButton != null)
        //     {
        //         DestroyImmediate(oldButton);
        //     }
            
        //     // Tambahkan komponen baru
        //     exitButtonComponent = exitButton.AddComponent<Button>();
            
        //     // Setup colors
        //     ColorBlock colors = exitButtonComponent.colors;
        //     colors.normalColor = Color.white;
        //     colors.highlightedColor = new Color(1f, 0.5f, 0.5f, 1f);
        //     exitButtonComponent.colors = colors;
            
        //     // Add event listener
        //     exitButtonComponent.onClick.RemoveAllListeners();
        //     exitButtonComponent.onClick.AddListener(OnExitButtonClick);
        //     Debug.Log("Emergency setup: Exit button recreated and configured successfully");
        // }

        // Emergency setup for narration button
        if (narationButton != null)
        {
            narationButton.onClick.RemoveAllListeners();
            narationButton.onClick.AddListener(ToggleNaration);
        }
        
        if (mapPanel != null) mapPanel.SetActive(true);
        if (buttonPanel != null) buttonPanel.SetActive(true);
        if (sphereUI != null) sphereUI.SetActive(false);
        if (narationPanel != null) narationPanel.SetActive(false);
        isNarationVisible = false;
        
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

    [ContextMenu("Setup Hover Animations")]
    private void SetupHoverAnimations()
    {
        if (buttonPanel == null)
        {
            Debug.LogError("Button panel is not assigned!");
            return;
        }
        
        Button[] buttons = buttonPanel.GetComponentsInChildren<Button>(true);
        int count = 0;
        
        foreach (Button btn in buttons)
        {
            if (btn == null) continue;
            
            HoverAnimationController hoverAnim = btn.gameObject.GetComponent<HoverAnimationController>();
            if (hoverAnim == null)
            {
                hoverAnim = btn.gameObject.AddComponent<HoverAnimationController>();
                
                hoverAnim.useScaleAnimation = true;
                hoverAnim.hoverScaleMultiplier = 1.1f;
                hoverAnim.scaleDuration = 0.15f;
                
                hoverAnim.useColorAnimation = true;
                hoverAnim.hoverColor = new Color(1f, 1f, 1f, 1f);
                hoverAnim.colorDuration = 0.15f;
                
                hoverAnim.useRotationAnimation = false;
                
                count++;
            }
        }
        
        // Menambahkan hover animation ke homeButton
        if (homeButton != null && homeButton.GetComponent<HoverAnimationController>() == null)
        {
            HoverAnimationController hoverAnim = homeButton.AddComponent<HoverAnimationController>();
            hoverAnim.useScaleAnimation = true;
            hoverAnim.hoverScaleMultiplier = 1.15f;
            hoverAnim.useColorAnimation = true;
            count++;
        }
        
        // Menambahkan hover animation ke playPauseButton
        if (playPauseButton != null && playPauseButton.GetComponent<HoverAnimationController>() == null)
        {
            HoverAnimationController hoverAnim = playPauseButton.AddComponent<HoverAnimationController>();
            hoverAnim.useScaleAnimation = true;
            hoverAnim.hoverScaleMultiplier = 1.15f;
            hoverAnim.useColorAnimation = true;
            count++;
        }
        
        // Menambahkan hover animation ke narationButton
        if (narationButton != null && narationButton.gameObject.GetComponent<HoverAnimationController>() == null)
        {
            HoverAnimationController hoverAnim = narationButton.gameObject.AddComponent<HoverAnimationController>();
            hoverAnim.useScaleAnimation = true;
            hoverAnim.hoverScaleMultiplier = 1.15f;
            hoverAnim.useColorAnimation = true;
            count++;
        }
        
        // Menambahkan hover animation ke exitButton
        if (exitButton != null && exitButton.GetComponent<HoverAnimationController>() == null)
        {
            HoverAnimationController hoverAnim = exitButton.AddComponent<HoverAnimationController>();
            hoverAnim.useScaleAnimation = true;
            hoverAnim.hoverScaleMultiplier = 1.15f;
            hoverAnim.useColorAnimation = true;
            // Warna merah saat hover untuk tombol exit
            hoverAnim.hoverColor = new Color(1f, 0.5f, 0.5f, 1f);
            count++;
        }
        
        Debug.Log($"Successfully added hover animations to {count} buttons");
        
        // Setup hover animations for exit button
        if (exitButtonComponent != null)
        {
            Color normalColor = Color.white;
            Color hoverColor = new Color(1f, 0.3f, 0.3f); // Red hover color for exit button
            
            exitButtonComponent.transition = Selectable.Transition.ColorTint;
            ColorBlock colorBlock = exitButtonComponent.colors;
            colorBlock.normalColor = normalColor;
            colorBlock.highlightedColor = hoverColor;
            colorBlock.selectedColor = hoverColor;
            colorBlock.pressedColor = new Color(hoverColor.r * 0.8f, hoverColor.g * 0.8f, hoverColor.b * 0.8f);
            colorBlock.fadeDuration = 0.1f;
            exitButtonComponent.colors = colorBlock;
        }
    }

    [ContextMenu("Toggle Naration")]
    private void DebugToggleNaration()
    {
        ToggleNaration();
    }

    [ContextMenu("Toggle Exit Button")]
    private void ToggleExitButton()
    {
        if (exitButton != null)
        {
            exitButton.SetActive(!exitButton.activeSelf);
            Debug.Log($"Exit Button is now {(exitButton.activeSelf ? "visible" : "hidden")}");
        }
    }

    [ContextMenu("Debug UI Status")]
    private void DebugUIStatus()
    {
        string status = "UI Status:\n";
        
        if (mapPanel != null)
            status += $"Map Panel: {(mapPanel.activeSelf ? "Visible" : "Hidden")}\n";
        
        if (buttonPanel != null)
            status += $"Button Panel: {(buttonPanel.activeSelf ? "Visible" : "Hidden")}\n";
        
        if (narationPanel != null)
            status += $"Narration Panel: {(narationPanel.activeSelf ? "Visible" : "Hidden")}\n";
        
        if (exitButton != null)
            status += $"Exit Button: {(exitButton.activeSelf ? "Visible" : "Hidden")}\n";
        
        if (sphereUI != null)
            status += $"Sphere UI: {(sphereUI.activeSelf ? "Visible" : "Hidden")}\n";
        
        for (int i = 0; i < sphereObjects.Length; i++)
        {
            if (sphereObjects[i] != null)
                status += $"Sphere {i}: {(sphereObjects[i].activeSelf ? "Visible" : "Hidden")}\n";
        }
        
        Debug.Log(status);
    }

    [ContextMenu("Fix Exit Button")]
    private void FixExitButton()
    {
        if (exitButton == null)
        {
            Debug.LogError("Exit button reference is null! Cannot fix.");
            return;
        }
        
        // Hapus komponen lama jika ada
        Button oldButton = exitButton.GetComponent<Button>();
        if (oldButton != null)
        {
            DestroyImmediate(oldButton);
            Debug.Log("Removed old Button component");
        }
        
        // Tambahkan komponen baru
        exitButtonComponent = exitButton.AddComponent<Button>();
        Debug.Log("Added new Button component to Exit Button");
        
        // Setup event
        exitButtonComponent.onClick.RemoveAllListeners();
        exitButtonComponent.onClick.AddListener(OnExitButtonClick);
        
        // Setup hover colors
        ColorBlock colors = exitButtonComponent.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.5f, 0.5f, 1f);
        colors.selectedColor = new Color(1f, 0.5f, 0.5f, 1f);
        colors.pressedColor = new Color(0.8f, 0.3f, 0.3f, 1f);
        exitButtonComponent.colors = colors;
        
        // Pastikan tombol aktif
        exitButton.SetActive(true);
        
        // Tambahkan hover animation
        HoverAnimationController hoverAnim = exitButton.GetComponent<HoverAnimationController>();
        if (hoverAnim == null)
        {
            hoverAnim = exitButton.AddComponent<HoverAnimationController>();
            hoverAnim.useScaleAnimation = true;
            hoverAnim.hoverScaleMultiplier = 1.15f;
            hoverAnim.useColorAnimation = true;
            hoverAnim.hoverColor = new Color(1f, 0.5f, 0.5f, 1f);
        }
        
        // Update posisi
        UpdateExitButtonPosition();
        
        Debug.Log("Exit Button has been fixed and configured!");
    }

    [ContextMenu("Update Exit Button Position")]
    private void UpdateExitButtonPosition()
    {
        if (exitButton == null)
        {
            Debug.LogError("Exit button reference is null!");
            return;
        }
        
        RectTransform rt = exitButton.GetComponent<RectTransform>();
        if (rt == null)
        {
            Debug.LogError("Exit button doesn't have RectTransform!");
            return;
        }
        
        // Posisi di kanan atas
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        
        // Atur posisi dan ukuran berdasarkan variabel yang bisa diubah di Inspector
        rt.anchoredPosition = new Vector2(exitButtonX, exitButtonY);
        rt.sizeDelta = exitButtonSize;
        
        // Pastikan terlihat
        exitButton.SetActive(true);
        
        Debug.Log($"Exit Button position updated: Pos({exitButtonX}, {exitButtonY}), Size({exitButtonSize.x}, {exitButtonSize.y})");
    }

    [ContextMenu("Create Complete Exit Button")]
    private void CreateCompleteExitButton()
    {
        // Cari Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in scene!");
            return;
        }
        
        // Hapus button lama jika ada
        if (exitButton != null)
        {
            DestroyImmediate(exitButton);
        }
        
        // Buat GameObject baru
        exitButton = new GameObject("ExitButton");
        exitButton.transform.SetParent(canvas.transform, false);
        
        // Tambahkan komponen
        RectTransform rt = exitButton.AddComponent<RectTransform>();
        Image img = exitButton.AddComponent<Image>();
        exitButtonComponent = exitButton.AddComponent<Button>();
        
        // Setup posisi
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(exitButtonX, exitButtonY);
        rt.sizeDelta = exitButtonSize;
        
        // Setup visual
        img.color = Color.white;
        
        // Setup teks
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(exitButton.transform, false);
        
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        
        // Gunakan TextMeshProUGUI jika ada di project
        TextMeshProUGUI exitText = textObj.AddComponent<TextMeshProUGUI>();
        exitText.text = "EXIT";
        exitText.fontSize = 18;
        exitText.alignment = TextAlignmentOptions.Center;
        exitText.color = Color.black;
        
        // Setup fungsi
        exitButtonComponent.onClick.AddListener(OnExitButtonClick);
        
        // Setup colors
        ColorBlock colors = exitButtonComponent.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.5f, 0.5f, 1f);
        exitButtonComponent.colors = colors;
        
        Debug.Log("Complete Exit Button created successfully!");
        
        // Tambahkan hover animation
        HoverAnimationController hoverAnim = exitButton.AddComponent<HoverAnimationController>();
        hoverAnim.useScaleAnimation = true;
        hoverAnim.hoverScaleMultiplier = 1.15f;
        hoverAnim.useColorAnimation = true;
        hoverAnim.hoverColor = new Color(1f, 0.5f, 0.5f, 1f);
    }

    [ContextMenu("Move Exit Button Up")]
    private void MoveExitButtonUp()
    {
        exitButtonY -= 10f;
        UpdateExitButtonPosition();
    }

    [ContextMenu("Move Exit Button Down")]
    private void MoveExitButtonDown()
    {
        exitButtonY += 10f;
        UpdateExitButtonPosition();
    }

    [ContextMenu("Move Exit Button Left")]
    private void MoveExitButtonLeft()
    {
        exitButtonX -= 10f;
        UpdateExitButtonPosition();
    }

    [ContextMenu("Move Exit Button Right")]
    private void MoveExitButtonRight()
    {
        exitButtonX += 10f;
        UpdateExitButtonPosition();
    }

    [ContextMenu("Make Exit Button Bigger")]
    private void MakeExitButtonBigger()
    {
        exitButtonSize = new Vector2(exitButtonSize.x + 10f, exitButtonSize.y + 10f);
        UpdateExitButtonPosition();
    }

    [ContextMenu("Make Exit Button Smaller")]
    private void MakeExitButtonSmaller()
    {
        exitButtonSize = new Vector2(Mathf.Max(20f, exitButtonSize.x - 10f), Mathf.Max(20f, exitButtonSize.y - 10f));
        UpdateExitButtonPosition();
    }
}