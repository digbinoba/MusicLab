using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

[System.Serializable]
public class CompositionAnalyzer : MonoBehaviour
{
    [Header("Analysis Settings")]
    [SerializeField] private float clusterDistance = 2f;
    [SerializeField] private bool enableDetailedLogging = true;
    
    // Singleton pattern for easy access
    public static CompositionAnalyzer Instance;
    
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
    
    public CompositionData PerformComprehensiveAnalysis(SelectableShape[] shapes)
    {
        if (enableDetailedLogging) Debug.Log("=== COMPREHENSIVE COMPOSITION ANALYSIS START ===");
        
        if (shapes == null || shapes.Length == 0)
        {
            Debug.LogWarning("No shapes provided for analysis");
            return CreateEmptyComposition();
        }
        
        CompositionData composition = new CompositionData();
        List<EnhancedShapeInfluence> influences = CalculateShapeInfluences(shapes);
        
        // Normalize weights and calculate center of mass
        NormalizeInfluences(influences, out Vector3 centerOfMass);
        composition.centerOfMass = centerOfMass;
        
        // Perform comprehensive blending
        composition = BlendEnhancedComposition(influences);
        composition = CalculateSpatialProperties(composition, influences);
        composition = CalculateTemporalProperties(composition, influences);
        composition = CalculateHarmonicProperties(composition, influences);
        composition = GenerateComprehensiveTags(composition);
        
        if (enableDetailedLogging) LogComprehensiveResults(composition, shapes);
        
        return composition;
    }
    
    private List<EnhancedShapeInfluence> CalculateShapeInfluences(SelectableShape[] shapes)
    {
        List<EnhancedShapeInfluence> influences = new List<EnhancedShapeInfluence>();
        
        foreach (SelectableShape shape in shapes)
        {
            EnhancedShapeInfluence influence = CalculateIndividualShapeInfluence(shape);
            influences.Add(influence);
            
            if (enableDetailedLogging)
            {
                Debug.Log($"Shape {shape.gameObject.name}: Weight={influence.weight:F2}, " +
                         $"Mood Impact: E:{influence.moodContribution.energy:F2} " +
                         $"H:{influence.moodContribution.happiness:F2} " +
                         $"D:{influence.moodContribution.darkness:F2}");
            }
        }
        
        return influences;
    }
    
    private EnhancedShapeInfluence CalculateIndividualShapeInfluence(SelectableShape shape)
    {
        EnhancedShapeInfluence influence = new EnhancedShapeInfluence();
        influence.shapeData = shape.GetAdvancedShapeData();
        influence.shape = shape;
        
        // Multi-factor weight calculation
        WeightFactors weights = CalculateWeightFactors(shape);
        influence.weight = weights.GetCombinedWeight();
        
        // Calculate contributions
        influence.moodContribution = GetEnhancedMoodContribution(
            shape.advancedData.emotionalColor, 
            shape.advancedData.intensity
        );
        influence.spatialInfluence = CalculateSpatialInfluence(shape);
        influence.temporalInfluence = CalculateTemporalInfluence(shape);
        
        return influence;
    }
    
    private WeightFactors CalculateWeightFactors(SelectableShape shape)
    {
        WeightFactors factors = new WeightFactors();
        
        // Size influence (quadratic for more dramatic effect)
        factors.sizeWeight = shape.shapeSize * shape.shapeSize;
        
        // Property multipliers
        factors.intensityWeight = shape.advancedData.intensity * 2f;
        factors.energyWeight = shape.advancedData.energy * 1.5f;
        factors.roleWeight = GetRoleWeight(shape.advancedData.musicalRole);
        factors.materialWeight = GetMaterialWeight(shape.advancedData.materialType);
        factors.animationWeight = GetAnimationWeight(shape.advancedData.animationType);
        
        // Duration influence (longer shapes have more sustained impact)
        factors.durationWeight = Mathf.Clamp(shape.advancedData.duration / 10f, 0.1f, 1f);
        
        return factors;
    }
    
