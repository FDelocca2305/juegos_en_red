using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraDoorScript
{
public class CameraOpenDoor : MonoBehaviour {
	public float DistanceOpen=3;
	public GameObject text;
	private GameObject _textInstantiated;
	
	void Start ()
	{
		_textInstantiated = Instantiate(text);
	}
	
	void Update () {
		RaycastHit hit;
		if (Physics.Raycast (transform.position, transform.forward, out hit, DistanceOpen)) {
				if (hit.transform.GetComponent<DoorScript.Door> ()) {
					_textInstantiated.SetActive (true);
				if (Input.GetKeyDown(KeyCode.E))
					hit.transform.GetComponent<DoorScript.Door> ().Toggle();
			}else{
					_textInstantiated.SetActive (false);
			}
		}else{
			_textInstantiated.SetActive (false);
		}
	}
}
}
