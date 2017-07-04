using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DoorBehaviour : MonoBehaviour {

	[HideInInspector]
	public int dir;//to know the rotation's sense; set by LoadScene
	[HideInInspector]
	public bool open;//to know the state of the door
	private bool moving;//to know if the door is moving so the coroutine doesn't run twice at the same time
	[HideInInspector]
	public bool active;//to disable its response to gazeTrigger

	void Awake () {
		open = false;
		moving = false;
		active = true;
	}

	/// Called when the viewer's trigger is used, between OnGazeEnter and OnGazeExit.
	public void OnGazeTrigger() {
		if (active && !moving) {
			moving = true;
			StartCoroutine (DoorMove ());
		}
	}

	IEnumerator DoorMove(){
		
		for (int i = 0; i < 30; i++) {
			if (!open) transform.rotation *= Quaternion.AngleAxis (-3 * dir, Vector3.up);
			else transform.rotation *= Quaternion.AngleAxis (3 * dir, Vector3.up);
			yield return new WaitForSeconds(0.01f);
		}
		open = true ^ open;
		moving = false;
	}
}
