using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Collider))]
public class WallBehaviour : MonoBehaviour, IGvrGazeResponder {
	//private Vector3 startingPosition;

	private GameObject wall_prefab;
	public GameObject options_panel;
	public GameObject button1;
	public GameObject button1_small;
	public GameObject button2;
	public GameObject scrollbar;
	[HideInInspector]
	public List<GameObject> wall_neighbours;//peretii alaturi de care formeaza un perete efectiv
	[HideInInspector]
	public List<GameObject> room_neighbours;//ceilalti pereti ai camerei
	[HideInInspector]
	public List<GameObject> aux_objs;//referinte catre usa/geamul pe care le contine
	[HideInInspector]
	public List<GameObject> attached_objs;//referinte catre obiecte atasate
	[HideInInspector]
	public GameObject twin;
	[HideInInspector]
	public bool destructible;
	[HideInInspector]
	public bool active;//folosit pentru a preveni activarea functiei DisplayOptions
	//public Material set_material;
	[HideInInspector]
	public int current_mid, set_mid;//material_id
	//get all available materials for walls/floors
	Material[] ms;

	class MyTuple{
		public GameObject a,b;

		public MyTuple(GameObject a, GameObject b){
			this.a=a;
			this.b=b;
		}
	}

	bool CloseEnough(float a, float b){
		return Mathf.Abs (a - b) < 0.0001f;
	}

	GameObject FuseWalls(GameObject a, GameObject b, int dir){
		//first find out the center of the wall that result from the fusion of the pair
		float middle;
		GameObject new_wall = Instantiate (wall_prefab);

		//if the parents are indestructible so is the new wall
		//only need to check one of them
		if (!a.GetComponent<WallBehaviour> ().destructible)
			new_wall.GetComponent<WallBehaviour> ().destructible = false;

		if (dir == 1) {//x
			if (a.transform.position.z < b.transform.position.z) {
				middle = (a.transform.position.z - a.transform.localScale.z / 2 +
					b.transform.position.z + b.transform.localScale.z / 2) / 2;
			}
			else {
				middle = (a.transform.position.z + a.transform.localScale.z / 2 +
					b.transform.position.z - b.transform.localScale.z / 2) / 2;
			}

			//then set its position
			new_wall.transform.position = new Vector3 (a.transform.position.x, a.transform.position.y, middle);
			new_wall.transform.localScale = new Vector3 (a.transform.localScale.x, a.transform.localScale.y, a.transform.localScale.z + b.transform.localScale.z);
		}
		else {//z
			if (a.transform.position.x < b.transform.position.x) {
				middle = (a.transform.position.x - a.transform.localScale.x / 2 +
					b.transform.position.x + b.transform.localScale.x / 2) / 2;
			}
			else {
				middle = (a.transform.position.x + a.transform.localScale.x / 2 +
					b.transform.position.x - b.transform.localScale.x / 2) / 2;
			}

			//then set its position
			new_wall.transform.position = new Vector3 (middle, a.transform.position.y, a.transform.position.z);
			new_wall.transform.localScale = new Vector3 (a.transform.localScale.x + b.transform.localScale.x, a.transform.localScale.y, a.transform.localScale.z);
		}

		//combine the aux_objs and attached_objs too
		List<GameObject> new_aux_objs=new List<GameObject>();
		foreach (GameObject o in a.GetComponent<WallBehaviour>().aux_objs) {
			new_aux_objs.Add (o);
		}
		foreach (GameObject o in b.GetComponent<WallBehaviour>().aux_objs) {
			new_aux_objs.Add (o);
		}
		new_wall.GetComponent<WallBehaviour> ().aux_objs = new_aux_objs;

		//only walls have wall_neighbours, while floors and ceilings don't
		//so it's not necessary to compute the wall_neighbours for a resulting component
		if (a.GetComponent<WallBehaviour>().wall_neighbours.Count!=0 &&
			b.GetComponent<WallBehaviour>().wall_neighbours.Count!=0) {

			//build the wall_neighbours for the new wall
			List<GameObject> new_wall_neighbours = new List<GameObject> ();
			foreach (GameObject n in a.GetComponent<WallBehaviour>().wall_neighbours) {
				new_wall_neighbours.Add (n);
				n.GetComponent<WallBehaviour> ().aux_objs = new_aux_objs;
			}
			new_wall_neighbours.Remove (a);
			foreach (GameObject n in b.GetComponent<WallBehaviour>().wall_neighbours) {
				new_wall_neighbours.Add (n);
				n.GetComponent<WallBehaviour> ().aux_objs = new_aux_objs;
			}
			new_wall_neighbours.Remove (b);
			new_wall_neighbours.Add (new_wall);

			foreach (GameObject n in new_wall_neighbours) {
				n.GetComponent<WallBehaviour> ().wall_neighbours = new_wall_neighbours;
			}
		}

		List<GameObject> new_attached_objs=new List<GameObject>();
		foreach (GameObject o in a.GetComponent<WallBehaviour>().attached_objs) {
			new_attached_objs.Add (o);
		}
		foreach (GameObject o in b.GetComponent<WallBehaviour>().attached_objs) {
			new_attached_objs.Add (o);
		}
		new_wall.GetComponent<WallBehaviour> ().attached_objs = new_attached_objs;

		Destroy (a);
		Destroy (b);
		return new_wall;
	}

