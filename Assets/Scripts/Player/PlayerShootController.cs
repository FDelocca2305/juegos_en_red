using System;
using Photon.Pun;
using UnityEngine;

public class PlayerShootController : MonoBehaviourPunCallbacks, IPlayerShootController
{
    [SerializeField] private GameObject bulletImpact;
    [SerializeField] private float bulletImpactLifetime = 10f;
    [SerializeField] private float muzzleDisplayTime = 0.05f;

    [Header("PlayerParticles")] 
    [SerializeField] private GameObject playerImpact;
    
    private Camera _camera;
    private float _nextShootTime;
    private float _muzzleCounter;
    private IPlayerInventory _playerInventory;

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
        gun.ActualBullets = Mathf.Clamp(quantity, 0, gun.MaxBullets);
        OnAmmoChanged?.Invoke((int)gun.ActualBullets, (int)gun.MaxBullets);
    }

    public void SetMaxBullets(float quantity)
    {
        var gun = _playerInventory.GetSelectedGun;
        gun.MaxBullets = Mathf.Max(0, quantity);
        gun.ActualBullets = Mathf.Clamp(gun.ActualBullets, 0, gun.MaxBullets);
        OnAmmoChanged?.Invoke((int)gun.ActualBullets, (int)gun.MaxBullets);
    }

    private void Awake()
    {
        _camera = Camera.main;
        _playerInventory = GetComponent<IPlayerInventory>() ?? GetComponentInParent<IPlayerInventory>();
        _nextShootTime = 0f;

        var gun = _playerInventory.GetSelectedGun;
        OnAmmoChanged?.Invoke((int)gun.ActualBullets, (int)gun.MaxBullets);
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            var gun = _playerInventory.GetSelectedGun;
            if (gun.MuzzleFlash.activeInHierarchy)
            {
                _muzzleCounter -= Time.deltaTime;
                if (_muzzleCounter <= 0) gun.MuzzleFlash.SetActive(false);
            }
        
            if (_playerInventory.IsWeaponSelected && Input.GetMouseButton(0))
                TryShoot();
        }
    }

    private void TryShoot()
    {
        var gun = _playerInventory.GetSelectedGun;
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
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                PhotonNetwork.Instantiate(playerImpact.name, hit.point, Quaternion.identity);
                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName);
            }
            else
            {
                var bulletImpactObject = Instantiate(
                    bulletImpact,
                    hit.point + hit.normal * 0.002f,
                    Quaternion.LookRotation(hit.normal, Vector3.up)
                );
                Destroy(bulletImpactObject, bulletImpactLifetime);
            }
        }

        var gun = _playerInventory.GetSelectedGun;
        gun.MuzzleFlash.SetActive(true);
        _muzzleCounter = muzzleDisplayTime;
    }

    [PunRPC]
    public void DealDamage(string damager)
    {
        TakeDamage();
    }

    private void TakeDamage()
    {
        gameObject.SetActive(false);
    }
}
