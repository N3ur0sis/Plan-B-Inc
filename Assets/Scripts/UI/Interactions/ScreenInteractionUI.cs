using UnityEngine;
using TMPro;

/// <summary>
/// UI d’interaction écran (canvas unique dans la scène).
/// </summary>
public class InteractionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;

    /// <summary>
    /// Affiche un texte d’interaction à l’écran.
    /// </summary>
    public void SetPrompt(string text)
    {
        if (promptText == null) return;

        promptText.text = text;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Cache le prompt d’interaction.
    /// </summary>
    public void ClearPrompt()
    {
        gameObject.SetActive(false);
    }
}
