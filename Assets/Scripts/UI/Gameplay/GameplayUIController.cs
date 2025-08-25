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

    private Coroutine _hintRoutine;

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
        objectivesPanel.SetActive(visible);
        if (visible && objectivesText)
            objectivesText.text = lines != null ? string.Join("\n• ", lines).Insert(0, "• ") : "";
    }

    // Implementación simple: solo muestra cantidad
    public void ShowRadarTargets(Transform[] targets)
    {
        ShowHint($"Radar: {targets?.Length ?? 0}", 0.5f);
        // Si querés blips reales, agregá aquí WorldToScreenPoint + iconos en un canvas.
    }
}