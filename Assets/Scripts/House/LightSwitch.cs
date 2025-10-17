using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class LightSwitch : MonoBehaviourPun, IInteractable, ISabotageable
{
    [Header("Group")]
    [SerializeField] private RoomLightGroup group;
    
    [Header("Visual Handle")]
    [SerializeField] private Transform handle;
    [SerializeField] private Vector3 onLocalEuler  = new Vector3(-20, 0, 0);
    [SerializeField] private Vector3 offLocalEuler = new Vector3( 20, 0, 0);
    [SerializeField] private float animTime = 0.08f;
    [SerializeField] private AudioSource clickSfx;
    
    [Header("Sabotage")]
    [SerializeField] private bool sabotagedArmed;
    [SerializeField] private Renderer indicator;
    [SerializeField] private Color sabotagedColor = Color.red;
    
    private PhotonView pv;
    
    void Awake()
    {
        pv = GetComponent<PhotonView>();
        if (!pv)
        {
            pv = GetComponentInParent<PhotonView>();
        }
        if (!pv)
        {
            Debug.LogError($"[LightSwitch] PhotonView.");
        }
    }

    void OnEnable()
    {
        if (group) group.StateChanged += OnGroupStateChanged;
        SnapTo(group && group.IsOn);
    }
    
    public bool CanInteract() => true;
    public string GetPrompt() => "Toggle light";

    public void Interact(int? ownerActorNumber)
    {
        if (FuseController.Exists && FuseController.I.Power == FuseController.PowerState.Blackout) return;

        if (sabotagedArmed && group && !group.IsOn)
        {
            sabotagedArmed = false;
            UpdateIndicator();

            FuseController.I.TriggerBlackout();
            return;
        }
        
        if (group) group.Toggle();
    }
    
    public bool CanSabotage()
    {
        if (FuseController.Exists && FuseController.I.Power == FuseController.PowerState.Blackout) return false;
        return !sabotagedArmed;
    }
    
    public string GetSabotagePrompt() => sabotagedArmed ? "Sabotaged" : "Sabotage Switch";

    public void Sabotage()
    {
        if (!CanSabotage()) return;
        if (!pv) pv = GetComponent<PhotonView>();

        if (PhotonNetwork.IsMasterClient)
            pv.RPC(nameof(RpcSetSabotaged), RpcTarget.AllBuffered, true);
        else
            pv.RPC(nameof(RpcRequestSabotage), RpcTarget.MasterClient);
    }

    void OnDisable()
    {
        if (group) group.StateChanged -= OnGroupStateChanged;
    }

    private void OnGroupStateChanged(bool isOn)
    {
        if (clickSfx) clickSfx.Play();
        if (!handle) return;
        StopAllCoroutines();
        StartCoroutine(AnimateTo(isOn ? onLocalEuler : offLocalEuler));
    }

    private void SnapTo(bool isOn)
    {
        if (handle) handle.localEulerAngles = isOn ? onLocalEuler : offLocalEuler;
    }

    private IEnumerator AnimateTo(Vector3 targetEuler)
    {
        if (!handle || animTime <= 0f) { SnapTo(group.IsOn); yield break; }

        Quaternion start = handle.localRotation;
        Quaternion end   = Quaternion.Euler(targetEuler);
        float t = 0f;

        while (t < animTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / animTime);
            handle.localRotation = Quaternion.Slerp(start, end, a);
            yield return null;
        }
        handle.localRotation = end;
    }
    
    private void UpdateIndicator()
    {
        if (!indicator) return;
        var mat = indicator.material;
        if (sabotagedArmed)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", sabotagedColor);
        }
        else
        {
            mat.SetColor("_EmissionColor", Color.black);
            mat.DisableKeyword("_EMISSION");
        }
    }
    
    [PunRPC] void RpcRequestSabotage()
    {
        if (PhotonNetwork.IsMasterClient)
            pv.RPC(nameof(RpcSetSabotaged), RpcTarget.AllBuffered, true);
    }

    [PunRPC] void RpcSetSabotaged(bool value)
    {
        sabotagedArmed = value;
        UpdateIndicator();
    }
}
