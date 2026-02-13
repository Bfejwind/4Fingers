using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.UI;

/// <summary>
/// Manages achievements: unlocking, displaying popups, and syncing with Firebase.
/// Implements singleton pattern for global access across scenes.
/// </summary>
public class AchievementManager : MonoBehaviour
{
    // Singleton instance for global access to the AchievementManager.
    public static AchievementManager Instance;
    [Header("UI References")]
    public GameObject achievementPopup;  // The popup panel GameObject that displays when an achievement is unlocked.
    public TMP_Text achievementTitleText; // Text component for displaying the achievement title in the popup.
    public TMP_Text achievementDescriptionText;  // Text component for displaying the achievement description in the popup.
    public UnityEngine.UI.Image achievementIconImage;  // Image component for displaying the achievement icon in the popup.
    public float displayDuration = 3f;  // Duration in seconds that the achievement popup remains visible.
    public float fadeDuration = 0.5f;  // Duration in seconds for the popup fade in/out animation.
    
    [Header("Achievement Settings")]
    public List<Achievement> achievements = new List<Achievement>();

    private CanvasGroup popupCanvasGroup; // CanvasGroup for controlling popup fade in/out
    private Coroutine currentDisplayCoroutine; // Reference to the currently running popup display coroutine
    private Dictionary<string, bool> unlockedAchievements = new Dictionary<string, bool>(); // Tracks unlocked achievements by ID
    private bool isLoadingFromFirebase = false; // Flag to indicate if loading from Firebase is in progress
    [System.Serializable]
    public class Achievement
    {

        public string id; // Unique identifier for the achievement
        public string title; // Title of the achievement
        public string description; // Description of the achievement
        [Header("Trigger Conditions")]
        public AchievementType triggerType; // Type of trigger for unlocking the achievement
        public string rockTag; // Relevant rock tag for rock-specific achievements
        public int collectionThreshold; // Number of samples required to unlock
        public int infoLevelThreshold;  // Info level required to unlock (1-3)
        public bool onlyOnce = true; // Whether the achievement can only be unlocked once
        [Header("Display Settings")]
        public Sprite icon; //  icon for the achievement
        public Color backgroundColor = Color.yellow; // Default to yellow
    }
    
    /// <summary>
    /// Enumeration of all possible achievement trigger types.
    /// Determines when and how an achievement is checked and unlocked.
    /// </summary>
    public enum AchievementType
    {
        //  Reach a specific info level for a rock type (1-3).
        RockMastery,
        //  Collect X number of samples total.
        CollectionCount,
        //  Complete the first successful drill.
        FirstDrill,
        //  Achieve a perfect score (100%) on any drill.
        PerfectDrill,
        //  Reach info level 3 for all rock types.
        AllRocksMastered,
        //   Collect all rock types with level 3 info.
        AllSamplesCollected
    }
    
    /// <summary>
    /// Initializes the singleton instance and sets up UI components.
    /// Called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {   
        // Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Setup CanvasGroup for fading
        popupCanvasGroup = achievementPopup.GetComponent<CanvasGroup>();
        if (popupCanvasGroup == null)
        {
            popupCanvasGroup = achievementPopup.AddComponent<CanvasGroup>();
        }
        
