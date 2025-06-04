using System.Collections.Generic;
using UnityEngine;

public class SelectableShape : MonoBehaviour 
{
    [Header("Selection Visual")]
    private GameObject outlineInstance;
    private bool isSelected = false;
    
    [Header("Persistent Shape Properties")]
    [SerializeField] private float _shapeSize = 1f; // Size multiplier
    [SerializeField] private string _shapeType = "Sphere";
    
    [Header("Advanced Properties")]
    public AdvancedShapeData advancedData;
    
    [Header("Animation")]
    [SerializeField] private ShapeAnimator shapeAnimator;
    
    private Material originalMaterial;
    private Vector3 originalScale;
    private Material persistentMaterial;
    // Public properties with persistence
    public EmotionalColor emotionalColor 
    { 
        get => advancedData.emotionalColor; 
        set 
        { 
            advancedData.emotionalColor = value;
            ApplyEmotionalColorToRenderer();
        }
    }
    public Color shapeColor => EmotionalColorUtility.GetEmotionalColor(advancedData.emotionalColor);
    
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
        Debug.Log($"=== START: {gameObject.name} ===");
        Debug.Log($"Starting position: {transform.position}");
        
        // Get original renderer and material
        Renderer renderer = GetComponent<Renderer>();
        originalMaterial = renderer.material;
    
        // Create NEW URP material with proper settings
        persistentMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        persistentMaterial.name = gameObject.name + "_EmotionalMaterial";
    
        // IMPORTANT: Set up proper rendering properties
        SetupURPMaterialProperties();
    
        renderer.material = persistentMaterial;
    
        Debug.Log($"Created new URP material: {persistentMaterial.name}");
        Debug.Log($"Material shader: {persistentMaterial.shader.name}");
    
        // Store the original scale
        originalScale = transform.localScale;
    
        // Initialize advanced data
        advancedData = new AdvancedShapeData();
        advancedData.position = transform.position;
        advancedData.size = shapeSize;
        advancedData.shapeType = DetermineShapeType();
        advancedData.spawnTime = Time.time;
    
        // Set defaults and apply color
        SetShapeDefaults();
        ApplyEmotionalColorToRenderer();
    
        // Get the animator component
        if (shapeAnimator == null)
            shapeAnimator = GetComponent<ShapeAnimator>();
    

        if (shapeAnimator != null)
        {
            // Initialize with current position (spawn position)
            shapeAnimator.Initialize();
            shapeAnimator.SetAnimationType(advancedData.animationType);
        }
        
        Debug.Log($"=== END START: {gameObject.name} ===");
    
