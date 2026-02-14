using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the display of rock information panels based on player's drill scores.
/// Handles unlocking and displaying rock-specific scientific information as players progress.
/// </summary>
public class RockInfoDisplay : MonoBehaviour
{
    public static RockInfoDisplay Instance;
    
    [Header("UI References")]
    public GameObject infoPanel; // The main panel for displaying rock info
    public TMP_Text rockNameText; // Header text for rock name
    public TMP_Text slot1Text; // Text for information slot 1
    public TMP_Text slot2Text; // Text for information slot 2
    public TMP_Text slot3Text; // Text for information slot 3
    public TMP_Text purityText; // Text for rock purity and score percentage
    public Image rockImage; // Image component for rock picture
    public Button closeButton; // Button to close the info panel

    [Header("Rock Information")]
    public List<RockInfo> rockInfos = new List<RockInfo>(); // List of all rock information data
    
    [Header("Display Settings")]
    public float fadeDuration = 0.5f; // Duration for fade in/out animations
    private CanvasGroup canvasGroup; // CanvasGroup for fade effects
    private Coroutine fadeCoroutine; // Reference to the current fade coroutine
    [System.Serializable]
    public class RockInfo
    {
        public string tag; // Unique tag identifier for the rock
        public string displayName; // Display name of the rock
        public Sprite image; // Image representing the rock
        
        // Three separate information slots
        [Header("Slot 1 - Unlocks at Level 1")]
        [TextArea(2, 4)]
        public string slot1Info; // Text for information slot 1
        
        [Header("Slot 2 - Unlocks at Level 2")]
        [TextArea(2, 4)]
        public string slot2Info; // Text for information slot 2
        
        [Header("Slot 3 - Unlocks at Level 3")]
        [TextArea(2, 4)]
        public string slot3Info; // Text for information slot 3
        [HideInInspector]
        public int unlockedLevel = 0;
    }

