using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class ObjectBehaviour : MonoBehaviour {

	[HideInInspector]
	public string path;
	[HideInInspector]
	public int type;

	private GameObject options_panel, button1;

	void Start(){
		gameObject.AddComponent<EventTrigger> ();
		EventTrigger trigger = GetComponent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerClick;
		entry.callback.AddListener( (eventData) => { OnGazeTrigger(); } );
		trigger.triggers.Add(entry);

		options_panel = GameObject.Find("UIMaster").GetComponent<UIMasterScript>().options_panel;
		button1 = GameObject.Find("UIMaster").GetComponent<UIMasterScript>().button1;
	}

	void StartRepositioning(){

		//e mai ok asa
		string[] parts = path.Split ('/');
		GameObject.Find("ObjectInserter").GetComponent<ObjectInsertion>().InsertObject (parts[parts.Length-3],parts[parts.Length-1]);
		NetworkedObjectDeletion ();
	}

	void NetworkedObjectDeletion(){

		if (GameObject.Find ("NetworkingHandler").GetComponent<NetworkUtils> ().connected) {
			int obj_id = System.Array.IndexOf (GameObject.FindGameObjectsWithTag ("Object"), gameObject);
			int[] input = { obj_id };
			PhotonNetwork.RaiseEvent (13, input, true, null);
		}

		ObjectDeletion ();
	}

	public void ObjectDeletion(){
		
		//remove from attached_objs lists
		if (type == 1) {
			foreach (GameObject f in GameObject.FindGameObjectsWithTag("Floor"))
				f.GetComponent<WallBehaviour> ().attached_objs.Remove (gameObject);
		}
		else if (type == 3) {
			foreach (GameObject c in GameObject.FindGameObjectsWithTag("Ceiling"))
				c.GetComponent<WallBehaviour> ().attached_objs.Remove (gameObject);
		}
		else {
			foreach (GameObject f in GameObject.FindGameObjectsWithTag("Floor"))
				f.GetComponent<WallBehaviour> ().attached_objs.Remove (gameObject);
			foreach (GameObject c in GameObject.FindGameObjectsWithTag("Ceiling"))
				c.GetComponent<WallBehaviour> ().attached_objs.Remove (gameObject);
			foreach (GameObject w in GameObject.FindGameObjectsWithTag("Wall"))
				w.GetComponent<WallBehaviour> ().attached_objs.Remove (gameObject);
		}

		Destroy (gameObject);
	}

	void DisplayObjectOptions(){
		GameObject panel = Instantiate (options_panel);
		GameObject camera = GameObject.Find("Camera Parent");
		panel.transform.SetParent(camera.transform,false);
		panel.transform.localPosition = new Vector3 (0.8f, 0, 2);
		panel.GetComponent<PanelManagement> ().nr_items = 3;
		panel.GetComponent<PanelManagement> ().type = 1;
		panel.transform.GetChild (1).GetComponent<Text> ().text = "Object Menu";

		GameObject child_panel = new GameObject ("Child Panel");
		child_panel.AddComponent<CanvasRenderer>();
		child_panel.AddComponent<Image>();
		child_panel.transform.SetParent (panel.transform, false);
		child_panel.transform.localPosition = new Vector3 (0, -50, 0);
		child_panel.AddComponent<Mask> ();
		child_panel.GetComponent<CanvasRenderer> ().GetComponent<RectTransform> ().sizeDelta = new Vector2 (500, 600);
		Color c = child_panel.GetComponent<Image> ().color;
		c.a = 1.0f/255;
		child_panel.GetComponent<Image> ().color = c;
		panel.GetComponent<PanelManagement> ().SetChildPanel (child_panel);

		GameObject go1 = GameObject.Instantiate(button1);
		go1.transform.SetParent (child_panel.transform,false);
		go1.transform.localPosition = new Vector3 (0, 200, 0.2f);
		Button b1 = go1.GetComponentInChildren<Button> ();
		b1.GetComponentInChildren<Text> ().text = "Reposition object";
		b1.onClick.AddListener (StartRepositioning);
		go1.transform.GetChild (0).GetComponent<Image> ().color = Color.red;

		GameObject go2 = GameObject.Instantiate(button1);
		go2.transform.SetParent (child_panel.transform,false);
		go2.transform.localPosition = new Vector3 (0, 0, 0.2f);
		Button b2 = go2.GetComponentInChildren<Button> ();
		b2.GetComponentInChildren<Text> ().text = "Delete object";
		b2.onClick.AddListener (NetworkedObjectDeletion);

		GameObject go3 = GameObject.Instantiate(button1);
		go3.transform.SetParent (child_panel.transform,false);
		go3.transform.localPosition = new Vector3 (0, -200, 0.2f);
		Button b3 = go3.GetComponentInChildren<Button> ();
		b3.GetComponentInChildren<Text> ().text = "Cancel";

		//deactivate objects' colliders so they cannot
		//be triggered while using the menu
		GameObject.Find("UtilsHolder").GetComponent<Utils>().DeactivateColliders("Wall");
		GameObject.Find("UtilsHolder").GetComponent<Utils>().DeactivateColliders("Door");

		//lock user movement
		GameObject.Find("Camera Parent").GetComponent<CameraParentMovement>().movement_locked=true;
	}

	public void OnGazeTrigger() {
		DisplayObjectOptions ();
	}
}
