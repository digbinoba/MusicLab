using UnityEngine;

public class SelectableShape : MonoBehaviour 
{
    [Header("Selection Visual")]
    private GameObject outlineInstance;
    private bool isSelected = false;
    
    [Header("Shape Properties")]
    public Color shapeColor = Color.white;
    public float shapeSize = 1f;
    public string shapeType = "Sphere";
    
    private Material originalMaterial;
    private Vector3 originalScale;
    
    void Start()
    {
        originalMaterial = GetComponent<Renderer>().material;
        originalScale = transform.localScale;
        gameObject.tag = "SpawnedShape";
    }
    
    // These will be called by the Unity Events
    public void OnHover() 
    {
        Debug.Log("Hovering over: " + gameObject.name);
        ShowOutline();
    }
    
    public void OnHoverExit()
    {
        Debug.Log("Hover exit: " + gameObject.name);
        if (!isSelected) HideOutline();
    }
    
    public void OnSelect() 
    {
        Debug.Log("*** SELECT EVENT FIRED on " + gameObject.name + " ***");
    
        // Debug the manager
        if (ShapeEditManager.Instance == null)
        {
            Debug.LogError("ShapeEditManager.Instance is NULL! Make sure you have a ShapeEditManager in the scene.");
        }
        else
        {
            Debug.Log("ShapeEditManager found, calling OpenEditPanel");
            ShapeEditManager.Instance.OpenEditPanel(this);
        }
    
        SelectShape();
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
        ShowOutline();
    }
    
    public void DeselectShape()
    {
        isSelected = false;
        HideOutline();
    }
    
    public void UpdateColor(Color newColor)
    {
        shapeColor = newColor;
        GetComponent<Renderer>().material.color = newColor;
    }
    
    public void UpdateSize(float newSize)
    {
        shapeSize = newSize;
        transform.localScale = originalScale * newSize;
        
        if (outlineInstance != null)
        {
            outlineInstance.transform.localScale = transform.localScale * 1.1f;
        }
    }
}