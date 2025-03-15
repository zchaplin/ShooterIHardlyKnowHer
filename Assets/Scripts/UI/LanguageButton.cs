using UnityEngine;
using UnityEngine.UI;

public class LanguageButton : MonoBehaviour
{
    [SerializeField] private int localeID;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnLanguageButtonClicked);
    }

    private void OnLanguageButtonClicked()
    {
        LocaleSwitcher.instance.SetLocale(localeID);
    }
}