using Photon.Pun;
using UnityEngine;

public class WeaponFX : MonoBehaviourPun
{
    [Header("FX")]
    public ParticleSystem muzzleFlashFP;
    public ParticleSystem muzzleFlashWorld;
    public AudioSource audioSrc;
    public AudioClip shootClip;
    public AudioClip emptyClip;

    public void PlayShootFX(bool localView)
    {
        if (localView && muzzleFlashFP) muzzleFlashFP.Play(true);
        if (muzzleFlashWorld) muzzleFlashWorld.Play(true);
        if (audioSrc && shootClip) audioSrc.PlayOneShot(shootClip);
        photonView.RPC(nameof(RPC_PlayRemote), RpcTarget.Others);
    }

    public void PlayEmpty()
    {
        if (audioSrc && emptyClip) audioSrc.PlayOneShot(emptyClip);
    }

    [PunRPC] void RPC_PlayRemote()
    {
        if (muzzleFlashWorld) muzzleFlashWorld.Play(true);
        if (audioSrc && shootClip) audioSrc.PlayOneShot(shootClip);
    }
}