	void DeactivateColliders(string tag){
		GameObject[] objs = GameObject.FindGameObjectsWithTag (tag);
		foreach (GameObject o in objs) {
			o.GetComponent<Collider> ().enabled = false;
		}
	}

	void Awake() {
		//SetGazedAt(false);
		//set_material = gameObject.GetComponent<Renderer>().material;
		set_mid = 0;
		current_mid = 0;

		active = true;
		//it cannot hold its own prefab because it will lead to unusual behaviour
		wall_prefab = GameObject.Find ("WallCreator").GetComponent<WallCreation> ().wall_prefab;

		twin=null;
		destructible=true;
	}

	void Start(){
		if (gameObject.tag=="Wall" || gameObject.tag=="Ceiling")
			ms = Resources.LoadAll ("Wall", typeof(Material)).Cast<Material>().ToArray();
		else // gameObject.tag=="Floor"
			ms = Resources.LoadAll ("Floor", typeof(Material)).Cast<Material>().ToArray();

		gameObject.GetComponent<Renderer> ().material = ms[set_mid];
	}

	//function which determines a wall's id
	//that is, his position in the array of walls in the scene
	public int FindOwnId(){
		return System.Array.IndexOf (GameObject.FindGameObjectsWithTag (tag), gameObject);
	}

	void NetworkedChangeMaterial(int id){
		ChangeMaterial (id);

		if (GameObject.Find ("NetworkingHandler").GetComponent<NetworkUtils> ().connected) {
			int wall_id = FindOwnId ();
			object[] values = {
				tag,
				wall_id,
				id,
			};
			PhotonNetwork.RaiseEvent (6, values, true, null);
		}
	}

	public void ChangeMaterial(int id){
		for (int i = 0; i < room_neighbours.Count; i++) {
			room_neighbours [i].GetComponent<Renderer> ().material = ms[id];
			room_neighbours [i].GetComponent<WallBehaviour> ().current_mid = id;
		}
	}

	void NetworkedCancelChangeMaterial(){
		CancelChangeMaterial ();
		if (GameObject.Find ("NetworkingHandler").GetComponent<NetworkUtils> ().connected) {
			int wall_id = FindOwnId ();
			object[] values = {
				tag,
				wall_id,
			};
			PhotonNetwork.RaiseEvent (7, values, true, null);
		}
	}

	public void CancelChangeMaterial(){
		for (int i = 0; i < room_neighbours.Count; i++)
			room_neighbours [i].GetComponent<Renderer> ().material = ms[set_mid];
	}

	void NetworkedConfirmChangeMaterial(){
		ConfirmChangeMaterial ();
		if (GameObject.Find ("NetworkingHandler").GetComponent<NetworkUtils> ().connected) {
			int wall_id = FindOwnId ();
			object[] values = {
				tag,
				wall_id,
			};
			PhotonNetwork.RaiseEvent (8, values, true, null);
		}
	}

	public void ConfirmChangeMaterial(){
		for (int i = 0; i < room_neighbours.Count; i++) 
			//room_neighbours [i].GetComponent<WallBehaviour> ().set_material = room_neighbours [i].GetComponent<Renderer> ().material;
			room_neighbours [i].GetComponent<WallBehaviour> ().set_mid = room_neighbours [i].GetComponent<WallBehaviour> ().current_mid;
	}

