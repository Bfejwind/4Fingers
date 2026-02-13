using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.UI;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;
    
    [Header("UI References")]
    public GameObject achievementPopup;
    public TMP_Text achievementTitleText;
    public TMP_Text achievementDescriptionText;
    public UnityEngine.UI.Image achievementIconImage;
    public float displayDuration = 3f;
    public float fadeDuration = 0.5f;
    
    [Header("Achievement Settings")]
    public List<Achievement> achievements = new List<Achievement>();
    
    private CanvasGroup popupCanvasGroup;
    private Coroutine currentDisplayCoroutine;
    private Dictionary<string, bool> unlockedAchievements = new Dictionary<string, bool>();
    private bool isLoadingFromFirebase = false;
    
    [System.Serializable]
    public class Achievement
    {
        public string id; // Unique ID for the achievement
        public string title;
        public string description;
        
        [Header("Trigger Conditions")]
        public AchievementType triggerType;
        public string rockTag; // For rock-specific achievements
        public int collectionThreshold; // Minimum collection count
        public int infoLevelThreshold; // Minimum info level (1-3) - CHANGED FROM 1-4
        public bool onlyOnce = true; // Can only be unlocked once
        
        [Header("Display Settings")]
        public Sprite icon;
        public Color backgroundColor = Color.yellow;
    }
    
    public enum AchievementType
    {
        RockMastery,        // Reach a specific info level for a rock type (1-3)
        CollectionCount,    // Collect X number of samples
        FirstDrill,         // First successful drill
        PerfectDrill,       // Perfect score on any drill (100%)
        AllRocksMastered,   // Reach info level 3 for all rock types
        AllSamplesCollected // NEW: Collect all rock types with level 3 info
    }
    
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
        
        popupCanvasGroup = achievementPopup.GetComponent<CanvasGroup>();
        if (popupCanvasGroup == null)
        {
            popupCanvasGroup = achievementPopup.AddComponent<CanvasGroup>();
        }
        
        achievementPopup.SetActive(false);
    }
    
    void Start()
    {
        // Debug all achievement settings
        DebugAchievementSettings();
    }
    
    // Call this when user logs in
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
                        if (firebaseAchievements.ContainsKey(achievement.id))
                        {
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
    
    // Call this when a drill is completed
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
            if (ShouldCheckAchievement(achievement))
            {
                switch (achievement.triggerType)
                {
                    case AchievementType.RockMastery:
                        if (rockTag == achievement.rockTag && infoLevel >= achievement.infoLevelThreshold)
                        {
                            await TryUnlockAchievement(achievement.id, score);
                        }
                        break;
                        
                    case AchievementType.FirstDrill:
                        // Check if this is the first drill (BEFORE incrementing)
                        if (totalDrillsBefore == 0)
                        {
                            await TryUnlockAchievement(achievement.id, score);
                        }
                        break;
                        
                    case AchievementType.PerfectDrill:
                        // Check if this drill was perfect (100%)
                        if (percentage >= 100f)
                        {
                            await TryUnlockAchievement(achievement.id, score);
                        }
                        break;
                        
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
    
    // Call this when a sample is collected
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
            if (ShouldCheckAchievement(achievement))
            {
                switch (achievement.triggerType)
                {
                    case AchievementType.CollectionCount:
                        int totalCollections = PlayerPrefs.GetInt("TotalSamplesCollected", 0);
                        if (totalCollections >= achievement.collectionThreshold)
                        {
                            await TryUnlockAchievement(achievement.id, 0f);
                        }
                        break;
                        
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
    
    // Calculate info level from percentage (0-100%)
    private int CalculateInfoLevel(float percentage)
    {
        // Info Level Breakdown (3 levels only):
        // Level 1: 10% (Basic info unlocked) - CHANGED from >0% to 10%
        // Level 2: 50% (More info unlocked) - CHANGED from 26% to 50%
        // Level 3: 80% (All info unlocked) - CHANGED from 51% to 80%
        // For testing: 2%, 5%, 10% (change these to your testing values)
        
        // For FINAL GAME (use these):
        // if (percentage >= 80f) return 3;
        // else if (percentage >= 50f) return 2;
        // else if (percentage >= 10f) return 1;
        // return 0;
        
        // For TESTING (use these):
        if (percentage >= 10f) return 3;    // Level 3 at 10%
        else if (percentage >= 5f) return 2;  // Level 2 at 5%
        else if (percentage >= 2f) return 1;  // Level 1 at 2%
        return 0;
    }
    
    // Check if all rocks have reached a specific info level
    private bool AreAllRocksAtInfoLevel(int targetLevel)
    {
        // If targetLevel is 0 or negative, return false (no achievement for level 0)
        if (targetLevel <= 0) return false;
        
        string[] allRocks = { "Basalt", "Regolith", "Gypsum", "SmeciteClay", "CarbonateRock", "Water" };
        
        foreach (string rock in allRocks)
        {
            float percentage = PlayerPrefs.GetFloat($"RockScore_{rock}_Percentage", 0f);
            int rockLevel = CalculateInfoLevel(percentage);
            
            if (rockLevel < targetLevel)
            {
                return false;
            }
        }
        
        return true;
    }
    
    // Check if all rock types have been collected AND have level 3 info
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
    
    // Call this when total score is updated (kept for compatibility)
    public async void CheckTotalScoreAchievements(float totalScore)
    {
        // Not used in info-based system, but kept for compatibility
    }
    
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
    
    private async Task TryUnlockAchievement(string achievementId, float score = 0f)
    {
        Achievement achievement = achievements.Find(a => a.id == achievementId);
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
                
                if (!alreadyUnlocked)
                {
                    // Mark as unlocked locally
                    unlockedAchievements[achievement.id] = true;
                    
                    // Save to Firebase
                    bool success = await DatabaseManager.Instance.UnlockAchievement(achievementId, score);
                    
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
    
    private void UnlockAchievementLocal(Achievement achievement, float score)
    {
        unlockedAchievements[achievement.id] = true;
        
        // Save locally (device-specific, not user-specific since we don't know the user)
        PlayerPrefs.SetInt($"Achievement_{achievement.id}", 1);
        PlayerPrefs.Save();
        
        ShowAchievementPopup(achievement);
        Debug.Log($"Achievement Unlocked (Local): {achievement.title}");
    }
    
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
                // Optional: Hide or show default icon when no icon is assigned
                achievementIconImage.enabled = false; // Hide if no icon
                // OR keep enabled with default sprite
                // achievementIconImage.sprite = defaultIcon;
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
    
    private void LoadLocalAchievementProgress(string userId = null)
    {
        unlockedAchievements.Clear();
        
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
    
    // Helper method to check if an achievement is unlocked
    public bool IsAchievementUnlocked(string achievementId)
    {
        return unlockedAchievements.ContainsKey(achievementId) && 
               unlockedAchievements[achievementId];
    }
    
    // Get all unlocked achievement IDs
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
    
    // Reset achievements (for testing)
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

    // Public method to manually reset drill count
    public void ResetDrillCount()
    {
        PlayerPrefs.DeleteKey("TotalDrills");
        PlayerPrefs.Save();
        Debug.Log("RESET: TotalDrills set to 0");
    }
    
    // NEW: Debug method to check achievement settings
    private void DebugAchievementSettings()
    {
        Debug.Log("=== ACHIEVEMENT SETTINGS ===");
        foreach (var a in achievements)
        {
            Debug.Log($"{a.id}: Type={a.triggerType}, Rock='{a.rockTag}', LevelReq={a.infoLevelThreshold}, CollectionReq={a.collectionThreshold}");
        }
    }
}