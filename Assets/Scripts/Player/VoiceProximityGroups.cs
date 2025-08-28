using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Voice.PUN;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class VoiceProximityGroups : MonoBehaviourPunCallbacks
{
    [SerializeField] private float hearRadius = 20f;
    [SerializeField] private float checkInterval = 0.5f;

    private readonly HashSet<byte> subscribed = new();
    private Coroutine routine;

    private void OnEnable()
    {
        if (photonView.IsMine)
            routine = StartCoroutine(Loop());
    }

    private void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        if (subscribed.Count > 0 && PunVoiceClient.Instance?.Client != null)
        {
            PunVoiceClient.Instance.Client.OpChangeGroups(subscribed.ToArray(), null);
            subscribed.Clear();
        }
    }

    private IEnumerator Loop()
    {
        var client = PunVoiceClient.Instance?.Client;
        if (client == null) yield break;

        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            var toAdd = new List<byte>();
            var toRem = new List<byte>(subscribed);

            foreach (var view in FindObjectsOfType<PhotonView>())
            {
                if (!view || view.IsMine || view.Owner == null) continue;

                byte group = (byte)view.Owner.ActorNumber;
                float d = Vector3.Distance(transform.position, view.transform.position);
                bool close = d <= hearRadius;

                if (close)
                {
                    if (!subscribed.Contains(group))
                    {
                        toAdd.Add(group);
                        subscribed.Add(group);
                    }
                    toRem.Remove(group);
                }
            }

            if (toAdd.Count > 0 || toRem.Count > 0)
            {
                client.OpChangeGroups(toRem.ToArray(), toAdd.ToArray());
                foreach (var g in toRem) subscribed.Remove(g);
            }
        }
    }
}
