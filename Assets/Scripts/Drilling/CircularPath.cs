using sc.terrain.proceduralpainter;
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
    private float time; //Apply intervals to movements


    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime; 
        // if (time>0.1f)
        //Calculate new position of target
        float x = centrePoint.position.x + Mathf.Cos(angle) * radius;
        float y = centrePoint.position.y + Mathf.Sin(angle) *radius;
        float z = centrePoint.position.z + depth;
        //Update the position of target
        transform.position = new Vector3(x,y,z);
        //Randomise speed of movement and direction
        RandomiseSpeedDirection();
        //Modify the angle to move the target
        if (posOrNeg < 4)
        {
            angle -= speed * Time.deltaTime;
        }
        else
        {
            angle += speed * Time.deltaTime;
        }
    }
    private void RandomiseSpeedDirection()
    {
        speed = Random.Range(0.1f,10f);
        posOrNeg = Random.Range(0,10);
    }
}