    private float GetRoleWeight(SelectableShape.ShapeRole role)
    {
        switch (role)
        {
            case SelectableShape.ShapeRole.Lead: return 0.8f;      // Highest influence
            case SelectableShape.ShapeRole.Bass: return 0.6f;      // Foundation influence
            case SelectableShape.ShapeRole.Rhythm: return 0.5f;    // Drive influence  
            case SelectableShape.ShapeRole.Percussion: return 0.4f; // Texture influence
            case SelectableShape.ShapeRole.Harmony: return 0.3f;   // Support influence
            case SelectableShape.ShapeRole.Ambient: return 0.2f;   // Atmosphere influence
            default: return 0.3f;
        }
    }
    
    private float GetMaterialWeight(SelectableShape.MaterialType material)
    {
        switch (material)
        {
            case SelectableShape.MaterialType.Glass: return 0.4f;    // Resonant, clear
            case SelectableShape.MaterialType.Metallic: return 0.3f; // Sharp, cutting
            case SelectableShape.MaterialType.Rough: return 0.2f;    // Textured, gritty
            case SelectableShape.MaterialType.Smooth: return 0.15f;  // Clean
            case SelectableShape.MaterialType.Wood: return 0.1f;     // Warm, organic
            case SelectableShape.MaterialType.Fabric: return 0.05f;  // Soft, muted
            default: return 0.1f;
        }
    }
    
    private float GetAnimationWeight(SelectableShape.AnimationType animation)
    {
        switch (animation)
        {
            case SelectableShape.AnimationType.Bounce: return 0.3f;  // Strong rhythmic
            case SelectableShape.AnimationType.Pulse: return 0.25f;  // Pulsing rhythm
            case SelectableShape.AnimationType.Spin: return 0.2f;    // Continuous movement
            case SelectableShape.AnimationType.Float: return 0.1f;   // Gentle movement
            case SelectableShape.AnimationType.None: return 0f;      // No movement
            default: return 0f;
        }
    }
    
    private EnhancedMoodContribution GetEnhancedMoodContribution(SelectableShape.EmotionalColor color, float intensity)
    {
        EnhancedMoodContribution baseMood = GetBaseMoodContribution(color);
        
        // Apply intensity modulation
        return new EnhancedMoodContribution
        {
            energy = baseMood.energy * (0.5f + intensity * 0.5f),
            happiness = baseMood.happiness * (0.5f + intensity * 0.5f),
            darkness = baseMood.darkness * (0.5f + intensity * 0.5f),
            calmness = baseMood.calmness * (2f - intensity), // Inverse relationship
            aggression = baseMood.aggression * intensity,
            mystery = baseMood.mystery * intensity,
            playfulness = baseMood.playfulness * intensity
        };
    }
    
