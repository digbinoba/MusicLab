using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;

public class PromptGenerator : MonoBehaviour
{
    [Header("Prompt Generation Settings")]
    [SerializeField] private bool enableDetailedLogging = true;
    [SerializeField] private bool includeInstrumentSuggestions = true;
    [SerializeField] private bool includeMoodDescriptors = true;
    [SerializeField] private bool includeStructuralElements = true;
    
    // Singleton for easy access
    public static PromptGenerator Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public string GenerateMusicPrompt(CompositionData composition, SelectableShape[] shapes)
    {
        if (enableDetailedLogging) Debug.Log("=== GENERATING MUSIC PROMPT ===");
        
        StringBuilder prompt = new StringBuilder();
        
        // Core style and genre
        string coreStyle = DetermineCoreStyle(composition);
        prompt.Append($"Create a {coreStyle} track");
        
        // Add tempo and energy
        string tempoDescription = GetTempoDescription(composition);
        prompt.Append($" with {tempoDescription}");
        
        // Add primary mood
        string moodDescription = GetMoodDescription(composition);
        prompt.Append($", {moodDescription}");
        
        // Add instrumental elements if enabled
        if (includeInstrumentSuggestions)
        {
            string instruments = GenerateInstrumentSuggestions(composition, shapes);
            if (!string.IsNullOrEmpty(instruments))
            {
                prompt.Append($". {instruments}");
            }
        }
        
        // Add structural elements if enabled
        if (includeStructuralElements)
        {
            string structure = GenerateStructuralDescription(composition, shapes);
            if (!string.IsNullOrEmpty(structure))
            {
                prompt.Append($" {structure}");
            }
        }
        
        // Add mood descriptors if enabled
        if (includeMoodDescriptors)
        {
            string moodDetails = GenerateDetailedMoodDescription(composition);
            if (!string.IsNullOrEmpty(moodDetails))
            {
                prompt.Append($" {moodDetails}");
            }
        }
        
        // Add material-based sonic characteristics
        string sonicCharacteristics = GenerateSonicCharacteristics(composition);
        if (!string.IsNullOrEmpty(sonicCharacteristics))
        {
            prompt.Append($" {sonicCharacteristics}");
        }
        
        // Add duration and looping info
        string durationInfo = GenerateDurationDescription(composition);
        prompt.Append($" {durationInfo}");
        
        string finalPrompt = prompt.ToString();
        
        if (enableDetailedLogging)
        {
            Debug.Log($"=== GENERATED MUSIC PROMPT ===");
            Debug.Log(finalPrompt);
            Debug.Log($"Prompt Length: {finalPrompt.Length} characters");
        }
        
        return finalPrompt;
    }
    
    private string DetermineCoreStyle(CompositionData composition)
    {
        // Determine genre/style based on composition analysis
        if (composition.energy > 0.7f && composition.aggression > 0.5f)
        {
            if (composition.metallic > 0.5f) return "electronic dance";
            else return "energetic pop";
        }
        else if (composition.darkness > 0.6f && composition.mystery > 0.5f)
        {
            return "dark ambient";
        }
        else if (composition.happiness > 0.7f && composition.playfulness > 0.5f)
        {
            return "upbeat pop";
        }
        else if (composition.calmness > 0.6f)
        {
            if (composition.organic > 0.4f) return "acoustic ambient";
            else return "chill electronic";
        }
        else if (composition.complexity > 0.7f)
        {
            return "progressive electronic";
        }
        else
        {
            return "modern instrumental";
        }
    }
    
    private string GetTempoDescription(CompositionData composition)
    {
        string baseTempoDescription = "";
        
        switch (composition.tempo)
        {
            case "fast":
                baseTempoDescription = "a fast, driving tempo";
                break;
            case "medium":
                baseTempoDescription = "a moderate, steady tempo";
                break;
            case "slow":
                baseTempoDescription = "a slow, deliberate tempo";
                break;
            default:
                baseTempoDescription = "a balanced tempo";
                break;
        }
        
        // Add energy qualifiers
        if (composition.energy > 0.8f)
            baseTempoDescription += " and high energy";
        else if (composition.energy < 0.3f)
            baseTempoDescription += " and relaxed energy";
        
        return baseTempoDescription;
    }
    
