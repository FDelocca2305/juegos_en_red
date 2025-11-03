using LootLocker.Requests;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNameHelper:MonoBehaviour
{
    public static void SetPlayerName(string name)
    {
        LootLockerSDKManager.SetPlayerName(name, resp =>
        {
            if (!resp.success) Debug.LogError("Fail Name");
            else Debug.Log("Setted Name");
        });
    }
}
