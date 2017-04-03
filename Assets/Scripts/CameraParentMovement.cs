using UnityEngine;
using System.Collections;

public class CameraParentMovement : MonoBehaviour {

	[HideInInspector]
	public bool movement_locked;
	private Rigidbody rb;
	public int speed;
	void Start () {
		rb = GetComponent<Rigidbody>();
		movement_locked = false;
	}

	void Update () {
		rb.angularVelocity = new Vector3 (0, 0, 0);
		rb.velocity = new Vector3 (0, 0, 0);
		if (movement_locked) return;
		
		if (Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow)) {
			//rb.MovePosition(transform.position + transform.forward * Time.deltaTime * 3);
			transform.position += new Vector3(Mathf.Sin(transform.GetChild(0).transform.localEulerAngles.y*Mathf.PI/180) * Time.deltaTime * speed, 0, Mathf.Cos(transform.GetChild(0).transform.localEulerAngles.y*Mathf.PI/180) * Time.deltaTime * speed);
		}
		if (Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow)) {
			transform.position -= new Vector3(Mathf.Sin(transform.GetChild(0).transform.localEulerAngles.y*Mathf.PI/180) * Time.deltaTime * speed, 0, Mathf.Cos(transform.GetChild(0).transform.localEulerAngles.y*Mathf.PI/180) * Time.deltaTime * speed);
			//rb.MovePosition(transform.position - transform.forward * Time.deltaTime * 3);
		}
		if (Input.GetKey("a") || Input.GetKey(KeyCode.LeftArrow)) {
			//rb.MovePosition(transform.position - transform.right * Time.deltaTime * 3);
			transform.position -= new Vector3(Mathf.Cos((-1)*transform.GetChild(0).transform.localEulerAngles.y*Mathf.PI/180) * Time.deltaTime * speed, 0, Mathf.Sin((-1)*transform.GetChild(0).transform.localEulerAngles.y*Mathf.PI/180) * Time.deltaTime * speed);
		}
		if (Input.GetKey("d") || Input.GetKey(KeyCode.RightArrow)) {
			transform.position += new Vector3(Mathf.Cos((-1)*transform.GetChild(0).transform.localEulerAngles.y*Mathf.PI/180) * Time.deltaTime * speed, 0, Mathf.Sin((-1)*transform.GetChild(0).transform.localEulerAngles.y*Mathf.PI/180) * Time.deltaTime * speed);
			//rb.MovePosition(transform.position + transform.right * Time.deltaTime * 3);
		}
	}
}
