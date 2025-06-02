using UnityEngine;

public class ShapeSelector : MonoBehaviour 
{
    [SerializeField] private Transform raycastOrigin; // Assign your controller/hand
    [SerializeField] private float rayDistance = 10f;
    [SerializeField] private LayerMask shapeLayer = -1;
    
    private SelectableShape currentHoveredShape;
    private SelectableShape currentSelectedShape;
    
    void Update()
    {
        PerformRaycast();
        CheckForSelection();
    }
    
    private void PerformRaycast()
    {
        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, rayDistance, shapeLayer))
        {
            SelectableShape shape = hit.collider.GetComponent<SelectableShape>();
            
            if (shape != null)
            {
                // New shape being hovered
                if (currentHoveredShape != shape)
                {
                    // Clear previous hover
                    if (currentHoveredShape != null)
                        currentHoveredShape.OnHoverExit();
                    
                    // Set new hover
                    currentHoveredShape = shape;
                    currentHoveredShape.OnHover();
                }
            }
            else
            {
                // No shape hit, clear hover
                ClearHover();
            }
        }
        else
        {
            // No hit, clear hover
            ClearHover();
        }
        
        // Debug ray
        Debug.DrawRay(raycastOrigin.position, raycastOrigin.forward * rayDistance, Color.red);
    }
    
    private void ClearHover()
    {
        if (currentHoveredShape != null)
        {
            currentHoveredShape.OnHoverExit();
            currentHoveredShape = null;
        }
    }
    
    private void CheckForSelection()
    {
        // Check for trigger press (controller) or pinch (hand)
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) || 
            OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            if (currentHoveredShape != null)
            {
                currentHoveredShape.OnSelect();
                currentSelectedShape = currentHoveredShape;
            }
        }
    }
}