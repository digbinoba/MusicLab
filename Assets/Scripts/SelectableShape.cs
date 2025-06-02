using UnityEngine;

public class SelectableShape : MonoBehaviour 
{
    [Header("Selection Visual")]
    private GameObject outlineInstance;
    private bool isSelected = false;
    
    [Header("Persistent Shape Properties")]
    [SerializeField] private Color _shapeColor = Color.white;
    [SerializeField] private float _shapeSize = 1f; // Size multiplier
    [SerializeField] private string _shapeType = "Sphere";
    
    private Material originalMaterial;
    private Vector3 originalScale;
    private Material persistentMaterial;
    // Public properties with persistence
    public Color shapeColor 
    { 
        get => _shapeColor; 
        set 
        { 
            _shapeColor = value;
            ApplyColorToRenderer();
        }
    }
    
    public float shapeSize 
    { 
        get => _shapeSize; 
        set 
        { 
            _shapeSize = value;
            ApplyScaleToTransform();
        }
    }
    
    public string shapeType 
    { 
        get => _shapeType; 
        set => _shapeType = value;
    }
    
    void Start()
    {
        // Create our own material instance so changes persist
        Renderer renderer = GetComponent<Renderer>();
        originalMaterial = renderer.material;
    
        Debug.Log($"Original material: {originalMaterial.name}, shader: {originalMaterial.shader.name}");
    
        persistentMaterial = new Material(originalMaterial);
        persistentMaterial.name = originalMaterial.name + "_Persistent";
        renderer.material = persistentMaterial;
    
        Debug.Log($"Created persistent material: {persistentMaterial.name}");
    
        // Store the original scale
        originalScale = transform.localScale;
    
        // Initialize properties
        _shapeColor = persistentMaterial.color;
        _shapeSize = 1f;
        _shapeType = DetermineShapeType();
    
        gameObject.tag = "SpawnedShape";
    
        Debug.Log($"Shape initialized: {gameObject.name}, Initial Color: {_shapeColor}");
    }
    void Update()
    {
        // Continuously ensure our properties are applied
        // This handles cases where external systems might reset values
        EnsurePropertiesApplied();
    }
    private void EnsurePropertiesApplied()
    {
        // Check if color was reset
        if (persistentMaterial.color != _shapeColor)
        {
            Debug.Log($"Color was reset! Reapplying {_shapeColor}");
            ApplyColorToRenderer();
        }
        
        // Check if scale was reset
        Vector3 expectedScale = originalScale * _shapeSize;
        if (Vector3.Distance(transform.localScale, expectedScale) > 0.01f)
        {
            Debug.Log($"Scale was reset! Reapplying size {_shapeSize}");
            ApplyScaleToTransform();
        }
    }
    private void ApplyColorToRenderer()
    {
        if (persistentMaterial != null)
        {
            Debug.Log($"ApplyColorToRenderer: Setting color {_shapeColor} on {gameObject.name}");
        
            // Set the color using multiple methods to ensure it works
            persistentMaterial.color = _shapeColor;
        
            if (persistentMaterial.HasProperty("_BaseColor"))
            {
                persistentMaterial.SetColor("_BaseColor", _shapeColor);
            }
        
            // Force the renderer to use the updated material
            Renderer renderer = GetComponent<Renderer>();
            renderer.material = persistentMaterial;
        
            // Additional force update
            renderer.material.color = _shapeColor;
        
            Debug.Log($"Applied color {_shapeColor}, renderer shows: {renderer.material.color}");
        }
    }
    
    private void ApplyScaleToTransform()
    {
        Vector3 newScale = originalScale * _shapeSize;
        transform.localScale = newScale;
        
        // Update outline if it exists
        if (outlineInstance != null)
        {
            outlineInstance.transform.localScale = newScale * 1.08f;
        }
        
        Debug.Log($"Applied scale {newScale} (multiplier: {_shapeSize}) to {gameObject.name}");
    }
    
