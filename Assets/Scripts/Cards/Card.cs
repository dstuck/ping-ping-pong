using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Card : MonoBehaviour
{
    [Header("Card Components")]
    [SerializeField] private TextMeshProUGUI cardText;
    [SerializeField] private Image cardBackground;

    public TextMeshProUGUI CardText => cardText;
    public Image CardBackground => cardBackground;

    public void SetValue(int value)
    {
        if (cardText != null)
        {
            cardText.text = value.ToString();
        }

        if (cardBackground != null)
        {
            float normalizedValue = value / 100f;
            Color cardColor = Color.Lerp(Color.red, Color.green, normalizedValue);
            cardBackground.color = cardColor;
        }
    }
}

