using Photon.Pun;
using UnityEngine;

namespace DoorScript
{
    [RequireComponent(typeof(PhotonView)), RequireComponent(typeof(AudioSource))]
    public class Door : MonoBehaviourPun, IPunObservable
    {
        [Header("Animación")]
        [SerializeField] float duration = 0.45f;
        [SerializeField] float openAngle = -90f;
        [SerializeField] float closeAngle = 0f;

        [Header("Audio")]
        [SerializeField] AudioSource asource;
        [SerializeField] AudioClip openDoor;
        [SerializeField] AudioClip closeDoor;
        
        bool         isOpen;
        double       animStart;
        int          seq;

        Quaternion   startRot;
        Quaternion   targetRot;
        Quaternion   qOpen, qClose;
        
        int          lastAppliedSeq = -1;

        void Awake()
        {
            if (!asource) asource = GetComponent<AudioSource>();
            qOpen  = Quaternion.Euler(0f, openAngle, 0f);
            qClose = Quaternion.Euler(0f, closeAngle, 0f);
            
            isOpen    = false;
            startRot  = transform.localRotation;
            targetRot = qClose;
            animStart = PhotonNetwork.Time;
        }

        void Start()
        {
            transform.localRotation = isOpen ? qOpen : qClose;
        }

        void Update()
        {
            float t = Mathf.Clamp01((float)((PhotonNetwork.Time - animStart) / duration));
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
        }
        
        public void Toggle() => TryToggle();

        public void TryToggle()
        {
            if ((PhotonNetwork.Time - animStart) < duration * 0.25f) return;

            if (photonView.IsMine)
            {
                ApplyOpenState(!isOpen, PhotonNetwork.Time, true);
            }
            else
            {
                photonView.RPC(nameof(RPC_RequestToggle), RpcTarget.MasterClient, PhotonNetwork.Time);
            }
        }

        [PunRPC]
        void RPC_RequestToggle(double clientTime, PhotonMessageInfo info)
        {
            if (!photonView.IsMine) return;
            ApplyOpenState(!isOpen, PhotonNetwork.Time, true);
        }

        void ApplyOpenState(bool open, double startTime, bool fromLocalOwner)
        {
            isOpen     = open;
            animStart  = startTime;
            startRot   = transform.localRotation;
            targetRot  = open ? qOpen : qClose;
            seq++;
            
            if (fromLocalOwner) PlaySfx(open);
        }

        void PlaySfx(bool open)
        {
            if (!asource) return;
            var clip = open ? openDoor : closeDoor;
            if (clip)
            {
                asource.clip = clip;
                asource.Play();
            }
        }
        
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(isOpen);
                stream.SendNext(animStart);
                stream.SendNext(seq);
                stream.SendNext(startRot);
            }
            else
            {
                bool   in_isOpen   = (bool)stream.ReceiveNext();
                double in_animStart= (double)stream.ReceiveNext();
                int    in_seq      = (int)stream.ReceiveNext();
                Quaternion in_start= (Quaternion)stream.ReceiveNext();
                
                if (in_seq > lastAppliedSeq)
                {
                    lastAppliedSeq = in_seq;

                    isOpen    = in_isOpen;
                    animStart = in_animStart;
                    startRot  = in_start;
                    targetRot = isOpen ? qOpen : qClose;
                    
                    PlaySfx(isOpen);
                }
            }
        }
    }
}
