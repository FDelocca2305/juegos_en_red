using System;
using UnityEngine;

public class PlayerShootController : MonoBehaviour, IPlayerShootController
{
    [SerializeField] private GameObject bulletImpact;
    [SerializeField] private float bulletImpactLifetime = 10f;
    [SerializeField] private float muzzleDisplayTime = 0.05f;

    private Camera _camera;
    private float _nextShootTime;
    private float _muzzleCounter;
    private IPlayerInventory _playerInventory;

    public event Action<int, int> OnAmmoChanged;

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
        _playerInventory = ServiceLocator.Resolve<IPlayerInventory>();
        _nextShootTime = 0f;

        var gun = _playerInventory.GetSelectedGun;
        OnAmmoChanged?.Invoke((int)gun.ActualBullets, (int)gun.MaxBullets);
    }

    private void Update()
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

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var bulletImpactObject = Instantiate(
                bulletImpact,
                hit.point + hit.normal * 0.002f,
                Quaternion.LookRotation(hit.normal, Vector3.up)
            );
            Destroy(bulletImpactObject, bulletImpactLifetime);
        }

        var gun = _playerInventory.GetSelectedGun;
        gun.MuzzleFlash.SetActive(true);
        _muzzleCounter = muzzleDisplayTime;
    }
}
