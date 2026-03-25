using UnityEngine;

public class FootstepPlayer : MonoBehaviour
{
    public AK.Wwise.Event footstepEvent;
    public AK.Wwise.Event stopfootstepEvent;
    public AK.Wwise.Bank sfxBank;

    public float stepInterval = 0.4f;
    public float movementThreshold = 0.001f;
    public float stopGraceTime = 0.1f;

    private float stepTimer = 0f;
    private float stopTimer = 0f;
    private Rigidbody rb;
    private Vector3 lastPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lastPosition = rb.position;

        
    }

    void FixedUpdate()
    {
        float distanceMoved = Vector3.Distance(rb.position, lastPosition);
        bool isMoving = distanceMoved > movementThreshold;
       

        if (isMoving)
        {
            stopTimer = 0f;
            stepTimer += Time.fixedDeltaTime;

            if (stepTimer >= stepInterval)
            {
                if (footstepEvent != null)
                {
                    footstepEvent.Post(gameObject);
                    
                }
                else
                {
                   
                }

                stepTimer = 0f;
            }
        }
        else
        {
            stopTimer += Time.fixedDeltaTime;

            if (stopTimer >= stopGraceTime)
            {
                //stopfootstepEvent.Post(gameObject);
               
                stepTimer = 0f;
            }
        }

        lastPosition = rb.position;
    }

}