	public void DisplayChangeMaterialOptions(){
		
		GameObject panel = Instantiate (options_panel);
		GameObject camera = GameObject.Find("Camera Parent");
		panel.transform.SetParent(camera.transform,false);
		panel.transform.localPosition = new Vector3 (0.8f, 0, 2);
		//set number of items in the panel's script
		panel.GetComponent<PanelManagement> ().nr_items = ms.Length-1;
		//also set its corresponding type
		panel.GetComponent<PanelManagement> ().type = 2;
		panel.transform.GetChild (1).GetComponent<Text> ().text = "Select color";

		//add a scrollbar to know the position in the list
		if (ms.Length > 4) {
			GameObject sb = GameObject.Instantiate (scrollbar);
			sb.GetComponent<Scrollbar> ().numberOfSteps = 0;
			sb.GetComponent<Scrollbar> ().size = 1.0f / ms.Length;
			sb.GetComponent<Scrollbar> ().value = 0;
			sb.transform.SetParent (panel.transform, false);
			sb.transform.localPosition = new Vector3 (220, 0, 0);
			sb.transform.localScale = new Vector3 (1.5f, 3, 1);
		}

		//create parent gameobject for buttons
		GameObject child_panel = new GameObject ("Child Panel");
		child_panel.AddComponent<CanvasRenderer>();
		child_panel.AddComponent<Image>();
		child_panel.transform.SetParent (panel.transform, false);
		child_panel.transform.localPosition = new Vector3 (0, 0, 0);
		child_panel.AddComponent<Mask> ();
		child_panel.GetComponent<CanvasRenderer> ().GetComponent<RectTransform> ().sizeDelta = new Vector2 (500, 550);
		Color c = child_panel.GetComponent<Image> ().color;
		c.a = 1.0f/255;
		child_panel.GetComponent<Image> ().color = c;
		panel.GetComponent<PanelManagement> ().SetChildPanel (child_panel);

		for (int i = 1; i < ms.Length; i++) {
			//add buttons
			GameObject go = GameObject.Instantiate(button2);
			go.transform.SetParent (child_panel.transform,false);
			go.transform.localPosition = new Vector3 (-140, 360 - i * 140, 0.2f);

			Button b = go.GetComponent<Button> ();
			int aux = i;
			b.onClick.AddListener(() => gameObject.GetComponent<WallBehaviour> ().NetworkedChangeMaterial(aux));

			Image im = go.transform.GetChild(1).GetComponent<Image> ();
			Material m_aux = Material.Instantiate ((Material)ms[i]);
			m_aux.shader = Shader.Find ("UI/Default");
			im.material = m_aux;
		}

		//set the first button as the hovered on one
		child_panel.transform.GetChild(0).GetComponent<Button> ().GetComponent<Image> ().color = child_panel.transform.GetChild(0).GetComponent<Button> ().colors.highlightedColor;

		//cancel and confirm buttons
		GameObject cancel_gameobject = GameObject.Instantiate(button1_small);
		cancel_gameobject.transform.SetParent (panel.transform,false);
		cancel_gameobject.transform.localPosition = new Vector3 (-120, -330, 0.2f);
		Button cancel_button = cancel_gameobject.GetComponentInChildren<Button> ();
		cancel_button.GetComponentInChildren<Text> ().text = "Cancel";
		cancel_button.onClick.AddListener (gameObject.GetComponent<WallBehaviour> ().NetworkedCancelChangeMaterial);

		GameObject confirm_gameobject = GameObject.Instantiate(button1_small);
		confirm_gameobject.transform.SetParent (panel.transform,false);
		confirm_gameobject.transform.localPosition = new Vector3 (120, -330, 0.2f);
		Button confirm_button = confirm_gameobject.GetComponentInChildren<Button> ();
		confirm_button.GetComponentInChildren<Text> ().text = "Confirm";
		confirm_button.onClick.AddListener (gameObject.GetComponent<WallBehaviour> ().NetworkedConfirmChangeMaterial);

		DeactivateColliders (tag);
		DeactivateColliders ("Door");

		//lock user movement
		GameObject.Find("Camera Parent").GetComponent<CameraParentMovement>().movement_locked=true;
	}

	void CombineFloorsAndCeilings(List<GameObject> new_ceiling_neighbours, List<GameObject> new_floor_neighbours){

		int i, j;
		bool flag = false;

		for (i = 0; i < new_ceiling_neighbours.Count - 1 && !flag; i++) {
			for (j = i + 1; j < new_ceiling_neighbours.Count && !flag; j++) {

				if (new_ceiling_neighbours[i].transform.position.x==
					new_ceiling_neighbours[j].transform.position.x){

					GameObject new_ceiling = FuseWalls (
						new_ceiling_neighbours[i],
						new_ceiling_neighbours[j],
						1);
					new_ceiling_neighbours.Remove (new_ceiling_neighbours[i]);
					new_ceiling_neighbours.Remove (new_ceiling_neighbours[j-1]);
					new_ceiling.tag = "Ceiling";
					new_ceiling_neighbours.Add (new_ceiling);
					new_ceiling.GetComponent<WallBehaviour> ().room_neighbours = new_ceiling_neighbours;

					GameObject new_floor =  FuseWalls (
						new_floor_neighbours[i],
						new_floor_neighbours[j],
						1);
					new_floor_neighbours.Remove (new_floor_neighbours [i]);
					new_floor_neighbours.Remove (new_floor_neighbours [j-1]);
					new_floor.tag = "Floor";
					new_floor_neighbours.Add (new_floor);
					new_floor.GetComponent<WallBehaviour> ().room_neighbours = new_floor_neighbours;

					flag = true;
					break;
				}
					
				if (new_ceiling_neighbours[i].transform.position.z==
					new_ceiling_neighbours[j].transform.position.z){

					GameObject new_ceiling = FuseWalls (
						new_ceiling_neighbours[i],
						new_ceiling_neighbours[j],
						-1);
					new_ceiling_neighbours.Remove (new_ceiling_neighbours[i]);
					new_ceiling_neighbours.Remove (new_ceiling_neighbours[j-1]);
					new_ceiling.tag = "Ceiling";
					new_ceiling_neighbours.Add (new_ceiling);
					new_ceiling.GetComponent<WallBehaviour> ().room_neighbours = new_ceiling_neighbours;

					GameObject new_floor =  FuseWalls (
						new_floor_neighbours[i],
						new_floor_neighbours[j],
						-1);
					new_floor_neighbours.Remove (new_floor_neighbours [i]);
					new_floor_neighbours.Remove (new_floor_neighbours [j-1]);
					new_floor.tag = "Floor";
					new_floor_neighbours.Add (new_floor);
					new_floor.GetComponent<WallBehaviour> ().room_neighbours = new_floor_neighbours;

					flag = true;
					break;
				}
			}
		}

		if (flag && new_floor_neighbours.Count>1)
			CombineFloorsAndCeilings (new_ceiling_neighbours, new_floor_neighbours);
	}

