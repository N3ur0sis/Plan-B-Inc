using UnityEngine;

/// <summary>
/// Représente une action personnalisée déclenchée par une interaction.
/// </summary>
public interface IInteractionAction
{
    /// <summary>
    /// Exécute l’action avec l’interacteur donné.
    /// </summary>
    /// <param name="interactor">L’objet (souvent le joueur) qui interagit.</param>
    void Execute(GameObject interactor);
}
