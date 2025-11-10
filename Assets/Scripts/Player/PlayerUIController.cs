using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour, IPlayerUIController
{
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text noAmmoText;
    [SerializeField] private Image ammoIcon;

    private IPlayerShootController _shoot;
    private IPlayerInventory _inventory;

    private void Awake()
    {
        StartCoroutine(BindServices());
    }
    
    private void BindInitialAmmo()
    {
        var gun = _inventory.GetSelectedGun;
        if (gun != null) SetAmmo((int)gun.ActualBullets, (int)gun.MaxBullets);
        else SetAmmo(0, 0);
    }

    private IEnumerator BindServices()
    {
        yield return ServiceLocatorUtil.WaitFor<IPlayerInventory>(svc => _inventory = svc);
        yield return ServiceLocatorUtil.WaitFor<IPlayerShootController>(svc => _shoot = svc);

        _shoot.OnAmmoChanged += HandleAmmoChanged;
        _shoot.OnAmmoChanged += HandleNoAmmoMessage;

        BindInitialAmmo();
    }

    public void SetAmmo(int current, int max)
    {
        if (!ammoText) return;

        bool hasAmmoInfo = max > 0;

        if (!hasAmmoInfo)
        {
            ammoText.text = "";
            if (noAmmoText) noAmmoText.gameObject.SetActive(false);
        }
        else
        {
            ammoText.text = $"{current}/{max}";
        }

        if (ammoIcon)
            ammoIcon.gameObject.SetActive(hasAmmoInfo);
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
    private void HandleNoAmmoMessage(int current, int max)
    {
        if (!noAmmoText) return;
        bool showNoAmmo = max > 0 && current <= 0;
        noAmmoText.gameObject.SetActive(showNoAmmo);
    }
}