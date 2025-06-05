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
    
    private bool isCurrentlyDownloading = false;
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
        isCurrentlyDownloading = false; // Reset for new generation
        
        UpdateDebugStatus("✅ Request Accepted", Color.green);
        UpdateDebugResponse(responseText);
        UpdateDebugProgress("Music generation started - waiting for completion...");
    
        if (enableEnhancedLogging)
        {
            Debug.Log("=== MUBERT API RESPONSE SUCCESS ===");
            Debug.Log(responseText);
        }
        // ADD THIS DEBUGGING SECTION:
        Debug.Log("=== DEBUGGING JSON PARSING ===");
        Debug.Log($"Raw response length: {responseText.Length}");
        Debug.Log($"Response starts with: {responseText.Substring(0, Mathf.Min(100, responseText.Length))}");
        try
        {
            Debug.Log("Attempting to parse JSON...");
            MubertResponse response = JsonUtility.FromJson<MubertResponse>(responseText);
        
            Debug.Log(response);
            Debug.Log(response.data);
            Debug.Log(response.data.generations);

            Debug.Log(response.status);
            Debug.Log(response.status.ToString());
            Debug.Log("Over here:testing response before downloading");
            
            Debug.Log($"JSON parsed successfully!");
            Debug.Log($"Response status: {response.status}");
            Debug.Log($"Response data is null: {response.data == null}");
            Debug.Log($"Response error is null: {response.error == null}");
            
            if (response.data != null)
            {
                Debug.Log($"Data session_id: {response.data.session_id}");
                Debug.Log($"Data generations is null: {response.data.generations == null}");
                Debug.Log($"Data generations length: {response.data.generations?.Length ?? 0}");
            
                if (response.data.generations != null && response.data.generations.Length > 0)
                {
                    Debug.Log($"First generation status: {response.data.generations[0].status}");
                    Debug.Log($"First generation session_id: {response.data.generations[0].session_id}");
                }
            }
            
            if ((response.status == 0 || response.status == 1) && response.data != null)
            {
                var firstTrack = response.data;
                Debug.Log("Checkpoint1");
                if (firstTrack.generations != null && firstTrack.generations.Length > 0)
                {
                    Debug.Log("Checkpoint2");

                    var generation = firstTrack.generations[0];
                    string sessionId = generation.session_id;
                
                    UpdateDebugStatus("🎵 Generation Started", Color.yellow);
                    UpdateDebugProgress($"Waiting for music generation... Session: {sessionId}");
                
                    Debug.Log("Checkpoint3");

                    // Start polling for completion
                    StartCoroutine(PollForGenerationCompletion(sessionId, generation));
                }
                else
                {
                    UpdateDebugStatus("Error: No generations found", Color.red);
                    Debug.LogError("No generations found in response");
                }
            }
            else
            {
                UpdateDebugStatus("API Error", Color.red);
                string errorMsg = response.error?.text ?? "Unknown API error";
                UpdateDebugResponse($"API Error: {errorMsg}");
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
    private IEnumerator PollForGenerationCompletion(string sessionId, Generation generation)
    {
        float pollingInterval = 5f; // Check every 5 seconds
        float maxWaitTime = 120f; // Wait max 2 minutes
        float timeWaited = 0f;
        bool generationComplete = false;
    
        UpdateDebugProgress("🔄 Polling for generation status...");
    
        while (timeWaited < maxWaitTime && !isCurrentlyDownloading)
        {
            yield return new WaitForSeconds(pollingInterval);
            timeWaited += pollingInterval;
    
            // Make a NEW API call to check current status
            yield return StartCoroutine(CheckGenerationStatus(sessionId));
    
            // Update progress only if still polling
            if (!isCurrentlyDownloading)
            {
                int timeRemaining = Mathf.RoundToInt(maxWaitTime - timeWaited);
                UpdateDebugProgress($"⏳ Waiting for generation... {timeRemaining}s remaining");
            }
        }
    
        // Only show timeout if we didn't find completion
        if (!generationComplete)
        {
            UpdateDebugStatus("⚠️ Generation Timeout", Color.red);
            UpdateDebugProgress("Music generation timed out after 2 minutes");
            Debug.LogError("Music generation timed out");
        }
    }
private IEnumerator CheckGenerationStatus(string sessionId)
{
    Debug.Log($"Checking status for session: {sessionId}");
    
    using (UnityWebRequest statusRequest = UnityWebRequest.Get(baseURL))
    {
        statusRequest.SetRequestHeader("customer-id", customerID);
        statusRequest.SetRequestHeader("access-token", accessToken);
        statusRequest.timeout = 10;
        
        yield return statusRequest.SendWebRequest();
        
        if (statusRequest.result == UnityWebRequest.Result.Success)
        {
            string responseText = statusRequest.downloadHandler.text;
            Debug.Log($"Status check response: {responseText}");
            
            try
            {
                MubertGetResponse getResponse = JsonUtility.FromJson<MubertGetResponse>(responseText);
                
                // Find the track with matching session_id
                foreach (var track in getResponse.data)
                {
                    if (track.session_id == sessionId)
                    {
                        Debug.Log($"Found matching track! Status: {track.generations[0].status}");
                        
                        // ONLY set the flag if the track is actually done AND has a URL
                        if (track.generations[0].status == "done" && !string.IsNullOrEmpty(track.generations[0].url))
                        {
                            // Add this check to prevent multiple downloads
                            if (!isCurrentlyDownloading)
                            {
                                isCurrentlyDownloading = true; // Set flag to prevent repeats
            
                                UpdateDebugStatus("🎵 Music Generated!", Color.green);
                                UpdateDebugProgress("Music generation complete! Starting download...");
            
                                // Start download process
                                StartCoroutine(DownloadAndPlayGeneratedMusic(track.generations[0].url, sessionId));
                            }
                            else
                            {
                                Debug.Log("Download already in progress, skipping...");
                            }
        
                            yield break; // Exit polling for this session
                        }
                        else
                        {
                            Debug.Log($"Still processing... Status: {track.generations[0].status}");
                            // DON'T set isCurrentlyDownloading here - let polling continue
                        }
                        break;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse status response: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"Status check failed: {statusRequest.error}");
        }
    }
}
private IEnumerator DownloadAndPlayGeneratedMusic(string audioURL, string sessionId)
{
    UpdateDebugProgress("📥 Downloading generated music...");
    
    if (enableEnhancedLogging)
    {
        Debug.Log($"=== DOWNLOADING GENERATED MUSIC ===");
        Debug.Log($"Audio URL: {audioURL}");
        Debug.Log($"Session ID: {sessionId}");
    }
    
    // Try method 1: UnityWebRequestMultimedia
    yield return StartCoroutine(TryDownloadWithMultimedia(audioURL, sessionId));
}

private IEnumerator TryDownloadWithMultimedia(string audioURL, string sessionId)
{
    Debug.Log("Attempting download with UnityWebRequestMultimedia...");
    
    using (UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip(audioURL, AudioType.MPEG))
    {
        audioRequest.timeout = 30;
        
        // Send the request
        yield return audioRequest.SendWebRequest();
        
        Debug.Log($"Download result: {audioRequest.result}");
        Debug.Log($"Response code: {audioRequest.responseCode}");
        Debug.Log($"Download progress: {audioRequest.downloadProgress}");
        Debug.Log($"Error: {audioRequest.error ?? "None"}");
        
        if (audioRequest.result == UnityWebRequest.Result.Success)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(audioRequest);
            
            if (clip != null)
            {
                Debug.Log("✅ Download successful with Multimedia method!");
                yield return StartCoroutine(ProcessSuccessfulDownload(clip, sessionId));
                yield break;
            }
            else
            {
                Debug.LogError("AudioClip is null despite successful download");
            }
        }
        else
        {
            Debug.LogError($"Multimedia download failed: {audioRequest.error}");
        }
    }
    
    // If multimedia failed, try raw download
    Debug.Log("Multimedia failed, trying raw download...");
    yield return StartCoroutine(TryDownloadRaw(audioURL, sessionId));
}

private IEnumerator TryDownloadRaw(string audioURL, string sessionId)
{
    Debug.Log("Attempting raw download...");
    
    using (UnityWebRequest rawRequest = UnityWebRequest.Get(audioURL))
    {
        rawRequest.timeout = 30;
        
        while (!rawRequest.isDone)
        {
            float progress = rawRequest.downloadProgress * 100f;
            UpdateDebugProgress($"📥 Raw downloading... {progress:F0}%");
            Debug.Log($"Raw download progress: {progress:F1}%");
            yield return null;
        }
        
        yield return rawRequest.SendWebRequest();
        
        Debug.Log($"Raw download result: {rawRequest.result}");
        Debug.Log($"Raw response code: {rawRequest.responseCode}");
        Debug.Log($"Raw error: {rawRequest.error ?? "None"}");
        
        if (rawRequest.result == UnityWebRequest.Result.Success)
        {
            byte[] audioData = rawRequest.downloadHandler.data;
            Debug.Log($"Downloaded {audioData.Length} bytes of audio data");
            
            // Save raw data to file first
            yield return StartCoroutine(SaveRawAudioData(audioData, sessionId));
            
            // Try to create AudioClip from raw data
            AudioClip clip = CreateAudioClipFromMP3Data(audioData, sessionId);
            
            if (clip != null)
            {
                Debug.Log("✅ Successfully created AudioClip from raw data!");
                yield return StartCoroutine(ProcessSuccessfulDownload(clip, sessionId));
            }
            else
            {
                UpdateDebugStatus("❌ Could not create AudioClip", Color.red);
                UpdateDebugProgress("Audio downloaded but couldn't create playable clip");
                Debug.LogError("Failed to create AudioClip from downloaded data");
            }
        }
        else
        {
            UpdateDebugStatus("❌ Download Failed", Color.red);
            UpdateDebugProgress($"Download failed: {rawRequest.error}");
            Debug.LogError($"Raw download failed: {rawRequest.error}");
        }
    }
}

private IEnumerator SaveRawAudioData(byte[] audioData, string sessionId)
{
    bool saveSuccess = SaveRawAudioDataToFile(audioData, sessionId);
    
    if (saveSuccess)
    {
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"GeneratedMusic_{sessionId}_{timestamp}.mp3";
        UpdateDebugProgress($"💾 Raw MP3 saved: {filename}");
    }
    else
    {
        UpdateDebugProgress("❌ Failed to save raw MP3");
    }
    
    yield return null;
}

private bool SaveRawAudioDataToFile(byte[] audioData, string sessionId)
{
    try
    {
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"GeneratedMusic_{sessionId}_{timestamp}.mp3";
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
        
        System.IO.File.WriteAllBytes(filePath, audioData);
        
        Debug.Log($"Raw MP3 saved: {filePath}");
        return true;
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Failed to save raw audio data: {e.Message}");
        return false;
    }
}

private AudioClip CreateAudioClipFromMP3Data(byte[] mp3Data, string sessionId)
{
    try
    {
        // For MP3, Unity's built-in methods might not work well
        // We'll try a simple approach first
        Debug.Log("Attempting to create AudioClip from MP3 data...");
        
        // This might not work for MP3, but worth trying
        AudioClip clip = AudioClip.Create($"GeneratedMusic_{sessionId}", 
            44100, // Sample rate
            2,     // Channels
            44100, // Frequency
            false  // Stream
        );
        
        // Note: This is a placeholder - MP3 decoding in Unity is complex
        // For now, we'll just save the file and note that it was downloaded
        Debug.LogWarning("MP3 to AudioClip conversion not fully implemented");
        return null;
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Failed to create AudioClip from MP3 data: {e.Message}");
        return null;
    }
}

private IEnumerator ProcessSuccessfulDownload(AudioClip clip, string sessionId)
{
    UpdateDebugStatus("✅ Download Complete!", Color.green);
    UpdateDebugProgress("Music downloaded successfully!");
    
    // Save as WAV if possible
    yield return StartCoroutine(SaveAudioClipLocally(clip, sessionId));
    
    // Countdown and play
    yield return StartCoroutine(CountdownAndPlay(clip));
    
    if (enableEnhancedLogging)
    {
        Debug.Log("=== MUSIC DOWNLOAD COMPLETE ===");
        Debug.Log($"Audio clip length: {clip.length:F2} seconds");
        Debug.Log($"Audio clip frequency: {clip.frequency} Hz");
        Debug.Log($"Audio clip channels: {clip.channels}");
    }
}
    private IEnumerator CountdownAndPlay(AudioClip clip)
    {
        UpdateDebugProgress("🎵 Ready to play! Starting countdown...");
    
        for (int i = 5; i > 0; i--)
        {
            UpdateDebugStatus($"▶️ Playing in {i}...", Color.cyan);
            Debug.Log($"Playing music in {i} seconds...");
            yield return new WaitForSeconds(1f);
        }
    
        UpdateDebugStatus("🎵 Now Playing!", Color.green);
        UpdateDebugProgress("Playing your generated music!");
    
        audioSource.clip = clip;
        audioSource.Play();
    
        Debug.Log("🎵 Generated music is now playing!");
    }

    private IEnumerator SaveAudioClipLocally(AudioClip clip, string sessionId)
    {
        // Move the try/catch logic to a separate method
        bool saveSuccess = SaveAudioClipToFile(clip, sessionId);
    
        if (saveSuccess)
        {
            UpdateDebugProgress($"💾 Saved locally as: GeneratedMusic_{sessionId}_{System.DateTime.Now.ToString("yyyyMMdd_HHmmss")}.wav");
        }
        else
        {
            UpdateDebugProgress("❌ Failed to save locally");
        }
    
        yield return null;
    }

    private bool SaveAudioClipToFile(AudioClip clip, string sessionId)
    {
        try
        {
            // Create filename with timestamp
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"GeneratedMusic_{sessionId}_{timestamp}.wav";
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
        
            // Save the audio clip as WAV
            SavWav.Save(filePath, clip);
        
            Debug.Log($"Music saved locally: {filePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save audio locally: {e.Message}");
            return false;
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
    public MubertResponseData data;
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
[System.Serializable]
public class MubertResponseData
{
    public string id;
    public string session_id;
    public string playlist_index;
    public string prompt;
    public int duration;
    public string intensity;
    public string mode;
    public int bpm;
    public string key;
    public Generation[] generations;
}
[System.Serializable]
public class Generation
{
    public string session_id;
    public string format;
    public int bitrate;
    public string status;
    public string generated_at;
    public string expired_at;
    public string created_at;
    public string url;
}
// For GET requests (polling) - returns array
[System.Serializable]
public class MubertGetResponse
{
    public MubertResponseData[] data;  // Array for GET response
    public MubertMeta meta;
}

[System.Serializable]
public class MubertMeta
{
    public int offset;
    public int limit;
    public int total;
}