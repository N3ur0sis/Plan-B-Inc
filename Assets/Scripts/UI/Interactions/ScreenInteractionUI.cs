using UnityEngine;
using TMPro;

/// <summary>
/// UI d’interaction écran (canvas unique dans la scène).
/// </summary>
public class ScreenInteractionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;

    private void OnDestroy()
    {
        promptText = null;
    }

    /// <summary>
    /// Affiche un texte d’interaction à l’écran.
    /// </summary>
    public void SetPrompt(string text)
    {
        if (this == null || promptText == null) return;

        promptText.text = text;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Cache le prompt d’interaction.
    /// </summary>
    public void ClearPrompt()
    {
        if (this == null || gameObject == null) return;

        gameObject.SetActive(false);
    }
}
