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


    void Awake()
    {
        RandomFirstInterval(2,6);
        miniGame.SetActive(false);
        gameTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        gameTime += Time.deltaTime;
        timeInterval1 += Time.deltaTime; 
        if (gameTime >= endGame)
        {
            GetScore.ClearHitScore();
            miniGame.SetActive(false);
            gameTime = 0;
            sampleGenerate.SpawnSample();
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
        //Modify the angle to move the target
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
        miniGame.SetActive(true);
    }
    
}
