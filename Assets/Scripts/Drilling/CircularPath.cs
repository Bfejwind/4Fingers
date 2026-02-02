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

    void Awake()
    {
        RandomFirstInterval(2,6);
        miniGame.SetActive(false);
        gameTime = 0;
        currentRockTag = "";
        isGameActive = false;
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
            
            // Pass the tag HERE so the function has its own copy
            CalculateFinalScore(finalHitScore, currentRockTag); 
            
            gameTime = 0;
            miniGame.SetActive(false);
            GetScore.ClearHitScore(); 
            sampleGenerate.SpawnSample();
            currentRockTag = ""; // Now it's safe to clear this
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
        
        // 1. Add 1 rock to the inventory
        Debug.Log($"Adding inventory item: {rockTag}");
        bool inventorySuccess = await DatabaseManager.Instance.AddInventoryItem(rockTag, 1);
        Debug.Log($"Inventory add result: {inventorySuccess}");
        
        // 2. Update the High Score separately
        // This uses the UpdateHighScore method you added to DatabaseManager
        Debug.Log($"Updating high score for: {dbRockTag} = {finalScore}");
        await DatabaseManager.Instance.UpdateHighScore(dbRockTag, finalScore);
        
        Debug.Log($"Drilling Complete! Rock: {currentRockTag}, Points: {finalScore}");
        
        // Clear rock tag AFTER we've used it
        currentRockTag = "";
        Debug.Log("Cleared currentRockTag after calculation");
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
        speed = Random.Range(0.1f,10f);
    }
    
    public void ActivateDrillingGame()
    {   
        Debug.Log($"ActivateDrillingGame called. Current rock tag: {currentRockTag}");
        miniGame.SetActive(true);
        isGameActive = true;
        gameTime = 0; // Reset timer when game starts
        Debug.Log("MiniGame activated");
    }
}