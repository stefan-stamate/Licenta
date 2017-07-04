using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class UIMasterScript : MonoBehaviour {

	public GameObject options_panel;
	public GameObject button1;
	public GameObject scrollbar;
	public GameObject touch_buttons;

	private GameObject wall_prefab;
	private GameObject door_interaction_prefab;
	private GameObject window_parent;

	//have to memorise them because we can't list
	//files in android
	private string[] floorplans = {
		"default 1",
		"default 2",
	};

	[HideInInspector]
	public bool stop;
	private const string roomName = "InteriorDesignVR";

	void Start () {
		stop = true;

		wall_prefab = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().wall_prefab;
		door_interaction_prefab = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().door_interaction_prefab;
		window_parent = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().window_parent;

		//GameObject child_cam = GameObject.Find ("Camera Parent");
		//child_cam.AddComponent (Camera);
		//child_cam.GetComponent<Camera>().enabled = true;

		if (Application.platform == RuntimePlatform.Android) {
			GameObject panel = Instantiate (options_panel);
			GameObject camera = GameObject.Find ("Camera Parent");
			panel.transform.SetParent (camera.transform, false);
			panel.transform.localPosition = new Vector3 (0, 0, 2);
			panel.GetComponent<PanelManagement> ().nr_items = 2;
			panel.GetComponent<PanelManagement> ().type = 1;
			panel.transform.GetChild (1).GetComponent<Text> ().text = "Select control mode";

			GameObject child_panel = new GameObject ("Child Panel");
			child_panel.AddComponent<CanvasRenderer> ();
			child_panel.AddComponent<Image> ();
			child_panel.transform.SetParent (panel.transform, false);
			child_panel.transform.localPosition = new Vector3 (0, -50, 0);
			child_panel.AddComponent<Mask> ();
			child_panel.GetComponent<CanvasRenderer> ().GetComponent<RectTransform> ().sizeDelta = new Vector2 (500, 600);
			Color c = child_panel.GetComponent<Image> ().color;
			c.a = 1.0f / 255;
			child_panel.GetComponent<Image> ().color = c;
			panel.GetComponent<PanelManagement> ().SetChildPanel (child_panel);

			GameObject go1 = GameObject.Instantiate (button1);
			go1.transform.SetParent (child_panel.transform, false);
			go1.transform.localPosition = new Vector3 (0, 200, 0.2f);
			Button b1 = go1.GetComponentInChildren<Button> ();
			b1.GetComponentInChildren<Text> ().text = "Mono (without cardboard)";
			b1.onClick.AddListener (delegate {
				ToggleViewMode (false);
			});
			go1.transform.GetChild (0).GetComponent<Image> ().color = Color.red;

			GameObject go2 = GameObject.Instantiate (button1);
			go2.transform.SetParent (child_panel.transform, false);
			go2.transform.localPosition = new Vector3 (0, -100, 0.2f);
			Button b2 = go2.GetComponentInChildren<Button> ();
			b2.GetComponentInChildren<Text> ().text = "Stereo (with cardboard)";
			b2.onClick.AddListener (delegate {
				ToggleViewMode (true);
			});
		}
		else {
			DisplayNetworkOptions ();
		}
	}

	void ToggleViewMode(bool useCardboard){
		GameObject.Find ("Camera Parent").GetComponent<CameraParentMovement> ().movement_locked = true;
		stop = true;

		GameObject.Find ("InputMaster").GetComponent<InputModule> ().useCardboard = useCardboard;

		if (useCardboard) {
			UnityEngine.VR.VRSettings.enabled = true;
		}
		else {
			Input.gyro.enabled = true;
			GameObject.Instantiate (touch_buttons);

			//reset child camera rotation because it gets changed by gyro activation or smth
			GameObject.Find("Camera Parent").transform.localRotation = Quaternion.Euler(0,0,0);

			//if the mono version of the app is used on the phone
			//then don't create the device ----------- chiar e o problema daca il creez si dezactivez vrsettings?
			//make it as though gvrViewer doesn't do anything
		}

		DisplayNetworkOptions ();
	}

	void DisplayNetworkOptions(){

		GameObject panel = Instantiate (options_panel);
		GameObject camera = GameObject.Find ("Camera Parent");
		panel.transform.SetParent (camera.transform, false);
		panel.transform.localPosition = new Vector3 (0, 0, 2);
		panel.GetComponent<PanelManagement> ().nr_items = 2;
		panel.GetComponent<PanelManagement> ().type = 1;
		panel.transform.GetChild (1).GetComponent<Text> ().text = "Select networking mode";

		GameObject child_panel = new GameObject ("Child Panel");
		child_panel.AddComponent<CanvasRenderer> ();
		child_panel.AddComponent<Image> ();
		child_panel.transform.SetParent (panel.transform, false);
		child_panel.transform.localPosition = new Vector3 (0, -50, 0);
		child_panel.AddComponent<Mask> ();
		child_panel.GetComponent<CanvasRenderer> ().GetComponent<RectTransform> ().sizeDelta = new Vector2 (500, 600);
		Color c = child_panel.GetComponent<Image> ().color;
		c.a = 1.0f / 255;
		child_panel.GetComponent<Image> ().color = c;
		panel.GetComponent<PanelManagement> ().SetChildPanel (child_panel);

		GameObject go1 = GameObject.Instantiate (button1);
		go1.transform.SetParent (child_panel.transform, false);
		go1.transform.localPosition = new Vector3 (0, 200, 0.2f);
		Button b1 = go1.GetComponentInChildren<Button> ();
		b1.GetComponentInChildren<Text> ().text = "Offline";
		b1.onClick.AddListener (delegate {
			ToggleMultiplayer (false);
		});
		go1.transform.GetChild (0).GetComponent<Image> ().color = Color.red;

		GameObject go2 = GameObject.Instantiate (button1);
		go2.transform.SetParent (child_panel.transform, false);
		go2.transform.localPosition = new Vector3 (0, -100, 0.2f);
		Button b2 = go2.GetComponentInChildren<Button> ();
		b2.GetComponentInChildren<Text> ().text = "Online";
		b2.onClick.AddListener (delegate {
			ToggleMultiplayer (true);
		});
	}

	void DisplayFloorplans(){

		GameObject.Find ("Camera Parent").GetComponent<CameraParentMovement> ().movement_locked = true;
		stop = true;

		if (!Directory.Exists(Application.persistentDataPath+ "/floorplans")){    
			//if it doesn't, create it
			Directory.CreateDirectory(Application.persistentDataPath+ "/floorplans");
		}
		List<string> persistent_files = new List<string> ();
		foreach (string path in System.IO.Directory.GetFiles(Application.persistentDataPath+ "/floorplans","*.obj")) {
			string[] parts = path.Split ('/');
			if (Application.platform != RuntimePlatform.Android)
				persistent_files.Add (parts [parts.Length - 1].Split ('\\') [1].Split ('.') [0]);
			else
				persistent_files.Add (parts [parts.Length - 1].Split ('.') [0]);
		}
	
		GameObject panel = Instantiate (options_panel);
		GameObject camera = GameObject.Find ("Camera Parent");
		panel.transform.SetParent (camera.transform, false);
		panel.transform.localPosition = new Vector3 (0, 0, 2);
		panel.GetComponent<PanelManagement> ().nr_items = floorplans.Length + persistent_files.Count + 1;
		panel.GetComponent<PanelManagement> ().type = 1;
		panel.transform.GetChild (1).GetComponent<Text> ().text = "Choose floorplan";

		if (panel.GetComponent<PanelManagement> ().nr_items > 3) {
			GameObject sb = GameObject.Instantiate (scrollbar);
			sb.GetComponent<Scrollbar> ().numberOfSteps = 0;
			sb.GetComponent<Scrollbar> ().size = 1.0f / panel.GetComponent<PanelManagement> ().nr_items;
			sb.GetComponent<Scrollbar> ().value = 0;
			sb.transform.SetParent (panel.transform, false);
			sb.transform.localPosition = new Vector3 (220, 0, 0);
			sb.transform.localScale = new Vector3 (1.5f, 3, 1);
		}

		GameObject child_panel = new GameObject ("Child Panel");
		child_panel.AddComponent<CanvasRenderer> ();
		child_panel.AddComponent<Image> ();
		child_panel.transform.SetParent (panel.transform, false);
		child_panel.transform.localPosition = new Vector3 (0, -50, 0);
		child_panel.AddComponent<Mask> ();
		child_panel.GetComponent<CanvasRenderer> ().GetComponent<RectTransform> ().sizeDelta = new Vector2 (500, 600);
		Color c = child_panel.GetComponent<Image> ().color;
		c.a = 1.0f / 255;
		child_panel.GetComponent<Image> ().color = c;
		panel.GetComponent<PanelManagement> ().SetChildPanel (child_panel);

		int height = 200;
		foreach (string file in floorplans) {
			GameObject go = GameObject.Instantiate (button1);
			go.transform.SetParent (child_panel.transform, false);
			go.transform.localPosition = new Vector3 (0, height, 0.2f);
			Button b = go.GetComponentInChildren<Button> ();
			b.GetComponentInChildren<Text> ().text = file;
			string aux = file;
			b.onClick.AddListener (delegate {
				//GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().StartBuild (true, file+".obj");
				//StartCreation(0, file+".obj");
				GameObject.Find("SceneLoader").GetComponent<LoadScene>().StartBuild(
					Path.Combine(Application.streamingAssetsPath, aux+".obj"));
			});
			height -= 200;
		}
			
		foreach (string file in persistent_files) {
			GameObject go = GameObject.Instantiate (button1);
			go.transform.SetParent (child_panel.transform, false);
			go.transform.localPosition = new Vector3 (0, height, 0.2f);
			Button b = go.GetComponentInChildren<Button> ();
			b.GetComponentInChildren<Text> ().text = file;
			string aux = file;
			b.onClick.AddListener (delegate {
				//GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().StartBuild (false, file+".obj");
				//StartCreation(1, file+".obj");
				GameObject.Find("SceneLoader").GetComponent<LoadScene>().StartBuild(
					Path.Combine(Application.persistentDataPath, "floorplans/" + aux+".obj"));
			});
			height -= 200;
		}
			
		GameObject quit_go = GameObject.Instantiate (button1);
		quit_go.transform.SetParent (child_panel.transform, false);
		quit_go.transform.localPosition = new Vector3 (0, height, 0.2f);
		Button quit_b = quit_go.GetComponentInChildren<Button> ();
		quit_b.GetComponentInChildren<Text> ().text = "Back";
		quit_b.onClick.AddListener (DisplayBuildingOptions);

		child_panel.transform.GetChild(0).transform.GetChild (0).GetComponent<Image> ().color = Color.red;
	}

	void DisplayUserSaves(){

		GameObject.Find ("Camera Parent").GetComponent<CameraParentMovement> ().movement_locked = true;
		stop = true;

		if (!Directory.Exists(Application.persistentDataPath+ "/saves")){    
			//if it doesn't, create it
			Directory.CreateDirectory(Application.persistentDataPath+ "/saves");

		}
		List<string> files = new List<string> ();
		string[] parts;
		foreach (string path in System.IO.Directory.GetFiles(Application.persistentDataPath+ "/saves","*.sav")){
			parts = path.Split ('/');
			if (Application.platform != RuntimePlatform.Android)
				files.Add (parts [parts.Length - 1].Split('\\')[1].Split('.')[0]);
			else
				files.Add (parts [parts.Length - 1].Split('.')[0]);
		}

		GameObject panel = Instantiate (options_panel);
		GameObject camera = GameObject.Find ("Camera Parent");
		panel.transform.SetParent (camera.transform, false);
		panel.transform.localPosition = new Vector3 (0, 0, 2);
		panel.GetComponent<PanelManagement> ().nr_items = files.Count + 1;
		panel.GetComponent<PanelManagement> ().type = 1;
		panel.transform.GetChild (1).GetComponent<Text> ().text = "Choose save";

		GameObject child_panel = new GameObject ("Child Panel");
		child_panel.AddComponent<CanvasRenderer> ();
		child_panel.AddComponent<Image> ();
		child_panel.transform.SetParent (panel.transform, false);
		child_panel.transform.localPosition = new Vector3 (0, -50, 0);
		child_panel.AddComponent<Mask> ();
		child_panel.GetComponent<CanvasRenderer> ().GetComponent<RectTransform> ().sizeDelta = new Vector2 (500, 600);
		Color c = child_panel.GetComponent<Image> ().color;
		c.a = 1.0f / 255;
		child_panel.GetComponent<Image> ().color = c;
		panel.GetComponent<PanelManagement> ().SetChildPanel (child_panel);

		int height = 200;
		foreach (string file in files) {
			GameObject go = GameObject.Instantiate (button1);
			go.transform.SetParent (child_panel.transform, false);
			go.transform.localPosition = new Vector3 (0, height, 0.2f);
			Button b = go.GetComponentInChildren<Button> ();
			parts = file.Split(',');
			b.GetComponentInChildren<Text> ().text = parts [0] + "." + parts [1] + " - " + parts [2] + ":" + parts [3];
			string aux = file;
			b.onClick.AddListener (delegate {
				//LoadSaveManager.LoadScene(file);
				//StartCreation(2,file);
				LoadSaveManager.LoadScene(aux);
			});
			height -= 200;
		}

		GameObject quit_go = GameObject.Instantiate (button1);
		quit_go.transform.SetParent (child_panel.transform, false);
		quit_go.transform.localPosition = new Vector3 (0, height, 0.2f);
		Button quit_b = quit_go.GetComponentInChildren<Button> ();
		quit_b.GetComponentInChildren<Text> ().text = "Back";
		quit_b.onClick.AddListener (DisplayBuildingOptions);

		child_panel.transform.GetChild(0).transform.GetChild (0).GetComponent<Image> ().color = Color.red;
	}

	void DisplayBuildingOptions(){

		GameObject.Find ("Camera Parent").GetComponent<CameraParentMovement> ().movement_locked = true;
		stop = true;

		GameObject panel = Instantiate (options_panel);
		GameObject camera = GameObject.Find ("Camera Parent");
		panel.transform.SetParent (camera.transform, false);
		panel.transform.localPosition = new Vector3 (0, 0, 2);
		panel.GetComponent<PanelManagement> ().nr_items = 2;
		panel.GetComponent<PanelManagement> ().type = 1;
		panel.transform.GetChild (1).GetComponent<Text> ().text = "Create or join session";

		GameObject child_panel = new GameObject ("Child Panel");
		child_panel.AddComponent<CanvasRenderer> ();
		child_panel.AddComponent<Image> ();
		child_panel.transform.SetParent (panel.transform, false);
		child_panel.transform.localPosition = new Vector3 (0, -50, 0);
		child_panel.AddComponent<Mask> ();
		child_panel.GetComponent<CanvasRenderer> ().GetComponent<RectTransform> ().sizeDelta = new Vector2 (500, 600);
		Color c = child_panel.GetComponent<Image> ().color;
		c.a = 1.0f / 255;
		child_panel.GetComponent<Image> ().color = c;
		panel.GetComponent<PanelManagement> ().SetChildPanel (child_panel);

		GameObject go1 = GameObject.Instantiate (button1);
		go1.transform.SetParent (child_panel.transform, false);
		go1.transform.localPosition = new Vector3 (0, 200, 0.2f);
		Button b1 = go1.GetComponentInChildren<Button> ();
		b1.GetComponentInChildren<Text> ().text = "Floorplans";
		b1.onClick.AddListener (DisplayFloorplans);
		go1.transform.GetChild (0).GetComponent<Image> ().color = Color.red;

		GameObject go2 = GameObject.Instantiate (button1);
		go2.transform.SetParent (child_panel.transform, false);
		go2.transform.localPosition = new Vector3 (0, -100, 0.2f);
		Button b2 = go2.GetComponentInChildren<Button> ();
		b2.GetComponentInChildren<Text> ().text = "Saves";
		b2.onClick.AddListener (DisplayUserSaves);
	}

	void ToggleMultiplayer(bool online){
		if (online) //--------display in text case that you are connecting
			GameObject.Find ("NetworkingHandler").GetComponent < NetworkUtils> ().StartNetwork ();
		else
			DisplayBuildingOptions ();
			//GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().StartBuild ();
	}

	public void DisplayConnections(){
		GameObject.Find ("Camera Parent").GetComponent<CameraParentMovement> ().movement_locked = true;
		stop = true;

		GameObject panel = Instantiate (options_panel);
		GameObject camera = GameObject.Find ("Camera Parent");
		panel.transform.SetParent (camera.transform, false);
		panel.transform.localPosition = new Vector3 (0, 0, 2);
		panel.GetComponent<PanelManagement> ().nr_items = 2;
		panel.GetComponent<PanelManagement> ().type = 1;
		panel.transform.GetChild (1).GetComponent<Text> ().text = "Create or join session";

		GameObject child_panel = new GameObject ("Child Panel");
		child_panel.AddComponent<CanvasRenderer> ();
		child_panel.AddComponent<Image> ();
		child_panel.transform.SetParent (panel.transform, false);
		child_panel.transform.localPosition = new Vector3 (0, -50, 0);
		child_panel.AddComponent<Mask> ();
		child_panel.GetComponent<CanvasRenderer> ().GetComponent<RectTransform> ().sizeDelta = new Vector2 (500, 600);
		Color c = child_panel.GetComponent<Image> ().color;
		c.a = 1.0f / 255;
		child_panel.GetComponent<Image> ().color = c;
		panel.GetComponent<PanelManagement> ().SetChildPanel (child_panel);

		GameObject go1 = GameObject.Instantiate (button1);
		go1.transform.SetParent (child_panel.transform, false);
		go1.transform.localPosition = new Vector3 (0, 200, 0.2f);
		Button b1 = go1.GetComponentInChildren<Button> ();
		b1.GetComponentInChildren<Text> ().text = "Create room";
		b1.onClick.AddListener (StartSession);
		go1.transform.GetChild (0).GetComponent<Image> ().color = Color.red;

		GameObject go2 = GameObject.Instantiate (button1);
		go2.transform.SetParent (child_panel.transform, false);
		go2.transform.localPosition = new Vector3 (0, -100, 0.2f);
		Button b2 = go2.GetComponentInChildren<Button> ();
		b2.GetComponentInChildren<Text> ().text = "Search rooms";
		b2.onClick.AddListener (DisplayHostList);
	}

	void StartSession(){
		PhotonNetwork.CreateRoom (roomName, new RoomOptions () { MaxPlayers = 4 }, null);
		//GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().StartBuild ();
		DisplayBuildingOptions ();
	}

	void DisplayHostList(){
		GameObject.Find ("Camera Parent").GetComponent<CameraParentMovement> ().movement_locked = true;
		stop = true;

		RoomInfo[] roomsList = PhotonNetwork.GetRoomList ();

		//display a back button
		GameObject panel = Instantiate (options_panel);
		GameObject camera = GameObject.Find ("Camera Parent");
		panel.transform.SetParent (camera.transform, false);
		panel.transform.localPosition = new Vector3 (0, 0, 2);
		panel.GetComponent<PanelManagement> ().nr_items = 1+roomsList.Length;
		panel.GetComponent<PanelManagement> ().type = 1;
		panel.transform.GetChild (1).GetComponent<Text> ().text = "Display host list";

		GameObject child_panel = new GameObject ("Child Panel");
		child_panel.AddComponent<CanvasRenderer> ();
		child_panel.AddComponent<Image> ();
		child_panel.transform.SetParent (panel.transform, false);
		child_panel.transform.localPosition = new Vector3 (0, -50, 0);
		child_panel.AddComponent<Mask> ();
		child_panel.GetComponent<CanvasRenderer> ().GetComponent<RectTransform> ().sizeDelta = new Vector2 (500, 600);
		Color c = child_panel.GetComponent<Image> ().color;
		c.a = 1.0f / 255;
		child_panel.GetComponent<Image> ().color = c;
		panel.GetComponent<PanelManagement> ().SetChildPanel (child_panel);

		GameObject go0 = GameObject.Instantiate (button1);
		go0.transform.SetParent (child_panel.transform, false);
		go0.transform.localPosition = new Vector3 (0, 200, 0.2f);
		Button b0 = go0.GetComponentInChildren<Button> ();
		b0.GetComponentInChildren<Text> ().text = "Back";
		b0.onClick.AddListener (delegate {
			DisplayConnections();
		});
		go0.transform.GetChild (0).GetComponent<Image> ().color = Color.red;

		for (int i = 0; i < roomsList.Length; i++) {
			GameObject go = GameObject.Instantiate (button1);
			go.transform.SetParent (child_panel.transform, false);
			go.transform.localPosition = new Vector3 (0, -100 - i * 300, 0.2f);
			Button b = go.GetComponentInChildren<Button> ();
			b.GetComponentInChildren<Text> ().text = roomsList [i].Name;
			b.onClick.AddListener (delegate {
				MyJoin(b.GetComponentInChildren<Text> ().text);
			});
		}
	}

	void MyJoin(string room_name){
		GameObject.Find("GvrReticle").GetComponent<MeshRenderer>().enabled = true;
		PhotonNetwork.JoinRoom(room_name);
	}

	public void NetworkedCreateAuxObj(string path, GameObject target, Vector3 hit_normal, Vector3 object_position){

		int wall_id = target.GetComponent<WallBehaviour> ().FindOwnId ();
		object[] input = {
			path,
			wall_id,
			hit_normal.x,
			hit_normal.z,
			object_position.x,
			object_position.y,
			object_position.z,
		};

		CreateAuxObj (input);
		if (GameObject.Find("NetworkingHandler").GetComponent<NetworkUtils>().connected)
			PhotonNetwork.RaiseEvent (11, input, true, null);
	}

	public void CreateAuxObj(object[] input){

		string path = (string)input [0];
		int type, tier;
		if (path.Contains ("Doors"))
			type = 1;
		else //contains("Windows")
			type = 2;

		if (path.Contains ("Tier 1"))
			tier = 1;
		else // contains("Tier 2")
			tier = 2;

		float obj_width;
		if (type == 1) {
			if (tier == 1)
				obj_width = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().tier1_door_size;
			else // tier == 2
				obj_width = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().tier2_door_size;
		}
		else { //type == 2
			if (tier == 1)
				obj_width = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().tier1_window_size;
			else // tier == 2
				obj_width = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().tier2_window_size;
		}

		float x_length, z_length;
		Vector3 hit_normal = new Vector3 ((float)input [2], 0, (float)input [3]);

		if (hit_normal.x != 0) {
			x_length = 0.2f;
			z_length = obj_width;
		}
		else {// hit.normal.z != 0
			x_length = obj_width;
			z_length = 0.2f;
		}

		GameObject object_prefab = Resources.Load (path) as GameObject;
		GameObject o = Instantiate (object_prefab);

		if (hit_normal.x == -1) 
			o.transform.rotation = Quaternion.Euler (0, 270, 0);
		else if (hit_normal.x == 1)
			o.transform.rotation = Quaternion.Euler (0, 90, 0);
		else if (hit_normal.z == 1) 
			o.transform.rotation = Quaternion.Euler (0, 0, 0);
		else if (hit_normal.z == -1) 
			o.transform.rotation = Quaternion.Euler (0, 180, 0);

		o.transform.position = new Vector3 (
			(float)input [4],
			(float)input [5],
			(float)input [6]
		);

		float x_pos = o.transform.position.x;
		float z_pos = o.transform.position.z;

		o.transform.position -= hit_normal * 0.2f;

		GameObject w1, w2, w3, w4 = null, tw1, tw2, tw3, tw4=null;
		float lat_min, lat_max, obj_lat_min, obj_lat_max;
		float uni_scale = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().uni_scale;
		float height = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().height;

		GameObject p;
		if (type == 1) {
			p = GameObject.Instantiate (door_interaction_prefab);
			if (hit_normal.x != 0) {
				if (hit_normal.x == 1) {
					p.transform.position = new Vector3 (x_pos, height * 3 / 8, z_pos - z_length * uni_scale);
					p.GetComponent<BoxCollider> ().center = new Vector3 (-z_length * uni_scale / p.transform.localScale.x, 0, 0);
					p.GetComponent<DoorBehaviour> ().dir = (int)Mathf.Sign (-x_length);//-------- * (-1)
				}
				else {
					p.transform.position = new Vector3 (x_pos, height * 3 / 8, z_pos + z_length * uni_scale);
					p.GetComponent<BoxCollider> ().center = new Vector3 (z_length * uni_scale / p.transform.localScale.x, 0, 0);
					p.GetComponent<DoorBehaviour> ().dir = (int)Mathf.Sign (-x_length);
				}

				p.transform.rotation = Quaternion.Euler (0, 90, 0);
				p.GetComponent<BoxCollider> ().size = new Vector3 (z_length * 2 * uni_scale, height * 3 / 4, p.GetComponent<BoxCollider> ().size.z);
			}
			else { //hit_normal.z != 0
				if (hit_normal.z == 1) {
					p.transform.position = new Vector3 (x_pos + x_length * uni_scale, height * 3 / 8, z_pos);
					p.GetComponent<BoxCollider> ().center = new Vector3 (-x_length * uni_scale / p.transform.localScale.x, 0, 0);
					p.GetComponent<DoorBehaviour> ().dir = (int)Mathf.Sign (-z_length);
				}
				else {
					p.transform.position = new Vector3 (x_pos - x_length * uni_scale, height * 3 / 8, z_pos);
					p.GetComponent<BoxCollider> ().center = new Vector3 (x_length * uni_scale / p.transform.localScale.x, 0, 0);
					p.GetComponent<DoorBehaviour> ().dir = (int)Mathf.Sign (-z_length);
				}

				p.transform.rotation = Quaternion.Euler (0, 0, 0);
				p.GetComponent<BoxCollider> ().size = new Vector3 (x_length * 2 * uni_scale, height * 3 / 4, p.GetComponent<BoxCollider> ().size.z);
			}
		}
		else { //type == 2
			p = GameObject.Instantiate (window_parent);
			if (hit_normal.x != 0)
				p.GetComponent<BoxCollider> ().size = new Vector3 (p.GetComponent<BoxCollider> ().size.x, height * 10 / 16, obj_width * 2 * uni_scale);
			else ////hit_normal.z != 0
				p.GetComponent<BoxCollider> ().size = new Vector3 (obj_width * 2 * uni_scale, height * 10 / 16, p.GetComponent<BoxCollider> ().size.z);

			p.transform.position = o.transform.position;
		}

		p.GetComponent<AuxObjectParent> ().path = path;
		o.transform.parent = p.transform;

		GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");
		GameObject target = walls [(int)input [1]];
		target.GetComponent<WallBehaviour>().aux_objs.Add(p);

		if (hit_normal.x != 0) {
			lat_min = target.transform.position.z - target.transform.localScale.z / 2;
			lat_max = target.transform.position.z + target.transform.localScale.z / 2;
			obj_lat_min = z_pos - obj_width * uni_scale;
			obj_lat_max = z_pos + obj_width * uni_scale;

			//build the walls that replace the original
			w1 = Instantiate (wall_prefab);
			w1.transform.position = new Vector3 (target.transform.position.x, height / 2, (lat_min + obj_lat_min) / 2);
			w1.transform.localScale = new Vector3 (0.2f, height, obj_lat_min - lat_min);

			w2 = Instantiate (wall_prefab);
			w2.transform.position = new Vector3 (target.transform.position.x, height / 2, (lat_max + obj_lat_max) / 2);
			w2.transform.localScale = new Vector3 (0.2f, height, lat_max - obj_lat_max);

			if (type == 1) {
				w3 = Instantiate (wall_prefab);
				w3.transform.position = new Vector3 (target.transform.position.x, height * 7 / 8, z_pos);
				w3.transform.localScale = new Vector3 (0.2f, height / 4, obj_lat_max - obj_lat_min);
			}
			else {
				w3 = Instantiate (wall_prefab);
				w3.transform.position = new Vector3 (target.transform.position.x, height * 15 / 16, z_pos);
				w3.transform.localScale = new Vector3 (0.2f, height * 1 / 8, obj_lat_max - obj_lat_min);

				w4 = Instantiate (wall_prefab);
				w4.transform.position = new Vector3 (target.transform.position.x, height * 2 / 16, z_pos);
				w4.transform.localScale = new Vector3 (0.2f, height * 2 / 8, obj_lat_max - obj_lat_min);
			}
		}
		else { //hit_normal.z != 0

			lat_min = target.transform.position.x - target.transform.localScale.x / 2;
			lat_max = target.transform.position.x + target.transform.localScale.x / 2;
			obj_lat_min = x_pos - obj_width * uni_scale;
			obj_lat_max = x_pos + obj_width * uni_scale;

			//build the walls that replace the original
			w1 = Instantiate (wall_prefab);
			w1.transform.position = new Vector3 ((lat_min + obj_lat_min)/2, height / 2, target.transform.position.z);
			w1.transform.localScale = new Vector3(obj_lat_min - lat_min, height, 0.2f);

			w2 = Instantiate (wall_prefab);
			w2.transform.position = new Vector3 ((lat_max + obj_lat_max)/2, height / 2, target.transform.position.z);
			w2.transform.localScale = new Vector3(lat_max - obj_lat_max, height, 0.2f);

			if (type == 1) {
				w3 = Instantiate (wall_prefab);
				w3.transform.position = new Vector3 (x_pos, height * 7 / 8, target.transform.position.z);
				w3.transform.localScale = new Vector3 (obj_lat_max - obj_lat_min, height / 4, 0.2f);
			}
			else {//type==2
				w3 = Instantiate (wall_prefab);
				w3.transform.position = new Vector3 (x_pos, height * 15 / 16, target.transform.position.z);
				w3.transform.localScale = new Vector3 (obj_lat_max - obj_lat_min, height * 1 / 8, 0.2f);

				w4 = Instantiate (wall_prefab);
				w4.transform.position = new Vector3 (x_pos, height * 2 / 16, target.transform.position.z);
				w4.transform.localScale = new Vector3 (obj_lat_max - obj_lat_min, height * 2 / 8, 0.2f);
			}
		}

		//and also their twins
		tw1 = Instantiate(wall_prefab);
		tw1.transform.position = w1.transform.position - hit_normal * 0.2f;
		tw1.transform.localScale = w1.transform.localScale;
		w1.GetComponent<WallBehaviour> ().twin = tw1;
		tw1.GetComponent<WallBehaviour> ().twin = w1;

		tw2 = Instantiate(wall_prefab);
		tw2.transform.position = w2.transform.position - hit_normal * 0.2f;
		tw2.transform.localScale = w2.transform.localScale;
		w2.GetComponent<WallBehaviour> ().twin = tw2;
		tw2.GetComponent<WallBehaviour> ().twin = w2;

		tw3 = Instantiate(wall_prefab);
		tw3.transform.position = w3.transform.position - hit_normal * 0.2f;
		tw3.transform.localScale = w3.transform.localScale;
		w3.GetComponent<WallBehaviour> ().twin = tw3;
		tw3.GetComponent<WallBehaviour> ().twin = w3;

		if (type == 2) {
			tw4 = Instantiate (wall_prefab);
			tw4.transform.position = w4.transform.position - hit_normal * 0.2f;
			tw4.transform.localScale = w4.transform.localScale;
			w4.GetComponent<WallBehaviour> ().twin = tw4;
			tw4.GetComponent<WallBehaviour> ().twin = w4;
		}

		List<GameObject> new_walls = new List<GameObject>();
		List<GameObject> new_twin_walls = new List<GameObject>();

		new_walls.Add (w1);
		new_walls.Add (w2);
		new_walls.Add (w3);

		new_twin_walls.Add (tw1);
		new_twin_walls.Add (tw2);
		new_twin_walls.Add (tw3);

		if (type==2){
			new_walls.Add (w4);
			new_twin_walls.Add (tw4);
		}

		//recompute wall_neighbours, room_neighbours, aux_objs
		List<GameObject> new_wall_neighbours,new_room_neighbours;

		new_wall_neighbours = new List<GameObject>();
		foreach (GameObject n in target.GetComponent<WallBehaviour>().wall_neighbours)
			new_wall_neighbours.Add (n);
		new_wall_neighbours.Remove (target);
		foreach(GameObject w in new_walls)
			new_wall_neighbours.Add (w);
		foreach (GameObject n in new_wall_neighbours) 
			n.GetComponent<WallBehaviour> ().wall_neighbours = new_wall_neighbours;

		new_room_neighbours=new List<GameObject>();
		foreach (GameObject n in target.GetComponent<WallBehaviour>().room_neighbours)
			new_room_neighbours.Add (n);
		new_room_neighbours.Remove (target);
		foreach(GameObject w in new_walls)
			new_room_neighbours.Add (w);
		foreach (GameObject n in new_room_neighbours) {
			n.GetComponent<WallBehaviour> ().room_neighbours = new_room_neighbours;
		}

		foreach(GameObject w in new_walls)
			w.GetComponent<WallBehaviour>().aux_objs=target.GetComponent<WallBehaviour>().aux_objs;

		//do it for twins too
		new_wall_neighbours = new List<GameObject>();
		foreach (GameObject n in target.GetComponent<WallBehaviour>().twin.GetComponent<WallBehaviour>().wall_neighbours)
			new_wall_neighbours.Add (n);
		new_wall_neighbours.Remove (target.GetComponent<WallBehaviour>().twin);
		foreach(GameObject w in new_twin_walls)
			new_wall_neighbours.Add (w);
		foreach (GameObject n in new_wall_neighbours) 
			n.GetComponent<WallBehaviour> ().wall_neighbours = new_wall_neighbours;

		new_room_neighbours=new List<GameObject>();
		foreach (GameObject n in target.GetComponent<WallBehaviour>().twin.GetComponent<WallBehaviour>().room_neighbours)
			new_room_neighbours.Add (n);
		new_room_neighbours.Remove (target.GetComponent<WallBehaviour>().twin);
		foreach(GameObject w in new_twin_walls)
			new_room_neighbours.Add (w);
		foreach (GameObject n in new_room_neighbours) {
			n.GetComponent<WallBehaviour> ().room_neighbours = new_room_neighbours;
		}

		foreach(GameObject w in new_twin_walls)
			w.GetComponent<WallBehaviour>().aux_objs=target.GetComponent<WallBehaviour>().aux_objs;

		//set destructible property
		if (target.GetComponent<WallBehaviour> ().destructible == false) {
			foreach(GameObject w in new_walls)
				w.GetComponent<WallBehaviour> ().destructible = false;

			foreach(GameObject w in new_twin_walls)
				w.GetComponent<WallBehaviour> ().destructible = false;
		}

		//attached_objs
		foreach (GameObject w in new_walls) {
			if (hit_normal.x != 0) {
				lat_min = target.transform.position.z - target.transform.localScale.z / 2;
				lat_max = target.transform.position.z + target.transform.localScale.z / 2;
				foreach (GameObject obj in target.GetComponent<WallBehaviour>().attached_objs) {
					if (obj.transform.position.z >= lat_min && obj.transform.position.z <= lat_max) {
						w.GetComponent<WallBehaviour> ().attached_objs.Add (obj);
						break;
					}
				}
			}
			else { //hit_normal.z != 0
				lat_min = target.transform.position.x - target.transform.localScale.x / 2;
				lat_max = target.transform.position.x + target.transform.localScale.x / 2;
				foreach (GameObject obj in target.GetComponent<WallBehaviour>().attached_objs) {
					if (obj.transform.position.x >= lat_min && obj.transform.position.x <= lat_max) {
						w.GetComponent<WallBehaviour> ().attached_objs.Add (obj);
						break;
					}
				}
			}
		}

		//------------si pentru twin!


		Destroy (target.GetComponent<WallBehaviour> ().twin);
		Destroy (target);
	}

	void StartRecenter(){
		GameObject.Find ("Recenterer").GetComponent<RecentererScript> ().active = true;
		GameObject.Find("Camera Parent").GetComponent<CameraParentMovement>().movement_locked=true;
		stop = true;
		GameObject.Find("UtilsHolder").GetComponent<Utils>().DeactivateColliders("Wall");
		GameObject.Find("UtilsHolder").GetComponent<Utils>().DeactivateColliders("Floor");
		GameObject.Find("UtilsHolder").GetComponent<Utils>().DeactivateColliders("Ceiling");
		GameObject.Find("UtilsHolder").GetComponent<Utils>().DeactivateColliders("Door");
	}
	
	void Update () {

		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}

		if (Application.platform == RuntimePlatform.Android) {
			//if the mono version of the app is used on the phone
			//the rotation has to be implemented manually
			if (!GameObject.Find ("InputMaster").GetComponent<InputModule> ().useCardboard) {
				GameObject.Find ("Camera Parent").transform.Rotate (-Input.gyro.rotationRateUnbiased.x * 2, -Input.gyro.rotationRateUnbiased.y * 3, 0);
				GameObject.Find ("Camera Parent").transform.localRotation = Quaternion.Euler (
					GameObject.Find ("Camera Parent").transform.localRotation.eulerAngles.x,
					GameObject.Find ("Camera Parent").transform.localRotation.eulerAngles.y,
					0);
			}
		}

		if (stop)
			return;

		if (GameObject.Find("InputMaster").GetComponent<InputModule>().CheckForPress(buttons.B)) {

			GameObject panel = Instantiate (options_panel);
			GameObject camera = GameObject.Find("Camera Parent");
			panel.transform.SetParent(camera.transform,false);
			panel.transform.localPosition = new Vector3 (0.8f, 0, 2);
			panel.GetComponent<PanelManagement> ().nr_items = 8;
			panel.GetComponent<PanelManagement> ().type = 1;
			panel.transform.GetChild (1).GetComponent<Text> ().text = "Main menu";

			GameObject sb = GameObject.Instantiate (scrollbar);
			sb.GetComponent<Scrollbar> ().numberOfSteps = 0;
			sb.GetComponent<Scrollbar> ().size = 1.0f / panel.GetComponent<PanelManagement> ().nr_items;
			sb.GetComponent<Scrollbar> ().value = 0;
			sb.transform.SetParent (panel.transform, false);
			sb.transform.localPosition = new Vector3 (220, 0, 0);
			sb.transform.localScale = new Vector3 (1.5f, 3, 1);

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
			b1.GetComponentInChildren<Text> ().text = "Add wall";
			b1.onClick.AddListener (gameObject.transform.GetChild(0).GetComponent<WallCreation>().ActivateRaycasting);
			go1.transform.GetChild (0).GetComponent<Image> ().color = Color.red;

			GameObject go2 = GameObject.Instantiate(button1);
			go2.transform.SetParent (child_panel.transform,false);
			go2.transform.localPosition = new Vector3 (0, 0, 0.2f);
			Button b2 = go2.GetComponentInChildren<Button> ();
			b2.GetComponentInChildren<Text> ().text = "Add object";
			//b2.onClick.AddListener (gameObject.transform.GetChild(1).GetComponent<ObjectInsertion>().DisplayContents);
			b2.onClick.AddListener(delegate{gameObject.transform.GetChild(1).GetComponent<ObjectInsertion>().DisplayContents("Root");});

			GameObject go3 = GameObject.Instantiate(button1);
			go3.transform.SetParent (child_panel.transform,false);
			go3.transform.localPosition = new Vector3 (0, -200, 0.2f);
			Button b3 = go3.GetComponentInChildren<Button> ();
			b3.GetComponentInChildren<Text> ().text = "Add door";
			b3.onClick.AddListener(delegate{gameObject.transform.GetChild(2).GetComponent<DandWInsertion>().DisplayTiers(1);});

			GameObject go4 = GameObject.Instantiate(button1);
			go4.transform.SetParent (child_panel.transform,false);
			go4.transform.localPosition = new Vector3 (0, -400, 0.2f);
			Button b4 = go4.GetComponentInChildren<Button> ();
			b4.GetComponentInChildren<Text> ().text = "Add window";
			b4.onClick.AddListener(delegate{gameObject.transform.GetChild(2).GetComponent<DandWInsertion>().DisplayTiers(2);});

			GameObject go5 = GameObject.Instantiate(button1);
			go5.transform.SetParent (child_panel.transform,false);
			go5.transform.localPosition = new Vector3 (0, -600, 0.2f);
			Button b5 = go5.GetComponentInChildren<Button> ();
			b5.GetComponentInChildren<Text> ().text = "Destroy door/window";
			b5.onClick.AddListener (gameObject.transform.GetChild(3).GetComponent<DandWDestroy>().StartDestroy);

			GameObject go6 = GameObject.Instantiate(button1);
			go6.transform.SetParent (child_panel.transform,false);
			go6.transform.localPosition = new Vector3 (0, -800, 0.2f);
			Button b6 = go6.GetComponentInChildren<Button> ();
			b6.GetComponentInChildren<Text> ().text = "Save Scene";
			b6.onClick.AddListener (GameObject.Find("LoadSaveHandler").GetComponent<LoadSaveManager>().SaveScene);

			GameObject go7 = GameObject.Instantiate(button1);
			go7.transform.SetParent (child_panel.transform,false);
			go7.transform.localPosition = new Vector3 (0, -1000, 0.2f);
			Button b7 = go7.GetComponentInChildren<Button> ();
			b7.GetComponentInChildren<Text> ().text = "Recenter";
			b7.onClick.AddListener (StartRecenter);

			GameObject go8 = GameObject.Instantiate(button1);
			go8.transform.SetParent (child_panel.transform,false);
			go8.transform.localPosition = new Vector3 (0, -1200, 0.2f);
			Button b8 = go8.GetComponentInChildren<Button> ();
			b8.GetComponentInChildren<Text> ().text = "Cancel";

			//deactivate objects' colliders so they cannot
			//be triggered while using the menu
			GameObject.Find("UtilsHolder").GetComponent<Utils>().DeactivateColliders("Wall");
			GameObject.Find("UtilsHolder").GetComponent<Utils>().DeactivateColliders("Floor");
			GameObject.Find("UtilsHolder").GetComponent<Utils>().DeactivateColliders("Ceiling");
			GameObject.Find("UtilsHolder").GetComponent<Utils>().DeactivateColliders("Door");

			//lock user movement
			GameObject.Find("Camera Parent").GetComponent<CameraParentMovement>().movement_locked=true;
		}
	}
}
