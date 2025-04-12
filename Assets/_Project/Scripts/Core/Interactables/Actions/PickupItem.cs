using UnityEngine;

/// <summary>
/// Action d’interaction : ramasse l’objet et le désactive.
/// </summary>
public class PickupItem : MonoBehaviour, IInteractionAction
{
    public void Execute(GameObject interactor)
    {
        gameObject.SetActive(false);
    }
}