    private string DetermineShapeType()
    {
        if (gameObject.name.ToLower().Contains("sphere"))
            return "Sphere";
        else if (gameObject.name.ToLower().Contains("cube"))
            return "Cube";
        else
            return "Unknown";
    }
    // These will be called by the Unity Events
    public void OnHover() 
    {
        Debug.Log("Hovering over: " + gameObject.name);
    
        // Don't show outline if this object is selected OR if any panel is open
        if (!isSelected && !IsPanelOpen())
        {
            ShowOutline();
        }
    }
    private bool IsPanelOpen()
    {
        // Check if any edit panel is currently open
        return ShapeEditManager.Instance != null && ShapeEditManager.Instance.IsPanelCurrentlyOpen();
    }
    public void OnHoverExit()
    {
        Debug.Log("Hover exit: " + gameObject.name);
    
        // Only hide outline if not selected
        if (!isSelected)
        {
            HideOutline();
        }
    }
    
    public void OnSelect() 
    {
        Debug.Log("Selected: " + gameObject.name);
        SelectShape();
        if (ShapeEditManager.Instance != null)
        {
            ShapeEditManager.Instance.OpenEditPanel(this);
        }
    }
    
    private void ShowOutline()
    {
        if (outlineInstance == null)
        {
            // Create the outline object
            outlineInstance = Instantiate(gameObject, transform.position, transform.rotation);
            outlineInstance.transform.localScale = transform.localScale * 1.1f;
        
            // Move it slightly back in render order
            outlineInstance.transform.position = transform.position - transform.forward * 0.001f;
        
            Renderer outlineRenderer = outlineInstance.GetComponent<Renderer>();
            Renderer originalRenderer = GetComponent<Renderer>();
        
            Color brightOutlineColor = CreateBrighterColor(originalRenderer.material.color);
        
            // Create outline material
            Material outlineMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            outlineRenderer.material = outlineMat;
        
            // Solid bright color for outline
            outlineMat.SetColor("_BaseColor", brightOutlineColor);
        
            // Render behind the original object
            outlineMat.renderQueue = 1999;
        
            // Remove components
            Destroy(outlineInstance.GetComponent<Collider>());
            RemoveInteractionComponents(outlineInstance);
        
            // Make sure original object renders on top
            originalRenderer.material.renderQueue = 2000;
        }
    }

    private Color CreateBrighterColor(Color originalColor)
    {
        // Lerp towards white for natural brightening
        Color brightColor = Color.Lerp(originalColor, Color.white, 0.4f);
        return brightColor;
    }
    private void RemoveInteractionComponents(GameObject obj)
    {
        Component[] components = obj.GetComponents<Component>();
        foreach (Component component in components)
        {
            string typeName = component.GetType().Name;
            if (typeName.Contains("Interactable") || 
                typeName.Contains("Grabbable") ||
                typeName.Contains("Ray") ||
                typeName.Contains("Event") ||
                typeName == "SelectableShape")
            {
                Destroy(component);
            }
        }
    }
    private void HideOutline()
    {
        if (outlineInstance != null)
        {
            Destroy(outlineInstance);
        }
    }
    
    private void SelectShape()
    {
        // Deselect other shapes first
        SelectableShape[] allShapes = FindObjectsOfType<SelectableShape>();
        foreach (SelectableShape shape in allShapes)
        {
            if (shape != this) shape.DeselectShape();
        }
    
        isSelected = true;
    
        // When selected, we don't want the hover outline
        // The panel being open indicates selection
        HideOutline();
    }
    
    public void DeselectShape()
    {
        isSelected = false;
        HideOutline();
    }
    
    
    // Public methods for external updates
    public void UpdateColor(Color newColor)
    {
        Debug.Log($"UpdateColor called: {newColor} on {gameObject.name}");
        shapeColor = newColor; // Uses the property setter
    }
    
    public void UpdateSize(float newSizeMultiplier)
    {
        Debug.Log($"UpdateSize called: {newSizeMultiplier} on {gameObject.name}");
        shapeSize = newSizeMultiplier; // Uses the property setter
    }
    // Method to get all properties for music generation later
    public ShapeData GetShapeData()
    {
        return new ShapeData
        {
            color = shapeColor,
            size = shapeSize,
            shapeType = shapeType,
            position = transform.position,
            rotation = transform.rotation,
            objectName = gameObject.name
        };
    }
    // Data structure for music generation
    [System.Serializable]
    public class ShapeData
    {
        public Color color;
        public float size;
        public string shapeType;
        public Vector3 position;
        public Quaternion rotation;
        public string objectName;
    }
}