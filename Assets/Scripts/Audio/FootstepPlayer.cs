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

        if (sfxBank != null)
        {
            sfxBank.Load();
            Debug.Log("SFX bank loaded");
        }
        else
        {
            Debug.LogError("sfxBank is null");
        }
    }

    void FixedUpdate()
    {
        float distanceMoved = Vector3.Distance(rb.position, lastPosition);
        bool isMoving = distanceMoved > movementThreshold;
        Debug.Log($"[Footstep Debug] distance: {distanceMoved:F5} | threshold: {movementThreshold} | isMoving: {isMoving} | stepTimer: {stepTimer:F2}");

        if (isMoving)
        {
            stopTimer = 0f;
            stepTimer += Time.fixedDeltaTime;

            if (stepTimer >= stepInterval)
            {
                if (footstepEvent != null)
                {
                    footstepEvent.Post(gameObject);
                    Debug.Log("Footstep played");
                }
                else
                {
                    Debug.LogError("footstepEvent is null");
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
                Debug.Log("Footstep stoped");
                stepTimer = 0f;
            }
        }

        lastPosition = rb.position;
    }

}