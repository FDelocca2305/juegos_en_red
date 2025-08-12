// Scripts/Game/PlayerInventory.cs
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PlayerInventory : MonoBehaviourPun
{
    [Header("Bindings (World/FP Models & Logic)")]
    [SerializeField] private GameObject detectiveGunGO; // tiene DetectiveGun
    [SerializeField] private GameObject tempGunGO;      // tiene TempGun
    [SerializeField] private GameObject assassinKnifeGO;// tiene AssassinMelee
    [SerializeField] private GameObject flashlightGO;   // tiene Light o PlayerFlashlight

    [Header("Temp Gun")]
    [SerializeField] private int tempGunShotsPerGrant = 1; // un tiro por N lupas
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
        var role = (PlayerRole)(byte)PhotonNetwork.LocalPlayer.CustomProperties[NetKeys.ROLE];

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

    // ---------- API pública para otros sistemas ----------

    public void GiveTempGun()
    {
        tempGunShots += tempGunShotsPerGrant;
        // Si no hay un primary válido (detective o temp) equipá temp
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

    // ---------- Equip helpers (solo local invoca, luego RPC para visuales) ----------

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
        var role = (PlayerRole)(byte)PhotonNetwork.LocalPlayer.CustomProperties[NetKeys.ROLE];
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

    private void EquipFlashlight()
    {
        SetVisualLocal(EquipVisual.Flashlight, turnOnFlashlight:true);
        BroadcastVisual(EquipVisual.Flashlight);
    }

    private void EquipPrimaryBest()
    {
        var role = (PlayerRole)(byte)PhotonNetwork.LocalPlayer.CustomProperties[NetKeys.ROLE];
        if (role == PlayerRole.Detective) { EquipDetective(); return; }
        if (tempGunShots > 0)             { EquipTemp(); return; }
        EquipNone();
    }

    private void EquipNone()
    {
        SetVisualLocal(EquipVisual.None);
        BroadcastVisual(EquipVisual.None);
    }

    // ---------- Visual toggling ----------

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
                    // Si la linterna tiene script PlayerFlashlight, el toggle real lo lleva ese script.
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
