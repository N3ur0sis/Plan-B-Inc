using TMPro;
using UnityEngine;

public class NameTag : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;

    public void SetName(string name)
    {
        if (nameText != null)
            nameText.text = name;
        else
            Debug.LogWarning("[NAMETAG] No TMP_Text assigned!");
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            Vector3 lookDir = transform.position - Camera.main.transform.position;
            lookDir.y = 0f;
            transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }
}
