using System;
using UnityEngine;

[RequireComponent(typeof(Outline3D))]
public class FuseBoxOutlineController : MonoBehaviour
{
    [SerializeField] private Outline3D outline;

    void Reset() { outline = GetComponent<Outline3D>(); }

    void Start()
    {
        if (!outline) outline = GetComponent<Outline3D>();
        TrySubscribe();
        Refresh();
    }

    private void OnEnable()
    {
        if (!outline) outline = GetComponent<Outline3D>();
        TrySubscribe();
        Refresh();
    }

    void OnDisable()
    {
        if (FuseController.Exists)
        {
            FuseController.I.OnPowerChanged -= OnPowerChanged;
            FuseController.I.OnLocalFuseHolderChanged -= OnLocalHolderChanged;
        }
    }

    void TrySubscribe()
    {
        if (!FuseController.Exists) return;
        FuseController.I.OnPowerChanged += OnPowerChanged;
        FuseController.I.OnLocalFuseHolderChanged += OnLocalHolderChanged;
    }

    void OnPowerChanged(FuseController.PowerState _) => Refresh();
    void OnLocalHolderChanged(bool _) => Refresh();

    void Refresh()
    {
        bool show = FuseController.Exists
                    && FuseController.I.Power == FuseController.PowerState.Blackout
                    && FuseController.I.IsLocalHolder();

        if (outline) outline.SetEnabled(show);
    }
}