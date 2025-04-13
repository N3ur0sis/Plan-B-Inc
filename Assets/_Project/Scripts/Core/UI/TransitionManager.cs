using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;
    public GameObject loadingUI;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowLoading() => loadingUI.SetActive(true);
    public void HideLoading() => loadingUI.SetActive(false);
}
