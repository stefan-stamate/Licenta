using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DoorBehaviour : MonoBehaviour, IGvrGazeResponder {

	[HideInInspector]
	public int dir;//to know the rotation's sense; set by LoadScene
	private bool open;//to know the state of the door
	private bool moving;//to know if the door is moving so the coroutine doesn't run twice at the same time

	void Start () {
		SetGazedAt(false);
		open = false;
		moving = false;
	}

	public void SetGazedAt(bool gazedAt) {
		
	}

	#if !UNITY_HAS_GOOGLEVR || UNITY_EDITOR
	public void ToggleDirectRender() {
		GvrViewer.Controller.directRender = !GvrViewer.Controller.directRender;
	}
	#endif  //  !UNITY_HAS_GOOGLEVR || UNITY_EDITOR

	#region IGvrGazeResponder implementation

	/// Called when the user is looking on a GameObject with this script,
	/// as long as it is set to an appropriate layer (see GvrGaze).
	public void OnGazeEnter() {
		SetGazedAt(true);
	}

	/// Called when the user stops looking on the GameObject, after OnGazeEnter
	/// was already called.
	public void OnGazeExit() {
		SetGazedAt(false);
	}

	/// Called when the viewer's trigger is used, between OnGazeEnter and OnGazeExit.
	public void OnGazeTrigger() {
		if (!moving) {
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

	#endregion
}
