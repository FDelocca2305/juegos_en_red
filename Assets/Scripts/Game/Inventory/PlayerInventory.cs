// Scripts/Game/PlayerInventory.cs

using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(PhotonView))]
public class PlayerInventory : MonoBehaviourPunCallbacks 
{
    [Header("Bindings (World/FP Models & Logic)")]
    [SerializeField] private GameObject detectiveGunGO;
    [SerializeField] private GameObject tempGunGO;
    [SerializeField] private GameObject assassinKnifeGO;
    [SerializeField] private GameObject flashlightGO;

    [Header("Temp Gun")]
    [SerializeField] private int tempGunShotsPerGrant = 1;
    private int tempGunShots;

    [Header("Input")]
    [SerializeField] private KeyCode slotPrimaryKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode slotMeleeKey   = KeyCode.Alpha2;
    [SerializeField] private KeyCode slotUtilityKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode toggleFlashKey = KeyCode.F;

    public static PlayerInventory Local;

    private EquipVisual currentVisual = EquipVisual.None;

    private void Awake()
    {
        if (photonView.IsMine) Local = this;
        DeactivateAll();
    }

    private void Start()
    {
        DeactivateAll();
        StartCoroutine(WaitForRoleAndEquip());
        if (TryGetRole(photonView.Owner, out var role))
        {
            Debug.Log(role);
            if (photonView.IsMine)
            {
                switch (role)
                {
                    case PlayerRole.Detective:
                        EquipDetective();
                        break;
                    case PlayerRole.Assassin:
                        EquipAssassin();
                        break;
                    case PlayerRole.Innocent:
                        EquipNone();
                        break;
                }
            }
            else
            {
                DeactivateAll();
            }
        }
    }
    
