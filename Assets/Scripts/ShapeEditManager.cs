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
        Instance = this;
        Debug.Log("ShapeEditManager Instance created");
    }
    
    public void OpenEditPanel(SelectableShape shape)
    {
        Debug.Log("OpenEditPanel called for: " + shape.gameObject.name);
        
        if (editPanel == null)
        {
            Debug.LogError("EditPanel prefab is not assigned in ShapeEditManager!");
            return;
        }
        
        currentShape = shape;
        
        // Close existing panel first
        CloseEditPanel();
        
        // Calculate VR-friendly position
        Vector3 panelPosition = CalculateVRPanelPosition(shape.transform.position);
        Quaternion panelRotation = CalculateVRPanelRotation(panelPosition);
    
        currentPanelInstance = Instantiate(editPanel, panelPosition, panelRotation);
        currentPanelInstance.SetActive(true);
    
        Debug.Log("VR Panel created at: " + panelPosition);
        Debug.Log("Distance from user: " + Vector3.Distance(panelPosition, Camera.main.transform.position));
        
        
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
    public void CloseEditPanel()
    {
        if (currentPanelInstance != null)
        {
            Debug.Log("Closing panel");
            Destroy(currentPanelInstance);
            currentPanelInstance = null;
        }
        
        if (currentShape != null)
        {
            currentShape.DeselectShape();
            currentShape = null;
        }
    }
    private void SetupPanelUI()
    {
        if (currentPanelInstance == null) return;
        
        // Find UI elements in the panel (adjust names to match your UI)
        Toggle redToggle = FindToggleByName("Red");
        Toggle blueToggle = FindToggleByName("Blue");
        Toggle greenToggle = FindToggleByName("Green");
        Toggle yellowToggle = FindToggleByName("Yellow");
        Button closeButton = FindButtonByName("CloseButton");
        Slider sizeSlider = currentPanelInstance.GetComponentInChildren<Slider>();
        
        // Wire up color buttons
        if (redToggle != null)
            redToggle.onValueChanged.AddListener((bool isOn) => { if (isOn) ChangeShapeColor(Color.red); });
        if (blueToggle != null)
            blueToggle.onValueChanged.AddListener((bool isOn) => { if (isOn) ChangeShapeColor(Color.blue); });
        if (greenToggle != null)
            greenToggle.onValueChanged.AddListener((bool isOn) => { if (isOn) ChangeShapeColor(Color.green); });
        if (yellowToggle != null)
            yellowToggle.onValueChanged.AddListener((bool isOn) => { if (isOn) ChangeShapeColor(Color.yellow); });
        
        // Wire up close button
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseEditPanel);
        
        // Wire up size slider
        if (sizeSlider != null)
        {
            sizeSlider.value = currentShape.shapeSize; // Set current size
            sizeSlider.onValueChanged.AddListener(ChangeShapeSize);
        }
        
// Set the correct toggle based on current shape color
        SetCurrentColorToggle(redToggle, blueToggle, greenToggle, yellowToggle);
    
        Debug.Log($"UI Setup: Red={redToggle!=null}, Blue={blueToggle!=null}, Green={greenToggle!=null}, Yellow={yellowToggle!=null}, Close={closeButton!=null}, Slider={sizeSlider!=null}");
    }
    
    private Button FindButtonByName(string buttonName)
    {
        Button[] buttons = currentPanelInstance.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            if (button.gameObject.name.Contains(buttonName.Replace("Button", ""))) // Flexible naming
            {
                return button;
            }
        }
        Debug.LogWarning($"Button '{buttonName}' not found in panel");
        return null;
    }
    
    // UI Event Methods
    public void ChangeShapeColor(Color newColor)
    {
        if (currentShape != null)
        {
            Debug.Log($"Changing shape color to: {newColor}");
            currentShape.UpdateColor(newColor);
        }
    }
    
    public void ChangeShapeSize(float newSize)
    {
        if (currentShape != null)
        {
            Debug.Log($"Changing shape size to: {newSize}");
            currentShape.UpdateSize(newSize);
        }
    }
    
    private Toggle FindToggleByName(string colorName)
    {
        Toggle[] toggles = currentPanelInstance.GetComponentsInChildren<Toggle>();
        foreach (Toggle toggle in toggles)
        {
            if (toggle.gameObject.name.ToLower().Contains(colorName.ToLower()))
            {
                return toggle;
            }
        }
        Debug.LogWarning($"Toggle for '{colorName}' not found in panel");
        return null;
    }

    private void SetCurrentColorToggle(Toggle redToggle, Toggle blueToggle, Toggle greenToggle, Toggle yellowToggle)
    {
        if (currentShape == null) return;
    
        Color shapeColor = currentShape.shapeColor;
    
        // Clear all toggles first (without triggering events)
        redToggle?.SetIsOnWithoutNotify(false);
        blueToggle?.SetIsOnWithoutNotify(false);
        greenToggle?.SetIsOnWithoutNotify(false);
        yellowToggle?.SetIsOnWithoutNotify(false);
    
        // Set the matching color toggle
        if (IsColorMatch(shapeColor, Color.red))
            redToggle?.SetIsOnWithoutNotify(true);
        else if (IsColorMatch(shapeColor, Color.blue))
            blueToggle?.SetIsOnWithoutNotify(true);
        else if (IsColorMatch(shapeColor, Color.green))
            greenToggle?.SetIsOnWithoutNotify(true);
        else if (IsColorMatch(shapeColor, Color.yellow))
            yellowToggle?.SetIsOnWithoutNotify(true);
        else
            redToggle?.SetIsOnWithoutNotify(true); // Default to red if no match
    }

    private bool IsColorMatch(Color a, Color b)
    {
        float tolerance = 0.1f;
        return Mathf.Abs(a.r - b.r) < tolerance && 
               Mathf.Abs(a.g - b.g) < tolerance && 
               Mathf.Abs(a.b - b.b) < tolerance;
    }
 
}