using Photon.Pun;
using UI.Gameplay;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PlayerInteractController : MonoBehaviourPunCallbacks
{
    [SerializeField] private float useDistance = 2f;
    [SerializeField] private LayerMask interactMask;
    private Camera _cam;
    private IGameplayUI _ui;
    private IInteractable _current;
    private string _lastPrompt;

    private void Start()
    {
        if (!photonView.IsMine) { enabled = false; return; }
        _cam = Camera.main;
        ServiceLocator.TryResolve(out _ui);
    }

    private void Update()
    {
        if (_cam == null) _cam = Camera.main;

        _current = null;
        var ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(ray, out var hit, useDistance, interactMask))
            _current = hit.collider.GetComponentInParent<IInteractable>();

        var prompt = _current != null && _current.CanInteract() ? $"E - {_current.GetPrompt()}" : "";
        
        if (prompt != _lastPrompt)
        {
            _lastPrompt = prompt;
            if (string.IsNullOrEmpty(prompt)) _ui?.ShowHint("", 0.05f);
            else _ui?.ShowHint(prompt, 1f);
        }

        if (_current != null && _current.CanInteract() && Input.GetKeyDown(KeyCode.E))
            _current.Interact();
    }
}