    private string GetMoodDescription(CompositionData composition)
    {
        List<string> moodDescriptors = new List<string>();
        
        // Primary mood
        moodDescriptors.Add($"{composition.primaryMood} feeling");
        
        // Add secondary characteristics
        if (composition.aggression > 0.6f) moodDescriptors.Add("intense");
        if (composition.mystery > 0.6f) moodDescriptors.Add("mysterious");
        if (composition.playfulness > 0.6f) moodDescriptors.Add("playful");
        
        return string.Join(", ", moodDescriptors);
    }
    
    private string GenerateInstrumentSuggestions(CompositionData composition, SelectableShape[] shapes)
    {
        List<string> instruments = new List<string>();
        
        // Analyze shapes for instrument suggestions
        var roleDistribution = shapes.GroupBy(s => s.advancedData.musicalRole)
                                   .ToDictionary(g => g.Key, g => g.Count());
        
        // Bass instruments
        if (roleDistribution.ContainsKey(SelectableShape.ShapeRole.Bass))
        {
            if (composition.metallic > 0.5f)
                instruments.Add("synthesized bass");
            else if (composition.organic > 0.5f)
                instruments.Add("acoustic bass");
            else
                instruments.Add("electric bass");
        }
        
        // Percussion
        if (roleDistribution.ContainsKey(SelectableShape.ShapeRole.Percussion) || 
            roleDistribution.ContainsKey(SelectableShape.ShapeRole.Rhythm))
        {
            if (composition.energy > 0.7f)
                instruments.Add("driving drums");
            else if (composition.calmness > 0.6f)
                instruments.Add("soft percussion");
            else
                instruments.Add("rhythmic drums");
        }
        
        // Lead instruments
        if (roleDistribution.ContainsKey(SelectableShape.ShapeRole.Lead))
        {
            if (composition.synthetic > 0.5f)
                instruments.Add("lead synthesizer");
            else if (composition.organic > 0.5f)
                instruments.Add("acoustic lead guitar");
            else
                instruments.Add("electric lead");
        }
        
        // Harmony instruments
        if (roleDistribution.ContainsKey(SelectableShape.ShapeRole.Harmony))
        {
            if (composition.smooth > 0.5f)
                instruments.Add("smooth pads");
            else
                instruments.Add("harmonic chords");
        }
        
        // Ambient elements
        if (roleDistribution.ContainsKey(SelectableShape.ShapeRole.Ambient))
        {
            instruments.Add("atmospheric textures");
        }
        
        if (instruments.Count > 0)
        {
            return $"Include {string.Join(", ", instruments)}.";
        }
        
        return "";
    }
    
    private string GenerateStructuralDescription(CompositionData composition, SelectableShape[] shapes)
    {
        List<string> structuralElements = new List<string>();
        
        // Clustering effects
        if (composition.clustering > 0.6f)
        {
            structuralElements.Add("with tightly layered elements");
        }
        else if (composition.clustering < 0.3f)
        {
            structuralElements.Add("with spacious arrangement");
        }
        
        // Complexity effects
        if (composition.complexity > 0.7f)
        {
            structuralElements.Add("featuring complex layering and build-ups");
        }
        else if (composition.complexity < 0.3f)
        {
            structuralElements.Add("with a clean, minimal arrangement");
        }
        
        // Movement and dynamics
        if (composition.movement > 0.8f)
        {
            structuralElements.Add("with dynamic movement and rhythm variations");
        }
        
        // Looping elements
        if (composition.hasLoopingElements)
        {
            structuralElements.Add("incorporating repetitive, hypnotic elements");
        }
        
        return string.Join(" ", structuralElements);
    }
    
