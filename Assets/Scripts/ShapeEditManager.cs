using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ShapeEditManager : MonoBehaviour 
{
    public static ShapeEditManager Instance;
    
    [Header("Edit Panel")]
    [SerializeField] private GameObject editPanel; // Your canvas prefab
    
    [Header("Advanced UI References")]
    [SerializeField] private Slider intensitySlider;
    [SerializeField] private Slider energySlider;
    [SerializeField] private Dropdown musicalRoleDropdown;
    [SerializeField] private Dropdown animationTypeDropdown;
    [SerializeField] private Dropdown materialTypeDropdown;
    [SerializeField] private Slider roughnessSlider;
    [SerializeField] private Toggle loopingToggle;
    [SerializeField] private Slider durationSlider;
    
    private SelectableShape currentShape;
    private GameObject currentPanelInstance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("ShapeEditManager Instance created");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        // Continuously monitor currentShape
        if (currentShape == null && currentPanelInstance != null)
        {
            Debug.LogWarning("currentShape is NULL but panel is open!");
        }
    }
    
    public void OpenEditPanel(SelectableShape shape)
    {
        Debug.Log($"=== OpenEditPanel START ===");
        Debug.Log($"OpenEditPanel called for: {shape.gameObject.name}");
        
        currentShape = shape;
        Debug.Log($"Current shape set to: {currentShape.gameObject.name}");
        Debug.Log($"Current shape instance ID: {currentShape.GetInstanceID()}");
        
        // Don't call CloseEditPanel here - it sets currentShape to null!
        if (currentPanelInstance != null)
        {
            Debug.Log("Destroying existing panel WITHOUT clearing currentShape");
            Destroy(currentPanelInstance);
            currentPanelInstance = null;
        }
        
        Vector3 panelPosition = CalculateVRPanelPosition(shape.transform.position);
        Quaternion panelRotation = CalculateVRPanelRotation(panelPosition);
        
        currentPanelInstance = Instantiate(editPanel, panelPosition, panelRotation);
        currentPanelInstance.SetActive(true);
        
        Debug.Log($"After panel creation - currentShape: {(currentShape != null ? currentShape.gameObject.name : "NULL")}");
        // AUTO-WIRE the UI events after instantiation
        WireUpPanelEvents();
        UpdatePanelToCurrentShape();

        // Update UI to match current shape (optional)
        Debug.Log($"=== OpenEditPanel END - currentShape: {(currentShape != null ? currentShape.gameObject.name : "NULL")} ===");
        
    }
    
