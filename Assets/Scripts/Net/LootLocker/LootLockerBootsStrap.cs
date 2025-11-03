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
        LootLockerSDKManager.StartGuestSession(UnityEngine.Random.Range(1f, 999f).ToString(), response =>
        {
            if (!response.success)
            {
                Debug.LogError("Fail");
                Debug.LogError(response.errorData.message);
                Debug.LogError(response.errorData.code);
                return;
            }
            SessionStarted = true;
            Debug.Log("Connected");
        } );
    }

}
