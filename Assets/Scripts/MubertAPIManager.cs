using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using TMPro;

public class MubertAPIManager : MonoBehaviour
{
    [Header("Mubert API Configuration")]
    [SerializeField] private string apiKey = "YOUR_API_KEY_HERE";
    [SerializeField] private string baseURL = "https://api-b2b.mubert.com/v2/";
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    
    // Singleton pattern
    public static MubertAPIManager Instance;
    
    [Header("Debug Panel")]
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI compositionText;
    [SerializeField] private TextMeshProUGUI requestText;
    [SerializeField] private TextMeshProUGUI responseText;
    [SerializeField] private TextMeshProUGUI progressText;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Get or create AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        // Initialize debug panel
        InitializeDebugPanel();
    }
    private void InitializeDebugPanel()
    {
        if (debugPanel != null)
        {
            UpdateDebugStatus("Ready", Color.white);
            UpdateDebugComposition("No shapes analyzed yet");
            UpdateDebugRequest("No request sent yet");
            UpdateDebugResponse("No response received yet");
            UpdateDebugProgress("Idle");
        }
    }
    
    // Debug panel update methods
    private void UpdateDebugStatus(string status, Color color)
    {
        if (statusText != null)
        {
            statusText.text = $"Status: {status}";
            statusText.color = color;
        }
        Debug.Log($"[STATUS] {status}");
    }
    
    private void UpdateDebugComposition(string composition)
    {
        if (compositionText != null)
        {
            compositionText.text = $"Composition:\n{composition}";
        }
        Debug.Log($"[COMPOSITION] {composition}");
    }
    
    private void UpdateDebugRequest(string request)
    {
        if (requestText != null)
        {
            requestText.text = $"Request:\n{request}";
        }
        Debug.Log($"[REQUEST] {request}");
    }
    
    private void UpdateDebugResponse(string response)
    {
        if (responseText != null)
        {
            responseText.text = $"Response:\n{response}";
        }
        Debug.Log($"[RESPONSE] {response}");
    }
    
    private void UpdateDebugProgress(string progress)
    {
        if (progressText != null)
        {
            progressText.text = $"Progress: {progress}";
        }
        Debug.Log($"[PROGRESS] {progress}");
    }
    
    // Show/Hide debug panel
    public void ShowDebugPanel(bool show)
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(show);
        }
    }
    // Main method to generate music from shapes
    public void GenerateMusicFromShapes()
    {
        Debug.Log("=== Generating Music from Shapes ===");
        ShowDebugPanel(true);
        UpdateDebugStatus("Starting Analysis...", Color.yellow);
        UpdateDebugProgress("Analyzing shapes in scene");
        
        // Collect all shapes in the scene
        SelectableShape[] allShapes = FindObjectsOfType<SelectableShape>();
        
        if (allShapes.Length == 0)
        {
            UpdateDebugStatus("Error: No Shapes Found", Color.red);
            UpdateDebugProgress("Failed - No shapes to analyze");
            Debug.LogWarning("No shapes found to generate music from!");
            return;
        }
        
        UpdateDebugProgress($"Found {allShapes.Length} shapes, analyzing...");
        
        // Analyze the composition
        CompositionData composition = AnalyzeShapeComposition(allShapes);
        
        // Update debug panel with composition info
        string compositionInfo = FormatCompositionInfo(composition, allShapes);
        UpdateDebugComposition(compositionInfo);
        
        UpdateDebugStatus("Sending API Request...", Color.yellow);
        UpdateDebugProgress("Preparing API request to Mubert");
        
        // Generate music
        StartCoroutine(RequestMusicGeneration(composition));
    }
    private string FormatCompositionInfo(CompositionData composition, SelectableShape[] shapes)
    {
        StringBuilder info = new StringBuilder();
        
        info.AppendLine($"Shapes: {shapes.Length}");
        info.AppendLine($"Primary Mood: {composition.primaryMood}");
        info.AppendLine($"Energy: {composition.energy:F2}");
        info.AppendLine($"Calmness: {composition.calmness:F2}");
        info.AppendLine($"Darkness: {composition.darkness:F2}");
        info.AppendLine($"Happiness: {composition.happiness:F2}");
        info.AppendLine($"Intensity: {composition.intensity:F2}");
        info.AppendLine($"Complexity: {composition.complexity:F2}");
        
        info.AppendLine("\nShape Details:");
        foreach (SelectableShape shape in shapes)
        {
            info.AppendLine($"• {shape.shapeType}: {shape.emotionalColor} {shape.advancedData.materialType}");
            info.AppendLine($"  Size: {shape.shapeSize:F1}, Anim: {shape.advancedData.animationType}");
        }
        
        return info.ToString();
    }
     private CompositionData AnalyzeShapeComposition(SelectableShape[] shapes)
    {
        UpdateDebugProgress("Calculating shape influences...");
        
        CompositionData composition = new CompositionData();
        
        // Calculate weighted influence of each shape
        float totalWeight = 0f;
        List<ShapeInfluence> influences = new List<ShapeInfluence>();
        
        foreach (SelectableShape shape in shapes)
        {
            ShapeInfluence influence = new ShapeInfluence();
            influence.shapeData = shape.GetAdvancedShapeData();
            influence.weight = shape.shapeSize * shape.shapeSize; // Larger shapes = more influence
            influences.Add(influence);
            totalWeight += influence.weight;
        }
        
        UpdateDebugProgress("Normalizing weights and blending moods...");
        
        // Normalize weights
        for (int i = 0; i < influences.Count; i++)
        {
            var influence = influences[i];
            influence.weight /= totalWeight;
            influences[i] = influence;
        }
        
        // Blend the moods based on weights
        composition = BlendComposition(influences);
        
        UpdateDebugProgress("Composition analysis complete");
        
        Debug.Log($"Composition Analysis: Primary Mood: {composition.primaryMood}, Energy: {composition.energy:F2}, Complexity: {composition.complexity:F2}");
        
        return composition;
    }
    
    private CompositionData BlendComposition(List<ShapeInfluence> influences)
    {
        CompositionData composition = new CompositionData();
        
        // Blend emotional colors
        float totalEnergy = 0f;
        float totalCalmness = 0f;
        float totalDarkness = 0f;
        float totalHappiness = 0f;
        float totalIntensity = 0f;
        
        foreach (ShapeInfluence influence in influences)
        {
            MoodContribution mood = GetMoodContribution(influence.shapeData.emotionalColor);
            
            totalEnergy += mood.energy * influence.weight;
            totalCalmness += mood.calmness * influence.weight;
            totalDarkness += mood.darkness * influence.weight;
            totalHappiness += mood.happiness * influence.weight;
            totalIntensity += influence.shapeData.intensity * influence.weight;
        }
        
        // Determine primary mood
        composition.energy = totalEnergy;
        composition.calmness = totalCalmness;
        composition.darkness = totalDarkness;
        composition.happiness = totalHappiness;
        composition.intensity = totalIntensity;
        composition.complexity = Mathf.Min(influences.Count / 5f, 1f); // More shapes = more complex
        
        // Determine primary mood from highest value
        if (totalEnergy > 0.6f && totalDarkness < 0.3f)
            composition.primaryMood = "energetic";
        else if (totalCalmness > 0.6f)
            composition.primaryMood = "calm";
        else if (totalDarkness > 0.6f)
            composition.primaryMood = "dark";
        else if (totalHappiness > 0.6f)
            composition.primaryMood = "happy";
        else
            composition.primaryMood = "balanced";
            
        return composition;
    }
    
     private MoodContribution GetMoodContribution(SelectableShape.EmotionalColor color)
    {
        switch (color)
        {
            case SelectableShape.EmotionalColor.Red:
                return new MoodContribution { energy = 1.0f, happiness = 0.6f, darkness = 0.2f, calmness = 0.0f };
            case SelectableShape.EmotionalColor.Orange:
                return new MoodContribution { energy = 0.8f, happiness = 0.9f, darkness = 0.0f, calmness = 0.2f };
            case SelectableShape.EmotionalColor.Yellow:
                return new MoodContribution { energy = 0.7f, happiness = 1.0f, darkness = 0.0f, calmness = 0.3f };
            case SelectableShape.EmotionalColor.Green:
                return new MoodContribution { energy = 0.3f, happiness = 0.6f, darkness = 0.1f, calmness = 1.0f };
            case SelectableShape.EmotionalColor.Blue:
                return new MoodContribution { energy = 0.2f, happiness = 0.2f, darkness = 0.6f, calmness = 0.8f };
            case SelectableShape.EmotionalColor.Purple:
                return new MoodContribution { energy = 0.5f, happiness = 0.3f, darkness = 0.7f, calmness = 0.4f };
            case SelectableShape.EmotionalColor.Black:
                return new MoodContribution { energy = 0.8f, happiness = 0.0f, darkness = 1.0f, calmness = 0.0f };
            case SelectableShape.EmotionalColor.White:
                return new MoodContribution { energy = 0.3f, happiness = 0.7f, darkness = 0.0f, calmness = 0.9f };
            default:
                return new MoodContribution { energy = 0.5f, happiness = 0.5f, darkness = 0.5f, calmness = 0.5f };
        }
    }
    
    private IEnumerator RequestMusicGeneration(CompositionData composition)
    {
        UpdateDebugProgress("Building API request...");
        
        // Create the request payload
        MubertRequest request = new MubertRequest
        {
            method = "GenerateTrackBy",
            @params = new MubertParams
            {
                mode = composition.primaryMood,
                duration = 30, // 30 seconds for prototype
                format = "mp3",
                intensity = Mathf.RoundToInt(composition.intensity * 10), // 0-10 scale
                api_key = apiKey
            }
        };
        
        string jsonData = JsonUtility.ToJson(request, true); // Pretty print
        UpdateDebugRequest(jsonData);
        UpdateDebugProgress("Sending HTTP request to Mubert...");
        
        // Send HTTP request
        using (UnityWebRequest webRequest = new UnityWebRequest(baseURL + "GenerateTrackBy", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            // Show progress while sending
            while (!webRequest.isDone)
            {
                UpdateDebugProgress($"Sending request... {webRequest.uploadProgress * 100:F0}%");
                yield return null;
            }
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                UpdateDebugStatus("API Request Successful", Color.green);
                string responseText = webRequest.downloadHandler.text;
                UpdateDebugResponse(responseText);
                UpdateDebugProgress("Parsing API response...");
                
                // Parse response and download audio
                MubertResponse response = JsonUtility.FromJson<MubertResponse>(responseText);
                
                if (response.status == 1 && !string.IsNullOrEmpty(response.data.link))
                {
                    UpdateDebugProgress("Starting audio download...");
                    StartCoroutine(DownloadAndPlayAudio(response.data.link));
                }
                else
                {
                    UpdateDebugStatus("API Error", Color.red);
                    UpdateDebugResponse($"Error: {response.error.text}");
                    UpdateDebugProgress("Failed - API returned error");
                }
            }
            else
            {
                UpdateDebugStatus("Network Error", Color.red);
                UpdateDebugResponse($"Network Error: {webRequest.error}");
                UpdateDebugProgress("Failed - Network error occurred");
            }
        }
    }
    
    private IEnumerator DownloadAndPlayAudio(string audioURL)
    {
        UpdateDebugProgress("Downloading audio file...");
        
        using (UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip(audioURL, AudioType.MPEG))
        {
            while (!audioRequest.isDone)
            {
                UpdateDebugProgress($"Downloading audio... {audioRequest.downloadProgress * 100:F0}%");
                yield return null;
            }
            
            yield return audioRequest.SendWebRequest();
            
            if (audioRequest.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(audioRequest);
                
                if (clip != null)
                {
                    UpdateDebugStatus("Music Generated Successfully!", Color.green);
                    UpdateDebugProgress("Playing generated music...");
                    audioSource.clip = clip;
                    audioSource.Play();
                }
                else
                {
                    UpdateDebugStatus("Audio Processing Error", Color.red);
                    UpdateDebugProgress("Failed - Could not create AudioClip");
                }
            }
            else
            {
                UpdateDebugStatus("Audio Download Error", Color.red);
                UpdateDebugResponse($"Audio Error: {audioRequest.error}");
                UpdateDebugProgress("Failed - Audio download failed");
            }
        }
    }
    
    // Test method without API
    public void TestCompositionAnalysis()
    {
        ShowDebugPanel(true);
        UpdateDebugStatus("Testing Composition Analysis", Color.cyan);
        
        SelectableShape[] allShapes = FindObjectsOfType<SelectableShape>();
        
        if (allShapes.Length == 0)
        {
            UpdateDebugStatus("No Shapes Found", Color.red);
            UpdateDebugComposition("No shapes in scene to analyze");
            return;
        }
        
        CompositionData composition = AnalyzeShapeComposition(allShapes);
        string compositionInfo = FormatCompositionInfo(composition, allShapes);
        
        UpdateDebugStatus("Test Complete", Color.green);
        UpdateDebugComposition(compositionInfo);
        UpdateDebugRequest("Test mode - No API request sent");
        UpdateDebugResponse("Test mode - No API response");
        UpdateDebugProgress("Test completed successfully");
    }
}

// Data structures for API communication
[System.Serializable]
public class MubertRequest
{
    public string method;
    public MubertParams @params;
}

[System.Serializable]
public class MubertParams
{
    public string mode;
    public int duration;
    public string format;
    public int intensity;
    public string api_key;
}

[System.Serializable]
public class MubertResponse
{
    public int status;
    public MubertData data;
    public MubertError error;
}

[System.Serializable]
public class MubertData
{
    public string link;
    public string track_id;
}

[System.Serializable]
public class MubertError
{
    public string text;
}

// Supporting data structures
[System.Serializable]
public class CompositionData
{
    public string primaryMood;
    public float energy;
    public float calmness;
    public float darkness;
    public float happiness;
    public float intensity;
    public float complexity;
}

[System.Serializable]
public class ShapeInfluence
{
    public SelectableShape.AdvancedShapeData shapeData;
    public float weight;
}

[System.Serializable]
public class MoodContribution
{
    public float energy;
    public float happiness;
    public float darkness;
    public float calmness;
}