using Photon.Pun;
using UnityEngine;

namespace CameraDoorScript
{
    public class CameraOpenDoor : MonoBehaviour
    {
        public float DistanceOpen = 3f;
        public GameObject text;

        private GameObject _textInstantiated;
        private PhotonView _ownerPv;

        void Awake() => _ownerPv = GetComponent<PhotonView>();

        void Start()
        {
            if (_ownerPv && !_ownerPv.IsMine) { enabled = false; return; }
            _textInstantiated = Instantiate(text);
            _textInstantiated.SetActive(false);
        }

        void Update()
        {
            if (_ownerPv && !_ownerPv.IsMine) return;

            bool show = false;
            if (Physics.Raycast(transform.position, transform.forward, out var hit, DistanceOpen))
            {
                var door = hit.transform.GetComponent<DoorScript.Door>();
                if (door)
                {
                    show = true;
                    if (Input.GetKeyDown(KeyCode.E))
                        door.TryToggle();
                }
            }
            if (_textInstantiated && _textInstantiated.activeSelf != show)
                _textInstantiated.SetActive(show);
        }
    }
}