	//the array of floors and ceilings have to be sent
	//because the selected wall (the ones that starts DeleteSelf) could
	//change the floors and ceilings in the scene and its twin needs them too
	void DeleteAttachedObjs(GameObject[] floors, GameObject[] ceilings){

		//the attached_objs that cross over the whole wall on both sides must be destroyed
		Dictionary<GameObject, GameObject> objects_parents = new Dictionary<GameObject, GameObject> ();

		foreach (GameObject n in room_neighbours) {
			if ((n.transform.localScale.x == 0.2f &&
			     CloseEnough (n.transform.position.x, transform.position.x)) ||
			     (n.transform.localScale.z == 0.2f &&
			     CloseEnough (n.transform.position.z, transform.position.z))) {

				foreach (GameObject o in n.GetComponent<WallBehaviour> ().attached_objs) {
					objects_parents [o] = n;
				}
			}
		}

		float lat_min, lat_max, obj_lat_min, obj_lat_max;
		float x_min, x_max, z_min, z_max;

		foreach (GameObject o in objects_parents.Keys) {
			
			if (transform.localScale.z == 0.2f) {
				obj_lat_min = o.transform.position.x -
				Mathf.Abs (o.transform.right.x) * o.GetComponent<BoxCollider> ().size.x / 2;
				obj_lat_max = o.transform.position.x +
				Mathf.Abs (o.transform.right.x) * o.GetComponent<BoxCollider> ().size.x / 2;
			}
			else { // transform.localScale.x == 0.2f
				obj_lat_min = o.transform.position.z -
				Mathf.Abs (o.transform.right.z) * o.GetComponent<BoxCollider> ().size.x / 2;
				obj_lat_max = o.transform.position.z +
				Mathf.Abs (o.transform.right.z) * o.GetComponent<BoxCollider> ().size.x / 2;
			}

			foreach (GameObject n in wall_neighbours) {

				if (transform.localScale.z == 0.2f) {
					lat_min = n.transform.position.x - n.transform.localScale.x / 2;
					lat_max = n.transform.position.x + n.transform.localScale.x / 2;
				}
				else { // transform.localScale.x == 0.2f
					lat_min = n.transform.position.z - n.transform.localScale.z / 2;
					lat_max = n.transform.position.z + n.transform.localScale.z / 2;
				}

				if (((obj_lat_min < lat_max && obj_lat_min > lat_min) ||
				   (obj_lat_max < lat_max && obj_lat_max > lat_min) ||
				   (obj_lat_min < lat_min && obj_lat_min < lat_max &&
				   obj_lat_max > lat_min && obj_lat_max > lat_max))) {

					Destroy (o);
					objects_parents [o].GetComponent<WallBehaviour> ().attached_objs.Remove (o);

					//determine the object's master_floor and master_ceiling
					//in order to remove the object from the list of attached_objs
					foreach (GameObject floor in floors) {
						x_min = floor.transform.position.x - floor.transform.localScale.x / 2;
						x_max = floor.transform.position.x + floor.transform.localScale.x / 2;
						z_min = floor.transform.position.z - floor.transform.localScale.z / 2;
						z_max = floor.transform.position.z + floor.transform.localScale.z / 2;

						if (o.transform.position.x > x_min &&
						   o.transform.position.x < x_max &&
						   o.transform.position.z > z_min &&
						   o.transform.position.z < z_max) {

							floor.GetComponent<WallBehaviour> ().attached_objs.Remove (o);
							break;
						}
					}

					foreach (GameObject ceiling in ceilings) {
						x_min = ceiling.transform.position.x - ceiling.transform.localScale.x / 2;
						x_max = ceiling.transform.position.x + ceiling.transform.localScale.x / 2;
						z_min = ceiling.transform.position.z - ceiling.transform.localScale.z / 2;
						z_max = ceiling.transform.position.z + ceiling.transform.localScale.z / 2;

						if (o.transform.position.x > x_min &&
						   o.transform.position.x < x_max &&
						   o.transform.position.z > z_min &&
						   o.transform.position.z < z_max) {

							ceiling.GetComponent<WallBehaviour> ().attached_objs.Remove (o);
							break;
						}
					}

					//if the object was deleted there is no need
					//to search the rest of the walls
					break;
				}
			}
		}
	}