    private EnhancedMoodContribution GetBaseMoodContribution(SelectableShape.EmotionalColor color)
    {
        switch (color)
        {
            case SelectableShape.EmotionalColor.Red:
                return new EnhancedMoodContribution { 
                    energy = 1.0f, happiness = 0.6f, darkness = 0.2f, calmness = 0.0f, 
                    aggression = 0.8f, mystery = 0.1f, playfulness = 0.3f 
                };
            case SelectableShape.EmotionalColor.Orange:
                return new EnhancedMoodContribution { 
                    energy = 0.8f, happiness = 0.9f, darkness = 0.0f, calmness = 0.2f,
                    aggression = 0.2f, mystery = 0.0f, playfulness = 0.9f 
                };
            case SelectableShape.EmotionalColor.Yellow:
                return new EnhancedMoodContribution { 
                    energy = 0.7f, happiness = 1.0f, darkness = 0.0f, calmness = 0.3f,
                    aggression = 0.1f, mystery = 0.0f, playfulness = 0.8f 
                };
            case SelectableShape.EmotionalColor.Green:
                return new EnhancedMoodContribution { 
                    energy = 0.3f, happiness = 0.6f, darkness = 0.1f, calmness = 1.0f,
                    aggression = 0.0f, mystery = 0.2f, playfulness = 0.4f 
                };
            case SelectableShape.EmotionalColor.Blue:
                return new EnhancedMoodContribution { 
                    energy = 0.2f, happiness = 0.2f, darkness = 0.6f, calmness = 0.8f,
                    aggression = 0.1f, mystery = 0.7f, playfulness = 0.1f 
                };
            case SelectableShape.EmotionalColor.Purple:
                return new EnhancedMoodContribution { 
                    energy = 0.5f, happiness = 0.3f, darkness = 0.7f, calmness = 0.4f,
                    aggression = 0.3f, mystery = 0.9f, playfulness = 0.2f 
                };
            case SelectableShape.EmotionalColor.Black:
                return new EnhancedMoodContribution { 
                    energy = 0.8f, happiness = 0.0f, darkness = 1.0f, calmness = 0.0f,
                    aggression = 0.9f, mystery = 0.8f, playfulness = 0.0f 
                };
            case SelectableShape.EmotionalColor.White:
                return new EnhancedMoodContribution { 
                    energy = 0.3f, happiness = 0.7f, darkness = 0.0f, calmness = 0.9f,
                    aggression = 0.0f, mystery = 0.1f, playfulness = 0.3f 
                };
            case SelectableShape.EmotionalColor.Pink:
                return new EnhancedMoodContribution { 
                    energy = 0.4f, happiness = 0.8f, darkness = 0.0f, calmness = 0.6f,
                    aggression = 0.0f, mystery = 0.2f, playfulness = 0.7f 
                };
            case SelectableShape.EmotionalColor.Brown:
                return new EnhancedMoodContribution { 
                    energy = 0.3f, happiness = 0.4f, darkness = 0.3f, calmness = 0.7f,
                    aggression = 0.2f, mystery = 0.4f, playfulness = 0.2f 
                };
            case SelectableShape.EmotionalColor.Gray:
                return new EnhancedMoodContribution { 
                    energy = 0.2f, happiness = 0.3f, darkness = 0.5f, calmness = 0.6f,
                    aggression = 0.1f, mystery = 0.6f, playfulness = 0.1f 
                };
            case SelectableShape.EmotionalColor.Teal:
                return new EnhancedMoodContribution { 
                    energy = 0.5f, happiness = 0.6f, darkness = 0.2f, calmness = 0.7f,
                    aggression = 0.1f, mystery = 0.5f, playfulness = 0.4f 
                };
            default:
                return new EnhancedMoodContribution { 
                    energy = 0.5f, happiness = 0.5f, darkness = 0.5f, calmness = 0.5f,
                    aggression = 0.3f, mystery = 0.3f, playfulness = 0.3f 
                };
        }
    }
    
    private SpatialInfluence CalculateSpatialInfluence(SelectableShape shape)
    {
        return new SpatialInfluence
        {
            height = shape.transform.position.y,
            spreadContribution = Vector3.Distance(shape.transform.position, Vector3.zero),
            clusteringEffect = CalculateClusteringEffect(shape)
        };
    }
    
    private float CalculateClusteringEffect(SelectableShape shape)
    {
        SelectableShape[] allShapes = FindObjectsOfType<SelectableShape>();
        int nearbyShapes = 0;
        
        foreach (SelectableShape otherShape in allShapes)
        {
            if (otherShape != shape)
            {
                float distance = Vector3.Distance(shape.transform.position, otherShape.transform.position);
                if (distance < clusterDistance)
                {
                    nearbyShapes++;
                }
            }
        }
        
        return nearbyShapes / (float)Mathf.Max(allShapes.Length - 1, 1);
    }
    
    private TemporalInfluence CalculateTemporalInfluence(SelectableShape shape)
    {
        return new TemporalInfluence
        {
            ageInfluence = Time.time - shape.advancedData.spawnTime,
            durationWeight = shape.advancedData.duration,
            loopingInfluence = shape.advancedData.isLooping ? 1f : 0f,
            animationTempo = GetAnimationTempo(shape.advancedData.animationType)
        };
    }
    
