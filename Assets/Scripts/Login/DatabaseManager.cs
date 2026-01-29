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
                ["tools"] = new Dictionary<string, object>
                {
                    ["Repair_tool"] = false,
                    ["Wiper_tool"] = false,
                    ["Extractor"] = false
                },
                ["samples"] = new Dictionary<string, object>
                {
                    ["Water"] = 0,
                    ["Regolith"] = 0,
                    ["Smecite_Clay"] = 0,
                    ["Gypsum"] = 0,
                    ["Carbonate_Rock"] = 0,
                    ["Basalt"] = 0
                }
            }
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
}