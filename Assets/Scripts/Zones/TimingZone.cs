using UnityEngine;

public enum HitQuality
{
    Miss,
    Bad,
    Fine,
    Good,
    Perfect
}

public class TimingZone : MonoBehaviour
{
    [Header("Zone Type")]
    [SerializeField] private HitQuality zoneType = HitQuality.Bad;
    
    [Header("Visual")]
    [SerializeField] private bool showVisual = true;
    private const float alpha = 0.1f;

    public HitQuality ZoneType => zoneType;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private GameObject visualChild;

    private void Start()
    {
        if (showVisual)
        {
            SetupVisual();
        }
    }

    private void SetupVisual()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogWarning($"TimingZone on {gameObject.name} has no BoxCollider2D for visual sizing");
            return;
        }

        // Create a child GameObject for the visual so we can scale it without affecting the collider
        visualChild = new GameObject("ZoneVisual");
        visualChild.transform.SetParent(transform);
        visualChild.transform.localPosition = Vector3.zero;
        visualChild.transform.localRotation = Quaternion.identity;
        
        spriteRenderer = visualChild.AddComponent<SpriteRenderer>();

        // Create a simple white sprite
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        spriteRenderer.sprite = sprite;

        // Scale the child transform (not the main transform) to match collider size
        // This way the collider stays at its original size
        Vector2 colliderSize = boxCollider.size;
        visualChild.transform.localScale = new Vector3(colliderSize.x, colliderSize.y, 1f);
        
        // Ensure the sprite renderer is set up correctly - render between table (0) and ball (20)
        spriteRenderer.sortingOrder = 10;

        // Color based on zone type
        Color zoneColor = GetZoneColor(zoneType);
        zoneColor.a = alpha;
        spriteRenderer.color = zoneColor;
    }

    private Color GetZoneColor(HitQuality quality)
    {
        return quality switch
        {
            HitQuality.Bad => Color.white,
            HitQuality.Fine => Color.white,
            HitQuality.Good => Color.white,
            HitQuality.Perfect => Color.blue,
            _ => Color.gray
        };
    }

    private void OnValidate()
    {
        // Update visual when zone type changes in editor
        if (Application.isPlaying && spriteRenderer != null)
        {
            Color zoneColor = GetZoneColor(zoneType);
            zoneColor.a = alpha;
            spriteRenderer.color = zoneColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var ballController = other.GetComponent<BallController>();
        if (ballController != null)
        {
            switch (zoneType)
            {
                case HitQuality.Bad:
                    ballController.EnterBadZone();
                    break;
                case HitQuality.Fine:
                    ballController.EnterFineZone();
                    break;
                case HitQuality.Good:
                    ballController.EnterGoodZone();
                    break;
                case HitQuality.Perfect:
                    ballController.EnterPerfectZone();
                    break;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var ballController = other.GetComponent<BallController>();
        if (ballController != null)
        {
            switch (zoneType)
            {
                case HitQuality.Bad:
                    ballController.ExitBadZone();
                    
                    // Check if ball is exiting the bad zone going to the right (missed ball)
                    // Ball moving right means positive X direction
                    if (ballController.Direction.x > 0)
                    {
                        // Ball is moving away from player - trigger missed ball
                        var gameManager = FindFirstObjectByType<GameManager>();
                        if (gameManager != null)
                        {
                            gameManager.OnBallMissed();
                        }
                    }
                    break;
                case HitQuality.Fine:
                    ballController.ExitFineZone();
                    break;
                case HitQuality.Good:
                    ballController.ExitGoodZone();
                    break;
                case HitQuality.Perfect:
                    ballController.ExitPerfectZone();
                    break;
            }
        }
    }
}
