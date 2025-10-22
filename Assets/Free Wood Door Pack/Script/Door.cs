using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
namespace DoorScript
{
	[RequireComponent(typeof(AudioSource))]


public class Door : MonoBehaviourPun {
	
	[Header("Animación")]
	[SerializeField] float duration = 0.45f;
	[SerializeField] float openAngle = -90f;
	[SerializeField] float closeAngle = 0f;
	
	
	public bool open;
	public float smooth = 1.0f;
	public AudioSource asource;
	public AudioClip openDoor,closeDoor;
	bool _isOpen;
	double _animStart;
	double _lastChangeTs;

	Quaternion _qOpen, _qClose;

	void Awake()
	{
		if (!asource) asource = GetComponent<AudioSource>();
		_qOpen  = Quaternion.Euler(0f, openAngle, 0f);
		_qClose = Quaternion.Euler(0f, closeAngle, 0f);
	}

	void Start()
	{
		transform.localRotation = _isOpen ? _qOpen : _qClose;
	}

	void Update()
	{
		var t = Mathf.Clamp01((float)((PhotonNetwork.Time - _animStart) / duration));
		var target = _isOpen ? _qOpen : _qClose;
		
		transform.localRotation = Quaternion.Slerp(transform.localRotation, target, t);
	}
	
	public void Toggle()
	{
		photonView.RPC(nameof(RPC_SetOpen), RpcTarget.AllBufferedViaServer, !_isOpen, PhotonNetwork.Time);
	}

	[PunRPC]
	void RPC_SetOpen(bool open, double sentAt, PhotonMessageInfo info)
	{
		
		if (info.SentServerTime < _lastChangeTs) return;
		_lastChangeTs = info.SentServerTime;

		_isOpen = open;
		_animStart = sentAt;

		if (asource)
		{
			asource.clip = open ? openDoor : closeDoor;
			asource.Play();
		}
	}
}
}