    private float GetAnimationTempo(SelectableShape.AnimationType animation)
    {
        switch (animation)
        {
            case SelectableShape.AnimationType.Bounce: return 1.2f; // Fast tempo
            case SelectableShape.AnimationType.Pulse: return 1.0f;  // Medium tempo
            case SelectableShape.AnimationType.Spin: return 0.8f;   // Steady tempo
            case SelectableShape.AnimationType.Float: return 0.6f;  // Slow tempo
            case SelectableShape.AnimationType.None: return 0.5f;   // Base tempo
            default: return 0.5f;
        }
    }
    
    private void NormalizeInfluences(List<EnhancedShapeInfluence> influences, out Vector3 centerOfMass)
    {
        float totalWeight = influences.Sum(i => i.weight);
        centerOfMass = Vector3.zero;
        
        // Calculate center of mass
        foreach (var influence in influences)
        {
            centerOfMass += influence.shape.transform.position * influence.weight;
        }
        centerOfMass /= totalWeight;
        
        // Normalize weights and update distance from center
        for (int i = 0; i < influences.Count; i++)
        {
            var influence = influences[i];
            influence.weight /= totalWeight;
            influence.spatialInfluence.distanceFromCenter = Vector3.Distance(
                influence.shape.transform.position, centerOfMass);
            influences[i] = influence;
        }
    }
    
    private CompositionData BlendEnhancedComposition(List<EnhancedShapeInfluence> influences)
    {
        CompositionData composition = new CompositionData();
        
        // Initialize blend accumulators
        float totalEnergy = 0f, totalCalmness = 0f, totalDarkness = 0f, totalHappiness = 0f;
        float totalAggression = 0f, totalMystery = 0f, totalPlayfulness = 0f;
        float totalIntensity = 0f, totalMovement = 0f;
        
        // Blend all mood contributions
        foreach (EnhancedShapeInfluence influence in influences)
        {
            EnhancedMoodContribution mood = influence.moodContribution;
            float weight = influence.weight;
            
            totalEnergy += mood.energy * weight;
            totalCalmness += mood.calmness * weight;
            totalDarkness += mood.darkness * weight;
            totalHappiness += mood.happiness * weight;
            totalAggression += mood.aggression * weight;
            totalMystery += mood.mystery * weight;
            totalPlayfulness += mood.playfulness * weight;
            totalIntensity += influence.shapeData.intensity * weight;
            totalMovement += influence.temporalInfluence.animationTempo * weight;
        }
        
        // Set composition values
        composition.energy = totalEnergy;
        composition.calmness = totalCalmness;
        composition.darkness = totalDarkness;
        composition.happiness = totalHappiness;
        composition.intensity = totalIntensity;
        composition.movement = totalMovement;
        composition.aggression = totalAggression;
        composition.mystery = totalMystery;
        composition.playfulness = totalPlayfulness;
        composition.complexity = Mathf.Min(influences.Count / 3f, 1f);
        
        // Determine moods
        composition.primaryMood = DeterminePrimaryMood(composition);
        composition.secondaryMood = DetermineSecondaryMood(composition);
        
        return composition;
    }
    
    private string DeterminePrimaryMood(CompositionData composition)
    {
        Dictionary<string, float> moodScores = new Dictionary<string, float>
        {
            {"energetic", composition.energy + composition.aggression * 0.5f},
            {"calm", composition.calmness + (1f - composition.energy) * 0.3f},
            {"dark", composition.darkness + composition.mystery * 0.4f},
            {"happy", composition.happiness + composition.playfulness * 0.3f},
            {"mysterious", composition.mystery + composition.darkness * 0.3f},
            {"aggressive", composition.aggression + composition.energy * 0.2f},
            {"playful", composition.playfulness + composition.happiness * 0.2f}
        };
        
        var primaryMood = moodScores.OrderByDescending(kvp => kvp.Value).First();
        
        if (enableDetailedLogging)
        {
            Debug.Log($"Primary mood calculation: {primaryMood.Key} = {primaryMood.Value:F2}");
        }
        
        return primaryMood.Key;
    }
    
