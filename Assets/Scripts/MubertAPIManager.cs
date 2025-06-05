using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using TMPro;

public class MubertAPIManager : MonoBehaviour
{
    [Header("Mubert API Configuration")]
    [SerializeField] private string accessToken = "YOUR_ACCESS_TOKEN_HERE";
    [SerializeField] private string baseURL = "https://music-api.mubert.com/api/v3/public/tracks";
    [SerializeField] private string customerID = "YOUR_CUSTOMER_ID_HERE";
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    
    [Header("Analysis Components")]
    [SerializeField] private CompositionAnalyzer compositionAnalyzer;
    [SerializeField] private PromptGenerator promptGenerator;
    [SerializeField] private bool enableEnhancedLogging = true;
    
    [Header("Prompt Generation Settings")]
    [SerializeField] private bool useSimplePrompts = false;
    [SerializeField] private bool testPromptsOnly = false;
    [SerializeField] private int maxPromptLength = 200; // Mubert limit is 200 characters
    
    [Header("Track Generation Settings")]
    [SerializeField] private int defaultBitrate = 128;
    [SerializeField] private string defaultIntensity = "medium";
    [SerializeField] private string defaultMode = "track"; // "track" or "loop"
    
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
        InitializeComponents();
        InitializeDebugPanel();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D)) DebugCurrentSettings();
        if (Input.GetKeyDown(KeyCode.N)) TestNetworkConnectivity();
        if (Input.GetKeyDown(KeyCode.G)) GenerateMusicFromShapes();
        if (Input.GetKeyDown(KeyCode.P)) // Press P for POST test
        {
            TestSimplePOST();
        }
    }
    private void InitializeComponents()
    {
        // Initialize audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Initialize composition analyzer
        if (compositionAnalyzer == null)
        {
            compositionAnalyzer = FindObjectOfType<CompositionAnalyzer>();
            if (compositionAnalyzer == null)
            {
                GameObject analyzerGO = new GameObject("CompositionAnalyzer");
                compositionAnalyzer = analyzerGO.AddComponent<CompositionAnalyzer>();
                Debug.Log("Created CompositionAnalyzer automatically");
            }
        }
        
        // Initialize prompt generator
        if (promptGenerator == null)
        {
            promptGenerator = FindObjectOfType<PromptGenerator>();
            if (promptGenerator == null)
            {
                GameObject promptGO = new GameObject("PromptGenerator");
                promptGenerator = promptGO.AddComponent<PromptGenerator>();
                Debug.Log("Created PromptGenerator automatically");
            }
        }
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
    
    // Debug methods
    private void UpdateDebugStatus(string status, Color color)
    {
        if (statusText != null)
        {
            statusText.text = $"Status: {status}";
            statusText.color = color;
        }
        if (enableEnhancedLogging) Debug.Log($"[MUBERT STATUS] {status}");
    }
    
    private void UpdateDebugComposition(string composition)
    {
        if (compositionText != null)
        {
            compositionText.text = $"Composition:\n{composition}";
        }
        if (enableEnhancedLogging) Debug.Log($"[MUBERT COMPOSITION] {composition}");
    }
    
    private void UpdateDebugRequest(string request)
    {
        if (requestText != null)
        {
            requestText.text = $"Request:\n{request}";
        }
        if (enableEnhancedLogging) Debug.Log($"[MUBERT REQUEST] {request}");
    }
    
    private void UpdateDebugResponse(string response)
    {
        if (responseText != null)
        {
            responseText.text = $"Response:\n{response}";
        }
        if (enableEnhancedLogging) Debug.Log($"[MUBERT RESPONSE] {response}");
    }
    
    private void UpdateDebugProgress(string progress)
    {
        if (progressText != null)
        {
            progressText.text = $"Progress: {progress}";
        }
        if (enableEnhancedLogging) Debug.Log($"[MUBERT PROGRESS] {progress}");
    }
    
    public void ShowDebugPanel(bool show)
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(show);
        }
    }
    
    // Main method for generating music from shapes
    public void GenerateMusicFromShapes()
    {
        if (enableEnhancedLogging) Debug.Log("=== GENERATING MUSIC FROM SHAPES WITH PROMPTS ===");
        ShowDebugPanel(true);
        UpdateDebugStatus("Starting Enhanced Analysis with Prompt Generation...", Color.yellow);
        UpdateDebugProgress("Collecting shapes from scene");
        
        SelectableShape[] allShapes = FindObjectsOfType<SelectableShape>();
        
        if (allShapes.Length == 0)
        {
            UpdateDebugStatus("Error: No Shapes Found", Color.red);
            UpdateDebugProgress("Failed - No shapes to analyze");
            Debug.LogWarning("No shapes found to generate music from!");
            return;
        }
        
        UpdateDebugProgress($"Found {allShapes.Length} shapes, analyzing composition...");
        
        // Perform composition analysis
        CompositionData composition = compositionAnalyzer.PerformComprehensiveAnalysis(allShapes);
        
        // Generate music prompt
        UpdateDebugProgress("Generating music prompt from composition data...");
        string musicPrompt = GenerateMusicPrompt(composition, allShapes);
        
        // Display analysis and prompt
        string compositionInfo = compositionAnalyzer.FormatDetailedCompositionInfo(composition, allShapes);
        string fullInfo = compositionInfo + "\n\n=== GENERATED PROMPT ===\n" + musicPrompt;
        UpdateDebugComposition(fullInfo);
        
        if (testPromptsOnly)
        {
            UpdateDebugStatus("Test Mode: Prompt Generated", Color.cyan);
            UpdateDebugProgress("Test mode - showing prompt only, no API call");
            UpdateDebugRequest($"Generated Prompt: {musicPrompt}");
            UpdateDebugResponse("Test mode - no API response");
            return;
        }
        
        UpdateDebugStatus("Building API Request with Prompt...", Color.yellow);
        UpdateDebugProgress("Creating Mubert API request with generated prompt");
        
        // Generate music with prompt
        StartCoroutine(RequestMusicGenerationWithPrompt(composition, allShapes, musicPrompt));
    }
    
    private string GenerateMusicPrompt(CompositionData composition, SelectableShape[] shapes)
    {
        string prompt;
        if (useSimplePrompts)
        {
            prompt = promptGenerator.GenerateSimplePrompt(composition);
        }
        else
        {
            prompt = promptGenerator.GenerateMusicPrompt(composition, shapes);
        }
        
        // Truncate prompt if it exceeds the limit
        if (prompt.Length > maxPromptLength)
        {
            prompt = prompt.Substring(0, maxPromptLength - 3) + "...";
            Debug.LogWarning($"Prompt truncated to {maxPromptLength} characters: {prompt}");
        }
        
        return prompt;
    }
    
    private IEnumerator RequestMusicGenerationWithPrompt(CompositionData composition, SelectableShape[] shapes, string musicPrompt)
    {
        UpdateDebugProgress("Building text-to-music API request...");
        
        // Create the correct text-to-music request structure
        TextToMusicRequest request = new TextToMusicRequest
        {
            playlist_index = "1.0.0",
            prompt = musicPrompt,
            bitrate = defaultBitrate,
            duration = DetermineOptimalDuration(composition, shapes.Length),
            format = "mp3",
            intensity = DetermineIntensityLevel(composition.intensity),
            mode = composition.hasLoopingElements ? "loop" : defaultMode,
            bpm = 120, // You can make this dynamic later based on composition.tempo
            key = "C#"  // You can make this dynamic later based on composition
        };
        
        string jsonData = JsonUtility.ToJson(request, true);
        
        // COMPREHENSIVE LOGGING
        LogDetailedTextToMusicRequest(request, jsonData);
        UpdateDebugRequest(FormatTextToMusicRequestSummary(request));
        
        UpdateDebugProgress("Sending text-to-music request to Mubert API...");
        
        // Send the request with correct endpoint and headers
        using (UnityWebRequest webRequest = new UnityWebRequest(baseURL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            
            // Set the correct headers as per Mubert API documentation
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("customer-id", customerID);
            webRequest.SetRequestHeader("access-token", accessToken);
            webRequest.timeout = 30;
            
            if (enableEnhancedLogging)
            {
                Debug.Log("=== SENDING POST TO MUBERT ===");
                Debug.Log($"URL: {baseURL}");
                Debug.Log($"Headers: customer-id: {customerID}");
                Debug.Log($"Headers: access-token: {accessToken}");
                Debug.Log($"Body: {jsonData}");
            }
            
            // Progress tracking
            // while (!webRequest.isDone)
            // {
            //     UpdateDebugProgress($"Sending request... {webRequest.uploadProgress * 100:F0}%");
            //     yield return null;
            // }
            
            yield return webRequest.SendWebRequest();
            
            // IMMEDIATE STATUS CODE DEBUGGING
            Debug.Log("=== IMMEDIATE POST-REQUEST STATUS ===");
            Debug.Log($"Request completed: {webRequest.isDone}");
            Debug.Log($"Response code: {webRequest.responseCode}");
            Debug.Log($"Result status: {webRequest.result}");
            Debug.Log($"Error message: {(string.IsNullOrEmpty(webRequest.error) ? "None" : webRequest.error)}");
            Debug.Log($"Download progress: {webRequest.downloadProgress}");
            Debug.Log($"Upload progress: {webRequest.uploadProgress}");

            // Check if we have any response data at all
            if (webRequest.downloadHandler != null)
            {
                Debug.Log($"Has download handler: YES");
                Debug.Log($"Download handler isDone: {webRequest.downloadHandler.isDone}");
                Debug.Log($"Raw response text length: {webRequest.downloadHandler.text?.Length ?? 0}");
                Debug.Log($"Raw response data length: {webRequest.downloadHandler.data?.Length ?? 0}");
    
                if (!string.IsNullOrEmpty(webRequest.downloadHandler.text))
                {
                    Debug.Log($"RAW RESPONSE TEXT: {webRequest.downloadHandler.text}");
                }
                else
                {
                    Debug.Log("Response text is null or empty");
                }
            }
            else
            {
                Debug.Log("Download handler is NULL");
            }

            Debug.Log("=== END IMMEDIATE STATUS ===");
            // ENHANCED RESPONSE LOGGING
            LogDetailedResponse(webRequest);
            
            // Handle response
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                HandleSuccessfulResponse(webRequest.downloadHandler.text);
            }
            else
            {
                HandleFailedResponse(webRequest);
            }
        }
    }
    
    private int DetermineOptimalDuration(CompositionData composition, int shapeCount)
    {
        int baseDuration = 30; // Default 30 seconds
        
        // Adjust for complexity
        float complexityMultiplier = 1f + (composition.complexity * 0.5f);
        
        // Adjust for shape count
        float shapeMultiplier = 1f + (shapeCount / 10f);
        
        // Adjust for looping elements
        if (composition.hasLoopingElements)
            complexityMultiplier *= 1.2f;
        
        int finalDuration = Mathf.RoundToInt(baseDuration * complexityMultiplier * shapeMultiplier);
        return Mathf.Clamp(finalDuration, 15, 120); // Between 15 seconds and 2 minutes
    }
    
    private string DetermineIntensityLevel(float intensity)
    {
        if (intensity > 0.7f) return "high";
        else if (intensity > 0.4f) return "medium";
        else return "low";
    }
    
    private void LogDetailedTextToMusicRequest(TextToMusicRequest request, string jsonData)
    {
        if (!enableEnhancedLogging) return;
    
        Debug.Log("=== TEXT-TO-MUSIC API REQUEST DETAILS ===");
        Debug.Log($"Endpoint: {baseURL}");
        Debug.Log($"Method: POST");
    
        // LOG ALL HEADERS EXACTLY
        Debug.Log("=== REQUEST HEADERS ===");
        Debug.Log($"Content-Type: application/json");
        Debug.Log($"customer-id: {customerID}");
        Debug.Log($"access-token: {accessToken}");
    
        Debug.Log("=== REQUEST BODY ===");
        Debug.Log($"Prompt: \"{request.prompt}\" (Length: {request.prompt.Length}/200)");
        Debug.Log($"Duration: {request.duration}s");
        Debug.Log($"Bitrate: {request.bitrate} kbps");
        Debug.Log($"Mode: {request.mode}");
        Debug.Log($"Intensity: {request.intensity}");
        Debug.Log($"Format: {request.format}");
    
        Debug.Log("=== RAW JSON BODY ===");
        Debug.Log(jsonData);
    
        // LOG CURL EQUIVALENT
        LogCurlEquivalent(request, jsonData);
    
        Debug.Log("=== END REQUEST DETAILS ===");
    }
    
    private void LogDetailedResponse(UnityWebRequest webRequest)
    {
        Debug.Log("=== RESPONSE LOGGING START ===");
        
        if (!enableEnhancedLogging)
        {
            Debug.Log("Enhanced logging is disabled");
            return;
        }
        
        try
        {
            Debug.Log("=== MUBERT API RESPONSE DETAILS ===");
            Debug.Log($"Response Code: {webRequest.responseCode}");
            Debug.Log($"Response Result: {webRequest.result}");
            Debug.Log($"Error (if any): {webRequest.error ?? "None"}");
            Debug.Log($"URL: {webRequest.url}");
            Debug.Log($"Method: {webRequest.method}");
            
            // Log response headers
            var responseHeaders = webRequest.GetResponseHeaders();
            if (responseHeaders != null && responseHeaders.Count > 0)
            {
// Add this to LogDetailedResponse method, right after "=== RESPONSE HEADERS ==="
                Debug.Log("=== RESPONSE HEADERS (Raw) ===");
                foreach (var header in responseHeaders)
                {
                    Debug.Log($"{header.Key}: {header.Value}");
                }
            }
            else
            {
                Debug.Log("No response headers received");
            }
            
            // Log response body with more detail
            if (webRequest.downloadHandler != null)
            {
                string responseText = webRequest.downloadHandler.text;
                if (!string.IsNullOrEmpty(responseText))
                {
                    Debug.Log("=== RESPONSE BODY ===");
                    Debug.Log($"Response length: {responseText.Length} characters");
                    Debug.Log($"Raw response: {responseText}");
                }
                else
                {
                    Debug.Log("Response body is empty or null");
                    
                    // Check raw bytes
                    if (webRequest.downloadHandler.data != null)
                    {
                        Debug.Log($"Raw data bytes available: {webRequest.downloadHandler.data.Length}");
                        if (webRequest.downloadHandler.data.Length > 0)
                        {
                            string rawText = System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                            Debug.Log($"Raw bytes as text: {rawText}");
                        }
                    }
                    else
                    {
                        Debug.Log("No raw data bytes available");
                    }
                }
            }
            else
            {
                Debug.Log("downloadHandler is null");
            }
            
            Debug.Log("=== END RESPONSE DETAILS ===");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in LogDetailedResponse: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
        
        Debug.Log("=== RESPONSE LOGGING END ===");
    }
    
    private string FormatTextToMusicRequestSummary(TextToMusicRequest request)
    {
        StringBuilder summary = new StringBuilder();
        summary.AppendLine("=== TEXT-TO-MUSIC API REQUEST ===");
        summary.AppendLine($"Prompt: \"{request.prompt}\"");
        summary.AppendLine($"Duration: {request.duration}s | Mode: {request.mode} | Intensity: {request.intensity}");
        summary.AppendLine($"Bitrate: {request.bitrate} kbps | Format: {request.format}");
        summary.AppendLine($"Endpoint: {baseURL}");
        return summary.ToString();
    }
    
    private void HandleSuccessfulResponse(string responseText)
    {
        UpdateDebugStatus("API Request Successful", Color.green);
        UpdateDebugResponse(responseText);
        UpdateDebugProgress("Parsing API response...");
        
        if (enableEnhancedLogging)
        {
            Debug.Log("=== MUBERT API RESPONSE SUCCESS ===");
            Debug.Log(responseText);
        }
        
        try
        {
            MubertResponse response = JsonUtility.FromJson<MubertResponse>(responseText);
            
            if (response.status == 1 && !string.IsNullOrEmpty(response.data.link))
            {
                UpdateDebugProgress("Starting audio download...");
                StartCoroutine(DownloadAndPlayAudio(response.data.link));
            }
            else
            {
                UpdateDebugStatus("API Error", Color.red);
                string errorMsg = response.error?.text ?? "Unknown API error";
                UpdateDebugResponse($"API Error: {errorMsg}");
                UpdateDebugProgress("Failed - API returned error");
                Debug.LogError($"Mubert API Error: {errorMsg}");
            }
        }
        catch (System.Exception e)
        {
            UpdateDebugStatus("Response Parse Error", Color.red);
            UpdateDebugResponse($"Parse Error: {e.Message}");
            Debug.LogError($"Failed to parse Mubert response: {e.Message}");
        }
    }
    
    private void HandleFailedResponse(UnityWebRequest webRequest)
    {
        UpdateDebugStatus("Network/API Error", Color.red);
        string errorMsg = $"HTTP {webRequest.responseCode}: {webRequest.error}";
        UpdateDebugResponse(errorMsg);
        UpdateDebugProgress("Failed - Request error occurred");
        
        Debug.LogError("=== MUBERT API REQUEST FAILED ===");
        Debug.LogError($"Response Code: {webRequest.responseCode}");
        Debug.LogError($"Error: {webRequest.error}");
        Debug.LogError($"URL: {baseURL}");
        
        // Check for common error codes
        switch (webRequest.responseCode)
        {
            case 400:
                Debug.LogError("Bad Request - Check your request parameters");
                break;
            case 401:
                Debug.LogError("Unauthorized - Check your access token");
                break;
            case 403:
                Debug.LogError("Forbidden - Check your customer ID and permissions");
                break;
            case 404:
                Debug.LogError("Not Found - Check the API endpoint URL");
                break;
            case 429:
                Debug.LogError("Too Many Requests - Rate limit exceeded");
                break;
            case 500:
                Debug.LogError("Internal Server Error - Mubert server issue");
                break;
        }
        
        if (!string.IsNullOrEmpty(webRequest.downloadHandler.text))
        {
            Debug.LogError($"Response Body: {webRequest.downloadHandler.text}");
            UpdateDebugResponse($"Error Response: {webRequest.downloadHandler.text}");
        }
    }
    
    private IEnumerator DownloadAndPlayAudio(string audioURL)
    {
        UpdateDebugProgress("Downloading generated audio...");
        
        if (enableEnhancedLogging)
        {
            Debug.Log($"=== DOWNLOADING AUDIO FROM MUBERT ===");
            Debug.Log($"Audio URL: {audioURL}");
        }
        
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
                    UpdateDebugStatus("🎵 Music Generated Successfully!", Color.green);
                    UpdateDebugProgress("Playing generated music...");
                    
                    audioSource.clip = clip;
                    audioSource.Play();
                    
                    if (enableEnhancedLogging)
                    {
                        Debug.Log("=== MUSIC GENERATION COMPLETE ===");
                        Debug.Log($"Audio clip length: {clip.length:F2} seconds");
                        Debug.Log($"Audio clip frequency: {clip.frequency} Hz");
                        Debug.Log($"Audio clip channels: {clip.channels}");
                    }
                }
                else
                {
                    UpdateDebugStatus("Audio Processing Error", Color.red);
                    UpdateDebugProgress("Failed - Could not create AudioClip");
                    Debug.LogError("Failed to create AudioClip from downloaded data");
                }
            }
            else
            {
                UpdateDebugStatus("Audio Download Error", Color.red);
                UpdateDebugResponse($"Audio Error: {audioRequest.error}");
                UpdateDebugProgress("Failed - Audio download failed");
                Debug.LogError($"Audio download failed: {audioRequest.error}");
            }
        }
    }
    
    // Test method for composition analysis and prompt generation without API call
    public void TestCompositionAndPrompt()
    {
        ShowDebugPanel(true);
        UpdateDebugStatus("Testing Composition & Prompt Generation", Color.cyan);
        
        SelectableShape[] allShapes = FindObjectsOfType<SelectableShape>();
        
        if (allShapes.Length == 0)
        {
            UpdateDebugStatus("No Shapes Found", Color.red);
            UpdateDebugComposition("No shapes in scene to analyze");
            return;
        }
        
        // Analyze composition
        CompositionData composition = compositionAnalyzer.PerformComprehensiveAnalysis(allShapes);
        
        // Generate all prompt variations
        promptGenerator.ShowPromptVariations(composition, allShapes);
        
        // Display in debug panel
        string compositionInfo = compositionAnalyzer.FormatDetailedCompositionInfo(composition, allShapes);
        string fullPrompt = promptGenerator.GenerateMusicPrompt(composition, allShapes);
        string simplePrompt = promptGenerator.GenerateSimplePrompt(composition);
        
        string combinedInfo = $"{compositionInfo}\n\n=== FULL PROMPT ===\n{fullPrompt}\n\n=== SIMPLE PROMPT ===\n{simplePrompt}";
        
        UpdateDebugStatus("Test Complete", Color.green);
        UpdateDebugComposition(combinedInfo);
        UpdateDebugRequest("Test mode - Multiple prompt variations generated");
        UpdateDebugResponse("Check console for all prompt variations");
        UpdateDebugProgress("Composition and prompt analysis completed");
    }
    
    // Method to test just prompt generation
    public void TestPromptOnly()
    {
        bool originalTestMode = testPromptsOnly;
        testPromptsOnly = true;
        
        GenerateMusicFromShapes();
        
        testPromptsOnly = originalTestMode;
    }
    
    // Method to toggle between simple and full prompts
    public void TogglePromptComplexity()
    {
        useSimplePrompts = !useSimplePrompts;
        Debug.Log($"Prompt complexity toggled. Using simple prompts: {useSimplePrompts}");
        
        if (statusText != null)
        {
            statusText.text = $"Status: Using {(useSimplePrompts ? "Simple" : "Full")} Prompts";
        }
    }
    
    // Method to set API credentials
    public void SetAPICredentials(string newAccessToken, string newCustomerID)
    {
        accessToken = newAccessToken;
        customerID = newCustomerID;
        
        Debug.Log("Mubert API credentials updated");
        
        if (enableEnhancedLogging)
        {
            Debug.Log($"Access Token: {accessToken}");
            Debug.Log($"Customer ID: {(string.IsNullOrEmpty(customerID) ? "NOT SET" : "SET")}");
        }
    }
    
    // Validation method to check if API is ready
    public bool IsAPIReady()
    {
        bool isReady = !string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(customerID);
        
        if (!isReady && enableEnhancedLogging)
        {
            Debug.LogWarning("Mubert API not ready. Missing access token or customer ID.");
        }
        
        return isReady;
    }
    
    // Audio control methods
    public void StopCurrentAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            UpdateDebugStatus("Audio Stopped", Color.yellow);
            UpdateDebugProgress("Audio playback stopped by user");
        }
    }
    
    public void PauseCurrentAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            UpdateDebugStatus("Audio Paused", Color.yellow);
            UpdateDebugProgress("Audio playback paused");
        }
    }
    
    public void ResumeCurrentAudio()
    {
        if (audioSource != null && audioSource.clip != null && !audioSource.isPlaying)
        {
            audioSource.UnPause();
            UpdateDebugStatus("Audio Resumed", Color.green);
            UpdateDebugProgress("Audio playback resumed");
        }
    }
    
    public void SetAudioVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume);
            Debug.Log($"Audio volume set to: {audioSource.volume:F2}");
        }
    }
    
    // Test network connectivity
    public void TestNetworkConnectivity()
    {
        StartCoroutine(TestNetworkConnectivityCoroutine());
    }
    
    private IEnumerator TestNetworkConnectivityCoroutine()
    {
        Debug.Log("=== TESTING NETWORK CONNECTIVITY ===");
        
        // Test basic internet connectivity
        using (UnityWebRequest testRequest = UnityWebRequest.Get("https://httpbin.org/get"))
        {
            testRequest.timeout = 10;
            yield return testRequest.SendWebRequest();
            
            Debug.Log($"Test request result: {testRequest.result}");
            Debug.Log($"Test response code: {testRequest.responseCode}");
            if (testRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Basic internet connectivity: OK");
            }
            else
            {
                Debug.LogError($"❌ Basic connectivity failed: {testRequest.error}");
            }
        }
        
        // Test Mubert endpoint accessibility
        using (UnityWebRequest mubertTest = UnityWebRequest.Get(baseURL))
        {
            mubertTest.timeout = 10;
            yield return mubertTest.SendWebRequest();
            
            Debug.Log($"Mubert endpoint test result: {mubertTest.result}");
            Debug.Log($"Mubert endpoint response code: {mubertTest.responseCode}");
            if (mubertTest.responseCode == 405) // Method Not Allowed is expected for GET on POST endpoint
            {
                Debug.Log("✅ Mubert endpoint reachable (405 Method Not Allowed is expected)");
            }
            else if (mubertTest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Mubert endpoint accessible");
            }
            else
            {
                Debug.LogError($"❌ Mubert endpoint test failed: {mubertTest.error}");
            }
        }
        
        Debug.Log("=== NETWORK TEST COMPLETE ===");
    }
    public void DebugCurrentSettings()
    {
        Debug.Log("=== MUBERT API MANAGER SETTINGS ===");
        Debug.Log($"Base URL: {baseURL}");
        Debug.Log($"Access Token: {(string.IsNullOrEmpty(accessToken) ? "NOT SET" : accessToken)}");
        Debug.Log($"Customer ID: {(string.IsNullOrEmpty(customerID) ? "NOT SET" : "SET (" + customerID.Length + " chars)")}");
        Debug.Log($"Enhanced Logging: {enableEnhancedLogging}");
        Debug.Log($"Use Simple Prompts: {useSimplePrompts}");
        Debug.Log($"Test Prompts Only: {testPromptsOnly}");
        Debug.Log($"Max Prompt Length: {maxPromptLength}");
        Debug.Log($"Default Bitrate: {defaultBitrate}");
        Debug.Log($"Default Intensity: {defaultIntensity}");
        Debug.Log($"Default Mode: {defaultMode}");
        Debug.Log($"Internet Reachability: {Application.internetReachability}");
    }
    public void LogRawRequestDetails(string prompt, int duration)
    {
        TextToMusicRequest request = new TextToMusicRequest
        {
            prompt = prompt,
            duration = duration,
            bitrate = defaultBitrate,
            mode = defaultMode,
            intensity = defaultIntensity,
            format = "mp3"
        };
    
        string jsonData = JsonUtility.ToJson(request, true);
        Debug.Log("=== RAW REQUEST JSON ===");
        Debug.Log(jsonData);
        Debug.Log("=== RAW REQUEST HEADERS ===");
        Debug.Log($"customer-id: {customerID}");
        Debug.Log($"access-token: {accessToken}");
        Debug.Log($"Content-Type: application/json");
    }
    public void LogCurlEquivalent(TextToMusicRequest request, string jsonData)
    {
        Debug.Log("=== CURL EQUIVALENT ===");
    
        StringBuilder curlCommand = new StringBuilder();
        curlCommand.AppendLine($"curl -X POST \"{baseURL}\" \\");
        curlCommand.AppendLine($"-H \"Content-Type: application/json\" \\");
        curlCommand.AppendLine($"-H \"customer-id: {customerID}\" \\");
        curlCommand.AppendLine($"-H \"access-token: {accessToken}\" \\");
        curlCommand.AppendLine($"-d '{jsonData}'");
    
        Debug.Log(curlCommand.ToString());
        Debug.Log("=== END CURL ===");
    }
