using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoBallController : MonoBehaviour
{
    [Header("Disco Ball Settings")]
    [SerializeField] private Transform discoBall;
    [SerializeField] private Transform hiddenPositionTarget;
    [SerializeField] private Transform showPositionTarget;
    [SerializeField] private float dropDuration = 2f;
    [SerializeField] private float rotationSpeed = 30f;
    
    private bool isSpinning = false;
    
    void Update()
    {
        // Rotate disco ball when spinning
        if (isSpinning && discoBall != null)
        {
            discoBall.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
    
    public void ActivateDiscoBall()
    {
        if (discoBall != null && showPositionTarget != null)
        {
            discoBall.gameObject.SetActive(true);
            StartCoroutine(DropAndSpin());
        }
    }
    
    public void DeactivateDiscoBall()
    {
        if (discoBall != null && hiddenPositionTarget != null)
        {
            isSpinning = false;
            StartCoroutine(RaiseAndHide());
        }
    }
    
    private IEnumerator DropAndSpin()
    {
        // Drop down animation
        float elapsed = 0f;
        Vector3 startPos = hiddenPositionTarget.position;
        Vector3 targetPos = showPositionTarget.position;
        
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / dropDuration;
            
            // Smooth drop with easing
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            discoBall.position = Vector3.Lerp(startPos, targetPos, easedProgress);
            
            yield return null;
        }
        
        discoBall.position = targetPos;
        isSpinning = true; // Start spinning
    }
    
    private IEnumerator RaiseAndHide()
    {
        // Raise up animation
        float elapsed = 0f;
        Vector3 startPos = discoBall.position;
        Vector3 targetPos = hiddenPositionTarget.position;
        
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / dropDuration;
            
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            discoBall.position = Vector3.Lerp(startPos, targetPos, easedProgress);
            
            yield return null;
        }
        
        discoBall.position = targetPos;
        discoBall.gameObject.SetActive(false);
    }
}
