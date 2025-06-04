using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EmotionalColorUtility
{
    public static Color GetEmotionalColor(SelectableShape.EmotionalColor emotion)
    {
        
        switch (emotion)
        {
            case SelectableShape.EmotionalColor.Red: return new Color(0.86f, 0.08f, 0.08f, 1f);      // Passion/Energy
            case SelectableShape.EmotionalColor.Orange: return new Color(1f, 0.55f, 0f, 1f);         // Joy/Creativity
            case SelectableShape.EmotionalColor.Yellow: return new Color(1f, 0.86f, 0f, 1f);         // Happiness/Cheerful
            case SelectableShape.EmotionalColor.Green: return new Color(0.2f, 0.7f, 0.2f, 1f);       // Calm/Natural
            case SelectableShape.EmotionalColor.Blue: return new Color(0.12f, 0.39f, 0.78f, 1f);     // Melancholy/Sad
            case SelectableShape.EmotionalColor.Purple: return new Color(0.59f, 0.2f, 0.78f, 1f);    // Mysterious/Spiritual
            case SelectableShape.EmotionalColor.Brown: return new Color(0.55f, 0.27f, 0.07f, 1f);    // Earthy/Grounded
            case SelectableShape.EmotionalColor.Pink: return new Color(1f, 0.71f, 0.76f, 1f);        // Romantic/Gentle
            case SelectableShape.EmotionalColor.Black: return new Color(0.12f, 0.12f, 0.12f, 1f);    // Dark/Intense
            case SelectableShape.EmotionalColor.White: return new Color(0.94f, 0.94f, 0.94f, 1f);    // Pure/Minimal
            case SelectableShape.EmotionalColor.Gray: return new Color(0.5f, 0.5f, 0.5f, 1f);        // Neutral/Contemplative
            case SelectableShape.EmotionalColor.Teal: return new Color(0f, 0.59f, 0.59f, 1f);        // Sophisticated/Modern
            default: return Color.white;
        }
    }
    
    public static string GetEmotionalDescription(SelectableShape.EmotionalColor emotion)
    {
        switch (emotion)
        {
            case SelectableShape.EmotionalColor.Red: return "Passion/Energy";
            case SelectableShape.EmotionalColor.Orange: return "Joy/Creativity";
            case SelectableShape.EmotionalColor.Yellow: return "Happiness/Cheerful";
            case SelectableShape.EmotionalColor.Green: return "Calm/Natural";
            case SelectableShape.EmotionalColor.Blue: return "Melancholy/Sad";
            case SelectableShape.EmotionalColor.Purple: return "Mysterious/Spiritual";
            case SelectableShape.EmotionalColor.Brown: return "Earthy/Grounded";
            case SelectableShape.EmotionalColor.Pink: return "Romantic/Gentle";
            case SelectableShape.EmotionalColor.Black: return "Dark/Intense";
            case SelectableShape.EmotionalColor.White: return "Pure/Minimal";
            case SelectableShape.EmotionalColor.Gray: return "Neutral/Contemplative";
            case SelectableShape.EmotionalColor.Teal: return "Sophisticated/Modern";
            default: return "Unknown";
        }
    }
}