        achievementPopup.SetActive(false); // Hide popup initially
    }
    
    /// <summary>
    /// Called before the first frame update.
    /// Logs all achievement settings for debugging purposes.
    /// </summary>
    void Start()
    {
        // Debug all achievement settings
        DebugAchievementSettings();
    }
    
    /// <summary>
    /// Loads achievement progress for a specific user from Firebase or local storage.
    /// Should be called immediately after user login.
    /// </summary>
    /// <param name="userId">The unique identifier of the user. If null or empty, loads local-only achievements.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public async Task LoadAchievementsForUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("No user ID provided, loading local achievements only");
            LoadLocalAchievementProgress();
            return;
        }
        
        isLoadingFromFirebase = true;
        
        if (DatabaseManager.Instance != null)
        {
            try
            {
                // Load achievements from Firebase
                var firebaseAchievements = await DatabaseManager.Instance.GetAchievements();
                
                if (firebaseAchievements != null)
                {
                    // Clear current unlocked achievements
                    unlockedAchievements.Clear();
                    
                    // Sync from Firebase
                    foreach (var achievement in achievements)
                    {   
                        // Check if achievement exists in Firebase data
                        if (firebaseAchievements.ContainsKey(achievement.id))
                        {   
                            // Parse unlocked status
                            var achievementData = firebaseAchievements[achievement.id] as Dictionary<string, object>;
                            if (achievementData != null && achievementData.ContainsKey("unlocked"))
                            {
                                bool isUnlocked = false;
                                object unlockedValue = achievementData["unlocked"];
                                
                                if (unlockedValue is bool) isUnlocked = (bool)unlockedValue;
                                else if (unlockedValue is long) isUnlocked = (long)unlockedValue == 1;
                                else if (unlockedValue is int) isUnlocked = (int)unlockedValue == 1;
                                
                                unlockedAchievements[achievement.id] = isUnlocked;
                                
                                // Also save locally with user-specific key
                                string localKey = $"Achievement_{userId}_{achievement.id}";
                                PlayerPrefs.SetInt(localKey, isUnlocked ? 1 : 0);
                            }
                        }
                        else
                        {
                            // Achievement not in Firebase, default to false
                            unlockedAchievements[achievement.id] = false;
                        }
                    }
                    
                    PlayerPrefs.Save();
                    Debug.Log($"Loaded achievements from Firebase for user {userId}");
                }
                else
                {
                    Debug.LogWarning("Could not load achievements from Firebase, loading local");
                    LoadLocalAchievementProgress(userId);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading achievements from Firebase: {e.Message}");
                LoadLocalAchievementProgress(userId);
            }
        }
        else
        {
            Debug.LogWarning("DatabaseManager not available, loading local achievements");
            LoadLocalAchievementProgress(userId);
        }
        
        isLoadingFromFirebase = false;
    }
    
    /// <summary>
    /// Checks and unlocks achievements related to drill completion.
    /// Should be called whenever a drill operation is completed.
    /// </summary>
    /// <param name="rockTag">The tag of the rock that was drilled.</param>
    /// <param name="score">The raw score achieved in the drill.</param>
    /// <param name="percentage">The percentage score achieved (0-100%).</param>
    public async void CheckDrillAchievements(string rockTag, float score, float percentage)
    {   
        // Get count BEFORE incrementing for FirstDrill check
        int totalDrillsBefore = PlayerPrefs.GetInt("TotalDrills", 0);
        
        // Calculate info level from percentage (0-100%)
        int infoLevel = CalculateInfoLevel(percentage);
        
        Debug.Log($"Checking achievements for {rockTag}: {percentage}% = Info Level {infoLevel}");
        
        // Check for info-based achievements for this specific rock
        foreach (var achievement in achievements)
        {
            // Only check relevant achievement types
            if (ShouldCheckAchievement(achievement))
            {   
                // Check based on achievement type
                switch (achievement.triggerType)
                {   
                    // Check for RockMastery
                    case AchievementType.RockMastery:
                        if (rockTag == achievement.rockTag && infoLevel >= achievement.infoLevelThreshold)
                        {
                            await TryUnlockAchievement(achievement.id, score);
                        }
                        break;

                    // Check for FirstDrill
                    case AchievementType.FirstDrill:
                        // Check if this is the first drill (BEFORE incrementing)
                        if (totalDrillsBefore == 0)
                        {
                            await TryUnlockAchievement(achievement.id, score);
                        }
                        break;

                    // Check for PerfectDrill
                    case AchievementType.PerfectDrill:
                        // Check if this drill was perfect (100%)
                        if (percentage >= 100f)
                        {
                            await TryUnlockAchievement(achievement.id, score);
                        }
                        break;

                    // Check for AllRocksMastered
                    case AchievementType.AllRocksMastered:
                        // Check if ALL rocks have reached info level 3
                        if (infoLevel >= achievement.infoLevelThreshold && AreAllRocksAtInfoLevel(achievement.infoLevelThreshold))
                        {
                            await TryUnlockAchievement(achievement.id, score);
                        }
                        break;
                }
            }
        }
        
        // Now increment the drill count
        PlayerPrefs.SetInt("TotalDrills", totalDrillsBefore + 1);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Checks and unlocks achievements related to sample collection.
    /// Should be called whenever a sample is collected from a rock.
    /// </summary>
    /// <param name="rockTag">The tag of the rock from which the sample was collected.</param>
    public async void CheckCollectionAchievements(string rockTag)
    {
        // Update collection count for this rock
        string key = $"Collection_{rockTag}";
        int count = PlayerPrefs.GetInt(key, 0);
        PlayerPrefs.SetInt(key, count + 1);
        
        // Update total collection count
        int totalCollected = PlayerPrefs.GetInt("TotalSamplesCollected", 0);
        PlayerPrefs.SetInt("TotalSamplesCollected", totalCollected + 1);
        
        PlayerPrefs.Save();
        
        // Check collection achievements
        foreach (var achievement in achievements)
        {   
            // Only check relevant achievement types
            if (ShouldCheckAchievement(achievement))
            {   
                // Check based on achievement type
                switch (achievement.triggerType)
                {   
                    // Check for CollectionCount
                    case AchievementType.CollectionCount:
                        int totalCollections = PlayerPrefs.GetInt("TotalSamplesCollected", 0);
                        if (totalCollections >= achievement.collectionThreshold)
                        {
                            await TryUnlockAchievement(achievement.id, 0f);
                        }
                        break;

                    // Check for AllSamplesCollected
                    case AchievementType.AllSamplesCollected:
                        // Check if all rock types have been collected AND have level 3 info
                        if (AreAllSamplesCollectedWithLevel3())
                        {
                            await TryUnlockAchievement(achievement.id, 0f);
                        }
                        break;
                }
            }
        }
    }
    
    /// <summary>
    /// Calculates the info level achieved based on drill score percentage.
    /// </summary>
    /// <param name="percentage">Drill score percentage (0-100%).</param>
    /// <returns>Info level: 0 (locked), 1 (basic), 2 (intermediate), or 3 (advanced).</returns>
    private int CalculateInfoLevel(float percentage)
    {
        // Info Level Breakdown (3 levels only):
        // Level 1: 10% (Basic info unlocked) 
        // Level 2: 50% (More info unlocked)
        // Level 3: 80% (All info unlocked) 

        if (percentage >= 80f) return 3;    // Level 3 at 80%
        else if (percentage >= 50f) return 2;  // Level 2 at 50%
        else if (percentage >= 10f) return 1;  // Level 1 at 10%
        return 0;
    }
    
    /// <summary>
    /// Checks if all rock types have reached a specific info level.
    /// </summary>
    /// <param name="targetLevel">The minimum info level required for each rock.</param>
    /// <returns>True if all rocks meet or exceed the target level, false otherwise.</returns>
    private bool AreAllRocksAtInfoLevel(int targetLevel)
    {
        // If targetLevel is 0 or negative, return false (no achievement for level 0)
        if (targetLevel <= 0) return false;
        
        string[] allRocks = { "Basalt", "Regolith", "Gypsum", "SmeciteClay", "CarbonateRock", "Water" };
        
        // Check each rock's info level
        foreach (string rock in allRocks)
        {
            float percentage = PlayerPrefs.GetFloat($"RockScore_{rock}_Percentage", 0f);
            int rockLevel = CalculateInfoLevel(percentage);
            
            // If any rock is below the target level, return false
            if (rockLevel < targetLevel)
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Checks if all rock types have been collected at least once AND have reached info level 3.
    /// </summary>
    /// <returns>True if all rocks meet both collection and mastery requirements, false otherwise.</returns>
    private bool AreAllSamplesCollectedWithLevel3()
    {
        string[] allRocks = { "Basalt", "Regolith", "Gypsum", "SmeciteClay", "CarbonateRock", "Water" };
        
        foreach (string rock in allRocks)
        {
            // Check if collected at least once
            int collectionCount = PlayerPrefs.GetInt($"Collection_{rock}", 0);
            if (collectionCount == 0)
            {
                return false; // Not collected yet
            }
            
            // Check if has level 3 info
            float percentage = PlayerPrefs.GetFloat($"RockScore_{rock}_Percentage", 0f);
            int rockLevel = CalculateInfoLevel(percentage);
            if (rockLevel < 3)
            {
                return false; // Not at level 3 yet
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Checks and unlocks achievements related to total score.
    /// Kept for compatibility with legacy systems; currently not used.
    /// </summary>
    /// <param name="totalScore">The total cumulative score.</param>
    public async void CheckTotalScoreAchievements(float totalScore)
    {
        // Not used in info-based system, but kept for compatibility
    }
    
    /// <summary>
    /// Determines whether an achievement should be checked for unlocking.
    /// Considers if it's already unlocked and if it can only be unlocked once.
    /// </summary>
    /// <param name="achievement">The achievement to evaluate.</param>
    /// <returns>True if the achievement should be checked, false if it should be skipped.</returns>
    private bool ShouldCheckAchievement(Achievement achievement)
    {
        // Skip if already unlocked and onlyOnce is true
        if (achievement.onlyOnce && 
            unlockedAchievements.ContainsKey(achievement.id) && 
            unlockedAchievements[achievement.id])
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Attempts to unlock an achievement by ID.
    /// Checks Firebase for current unlock status, saves if newly unlocked, and displays popup.
    /// </summary>
    /// <param name="achievementId">The unique ID of the achievement to unlock.</param>
    /// <param name="score">The score associated with the achievement unlock.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    private async Task TryUnlockAchievement(string achievementId, float score = 0f)
    {   
        // Find the achievement
        Achievement achievement = achievements.Find(a => a.id == achievementId);
        // Safety check
        if (achievement == null)
        {
            Debug.LogWarning($"Achievement with ID {achievementId} not found!");
            return;
        }
        
        // Check if already unlocked in Firebase (if available)
        if (DatabaseManager.Instance != null)
        {
            try
            {
                bool alreadyUnlocked = await DatabaseManager.Instance.IsAchievementUnlocked(achievementId);
                
                // If not unlocked yet
                if (!alreadyUnlocked)
                {
                    // Mark as unlocked locally
                    unlockedAchievements[achievement.id] = true;
                    
                    // Save to Firebase
                    bool success = await DatabaseManager.Instance.UnlockAchievement(achievementId, score);
                    
                    // If successfully saved to Firebase
                    if (success)
                    {
                        // Also save locally with user-specific key
                        string userId = DatabaseManager.Instance.GetCurrentUserId();
                        if (!string.IsNullOrEmpty(userId))
                        {
                            string localKey = $"Achievement_{userId}_{achievementId}";
                            PlayerPrefs.SetInt(localKey, 1);
                            PlayerPrefs.Save();
                        }
                        
                        // Show popup
                        ShowAchievementPopup(achievement);
                        Debug.Log($"Achievement Unlocked: {achievement.title}");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to save achievement {achievementId} to Firebase");
                    }
                }
                // If already unlocked, don't show popup
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error checking/unlocking achievement: {e.Message}");
                // Fallback to local only
                UnlockAchievementLocal(achievement, score);
            }
        }
        else
        {
            // Fallback to local only
            UnlockAchievementLocal(achievement, score);
        }
    }
    
    /// <summary>
    /// Unlocks an achievement using only local storage (PlayerPrefs).
    /// Used as fallback when Firebase is unavailable.
    /// </summary>
    /// <param name="achievement">The achievement to unlock.</param>
    /// <param name="score">Optional score associated with the achievement.</param>
    private void UnlockAchievementLocal(Achievement achievement, float score)
    {
        unlockedAchievements[achievement.id] = true;
        
        // Save locally (device-specific, not user-specific since we don't know the user)
        PlayerPrefs.SetInt($"Achievement_{achievement.id}", 1);
        PlayerPrefs.Save();
        
        ShowAchievementPopup(achievement);
        Debug.Log($"Achievement Unlocked (Local): {achievement.title}");
    }
    
    /// <summary>
    /// Displays the achievement unlocked popup UI with fade animation.
    /// Updates all UI elements with the achievement's data.
    /// </summary>
    /// <param name="achievement">The achievement that was unlocked.</param>
    private void ShowAchievementPopup(Achievement achievement)
    {
        // Update UI text
        achievementTitleText.text = achievement.title;
        achievementDescriptionText.text = achievement.description;
        
        // Update icon if it exists
        if (achievementIconImage != null)
        {
            if (achievement.icon != null)
            {
                achievementIconImage.sprite = achievement.icon;
                achievementIconImage.enabled = true;  // Make sure it's visible
                achievementIconImage.color = Color.white; // Reset color if it was changed
            }
            else
            {
                achievementIconImage.enabled = false; // Hide if no icon
            }
        }
        
        // Also set background color if you want
        Image popupBackground = achievementPopup.GetComponent<Image>();
        if (popupBackground != null)
        {
            popupBackground.color = achievement.backgroundColor;
        }
        
        // Stop any existing coroutine
        if (currentDisplayCoroutine != null)
        {
            StopCoroutine(currentDisplayCoroutine);
        }
        
        // Start display coroutine
        currentDisplayCoroutine = StartCoroutine(DisplayPopupCoroutine());
    }
    
    /// <summary>
    /// Coroutine that handles the popup fade in, display, and fade out animation.
    /// </summary>
    /// <returns>IEnumerator for Unity coroutine system.</returns>
    private IEnumerator DisplayPopupCoroutine()
    {
        // Fade in
        achievementPopup.SetActive(true);
        popupCanvasGroup.alpha = 0f;
        
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            popupCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        popupCanvasGroup.alpha = 1f;
        
        // Wait for display duration
        yield return new WaitForSeconds(displayDuration);
        
        // Fade out
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            popupCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        popupCanvasGroup.alpha = 0f;
        
        achievementPopup.SetActive(false);
        currentDisplayCoroutine = null;
    }
    
    /// <summary>
    /// Loads achievement progress from local PlayerPrefs storage.
    /// Can load either user-specific or device-global progress.
    /// </summary>
    /// <param name="userId">Optional user ID for loading user-specific progress.</param>
    private void LoadLocalAchievementProgress(string userId = null)
    {
        unlockedAchievements.Clear(); // Clear current data
        
        // Load from PlayerPrefs
        foreach (var achievement in achievements)
        {
            string key;
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Try user-specific key first
                key = $"Achievement_{userId}_{achievement.id}";
                bool isUnlocked = PlayerPrefs.GetInt(key, 0) == 1;
                
                if (!isUnlocked)
                {
                    // Fallback to device-global key
                    key = $"Achievement_{achievement.id}";
                    isUnlocked = PlayerPrefs.GetInt(key, 0) == 1;
                }
                
                unlockedAchievements[achievement.id] = isUnlocked;
            }
            else
            {
                // Device-global only (for guest users)
                key = $"Achievement_{achievement.id}";
                bool isUnlocked = PlayerPrefs.GetInt(key, 0) == 1;
                unlockedAchievements[achievement.id] = isUnlocked;
            }
        }
        
        Debug.Log("Loaded local achievement progress");
    }
    
    /// <summary>
    /// Checks if a specific achievement has been unlocked.
    /// </summary>
    /// <param name="achievementId">The unique ID of the achievement to check.</param>
    /// <returns>True if the achievement is unlocked, false otherwise.</returns>
    public bool IsAchievementUnlocked(string achievementId)
    {
        return unlockedAchievements.ContainsKey(achievementId) && 
               unlockedAchievements[achievementId];
    }
    
    /// <summary>
    /// Gets a list of all unlocked achievement IDs.
    /// </summary>
    /// <returns>List of achievement IDs that have been unlocked.</returns>
    public List<string> GetUnlockedAchievementIds()
    {
        List<string> unlocked = new List<string>();
        foreach (var entry in unlockedAchievements)
        {
            if (entry.Value)
            {
                unlocked.Add(entry.Key);
            }
        }
        return unlocked;
    }
    
    /// <summary>
    /// Resets all achievements to locked state.
    /// Primarily used for testing purposes.
    /// </summary>
    /// <param name="userId">Optional user ID to clear user-specific save data.</param>
    public void ResetAchievements(string userId = null)
    {
        unlockedAchievements.Clear();
        
        // Clear from PlayerPrefs
        if (!string.IsNullOrEmpty(userId))
        {
            foreach (var achievement in achievements)
            {
                PlayerPrefs.DeleteKey($"Achievement_{userId}_{achievement.id}");
            }
        }
        
        foreach (var achievement in achievements)
        {
            PlayerPrefs.DeleteKey($"Achievement_{achievement.id}");
            unlockedAchievements[achievement.id] = false;
        }
        
        PlayerPrefs.Save();
        Debug.Log("Achievements reset");
    }

    /// <summary>
    /// Checks and logs the current drill count for new users.
    /// Used to verify drill tracking is working correctly.
    /// </summary>
    public void ResetDrillCountForNewUser()
    {
        // Check if this is a completely new user
        if (PlayerPrefs.GetInt("TotalDrills", 0) > 0)
        {
            Debug.Log($"User already has {PlayerPrefs.GetInt("TotalDrills")} drills recorded");
        }
        else
        {
            Debug.Log("New user - drill count is 0");
        }
    }

    /// <summary>
    /// Manually resets the total drill count to zero.
    /// Useful for testing and debugging drill-related achievements.
    /// </summary>
    public void ResetDrillCount()
    {
        PlayerPrefs.DeleteKey("TotalDrills");
        PlayerPrefs.Save();
        Debug.Log("RESET: TotalDrills set to 0");
    }
    
    /// <summary>
    /// Logs all achievement settings to the Unity Console for debugging.
    /// Displays ID, trigger type, rock tag, level requirement, and collection requirement.
    /// </summary>
    private void DebugAchievementSettings()
    {
        // Log all achievement settings
        Debug.Log("Achievement Settings:");
        foreach (var a in achievements)
        {
            Debug.Log($"{a.id}: Type={a.triggerType}, Rock='{a.rockTag}', LevelReq={a.infoLevelThreshold}, CollectionReq={a.collectionThreshold}");
        }
    }
}