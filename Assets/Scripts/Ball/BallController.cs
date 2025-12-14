using UnityEngine;

public class BallController : MonoBehaviour
{
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private Vector2 initialDirection = Vector2.left;

    private float currentSpeed;
    private Vector2 direction;
    
    // Zone tracking - can be in multiple zones at once
    private bool inBadZone = false;
    private bool inFineZone = false;
    private bool inGoodZone = true;
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

    public void ReverseDirectionY()
    {
        direction = new Vector2(direction.x, -direction.y);
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

    public void ResetBall(Vector2 position, Vector2 newDirection)
    {
        transform.position = position;
        direction = newDirection.normalized;
        currentSpeed = baseSpeed;
        
        // Reset all zones
        inBadZone = false;
        inFineZone = false;
        inGoodZone = false;
        inPerfectZone = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle table bounces - reverse Y direction
        if (collision.gameObject.CompareTag("Table"))
        {
            ReverseDirectionY();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
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