    private IEnumerator WaitForRoleAndEquip()
    {
        var owner = photonView.Owner;
        
        float timeout = 20f;
        while (timeout > 0f)
        {
            if (owner != null && TryGetRole(owner, out var role))
            {
                Debug.Log($"[Inventory] Role listo: {role} (owner #{owner.ActorNumber})");

                if (photonView.IsMine)
                {
                    switch (role)
                    {
                        case PlayerRole.Detective: EquipDetective(); break;
                        case PlayerRole.Assassin:  EquipAssassin();  break;
                        default:                   EquipNone();      break;
                    }
                }
                else
                {
                    DeactivateAll();
                }
                yield break;
            }

            timeout -= Time.deltaTime;
            yield return null;
        }

        Debug.LogWarning("[Inventory] No llegó ROLE a tiempo, quedo en None (se equipará si cambia luego).");
    }
    
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer != photonView.Owner) return;
        if (changedProps != null && changedProps.ContainsKey(NetKeys.ROLE))
        {
            if (TryGetRole(targetPlayer, out var role))
            {
                Debug.Log($"[Inventory] ROLE actualizado: {role}");
                if (photonView.IsMine)
                {
                    switch (role)
                    {
                        case PlayerRole.Detective: EquipDetective(); break;
                        case PlayerRole.Assassin:  EquipAssassin(); break;
                        default: EquipNone(); break;
                    }
                }
                else
                {
                    DeactivateAll();
                }
            }
        }
    }
    
    private bool TryGetRole(Player p, out PlayerRole role)
    {
        role = PlayerRole.Innocent;

        if (p == null || p.CustomProperties == null) return false;
        if (!p.CustomProperties.TryGetValue(NetKeys.ROLE, out var raw)) return false;
        
        byte b;
        if(raw is byte bb) b = bb;
        else if (raw is int  ii) b = (byte)ii;
        else b = System.Convert.ToByte(raw);

        role = (PlayerRole)b;
        return true;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        
        if (Input.GetKeyDown(slotPrimaryKey))  EquipPrimaryBest();
        if (Input.GetKeyDown(slotMeleeKey))    EquipMelee();
        if (Input.GetKeyDown(slotUtilityKey))  EquipFlashlight();
        
        if (Input.GetKeyDown(toggleFlashKey) && currentVisual == EquipVisual.Flashlight)
        {
            ToggleFlashlightLocal();
        }
    }

    public void GiveTempGun()
    {
        tempGunShots += tempGunShotsPerGrant;
        if (photonView.IsMine && (currentVisual != EquipVisual.DetectiveGun))
            EquipTemp();
    }

    public void ConsumeTempGun()
    {
        tempGunShots = Mathf.Max(0, tempGunShots - 1);
        if (tempGunShots == 0 && currentVisual == EquipVisual.TempGun)
        {
            EquipNone();
        }
    }

    private void EquipDetective()
    {
        SetVisualLocal(EquipVisual.DetectiveGun);
        BroadcastVisual(EquipVisual.DetectiveGun);
    }

    private void EquipAssassin()
    {
        SetVisualLocal(EquipVisual.AssassinKnife);
        BroadcastVisual(EquipVisual.AssassinKnife);
    }

    private void EquipTemp()
    {
        if (tempGunShots <= 0) return;
        SetVisualLocal(EquipVisual.TempGun);
        BroadcastVisual(EquipVisual.TempGun);
    }

    private void EquipMelee()
    {
        //var role = (PlayerRole)(byte)PhotonNetwork.LocalPlayer.CustomProperties[NetKeys.ROLE];
        if (TryGetRole(photonView.Owner, out var role))
        {
            if (role == PlayerRole.Assassin)
            {
                SetVisualLocal(EquipVisual.AssassinKnife);
                BroadcastVisual(EquipVisual.AssassinKnife);
            }
            else
            {
                EquipNone();
            }
        }
    }

    private void EquipFlashlight()
    {
        SetVisualLocal(EquipVisual.Flashlight, turnOnFlashlight:true);
        BroadcastVisual(EquipVisual.Flashlight);
    }

    private void EquipPrimaryBest()
    {
        //var role = (PlayerRole)(byte)PhotonNetwork.LocalPlayer.CustomProperties[NetKeys.ROLE];
        if (TryGetRole(photonView.Owner, out var role))
        {
            if (role == PlayerRole.Detective) { EquipDetective(); return; }
            if (tempGunShots > 0)             { EquipTemp(); return; }
            EquipNone();
        }
    }

    private void EquipNone()
    {
        SetVisualLocal(EquipVisual.None);
        BroadcastVisual(EquipVisual.None);
    }

    private void SetVisualLocal(EquipVisual vis, bool turnOnFlashlight = false)
    {
        currentVisual = vis;
        DeactivateAll();

        switch (vis)
        {
            case EquipVisual.DetectiveGun:
                if (detectiveGunGO) detectiveGunGO.SetActive(true);
                break;
            case EquipVisual.TempGun:
                if (tempGunGO) tempGunGO.SetActive(true);
                break;
            case EquipVisual.AssassinKnife:
                if (assassinKnifeGO) assassinKnifeGO.SetActive(true);
                break;
            case EquipVisual.Flashlight:
                if (flashlightGO)
                {
                    flashlightGO.SetActive(true);
                    var light = flashlightGO.GetComponentInChildren<Light>(true);
                    if (light) light.enabled = turnOnFlashlight;
                }
                break;
            case EquipVisual.None:
            default:
                break;
        }
    }

    private void DeactivateAll()
    {
        if (detectiveGunGO) detectiveGunGO.SetActive(false);
        if (tempGunGO)      tempGunGO.SetActive(false);
        if (assassinKnifeGO)assassinKnifeGO.SetActive(false);
        if (flashlightGO)   flashlightGO.SetActive(false);
    }

    private void ToggleFlashlightLocal()
    {
        if (!flashlightGO) return;
        var light = flashlightGO.GetComponentInChildren<Light>(true);
        if (light) light.enabled = !light.enabled;
    }
    

    private void BroadcastVisual(EquipVisual vis)
    {
        photonView.RPC(nameof(RPC_SetVisual), RpcTarget.Others, (int)vis);
    }

    [PunRPC]
    private void RPC_SetVisual(int vis)
    {
        SetVisualLocal((EquipVisual)vis);
    }
}
