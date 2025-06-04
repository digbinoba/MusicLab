using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Add this to SelectableShape
public class ShapeAnimator : MonoBehaviour
{
    [SerializeField] private SelectableShape selectableShape;
    [SerializeField] private float animationSpeed = 1f;
    [SerializeField] private float animationIntensity = 0.1f;
    
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private SelectableShape.AnimationType currentAnimationType = SelectableShape.AnimationType.None;
    private bool initialized = false;
    
    // NEW: Position tracking for grab detection
    private Vector3 lastKnownPosition;
    private float positionCheckTimer = 0f;
    private const float POSITION_CHECK_INTERVAL = 0.1f; // Check every 0.1 seconds
    private const float MOVEMENT_THRESHOLD = 0.01f; // How much movement to consider "moved"

    
    void Start()
    {
        if (selectableShape == null)
            selectableShape = GetComponent<SelectableShape>();
            
       // StoreOriginalTransform();
    }
    
    // This will be called by SelectableShape after everything is set up
    public void Initialize()
    {
        Debug.Log($"=== ShapeAnimator Initialize on {gameObject.name} ===");
        Debug.Log($"Current transform position: {transform.position}");
        Debug.Log($"Current transform scale: {transform.localScale}");
        
        StoreOriginalTransform();
        lastKnownPosition = transform.position; // Track starting position
        initialized = true;
        
        Debug.Log($"Stored original position: {originalPosition}");
        Debug.Log($"Stored original scale: {originalScale}");
    }
    private void StoreOriginalTransform()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
    }
    
    public void SetAnimationType(SelectableShape.AnimationType animType)
    {
        Debug.Log($"ShapeAnimator: Setting animation to {animType} on {gameObject.name}");
        
        // If we're changing animation type, update the original position to current position
        if (initialized)
        {
            originalPosition = GetBasePosition();
            lastKnownPosition = originalPosition;
        }
        
        currentAnimationType = animType;
    }
    
    private void ResetToOriginal()
    {
        transform.position = originalPosition;
        transform.localScale = originalScale;
        transform.rotation = originalRotation;
    }
    
    void Update()
    {
        // Don't animate until properly initialized
        if (!initialized) return;
        // Check if shape has been moved by user
        CheckForPositionChange();
        // Use the controlled animation type instead of directly accessing selectableShape
        switch (currentAnimationType)
        {
            case SelectableShape.AnimationType.Float:
                AnimateFloat();
                break;
            case SelectableShape.AnimationType.Bounce:
                AnimateBounce();
                break;
            case SelectableShape.AnimationType.Spin:
                AnimateSpin();
                break;
            case SelectableShape.AnimationType.Pulse:
                AnimatePulse();
                break;
            case SelectableShape.AnimationType.None:
            default:
                // Reset to original position for "None" animation
                transform.position = originalPosition;
                transform.localScale = originalScale;
                transform.rotation = originalRotation;
                break;
        }
    }
    private void CheckForPositionChange()
    {
        positionCheckTimer += Time.deltaTime;
        
        if (positionCheckTimer >= POSITION_CHECK_INTERVAL)
        {
            positionCheckTimer = 0f;
            
            // Calculate distance moved (ignoring animation movement)
            Vector3 basePosition = GetBasePosition(); // Position without animation
            float distanceMoved = Vector3.Distance(basePosition, lastKnownPosition);
            
            if (distanceMoved > MOVEMENT_THRESHOLD)
            {
                Debug.Log($"Shape {gameObject.name} was moved! Updating animation center.");
                Debug.Log($"Old center: {lastKnownPosition}, New center: {basePosition}");
                
                // Update the original position to the new location
                originalPosition = basePosition;
                lastKnownPosition = basePosition;
            }
        }
    }
    private Vector3 GetBasePosition()
    {
        // For animations that modify position, we need to figure out the "base" position
        switch (currentAnimationType)
        {
            case SelectableShape.AnimationType.Float:
            case SelectableShape.AnimationType.Bounce:
                // For vertical animations, the base is the current position minus animation offset
                return new Vector3(transform.position.x, originalPosition.y, transform.position.z);
            
            case SelectableShape.AnimationType.Spin:
            case SelectableShape.AnimationType.Pulse:
            case SelectableShape.AnimationType.None:
            default:
                // For these animations, current position IS the base position
                return transform.position;
        }
    }
    
    private void AnimateFloat()
    {
        float offset = Mathf.Sin(Time.time * animationSpeed) * animationIntensity;
        transform.position = originalPosition + Vector3.up * offset;
    }
    
    private void AnimateBounce()
    {
        float bounce = Mathf.Abs(Mathf.Sin(Time.time * animationSpeed * 2f)) * animationIntensity;
        transform.position = originalPosition + Vector3.up * bounce;
    }
    
    private void AnimateSpin()
    {
        transform.position = originalPosition;
        transform.Rotate(Vector3.up, animationSpeed * 50f * Time.deltaTime);    }
    
    private void AnimatePulse()
    {
        float pulse = 1f + Mathf.Sin(Time.time * animationSpeed) * 0.1f;
        transform.position = originalPosition;
        transform.localScale = originalScale * pulse;
    }
    
    // Method to update original position when shape is moved
    public void UpdateOriginalPosition()
    {
        if (initialized)
        {
            originalPosition = transform.position;
            lastKnownPosition = originalPosition;
            Debug.Log($"Manually updated original position to: {originalPosition}");
        }
    }
    
    // Method to update original scale when shape is resized
    public void UpdateOriginalScale()
    {
        if (initialized)
        {
            originalScale = transform.localScale;
            Debug.Log($"Updated original scale to: {originalScale}");
        }
    }
    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = speed;
    }
    
    public void SetAnimationIntensity(float intensity)
    {
        animationIntensity = intensity;
    }
}