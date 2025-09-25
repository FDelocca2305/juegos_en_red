using System.Collections;
using TMPro;
using UI.Gameplay;
using UnityEngine;

public class GameplayUIController : MonoBehaviour, IGameplayUI
{
    [Header("Refs")]
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private GameObject objectivesPanel;
    [SerializeField] private TMP_Text objectivesText;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private TMP_Text deathText;
    [SerializeField] private GameObject blindScreen;
    [SerializeField] private TMP_Text blindText;
    
    private Coroutine _hintRoutine;
    private Coroutine _blindRoutine;
    
    public string DeathText
    {
        set => deathText.text = value;
    }

    public bool DeathScreenActivate
    {
        set => deathScreen.gameObject.SetActive(value);
    }

    public void ShowHint(string text, float seconds = 1.25f)
    {
        if (!hintText) return;
        if (_hintRoutine != null) StopCoroutine(_hintRoutine);
        hintText.gameObject.SetActive(true);
        hintText.text = text;
        _hintRoutine = StartCoroutine(HideAfter(seconds));
    }

    private IEnumerator HideAfter(float s)
    {
        yield return new WaitForSeconds(s);
        if (hintText) hintText.gameObject.SetActive(false);
        _hintRoutine = null;
    }

    public void ShowObjectives(string[] lines, bool visible)
    {
        if (!objectivesPanel) return;
        if (visible && objectivesText)
            objectivesText.text = lines != null ? string.Join("\n• ", lines).Insert(0, "• ") : "";
        objectivesPanel.SetActive(visible);
    }
    
    public void ShowRadarTargets(Transform[] targets)
    {
        ShowHint($"Radar: {targets?.Length ?? 0}", 0.5f);
    }
    
    public void ShowBlind(bool visible, string message = null)
    {
        if (_blindRoutine != null && !visible) { StopCoroutine(_blindRoutine); _blindRoutine = null; }
        if (blindScreen) blindScreen.SetActive(visible);
        if (blindText && !string.IsNullOrEmpty(message)) blindText.text = message;
    }

    public void BlindFor(float seconds, string message = null)
    {
        if (_blindRoutine != null) StopCoroutine(_blindRoutine);
        _blindRoutine = StartCoroutine(BlindRoutine(seconds, message));
    }

    private IEnumerator BlindRoutine(float s, string message)
    {
        ShowBlind(true, message);
        yield return new WaitForSeconds(s);
        ShowBlind(false);
        _blindRoutine = null;
    }
}