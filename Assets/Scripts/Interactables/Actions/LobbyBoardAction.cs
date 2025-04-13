using UnityEngine;

public class LobbyBoardAction : MonoBehaviour, IInteractionAction
{
    [SerializeField] private LobbyBoardInteractable board;

    public void Execute(GameObject interactor)
    {
        if (board != null)
        {
            board.EnterBoardView();
        }
        else
        {
            Debug.LogWarning("[LOBBY BOARD ACTION] Missing LobbyBoardInteractable reference.");
        }
    }
}
