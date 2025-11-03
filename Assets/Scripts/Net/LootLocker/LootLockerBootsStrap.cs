using LootLocker.Requests;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootLockerBootsStrap : MonoBehaviour
{
    public static bool SessionStarted { get; private set; }
    [SerializeField] string playerIdentifier = "1";
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StartGuest();
    }

    private void StartGuest()
    {
        LootLockerSDKManager.StartGuestSession(playerIdentifier, response =>
        {
            if (!response.success)
            {
                Debug.LogError("Fail");
                return;
            }
            SessionStarted = true;
            Debug.Log("Connected");
        } );
    }

}