    private string DetermineSecondaryMood(CompositionData composition)
    {
        Dictionary<string, float> moodScores = new Dictionary<string, float>
        {
            {"energetic", composition.energy},
            {"calm", composition.calmness},
            {"dark", composition.darkness},
            {"happy", composition.happiness},
            {"mysterious", composition.mystery},
            {"aggressive", composition.aggression},
            {"playful", composition.playfulness}
        };
        
        var sortedMoods = moodScores.OrderByDescending(kvp => kvp.Value).ToList();
        return sortedMoods.Count > 1 ? sortedMoods[1].Key : "balanced";
    }
    
    private CompositionData CalculateSpatialProperties(CompositionData composition, List<EnhancedShapeInfluence> influences)
    {
        if (influences.Count == 0) return composition;
        
        composition.averageHeight = influences.Average(i => i.spatialInfluence.height);
        composition.spread = influences.Max(i => i.spatialInfluence.spreadContribution);
        composition.clustering = influences.Average(i => i.spatialInfluence.clusteringEffect);
        
        if (enableDetailedLogging)
        {
            Debug.Log($"Spatial Properties - Height: {composition.averageHeight:F2}, " +
                     $"Spread: {composition.spread:F2}, Clustering: {composition.clustering:F2}");
        }
        
        return composition;
    }
    
    private CompositionData CalculateTemporalProperties(CompositionData composition, List<EnhancedShapeInfluence> influences)
    {
        if (influences.Count == 0) return composition;
        
        composition.averageDuration = influences.Average(i => i.temporalInfluence.durationWeight);
        composition.rhythmic = influences.Average(i => i.temporalInfluence.animationTempo);
        composition.hasLoopingElements = influences.Any(i => i.temporalInfluence.loopingInfluence > 0);
        
        // Determine tempo category
        float avgTempo = composition.rhythmic;
        if (avgTempo > 1.0f) composition.tempo = "fast";
        else if (avgTempo > 0.7f) composition.tempo = "medium";
        else composition.tempo = "slow";
        
        if (enableDetailedLogging)
        {
            Debug.Log($"Temporal Properties - Duration: {composition.averageDuration:F2}, " +
                     $"Tempo: {composition.tempo}, Looping: {composition.hasLoopingElements}");
        }
        
        return composition;
    }
    
    private CompositionData CalculateHarmonicProperties(CompositionData composition, List<EnhancedShapeInfluence> influences)
    {
        // Role distribution analysis
        var roleDistribution = influences.GroupBy(i => i.shapeData.musicalRole)
                                       .ToDictionary(g => g.Key, g => g.Sum(inf => inf.weight));
        
        if (roleDistribution.Count > 0)
        {
            var dominantRole = roleDistribution.OrderByDescending(kvp => kvp.Value).First();
            composition.dominantRole = dominantRole.Key.ToString();
        }
        
        // Material distribution analysis
        var materialDistribution = influences.GroupBy(i => i.shapeData.materialType)
                                           .ToDictionary(g => g.Key, g => g.Sum(i => i.weight));
        
        // Calculate material characteristics
        composition.metallic = materialDistribution.GetValueOrDefault(SelectableShape.MaterialType.Metallic, 0f);
        composition.organic = materialDistribution.GetValueOrDefault(SelectableShape.MaterialType.Wood, 0f) +
                             materialDistribution.GetValueOrDefault(SelectableShape.MaterialType.Fabric, 0f);
        composition.synthetic = materialDistribution.GetValueOrDefault(SelectableShape.MaterialType.Glass, 0f);
        composition.smooth = materialDistribution.GetValueOrDefault(SelectableShape.MaterialType.Smooth, 0f);
        composition.averageRoughness = influences.Average(i => i.shapeData.roughness);
        
        if (enableDetailedLogging)
        {
            Debug.Log($"Harmonic Properties - Dominant Role: {composition.dominantRole}, " +
                     $"Material Mix - Metallic: {composition.metallic:F2}, " +
                     $"Organic: {composition.organic:F2}, Synthetic: {composition.synthetic:F2}");
        }
        
        return composition;
    }
    
