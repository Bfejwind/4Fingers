using sc.terrain.proceduralpainter;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class CircularPath : MonoBehaviour
{
    public Transform centrePoint; //Rotate around this
    private float speed; //Speed of movement
    private int posOrNeg; //Direction determinant
    [SerializeField]
    float radius; //Radius of circular path
    [SerializeField]
    float angle; //Tracks current angle of target
    [SerializeField]
    float depth; //Adjust depth of target
    private float timeInterval1;
    private float timeInterval2; //Track time
    public float firstInterval;
    public float secondInterval;
    public GameObject miniGame;
    public float gameTime;
    public float endGame = 10.0f;
    public DrillingScoreTracking GetScore;
    public RockSampleExtracted sampleGenerate; 
    public string currentRockTag; //Tag of current rock being drilled
    
    // Add a flag to track if game is active
    private bool isGameActive = false;
    
    // NEW: Score Tracking
    public float maxPossibleScore = 1000f; // Adjust based on your game
    public AudioClip rockBreakSFX;
    private bool firstDrill;
    public GameObject drillTutorial;

    void Awake()
    {
        RandomFirstInterval(2,6);
        miniGame.SetActive(false);
        gameTime = 0;
        currentRockTag = "";
        isGameActive = false;
        firstDrill = true;
        drillTutorial.SetActive(false);
        Debug.Log("CircularPath Awake: Initialized with no rock tag");
    }   
    
    public void HoverEnter(UnityEngine.XR.Interaction.Toolkit.HoverEnterEventArgs args)
    {
        // Get the tag of the object being hovered
        string detectedTag = args.interactableObject.transform.tag;
        
        // Set the tag so the game knows what rock we are drilling
        currentRockTag = detectedTag; 
        
        // Start the game logic
        ActivateDrillingGame();
    }
        
    void OnTriggerExit(Collider other)
    {
        Debug.Log($"Drill exited: {other.tag} | Current rock tag: {currentRockTag}");
    }

    // Update is called once per frame
    void Update()
    {   
        if (!miniGame.activeSelf) 
        {
            // If game is not active but isGameActive is true, reset it
            if (isGameActive)
            {
                Debug.LogWarning("Game object deactivated but isGameActive flag was still true. Resetting.");
                isGameActive = false;
                currentRockTag = "";
            }
            return;
        }
        
        if (!isGameActive)
        {
            Debug.LogWarning("Game object is active but isGameActive flag is false. Activating.");
            isGameActive = true;
        }
        
        gameTime += Time.deltaTime;
        timeInterval1 += Time.deltaTime; 
        
        if (gameTime >= endGame)
        {   
            float finalHitScore = GetScore.GetCurrentTotalScore(); 
            
            // Create a local copy BEFORE clearing
            string rockTagCopy = currentRockTag;
            
            // Pass the copy
            CalculateFinalScore(finalHitScore, rockTagCopy); 
            
            gameTime = 0;
            AudioSource.PlayClipAtPoint(rockBreakSFX,transform.position);
            miniGame.SetActive(false);
            GetScore.ClearHitScore(); 
            sampleGenerate.SpawnSample();
            currentRockTag = ""; 
        }
        
        if (timeInterval1 > firstInterval)
        {
            CalculatePosition();
            angle -= speed * Time.deltaTime;
            RandomSecondInterval(2,6);
            timeInterval2 += Time.deltaTime;
            if (timeInterval2 > secondInterval)
            {
                RandomFirstInterval(2,6);
                timeInterval2 = 0;
                timeInterval1 = 0;
            }

        }
        else
        {
            CalculatePosition();
            angle += speed * Time.deltaTime;
        }
        
        //Randomise speed of movement and direction
        RandomiseSpeed();
    }
    
    private void CalculatePosition()
    {
        //Calculate new position of target
        float x = centrePoint.position.x + Mathf.Cos(angle) * radius;
        float y = centrePoint.position.y + Mathf.Sin(angle) *radius;
        float z = centrePoint.position.z + depth;
        //Update the position of target
        transform.position = new Vector3(x,y,z);
    }
    
    async void CalculateFinalScore(float timeOnTarget, string rockTag)
    {   
        if (string.IsNullOrEmpty(rockTag))
        {
            Debug.LogError("Score failed: rockTag was empty!");
            return;
        }

        Debug.Log($"CalculateFinalScore called with: timeOnTarget={timeOnTarget}, rockTag={rockTag}");
        
        // Round timeOnTarget to 2 decimal places
        timeOnTarget = Mathf.Round(timeOnTarget * 100f) / 100f;
        
        int basePoints = 0;
        string dbRockTag = rockTag;
        switch (rockTag)
        {
            case "Regolith": 
                basePoints = 100;
                dbRockTag = "Regolith";
                break;
            case "Basalt": 
                basePoints = 100;
                dbRockTag = "Basalt";
                break;
            case "Gypsum": 
                basePoints = 300;
                dbRockTag = "Gypsum";
                break;
            case "Smecite_Clay": 
                basePoints = 300;
                dbRockTag = "Smecite_Clay"; // Convert to database format
                break;
            case "Carbonate_Rock": 
                basePoints = 500;
                dbRockTag = "Carbonate_Rock"; // Convert to database format
                break;
            case "Water": 
                basePoints = -500;
                dbRockTag = "Water";
                break;
            default:
                Debug.LogWarning($"Unknown rock tag in switch: {currentRockTag}");
                basePoints = 0;
                break;
        }

        float finalScore = basePoints * timeOnTarget;
        
        // Debug log to see what's being calculated
        Debug.Log($"Calculating score: Rock={currentRockTag} (DB: {dbRockTag}), timeOnTarget={timeOnTarget}, basePoints={basePoints}, finalScore={finalScore}");
        
        // Check DatabaseManager
        if (DatabaseManager.Instance == null)
        {
            Debug.LogError("DatabaseManager.Instance is null!");
            currentRockTag = ""; // Clear after use
            return;
        }
        
        // Update the High Score separately
        // This uses the UpdateHighScore method you added to DatabaseManager
        Debug.Log($"Updating high score for: {dbRockTag} = {finalScore}");
        await DatabaseManager.Instance.UpdateHighScore(dbRockTag, finalScore);
        
        // Calculate score percentage and update unlocked info level
        float scorePercentage = CalculateScorePercentage(dbRockTag, finalScore, maxPossibleScore);
        Debug.Log($"Score Percentage for {dbRockTag}: {scorePercentage}%");

        // Save score percentage to Playerprefs
        PlayerPrefs.SetFloat($"RockScore_{dbRockTag}_Percentage", scorePercentage);
        PlayerPrefs.Save();

        // Update unlocked info level
        RockInfoDisplay.UpdateInfoLevel(dbRockTag, scorePercentage);

        // Trigger achievement checks
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.CheckDrillAchievements(dbRockTag, finalScore, scorePercentage);
        }
        
        Debug.Log($"Drilling Complete! Rock: {currentRockTag}, Points: {finalScore}, Percentage: {scorePercentage}%");
        

    }
    
    private void RandomFirstInterval(float min,float max)
    {
        firstInterval = Random.Range(min,max);
    }
    
    private void RandomSecondInterval(float min,float max)
    {
        secondInterval= Random.Range(min,max);
    }
    
    private void RandomiseSpeed()
    {
        speed = Random.Range(0.1f,4f);
    }
    
    public void ActivateDrillingGame()
    {   
        // Shoot a ray downwards to see what we are standing on
        RaycastHit hit;
        // Adjust Vector3.down and the distance (5f) based on your drill height
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 5f))
        {
            currentRockTag = hit.transform.tag;
            Debug.Log($"Raycast found rock: {currentRockTag}");
        }
        else if (string.IsNullOrEmpty(currentRockTag))
        {
            Debug.LogWarning("Game started but no rock detected via Raycast or Hover!");
        }

        miniGame.SetActive(true);
        isGameActive = true;
        gameTime = 0; 
    }
    
    // NEW: Helper method to calculate score percentage
    private float CalculateScorePercentage(string rockTag, float currentScore, float maxPossible)
    {
        // Different max scores per rock type if needed
        float rockMaxScore = maxPossible;
        
        // Adjust per rock type if you want different thresholds
        switch (rockTag)
        {
            case "Basalt":
            case "Regolith":
                rockMaxScore = 800f;
                break;
            case "Gypsum":
            case "Smecite_Clay":
                rockMaxScore = 1200f;
                break;
            case "Carbonate_Rock":
                rockMaxScore = 1500f;
                break;
            case "Water":
                rockMaxScore = 600f;
                break;
        }
        
        float percentage = (currentScore / rockMaxScore) * 100f;
        return Mathf.Clamp(percentage, 0f, 100f);
    }
    public void DrillTutorial()
    {
        Debug.Log("Tutorial");
        if (firstDrill)
        {
            drillTutorial.SetActive(true);
            firstDrill = false;
        }
    }
}