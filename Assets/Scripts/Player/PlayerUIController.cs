using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUIController : MonoBehaviour, IPlayerUIController
{
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text noAmmoText;

    private IPlayerShootController _shoot;
    private IPlayerInventory _inventory;

    private void Awake()
    {
        StartCoroutine(BindServices());
    }
    
    private IEnumerator BindServices()
    {
        yield return ServiceLocatorUtil.WaitFor<IPlayerInventory>(svc => _inventory = svc);
        yield return ServiceLocatorUtil.WaitFor<IPlayerShootController>(svc => _shoot = svc);

        _shoot.OnAmmoChanged += HandleAmmoChanged;
        _shoot.OnAmmoChanged += HandleNoAmmoMessage;

        SetAmmo((int)_inventory.GetSelectedGun.ActualBullets, (int)_inventory.GetSelectedGun.MaxBullets);
    }

    private void OnDestroy()
    {
        if (_shoot != null)
        {
            _shoot.OnAmmoChanged -= HandleAmmoChanged;
            _shoot.OnAmmoChanged -= HandleNoAmmoMessage;
        }
    }

    private void HandleAmmoChanged(int current, int max) => SetAmmo(current, max);
    private void HandleNoAmmoMessage(int current, int max) => noAmmoText?.gameObject.SetActive(current <= 0);
    public void SetAmmo(int current, int max) { if (ammoText) ammoText.text = $"{current}/{max}"; }
}