    private CompositionData GenerateComprehensiveTags(CompositionData composition)
    {
        List<string> tags = new List<string>();
        
        // Primary mood tag
        tags.Add(composition.primaryMood);
        
        // Secondary mood if significantly different
        if (composition.secondaryMood != composition.primaryMood && 
            composition.secondaryMood != "balanced")
        {
            tags.Add(composition.secondaryMood);
        }
        
        // Emotional dimension tags
        if (composition.darkness > 0.6f) tags.Add("dark");
        if (composition.happiness > 0.6f) tags.Add("uplifting");
        if (composition.calmness > 0.6f) tags.Add("ambient");
        if (composition.energy > 0.7f) tags.Add("energetic");
        if (composition.aggression > 0.6f) tags.Add("intense");
        if (composition.mystery > 0.6f) tags.Add("mysterious");
        if (composition.playfulness > 0.6f) tags.Add("playful");
        
        // Material-based tags
        if (composition.metallic > 0.5f) tags.Add("metallic");
        if (composition.organic > 0.5f) tags.Add("organic");
        if (composition.synthetic > 0.5f) tags.Add("synthetic");
        
        // Spatial tags
        if (composition.clustering > 0.6f) tags.Add("layered");
        if (composition.spread > 5f) tags.Add("expansive");
        
        // Complexity tags
        if (composition.complexity > 0.7f) tags.Add("complex");
        else if (composition.complexity < 0.3f) tags.Add("minimal");
        
        composition.tags = tags;
        return composition;
    }
    
    public string FormatDetailedCompositionInfo(CompositionData composition, SelectableShape[] shapes)
    {
        StringBuilder info = new StringBuilder();
        
        info.AppendLine("=== COMPREHENSIVE COMPOSITION ANALYSIS ===");
        info.AppendLine($"Shapes: {shapes.Length} | Center of Mass: {composition.centerOfMass}");
        info.AppendLine($"Primary Mood: {composition.primaryMood} | Secondary: {composition.secondaryMood}");
        info.AppendLine();
        
        info.AppendLine("EMOTIONAL DIMENSIONS:");
        info.AppendLine($"• Energy: {composition.energy:F2} | Calmness: {composition.calmness:F2}");
        info.AppendLine($"• Happiness: {composition.happiness:F2} | Darkness: {composition.darkness:F2}");
        info.AppendLine($"• Aggression: {composition.aggression:F2} | Mystery: {composition.mystery:F2}");
        info.AppendLine($"• Playfulness: {composition.playfulness:F2}");
        info.AppendLine();
        
        info.AppendLine("MUSICAL PROPERTIES:");
        info.AppendLine($"• Tempo: {composition.tempo} | Intensity: {composition.intensity:F2}");
        info.AppendLine($"• Complexity: {composition.complexity:F2} | Movement: {composition.movement:F2}");
        info.AppendLine($"• Dominant Role: {composition.dominantRole} | Looping: {composition.hasLoopingElements}");
        info.AppendLine($"• Generated Tags: {string.Join(", ", composition.tags)}");
        info.AppendLine();
        
        info.AppendLine("SPATIAL & MATERIAL:");
        info.AppendLine($"• Height: {composition.averageHeight:F2} | Spread: {composition.spread:F2} | Clustering: {composition.clustering:F2}");
        info.AppendLine($"• Metallic: {composition.metallic:F2} | Organic: {composition.organic:F2} | Synthetic: {composition.synthetic:F2}");
        info.AppendLine();
        
        info.AppendLine("INDIVIDUAL SHAPES:");
        foreach (SelectableShape shape in shapes)
        {
            info.AppendLine($"• {shape.shapeType}: {shape.emotionalColor} | {shape.advancedData.materialType} | {shape.advancedData.musicalRole}");
            info.AppendLine($"  Properties: Size={shape.shapeSize:F1}, Int={shape.advancedData.intensity:F2}, " +
                           $"Energy={shape.advancedData.energy:F2}, Anim={shape.advancedData.animationType}");
        }
        
        return info.ToString();
    }
    
