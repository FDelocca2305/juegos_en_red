using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class BaseGun : MonoBehaviour
{
    [SerializeField] private float timeBetweenShots = 1.5f;
    [SerializeField] private float actualBullets = 4f;
    [SerializeField] private float maxBullets = 4f;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Sprite icon;
    
    public Sprite Icon => icon;
    
    public float TimeBetweenShots
    {
        get => timeBetweenShots;
    }

    public float ActualBullets
    {
        get => actualBullets;
        set => actualBullets = value;
    }
    
    public float MaxBullets
    {
        get => maxBullets;
        set => maxBullets = value;
    }

    public GameObject MuzzleFlash => muzzleFlash;
}
