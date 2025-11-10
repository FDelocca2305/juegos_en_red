using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

public class PlayerShootController : MonoBehaviourPunCallbacks, IPlayerShootController
{
    [SerializeField] private GameObject bulletImpact;
    [SerializeField] private float bulletImpactLifetime = 10f;
    [SerializeField] private float muzzleDisplayTime = 0.05f;
    [SerializeField] private float detectiveBlindSeconds = 10f;
    
    [Header("PlayerParticles")] 
    [SerializeField] private GameObject playerImpact;
    
    private Camera _camera;
    private float _nextShootTime;
    private float _muzzleCounter;
    private IPlayerInventory _playerInventory;
    private ILocalRoleProvider _roles;
    private AudioManager _audioManager;
    private ImpactAudioController _impactAudioController;
    
    public event Action<int, int> OnAmmoChanged;
    
    public override void OnEnable()
    {
        if (photonView.IsMine)
            ServiceLocator.Register<IPlayerShootController>(this);
    }

    public override void OnDisable()
    {
        if (photonView.IsMine)
            ServiceLocator.Deregister<IPlayerShootController>(this);
    }

    public void SetActualBullets(float quantity)
    {
        var gun = _playerInventory.GetSelectedGun;
        if (gun == null) return;
        gun.ActualBullets = Mathf.Clamp(quantity, 0, gun.MaxBullets);
        OnAmmoChanged?.Invoke((int)gun.ActualBullets, (int)gun.MaxBullets);
    }

    public void SetMaxBullets(float quantity)
    {
        var gun = _playerInventory.GetSelectedGun;
        if (gun == null) return;
        gun.MaxBullets = Mathf.Max(0, quantity);
        gun.ActualBullets = Mathf.Clamp(gun.ActualBullets, 0, gun.MaxBullets);
        OnAmmoChanged?.Invoke((int)gun.ActualBullets, (int)gun.MaxBullets);
    }

    private void Awake()
    {
        _camera = Camera.main;
        _playerInventory = GetComponent<IPlayerInventory>() ?? GetComponentInParent<IPlayerInventory>();
        ServiceLocator.TryResolve(out _roles);
        _audioManager = AudioManager.Instance;
        _impactAudioController = GetComponent<ImpactAudioController>() ?? GetComponentInParent<ImpactAudioController>();
        _nextShootTime = 0f;

        var gun = _playerInventory.GetSelectedGun;
        if (gun != null) OnAmmoChanged?.Invoke((int)gun.ActualBullets, (int)gun.MaxBullets);
        else OnAmmoChanged?.Invoke(0, 0);
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        var gun = _playerInventory.GetSelectedGun;

        if (gun != null && gun.MuzzleFlash.activeInHierarchy)
        {
            _muzzleCounter -= Time.deltaTime;
            if (_muzzleCounter <= 0) gun.MuzzleFlash.SetActive(false);
        }

        if (_playerInventory.IsWeaponSelected && gun != null && Input.GetMouseButton(0))
            TryShoot();
    }

    private void TryShoot()
    {
        var gun = _playerInventory.GetSelectedGun;
        if (gun == null) return;
        if (gun.ActualBullets <= 0) return;
        if (Time.time < _nextShootTime) return;

        Shoot();
        _nextShootTime = Time.time + gun.TimeBetweenShots;
        SetActualBullets(gun.ActualBullets - 1);
    }

    private void Shoot()
    {
        var cam = _camera != null ? _camera : Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        ray.origin = cam.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var targetView   = hit.collider.GetComponentInParent<PhotonView>();
            var roleProvider = hit.collider.GetComponentInParent<IRoleProvider>();
            var targetRole   = roleProvider != null ? roleProvider.Role : RoleId.Innocent;
            var myRole       = _roles?.LocalRole ?? RoleId.Innocent;
            
            bool isOtherPlayer = targetView != null && targetView != photonView && targetView.gameObject.CompareTag("Player");
            
            bool doDamage = isOtherPlayer;
            bool badShot  = doDamage && myRole == RoleId.Detective && targetRole == RoleId.Innocent;

            if (doDamage)
            {
                PhotonNetwork.Instantiate(playerImpact.name, hit.point, Quaternion.identity);
                targetView.RPC(nameof(DealDamage), RpcTarget.All, photonView.Owner.NickName);
                
                if (_impactAudioController != null)
                {
                    _impactAudioController.PlayPlayerImpact(hit.point);
                }
            }
            else
            {
                var bulletImpactObject = Instantiate(
                    bulletImpact,
                    hit.point + hit.normal * 0.002f,
                    Quaternion.LookRotation(hit.normal, Vector3.up)
                );
                Destroy(bulletImpactObject, bulletImpactLifetime);
                
                if (_impactAudioController != null)
                {
                    _impactAudioController.PlayWallImpact(hit.point);
                }
            }

            if (badShot)
                photonView.RPC(nameof(DetectivePunish), RpcTarget.All);
        }

        var gun = _playerInventory.GetSelectedGun;
        gun.MuzzleFlash.SetActive(true);
        _muzzleCounter = muzzleDisplayTime;
        
        PlayShootSound(gun);
    }


    [PunRPC]
    public void DealDamage(string damager)
    {
        TakeDamage(damager);
    }
    
    [PunRPC]
    private void DetectivePunish()
    {
        if (!photonView.IsMine) return;

        var ui = ServiceLocator.Resolve<UI.Gameplay.IGameplayUI>();
        ui?.BlindFor(detectiveBlindSeconds, "You Killed an innocent!\nBlindness for 10s");
        
        StartCoroutine(BlindRoutineAmmo());
    }

    private IEnumerator BlindRoutineAmmo()
    {
        var gun = _playerInventory.GetSelectedGun;
        int prevMax = gun ? (int)gun.MaxBullets   : 0;
        int prevCur = gun ? (int)gun.ActualBullets: 0;

        SetMaxBullets(0);
        SetActualBullets(0);

        yield return new WaitForSeconds(detectiveBlindSeconds);

        if (gun != null)
        {
            SetMaxBullets(prevMax);
            SetActualBullets(prevCur);
        }
    }

    private void TakeDamage(string damager)
    {
        if (photonView.IsMine)
        {
            ServiceLocator.Resolve<IPlayerSpawner>().Die(damager);
        }
    }

    private void PlayShootSound(BaseGun gun)
    {
        if (_audioManager == null || gun == null) return;
        
        string gunType = gun.name.ToLower();
        string soundName = "shot_pistol";

        if (gunType.Contains("rifle"))
        {
            soundName = "shot_rifle";
        }
        else if (gunType.Contains("machinegun") || gunType.Contains("machine"))
        {
            soundName = "shot_machinegun";
        }
        else if (gunType.Contains("pistol"))
        {
            soundName = "shot_pistol";
        }
        
        _audioManager.PlayNetworkSoundAtPosition(soundName, transform.position);
    }
}