	void NetworkedDeleteSelf(){

		if (GameObject.Find ("NetworkingHandler").GetComponent<NetworkUtils> ().connected) {
			int wall_id = FindOwnId ();
			int[] values = { wall_id };
			PhotonNetwork.RaiseEvent (9, values, true, null);
		}

		DeleteSelf ();
	}

	public void DeleteSelf(){

		if (twin != null) {
			GameObject[] floors = GameObject.FindGameObjectsWithTag ("Floor");
			GameObject[] ceilings = GameObject.FindGameObjectsWithTag ("Ceiling");

			//any walls or windows contained must be destroyed
			foreach (GameObject g in aux_objs) {
				Destroy (g);
			}

			DeleteAttachedObjs (floors, ceilings);
			twin.GetComponent<WallBehaviour> ().DeleteAttachedObjs (floors, ceilings);
				
			//readjust the neighbours array for the remaining walls so we get correct behaviour
			List<GameObject> new_neighbours = new List<GameObject> (room_neighbours);

			//get the walls in the twin's room
			if (twin.GetComponent<WallBehaviour> ().room_neighbours != room_neighbours) {
				foreach (GameObject n in twin.GetComponent<WallBehaviour>().room_neighbours) {
					new_neighbours.Add (n);
				}
			}

			foreach (GameObject n in wall_neighbours) {
				new_neighbours.Remove (n);
				room_neighbours.Remove (n);
			}
			List<GameObject> twin_room_neighbours = twin.GetComponent<WallBehaviour> ().room_neighbours;
			foreach (GameObject n in twin.GetComponent<WallBehaviour>().wall_neighbours) {
				new_neighbours.Remove (n);
				twin_room_neighbours.Remove (n);
			}

			//see if any of the walls of the new room can be fused together
			float lower1, lower2, upper1, upper2;
			List<MyTuple> pairs = new List<MyTuple> ();

			foreach (GameObject g1 in room_neighbours) {
				foreach (GameObject g2 in twin_room_neighbours) {
					if (g1.transform.localScale.x == g2.transform.localScale.x &&
					    g1.transform.localScale.x == 0.2f && g1.transform.position.x == g2.transform.position.x) {

						lower1 = g1.transform.position.z - g1.transform.localScale.z / 2;
						lower2 = g2.transform.position.z - g2.transform.localScale.z / 2;
						upper1 = g1.transform.position.z + g1.transform.localScale.z / 2;
						upper2 = g2.transform.position.z + g2.transform.localScale.z / 2;
						if ((CloseEnough (lower1, upper2) && (CloseEnough (lower1, transform.position.z - 0.1f) || CloseEnough (lower1, transform.position.z + 0.1f))) ||
						    (CloseEnough (lower2, upper1) && (CloseEnough (lower2, transform.position.z - 0.1f) || CloseEnough (lower2, transform.position.z + 0.1f)))) {
							pairs.Add (new MyTuple (g1, g2));
						}
					} else if (g1.transform.localScale.z == g2.transform.localScale.z &&
					         g1.transform.localScale.z == 0.2f && g1.transform.position.z == g2.transform.position.z) {

						lower1 = g1.transform.position.x - g1.transform.localScale.x / 2;
						lower2 = g2.transform.position.x - g2.transform.localScale.x / 2;
						upper1 = g1.transform.position.x + g1.transform.localScale.x / 2;
						upper2 = g2.transform.position.x + g2.transform.localScale.x / 2;
						if ((CloseEnough (lower1, upper2) && (CloseEnough (lower1, transform.position.x - 0.1f) || CloseEnough (lower1, transform.position.x + 0.1f))) ||
						    (CloseEnough (lower2, upper1) && (CloseEnough (lower2, transform.position.x - 0.1f) || CloseEnough (lower2, transform.position.x + 0.1f)))) {
							pairs.Add (new MyTuple (g1, g2));
						}
					}
				}
			}

			foreach (MyTuple mt in pairs) {
				if (mt.a.transform.localScale.x == 0.2f) {
					GameObject new_wall = FuseWalls (mt.a, mt.b, 1);
					new_neighbours.Add (new_wall);
					new_neighbours.Remove (mt.a);
					new_neighbours.Remove (mt.b);

					//if the walls that will be fused have twins, they also have to be fused
					if (mt.a.GetComponent<WallBehaviour> ().twin != null &&
					    mt.b.GetComponent<WallBehaviour> ().twin != null) {

						GameObject twin_a = mt.a.GetComponent<WallBehaviour> ().twin;
						GameObject twin_b = mt.b.GetComponent<WallBehaviour> ().twin;

						//the room_neighbours of the twins of the walls
						//about to be fused need to be changed
						List<GameObject> pair_twins_room_neighbours = twin_a.GetComponent<WallBehaviour> ().room_neighbours;
						GameObject new_twin_wall = FuseWalls (twin_a, twin_b, 1);
						pair_twins_room_neighbours.Add (new_twin_wall);
						pair_twins_room_neighbours.Remove (twin_a);
						pair_twins_room_neighbours.Remove (twin_b);
						new_twin_wall.GetComponent<WallBehaviour> ().room_neighbours = pair_twins_room_neighbours;
						new_wall.GetComponent<WallBehaviour> ().twin = new_twin_wall;
						new_twin_wall.GetComponent<WallBehaviour> ().twin = new_wall;
					}
					else
						new_wall.GetComponent<WallBehaviour> ().twin = null;
				}
				else if (mt.a.transform.localScale.z == 0.2f) {
					GameObject new_wall = FuseWalls (mt.a, mt.b, -1);
					new_neighbours.Add (new_wall);
					new_neighbours.Remove (mt.a);
					new_neighbours.Remove (mt.b);

					//if the walls that will be fused have twins, they also have to be fused
					if (mt.a.GetComponent<WallBehaviour> ().twin != null &&
					    mt.b.GetComponent<WallBehaviour> ().twin != null) {

						GameObject twin_a = mt.a.GetComponent<WallBehaviour> ().twin;
						GameObject twin_b = mt.b.GetComponent<WallBehaviour> ().twin;

						//the room_neighbours of the twins of the walls
						//about to be fused need to be changed
						List<GameObject> pair_twins_room_neighbours = twin_a.GetComponent<WallBehaviour> ().room_neighbours;
						GameObject new_twin_wall = FuseWalls (twin_a, twin_b, -1);
						pair_twins_room_neighbours.Add (new_twin_wall);
						pair_twins_room_neighbours.Remove (twin_a);
						pair_twins_room_neighbours.Remove (twin_b);
						new_twin_wall.GetComponent<WallBehaviour> ().room_neighbours = pair_twins_room_neighbours;
						new_wall.GetComponent<WallBehaviour> ().twin = new_twin_wall;
						new_twin_wall.GetComponent<WallBehaviour> ().twin = new_wall;
					}
					else
						new_wall.GetComponent<WallBehaviour> ().twin = null;
				}
			}

			//set the new neighbours for the walls in the twin's room
			if (twin.GetComponent<WallBehaviour> ().room_neighbours != room_neighbours) {
				foreach (GameObject n in twin.GetComponent<WallBehaviour>().room_neighbours) {
					n.GetComponent<WallBehaviour> ().room_neighbours = new_neighbours;
				}
			}

			twin.GetComponent<WallBehaviour> ().twin = null;
			twin.GetComponent<WallBehaviour> ().DeleteSelf ();

			foreach (GameObject n in new_neighbours) {
				n.GetComponent<WallBehaviour> ().room_neighbours = new_neighbours;
				n.GetComponent<WallBehaviour> ().current_mid = 0;
				n.GetComponent<WallBehaviour> ().set_mid = 0;
				n.GetComponent<Renderer> ().material = ms[0];
			}

			//the ceiling and floor also need to be fused
			int i, j;
			bool flag = false;
			List<GameObject> new_ceiling_neighbours = null, new_floor_neighbours = null;
			for (i = 0; i < ceilings.Length - 1 && !flag; i++) {
				for (j = i + 1; j < ceilings.Length && !flag; j++) {
					GameObject g1 = ceilings [i], g2 = ceilings [j];

					//check if the the ceilings were separated by the wall that is going to be removed
					lower1 = g1.transform.position.z - g1.transform.localScale.z / 2;
					lower2 = g2.transform.position.z - g2.transform.localScale.z / 2;
					upper1 = g1.transform.position.z + g1.transform.localScale.z / 2;
					upper2 = g2.transform.position.z + g2.transform.localScale.z / 2;
					if ((CloseEnough (lower1, upper2) && (CloseEnough (lower1, transform.position.z - 0.1f) || CloseEnough (lower1, transform.position.z + 0.1f))) ||
					    (CloseEnough (lower2, upper1) && (CloseEnough (lower2, transform.position.z - 0.1f) || CloseEnough (lower2, transform.position.z + 0.1f)))) {

						lower1 = g1.transform.position.x - g1.transform.localScale.x / 2;
						lower2 = g2.transform.position.x - g2.transform.localScale.x / 2;
						upper1 = g1.transform.position.x + g1.transform.localScale.x / 2;
						upper2 = g2.transform.position.x + g2.transform.localScale.x / 2;

						if (lower1 < transform.position.x && lower2 < transform.position.x &&
						    upper1 > transform.position.x && upper2 > transform.position.x &&
						    lower1 < upper2 && lower2 < upper1) {

							new_ceiling_neighbours = new List<GameObject> ();
							foreach (GameObject n in g1.GetComponent<WallBehaviour>().room_neighbours) {
								new_ceiling_neighbours.Add (n);
							}
							if (g1.GetComponent<WallBehaviour> ().room_neighbours !=
							    g2.GetComponent<WallBehaviour> ().room_neighbours) {
								foreach (GameObject n in g2.GetComponent<WallBehaviour>().room_neighbours) {
									new_ceiling_neighbours.Add (n);
								}
							}

							new_floor_neighbours = new List<GameObject> ();
							foreach (GameObject n in floors[i].GetComponent<WallBehaviour>().room_neighbours) {
								new_floor_neighbours.Add (n);
							}
							if (floors [i].GetComponent<WallBehaviour> ().room_neighbours !=
							    floors [j].GetComponent<WallBehaviour> ().room_neighbours) {
								foreach (GameObject n in floors[j].GetComponent<WallBehaviour>().room_neighbours) {
									new_floor_neighbours.Add (n);
								}
							}

							foreach (GameObject c in new_ceiling_neighbours) {
								c.GetComponent<WallBehaviour> ().room_neighbours = new_ceiling_neighbours;
							}
							foreach (GameObject f in new_floor_neighbours) {
								f.GetComponent<WallBehaviour> ().room_neighbours = new_floor_neighbours;
							}

							flag = true;
						}
					}
					else {
						lower1 = g1.transform.position.x - g1.transform.localScale.x / 2;
						lower2 = g2.transform.position.x - g2.transform.localScale.x / 2;
						upper1 = g1.transform.position.x + g1.transform.localScale.x / 2;
						upper2 = g2.transform.position.x + g2.transform.localScale.x / 2;

						if ((CloseEnough (lower1, upper2) && (CloseEnough (lower1, transform.position.x - 0.1f) || CloseEnough (lower1, transform.position.x + 0.1f))) ||
						    (CloseEnough (lower2, upper1) && (CloseEnough (lower2, transform.position.x - 0.1f) || CloseEnough (lower2, transform.position.x + 0.1f)))) {

							lower1 = g1.transform.position.z - g1.transform.localScale.z / 2;
							lower2 = g2.transform.position.z - g2.transform.localScale.z / 2;
							upper1 = g1.transform.position.z + g1.transform.localScale.z / 2;
							upper2 = g2.transform.position.z + g2.transform.localScale.z / 2;

							if (lower1 < transform.position.z && lower2 < transform.position.z &&
							    upper1 > transform.position.z && upper2 > transform.position.z &&
							    lower1 < upper2 && lower2 < upper1) {

								new_ceiling_neighbours = new List<GameObject> ();
								foreach (GameObject n in g1.GetComponent<WallBehaviour>().room_neighbours) {
									new_ceiling_neighbours.Add (n);
								}
								if (g1.GetComponent<WallBehaviour> ().room_neighbours !=
								    g2.GetComponent<WallBehaviour> ().room_neighbours) {
									foreach (GameObject n in g2.GetComponent<WallBehaviour>().room_neighbours) {
										new_ceiling_neighbours.Add (n);
									}
								}

								new_floor_neighbours = new List<GameObject> ();
								foreach (GameObject n in floors[i].GetComponent<WallBehaviour>().room_neighbours) {
									new_floor_neighbours.Add (n);
								}
								if (floors [i].GetComponent<WallBehaviour> ().room_neighbours !=
								    floors [j].GetComponent<WallBehaviour> ().room_neighbours) {
									foreach (GameObject n in floors[j].GetComponent<WallBehaviour>().room_neighbours) {
										new_floor_neighbours.Add (n);
									}
								}
								/*
								if (lower1 == lower2 && upper1 == upper2) {
									new_ceiling_neighbours.Remove (g1);
									new_ceiling_neighbours.Remove (g2);
									GameObject new_ceiling = FuseWalls (g1, g2, -1);
									new_ceiling.tag = "Ceiling";
									new_ceiling_neighbours.Add (new_ceiling);

									new_floor_neighbours.Remove (floors [i]);
									new_floor_neighbours.Remove (floors [j]);
									GameObject new_floor = FuseWalls (floors [i], floors [j], -1);
									new_floor.tag = "Floor";
									new_floor_neighbours.Add (new_floor);
								}
								*/
								foreach (GameObject c in new_ceiling_neighbours) {
									c.GetComponent<WallBehaviour> ().room_neighbours = new_ceiling_neighbours;
								}
								foreach (GameObject f in new_floor_neighbours) {
									f.GetComponent<WallBehaviour> ().room_neighbours = new_floor_neighbours;
								}



								flag = true;
							}
						}
					}
				}
			}

			//if a new list of ceiling or floor neighbours was formed
			//check whether the objects can be fused
			if (flag)
				CombineFloorsAndCeilings (new_ceiling_neighbours, new_floor_neighbours);

			//search for a "hanging" wall
			//i.e. one where the twins are room neighbours
			flag = false;
			GameObject target = null;
			foreach (GameObject g in new_neighbours) {
				if (new_neighbours.IndexOf (g.GetComponent<WallBehaviour> ().twin) != -1) {
					flag = true;
					target = g;
					break;
				}
			}

			if (flag)
				GameObject.Find ("WallCreator").GetComponent<WallCreation> ().InitiateDelete (target);
		}

		//destroy the wall and his wall_neighbours
		foreach (GameObject n in wall_neighbours) {
			//it is safe to do this because destroy takes effect starting with the next frame
			Destroy (n);
		}
	}

