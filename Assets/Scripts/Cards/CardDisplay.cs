using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    [Header("Card Prefab")]
    [SerializeField] private GameObject cardPrefab;

    private int[] currentCardValues = new int[3];
    private Card[] cards = new Card[3];

    // Card positions as anchor positions (relative to parent)
    private readonly Vector2[] cardPositions = new Vector2[]
    {
        new Vector2(0.2f, 0.3f), // Left card
        new Vector2(0.5f, 0.7f), // Center card
        new Vector2(0.8f, 0.3f)  // Right card
    };

    private void Start()
    {
        CreateCards();
        GenerateNewCards();
    }

    private void CreateCards()
    {
        if (cardPrefab == null)
        {
            Debug.LogError("CardDisplay: Card prefab is not assigned!");
            return;
        }

        RectTransform parentRect = GetComponent<RectTransform>();
        if (parentRect == null)
        {
            Debug.LogError("CardDisplay: Parent must have a RectTransform component!");
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            GameObject cardInstance = Instantiate(cardPrefab, transform);
            Card card = cardInstance.GetComponent<Card>();
            
            if (card == null)
            {
                Debug.LogError($"CardDisplay: Card prefab {i} does not have a Card component!");
                continue;
            }

            RectTransform cardRect = cardInstance.GetComponent<RectTransform>();
            if (cardRect == null)
            {
                Debug.LogError($"CardDisplay: Card prefab {i} does not have a RectTransform component!");
                continue;
            }

            // Set anchor to the specified position, pivot to center
            cardRect.anchorMin = cardPositions[i];
            cardRect.anchorMax = cardPositions[i];
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = Vector2.zero;

            cards[i] = card;
        }
    }

    public void GenerateNewCards()
    {
        for (int i = 0; i < 3; i++)
        {
            currentCardValues[i] = Random.Range(1, 101);
            if (cards[i] != null)
            {
                cards[i].SetValue(currentCardValues[i]);
            }
        }
    }

    public int GetCardValue(int cardIndex)
    {
        if (cardIndex >= 0 && cardIndex < 3)
        {
            return currentCardValues[cardIndex];
        }
        return -1;
    }
}

