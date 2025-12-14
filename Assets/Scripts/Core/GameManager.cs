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
    [SerializeField] private float winSpeedPointsThreshold = 100f; // To be tuned based on table size
    [SerializeField] private float speedPointsIncrementPerCardValue = 1f; // Speed points added when card value > 50

    [Header("References")]
    [SerializeField] private BallController ballController;
    [SerializeField] private CardDisplay cardDisplay;
    [SerializeField] private SoundFXManager soundFXManager;

    [Header("Timing Modifiers")]
    [SerializeField] private float badModifier = 0.5f;
    [SerializeField] private float fineModifier = 0.8f;
    [SerializeField] private float goodModifier = 1.0f;
    [SerializeField] private float perfectModifier = 1.2f;

    [Header("Ball Reset")]
    [SerializeField] private Vector2 ballResetPosition = Vector2.zero;
    [SerializeField] private Vector2 ballResetDirection = Vector2.right;

    private GameState currentState = GameState.Playing;
    private float speedPoints = 0f;
    private InputSystem_Actions inputActions;
    private InputSystem_Actions.PlayerActions playerActions;
    
    // Store delegates to ensure proper cleanup
    private System.Action<InputAction.CallbackContext> onChoiceA;
    private System.Action<InputAction.CallbackContext> onChoiceB;
    private System.Action<InputAction.CallbackContext> onChoiceC;

    public GameState CurrentState => currentState;
    public float SpeedPoints => speedPoints;

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

    private void OnDestroy()
    {
        if (playerActions.ChoiceA != null && onChoiceA != null)
            playerActions.ChoiceA.performed -= onChoiceA;
        if (playerActions.ChoiceB != null && onChoiceB != null)
            playerActions.ChoiceB.performed -= onChoiceB;
        if (playerActions.ChoiceC != null && onChoiceC != null)
            playerActions.ChoiceC.performed -= onChoiceC;

        playerActions.Disable();
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

        if (soundFXManager == null)
        {
            soundFXManager = FindObjectOfType<SoundFXManager>();
        }

        // Initialize cards
        if (cardDisplay != null)
        {
            cardDisplay.GenerateNewCards();
        }
    }

    private void OnCardSelected(int cardIndex)
    {
        if (currentState != GameState.Playing)
            return;

        if (ballController == null || cardDisplay == null)
            return;

        // Get current hit quality from ball
        HitQuality hitQuality = ballController.GetCurrentHitQuality();

        // Check if miss
        if (hitQuality == HitQuality.Miss)
        {
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

        // Modify ball speed
        float newSpeed = ballController.CurrentSpeed * speedMultiplier;
        ballController.SetSpeed(newSpeed);

        // Increase speed points if card value > 50
        if (cardValue > 50)
        {
            speedPoints += speedPointsIncrementPerCardValue;
        }

        // Reverse ball direction
        ballController.ReverseDirectionX();

        // Play sound
        if (soundFXManager != null)
        {
            soundFXManager.PlayPlayerSwingSound();
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

        // Check if speed points are high enough to win
        if (speedPoints >= winSpeedPointsThreshold)
        {
            Win();
        }
        else
        {
            // Opponent hit it back - reverse ball direction
            if (ballController != null)
            {
                ballController.ReverseDirectionX();
            }

            if (soundFXManager != null)
            {
                soundFXManager.PlayOpponentHitSound();
            }
        }
    }

    private void Win()
    {
        currentState = GameState.Won;
        Debug.Log("You Win!");
        // TODO: Add win UI/effects
    }

    private void Lose()
    {
        currentState = GameState.Lost;
        Debug.Log("You Lose!");
        // TODO: Add lose UI/effects
    }

    public void ResetGame()
    {
        currentState = GameState.Playing;
        speedPoints = 0f;

        if (ballController != null)
        {
            ballController.ResetBall(ballResetPosition, ballResetDirection);
        }

        if (cardDisplay != null)
        {
            cardDisplay.GenerateNewCards();
        }
    }
}

