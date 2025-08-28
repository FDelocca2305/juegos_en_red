using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomButton : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;

    private RoomInfo info;

    public void SetButtonDetails(RoomInfo inputInfo)
    {
        info = inputInfo;
        buttonText.text = inputInfo.Name;
    }

    public void OpenRoom()
    {
        ServiceLocator.Resolve<IPhotonLauncher>().JoinRoom(info);
    }

}
