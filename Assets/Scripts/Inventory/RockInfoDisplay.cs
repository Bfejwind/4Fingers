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
    public TMP_Text rockNameText;        // Top header - Rock name
    public TMP_Text slot1Text;           // Slot 1 (unlocks first)
    public TMP_Text slot2Text;           // Slot 2 (unlocks second)
    public TMP_Text slot3Text;           // Slot 3 (unlocks third)
    public TMP_Text purityText;          // Rock name + Drill Score
    public Image rockImage;
    public Button closeButton;
    
    [Header("Rock Information")]
    public List<RockInfo> rockInfos = new List<RockInfo>();
    
    [Header("Display Settings")]
    public float fadeDuration = 0.5f;
    
    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;
    
    [System.Serializable]
    public class RockInfo
    {
        public string tag; // Must match rock GameObject tags
        public string displayName;
        public Sprite image;
        
        // Three separate information slots
        [Header("Slot 1 - Unlocks at Level 1")]
        [TextArea(2, 4)]
        public string slot1Info;
        
        [Header("Slot 2 - Unlocks at Level 2")]
        [TextArea(2, 4)]
        public string slot2Info;
        
        [Header("Slot 3 - Unlocks at Level 3")]
        [TextArea(2, 4)]
        public string slot3Info;
        
        [HideInInspector]
        public int unlockedLevel = 0; // 0 = locked, 1 = basic, 2 = intermediate, 3 = advanced
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
        
        canvasGroup = infoPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = infoPanel.AddComponent<CanvasGroup>();
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HidePanel);
        }
        else
        {
            Debug.LogWarning("Close button not assigned!");
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
        
        if (scorePercentage >= 10f)
            newLevel = 3;
        else if (scorePercentage >= 5f)
            newLevel = 2;
        else if (scorePercentage >= 2f)
            newLevel = 1;
        
        if (newLevel > info.unlockedLevel)
        {
            info.unlockedLevel = newLevel;
            Debug.Log($"Unlocked Level {newLevel} info for {rockTag}!");
            
            SaveUnlockedLevel(rockTag, newLevel);
        }
    }
    
    /// <summary>
    /// Show rock info in the requested format: rock name, slot1, slot2, slot3, purity text
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
    
    private void UpdateUIWithSlots(RockInfo info)
    {
        // 1. TOP: Rock name header
        if (rockNameText != null)
        {
            rockNameText.text = info.displayName;
        }
        
        // 2. SLOT 1 - Always shown but content depends on level
        if (slot1Text != null)
        {
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
        
        // 3. SLOT 2 - Unlocks at level 2
        if (slot2Text != null)
        {
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
        
        // 4. BOTTOM SECTION: Rock name + Score Percentage
        if (purityText != null)
        {
            float savedScorePercentage = LoadScorePercentage(info.tag);
            purityText.text = $"{info.displayName}\nDrill Score: {savedScorePercentage:F1}%";
        }
        
        // 5. SLOT 3 - Unlocks at level 3 (below purity text)
        if (slot3Text != null)
        {
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
        
        // 6. Rock image
        if (rockImage != null)
        {
            rockImage.sprite = info.image != null ? info.image : null;
            rockImage.enabled = info.image != null;
        }
    }
    
    private float LoadScorePercentage(string rockTag)
    {
        string key = $"RockScore_{rockTag}_Percentage";
        return PlayerPrefs.GetFloat(key, 0f);
    }
    
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
    
    private void SaveUnlockedLevel(string rockTag, int level)
    {
        string key = $"RockInfo_{rockTag}_Level";
        PlayerPrefs.SetInt(key, level);
        PlayerPrefs.Save();
    }
    
    private int LoadUnlockedLevel(string rockTag)
    {
        string key = $"RockInfo_{rockTag}_Level";
        return PlayerPrefs.GetInt(key, 0);
    }
    
    private IEnumerator FadeInCoroutine()
    {
        infoPanel.SetActive(true);
        
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
    
    public void HidePanel()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        StartCoroutine(FadeOutCoroutine());
    }
    
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
