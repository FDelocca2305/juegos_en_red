using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace CameraDoorScript
{
public class CameraOpenDoor : MonoBehaviour {
	public float DistanceOpen=3;
	public GameObject text;
	private GameObject _textInstantiated;
	
	private PhotonView _ownerPv;

	void Awake()
	{
		_ownerPv = GetComponent<PhotonView>();
	}

	void Start()
	{
		if (_ownerPv && !_ownerPv.IsMine)
		{
			enabled = false;
			return;
		}
		
		_textInstantiated = Instantiate(text);
		_textInstantiated.SetActive(false);
	}

	void Update()
	{
		if (_ownerPv && !_ownerPv.IsMine) return;

		RaycastHit hit;
		var show = false;

		if (Physics.Raycast(transform.position, transform.forward, out hit, DistanceOpen))
		{
			if (hit.transform.GetComponent<DoorScript.Door>())
			{
				show = true;
				if (Input.GetKeyDown(KeyCode.E))
					hit.transform.GetComponent<DoorScript.Door>().Toggle();
			}
		}

		if (_textInstantiated && _textInstantiated.activeSelf != show)
			_textInstantiated.SetActive(show);
	}
}
}