    private CompositionData CreateEmptyComposition()
    {
        return new CompositionData
        {
            primaryMood = "neutral",
            secondaryMood = "balanced",
            tempo = "medium",
            energyLevel = "medium",
            tags = new List<string> { "neutral" }
        };
    }
    
    private void LogComprehensiveResults(CompositionData composition, SelectableShape[] shapes)
    {
        Debug.Log("=== COMPOSITION ANALYSIS COMPLETE ===");
        Debug.Log($"Primary: {composition.primaryMood} | Secondary: {composition.secondaryMood}");
        Debug.Log($"Energy: {composition.energy:F2} | Intensity: {composition.intensity:F2} | Complexity: {composition.complexity:F2}");
        Debug.Log($"Tempo: {composition.tempo} | Tags: [{string.Join(", ", composition.tags)}]");
        Debug.Log($"Shapes Analyzed: {shapes.Length}");
    }
}

// Enhanced data structures
[System.Serializable]
public class EnhancedShapeInfluence
{
    public SelectableShape.AdvancedShapeData shapeData;
    public SelectableShape shape;
    public float weight;
    public EnhancedMoodContribution moodContribution;
    public SpatialInfluence spatialInfluence;
    public TemporalInfluence temporalInfluence;
}

[System.Serializable]
public class WeightFactors
{
    public float sizeWeight;
    public float intensityWeight;
    public float energyWeight;
    public float roleWeight;
    public float materialWeight;
    public float animationWeight;
    public float durationWeight;
    
    public float GetCombinedWeight()
    {
        return sizeWeight * (1f + intensityWeight + energyWeight + roleWeight + materialWeight + animationWeight + durationWeight);
    }
}

[System.Serializable]
public class EnhancedMoodContribution
{
    public float energy;
    public float happiness;
    public float darkness;
    public float calmness;
    public float aggression;
    public float mystery;
    public float playfulness;
}

[System.Serializable]
public class SpatialInfluence
{
    public float height;
    public float spreadContribution;
    public float clusteringEffect;
    public float distanceFromCenter;
}

[System.Serializable]
public class TemporalInfluence
{
    public float ageInfluence;
    public float durationWeight;
    public float loopingInfluence;
    public float animationTempo;
}

// Enhanced CompositionData to include all new properties
[System.Serializable]
public class CompositionData
{
    [Header("Primary Mood Calculations")]
    public string primaryMood = "neutral";
    public string secondaryMood = "balanced";
    
    [Header("Core Emotional Dimensions")]
    public float energy = 0.5f;
    public float calmness = 0.5f;
    public float darkness = 0.5f;
    public float happiness = 0.5f;
    public float intensity = 0.5f;
    public float complexity = 0.5f;
    
    [Header("Extended Emotional Properties")]
    public float aggression = 0.3f;
    public float mystery = 0.3f;
    public float playfulness = 0.3f;
    
    [Header("Movement and Animation")]
    public float movement = 0.5f;
    public float rhythmic = 0.5f;
    public float smooth = 0.5f;
    
    [Header("Material Characteristics")]
    public float metallic = 0f;
    public float organic = 0f;
    public float synthetic = 0f;
    public float transparency = 0f;
    
    [Header("Musical Properties")]
    public float averageDuration = 4f;
    public float averageRoughness = 0f;
    public bool hasLoopingElements = false;
    public string dominantRole = "Harmony";
    
    [Header("Spatial Properties")]
    public float averageHeight = 0f;
    public float spread = 0f;
    public float clustering = 0f;
    public Vector3 centerOfMass = Vector3.zero;
    
    [Header("Derived Properties")]
    public string tempo = "medium";
    public string energyLevel = "medium";
    public List<string> tags = new List<string>();
}