using UnityEngine;

/// <summary>
/// Interface de base pour tout objet interactif.
/// </summary>
public interface IInteractable
{
    /// <summary>Texte du prompt à afficher (UI/Monde).</summary>
    string GetInteractionPrompt();

    /// <summary>Interaction simple sans interactor.</summary>
    void Interact();

    /// <summary>Interaction avec un interactor (joueur, objet, etc).</summary>
    void InteractWith(GameObject interactor)
    {
        Interact();
    }
}
