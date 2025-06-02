using UnityEngine;
using UnityEngine.UI;
public class ShapeEditManager : MonoBehaviour 
{
    public static ShapeEditManager Instance;
    
    [Header("Edit Panel")]
    [SerializeField] private GameObject editPanel; // Your canvas prefab
    
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
        
        Debug.Log($"Found {toggles.Length} toggles, {buttons.Length} buttons, {sliders.Length} sliders");
        
        foreach (Toggle toggle in toggles)
        {
            string toggleName = toggle.gameObject.name.ToLower();
            Debug.Log($"Wiring toggle: {toggle.gameObject.name}");
            
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
    }
    private void UpdatePanelToCurrentShape()
    {
        if (currentPanelInstance == null || currentShape == null) return;
    
        Debug.Log($"Updating panel UI for {currentShape.gameObject.name}: Color={currentShape.shapeColor}, Size={currentShape.shapeSize}");
    
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