using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
public class ShapeSpawner : MonoBehaviour 
{
    [SerializeField] private GameObject spherePrefab;
    [SerializeField] private GameObject cubePrefab;
        

    void Start() 
    {
        // Debug what interaction objects exist
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Hand") || obj.name.Contains("Controller"))
            {
                Debug.Log("Found: " + obj.name + " at " + obj.transform.position);
            }
        }
    }
    
    // Auto-find the active interactor
    public void SpawnSphere() 
    {
        Transform interactor = FindActiveInteractor();
        if (interactor != null)
        {
            Vector3 spawnPos = interactor.position + interactor.forward * 0.2f;
            GameObject newSphere = Instantiate(spherePrefab, spawnPos, Quaternion.identity);
            Debug.Log("Spawning sphere");
            // Ensure it has the selectable component
            if (newSphere.GetComponent<SelectableShape>() == null)
            {
                newSphere.AddComponent<SelectableShape>();
            }
        }
    }
    
    public void SpawnCube() 
    {
        Transform interactor = FindActiveInteractor();
        if (interactor != null)
        {
            Vector3 spawnPos = interactor.position + interactor.forward * 0.2f;
            GameObject newSphere = Instantiate(cubePrefab, spawnPos, Quaternion.identity);
            Debug.Log("Spawning cube");

            // Ensure it has the selectable component
            if (newSphere.GetComponent<SelectableShape>() == null)
            {
                newSphere.AddComponent<SelectableShape>();
            }
        }
    }
    
    private Transform FindActiveInteractor()
    {
        // Look for hand or controller objects
        GameObject leftHand = GameObject.Find("LeftHandAnchor") ?? GameObject.Find("LeftHand");
        GameObject rightHand = GameObject.Find("RightHandAnchor") ?? GameObject.Find("RightHand");
        GameObject leftController = GameObject.Find("LeftControllerAnchor");
        GameObject rightController = GameObject.Find("RightControllerAnchor");
        
        // Return whichever is active and closest to the UI
        if (rightHand != null && rightHand.activeInHierarchy) return rightHand.transform;
        if (leftHand != null && leftHand.activeInHierarchy) return leftHand.transform;
        if (rightController != null && rightController.activeInHierarchy) return rightController.transform;
        if (leftController != null && leftController.activeInHierarchy) return leftController.transform;
        
        return null;
    }
}
