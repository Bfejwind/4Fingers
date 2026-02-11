using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class SimpleXRFootsteps : MonoBehaviour
{
    public AudioClip[] footstepClips;
    public float stepInterval = 0.5f;
    public float minMoveSpeed = 0.1f;

    CharacterController controller;
    AudioSource audioSource;
    float timer;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        Vector3 velocity = controller.velocity;
        velocity.y = 0f;

        if (velocity.magnitude > minMoveSpeed && controller.isGrounded)
        {
            timer += Time.deltaTime;

            if (timer >= stepInterval)
            {
                audioSource.PlayOneShot(
                    footstepClips[Random.Range(0, footstepClips.Length)]
                );
                timer = 0f;
            }
        }
        else
        {
            timer = 0f;
        }
    }
}
