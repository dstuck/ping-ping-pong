using UnityEngine;

public class BallController : MonoBehaviour
{
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private Vector2 initialDirection = Vector2.left;
    
    public Vector2 InitialDirection => initialDirection;

    private float currentSpeed;
    private Vector2 direction;
    
    public float BaseSpeed => baseSpeed;
    
    // Zone tracking - can be in multiple zones at once
    private bool inBadZone = false;
    private bool inFineZone = false;
    private bool inGoodZone = false;
    private bool inPerfectZone = false;

    public float CurrentSpeed => currentSpeed;
    public Vector2 Direction => direction;

    private void Start()
    {
        currentSpeed = 0f;
        direction = initialDirection.normalized;
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * currentSpeed * Time.deltaTime);
    }

    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
    }

    public void ReverseDirection()
    {
        direction = -direction;
    }

    public void ReverseDirectionX()
    {
        direction = new Vector2(-direction.x, direction.y);
    }

    // Zone entry/exit methods
    public void EnterBadZone() { inBadZone = true; }
    public void ExitBadZone() { inBadZone = false; }
    public void EnterFineZone() { inFineZone = true; }
    public void ExitFineZone() { inFineZone = false; }
    public void EnterGoodZone() { inGoodZone = true; }
    public void ExitGoodZone() { inGoodZone = false; }
    public void EnterPerfectZone() { inPerfectZone = true; }
    public void ExitPerfectZone() { inPerfectZone = false; }

    public HitQuality GetCurrentHitQuality()
    {
        // Priority: Perfect > Good > Fine > Bad > Miss
        if (inPerfectZone) return HitQuality.Perfect;
        if (inGoodZone) return HitQuality.Good;
        if (inFineZone) return HitQuality.Fine;
        if (inBadZone) return HitQuality.Bad;
        return HitQuality.Miss;
    }

    public void ResetBall(Vector2 localPosition, Vector2 newDirection)
    {
        // Set local position relative to parent (board)
        transform.localPosition = localPosition;
        direction = newDirection.normalized;
        currentSpeed = 0f; // Start stationary, waiting for first hit
        
        // Reset all zones - start in Good zone so first hit doesn't immediately miss
        inBadZone = false;
        inFineZone = false;
        inGoodZone = true; // Start in Good zone for initial serve
        inPerfectZone = false;
    }
    
    // Clear all zones - call this when ball reverses direction to ensure clean state
    public void ClearAllZones()
    {
        inBadZone = false;
        inFineZone = false;
        inGoodZone = false;
        inPerfectZone = false;
    }
    
    // Check if ball is currently in any zone
    public bool IsInAnyZone()
    {
        return inBadZone || inFineZone || inGoodZone || inPerfectZone;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Handle table bounces via trigger (if table collider is a trigger)
        if (other.CompareTag("Table"))
        {            
            // Play table hit sound
            if (SoundFXManager.instance != null)
            {
                SoundFXManager.instance.PlayHitTableSound(transform);
            }
        }
        
        // Handle opponent collider
        if (other.CompareTag("OpponentCollider"))
        {
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.OnBallHitOpponentCollider();
            }
        }
    }
}