private void WireUpPanelEvents()
    {
        Debug.Log($"=== WireUpPanelEvents START - currentShape: {(currentShape != null ? currentShape.gameObject.name : "NULL")} ===");
        
        if (currentPanelInstance == null) return;
        
        Toggle[] toggles = currentPanelInstance.GetComponentsInChildren<Toggle>();
        Button[] buttons = currentPanelInstance.GetComponentsInChildren<Button>();
        Slider[] sliders = currentPanelInstance.GetComponentsInChildren<Slider>();
        TMPro.TMP_Dropdown[] dropdowns = currentPanelInstance.GetComponentsInChildren<TMPro.TMP_Dropdown>();
        
        Debug.Log($"Found {toggles.Length} toggles, {buttons.Length} buttons, {sliders.Length} sliders, {dropdowns.Length} dropdowns");
        foreach (Slider slider in sliders)
        {
            Debug.Log($"Wiring slider: {slider.gameObject.name}");
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener((float value) => {
                Debug.Log($"Slider event - value: {value}, currentShape: {(currentShape != null ? currentShape.gameObject.name : "NULL")}");
                ChangeShapeSize(value);
            });
        }
        
        Debug.Log($"=== WireUpPanelEvents END - currentShape: {(currentShape != null ? currentShape.gameObject.name : "NULL")} ===");
        // Wire existing color toggles (keep your existing code)
        WireColorToggles(toggles);
        
        // Wire existing close button (keep your existing code)
        WireCloseButton(buttons);
        
        // Wire new advanced controls
        WireAdvancedSliders(sliders);
        WireDropdowns(dropdowns);
        WireAdvancedToggles(toggles);
    }
    private void WireColorToggles(Toggle[] toggles)
    {
        foreach (Toggle toggle in toggles)
        {
            string toggleName = toggle.gameObject.name.ToLower();
            Debug.Log($"Wiring color toggle: {toggle.gameObject.name}");
        
            if (toggleName.Contains("red"))
            {
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((bool isOn) => { 
                    Debug.Log($"Red toggle event - isOn: {isOn}, currentShape: {(currentShape != null ? currentShape.gameObject.name : "NULL")}");
                    if (isOn) SetColorRed(); 
                });
            }
            else if (toggleName.Contains("blue"))
            {
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((bool isOn) => { 
                    Debug.Log($"Blue toggle event - isOn: {isOn}, currentShape: {(currentShape != null ? currentShape.gameObject.name : "NULL")}");
                    if (isOn) SetColorBlue(); 
                });
            }
            else if (toggleName.Contains("green"))
            {
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((bool isOn) => { 
                    Debug.Log($"Green toggle event - isOn: {isOn}, currentShape: {(currentShape != null ? currentShape.gameObject.name : "NULL")}");
                    if (isOn) SetColorGreen(); 
                });
            }
            else if (toggleName.Contains("yellow"))
            {
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((bool isOn) => { 
                    Debug.Log($"Yellow toggle event - isOn: {isOn}, currentShape: {(currentShape != null ? currentShape.gameObject.name : "NULL")}");
                    if (isOn) SetColorYellow(); 
                });
            }
        }
    }
    private void WireCloseButton(Button[] buttons)
    {
        foreach (Button button in buttons)
        {
            string buttonName = button.gameObject.name.ToLower();
            Debug.Log($"Wiring button: {button.gameObject.name}");
        
            if (buttonName.Contains("close"))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => {
                    Debug.Log("Close button pressed");
                    CloseEditPanel();
                });
            }
        }
    }
