using System.Collections;
using Photon.Pun;
using UI.Gameplay;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PlayerInteractController : MonoBehaviourPunCallbacks
{
    [SerializeField] private float useDistance = 2f;
    [SerializeField] private LayerMask interactMask;
    [SerializeField] private KeyCode useKey = KeyCode.E;
    [SerializeField] private KeyCode sabotageKey = KeyCode.R;
    [SerializeField] private AssassinSabotageAbility sabotageAbility;
    
    private Camera _cam;
    private IGameplayUI _ui;
    private IInteractable _current;
    private ISabotageable _currentSabotage;
    private string _lastPrompt;
    private bool _sabotageMode;
    private Coroutine _sabotageWindow;
    
    void Start()
    {
        if (!photonView.IsMine) { enabled = false; return; }
        _cam = Camera.main;
        ServiceLocator.TryResolve(out _ui);
        if (!sabotageAbility) sabotageAbility = GetComponent<AssassinSabotageAbility>();
    }

    void Update()
    {
        if (_cam == null) _cam = Camera.main;

        _current = null; _currentSabotage = null;
        var ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(ray, out var hit, useDistance, interactMask))
        {
            _current = hit.collider.GetComponentInParent<IInteractable>();
            _currentSabotage = hit.collider.GetComponentInParent<ISabotageable>();
        }
        
        string prompt = "";
        if (_current != null && _current.CanInteract())
            prompt = $"{useKey} - {_current.GetPrompt()}";

        if (_currentSabotage != null && _currentSabotage.CanSabotage() &&
            sabotageAbility && sabotageAbility.IsAssassinLocal)
        {
            if (sabotageAbility.CanUse())
                prompt = string.IsNullOrEmpty(prompt)
                    ? $"{sabotageKey} - {_currentSabotage.GetSabotagePrompt()}"
                    : $"{prompt}   |   {sabotageKey} - {_currentSabotage.GetSabotagePrompt()}";
            else
                prompt = string.IsNullOrEmpty(prompt)
                    ? $"Sabotage cd ({sabotageAbility.RemainingCooldown:0.0}s)"
                    : $"{prompt}   |   Sabotage cd ({sabotageAbility.RemainingCooldown:0.0}s)";
        }

        if (prompt != _lastPrompt) { _lastPrompt = prompt; _ui?.ShowHint(prompt, 1f); }

        if (_current != null && _current.CanInteract() && Input.GetKeyDown(useKey))
            _current.Interact(photonView.OwnerActorNr);
        
        if (_currentSabotage != null && Input.GetKeyDown(sabotageKey) &&
            sabotageAbility && sabotageAbility.IsAssassinLocal)
        {
            if (!sabotageAbility.CanUse())
            {
                _ui?.ShowHint($"Sabotage on cooldown ({sabotageAbility.RemainingCooldown:0.0}s)", 0.2f);
                return;
            }

            if (_currentSabotage.CanSabotage())
            {
                _currentSabotage.Sabotage();
                sabotageAbility.Consume();
                _ui?.ShowHint("Switch sabotaged", 1f);
            }
        }
    }
    
    public void SetSabotageMode(bool enabled)
    {
        _sabotageMode = enabled;
        if (!enabled) ClearPrompt();
    }

    public void EnterSabotageMode(float seconds)
    {
        if (_sabotageWindow != null) StopCoroutine(_sabotageWindow);
        _sabotageWindow = StartCoroutine(SabotageWindow(seconds));
    }

    private IEnumerator SabotageWindow(float seconds)
    {
        _sabotageMode = true;
        yield return new WaitForSeconds(seconds);
        _sabotageMode = false;
        ClearPrompt();
    }

    private void ClearPrompt()
    {
        _lastPrompt = "";
        _ui?.ShowHint("", 0.2f);
    }
}