    /// <summary>
    /// Initializes the singleton instance and sets up UI components.
    /// Called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {   
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this; 
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // add CanvasGroup for fade effects
        canvasGroup = infoPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = infoPanel.AddComponent<CanvasGroup>();
        }
        
        // Assign close button listener
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HidePanel);
        }
        else
        {
            Debug.LogWarning("Close button not assigned!");
        }
    }
    
    /// <summary>
    /// Initializes the panel to a hidden state.
    /// Called before the first frame update.
    /// </summary>
    void Start()
    {
        infoPanel.SetActive(false);
        canvasGroup.alpha = 0f;
    }
    
    /// <summary>
    /// Updates the unlocked information level for a specific rock based on drill performance.
    /// Saves the new level if it's higher than the current unlocked level.
    /// </summary>
    /// <param name="rockTag">The tag of the rock to update.</param>
    /// <param name="scorePercentage">The drill score percentage achieved (0-100%).</param>
    public void UpdateUnlockedLevel(string rockTag, float scorePercentage)
    {
        RockInfo info = GetRockInfoByTag(rockTag);
        if (info == null) return;
        
        int newLevel = 0;
        
        if (scorePercentage >= 80f)
            newLevel = 3;
        else if (scorePercentage >= 50f)
            newLevel = 2;
        else if (scorePercentage >= 10f)
            newLevel = 1;
        
        if (newLevel > info.unlockedLevel)
        {
            info.unlockedLevel = newLevel;
            Debug.Log($"Unlocked Level {newLevel} info for {rockTag}!");
            
            SaveUnlockedLevel(rockTag, newLevel);
        }
    }
    
    /// <summary>
    /// Displays the rock information panel for a specific rock type.
    /// Loads saved progress and updates all UI elements before fading in.
    /// </summary>
    /// <param name="rockTag">The tag of the rock to display information for.</param>
    public void ShowRockInfo(string rockTag)
    {   
        // Find rock info by tag
        RockInfo info = GetRockInfoByTag(rockTag);
        if (info == null)
        {
            Debug.LogWarning($"No rock info found for tag: {rockTag}");
            return;
        }
        
        // Load saved unlocked level
        info.unlockedLevel = LoadUnlockedLevel(rockTag);
        
        // Update all UI elements
        UpdateUIWithSlots(info);
        
        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        // Fade in the panel
        fadeCoroutine = StartCoroutine(FadeInCoroutine());
    }
    
    /// <summary>
    /// Updates all UI text and image elements with the rock's information.
    /// Shows locked/unlocked states based on current progress.
    /// </summary>
    /// <param name="info">The RockInfo object containing display data.</param>
    private void UpdateUIWithSlots(RockInfo info)
    {
        // Rock name header
        if (rockNameText != null)
        {
            rockNameText.text = info.displayName;
        }
        
        // Slot 1 - Unlocks at level 1
        if (slot1Text != null)
        {   
            // Level 1 unlock
            if (info.unlockedLevel >= 1)
            {
                slot1Text.text = info.slot1Info;
                slot1Text.color = Color.white;
            }
            else
            {
                slot1Text.text = "To Be Unlocked";
                slot1Text.color = new Color(1f, 0.3f, 0.3f); // Red for locked
            }
        }
        
        // Slot 2 - Unlocks at level 2
        if (slot2Text != null)
        {   
            // Level 2 unlock
            if (info.unlockedLevel >= 2)
            {
                slot2Text.text = info.slot2Info;
                slot2Text.color = Color.white;
            }
            else
            {
                slot2Text.text = "To Be Unlocked";
                slot2Text.color = new Color(1f, 0.3f, 0.3f); // Red for locked
            }
        }
        
        //  Rock name + Score Percentage
        if (purityText != null)
        {
            float savedScorePercentage = LoadScorePercentage(info.tag);
            purityText.text = $"{info.displayName}\nDrill Score: {savedScorePercentage:F1}%";
        }
        
        // Slot 3 - Unlocks at level 3
        if (slot3Text != null)
        {   
            // Level 3 unlock
            if (info.unlockedLevel >= 3)
            {
                slot3Text.text = info.slot3Info;
                slot3Text.color = Color.white;
            }
            else
            {
                slot3Text.text = "To Be Unlocked";
                slot3Text.color = new Color(1f, 0.3f, 0.3f); // Red for locked
            }
        }
        
        //  Rock image
        if (rockImage != null)
        {
            rockImage.sprite = info.image != null ? info.image : null;
            rockImage.enabled = info.image != null;
        }
    }
    
    /// <summary>
    /// Loads the saved drill score percentage for a specific rock from PlayerPrefs.
    /// </summary>
    /// <param name="rockTag">The tag of the rock to load the score for.</param>
    /// <returns>The saved score percentage, or 0 if no score exists.</returns>
    private float LoadScorePercentage(string rockTag)
    {
        string key = $"RockScore_{rockTag}_Percentage";
        return PlayerPrefs.GetFloat(key, 0f);
    }
    
    /// <summary>
    /// Retrieves the RockInfo object for a specific rock tag.
    /// Performs case-insensitive comparison and ignores underscores.
    /// </summary>
    /// <param name="tag">The rock tag to search for.</param>
    /// <returns>The matching RockInfo object, or null if not found.</returns>
    private RockInfo GetRockInfoByTag(string tag)
    {
        // Clean tag by removing underscores for comparison
        string cleanTag = tag.Replace("_", "").ToLower();
        
        foreach (RockInfo info in rockInfos)
        {
            string cleanInfoTag = info.tag.Replace("_", "").ToLower();
            if (cleanInfoTag == cleanTag)
            {
                return info;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Saves the unlocked information level for a specific rock to PlayerPrefs.
    /// </summary>
    /// <param name="rockTag">The tag of the rock to save.</param>
    /// <param name="level">The unlocked level to save (0-3).</param>
    private void SaveUnlockedLevel(string rockTag, int level)
    {
        string key = $"RockInfo_{rockTag}_Level";
        PlayerPrefs.SetInt(key, level);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Loads the saved unlocked information level for a specific rock from PlayerPrefs.
    /// </summary>
    /// <param name="rockTag">The tag of the rock to load the level for.</param>
    /// <returns>The saved unlocked level, or 0 if no level is saved.</returns>
    private int LoadUnlockedLevel(string rockTag)
    {
        string key = $"RockInfo_{rockTag}_Level";
        return PlayerPrefs.GetInt(key, 0);
    }
    
    /// <summary>
    /// Coroutine that handles the panel fade in animation.
    /// Smoothly transitions the canvas alpha from 0 to 1.
    /// </summary>
    /// <returns>IEnumerator for Unity coroutine system.</returns>
    private IEnumerator FadeInCoroutine()
    {
        infoPanel.SetActive(true); // Ensure panel is active
        
        float timer = 0f; // Reset timer
        // Fade from 0 to 1 alpha
        while (timer < fadeDuration) // Fade in
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f; // Ensure fully visible
    }
    
    /// <summary>
    /// Hides the information panel with a fade out animation.
    /// Public method typically called by the close button.
    /// </summary>
    public void HidePanel()
    {   
        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        StartCoroutine(FadeOutCoroutine()); // Start fade out
    }
    
    /// <summary>
    /// Coroutine that handles the panel fade out animation.
    /// Smoothly transitions the canvas alpha from current value to 0, then deactivates the panel.
    /// </summary>
    /// <returns>IEnumerator for Unity coroutine system.</returns>
    private IEnumerator FadeOutCoroutine()
    {
        float timer = 0f; // Reset timer
        float startAlpha = canvasGroup.alpha; // Current alpha
        
        // Fade from current alpha to 0
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime; // Increment timer
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f; // Ensure fully invisible
        infoPanel.SetActive(false); // Deactivate panel
    }
    
    /// <summary>
    /// Static convenience method to show rock information from any script.
    /// </summary>
    /// <param name="rockTag">The tag of the rock to display information for.</param>
    public static void ShowInfo(string rockTag)
    {   
        // Call instance method
        if (Instance != null)
        {
            Instance.ShowRockInfo(rockTag); // Show rock info
        }
    }
    
    /// <summary>
    /// Static convenience method to update unlocked level from any script.
    /// </summary>
    /// <param name="rockTag">The tag of the rock to update.</param>
    /// <param name="scorePercentage">The drill score percentage achieved.</param>
    public static void UpdateInfoLevel(string rockTag, float scorePercentage)
    {
        if (Instance != null)
        {
            Instance.UpdateUnlockedLevel(rockTag, scorePercentage);
        }
    }
}