	public IEnumerator DelayedDeleteSelf(){
		//a small delay has to be put for when we call
		//DeleteSelf from another one because Destroy is not
		//immediate, it only takes effect starting with the next frame
		yield return new WaitForEndOfFrame ();
		DeleteSelf ();
	}
		
	public void DisplayOptions(){
		//instantiate panel
		GameObject panel = Instantiate (options_panel);
		GameObject camera = GameObject.Find("Camera Parent");
		panel.transform.SetParent(camera.transform,false);
		panel.transform.localPosition = new Vector3 (0.8f, 0, 2);
		//set number of items in the panel's script
		panel.GetComponent<PanelManagement> ().nr_items = 3;
		//also set its corresponding type
		panel.GetComponent<PanelManagement> ().type = 1;
		//and its title
		panel.transform.GetChild (1).GetComponent<Text> ().text = "Wall options";

		GameObject child_panel = new GameObject ("Child Panel");
		child_panel.AddComponent<CanvasRenderer>();
		child_panel.AddComponent<Image>();
		child_panel.transform.SetParent (panel.transform, false);
		child_panel.transform.localPosition = new Vector3 (0, 0, 0);
		child_panel.GetComponent<CanvasRenderer> ().GetComponent<RectTransform> ().sizeDelta = new Vector2 (500, 800);
		Color c = child_panel.GetComponent<Image> ().color;
		c.a = 1.0f/255;
		child_panel.GetComponent<Image> ().color = c;
		panel.GetComponent<PanelManagement> ().SetChildPanel (child_panel);

		//add buttons
		GameObject go1 = GameObject.Instantiate(button1);
		go1.transform.SetParent (child_panel.transform,false);
		go1.transform.localPosition = new Vector3 (0, 200, 0.2f);
		Button b1 = go1.GetComponentInChildren<Button> ();
		b1.GetComponentInChildren<Text> ().text = "Change Color";
		b1.onClick.AddListener (gameObject.GetComponent<WallBehaviour> ().DisplayChangeMaterialOptions);
		go1.transform.GetChild (0).GetComponent<Image> ().color = Color.red;

		GameObject go2 = GameObject.Instantiate(button1);
		go2.transform.SetParent (child_panel.transform,false);
		go2.transform.localPosition = new Vector3 (0, 0, 0.2f);
		Button b2 = go2.GetComponentInChildren<Button> ();
		if (tag!="Wall" || destructible == false) b2.interactable = false;
		b2.onClick.AddListener(gameObject.GetComponent<WallBehaviour> ().NetworkedDeleteSelf);
		b2.GetComponentInChildren<Text> ().text = "Destroy";

		GameObject go3 = GameObject.Instantiate(button1);
		go3.transform.SetParent (child_panel.transform,false);
		go3.transform.localPosition = new Vector3 (0, -200, 0.2f);
		Button b3 = go3.GetComponentInChildren<Button> ();
		//don't assign function to this button, it doesn't have to do anything
		b3.GetComponentInChildren<Text> ().text = "Cancel";

		//shut off the colliders on all objects so that you
		//cannot trigger anything while using the panel
		DeactivateColliders ("Wall");//----------setwallsstate? -> cam aceeasi chestie ca oricum nu ma pot deplasa cat timp e meniul pornit
		DeactivateColliders ("Floor");
		DeactivateColliders ("Ceiling");
		DeactivateColliders ("Door");

		//lock user movement
		GameObject.Find("Camera Parent").GetComponent<CameraParentMovement>().movement_locked=true;
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
		if (active)
			DisplayOptions ();
	}

	#endregion
}

