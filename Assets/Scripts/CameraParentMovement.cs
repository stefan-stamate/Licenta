using UnityEngine;
using System.Collections;

public class CameraParentMovement : MonoBehaviour {

	[HideInInspector]
	public bool movement_locked;
	private Rigidbody rb;
	public int speed;
	void Start () {
		rb = GetComponent<Rigidbody>();
		movement_locked = true;
		if (Application.platform != RuntimePlatform.Android)
			StartCoroutine (StartCamera ());
	}

	IEnumerator StartCamera(){
		yield return new WaitForEndOfFrame ();
		//gameObject.GetComponent<Camera> ().enabled = true;
		Destroy(transform.GetChild(0).GetComponent<GvrHead>());
	}

	void Update () {
		rb.angularVelocity = new Vector3 (0, 0, 0);
		rb.velocity = new Vector3 (0, 0, 0);
		if (movement_locked) return;
		
		if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.up, true)) {
			transform.position += new Vector3(Mathf.Sin(transform.localEulerAngles.y*Mathf.PI/180) * Time.deltaTime * speed, 0, Mathf.Cos(transform.localEulerAngles.y*Mathf.PI/180) * Time.deltaTime * speed);
		}
		if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.down, true)) {
			transform.position -= new Vector3(Mathf.Sin(transform.localEulerAngles.y*Mathf.PI/180) * Time.deltaTime * speed, 0, Mathf.Cos(transform.localEulerAngles.y*Mathf.PI/180) * Time.deltaTime * speed);
		}
		if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.left, true)) {
			transform.position -= new Vector3(Mathf.Cos((-1)*transform.localEulerAngles.y*Mathf.PI/180) * Time.deltaTime * speed, 0, Mathf.Sin((-1)*transform.localEulerAngles.y*Mathf.PI/180) * Time.deltaTime * speed);
		}
		if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.right, true)) {
			transform.position += new Vector3(Mathf.Cos((-1)*transform.localEulerAngles.y*Mathf.PI/180) * Time.deltaTime * speed, 0, Mathf.Sin((-1)*transform.localEulerAngles.y*Mathf.PI/180) * Time.deltaTime * speed);
		}
	}
}
