using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState
{
    Playing,
    Won,
    Lost,
    Paused
}

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float winSpeedThreshold = 25f; // Ball speed threshold to win (to be tuned based on table size)

    [Header("References")]
    [SerializeField] private BallController ballController;
    [SerializeField] private CardDisplay cardDisplay;

    [Header("Timing Modifiers")]
    [SerializeField] private float badModifier = 0.5f;
    [SerializeField] private float fineModifier = 0.8f;
    [SerializeField] private float goodModifier = 1.0f;
    [SerializeField] private float perfectModifier = 1.2f;

    [Header("Ball Reset")]
    [SerializeField] private Vector2 ballResetLocalPosition = new Vector2(4f, 0f); // Relative to parent (board)

    private GameState currentState = GameState.Playing;
    private bool canPlayerHit = true; // Track if ball is ready to be hit by player
    private InputSystem_Actions inputActions;
    private InputSystem_Actions.PlayerActions playerActions;
    
    // Store delegates to ensure proper cleanup
    private System.Action<InputAction.CallbackContext> onChoiceA;
    private System.Action<InputAction.CallbackContext> onChoiceB;
    private System.Action<InputAction.CallbackContext> onChoiceC;

    public GameState CurrentState => currentState;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        playerActions = inputActions.Player;

        // Store delegate instances for proper cleanup
        onChoiceA = _ => OnCardSelected(0);
        onChoiceB = _ => OnCardSelected(1);
        onChoiceC = _ => OnCardSelected(2);

        playerActions.ChoiceA.performed += onChoiceA;
        playerActions.ChoiceB.performed += onChoiceB;
        playerActions.ChoiceC.performed += onChoiceC;

        playerActions.Enable();
    }
    
    private void Update()
    {
        // Check for down arrow key or gamepad down to reshuffle cards
        if (currentState == GameState.Playing && (Keyboard.current != null && Keyboard.current.downArrowKey.wasPressedThisFrame))
        {
            ReshuffleCards();
        }
    }
    
    private void ReshuffleCards()
    {
        if (cardDisplay != null)
        {
            cardDisplay.GenerateNewCards();
        }
    }

    private void OnDestroy()
    {
        if (playerActions != null)
        {
            if (playerActions.ChoiceA != null && onChoiceA != null)
                playerActions.ChoiceA.performed -= onChoiceA;
            if (playerActions.ChoiceB != null && onChoiceB != null)
                playerActions.ChoiceB.performed -= onChoiceB;
            if (playerActions.ChoiceC != null && onChoiceC != null)
                playerActions.ChoiceC.performed -= onChoiceC;

            playerActions.Disable();
        }
        inputActions?.Dispose();
    }

    private void Start()
    {
        if (ballController == null)
        {
            ballController = FindObjectOfType<BallController>();
        }

        if (cardDisplay == null)
        {
            cardDisplay = FindObjectOfType<CardDisplay>();
        }
        
        // Initialize ball to starting position
        InitializeBall();

        // Initialize cards
        if (cardDisplay != null)
        {
            cardDisplay.GenerateNewCards();
        }
    }
    
    private void InitializeBall()
    {
        if (ballController != null)
        {
            // Use the ball's initial direction from BallController
            ballController.ResetBall(ballResetLocalPosition, ballController.InitialDirection);
        }
    }

    private void OnCardSelected(int cardIndex)
    {
        if (currentState != GameState.Playing)
            return;

        if (ballController == null || cardDisplay == null)
            return;

        // Check if ball is ready to be hit
        if (!canPlayerHit)
        {
            return;
        }

        // Get current hit quality from ball
        HitQuality hitQuality = ballController.GetCurrentHitQuality();

        // Check if miss
        if (hitQuality == HitQuality.Miss)
        {
            // Play missed sound
            if (SoundFXManager.instance != null && ballController != null)
            {
                SoundFXManager.instance.PlayMissedSound(ballController.transform);
            }
            Lose();
            return;
        }

        // Get card value
        int cardValue = cardDisplay.GetCardValue(cardIndex);
        if (cardValue < 0)
        {
            // Invalid card selection
            return;
        }

        // Apply timing modifier
        float timingModifier = GetTimingModifier(hitQuality);
        float speedMultiplier = timingModifier;

        // Modify ball speed - use baseSpeed if current speed is 0 (initial serve)
        float currentSpeed = ballController.CurrentSpeed;
        if (currentSpeed <= 0f)
        {
            currentSpeed = ballController.BaseSpeed;
        }
        float newSpeed = currentSpeed * speedMultiplier;
        ballController.SetSpeed(newSpeed);

        // Reverse ball direction
        ballController.ReverseDirectionX();
        
        // Clear all zones when ball reverses - ensures clean state for next return
        ballController.ClearAllZones();

        // Mark ball as not hittable until it returns from opponent
        canPlayerHit = false;

        // Debug logging
        Debug.Log($"Ball Hit - Card Value: {cardValue}, Hit Type: {hitQuality}, New Speed: {newSpeed:F2}");

        // Play sound
        if (SoundFXManager.instance != null)
        {
            SoundFXManager.instance.PlayPaddleSound(ballController.transform);
        }

        // Generate new cards for next round
        cardDisplay.GenerateNewCards();
    }

    private float GetTimingModifier(HitQuality hitQuality)
    {
        return hitQuality switch
        {
            HitQuality.Bad => badModifier,
            HitQuality.Fine => fineModifier,
            HitQuality.Good => goodModifier,
            HitQuality.Perfect => perfectModifier,
            _ => 1.0f
        };
    }

    public void OnBallHitOpponentCollider()
    {
        if (currentState != GameState.Playing)
            return;

        // Check if ball speed is high enough to win
        float currentBallSpeed = ballController != null ? ballController.CurrentSpeed : 0f;
        if (currentBallSpeed >= winSpeedThreshold)
        {
            Win();
        }
        else
        {
            // Opponent hit it back - reverse ball direction
            if (ballController != null)
            {
                ballController.ReverseDirectionX();
                // Don't clear zones here - let them register naturally as ball travels back
            }

            // Mark ball as ready to be hit by player again
            canPlayerHit = true;

            if (SoundFXManager.instance != null && ballController != null)
            {
                SoundFXManager.instance.PlayPaddleSound(ballController.transform);
            }
        }
    }

    private void Win()
    {
        currentState = GameState.Won;
        // Play missed sound (opponent missed)
        if (SoundFXManager.instance != null && ballController != null)
        {
            SoundFXManager.instance.PlayMissedSound(ballController.transform);
        }
        // Wait 5 seconds then reset
        StartCoroutine(ResetAfterDelay(5f));
        // TODO: Add win UI/effects
    }

    private void Lose()
    {
        currentState = GameState.Lost;
        // Wait 5 seconds then reset
        StartCoroutine(ResetAfterDelay(5f));
        // TODO: Add lose UI/effects
    }
    
    private System.Collections.IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetGame();
    }

    public void ResetGame()
    {
        currentState = GameState.Playing;
        canPlayerHit = true; // Reset hit state

        // Reset ball to starting position (same as initialization)
        InitializeBall();

        if (cardDisplay != null)
        {
            cardDisplay.GenerateNewCards();
        }
    }
}