public void TestSimplePOST()
{
    StartCoroutine(TestSimplePOSTCoroutine());
}

private IEnumerator TestSimplePOSTCoroutine()
{
    Debug.Log("=== TESTING SIMPLE POST REQUEST ===");
    
    // Test 1: Simple POST to httpbin (should work)
    string testJson = "{\"test\": \"data\"}";
    using (UnityWebRequest testRequest = new UnityWebRequest("https://httpbin.org/post", "POST"))
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(testJson);
        testRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        testRequest.downloadHandler = new DownloadHandlerBuffer();
        testRequest.SetRequestHeader("Content-Type", "application/json");
        testRequest.timeout = 10;
        
        Debug.Log("Sending test POST to httpbin...");
        yield return testRequest.SendWebRequest();
        
        Debug.Log($"Test POST result: {testRequest.result}");
        Debug.Log($"Test POST response code: {testRequest.responseCode}");
        Debug.Log($"Test POST error: {testRequest.error ?? "None"}");
        
        if (testRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Basic POST requests work");
            Debug.Log($"Response: {testRequest.downloadHandler.text}");
        }
        else
        {
            Debug.LogError("❌ Basic POST requests failing");
        }
    }
    
// Test 2: POST to Mubert endpoint with correct headers and data
    string mubertTestJson = @"{
    ""playlist_index"": ""1.0.0"",
    ""prompt"": ""Relaxing ambient music"",
    ""bitrate"": 320,
    ""duration"": 300,
    ""format"": ""mp3"",
    ""intensity"": ""medium"",
    ""mode"": ""loop"",
    ""bpm"": 120,
    ""key"": ""C#""
}";

    using (UnityWebRequest mubertPost = new UnityWebRequest("https://music-api.mubert.com/api/v3/public/tracks", "POST"))
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(mubertTestJson);
        mubertPost.uploadHandler = new UploadHandlerRaw(bodyRaw);
        mubertPost.downloadHandler = new DownloadHandlerBuffer();
    
        // Use correct headers
        mubertPost.SetRequestHeader("Content-Type", "application/json");
        mubertPost.SetRequestHeader("customer-id", "YOUR_CUSTOMER_ID_HERE");
        mubertPost.SetRequestHeader("access-token", "YOUR_ACCESS_TOKEN_HERE");
        mubertPost.timeout = 30;
    
        Debug.Log("=== SENDING POST TO MUBERT ===");
        Debug.Log($"URL: YOUR_HARDCODED_URL_HERE");
        Debug.Log($"Headers: customer-id: YOUR_CUSTOMER_ID_HERE");
        Debug.Log($"Headers: access-token: YOUR_ACCESS_TOKEN_HERE");
        Debug.Log($"Body: {mubertTestJson}");
    
        yield return mubertPost.SendWebRequest();
    
        Debug.Log("=== MUBERT POST RESPONSE ===");
        Debug.Log($"Result: {mubertPost.result}");
        Debug.Log($"Response Code: {mubertPost.responseCode}");
        Debug.Log($"Error: {mubertPost.error ?? "None"}");
        Debug.Log($"Response: {mubertPost.downloadHandler.text ?? "No response"}");
    
        if (mubertPost.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Mubert POST with customer-id/access-token works!");
        }
        else if (mubertPost.responseCode > 0)
        {
            Debug.Log($"✅ Got response code: {mubertPost.responseCode}");
        }
        else
        {
            Debug.LogError("❌ No response from Mubert POST");
        }
    }
    Debug.Log("=== POST TEST COMPLETE ===");
}
}

// Data structures for text-to-music API
[System.Serializable]
public class TextToMusicRequest
{
    public string playlist_index = "1.0.0";
    public string prompt;
    public int bitrate = 128;
    public int duration;
    public string format = "mp3";
    public string intensity = "medium";
    public string mode = "track";
    public int bpm = 120;
    public string key = "C#";
}

// Response structures (these should match Mubert's text-to-music response)
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
    public string status;
}

[System.Serializable]
public class MubertError
{
    public string text;
    public int code;
}
