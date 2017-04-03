using UnityEngine;
using System.Collections;

public class LampBehaviour : MonoBehaviour, IGvrGazeResponder {

	void LateUpdate() {
		GvrViewer.Instance.UpdateState();
		if (GvrViewer.Instance.BackButtonPressed) {
			Application.Quit();
		}
	}

	public void SetGazedAt(bool gazedAt) {
	}

	public void Reset() {
	}

	public void ChangeLamp(){
		GameObject.Find("Lamps").GetComponent<Lamps>().ChangeLamp ();
		Destroy (gameObject);
	}

	public void ToggleVRMode() {
		GvrViewer.Instance.VRModeEnabled = !GvrViewer.Instance.VRModeEnabled;
	}

	public void ToggleDistortionCorrection() {
		GvrViewer.Instance.DistortionCorrectionEnabled =
			!GvrViewer.Instance.DistortionCorrectionEnabled;
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
	}

	/// Called when the user stops looking on the GameObject, after OnGazeEnter
	/// was already called.
	public void OnGazeExit() {
	}

	/// Called when the viewer's trigger is used, between OnGazeEnter and OnGazeExit.
	public void OnGazeTrigger() {
		ChangeLamp ();
	}

	#endregion
}
