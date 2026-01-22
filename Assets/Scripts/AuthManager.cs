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
    public SceneTransitionManager transitionManager; 
    public int sceneToLoadIndex = 1; // The index of your next scene
    public FirebaseAuth auth; // Firebase Authentication instance
    public FirebaseUser user; // Currently logged-in user

    // UI Elements
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject signupPanel;

    [Header("Login UI")]
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;
    public Button loginButton;
    public Button gotoSignupButton;
    public TextMeshProUGUI loginStatusText;

    [Header("Signup UI")]
    public TMP_InputField signupEmail;
    public TMP_InputField signupPassword;
    public TMP_InputField signupConfirmPassword;
    public Button signupButton;
    public Button gotoLoginButton;
    public TextMeshProUGUI signupStatusText;

    private bool isFirebaseReady = false;

    // Initialize Firebase and set up UI
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

    // Initialize Firebase Authentication
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
        }
        else
        {
            Debug.LogError($"Firebase error: {dependencyStatus}");
        }
    }

    // Show specified panel and hide others
    void ShowPanel(GameObject panel)
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        panel.SetActive(true);
    }

    /// Handle user signup
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
            UpdateStatus(signupStatusText, "Account created!", Color.green);
            ShowPanel(loginPanel); // Go back to login panel
        }
        catch (FirebaseException e)
        {
            if ((AuthError)e.ErrorCode == AuthError.EmailAlreadyInUse)
                UpdateStatus(signupStatusText, "Email already registered", Color.red);
            else
                UpdateStatus(signupStatusText, $"Signup failed: {e.Message}", Color.red);
        }
    }

    /// Handle user login
    public async void Login()
    {   
        // Check if Firebase is initialized
        if (!isFirebaseReady) { UpdateStatus(loginStatusText, "Firebase not ready", Color.red); return; }

        string email = loginEmail.text;
        string password = loginPassword.text;

        // Basic input validation

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

    // Update status text with message and color
    void UpdateStatus(TextMeshProUGUI textElement, string message, Color color)
    {   
        if (textElement != null)
        {
            textElement.text = message;
            textElement.color = color;
        }
    }
}
