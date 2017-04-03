using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class WallBehaviour : MonoBehaviour, IGvrGazeResponder {
	//private Vector3 startingPosition;

	public GameObject options_panel;
	public GameObject button1;
	public GameObject button1_small;
	public GameObject button2;
	//public Material[] mats;
	//[HideInInspector]
	//public int m;//indicele materialului curent
	[HideInInspector]
	public List<GameObject> neighbours;
	[HideInInspector]
	public GameObject twin;
	//[HideInInspector]
	public Material set_material;

	//public material_index <- set by the panel when called -> bullshit!!!, ar trebui sa faca wall-ul tot


	//int crt_ind, set_ind;

	void Start() {
		//SetGazedAt(false);
		set_material=gameObject.GetComponent<Renderer>().material;
	}

	void LateUpdate() {
		GvrViewer.Instance.UpdateState();
		if (GvrViewer.Instance.BackButtonPressed) {
			Application.Quit();
		}
	}

	public void ChangeMaterial(Material m){
		/*
		int max_m = m;
		WallBehaviour script;
		//find the next material that should be set
		for (int i = 0; i < neighbours.Count; i++) {
			script = neighbours [i].GetComponent<WallBehaviour> ();
			if (max_m < script.m)
				max_m = script.m;
		}

		//reset m if end of materials list was reached
		m = max_m + 1;
		if (m == mats.Length) {
			for (int i = 0; i < neighbours.Count; i++) {
				script = neighbours [i].GetComponent<WallBehaviour> ();
				script.m = 0;
			}
		}
		*/
		for (int i = 0; i < neighbours.Count; i++)
			neighbours [i].GetComponent<Renderer> ().material = m;
	}

	public void CancelChangeMaterial(){
		for (int i = 0; i < neighbours.Count; i++)
			neighbours [i].GetComponent<Renderer> ().material = set_material;
	}

	public void ConfirmChangeMaterial(){
		for (int i = 0; i < neighbours.Count; i++)
			neighbours [i].GetComponent<WallBehaviour> ().set_material = neighbours [i].GetComponent<Renderer> ().material;
	}

	public void DisplayChangeMaterialOptions(){
		//get all available materials for walls
		Object[] ms=null;
		if (gameObject.tag=="Wall")
			ms = Resources.LoadAll ("Wall");
		else if (gameObject.tag=="Floor")
			ms = Resources.LoadAll ("Floor");

		GameObject panel = Instantiate (options_panel);
		GameObject camera = GameObject.Find("Main Camera");
		panel.transform.SetParent(camera.transform,false);
		panel.transform.localPosition = new Vector3 (0.5f, 0, 2);
		//set number of items in the panel's script
		panel.GetComponent<PanelManagement> ().nr_items = ms.Length;
		//also set its corresponding type
		panel.GetComponent<PanelManagement> ().type = 2;

		//the maximum number of buttons that can be displayed
		//at any time is 4
		for (int i = 0; i < 4; i++) {
			//add buttons
			GameObject go = GameObject.Instantiate(button2);
			go.transform.SetParent (panel.transform,false);
			go.transform.localPosition = new Vector3 (-120, 220 - i * 140, 0.2f);

			Button b = go.GetComponent<Button> ();
			Material m_copy = Material.Instantiate ((Material)ms[i]);
			b.onClick.AddListener(() => gameObject.GetComponent<WallBehaviour> ().ChangeMaterial(m_copy));

			Image im = go.transform.GetChild(1).GetComponent<Image> ();
			Material m_aux = Material.Instantiate ((Material)ms[i]);
			m_aux.shader = Shader.Find ("UI/Default");
			im.material = m_aux;
		}

		//set the first button as the hovered on one
		panel.transform.GetChild (2).GetComponent<Button> ().GetComponent<Image> ().color = panel.transform.GetChild (2).GetComponent<Button> ().colors.highlightedColor;

		//cancel and confirm buttons
		GameObject cancel_gameobject = GameObject.Instantiate(button1_small);
		cancel_gameobject.transform.SetParent (panel.transform,false);
		cancel_gameobject.transform.localPosition = new Vector3 (-120, -330, 0.2f);
		Button cancel_button = cancel_gameobject.GetComponentInChildren<Button> ();
		cancel_button.GetComponentInChildren<Text> ().text = "Cancel";
		cancel_button.onClick.AddListener (gameObject.GetComponent<WallBehaviour> ().CancelChangeMaterial);

		GameObject confirm_gameobject = GameObject.Instantiate(button1_small);
		confirm_gameobject.transform.SetParent (panel.transform,false);
		confirm_gameobject.transform.localPosition = new Vector3 (120, -330, 0.2f);
		Button confirm_button = confirm_gameobject.GetComponentInChildren<Button> ();
		confirm_button.GetComponentInChildren<Text> ().text = "Confirm";
		confirm_button.onClick.AddListener (gameObject.GetComponent<WallBehaviour> ().ConfirmChangeMaterial);

		//lock user movement
		GameObject.Find("CameraParent").GetComponent<CameraParentMovement>().movement_locked=true;
	}

	public void DeleteSelf(){
		if (twin != null) {
			twin.GetComponent<WallBehaviour> ().twin = null;
			twin.GetComponent<WallBehaviour> ().DeleteSelf ();
		}

		List<GameObject> aux = new List<GameObject>(neighbours);
		foreach (GameObject go in aux)
			go.GetComponent<WallBehaviour> ().neighbours.Remove (gameObject);
		Destroy (gameObject);
	}

	public void DisplayOptions(){
		//instantiate panel
		GameObject panel = Instantiate (options_panel);
		GameObject camera = GameObject.Find("Main Camera");
		panel.transform.SetParent(camera.transform,false);
		panel.transform.localPosition = new Vector3 (0.5f, 0, 2);
		//set number of items in the panel's script
		panel.GetComponent<PanelManagement> ().nr_items = 3;
		//also set its corresponding type
		panel.GetComponent<PanelManagement> ().type = 1;

		//add buttons
		GameObject go1 = GameObject.Instantiate(button1);
		go1.transform.SetParent (panel.transform,false);
		go1.transform.localPosition = new Vector3 (0, 160, 0.2f);
		Button b1 = go1.GetComponentInChildren<Button> ();
		b1.GetComponentInChildren<Text> ().text = "Change Color";
		b1.onClick.AddListener (gameObject.GetComponent<WallBehaviour> ().DisplayChangeMaterialOptions);
		go1.transform.GetChild (0).GetComponent<Image> ().color = Color.red;

		GameObject go2 = GameObject.Instantiate(button1);
		go2.transform.SetParent (panel.transform,false);
		go2.transform.localPosition = new Vector3 (0, 0, 0.2f);
		Button b2 = go2.GetComponentInChildren<Button> ();
		if (twin == null) b2.interactable = false;
		b2.onClick.AddListener(gameObject.GetComponent<WallBehaviour> ().DeleteSelf);
		b2.GetComponentInChildren<Text> ().text = "Destroy";

		GameObject go3 = GameObject.Instantiate(button1);
		go3.transform.SetParent (panel.transform,false);
		go3.transform.localPosition = new Vector3 (0, -160, 0.2f);
		Button b3 = go3.GetComponentInChildren<Button> ();
		//don't assign function to this button, it doesn't have to do anything
		b3.GetComponentInChildren<Text> ().text = "Cancel";

		//lock user movement
		GameObject.Find("CameraParent").GetComponent<CameraParentMovement>().movement_locked=true;
	}

	public void Reset() {
		//transform.localPosition = startingPosition;
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
		//SetGazedAt(true);
	}

	/// Called when the user stops looking on the GameObject, after OnGazeEnter
	/// was already called.
	public void OnGazeExit() {
		//SetGazedAt(false);
	}

	/// Called when the viewer's trigger is used, between OnGazeEnter and OnGazeExit.
	public void OnGazeTrigger() {
		DisplayOptions ();
	}

	public void Bla(){
	}

	#endregion
}

