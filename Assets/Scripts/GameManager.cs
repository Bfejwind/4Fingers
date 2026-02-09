using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public float currentHealth;
    public float maxHealth;
    public ParticleSystem sparksMinor;
    public ParticleSystem sparksHalf;
    public ParticleSystem sparksLow;
    public string highHP = "#FFE700";
    public string halfHP = "#F97427";
    public string lowHP = "#e51414";
    public bool firstDrill;
    public GameObject drillTutorial;
    private bool firstDamage;
    public GameObject damageTutorial;
    public RightLine rightLine;
    public WiperBehaviour wiperScript;
    public GameObject movementScript;
    public GameObject wiperTutorial;
    public GameObject scannerTutorial;

    private void Awake()
    {
        // If an instance already exists and it's not this one, destroy this
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set instance
        Instance = this;

        // Persist across scenes
        DontDestroyOnLoad(gameObject);
    }
    /// <summary>
    /// Health Manager
    /// </summary>
    void Start()
    {
        maxHealth = 100;
        currentHealth = maxHealth;
        firstDrill = true;
        drillTutorial.SetActive(false);
        firstDamage = true;
        damageTutorial.SetActive(false);
        wiperTutorial.SetActive(false);
        scannerTutorial.SetActive(false);
    }
    public void TakeDamage(float dmg)
    {
        if (firstDamage)
        {
            PauseGame();
            damageTutorial.SetActive(true);
            firstDamage = false;
            rightLine.HoverOver();
        }
        currentHealth -= dmg;
        wiperScript.GettingDirty();
        currentHealth = Mathf.Clamp(currentHealth,0,maxHealth);
        if (currentHealth<= 0)
        {
            //Death effect
        }
        if (sparksMinor.isPlaying)
        {
            if (currentHealth <= maxHealth / 5)
            {
                sparksHalf.Play();
                SparksColorChange(lowHP);
            }
            else if (currentHealth <= maxHealth / 2)
            {
                sparksLow.Play();
                SparksColorChange(halfHP);
            }
        }
        else if (currentHealth < maxHealth)
        {
            //Start Sparks
            sparksMinor.Play();
        }
    }
    private void SparksColorChange(string hpLevel)
    {
        var sparksMain = sparksMinor.main;
        var sparksHalfMain = sparksHalf.main;
        var sparksLowMain = sparksLow.main;
        if (ColorUtility.TryParseHtmlString(hpLevel, out Color newColor))
        {
            sparksMain.startColor = new ParticleSystem.MinMaxGradient(newColor);
            sparksHalfMain.startColor = new ParticleSystem.MinMaxGradient(newColor);
            sparksLowMain.startColor = new ParticleSystem.MinMaxGradient(newColor);
        }
    }
    public void RepairHealth(float angle)
    {
        if (currentHealth < maxHealth)
        {
            currentHealth += angle;
            currentHealth = Mathf.Clamp(currentHealth,0,maxHealth);
            if (currentHealth == maxHealth)
            {
                //Stop Sparks
                sparksMinor.Stop();
            }
            else if (currentHealth >= maxHealth / 2)
            {
                sparksLow.Stop();
                SparksColorChange(highHP);
            }
            else if (currentHealth >= maxHealth / 5)
            {
                sparksHalf.Stop();
                SparksColorChange(halfHP);
            }
        }
    }
    public void DrillTutorial()
    {
        Debug.Log("Tutorial");
        if (firstDrill)
        {
            rightLine.HoverOver();
            drillTutorial.SetActive(true);
            firstDrill = false;
        }
    }
    public void PauseGame()
    {
        Time.timeScale = 0f;
        movementScript.SetActive(false);
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        movementScript.SetActive(true);
    }
    public void ScannerTutorial()
    {
        PauseGame();
        scannerTutorial.SetActive(true);
    }
    
}
