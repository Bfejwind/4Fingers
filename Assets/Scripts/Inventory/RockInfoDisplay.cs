using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RockInfoDisplay : MonoBehaviour
{
    public static RockInfoDisplay Instance;
    
    [Header("UI References")]
    public GameObject infoPanel;
    public TMP_Text rockNameText;
    public TMP_Text descriptionText;
    public TMP_Text levelText; // Shows which level of info is unlocked
    public Image rockImage;
    public Button closeButton;
    
    [Header("Rock Information")]
    public List<RockInfo> rockInfos = new List<RockInfo>();
    
    [Header("Display Settings")]
    public float fadeDuration = 0.5f; // Duration for fade in/out
    
    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;
    
    [System.Serializable]
    public class RockInfo
    {
        public string tag; // Must match rock GameObject tags
        public string displayName;
        public Sprite image;
        
        // Progressive info levels
        [TextArea(2, 3)]
        public string level1Info; // Basic info (unlocks at 10% score)
        [TextArea(3, 4)]
        public string level2Info; // Intermediate info (unlocks at 50% score)
        [TextArea(4, 5)]
        public string level3Info; // Advanced info (unlocks at 80% score)
        
        [HideInInspector]
        public int unlockedLevel = 0; // 0 = locked, 1 = basic, 2 = intermediate, 3 = advanced
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep between scenes
        }
        else
        {
            Destroy(gameObject);
        }
        
        canvasGroup = infoPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = infoPanel.AddComponent<CanvasGroup>();
        }
        
        // Close button will manually close the panel
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HidePanel);
        }
        else
        {
            Debug.LogWarning("Close button not assigned - user won't be able to close the panel!");
        }
    }
    
    void Start()
    {
        infoPanel.SetActive(false);
        canvasGroup.alpha = 0f;
    }
    
    /// <summary>
    /// Check and update unlocked level based on high score percentage
    /// </summary>
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
        
        // Only update if new level is higher
        if (newLevel > info.unlockedLevel)
        {
            info.unlockedLevel = newLevel;
            Debug.Log($"Unlocked Level {newLevel} info for {rockTag}!");
            
            // Save to PlayerPrefs or database
            SaveUnlockedLevel(rockTag, newLevel);
        }
    }
    
    /// <summary>
    /// Show appropriate info based on unlocked level
    /// </summary>
    public void ShowRockInfo(string rockTag)
    {
        RockInfo info = GetRockInfoByTag(rockTag);
        if (info == null)
        {
            Debug.LogWarning($"No rock info found for tag: {rockTag}");
            return;
        }
        
        // Load saved unlocked level
        info.unlockedLevel = LoadUnlockedLevel(rockTag);
        
        // Update UI elements
        if (rockNameText != null)
            rockNameText.text = info.displayName;
        
        if (descriptionText != null)
            descriptionText.text = GetInfoForCurrentLevel(info);
        
        if (levelText != null)
            levelText.text = $"Info Level: {info.unlockedLevel}/3";
        
        if (rockImage != null && info.image != null)
            rockImage.sprite = info.image;
        
        // Color code based on level
        UpdateTextColor(info.unlockedLevel);
        
        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        // Fade in the panel (stays open until user closes it)
        fadeCoroutine = StartCoroutine(FadeInCoroutine());
    }
    
    private string GetInfoForCurrentLevel(RockInfo info)
    {
        switch (info.unlockedLevel)
        {
            case 3:
                return $"{info.level3Info}\n\n" + 
                    $"<color=#00FF00>Advanced Info Unlocked!</color>";
            
            case 2:
                return $"{info.level2Info}\n\n" + 
                    $"<color=#FFFF00>Intermediate Info</color>";
            
            case 1:
                return $"{info.level1Info}\n\n" +  
                    $"<color=#FF9900>Basic Info</color>";
            
            default:
                return $"<color=#FF0000>Collect this rock to unlock information!</color>\n" +
                    $"Drill score needed: 10%";
        }
    }
    
    private void UpdateTextColor(int level)
    {
        Color levelColor = Color.white;
        
        switch (level)
        {
            case 3: levelColor = new Color(0f, 1f, 0f); break; // Green
            case 2: levelColor = new Color(1f, 1f, 0f); break; // Yellow
            case 1: levelColor = new Color(1f, 0.6f, 0f); break; // Orange
            default: levelColor = new Color(1f, 0.3f, 0.3f); break; // Red
        }
        
        if (levelText != null)
            levelText.color = levelColor;
    }
    
    private RockInfo GetRockInfoByTag(string tag)
    {
        foreach (RockInfo info in rockInfos)
        {
            if (info.tag == tag || 
                info.tag.Replace("_", "") == tag.Replace("_", ""))
            {
                return info;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Save unlocked level to PlayerPrefs
    /// </summary>
    private void SaveUnlockedLevel(string rockTag, int level)
    {
        string key = $"RockInfo_{rockTag}_Level";
        PlayerPrefs.SetInt(key, level);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Load unlocked level from PlayerPrefs
    /// </summary>
    private int LoadUnlockedLevel(string rockTag)
    {
        string key = $"RockInfo_{rockTag}_Level";
        return PlayerPrefs.GetInt(key, 0);
    }
    
    /// <summary>
    /// Fade in the panel (no auto-close)
    /// </summary>
    private IEnumerator FadeInCoroutine()
    {
        // Activate panel
        infoPanel.SetActive(true);
        
        // Fade in
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        
    }
    
    /// <summary>
    /// User clicks close button to hide panel
    /// </summary>
    public void HidePanel()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        StartCoroutine(FadeOutCoroutine());
    }
    
    /// <summary>
    /// Fade out and hide panel
    /// </summary>
    private IEnumerator FadeOutCoroutine()
    {
        float timer = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        infoPanel.SetActive(false);
    }
    
    public static void ShowInfo(string rockTag)
    {
        if (Instance != null)
        {
            Instance.ShowRockInfo(rockTag);
        }
    }
    
    public static void UpdateInfoLevel(string rockTag, float scorePercentage)
    {
        if (Instance != null)
        {
            Instance.UpdateUnlockedLevel(rockTag, scorePercentage);
        }
    }
}