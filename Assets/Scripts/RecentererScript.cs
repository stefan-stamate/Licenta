using UnityEngine;
using System.Collections;

public class RecentererScript : MonoBehaviour {

	[HideInInspector]
	public bool active;

	// Use this for initialization
	void Start () {
		active = false;
	}

	void ReactivateColliders(string tag){
		GameObject[] objs = GameObject.FindGameObjectsWithTag (tag);
		foreach (GameObject o in objs) {
			o.GetComponent<Collider> ().enabled = true;
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (!active)
			return;

		if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.B, true)) {

			active = false;
			GameObject.Find ("Camera Parent").GetComponent<CameraParentMovement> ().movement_locked = false;
			GameObject.Find ("UIMaster").GetComponent<UIMasterScript> ().stop = false;
			ReactivateColliders ("Wall");
			ReactivateColliders ("Floor");
			ReactivateColliders ("Ceiling");
			ReactivateColliders ("Door");
		}

		if (Application.platform == RuntimePlatform.Android &&
			!GameObject.Find ("InputMaster").GetComponent<InputModule> ().useCardboard) {

			if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.up, true)) {
				GameObject.Find ("Camera Parent").transform.Rotate (0, 5, 0);
			}
			else if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.down, true)) {
				GameObject.Find ("Camera Parent").transform.Rotate (0, -5, 0);
			}
			else if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.left, true)) {
				GameObject.Find ("Camera Parent").transform.Rotate (-3, 0, 0);
			}
			else if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.right, true)) {
				GameObject.Find ("Camera Parent").transform.Rotate (3, 0, 0);
			}
		}
		else {
			if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.up, true)) {
				GameObject.Find ("Camera Parent").GetComponent<GvrHead> ().r = Quaternion.Euler (
					GameObject.Find ("Camera Parent").transform.localRotation.eulerAngles.x - 0.7f,
					GameObject.Find ("Camera Parent").transform.localRotation.eulerAngles.y,
					0);
				//GameObject.Find ("Camera Parent").GetComponent<GvrHead> ().r *= Quaternion.Euler (3, 0, 0);
			}
			else if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.down, true)) {
				GameObject.Find ("Camera Parent").GetComponent<GvrHead> ().r = Quaternion.Euler (
					GameObject.Find ("Camera Parent").transform.localRotation.eulerAngles.x + 0.7f,
					GameObject.Find ("Camera Parent").transform.localRotation.eulerAngles.y,
					0);
			}
			else if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.left, true)) {
				GameObject.Find ("Camera Parent").GetComponent<GvrHead> ().r = Quaternion.Euler (
					GameObject.Find ("Camera Parent").transform.localRotation.eulerAngles.x,
					GameObject.Find ("Camera Parent").transform.localRotation.eulerAngles.y - 1,
					0);
			}
			else if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.right, true)) {
				GameObject.Find ("Camera Parent").GetComponent<GvrHead> ().r = Quaternion.Euler (
					GameObject.Find ("Camera Parent").transform.localRotation.eulerAngles.x,
					GameObject.Find ("Camera Parent").transform.localRotation.eulerAngles.y + 1,
					0);
			}
		}
	}
}
