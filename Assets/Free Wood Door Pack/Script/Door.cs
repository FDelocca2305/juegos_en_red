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

        // Estado net
        bool         isOpen;
        double       animStart;      // PhotonNetwork.Time del inicio de anim
        int          seq;            // versión local que incrementa el owner

        // Para reconstruir la anim localmente en todos
        Quaternion   startRot;
        Quaternion   targetRot;
        Quaternion   qOpen, qClose;

        // Para evitar doble SFX
        int          lastAppliedSeq = -1;

        void Awake()
        {
            if (!asource) asource = GetComponent<AudioSource>();
            qOpen  = Quaternion.Euler(0f, openAngle, 0f);
            qClose = Quaternion.Euler(0f, closeAngle, 0f);

            // Estado inicial (puerta cerrada por defecto)
            isOpen    = false;
            startRot  = transform.localRotation;
            targetRot = qClose;
            animStart = PhotonNetwork.Time;
        }

        void Start()
        {
            // IMPORTANTE (Editor):
            // - Marcar el PhotonView como *Scene Object* (para que lo posea el MasterClient).
            // - En Observed Components agregar este Door.
            // - Synchronization: Unreliable On Change.
            transform.localRotation = isOpen ? qOpen : qClose;
        }

        void Update()
        {
            // Progreso normalizado según reloj de red
            float t = Mathf.Clamp01((float)((PhotonNetwork.Time - animStart) / duration));
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
        }

        // Lado local: pedir toggle
        public void Toggle() => TryToggle();

        public void TryToggle()
        {
            // Anti-spam: no permitir spam durante anim
            if ((PhotonNetwork.Time - animStart) < duration * 0.25f) return;

            if (photonView.IsMine)
            {
                // Owner aplica y SerializeView replica
                ApplyOpenState(!isOpen, PhotonNetwork.Time, true);
            }
            else
            {
                // Pedir al owner (MasterClient si es Scene Object)
                photonView.RPC(nameof(RPC_RequestToggle), RpcTarget.MasterClient, PhotonNetwork.Time);
            }
        }

        [PunRPC]
        void RPC_RequestToggle(double clientTime, PhotonMessageInfo info)
        {
            if (!photonView.IsMine) return;                 // solo owner procesa
            // (podrías validar distancia, permisos, cooldown, etc.)
            ApplyOpenState(!isOpen, PhotonNetwork.Time, true);
        }

        void ApplyOpenState(bool open, double startTime, bool fromLocalOwner)
        {
            isOpen     = open;
            animStart  = startTime;
            startRot   = transform.localRotation;           // anim desde rot actual
            targetRot  = open ? qOpen : qClose;
            seq++;                                          // nueva versión

            // SFX: lo reproducen todos cuando apliquen esta versión (ver OnPhotonSerializeView Read)
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

        // ---------- Serialization View ----------
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting) // ONLY owner
            {
                stream.SendNext(isOpen);
                stream.SendNext(animStart);
                stream.SendNext(seq);
                stream.SendNext(startRot);                  // snapshot de partida
                // targetRot se deriva de isOpen (qOpen/qClose)
            }
            else
            {
                bool   in_isOpen   = (bool)stream.ReceiveNext();
                double in_animStart= (double)stream.ReceiveNext();
                int    in_seq      = (int)stream.ReceiveNext();
                Quaternion in_start= (Quaternion)stream.ReceiveNext();

                // Ordenar por versión (y por tiempo si hiciera falta)
                if (in_seq > lastAppliedSeq)
                {
                    lastAppliedSeq = in_seq;

                    isOpen    = in_isOpen;
                    animStart = in_animStart;
                    startRot  = in_start;
                    targetRot = isOpen ? qOpen : qClose;

                    // SFX en TODOS al aplicar una nueva versión
                    PlaySfx(isOpen);
                }
            }
        }
    }
}