    private string GenerateDetailedMoodDescription(CompositionData composition)
    {
        List<string> moodDetails = new List<string>();
        
        // Emotional combinations
        if (composition.happiness > 0.6f && composition.energy > 0.6f)
        {
            moodDetails.Add("The track should feel uplifting and energizing");
        }
        else if (composition.darkness > 0.6f && composition.mystery > 0.6f)
        {
            moodDetails.Add("Create a deep, mysterious atmosphere");
        }
        else if (composition.calmness > 0.6f && composition.organic > 0.4f)
        {
            moodDetails.Add("Evoke a peaceful, natural feeling");
        }
        
        // Intensity descriptions
        if (composition.intensity > 0.8f)
        {
            moodDetails.Add("with powerful, intense moments");
        }
        else if (composition.intensity < 0.3f)
        {
            moodDetails.Add("maintaining a gentle, subtle intensity");
        }
        
        return string.Join(", ", moodDetails);
    }
    
    private string GenerateSonicCharacteristics(CompositionData composition)
    {
        List<string> characteristics = new List<string>();
        
        // Material-based sonic qualities
        if (composition.metallic > 0.5f)
        {
            characteristics.Add("with sharp, metallic tones");
        }
        
        if (composition.organic > 0.5f)
        {
            characteristics.Add("featuring warm, organic textures");
        }
        
        if (composition.synthetic > 0.5f)
        {
            characteristics.Add("using modern synthetic sounds");
        }
        
        if (composition.smooth > 0.5f)
        {
            characteristics.Add("with polished, clean production");
        }
        
        // Roughness effects
        if (composition.averageRoughness > 0.6f)
        {
            characteristics.Add("adding gritty, textured elements");
        }
        
        return string.Join(" ", characteristics);
    }
    
    private string GenerateDurationDescription(CompositionData composition)
    {
        if (composition.hasLoopingElements)
        {
            return "Make it suitable for looping.";
        }
        else
        {
            return "Create a complete musical piece with clear beginning and end.";
        }
    }
    
    // Alternative method for simpler prompts
    public string GenerateSimplePrompt(CompositionData composition)
    {
        string style = DetermineCoreStyle(composition);
        string mood = composition.primaryMood;
        string tempo = composition.tempo;
        string energy = composition.energy > 0.6f ? "high energy" : composition.energy < 0.4f ? "low energy" : "moderate energy";
        
        return $"Create a {style} track with {tempo} tempo, {mood} mood, and {energy}.";
    }
    
    // Method to get prompt with custom parameters
    public string GenerateCustomPrompt(CompositionData composition, SelectableShape[] shapes, bool includeInstruments, bool includeMood, bool includeStructure)
    {
        bool originalInstruments = includeInstrumentSuggestions;
        bool originalMood = includeMoodDescriptors;
        bool originalStructure = includeStructuralElements;
        
        includeInstrumentSuggestions = includeInstruments;
        includeMoodDescriptors = includeMood;
        includeStructuralElements = includeStructure;
        
        string prompt = GenerateMusicPrompt(composition, shapes);
        
        // Restore original settings
        includeInstrumentSuggestions = originalInstruments;
        includeMoodDescriptors = originalMood;
        includeStructuralElements = originalStructure;
        
        return prompt;
    }
    
    // Debug method to show all possible prompt variations
    public void ShowPromptVariations(CompositionData composition, SelectableShape[] shapes)
    {
        Debug.Log("=== PROMPT VARIATIONS ===");
        Debug.Log($"Simple: {GenerateSimplePrompt(composition)}");
        Debug.Log($"Full: {GenerateMusicPrompt(composition, shapes)}");
        Debug.Log($"Instruments Only: {GenerateCustomPrompt(composition, shapes, true, false, false)}");
        Debug.Log($"Mood Only: {GenerateCustomPrompt(composition, shapes, false, true, false)}");
        Debug.Log($"Structure Only: {GenerateCustomPrompt(composition, shapes, false, false, true)}");
    }
}