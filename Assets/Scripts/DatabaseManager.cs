using UnityEngine;
using Firebase;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Manages interactions with the Firebase Realtime Database.
/// Provides methods for initializing, creating, loading, and updating user data.
/// Implements the Singleton pattern to ensure only one instance exists.
/// </summary>
public class DatabaseManager : MonoBehaviour
{
    private DatabaseReference databaseReference; // Reference to the Firebase Realtime Database root.
    private string currentUserId; // Currently logged-in user's ID.
    private Dictionary<string, object> userData; // Local cache of user data.
    public static DatabaseManager Instance { get; private set; } // Singleton instance
    
    /// <summary>
    /// Called when the script instance is being loaded.
    /// Implements the Singleton pattern to ensure only one instance exists.
    /// </summary>
    private void Awake()
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
    
    /// <summary>
    /// Initializes the Firebase Realtime Database connection.
    /// Checks and fixes Firebase dependencies asynchronously.
    /// </summary>
    /// <returns>
    /// A Task that represents the asynchronous operation.
    /// The task result contains true if initialization was successful, false otherwise.
    /// </returns>
    public async Task<bool> InitializeDatabase()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            Debug.Log("Database ready");
            return true;
        }
        else
        {
            Debug.LogError($"Database error: {dependencyStatus}");
            return false;
        }
    }
    
    /// <summary>
    /// Creates initial user data in the Firebase Realtime Database.
    /// Sets up the user's profile with email, account creation time, and last login time.
    /// </summary>
    /// <returns>
    /// A Task that represents the asynchronous operation.
    /// The task result contains true if user data was created successfully, false otherwise.
    /// </returns>
    public async Task<bool> CreateUserData(string userId, string email)
    {
        if (databaseReference == null) 
        {
            Debug.LogError("Database reference is null!");
            return false;
        }
        
        currentUserId = userId;
        
        // Create initial achievements structure
        Dictionary<string, object> initialAchievements = CreateInitialAchievements();
        
        // Simplified data structure - no redundant userId field
        userData = new Dictionary<string, object>
        {
            ["profile"] = new Dictionary<string, object>
            {
                ["email"] = email,
                ["accountCreated"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ["lastLogin"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            },
            ["inventory"] = new Dictionary<string, object>
            {
                ["tools"] = new List<object> 
                { 
                    "repairTool", 
                    "wiperTool", 
                    "extractor" 
                },
                ["samples"] = new Dictionary<string, object>
                {
                    ["water"] = new Dictionary<string, object>
                    {
                        ["amount"] = 0,
                        ["highScore"] = 0f
                    },
                    ["regolith"] = new Dictionary<string, object>
                    {
                        ["amount"] = 0,
                        ["highScore"] = 0f
                    },
                    ["smeciteClay"] = new Dictionary<string, object>
                    {
                        ["amount"] = 0,
                        ["highScore"] = 0f
                    },
                    ["gypsum"] = new Dictionary<string, object>
                    {
                        ["amount"] = 0,
                        ["highScore"] = 0f
                    },
                    ["carbonateRock"] = new Dictionary<string, object>
                    {
                        ["amount"] = 0,
                        ["highScore"] = 0f
                    },
                    ["basalt"] = new Dictionary<string, object>
                    {
                        ["amount"] = 0,
                        ["highScore"] = 0f
                    }
                }
            },
            ["scores"] = new Dictionary<string, object>
            {
                ["totalScore"] = 0f // Total score across all rock types
            },
            ["achievements"] = initialAchievements // Add achievements section
        };
        
        try
        {
            await databaseReference.Child("users").Child(userId).SetValueAsync(userData);
            Debug.Log($"User data created for {userId}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error creating user data: {e.Message}");
            Debug.LogError($"Full stack trace: {e.StackTrace}");
            return false;
        }
    }
    
    /// <summary>
    /// Creates the initial achievements data structure
    /// </summary>
    private Dictionary<string, object> CreateInitialAchievements()
    {
        return new Dictionary<string, object>
        {
            ["firstDrill"] = new Dictionary<string, object>
            {
                ["unlocked"] = false,
                ["unlockDate"] = "",
                ["score"] = 0f
            },
            ["perfectDrill"] = new Dictionary<string, object>
            {
                ["unlocked"] = false,
                ["unlockDate"] = "",
                ["score"] = 0f
            },
            ["regolithMaster"] = new Dictionary<string, object>
            {
                ["unlocked"] = false,
                ["unlockDate"] = "",
                ["score"] = 0f
            },
            ["basaltMaster"] = new Dictionary<string, object>
            {
                ["unlocked"] = false,
                ["unlockDate"] = "",
                ["score"] = 0f
            },
            ["gypsumMaster"] = new Dictionary<string, object>
            {
                ["unlocked"] = false,
                ["unlockDate"] = "",
                ["score"] = 0f
            },
            ["clayMaster"] = new Dictionary<string, object>
            {
                ["unlocked"] = false,
                ["unlockDate"] = "",
                ["score"] = 0f
            },
            ["carbonateMaster"] = new Dictionary<string, object>
            {
                ["unlocked"] = false,
                ["unlockDate"] = "",
                ["score"] = 0f
            },
            ["waterMaster"] = new Dictionary<string, object>
            {
                ["unlocked"] = false,
                ["unlockDate"] = "",
                ["score"] = 0f
            },
            ["allRocksMastered"] = new Dictionary<string, object>
            {
                ["unlocked"] = false,
                ["unlockDate"] = "",
                ["score"] = 0f
            }
        };
    }
    
    /// <summary>
    /// Loads user data from the Firebase Realtime Database for the specified user ID.
    /// Updates the last login timestamp after successfully loading the data.
    /// </summary>
    /// <returns>
    /// A Task that represents the asynchronous operation.
    /// The task result contains the user data as a Dictionary, or null if loading failed.
    /// </returns>
    public async Task<Dictionary<string, object>> LoadUserData(string userId)
    {   
        // Validate database reference
        if (databaseReference == null) 
        {
            Debug.LogError("Database reference is null!");
            return null;
        }
        
        currentUserId = userId;
        
        try
        {
            var snapshot = await databaseReference.Child("users").Child(userId).GetValueAsync();
            
            // Check if data exists
            if (snapshot.Exists && snapshot.Value != null)
            {
                userData = snapshot.Value as Dictionary<string, object>;
                
                if (userData == null)
                {
                    Debug.LogWarning($"User data exists but couldn't be parsed for {userId}");
                    return null;
                }
                
                Debug.Log($"User data loaded for {userId}");
                
                // Update last login time
                await UpdateUserField("profile/lastLogin", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                
                return userData;
            }
            else
            {
                Debug.LogWarning($"No data found for user {userId}");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading user data: {e.Message}");
            Debug.LogError($"Full stack trace: {e.StackTrace}");
            return null;
        }
    }
    
    /// <summary>
    /// Updates a specific field in the user's data in Firebase Realtime Database.
    /// Also updates the local cached data structure.
    /// </summary>
    /// <returns>
    /// A Task that represents the asynchronous operation.
    /// The task result contains true if the update was successful, false otherwise.
    /// </returns>
    public async Task<bool> UpdateUserField(string path, object value)
    {   
        // Validate inputs
        if (string.IsNullOrEmpty(currentUserId) || databaseReference == null) 
        {
            Debug.LogError($"Cannot update field: currentUserId={currentUserId}, databaseReference={databaseReference}");
            return false;
        }
        
        try
        {
            await databaseReference.Child("users").Child(currentUserId).Child(path).SetValueAsync(value);
            
            // Update local data
            UpdateLocalData(path, value);
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error updating field: {e.Message}");
            Debug.LogError($"Full stack trace: {e.StackTrace}");
            return false;
        }
    }
    
    /// <summary>
    /// Updates the local cached user data structure with a new value at the specified path.
    /// Creates missing dictionary entries if the path doesn't exist.
    /// </summary>
    private void UpdateLocalData(string path, object value)
    {   
        // Validate local data
        if (userData == null) return;
        
        var keys = path.Split('/');
        Dictionary<string, object> current = userData;
        
        // Navigate to the target path
        for (int i = 0; i < keys.Length - 1; i++)
        {   
            // Create missing path if it doesn't exist
            if (current == null || !current.ContainsKey(keys[i]))
            {
                // Create missing path if it doesn't exist
                current[keys[i]] = new Dictionary<string, object>();
            }
            
            current = current[keys[i]] as Dictionary<string, object>;
            
            // Safety check
            if (current == null)
            {
                Debug.LogError($"Cannot navigate path: {path} at key {keys[i]}");
                return;
            }
        }
        
        // Set the value at the target key
        if (current != null)
        {
            current[keys[keys.Length - 1]] = value;
        }
    }
    
    /// <summary>
    /// Gets the email address from the current user's cached data.
    /// </summary>
    /// <returns>The user's email address, or null if not found.</returns>
    public string GetUserEmail()
    {   
        if (userData == null || !userData.ContainsKey("profile")) 
        {
            Debug.LogWarning("User data or profile is null");
            return null;
        }
        
        var profile = userData["profile"] as Dictionary<string, object>;

        if (profile != null && profile.ContainsKey("email"))
        {
            return profile["email"] as string;
        }
        
        Debug.LogWarning("Email not found in profile");
        return null;
    }
    
    /// <summary>
    /// Gets the account creation date from the current user's cached data.
    /// </summary>
    /// <returns>The account creation date as a string, or null if not found.</returns>
    public string GetAccountCreatedDate()
    {
        if (userData == null || !userData.ContainsKey("profile")) 
        {
            Debug.LogWarning("User data or profile is null");
            return null;
        }
        
        var profile = userData["profile"] as Dictionary<string, object>;
        if (profile != null && profile.ContainsKey("accountCreated"))
        {
            return profile["accountCreated"] as string;
        }
        
        Debug.LogWarning("AccountCreated date not found in profile");
        return null;
    }
    
    /// <summary>
    /// Gets the last login date from the current user's cached data.
    /// </summary>
    /// <returns>The last login date as a string, or null if not found.</returns>
    public string GetLastLoginDate()
    {
        if (userData == null || !userData.ContainsKey("profile")) 
        {
            Debug.LogWarning("User data or profile is null");
            return null;
        }
        
        var profile = userData["profile"] as Dictionary<string, object>;
        if (profile != null && profile.ContainsKey("lastLogin"))
        {
            return profile["lastLogin"] as string;
        }
        
        Debug.LogWarning("LastLogin date not found in profile");
        return null;
    }
    
    /// <summary>
    /// Checks if the current user has cached data available.
    /// </summary>
    /// <returns>True if user data exists and contains a profile, false otherwise.</returns>
    public bool HasUserData()
    {
        return userData != null && userData.ContainsKey("profile");
    }
    
    /// <summary>
    /// Gets the current user ID that was last loaded or created.
    /// </summary>
    /// <returns>The current user ID, or null if no user is set.</returns>
    public string GetCurrentUserId()
    {
        return currentUserId;
    }
    
    /// <summary>
    /// Adds an inventory item to the user's data in Firebase.
    /// Uses switch case for different item types.
    /// </summary>
    /// <returns>
    /// A Task that represents the asynchronous operation.
    /// The task result contains true if the update was successful, false otherwise.
    /// </returns>
    public async Task<bool> AddInventoryItem(string itemTag, int amount = 1)
    {
        if (string.IsNullOrEmpty(currentUserId) || databaseReference == null)
        {
            Debug.LogError($"Cannot add item: currentUserId={currentUserId}, databaseReference={databaseReference}");
            return false;
        }
        
        if (userData == null || !userData.ContainsKey("inventory"))
        {
            Debug.LogError("User data or inventory not loaded");
            return false;
        }
        
        try
        {
            // Get the current inventory
            var inventory = userData["inventory"] as Dictionary<string, object>;
            if (inventory == null || !inventory.ContainsKey("samples"))
            {
                Debug.LogError("Inventory samples not found");
                return false;
            }
            
            var samples = inventory["samples"] as Dictionary<string, object>;
            if (samples == null)
            {
                Debug.LogError("Samples dictionary is null");
                return false;
            }
            
            string firebaseKey = "";
            
            // Use switch case to map item tag to Firebase key
            switch (itemTag)
            {
                case "Basalt":
                    firebaseKey = "basalt";
                    break;
                case "Water":
                    firebaseKey = "water";
                    break;
                case "Regolith":
                    firebaseKey = "regolith";
                    break;
                case "SmeciteClay":
                    firebaseKey = "smeciteClay";
                    break;
                case "Gypsum":
                    firebaseKey = "gypsum";
                    break;
                case "CarbonateRock":
                    firebaseKey = "carbonateRock";
                    break;
                default:
                    Debug.LogWarning($"Unknown item tag: {itemTag}");
                    return false;
            }
            
            // Get current amount - declare currentAmount here so it's accessible throughout the method
            int currentAmount = 0;
            Dictionary<string, object> sampleData = null;
            
            if (samples.ContainsKey(firebaseKey) && samples[firebaseKey] != null)
            {
                sampleData = samples[firebaseKey] as Dictionary<string, object>;
                if (sampleData != null && sampleData.ContainsKey("amount") && sampleData["amount"] != null)
                {
                    currentAmount = Convert.ToInt32(sampleData["amount"]);
                }
            }

            if (sampleData == null)
            {
                // Create new sample data if it doesn't exist
                sampleData = new Dictionary<string, object>
                {
                    ["amount"] = amount,
                    ["highScore"] = 0f
                };
            }
            else
            {
                // Calculate new amount
                int newAmount = currentAmount + amount;
                sampleData["amount"] = newAmount;
            }

            // Update the entire sample data dictionary in Firebase
            await UpdateUserField($"inventory/samples/{firebaseKey}/amount", currentAmount + amount);

            Debug.Log($"Added {amount} {itemTag} to inventory.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error adding inventory item: {e.Message}");
            Debug.LogError($"Full stack trace: {e.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Updates high score for a specific rock type and total score
    /// </summary>
    public async Task UpdateHighScore(string rockTag, float newScore)
    {
        if (userData == null || !userData.ContainsKey("inventory"))
        {
            Debug.LogError("User data or inventory not loaded");
            return;
        }
        
        string firebaseKey = "";
        
        // Use switch case to map rock tag to Firebase key
        switch (rockTag)
        {
            case "Basalt":
                firebaseKey = "basalt";
                break;
            case "Water":
                firebaseKey = "water";
                break;
            case "Regolith":
                firebaseKey = "regolith";
                break;
            case "SmeciteClay":
                firebaseKey = "smeciteClay";
                break;
            case "Gypsum":
                firebaseKey = "gypsum";
                break;
            case "CarbonateRock":
                firebaseKey = "carbonateRock";
                break;
            default:
                Debug.LogWarning($"Unknown rock tag: {rockTag}");
                return;
        }
        
        var inventory = userData["inventory"] as Dictionary<string, object>;
        if (inventory == null || !inventory.ContainsKey("samples"))
        {
            Debug.LogError("Inventory or samples not found");
            return;
        }
        
        var samples = inventory["samples"] as Dictionary<string, object>;
        if (samples == null || !samples.ContainsKey(firebaseKey))
        {
            Debug.LogError($"Sample data for {rockTag} not found");
            return;
        }
        
        var sampleData = samples[firebaseKey] as Dictionary<string, object>;
        if (sampleData == null)
        {
            Debug.LogError($"Sample data dictionary for {rockTag} is null");
            return;
        }
        
        // Get current high score from the sample data
        float currentHighScore = 0f;
        if (sampleData.ContainsKey("highScore") && sampleData["highScore"] != null)
        {
            currentHighScore = Convert.ToSingle(sampleData["highScore"]);
        }
        
        // Only update if the new score is better
        if (newScore > currentHighScore)
        {
            // Update the high score in the sample data
            sampleData["highScore"] = newScore;
            
            // Update the entire sample data dictionary
            await UpdateUserField($"inventory/samples/{firebaseKey}", sampleData);
            
            Debug.Log($"New High Score for {rockTag}: {newScore}!");
        }
        
        // Get current total score
        float currentTotalScore = 0f;
        if (userData.ContainsKey("scores"))
        {
            var scores = userData["scores"] as Dictionary<string, object>;
            if (scores != null && scores.ContainsKey("totalScore"))
            {
                currentTotalScore = Convert.ToSingle(scores["totalScore"]);
            }
        }

        // Calculate new total score
        float newTotalScore = currentTotalScore + newScore;
        
        // Update total score in Firebase
        await UpdateUserField("scores/totalScore", newTotalScore);
        
        // Trigger total score achievements
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.CheckTotalScoreAchievements(newTotalScore);
        }

        Debug.Log($"Added {newScore} to total score. Total: {currentTotalScore} → {newTotalScore}");
    }
    
    /// <summary>
    /// Unlocks an achievement in Firebase
    /// </summary>
    public async Task<bool> UnlockAchievement(string achievementId, float score = 0f)
    {
        if (string.IsNullOrEmpty(currentUserId) || databaseReference == null)
        {
            Debug.LogError($"Cannot unlock achievement: currentUserId={currentUserId}, databaseReference={databaseReference}");
            return false;
        }
        
        if (userData == null)
        {
            Debug.LogError("User data not loaded");
            return false;
        }
        
        try
        {
            // Ensure achievements section exists in user data
            if (!userData.ContainsKey("achievements"))
            {
                userData["achievements"] = CreateInitialAchievements();
            }
            
            var achievements = userData["achievements"] as Dictionary<string, object>;
            if (achievements == null || !achievements.ContainsKey(achievementId))
            {
                Debug.LogError($"Achievement {achievementId} not found in data structure");
                return false;
            }
            
            // Get the achievement data
            var achievementData = achievements[achievementId] as Dictionary<string, object>;
            if (achievementData == null)
            {
                achievementData = new Dictionary<string, object>();
            }
            
            // Update achievement data
            achievementData["unlocked"] = true;
            achievementData["unlockDate"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            achievementData["score"] = score;
            
            // Update in Firebase
            string path = $"achievements/{achievementId}";
            await UpdateUserField(path, achievementData);
            
            Debug.Log($"Achievement {achievementId} unlocked and saved to Firebase!");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error unlocking achievement: {e.Message}");
            Debug.LogError($"Full stack trace: {e.StackTrace}");
            return false;
        }
    }
    
    /// <summary>
    /// Gets all achievements from Firebase
    /// </summary>
    public async Task<Dictionary<string, object>> GetAchievements()
    {
        if (string.IsNullOrEmpty(currentUserId) || databaseReference == null)
        {
            Debug.LogError("Cannot get achievements: database not initialized");
            return null;
        }
        
        try
        {
            var snapshot = await databaseReference.Child("users").Child(currentUserId).Child("achievements").GetValueAsync();
            
            if (snapshot.Exists && snapshot.Value != null)
            {
                return snapshot.Value as Dictionary<string, object>;
            }
            
            // If no achievements exist, return initial structure
            return CreateInitialAchievements();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error getting achievements: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Checks if an achievement is already unlocked in Firebase
    /// </summary>
    public async Task<bool> IsAchievementUnlocked(string achievementId)
    {
        var achievements = await GetAchievements();
        
        if (achievements == null || !achievements.ContainsKey(achievementId))
        {
            return false;
        }
        
        var achievementData = achievements[achievementId] as Dictionary<string, object>;
        if (achievementData == null || !achievementData.ContainsKey("unlocked"))
        {
            return false;
        }
        
        object unlockedValue = achievementData["unlocked"];
        if (unlockedValue is bool)
        {
            return (bool)unlockedValue;
        }
        else if (unlockedValue is long)
        {
            return (long)unlockedValue == 1;
        }
        else if (unlockedValue is int)
        {
            return (int)unlockedValue == 1;
        }
        
        return false;
    }
    
    /// <summary>
    /// Gets the number of unlocked achievements
    /// </summary>
    public async Task<int> GetUnlockedAchievementCount()
    {
        var achievements = await GetAchievements();
        
        if (achievements == null)
        {
            return 0;
        }
        
        int count = 0;
        foreach (var achievement in achievements)
        {
            var achievementData = achievement.Value as Dictionary<string, object>;
            if (achievementData != null && achievementData.ContainsKey("unlocked"))
            {
                object unlockedValue = achievementData["unlocked"];
                bool isUnlocked = false;
                
                if (unlockedValue is bool)
                {
                    isUnlocked = (bool)unlockedValue;
                }
                else if (unlockedValue is long)
                {
                    isUnlocked = (long)unlockedValue == 1;
                }
                else if (unlockedValue is int)
                {
                    isUnlocked = (int)unlockedValue == 1;
                }
                
                if (isUnlocked)
                {
                    count++;
                }
            }
        }
        
        return count;
    }
}