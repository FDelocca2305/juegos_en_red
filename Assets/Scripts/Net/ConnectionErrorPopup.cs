// Assets/_Project/Net/Photon/ConnectionErrorPopup.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionErrorPopup : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text bodyText;
    [SerializeField] Button okButton;

    public void Show(string title, string body, System.Action onOk)
    {
        Cursor.lockState = CursorLockMode.None;
        gameObject.SetActive(true);
        titleText.text = title;
        bodyText.text = body;
        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(() => onOk?.Invoke());
    }

    public void Hide() => gameObject.SetActive(false);
}
