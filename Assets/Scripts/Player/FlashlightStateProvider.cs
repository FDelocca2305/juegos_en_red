using Photon.Pun;
using UnityEngine;

public interface ILocalFlashlightState { bool IsOn { get; } }

public class FlashlightStateProvider : MonoBehaviourPunCallbacks, ILocalFlashlightState
{
    [SerializeField] private Animator animator;
    [SerializeField] private string paramName = "flashlight";
    public bool IsOn => animator && animator.GetBool(paramName);

    public void Start()
    {
        if (photonView.IsMine) ServiceLocator.Register<ILocalFlashlightState>(this);
    }
    
    public void OnDestroy()
    {
        if (photonView.IsMine) ServiceLocator.Deregister<ILocalFlashlightState>(this);
    }
}