        gameObject.tag = "SpawnedShape";
    }
    
    public void UpdateAnimationType(AnimationType animType)
    {
        Debug.Log($"UpdateAnimationType called: OLD={advancedData.animationType}, NEW={animType}");
        advancedData.animationType = animType;
    
        // Update the visual animation
        if (shapeAnimator != null)
        {
            shapeAnimator.SetAnimationType(animType);
        }
    
        Debug.Log($"Updated {gameObject.name} animation to: {advancedData.animationType}");
    }
    private void SetupURPMaterialProperties()
    {
        // Essential URP material properties for visibility
    
        // Surface Type: Opaque (not transparent)
        persistentMaterial.SetFloat("_Surface", 0); // 0 = Opaque, 1 = Transparent
    
        // Blend Mode: Alpha (for opaque)
        persistentMaterial.SetFloat("_Blend", 0); // 0 = Alpha, 1 = Premultiply, etc.
    
        // Alpha Clipping: Off
        persistentMaterial.SetFloat("_AlphaClip", 0);
    
        // Cull Mode: Back (normal)
        persistentMaterial.SetFloat("_Cull", 2); // 0 = Off, 1 = Front, 2 = Back
    
        // Z Write: On (writes to depth buffer)
        persistentMaterial.SetFloat("_ZWrite", 1);
    
        // Z Test: LessEqual (normal depth testing)
        persistentMaterial.SetFloat("_ZTest", 4);
    
        // Render Queue: Geometry (normal opaque objects)
        persistentMaterial.renderQueue = 2000;
    
        // Basic material properties
        persistentMaterial.SetFloat("_Smoothness", 0.5f);
        persistentMaterial.SetFloat("_Metallic", 0.0f);
        persistentMaterial.SetFloat("_SpecularHighlights", 1.0f);
        persistentMaterial.SetFloat("_EnvironmentReflections", 1.0f);
    
        // Default white base color (will be changed by emotional color)
        persistentMaterial.SetColor("_BaseColor", Color.white);
    
        // Enable/disable keywords for proper rendering
        persistentMaterial.DisableKeyword("_ALPHATEST_ON");
        persistentMaterial.DisableKeyword("_ALPHABLEND_ON");
        persistentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        persistentMaterial.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        persistentMaterial.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
    
        Debug.Log("URP Material properties configured for visibility");
    }
    
    void Update()
    {
        // Continuously ensure our properties are applied
        // This handles cases where external systems might reset values
        EnsurePropertiesApplied();
    }
    private void EnsurePropertiesApplied()
    {
        // Only check if emotional color needs reapplying
        Color expectedColor = EmotionalColorUtility.GetEmotionalColor(advancedData.emotionalColor);
        Color currentColor = persistentMaterial.GetColor("_BaseColor");
    
        if (Vector3.Distance(new Vector3(currentColor.r, currentColor.g, currentColor.b),
                new Vector3(expectedColor.r, expectedColor.g, expectedColor.b)) > 0.01f)
        {
            Debug.Log($"Emotional color was reset! Reapplying {advancedData.emotionalColor} ({expectedColor})");
            ApplyEmotionalColorToRenderer();
        }
    
        // Check if scale was reset
        Vector3 expectedScale = originalScale * _shapeSize;
        if (Vector3.Distance(transform.localScale, expectedScale) > 0.01f)
        {
            Debug.Log($"Scale was reset! Reapplying size {_shapeSize}");
            ApplyScaleToTransform();
        }
    }
    // private void ApplyColorToRenderer()
    // {
    //     if (persistentMaterial != null)
    //     {
    //         Debug.Log($"ApplyColorToRenderer: Setting color {_shapeColor} on {gameObject.name}");
    //     
    //         // Set the color using multiple methods to ensure it works
    //         persistentMaterial.color = _shapeColor;
    //     
    //         if (persistentMaterial.HasProperty("_BaseColor"))
    //         {
    //             persistentMaterial.SetColor("_BaseColor", _shapeColor);
    //         }
    //     
    //         // Force the renderer to use the updated material
    //         Renderer renderer = GetComponent<Renderer>();
    //         renderer.material = persistentMaterial;
    //     
    //         // Additional force update
    //         renderer.material.color = _shapeColor;
    //     
    //         Debug.Log($"Applied color {_shapeColor}, renderer shows: {renderer.material.color}");
    //     }
    // }
    
    private void ApplyScaleToTransform()
    {
        Vector3 newScale = originalScale * _shapeSize;
        transform.localScale = newScale;
        
        // Update animator's original scale so pulse animation works correctly
        if (shapeAnimator != null)
        {
            shapeAnimator.UpdateOriginalScale();
        }
    
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
        else if (name.Contains("cylinder"))
            return "Cylinder";
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
    private void SetShapeDefaults()
    {
        // Set intelligent defaults based on shape type
        switch (shapeType)
        {
            case "Sphere":
                advancedData.emotionalColor = EmotionalColor.Green;
                advancedData.musicalRole = ShapeRole.Harmony;
                advancedData.materialType = MaterialType.Smooth;
                advancedData.animationType = AnimationType.Float;
                advancedData.intensity = 0.5f;
                advancedData.energy = 0.6f;
                advancedData.duration = 4f;
                Debug.Log("Applied Sphere defaults: Harmony role, Smooth material");
                break;
            
            case "Cube":
                advancedData.emotionalColor = EmotionalColor.Red;
                advancedData.musicalRole = ShapeRole.Rhythm;
                advancedData.materialType = MaterialType.Rough;
                advancedData.animationType = AnimationType.Bounce;
                advancedData.intensity = 0.7f;
                advancedData.energy = 0.8f;
                advancedData.duration = 2f; // Shorter for rhythmic elements
                Debug.Log("Applied Cube defaults: Rhythm role, Rough material");
                break;
            
            case "Cylinder":
                advancedData.emotionalColor = EmotionalColor.Purple;
                advancedData.musicalRole = ShapeRole.Bass;
                advancedData.materialType = MaterialType.Metallic;
                advancedData.animationType = AnimationType.Spin;
                advancedData.intensity = 0.6f;
                advancedData.energy = 0.4f; // Lower energy for bass
                advancedData.duration = 6f; // Longer sustain for bass
                Debug.Log("Applied Cylinder defaults: Bass role, Metallic material");
                break;
            
            default:
                // Fallback defaults
                advancedData.musicalRole = ShapeRole.Harmony;
                advancedData.materialType = MaterialType.Smooth;
                advancedData.animationType = AnimationType.None;
                break;
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
    // New method for updating emotional color
    private void ApplyEmotionalColorToRenderer()
    {
        Debug.Log($"=== ApplyEmotionalColorToRenderer START ===");
        Debug.Log($"GameObject: {gameObject.name}");
        Debug.Log($"Current emotional color: {advancedData.emotionalColor}");
    
        if (persistentMaterial != null)
        {
            Color renderColor = EmotionalColorUtility.GetEmotionalColor(advancedData.emotionalColor);
            Debug.Log($"Target render color: {renderColor}");
            Debug.Log($"Material shader: {persistentMaterial.shader.name}");
        
            // Apply the visual material properties INSTEAD of just setting color
            ApplyVisualMaterial(advancedData.materialType);
        
            Debug.Log($"Applied emotional color {advancedData.emotionalColor} with material {advancedData.materialType}");
        
            // Force the renderer to use the updated material
            Renderer renderer = GetComponent<Renderer>();
            renderer.material = persistentMaterial;
        
            Debug.Log($"Final material - Metallic: {persistentMaterial.GetFloat("_Metallic")}, Smoothness: {persistentMaterial.GetFloat("_Smoothness")}");
        }
        else
        {
            Debug.LogError($"persistentMaterial is NULL on {gameObject.name}!");
        }
    
        Debug.Log($"=== ApplyEmotionalColorToRenderer END ===");
    }
    
    public void UpdateEmotionalColor(EmotionalColor newEmotionalColor)
    {
        Debug.Log($"=== UpdateEmotionalColor START ===");
        Debug.Log($"GameObject: {gameObject.name}");
        Debug.Log($"OLD emotional color: {advancedData.emotionalColor}");
        Debug.Log($"NEW emotional color: {newEmotionalColor}");
    
        advancedData.emotionalColor = newEmotionalColor;
        Debug.Log($"Advanced data updated to: {advancedData.emotionalColor}");
    
        ApplyEmotionalColorToRenderer();
    
        Debug.Log($"=== UpdateEmotionalColor END ===");
    }
    
    // Keep this for backward compatibility but make it use emotional colors
    public void UpdateColor(Color newColor)
    {
        // Convert regular color to closest emotional color
        EmotionalColor closestEmotional = FindClosestEmotionalColor(newColor);
        UpdateEmotionalColor(closestEmotional);
    }
    
    private EmotionalColor FindClosestEmotionalColor(Color targetColor)
    {
        EmotionalColor closest = EmotionalColor.Red;
        float minDistance = float.MaxValue;
        
        foreach (EmotionalColor emotionalColor in System.Enum.GetValues(typeof(EmotionalColor)))
        {
            Color emotionalColorValue = EmotionalColorUtility.GetEmotionalColor(emotionalColor);
            float distance = Vector3.Distance(
                new Vector3(targetColor.r, targetColor.g, targetColor.b),
                new Vector3(emotionalColorValue.r, emotionalColorValue.g, emotionalColorValue.b)
            );
            
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = emotionalColor;
            }
        }
        
        return closest;
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
    [System.Serializable]
    public class AdvancedShapeData
    {
        [Header("Basic Properties")]
        public EmotionalColor emotionalColor = EmotionalColor.Red;
        public float size;
        public string shapeType;
    
        [Header("Spatial Properties")]
        public Vector3 position;
        public float height;
        public float distanceFromCenter;
    
        [Header("Timing Properties")]
        public float spawnTime;
        public float duration = 4f; // Default 4 beats
        public bool isLooping = true;
        public float beatOffset;
    
        [Header("Movement Properties")]
        public Vector3 velocity;
        public float rotationSpeed;
        public AnimationType animationType = AnimationType.None;
    
        [Header("Intensity Properties")]
        [Range(0f, 1f)] public float intensity = 0.5f;
        [Range(0f, 1f)] public float energy = 0.5f;
        public PulseType pulseType = PulseType.None;
    
        [Header("Material Properties")]
        public MaterialType materialType = MaterialType.Smooth;
        [Range(0f, 1f)] public float roughness = 0f;
        [Range(0f, 1f)] public float transparency = 0f;
        public bool isGlowing = false;
    
        [Header("Musical Properties")]
        public string groupID = "default";
        public ShapeRole musicalRole = ShapeRole.Harmony;
        public List<string> connectedShapeIDs;
        [Range(0f, 1f)] public float harmony = 0.5f;
    }
    public enum EmotionalColor
    {
        Red,        // Passion/Energy
        Orange,     // Joy/Creativity  
        Yellow,     // Happiness/Cheerful
        Green,      // Calm/Natural
        Blue,       // Sad/Melancholy
        Purple,     // Mysterious/Spiritual
        Brown,      // Earthy/Grounded
        Pink,       // Romantic/Gentle
        Black,      // Dark/Intense
        White,      // Pure/Minimal
        Gray,       // Neutral/Contemplative
        Teal        // Sophisticated/Modern
    }
    public enum AnimationType { None, Bounce, Spin, Float, Pulse }
    public enum PulseType { None, Slow, Fast, Irregular }
    public enum MaterialType { Smooth, Rough, Metallic, Glass, Wood, Fabric }
    public enum ShapeRole { Lead, Harmony, Rhythm, Bass, Percussion, Ambient }
    
    // New methods for advanced properties
    public void UpdateIntensity(float intensity)
    {
        advancedData.intensity = intensity;
        Debug.Log($"Updated {gameObject.name} intensity to: {intensity}");
        // Visual feedback could be added here (brightness, glow, etc.)
    }
    
    public void UpdateEnergy(float energy)
    {
        advancedData.energy = energy;
        Debug.Log($"Updated {gameObject.name} energy to: {energy}");
        // Could affect animation speed, particle effects, etc.
    }
    
    public void UpdateRoughness(float roughness)
    {
        advancedData.roughness = roughness;
        Debug.Log($"Updated {gameObject.name} roughness to: {roughness}");
        // Could affect material appearance
    }
    
    public void UpdateDuration(float duration)
    {
        advancedData.duration = duration;
        Debug.Log($"Updated {gameObject.name} duration to: {duration}");
    }
    
    public void UpdateMusicalRole(ShapeRole role)
    {
        Debug.Log($"UpdateMusicalRole called: OLD={advancedData.musicalRole}, NEW={role}");
        advancedData.musicalRole = role;
        Debug.Log($"Updated {gameObject.name} musical role to: {advancedData.musicalRole}");
    }
    
    public void UpdateMaterialType(MaterialType materialType)
    {
        Debug.Log($"UpdateMaterialType called: OLD={advancedData.materialType}, NEW={materialType}");
        advancedData.materialType = materialType;
    
        // Apply the visual material immediately
        ApplyVisualMaterial(materialType);
    
        Debug.Log($"Updated {gameObject.name} material to: {advancedData.materialType}");
    }
    
    public void UpdateLooping(bool isLooping)
    {
        advancedData.isLooping = isLooping;
        Debug.Log($"Updated {gameObject.name} looping to: {isLooping}");
    }
    
    // Enhanced data collection for music generation
    public AdvancedShapeData GetAdvancedShapeData()
    {
        // Update position data
        advancedData.position = transform.position;
        advancedData.height = transform.position.y;
        advancedData.distanceFromCenter = Vector3.Distance(transform.position, Vector3.zero);
        
        return advancedData;
    }

private void ApplyVisualMaterial(MaterialType materialType)
{
    if (persistentMaterial != null)
    {
        // Get current emotional color
        Color currentColor = EmotionalColorUtility.GetEmotionalColor(advancedData.emotionalColor);
        
        // Apply material-specific properties while keeping the emotional color
        switch (materialType)
        {
            case MaterialType.Smooth:
                ApplySmoothMaterial(currentColor);
                break;
            case MaterialType.Rough:
                ApplyRoughMaterial(currentColor);
                break;
            case MaterialType.Metallic:
                ApplyMetallicMaterial(currentColor);
                break;
            case MaterialType.Glass:
                ApplyGlassMaterial(currentColor);
                break;
            case MaterialType.Wood:
                ApplyWoodMaterial(currentColor);
                break;
            case MaterialType.Fabric:
                ApplyFabricMaterial(currentColor);
                break;
        }
        
        Debug.Log($"Applied visual material: {materialType} with color: {currentColor}");
    }
}

private void ApplySmoothMaterial(Color baseColor)
{
    // Smooth = Clean, matte finish
    persistentMaterial.SetColor("_BaseColor", baseColor);
    persistentMaterial.SetFloat("_Metallic", 0.0f);
    persistentMaterial.SetFloat("_Smoothness", 0.9f); // Very smooth
    
    // Disable emission for clean look
    persistentMaterial.SetColor("_EmissionColor", Color.black);
    persistentMaterial.DisableKeyword("_EMISSION");
}

private void ApplyRoughMaterial(Color baseColor)
{
    // Rough = Textured, matte, slightly darkened
    Color roughColor = baseColor * 0.8f; // Darken slightly
    persistentMaterial.SetColor("_BaseColor", roughColor);
    persistentMaterial.SetFloat("_Metallic", 0.0f);
    persistentMaterial.SetFloat("_Smoothness", 0.1f); // Very rough
    
    // Add slight emission for gritty look
    persistentMaterial.SetColor("_EmissionColor", baseColor * 0.1f);
    persistentMaterial.EnableKeyword("_EMISSION");
}

private void ApplyMetallicMaterial(Color baseColor)
{
    // Metallic = Shiny, reflective, bright
    Color metallicColor = Color.Lerp(baseColor, Color.white, 0.3f); // Brighten
    persistentMaterial.SetColor("_BaseColor", metallicColor);
    persistentMaterial.SetFloat("_Metallic", 1.0f); // Full metallic
    persistentMaterial.SetFloat("_Smoothness", 0.8f); // Very shiny
    
    // Bright emission for metallic shine
    persistentMaterial.SetColor("_EmissionColor", baseColor * 0.2f);
    persistentMaterial.EnableKeyword("_EMISSION");
}

private void ApplyGlassMaterial(Color baseColor)
{
    // Glass = Transparent, bright, crystal-like
    Color glassColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.7f); // Semi-transparent
    persistentMaterial.SetColor("_BaseColor", glassColor);
    persistentMaterial.SetFloat("_Metallic", 0.1f);
    persistentMaterial.SetFloat("_Smoothness", 1.0f); // Perfect smoothness
    
    // Set up transparency
    persistentMaterial.SetFloat("_Surface", 1); // Transparent surface
    persistentMaterial.SetFloat("_Blend", 0); // Alpha blend
    
    // Bright emission for glass clarity
    persistentMaterial.SetColor("_EmissionColor", baseColor * 0.3f);
    persistentMaterial.EnableKeyword("_EMISSION");
    
    // Update render queue for transparency
    persistentMaterial.renderQueue = 3000;
}

private void ApplyWoodMaterial(Color baseColor)
{
    // Wood = Warm, organic, slightly rough
    Color woodColor = Color.Lerp(baseColor, new Color(0.6f, 0.4f, 0.2f, 1f), 0.3f); // Warm tint
    persistentMaterial.SetColor("_BaseColor", woodColor);
    persistentMaterial.SetFloat("_Metallic", 0.0f);
    persistentMaterial.SetFloat("_Smoothness", 0.4f); // Moderate smoothness
    
    // Warm, subtle emission
    persistentMaterial.SetColor("_EmissionColor", woodColor * 0.1f);
    persistentMaterial.EnableKeyword("_EMISSION");
}

private void ApplyFabricMaterial(Color baseColor)
{
    // Fabric = Soft, muted, matte
    Color fabricColor = baseColor * 0.7f; // Mute the color
    persistentMaterial.SetColor("_BaseColor", fabricColor);
    persistentMaterial.SetFloat("_Metallic", 0.0f);
    persistentMaterial.SetFloat("_Smoothness", 0.2f); // Low smoothness
    
    // Very subtle emission for soft glow
    persistentMaterial.SetColor("_EmissionColor", fabricColor * 0.05f);
    persistentMaterial.EnableKeyword("_EMISSION");
}
}