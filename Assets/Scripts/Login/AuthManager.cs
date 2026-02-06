using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine.UI;
using System;

/// <summary>
/// Manages user authentication using Firebase Authentication.
/// Provides UI for login and signup, and handles authentication logic.
/// </summary>

public class AuthManager : MonoBehaviour
{   
    public int sceneToLoadIndex = 1; // The index of your next scene
    public FirebaseAuth auth; // Firebase Authentication instance
    public FirebaseUser user; // Currently logged-in user
    public DatabaseManager databaseManager; // Reference to DatabaseManager
        public SceneTransitionManager transitionManager;  // Reference to SceneTransitionManager

    // UI Elements

    /// <summary>
    /// UI for authentication
    /// </summary>
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject signupPanel;

    /// <summary>
    /// UI elements for the login panel
    /// </summary>
    [Header("Login UI")]
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;
    public Button loginButton;
    public Button gotoSignupButton;
    public TextMeshProUGUI loginStatusText;


    /// <summary>
    /// UI elements for the signup panel
    /// </summary>
    [Header("Signup UI")]
    public TMP_InputField signupEmail;
    public TMP_InputField signupPassword;
    public TMP_InputField signupConfirmPassword;
    public Button signupButton;
    public Button gotoLoginButton;
    public TextMeshProUGUI signupStatusText;

    private bool isFirebaseReady = false; //Flag indicating whether Firebase has been successfully initialized

    /// <summary>
    /// Initializes Firebase and sets up UI button listeners.
    /// Called when the script instance is being loaded.
    /// </summary>
    private void Start()
    {
        InitializeFirebase();

        // Button listeners
        loginButton.onClick.AddListener(Login);
        signupButton.onClick.AddListener(SignUp);
        gotoSignupButton.onClick.AddListener(() => ShowPanel(signupPanel));
        gotoLoginButton.onClick.AddListener(() => ShowPanel(loginPanel));

        ShowPanel(loginPanel);
    }

    /// <summary>
    /// Initializes Firebase Authentication and checks dependencies.
    /// This is an async method that runs without blocking the main thread.
    /// </summary>
    async void InitializeFirebase()
    {
        // Check and fix Firebase dependencies
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        // If all dependencies are available, initialize Firebase Auth
        if (dependencyStatus == DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            isFirebaseReady = true;
            Debug.Log("Firebase ready");
            
            // Initialize DatabaseManager if it exists
            if (databaseManager != null)
            {
                bool dbInitialized = await databaseManager.InitializeDatabase();
                if (!dbInitialized)
                {
                    Debug.LogWarning("Database initialization failed, but continuing...");
                }
            }
            else
            {
                Debug.LogWarning("DatabaseManager not assigned to AuthManager!");
            }
        }
        else
        {
            Debug.LogError($"Firebase error: {dependencyStatus}");
        }
    }

    /// <summary>
    /// Shows the specified panel and hides all other authentication panels.
    /// </summary>
    void ShowPanel(GameObject panel)
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        panel.SetActive(true);
    }

    /// <summary>
    /// Handles user signup process.
    /// Validates input, creates Firebase user account, and creates user data in the database.
    /// </summary>
    public async void SignUp()
    {   
        // Check if Firebase is initialized
        if (!isFirebaseReady) { UpdateStatus(signupStatusText, "Firebase not ready", Color.red); return; }

        string email = signupEmail.text;
        string password = signupPassword.text;
        string confirmPassword = signupConfirmPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            UpdateStatus(signupStatusText, "Enter email and password", Color.red);
            return;
        }

        if (password != confirmPassword)
        {
            UpdateStatus(signupStatusText, "Passwords do not match", Color.red);
            return;
        }

        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = result.User;

            if (databaseManager != null)
            {
                // Call the database manager to create the user's initial record
                await databaseManager.CreateUserData(user.UserId, email);

                PlayerPrefs.DeleteKey("TotalDrills");
                PlayerPrefs.Save();
                Debug.Log("Drill count reset for new user: TotalDrills = 0");
                
                // Immediately load it to ensure lastLogin is set
                await databaseManager.LoadUserData(user.UserId);
            }

            UpdateStatus(signupStatusText, "Account created!", Color.green);
            
            // Switch to login panel
            ShowPanel(loginPanel);
            
            // Auto-fill login fields
            loginEmail.text = email;
            loginPassword.text = password;
            
        }
        catch (FirebaseException e)
        {
            if ((AuthError)e.ErrorCode == AuthError.EmailAlreadyInUse)
                UpdateStatus(signupStatusText, "Email already registered", Color.red);
            else
                UpdateStatus(signupStatusText, $"Signup failed: {e.Message}", Color.red);
        }
    }

    /// <summary>
    /// Handles user login process.
    /// Validates credentials, authenticates with Firebase, loads user data, and transitions to the next scene.
    /// </summary>
    public async void Login()
    {   
        // Check if Firebase is initialized
        if (!isFirebaseReady) { UpdateStatus(loginStatusText, "Firebase not ready", Color.red); return; }

        string email = loginEmail.text;
        string password = loginPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            UpdateStatus(loginStatusText, "Enter email and password", Color.red);
            return;
        }

        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            user = result.User;
            UpdateStatus(loginStatusText, "Login successful!", Color.green);

            // Load user data which updates lastLogin timestamp
            if (databaseManager != null)
            {
                // Load fresh data from Firebase (this will update lastLogin)
                var userData = await databaseManager.LoadUserData(user.UserId);
                
                if (userData == null)
                {
                    // User exists in Auth but not in Database - create their data
                    Debug.Log("User found in Auth but not in Database, creating data...");
                    await databaseManager.CreateUserData(user.UserId, email);
                }
            }

            // Transition to the next scene
            if (transitionManager != null)
            {
                transitionManager.GoToSceneAsync(sceneToLoadIndex);
            }
        }
        catch (Exception e)
        {
            UpdateStatus(loginStatusText, $"Login failed: {e.Message}", Color.red);
        }
    }

    /// <summary>
    /// Updates the status text element with a message and color.
    /// </summary>
    void UpdateStatus(TextMeshProUGUI textElement, string message, Color color)
    {   
        if (textElement != null)
        {
            textElement.text = message;
            textElement.color = color;
        }
    }
}
