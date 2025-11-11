using LootLocker.Requests;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootLockerBootsStrap : MonoBehaviour
{
    public static bool SessionStarted { get; private set; }
    [SerializeField] string playerIdentifier = "1";

    public string Identifier { get; set; }
    
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StartGuest();
    }

    private void StartGuest()
    {
        Identifier = UnityEngine.Random.Range(1f, 999f).ToString();
        LootLockerSDKManager.StartGuestSession(Identifier, response =>
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