private void WireAdvancedSliders(Slider[] sliders)
{
    foreach (Slider slider in sliders)
    {
        string sliderName = slider.gameObject.name.ToLower();
        Debug.Log($"Wiring slider: {slider.gameObject.name}");
        
        if (sliderName == "sizeslider" || sliderName.EndsWith("sizeslider"))
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener((float value) => {
                Debug.Log($"SIZE slider event - value: {value}");
                ChangeShapeSize(value);
                UpdateSliderValueDisplays(); // ADD THIS
            });
        }
        else if (sliderName == "intensityslider" || sliderName.EndsWith("intensityslider"))
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener((float value) => {
                Debug.Log($"INTENSITY changed to: {value}");
                ChangeShapeIntensity(value);
                UpdateSliderValueDisplays(); // ADD THIS
            });
        }
        else if (sliderName == "energyslider" || sliderName.EndsWith("energyslider"))
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener((float value) => {
                Debug.Log($"ENERGY changed to: {value}");
                ChangeShapeEnergy(value);
                UpdateSliderValueDisplays(); // ADD THIS
            });
        }
        else if (sliderName == "roughnessslider" || sliderName.EndsWith("roughnessslider"))
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener((float value) => {
                Debug.Log($"ROUGHNESS changed to: {value}");
                ChangeShapeRoughness(value);
                UpdateSliderValueDisplays(); // ADD THIS
            });
        }
        else if (sliderName == "durationslider" || sliderName.EndsWith("durationslider"))
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener((float value) => {
                Debug.Log($"DURATION changed to: {value}");
                ChangeShapeDuration(value);
                UpdateSliderValueDisplays(); // ADD THIS
            });
        }
    }
}
    private void WireDropdowns(TMPro.TMP_Dropdown[] dropdowns)
    {
        foreach (TMPro.TMP_Dropdown dropdown in dropdowns)
        {
            string dropdownName = dropdown.gameObject.name.ToLower();
            Debug.Log($"Wiring TMP dropdown: {dropdown.gameObject.name}");
        
            if (dropdownName.Contains("role") || dropdownName.Contains("musical"))
            {
                dropdown.onValueChanged.RemoveAllListeners();
                dropdown.onValueChanged.AddListener((int value) => {
                    Debug.Log($"Musical role changed to: {value}");
                    ChangeMusicalRole(value);
                });
            }
            else if (dropdownName.Contains("animation") || dropdownName.Contains("anim"))
            {
                dropdown.onValueChanged.RemoveAllListeners();
                dropdown.onValueChanged.AddListener((int value) => {
                    Debug.Log($"Animation type changed to: {value}");
                    ChangeAnimationType(value);
                });
            }
            else if (dropdownName.Contains("material"))
            {
                dropdown.onValueChanged.RemoveAllListeners();
                dropdown.onValueChanged.AddListener((int value) => {
                    Debug.Log($"Material type changed to: {value}");
                    ChangeMaterialType(value);
                });
            }
        }
    }
    private void WireAdvancedToggles(Toggle[] toggles)
    {
        foreach (Toggle toggle in toggles)
        {
            string toggleName = toggle.gameObject.name.ToLower();
            
            if (toggleName.Contains("loop"))
            {
                toggle.onValueChanged.AddListener((bool isOn) => {
                    Debug.Log($"Looping changed to: {isOn}");
                    ChangeShapeLooping(isOn);
                });
            }
            // Your existing color toggles are handled in WireColorToggles
        }
    }
    
    // New methods for advanced properties
    public void ChangeShapeIntensity(float intensity)
    {
        if (currentShape != null)
        {
            currentShape.UpdateIntensity(intensity);
        }
    }
    
    public void ChangeShapeEnergy(float energy)
    {
        if (currentShape != null)
        {
            currentShape.UpdateEnergy(energy);
        }
    }
    
    public void ChangeShapeRoughness(float roughness)
    {
        if (currentShape != null)
        {
            currentShape.UpdateRoughness(roughness);
        }
    }
    
    public void ChangeShapeDuration(float duration)
    {
        if (currentShape != null)
        {
            currentShape.UpdateDuration(duration);
        }
    }
    
    public void ChangeMusicalRole(int roleIndex)
    {
        if (currentShape != null)
        {
            currentShape.UpdateMusicalRole((SelectableShape.ShapeRole)roleIndex);
            UpdateShapeInfoDisplay(); // ADD THIS - updates the role text
        }
        else
        {
            Debug.LogError("currentShape is NULL in ChangeMusicalRole!");
        }
    }
    
    public void ChangeAnimationType(int animIndex)
    {
        if (currentShape != null)
        {
            currentShape.UpdateAnimationType((SelectableShape.AnimationType)animIndex);
        }
    }
    
    public void ChangeMaterialType(int materialIndex)
    {
        if (currentShape != null)
        {
            currentShape.UpdateMaterialType((SelectableShape.MaterialType)materialIndex);
        }
    }
    
    public void ChangeShapeLooping(bool isLooping)
    {
        if (currentShape != null)
        {
            currentShape.UpdateLooping(isLooping);
        }
    }
    
    private void UpdatePanelToCurrentShape()
    {
        if (currentPanelInstance == null || currentShape == null) return;
    
        Debug.Log($"Updating panel UI for {currentShape.gameObject.name}: Color={currentShape.shapeColor}, Size={currentShape.shapeSize}");
    
        // UPDATE SHAPE INFO DISPLAY
        UpdateShapeInfoDisplay();
        
        // Update existing controls
        UpdateExistingControls();
        
        // Update advanced controls  
        UpdateAdvancedControls();
        
        // Update size slider
        Slider sizeSlider = currentPanelInstance.GetComponentInChildren<Slider>();
        if (sizeSlider != null)
        {
            float sliderValue = Mathf.InverseLerp(1f, 3f, currentShape.shapeSize);
            sizeSlider.SetValueWithoutNotify(sliderValue);
            Debug.Log($"Set slider to {sliderValue} (from size {currentShape.shapeSize})");
        }
    
        // Update color toggles to match current color
        SetCurrentColorToggle();
        
        // Update existing controls (size slider, color toggles)
        UpdateExistingControls();
        
        // Update new advanced controls
        UpdateAdvancedControls();
        
        //Debug for dropdowns
        TMPro.TMP_Dropdown[] dropdowns = currentPanelInstance.GetComponentsInChildren<TMPro.TMP_Dropdown>();
        Debug.Log($"Found {dropdowns.Length} TMP dropdowns to update");


        foreach (TMPro.TMP_Dropdown dropdown in dropdowns)
        {
            string name = dropdown.gameObject.name.ToLower();
            Debug.Log($"Updating TMP dropdown: {dropdown.gameObject.name}");
        
            if (name.Contains("role") || name.Contains("musical"))
            {
                int roleValue = (int)currentShape.advancedData.musicalRole;
                Debug.Log($"Setting musical role dropdown to: {roleValue} ({currentShape.advancedData.musicalRole})");
                dropdown.SetValueWithoutNotify(roleValue);
            }
            else if (name.Contains("animation") || name.Contains("anim"))
            {
                int animValue = (int)currentShape.advancedData.animationType;
                Debug.Log($"Setting animation dropdown to: {animValue} ({currentShape.advancedData.animationType})");
                dropdown.SetValueWithoutNotify(animValue);
            }
            else if (name.Contains("material"))
            {
                int materialValue = (int)currentShape.advancedData.materialType;
                Debug.Log($"Setting material dropdown to: {materialValue} ({currentShape.advancedData.materialType})");
                dropdown.SetValueWithoutNotify(materialValue);
            }
        }
    }
    private void UpdateShapeInfoDisplay()
    {
        // Update shape title and info
        TMPro.TextMeshProUGUI[] texts = currentPanelInstance.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        
        foreach (TMPro.TextMeshProUGUI text in texts)
        {
            string textName = text.gameObject.name.ToLower();
            
            if (textName.Contains("shapetitle"))
            {
                text.text = $"Selected: {currentShape.gameObject.name}";
            }
            else if (textName.Contains("shapetype"))
            {
                text.text = $"{currentShape.advancedData.shapeType}";
            }
            else if (textName.Contains("shaperole"))
            {
                text.text = $"Role: {currentShape.advancedData.musicalRole}";
            }
        }
        
        Debug.Log($"Updated shape info: {currentShape.advancedData.shapeType} - {currentShape.advancedData.musicalRole}");
    }
    private void UpdateSliderValueDisplays()
    {
        TMPro.TextMeshProUGUI[] texts = currentPanelInstance.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        
        foreach (TMPro.TextMeshProUGUI text in texts)
        {
            string textName = text.gameObject.name.ToLower();
            
            if (textName.Contains("sizevalue"))
            {
                text.text = currentShape.shapeSize.ToString("F1");
            }
            else if (textName.Contains("intensityvalue"))
            {
                text.text = currentShape.advancedData.intensity.ToString("F2");
            }
            else if (textName.Contains("energyvalue"))
            {
                text.text = currentShape.advancedData.energy.ToString("F2");
            }
            else if (textName.Contains("roughnessvalue"))
            {
                text.text = currentShape.advancedData.roughness.ToString("F2");
            }
            else if (textName.Contains("durationvalue"))
            {
                text.text = currentShape.advancedData.duration.ToString("F1") + "s";
            }
        }
    }
    private void UpdateExistingControls()
    {
        // Update existing size slider
        Slider[] sliders = currentPanelInstance.GetComponentsInChildren<Slider>();
        foreach (Slider slider in sliders)
        {
            string name = slider.gameObject.name.ToLower();
            if (name.Contains("size"))
            {
                float sliderValue = Mathf.InverseLerp(1f, 3f, currentShape.shapeSize);
                slider.SetValueWithoutNotify(sliderValue);
                Debug.Log($"Set size slider to {sliderValue} (from size {currentShape.shapeSize})");
                break; // Found the size slider, no need to continue
            }
        }
    
        // Update existing color toggles
        SetCurrentColorToggle();
    }
    private void UpdateAdvancedControls()
    {
        Slider[] sliders = currentPanelInstance.GetComponentsInChildren<Slider>();
        Dropdown[] dropdowns = currentPanelInstance.GetComponentsInChildren<Dropdown>();
        Toggle[] toggles = currentPanelInstance.GetComponentsInChildren<Toggle>();
        
        // Update sliders
        foreach (Slider slider in sliders)
        {
            string name = slider.gameObject.name.ToLower();
            if (name.Contains("intensity"))
                slider.SetValueWithoutNotify(currentShape.advancedData.intensity);
            else if (name.Contains("energy"))
                slider.SetValueWithoutNotify(currentShape.advancedData.energy);
            else if (name.Contains("roughness"))
                slider.SetValueWithoutNotify(currentShape.advancedData.roughness);
            else if (name.Contains("duration"))
                slider.SetValueWithoutNotify(currentShape.advancedData.duration);
        }
        
        // Update dropdowns
        foreach (Dropdown dropdown in dropdowns)
        {
            string name = dropdown.gameObject.name.ToLower();
            if (name.Contains("role") || name.Contains("musical"))
                dropdown.SetValueWithoutNotify((int)currentShape.advancedData.musicalRole);
            else if (name.Contains("animation") || name.Contains("anim"))
                dropdown.SetValueWithoutNotify((int)currentShape.advancedData.animationType);
            else if (name.Contains("material"))
                dropdown.SetValueWithoutNotify((int)currentShape.advancedData.materialType);
        }
        
        // Update advanced toggles
        foreach (Toggle toggle in toggles)
        {
            string name = toggle.gameObject.name.ToLower();
            if (name.Contains("loop"))
                toggle.SetIsOnWithoutNotify(currentShape.advancedData.isLooping);
        }
        // ADD THIS LINE AT THE END:
        UpdateSliderValueDisplays();
    }
    private void SetCurrentColorToggle()
    {
        if (currentPanelInstance == null || currentShape == null) return;
    
        Toggle[] toggles = currentPanelInstance.GetComponentsInChildren<Toggle>();
        Color currentColor = currentShape.shapeColor;
    
        foreach (Toggle toggle in toggles)
        {
            string toggleName = toggle.gameObject.name.ToLower();
            bool shouldBeOn = false;
        
            if (toggleName.Contains("red") && IsColorMatch(currentColor, Color.red))
                shouldBeOn = true;
            else if (toggleName.Contains("blue") && IsColorMatch(currentColor, Color.blue))
                shouldBeOn = true;
            else if (toggleName.Contains("green") && IsColorMatch(currentColor, Color.green))
                shouldBeOn = true;
            else if (toggleName.Contains("yellow") && IsColorMatch(currentColor, Color.yellow))
                shouldBeOn = true;
        
            toggle.SetIsOnWithoutNotify(shouldBeOn);
        }
    
        Debug.Log($"Updated toggles to match current color: {currentColor}");
    }
    private bool IsColorMatch(Color a, Color b)
    {
        float tolerance = 0.1f;
        return Vector3.Distance(new Vector3(a.r, a.g, a.b), new Vector3(b.r, b.g, b.b)) < tolerance;
    }
    public void SetColorRed() 
    { 
        Debug.Log($"SetColorRed called! currentShape: {(currentShape != null ? currentShape.gameObject.name : "NULL")}"); 
        if (currentShape != null)
        {
            ChangeShapeColor(Color.red);
        }
        else
        {
            Debug.LogError("currentShape is NULL in SetColorRed!");
        }
    }
    
    // Add similar debug to other color methods...
    public void SetColorBlue() 
    { 
        Debug.Log($"SetColorBlue called! currentShape: {(currentShape != null ? currentShape.gameObject.name : "NULL")}"); 
        if (currentShape != null)
        {
            ChangeShapeColor(Color.blue);
        }
        else
        {
            Debug.LogError("currentShape is NULL in SetColorBlue!");
        }
    }
    
    public void SetColorGreen() 
    { 
        Debug.Log($"SetColorGreen called! currentShape: {(currentShape != null ? currentShape.gameObject.name : "NULL")}"); 
        if (currentShape != null)
        {
            ChangeShapeColor(Color.green);
        }
        else
        {
            Debug.LogError("currentShape is NULL in SetColorGreen!");
        }
    }
    
    public void SetColorYellow() 
    { 
        Debug.Log($"SetColorYellow called! currentShape: {(currentShape != null ? currentShape.gameObject.name : "NULL")}"); 
        if (currentShape != null)
        {
            ChangeShapeColor(Color.yellow);
        }
        else
        {
            Debug.LogError("currentShape is NULL in SetColorYellow!");
        }
    }
    
    private Vector3 CalculateVRPanelPosition(Vector3 shapePosition)
    {
        Vector3 userPosition = Camera.main.transform.position;
        Vector3 userRight = Camera.main.transform.right;
    
        // 30cm to the right of the object, facing user
        Vector3 panelPosition = shapePosition + userRight * 0.3f + Vector3.up * 0.1f;
    
        // Ensure minimum distance from user
        if (Vector3.Distance(panelPosition, userPosition) < 0.5f)
        {
            Vector3 awayFromUser = (panelPosition - userPosition).normalized;
            panelPosition = userPosition + awayFromUser * 0.5f;
        }
    
        return panelPosition;
    }

    private Quaternion CalculateVRPanelRotation(Vector3 panelPosition)
    {
        
            Vector3 userPosition = Camera.main.transform.position;
            Vector3 directionToUser = (userPosition - panelPosition).normalized;
            directionToUser.y = 0;
    
            Quaternion lookRotation = Quaternion.LookRotation(directionToUser, Vector3.up);
    
            // Add 180-degree Y rotation if panel is backwards
            return lookRotation * Quaternion.Euler(0, 180, 0);
    }
    public void ChangeShapeSize(float sliderValue)
    {
        Debug.Log($"ChangeShapeSize called with slider value: {sliderValue}");
    
        if (currentShape != null)
        {
            // Map slider value (0-1) to size multiplier (1-3)
            float sizeMultiplier = Mathf.Lerp(1f, 3f, sliderValue);
        
            Debug.Log($"Slider {sliderValue} mapped to size multiplier: {sizeMultiplier}");
        
            currentShape.UpdateSize(sizeMultiplier);
        }
        else
        {
            Debug.LogError("currentShape is NULL in ChangeShapeSize!");
        }
    }


    public void CloseEditPanel()
    {
        Debug.Log("=== CloseEditPanel called ===");
        if (currentPanelInstance != null)
        {
            Destroy(currentPanelInstance);
            currentPanelInstance = null;
        }
        
        if (currentShape != null)
        {
            currentShape.DeselectShape();
            currentShape = null;
            Debug.Log("currentShape set to NULL in CloseEditPanel");
        }
    }
    
    private void ChangeShapeColor(Color newColor)
    {
        Debug.Log($"ChangeShapeColor called with color: {newColor}");
        if (currentShape != null)
        {
            currentShape.UpdateColor(newColor);
        }
        else
        {
            Debug.LogError("currentShape is NULL in ChangeShapeColor!");
        }
    }
    public bool IsPanelCurrentlyOpen()
    {
        return currentPanelInstance != null;
    }
}