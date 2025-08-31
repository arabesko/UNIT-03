using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] public GameObject _panelUI;
    [SerializeField] public TextMeshProUGUI _interactionText;

    private void Start()
    {
        _panelUI.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        _interactionText.text = message;
        _panelUI.SetActive(true);
    }

    public void HideMessage()
    {
        _panelUI.SetActive(false);
    }
}
