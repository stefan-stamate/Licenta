using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPositioning : MonoBehaviour {

	[HideInInspector]
	public string folder;
	[HideInInspector]
	public int type;

	//if a valid position was established for the object
	private bool valid;
	//if the object was successfully instantiated
	//this is necessary because update is still called for a bit
	private bool done;
	//the wall, floor and ceiling to which an object could be
	//attached depending on its type
	private GameObject master_floor,master_wall,master_ceiling;

	void Start () {
		valid = false;
		done = false;
		master_floor = master_wall = master_ceiling = null;
	}

	private IEnumerator AfterCompletion(){
		//set a delay so that the user's button pressing
		//does not trigger a response from a wall or similar object
		yield return new WaitForSeconds (0.1f);
		GameObject.Find ("UtilsHolder").GetComponent<Utils> ().SetWallsState (true);
		GameObject.Find("UIMaster").GetComponent<UIMasterScript>().stop=false;
		transform.GetChild (0).GetComponent<BoxCollider> ().enabled = true;
		transform.GetChild(0).transform.parent = null;
		Destroy (gameObject);
	}

	bool CloseEnough(float a, float b){
		return Mathf.Abs (a - b) < 0.0001f;
	}

	private void Invalidate(){
		transform.localScale = new Vector3 (0, 0, 0);
		transform.GetChild (0).transform.position = new Vector3 (0, 0, 0);
		valid = false;
		master_floor = master_wall = master_ceiling = null;
	}

	private void NetworkedObjectInsertion(int type){

		int id;
		if (type == 1) {
			id = master_floor.GetComponent<WallBehaviour> ().FindOwnId ();
			GameObject o = transform.GetChild (0).gameObject;
			object[] values = {
				1, //floor
				id,
				o.GetComponent<ObjectBehaviour> ().type,
				o.GetComponent<ObjectBehaviour> ().path,
				o.transform.position.x,
				o.transform.position.y,
				o.transform.position.z,
				o.transform.rotation.eulerAngles.y,
			};
			PhotonNetwork.RaiseEvent (5, values, true, null);
		}
		else if (type == 3) {
			id = master_ceiling.GetComponent<WallBehaviour> ().FindOwnId ();
			GameObject o = transform.GetChild (0).gameObject;
			object[] values = {
				3, //ceiling
				id,
				o.GetComponent<ObjectBehaviour> ().type,
				o.GetComponent<ObjectBehaviour> ().path,
				o.transform.position.x,
				o.transform.position.y,
				o.transform.position.z,
				o.transform.rotation.eulerAngles.y,
			};
			PhotonNetwork.RaiseEvent (5, values, true, null);
		}
		else if (type == 2) {
			id = master_wall.GetComponent<WallBehaviour> ().FindOwnId ();
			GameObject o = transform.GetChild (0).gameObject;
			object[] values = {
				2, //wall
				id,
				o.GetComponent<ObjectBehaviour> ().path,
				o.transform.position.x,
				o.transform.position.y,
				o.transform.position.z,
				o.transform.rotation.eulerAngles.y,
			};
			PhotonNetwork.RaiseEvent (5, values, true, null);

			int fid = master_floor.GetComponent<WallBehaviour> ().FindOwnId ();
			object[] fvalues = {
				1, //floor
				fid,
				o.GetComponent<ObjectBehaviour> ().type,
				id,
				master_wall.GetComponent<WallBehaviour>().attached_objs.Count-1,
			};
			PhotonNetwork.RaiseEvent (5, fvalues, true, null);

			int cid = master_ceiling.GetComponent<WallBehaviour> ().FindOwnId ();
			object[] cvalues = {
				1, //floor
				cid,
				o.GetComponent<ObjectBehaviour> ().type,
				id,
				master_wall.GetComponent<WallBehaviour>().attached_objs.Count-1,
			};
			PhotonNetwork.RaiseEvent (5, cvalues, true, null);
		}
	}
	
	void Update () {

		if (done)
			return;

		if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.A) && valid) {
			done = true;
			transform.GetChild (0).tag = "Object";
			if (type == 1) {
				master_floor.GetComponent<WallBehaviour> ().attached_objs.Add (transform.GetChild (0).gameObject);
			}
			else if (type == 2) {
				master_floor.GetComponent<WallBehaviour> ().attached_objs.Add (transform.GetChild (0).gameObject);
				master_wall.GetComponent<WallBehaviour> ().attached_objs.Add (transform.GetChild (0).gameObject);
				master_ceiling.GetComponent<WallBehaviour> ().attached_objs.Add (transform.GetChild (0).gameObject);
			
			}
			else if (type == 3) {
				master_ceiling.GetComponent<WallBehaviour> ().attached_objs.Add (transform.GetChild (0).gameObject);
			}

			if (GameObject.Find ("NetworkingHandler").GetComponent<NetworkUtils> ().connected)
				NetworkedObjectInsertion (type);

			StartCoroutine (AfterCompletion());
			return;
		}

		//---------sa adaug posibilitatea de a roti manual obiectul? (merg numai incremente de 90? -> ar putea merita incercat)
		if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.B)) {
			GameObject.Find ("ObjectInserter").GetComponent<ObjectInsertion> ().DisplayContents (folder);
			GameObject.Find ("UtilsHolder").GetComponent<Utils> ().SetWallsState (true);
			Destroy (gameObject);
			return;
		}
			
		Transform child_transform = transform.GetChild (0).transform;
		RaycastHit hit;
		GameObject camera_parent = GameObject.Find ("Camera Parent");
		//GameObject camera = GameObject.Find ("Main Camera");
		Vector3 dir = new Vector3 (
			Mathf.Sin (camera_parent.transform.localEulerAngles.y * Mathf.PI / 180),
			-Mathf.Sin (camera_parent.transform.localEulerAngles.x * Mathf.PI / 180),
			Mathf.Cos (camera_parent.transform.localEulerAngles.y * Mathf.PI / 180)
		);

		if (!Physics.Raycast (camera_parent.transform.position, dir, out hit)) {
			Invalidate ();
			return;
		}

		bool flag;
		if (type == 1 && (hit.transform.tag == "Wall" || hit.transform.tag == "Floor")) {

			if (hit.transform.tag == "Wall" && (hit.normal.y != 0 ||
			    (hit.normal.x != 0 && hit.transform.localScale.z == 0.2f) ||
			    (hit.normal.z != 0 && hit.transform.localScale.x == 0.2f))) {

				Invalidate ();
				return;
			}

			if (hit.transform.tag == "Wall") {
				if (hit.normal.x == 1)
					child_transform.rotation = Quaternion.Euler (0, 90, 0);
				else if (hit.normal.x == -1)
					child_transform.rotation = Quaternion.Euler (0, 270, 0);
				else if (hit.normal.z == 1)
					child_transform.rotation = Quaternion.Euler (0, 0, 0);
				else // hit.normal.z == -1
					child_transform.rotation = Quaternion.Euler (0, 180, 0);
			}
			//----else set random? -> e cand se uita la floor

			transform.localScale = new Vector3 (1, 1, 1);
			child_transform.localPosition = new Vector3 (hit.point.x, 0, hit.point.z);
			//transform.GetChild (0).transform.localPosition -= transform.GetChild (0).transform.right * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
			child_transform.localPosition += child_transform.up * transform.GetChild (0).GetComponent<BoxCollider> ().size.y / 2;
			child_transform.localPosition -= child_transform.up * transform.GetChild (0).GetComponent<BoxCollider> ().center.y;
			if (hit.transform.tag == "Wall")
				child_transform.localPosition += child_transform.forward * (transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2 + 0.000001f);

			//first we need to find the floor on which the object will sit
			float x_min, x_max, z_min, z_max;
			if (hit.transform.tag == "Wall") {
				GameObject[] floors = GameObject.FindGameObjectsWithTag ("Floor");
				foreach (GameObject floor in floors) {
					x_min = floor.transform.position.x - floor.transform.localScale.x / 2;
					x_max = floor.transform.position.x + floor.transform.localScale.x / 2;
					z_min = floor.transform.position.z - floor.transform.localScale.z / 2;
					z_max = floor.transform.position.z + floor.transform.localScale.z / 2;

					if (child_transform.position.x > x_min &&
					    child_transform.position.x < x_max &&
					    child_transform.position.z > z_min &&
					    child_transform.position.z < z_max) {

						master_floor = floor;
						break;
					}
				}
			}
			else {
				master_floor = hit.transform.gameObject;
			}

			//and after we have to reposition the object so that
			//it doesn't stand on a floor that is not part of the current room
			x_min = child_transform.position.x -
			Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 -
			Mathf.Abs (child_transform.forward.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2;
			x_max = child_transform.position.x +
			Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 +
			Mathf.Abs (child_transform.forward.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2;
			z_min = child_transform.position.z -
			Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 -
			Mathf.Abs (child_transform.forward.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2; 
			z_max = child_transform.position.z +
			Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 +
			Mathf.Abs (child_transform.forward.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2;

			Vector2[] corners = {
				new Vector2 (x_min, z_min),
				new Vector2 (x_min, z_max),
				new Vector2 (x_max, z_min),
				new Vector2 (x_max, z_max)
			};

			//check if every corner of the object has a valid floor underneath
			List<Vector4> add_values = new List<Vector4> ();
			List<GameObject> floor_neighbours = master_floor.GetComponent<WallBehaviour> ().room_neighbours;
			for (int i = 0; i < floor_neighbours.Count; i++) {
				add_values.Add (new Vector4 (0.2f, 0.2f, 0.2f, 0.2f));
			}

			for (int i = 0; i < floor_neighbours.Count - 1; i++) {
				for (int j = i + 1; j < floor_neighbours.Count; j++) {
					if (floor_neighbours [i].transform.position.x - floor_neighbours [i].transform.localScale.x / 2 == //--------closeenough?
					    floor_neighbours [j].transform.position.x + floor_neighbours [j].transform.localScale.x / 2) {
						add_values [i] -= new Vector4 (0.2f, 0, 0, 0);
						add_values [j] -= new Vector4 (0, 0.2f, 0, 0);
					}
					else if (floor_neighbours [i].transform.position.x + floor_neighbours [i].transform.localScale.x / 2 ==
					         floor_neighbours [j].transform.position.x - floor_neighbours [j].transform.localScale.x / 2) {
						add_values [i] -= new Vector4 (0, 0.2f, 0, 0);
						add_values [j] -= new Vector4 (0.2f, 0, 0, 0);
					}
					else if (floor_neighbours [i].transform.position.z - floor_neighbours [i].transform.localScale.z / 2 ==
					         floor_neighbours [j].transform.position.z + floor_neighbours [j].transform.localScale.z / 2) {
						add_values [i] -= new Vector4 (0, 0, 0.2f, 0);
						add_values [j] -= new Vector4 (0, 0, 0, 0.2f);
					}
					else if (floor_neighbours [i].transform.position.z + floor_neighbours [i].transform.localScale.z / 2 ==
					         floor_neighbours [j].transform.position.z - floor_neighbours [j].transform.localScale.z / 2) {
						add_values [i] -= new Vector4 (0, 0, 0, 0.2f);
						add_values [j] -= new Vector4 (0, 0, 0.2f, 0);
					}
				}
			}

			flag = true;
			for (int i = 0; i < 4 && flag; i++) {
				bool little_flag = false;
				for (int j = 0; j < floor_neighbours.Count; j++) {
					if (corners [i] [0] > floor_neighbours [j].transform.position.x -
					    floor_neighbours [j].transform.localScale.x / 2 + add_values [j] [0] &&
					    corners [i] [0] < floor_neighbours [j].transform.position.x +
					    floor_neighbours [j].transform.localScale.x / 2 - add_values [j] [1] &&
					    corners [i] [1] > floor_neighbours [j].transform.position.z -
					    floor_neighbours [j].transform.localScale.z / 2 + add_values [j] [2] &&
					    corners [i] [1] < floor_neighbours [j].transform.position.z +
					    floor_neighbours [j].transform.localScale.z / 2 - add_values [j] [3]) {

						//I found a good floor for current corner
						//register that and stop the search
						little_flag = true;
						break;
					}
				}

				//daca un singur punct nu are un loc bun atunci obiectul
				//nu poate sta in locul curent
				if (!little_flag)
					flag = false;
			}

			if (!flag) {
				if (hit.normal.z != 0 || hit.normal.y != 0) {
					if (x_min < master_floor.transform.position.x - master_floor.transform.localScale.x / 2 + 0.2f) {
						child_transform.position = new Vector3 (
							child_transform.position.x + master_floor.transform.position.x - master_floor.transform.localScale.x / 2 - x_min + 0.2f + 0.00001f,
							child_transform.position.y,
							child_transform.position.z
						);
					}
					else if (x_max > master_floor.transform.position.x + master_floor.transform.localScale.x / 2 - 0.2f) {
						child_transform.position = new Vector3 (
							child_transform.position.x - x_max + master_floor.transform.position.x + master_floor.transform.localScale.x / 2 - 0.2f - 0.00001f,
							child_transform.position.y,
							child_transform.position.z
						);
					}
				}
				if (hit.normal.x != 0 || hit.normal.y != 0) {
					if (z_min < master_floor.transform.position.z - master_floor.transform.localScale.z / 2 + 0.2f) {
						child_transform.position = new Vector3 (
							child_transform.position.x,
							child_transform.position.y,
							child_transform.position.z + master_floor.transform.position.z - master_floor.transform.localScale.z / 2 - z_min + 0.2f + 0.00001f
						);
					}
					else if (z_max > master_floor.transform.position.z + master_floor.transform.localScale.z / 2 - 0.2f) {
						child_transform.position = new Vector3 (
							child_transform.position.x,
							child_transform.position.y,
							child_transform.position.z - z_max + master_floor.transform.position.z + master_floor.transform.localScale.z / 2 - 0.2f - 0.00001f
						);
					}
				}

				//do one more check in case the object cannot fit on the floor
				//no correctionary rotations will be attempted
				x_min = child_transform.position.x -
					Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 -
					Mathf.Abs (child_transform.forward.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2;
				x_max = child_transform.position.x +
					Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 +
					Mathf.Abs (child_transform.forward.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2;
				z_min = child_transform.position.z -
					Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 -
					Mathf.Abs (child_transform.forward.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2; 
				z_max = child_transform.position.z +
					Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 +
					Mathf.Abs (child_transform.forward.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2;

				flag=true;
				if (hit.normal.z != 0 || hit.normal.y != 0) {
					if ((x_min < master_floor.transform.position.x - master_floor.transform.localScale.x / 2 + 0.2f) || 
						(x_max > master_floor.transform.position.x + master_floor.transform.localScale.x / 2 - 0.2f)) {
						flag = false;
					}
				}
				if (hit.normal.x != 0 || hit.normal.y != 0) {
					if ((z_min < master_floor.transform.position.z - master_floor.transform.localScale.z / 2 + 0.2f) ||
						(z_max > master_floor.transform.position.z + master_floor.transform.localScale.z / 2 - 0.2f)) {
						flag = false;
					}
				}

				if (!flag) {
					Invalidate ();
					return;
				}
			}

			child_transform.localPosition -= child_transform.right * transform.GetChild (0).GetComponent<BoxCollider> ().center.x;
			child_transform.localPosition -= child_transform.forward * transform.GetChild (0).GetComponent<BoxCollider> ().center.z;

			//check if the floor doesn't contain an object with which
			//the new one overlaps----------------------> daca se intampla incearca sa redresezi -> foloseste un flag_x si flag_z ca sa retii daca deja a trebuit sa modifici pe vreo axa ca sa incapa
			float y_min, y_max;
			y_min = child_transform.position.y - transform.GetChild (0).GetComponent<BoxCollider> ().size.y / 2;
			y_max = child_transform.position.y + transform.GetChild (0).GetComponent<BoxCollider> ().size.y / 2;

			float lower_x, upper_x, lower_z, upper_z, lower_y, upper_y;
			flag = true;
			foreach (GameObject p in master_floor.GetComponent<WallBehaviour>().room_neighbours) {
				
				if (!flag)
					break;
				
				foreach (GameObject g in p.GetComponent<WallBehaviour>().attached_objs) {
					lower_x = g.transform.position.x -
					Mathf.Abs (g.transform.right.x) * g.GetComponent<BoxCollider> ().size.x / 2 -
					Mathf.Abs (g.transform.forward.x) * g.GetComponent<BoxCollider> ().size.z / 2 +
					g.transform.right.x * g.GetComponent<BoxCollider> ().center.x +
					g.transform.forward.x * g.GetComponent<BoxCollider> ().center.z;
					upper_x = g.transform.position.x +
					Mathf.Abs (g.transform.right.x) * g.GetComponent<BoxCollider> ().size.x / 2 +
					Mathf.Abs (g.transform.forward.x) * g.GetComponent<BoxCollider> ().size.z / 2 +
					g.transform.right.x * g.GetComponent<BoxCollider> ().center.x +
					g.transform.forward.x * g.GetComponent<BoxCollider> ().center.z;
					lower_z = g.transform.position.z -
					Mathf.Abs (g.transform.right.z) * g.GetComponent<BoxCollider> ().size.x / 2 -
					Mathf.Abs (g.transform.forward.z) * g.GetComponent<BoxCollider> ().size.z / 2 +
					g.transform.right.z * g.GetComponent<BoxCollider> ().center.x +
					g.transform.forward.z * g.GetComponent<BoxCollider> ().center.z;
					upper_z = g.transform.position.z +
					Mathf.Abs (g.transform.right.z) * g.GetComponent<BoxCollider> ().size.x / 2 +
					Mathf.Abs (g.transform.forward.z) * g.GetComponent<BoxCollider> ().size.z / 2 +
					g.transform.right.z * g.GetComponent<BoxCollider> ().center.x +
					g.transform.forward.z * g.GetComponent<BoxCollider> ().center.z;
					lower_y = g.transform.position.y - g.GetComponent<BoxCollider> ().size.y / 2;
					upper_y = g.transform.position.y + g.GetComponent<BoxCollider> ().size.y / 2;

					if ((y_min >= lower_y && y_min <= upper_y) || (y_max >= lower_y && y_max <= upper_y) ||
					    (y_min <= lower_y && y_min <= lower_y && y_max >= upper_y && y_max >= upper_y)) {

						if ((((x_min <= upper_x && x_min >= lower_x) || (x_max <= upper_x && x_max >= lower_x)) &&
						    ((z_min <= upper_z && z_min >= lower_z) || (z_max <= upper_z && z_max >= lower_z))) ||

						    (((x_min < lower_x && x_min < upper_x && x_max > lower_x && x_max > upper_x) ||
						    (lower_x < x_min && lower_x < x_max && upper_x > x_min && upper_x > x_max)) &&
						    ((z_min <= upper_z && z_min >= lower_z) || (z_max <= upper_z && z_max >= lower_z))) ||
					
						    (((z_min < lower_z && z_min < upper_z && z_max > lower_z && z_max > upper_z) ||
						    (lower_z < z_min && lower_z < z_max && upper_z > z_min && upper_z > z_max)) &&
						    ((x_min <= upper_x && x_min >= lower_x) || (x_max <= upper_x && x_max >= lower_x)))) {

							flag = false;
							break;
						}
					}
				}
			}

			if (flag) {//-----mai fa inca un check pentru cazul in care e prea mare obiectul ca sa incapa pe floor
				valid = true;
			}
			else {
				Invalidate ();
			}
		}
		else if (type == 2 && hit.transform.tag == "Wall") {
			
			if (hit.normal.y != 0 ||
			    (hit.normal.x != 0 && hit.transform.localScale.z == 0.2f) ||
			    (hit.normal.z != 0 && hit.transform.localScale.x == 0.2f)) {

				Invalidate ();
				return;
			}

			if (hit.normal.x == 1)
				child_transform.rotation = Quaternion.Euler (0, 90, 0);
			else if (hit.normal.x == -1)
				child_transform.rotation = Quaternion.Euler (0, 270, 0);
			else if (hit.normal.z == 1)
				child_transform.rotation = Quaternion.Euler (0, 0, 0);
			else if (hit.normal.z == -1)
				child_transform.rotation = Quaternion.Euler (0, 180, 0);

			transform.localScale = new Vector3 (1, 1, 1);
			child_transform.localPosition = new Vector3 (hit.point.x, hit.point.y, hit.point.z);
			//-------add center.y?
			child_transform.localPosition += child_transform.forward * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2;

			//an object can be attached to multiple walls that have the same orientation
			List<Vector4> valid_walls = new List<Vector4> ();
			foreach (GameObject n in hit.transform.gameObject.GetComponent<WallBehaviour>().room_neighbours) {

				if (hit.normal.x != 0 && n.transform.localScale.x == 0.2f &&
				    CloseEnough (n.transform.position.x, hit.transform.position.x))
					valid_walls.Add (new Vector4 (n.transform.position.z, n.transform.localScale.z / 2,
						n.transform.position.y, n.transform.localScale.y / 2));
				else if (hit.normal.z != 0 && n.transform.localScale.z == 0.2f &&
				         CloseEnough (n.transform.position.z, hit.transform.position.z))
					valid_walls.Add (new Vector4 (n.transform.position.x, n.transform.localScale.x / 2,
						n.transform.position.y, n.transform.localScale.y / 2));
			}
				
			//some of the walls are too long
			//i.e. they go through other adiacent walls because
			//it is not taken into account the fact that walls have a width of 0.4f
			//those walls need to be replaced with shorter versions
			//because that extra portion is not valid to place an object on.
			//these walls are placed at the extremes, so first the maximum and
			//minimum values need to be determined
			//----------also se afla golurile din perete i.e. acolo unde am aux_obj-uri
			float lat_min, lat_max, len_min, len_max;
			List<Vector4> empty_spaces = new List<Vector4> ();
			if (valid_walls.Count == 1) {
				valid_walls [0] -= new Vector4 (0, 1, 0, 0) * 0.2f;
				lat_min = valid_walls [0] [0] - valid_walls [0] [1];
				lat_max = valid_walls [0] [0] + valid_walls [0] [1];
				len_min = valid_walls [0] [2] - valid_walls [0] [3];
				len_max = valid_walls [0] [2] + valid_walls [0] [3];
			}
			else {
				float min_v, max_v;
				min_v = max_v = valid_walls [0] [0];
				int min_pos = 0, max_pos = 0;

				for (int i = 1; i < valid_walls.Count; i++) {
					if (valid_walls [i] [0] < min_v) {
						min_v = valid_walls [i] [0];
						min_pos = i;
					}
					else if (valid_walls [i] [0] > max_v) {
						max_v = valid_walls [i] [0];
						max_pos = i;
					}
				}

				//shrink the extreme walls' length and reposition them so that
				//they have their actual coverage
				valid_walls [min_pos] += new Vector4 (1, -1, 0, 0) * 0.1f;
				valid_walls [max_pos] += new Vector4 (-1, -1, 0, 0) * 0.1f;

				lat_min = valid_walls [min_pos] [0] - valid_walls [min_pos] [1];
				lat_max = valid_walls [max_pos] [0] + valid_walls [max_pos] [1];

				len_min = valid_walls [0] [2] - valid_walls [0] [3];
				len_max = valid_walls [0] [2] + valid_walls [0] [3];
				for (int i = 1; i < valid_walls.Count; i++) {
					if (len_min > valid_walls [i] [2] - valid_walls [i] [3])
						len_min = valid_walls [i] [2] - valid_walls [i] [3];
					else if (len_max < valid_walls [i] [2] + valid_walls [i] [3])
						len_max = valid_walls [i] [2] + valid_walls [i] [3];
				}

				int mem;
				for (int i = 0; i < valid_walls.Count; i++) {
					if (valid_walls [i] [2] + valid_walls [i] [3] == len_max &&
					    valid_walls [i] [2] - valid_walls [i] [3] != len_min) {
						flag = false;
						mem = -1;
						//check if this wall has a counterpart
						//this happens when they are the walls surrounding a window
						for (int j = 0; j < valid_walls.Count && !flag; j++) {
							if (valid_walls [j] [2] - valid_walls [j] [3] == len_min &&
							    valid_walls [j] [0] == valid_walls [i] [0]) {

								flag = true;
								mem = j;
							}
						}

						if (!flag) {//without counterpart
							empty_spaces.Add (new Vector4 (valid_walls [i] [0] - valid_walls [i] [1],	//lat_min
								valid_walls [i] [0] + valid_walls [i] [1],	//lat_max
								len_min,										//len_min
								valid_walls [i] [2] - valid_walls [i] [3]));	//len_max
						}
						else //with counterpart
							empty_spaces.Add (new Vector4 (valid_walls [i] [0] - valid_walls [i] [1],	//lat_min
								valid_walls [i] [0] + valid_walls [i] [1],	//lat_max
								valid_walls [mem] [2] + valid_walls [mem] [3],//len_min
								valid_walls [i] [2] - valid_walls [i] [3]));	//len_max
					}
				}
			}

			//check whether the object is exceeding the valid walls' range
			float obj_lat_min, obj_lat_max, obj_len_min, obj_len_max;
			if (hit.normal.z != 0) {
				obj_lat_min = child_transform.position.x -
				Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
				obj_lat_max = child_transform.position.x +
				Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
			}
			else { // hit.normal.x != 0
				obj_lat_min = child_transform.position.z -
				Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
				obj_lat_max = child_transform.position.z +
				Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
			}
			obj_len_min = child_transform.position.y -
			Mathf.Abs (child_transform.up.y) * transform.GetChild (0).GetComponent<BoxCollider> ().size.y / 2;
			obj_len_max = child_transform.position.y +
			Mathf.Abs (child_transform.up.y) * transform.GetChild (0).GetComponent<BoxCollider> ().size.y / 2;

			bool flag_h, flag_v;
			flag_h = flag_v = false;
			if (obj_lat_min < lat_min) {
				flag_h = true;
				if (hit.normal.z != 0)
					child_transform.position = new Vector3 (
						child_transform.position.x - obj_lat_min + lat_min,
						child_transform.position.y,
						child_transform.position.z
					);
				else //hit.normal.x != 0
					child_transform.position = new Vector3 (
						child_transform.position.x,
						child_transform.position.y,
						child_transform.position.z - obj_lat_min + lat_min
					);
			}
			else if (obj_lat_max > lat_max) {
				flag_h = true;
				if (hit.normal.z != 0)
					child_transform.position = new Vector3 (
						child_transform.position.x + lat_max - obj_lat_max,
						child_transform.position.y,
						child_transform.position.z
					);
				else //hit.normal.x != 0
					child_transform.position = new Vector3 (
						child_transform.position.x,
						child_transform.position.y,
						child_transform.position.z + lat_max - obj_lat_max
					);
			}

			if (obj_len_min < len_min) {
				flag_v = true;
				child_transform.position -= child_transform.up * (obj_len_min - len_min);
			}
			else if (obj_len_max > len_max) {
				flag_v = true;
				child_transform.position += child_transform.up * (len_max - obj_len_max);
			}

			//check if the object overlaps with an empty space
			if (hit.normal.z != 0) {
				obj_lat_min = child_transform.position.x -
				Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
				obj_lat_max = child_transform.position.x +
				Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
			}
			else { // hit.normal.x != 0
				obj_lat_min = child_transform.position.z -
				Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
				obj_lat_max = child_transform.position.z +
				Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
			}
			obj_len_min = child_transform.position.y -
			Mathf.Abs (child_transform.up.y) * transform.GetChild (0).GetComponent<BoxCollider> ().size.y / 2;
			obj_len_max = child_transform.position.y +
			Mathf.Abs (child_transform.up.y) * transform.GetChild (0).GetComponent<BoxCollider> ().size.y / 2;

			//nu se ia in considerare cazul in care gen max-ul
			//obiectului e mai mare si decat min si decat max wall
			float dif;
			for (int i = 0; i < empty_spaces.Count; i++) {
				if (((obj_lat_min < empty_spaces [i] [1] && obj_lat_min > empty_spaces [i] [0]) ||
				    (obj_lat_max < empty_spaces [i] [1] && obj_lat_max > empty_spaces [i] [0])) &&
				    ((obj_len_min < empty_spaces [i] [3] && obj_len_min > empty_spaces [i] [2]) ||
				    (obj_len_max < empty_spaces [i] [3] && obj_len_max > empty_spaces [i] [2]))) {

					//first try to move the object horizontally
					//do this only if it flag_h in not true
					//(that is, if the object was not already repositioned horizontally)
					if (!flag_h) {
						if (obj_lat_min < empty_spaces [i] [1] && obj_lat_min > empty_spaces [i] [0]) {
							dif = empty_spaces [i] [1] - obj_lat_min;
							if (hit.normal.z != 0)
								child_transform.position = new Vector3 (
									child_transform.position.x + empty_spaces [i] [1] - obj_lat_min,
									child_transform.position.y,
									child_transform.position.z
								);
							else //hit.normal.x != 0
								child_transform.position = new Vector3 (
									child_transform.position.x,
									child_transform.position.y,
									child_transform.position.z + empty_spaces [i] [1] - obj_lat_min
								);
						}
						else { // obj_lat_max < empty_spaces [i][1] && obj_lat_max > empty_spaces [i][0]
							dif = empty_spaces [i] [0] - obj_lat_max;
							if (hit.normal.z != 0)
								child_transform.position = new Vector3 (
									child_transform.position.x + empty_spaces [i] [0] - obj_lat_max,
									child_transform.position.y,
									child_transform.position.z
								);
							else //hit.normal.x != 0
								child_transform.position = new Vector3 (
									child_transform.position.x,
									child_transform.position.y,
									child_transform.position.z + empty_spaces [i] [0] - obj_lat_max
								);
						}

						if (hit.normal.z != 0) {
							obj_lat_min = child_transform.position.x -
							Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
							obj_lat_max = child_transform.position.x +
							Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
						}
						else { // hit.normal.x != 0
							obj_lat_min = child_transform.position.z -
							Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
							obj_lat_max = child_transform.position.z +
							Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
						}

						//check if new position is ok and if so go to next step
						//if not, try moving the object vertically
						flag = (obj_lat_min > lat_min) && (obj_lat_max < lat_max);
						for (int j = 0; j < empty_spaces.Count && flag; j++) {
							if (((obj_lat_min < empty_spaces [j] [1] && obj_lat_min > empty_spaces [j] [0]) ||
							    (obj_lat_max < empty_spaces [j] [1] && obj_lat_max > empty_spaces [j] [0]) ||
							    (obj_lat_min < empty_spaces [i] [0] && obj_lat_min < empty_spaces [i] [1] &&
							    obj_lat_max > empty_spaces [i] [0] && obj_lat_max > empty_spaces [i] [1])) &&
							    ((obj_len_min < empty_spaces [j] [3] && obj_len_min > empty_spaces [j] [2]) ||
							    (obj_len_max < empty_spaces [j] [3] && obj_len_max > empty_spaces [j] [2]))) {
								flag = false;
							}
						}

						if (flag) {//if everything is all right go to next step
							break;
						}
						else {//if the object is still on top of an empty space
							//put it back where it was
							if (hit.normal.z != 0)
								child_transform.position = new Vector3 (
									child_transform.position.x - dif,
									child_transform.position.y,
									child_transform.position.z
								);
							else //hit.normal.x != 0
								child_transform.position = new Vector3 (
									child_transform.position.x,
									child_transform.position.y,
									child_transform.position.z - dif
								);

							if (hit.normal.z != 0) {
								obj_lat_min = child_transform.position.x -
								Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
								obj_lat_max = child_transform.position.x +
								Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
							}
							else { // hit.normal.x != 0
								obj_lat_min = child_transform.position.z -
								Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
								obj_lat_max = child_transform.position.z +
								Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
							}
						}
					}
					//then try to move it vertically if you haven't done already
					if (!flag_v) {
						//if the object fits in the space above a door/window
						flag = false;
						if (obj_len_min < empty_spaces [i] [3] && obj_len_min > empty_spaces [i] [2]) {
							if (obj_len_max - obj_len_min < len_max - empty_spaces [i] [3]) {

								child_transform.position += child_transform.up * (empty_spaces [i] [3] - obj_len_min);
								flag = true;
							}
						}
						//or below one
						else { // obj_len_max < empty_spaces [i] [3] && obj_len_max > empty_spaces [i] [2]
							if (obj_len_max - obj_len_min < empty_spaces [i] [2] - len_min) {

								child_transform.position += child_transform.up * (empty_spaces [i] [2] - obj_len_max);
								flag = true;
							}
						}

						if (flag) {
							obj_len_min = child_transform.position.y -
							Mathf.Abs (child_transform.up.y) * transform.GetChild (0).GetComponent<BoxCollider> ().size.y / 2;
							obj_len_max = child_transform.position.y +
							Mathf.Abs (child_transform.up.y) * transform.GetChild (0).GetComponent<BoxCollider> ().size.y / 2;
							break;
						}
					}

					//if the object cannot fit in any way it is invalidated
					Invalidate ();
					return;
				}
				else if (obj_lat_min < empty_spaces [i] [0] && obj_lat_min < empty_spaces [i] [1] &&
				         obj_lat_max > empty_spaces [i] [0] && obj_lat_max > empty_spaces [i] [1] &&
				         ((obj_len_min > empty_spaces [i] [2] && obj_len_min < empty_spaces [i] [3]) ||
				         (obj_len_max > empty_spaces [i] [2] && obj_len_max < empty_spaces [i] [3]))) {

					if (!flag_h) {
						//adjust on the horizontal so that the object doesn't go through an empty space
						float obj_mid, empty_mid;
						obj_mid = (obj_lat_min + obj_lat_max) / 2;
						empty_mid = (empty_spaces [i] [0] + empty_spaces [i] [1]) / 2;
						if (obj_mid < empty_mid) {
							dif = empty_spaces [i] [0] - obj_lat_max;
							if (hit.normal.z != 0)
								child_transform.position = new Vector3 (
									child_transform.position.x + empty_spaces [i] [0] - obj_lat_max,
									child_transform.position.y,
									child_transform.position.z
								);
							else //hit.normal.x != 0
							child_transform.position = new Vector3 (
									child_transform.position.x,
									child_transform.position.y,
									child_transform.position.z + empty_spaces [i] [0] - obj_lat_max
								);
						}
						else {
							dif = empty_spaces [i] [1] - obj_lat_min;
							if (hit.normal.z != 0)
								child_transform.position = new Vector3 (
									child_transform.position.x + empty_spaces [i] [1] - obj_lat_min,
									child_transform.position.y,
									child_transform.position.z
								);
							else //hit.normal.x != 0
							child_transform.position = new Vector3 (
									child_transform.position.x,
									child_transform.position.y,
									child_transform.position.z + empty_spaces [i] [1] - obj_lat_min
								);
						}

						//check if the position is valid
						if (hit.normal.z != 0) {
							obj_lat_min = child_transform.position.x -
							Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
							obj_lat_max = child_transform.position.x +
							Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
						}
						else { // hit.normal.x != 0
							obj_lat_min = child_transform.position.z -
							Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
							obj_lat_max = child_transform.position.z +
							Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
						}

						//check if new position is ok and if so go to next step
						//if not, try moving the object vertically
						flag = (obj_lat_min > lat_min) && (obj_lat_max < lat_max);
						for (int j = 0; j < empty_spaces.Count && flag; j++) {
							if (((obj_lat_min < empty_spaces [j] [1] && obj_lat_min > empty_spaces [j] [0]) ||
							    (obj_lat_max < empty_spaces [j] [1] && obj_lat_max > empty_spaces [j] [0]) ||
							    (obj_lat_min < empty_spaces [i] [0] && obj_lat_min < empty_spaces [i] [1] &&
							    obj_lat_max > empty_spaces [i] [0] && obj_lat_max > empty_spaces [i] [1])) &&
							    ((obj_len_min < empty_spaces [j] [3] && obj_len_min > empty_spaces [j] [2]) ||
							    (obj_len_max < empty_spaces [j] [3] && obj_len_max > empty_spaces [j] [2]))) {
								flag = false;
							}
						}

						//if things are good then move on
						//if not, put the object back where it was
						if (flag)//---------------------------set flag_h to use it when readjusting the object if it overlaps
							break;
						else {
							if (hit.normal.z != 0) {
								child_transform.position = new Vector3 (
									child_transform.position.x - dif,
									child_transform.position.y,
									child_transform.position.z
								);
								obj_lat_min = child_transform.position.x -
								Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
								obj_lat_max = child_transform.position.x +
								Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
							}
							else { //hit.normal.x != 0
								child_transform.position = new Vector3 (
									child_transform.position.x,
									child_transform.position.y,
									child_transform.position.z - dif
								);
								obj_lat_min = child_transform.position.z -
								Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
								obj_lat_max = child_transform.position.z +
								Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2;
							}
						}
					}
					if (!flag_v) {
						//if the object fits in the space above a door/window
						flag = false;
						if (obj_len_min < empty_spaces [i] [3] && obj_len_min > empty_spaces [i] [2]) {
							if (obj_len_max - obj_len_min < len_max - empty_spaces [i] [3]) {

								child_transform.position += child_transform.up * (empty_spaces [i] [3] - obj_len_min);
								flag = true;
							}
						}
						//or below one
						else { // obj_len_max < empty_spaces [i] [3] && obj_len_max > empty_spaces [i] [2]
							if (obj_len_max - obj_len_min < empty_spaces [i] [2] - len_min) {
								
								child_transform.position += child_transform.up * (empty_spaces [i] [2] - obj_len_max);
								flag = true;
							}
						}
							
						if (flag) {
							obj_len_min = child_transform.position.y -
							Mathf.Abs (child_transform.up.y) * transform.GetChild (0).GetComponent<BoxCollider> ().size.y / 2;
							obj_len_max = child_transform.position.y +
							Mathf.Abs (child_transform.up.y) * transform.GetChild (0).GetComponent<BoxCollider> ().size.y / 2;
							break;
						}
					}

					Invalidate ();
					return;
				}
			}


			//check if the object overlaps with any other object in the room
			float x_min, x_max, z_min, z_max;
			master_wall = hit.transform.gameObject;

			GameObject[] floors = GameObject.FindGameObjectsWithTag ("Floor");
			foreach (GameObject floor in floors) {
				x_min = floor.transform.position.x - floor.transform.localScale.x / 2;
				x_max = floor.transform.position.x + floor.transform.localScale.x / 2;
				z_min = floor.transform.position.z - floor.transform.localScale.z / 2;
				z_max = floor.transform.position.z + floor.transform.localScale.z / 2;

				if (child_transform.position.x > x_min &&
				    child_transform.position.x < x_max &&
				    child_transform.position.z > z_min &&
				    child_transform.position.z < z_max) {

					master_floor = floor;
					break;
				}
			}

			GameObject[] ceilings = GameObject.FindGameObjectsWithTag ("Ceiling");
			foreach (GameObject ceiling in ceilings) {
				x_min = ceiling.transform.position.x - ceiling.transform.localScale.x / 2;
				x_max = ceiling.transform.position.x + ceiling.transform.localScale.x / 2;
				z_min = ceiling.transform.position.z - ceiling.transform.localScale.z / 2;
				z_max = ceiling.transform.position.z + ceiling.transform.localScale.z / 2;

				if (child_transform.position.x > x_min &&
				    child_transform.position.x < x_max &&
				    child_transform.position.z > z_min &&
				    child_transform.position.z < z_max) {

					master_ceiling = ceiling;
					break;
				}
			}

			List<GameObject> object_neighbours = new List<GameObject> ();
			foreach (GameObject fp in master_floor.GetComponent<WallBehaviour>().room_neighbours) {
				foreach (GameObject fc in fp.GetComponent<WallBehaviour>().attached_objs) {
					object_neighbours.Add (fc);
				}
			}
			foreach (GameObject cp in master_ceiling.GetComponent<WallBehaviour>().room_neighbours) {
				foreach (GameObject cc in cp.GetComponent<WallBehaviour>().attached_objs) {
					//object attached to walls appear in the attached_objs list of
					//both their master_floor and master_ceiling
					if (object_neighbours.IndexOf (cc) == -1)
						object_neighbours.Add (cc);
				}
			}

			x_min = child_transform.position.x -
			Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 -
			Mathf.Abs (child_transform.forward.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2;
			x_max = child_transform.position.x +
			Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 +
			Mathf.Abs (child_transform.forward.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2;
			z_min = child_transform.position.z -
			Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 -
			Mathf.Abs (child_transform.forward.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2; 
			z_max = child_transform.position.z +
			Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 +
			Mathf.Abs (child_transform.forward.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2;
			float y_min, y_max;
			y_min = child_transform.position.y - transform.GetChild (0).GetComponent<BoxCollider> ().size.y / 2;
			y_max = child_transform.position.y + transform.GetChild (0).GetComponent<BoxCollider> ().size.y / 2;

			float lower_x, upper_x, lower_z, upper_z, lower_y, upper_y;
			flag = true;
			foreach (GameObject g in object_neighbours) {
				lower_x = g.transform.position.x -
				Mathf.Abs (g.transform.right.x) * g.GetComponent<BoxCollider> ().size.x / 2 -
				Mathf.Abs (g.transform.forward.x) * g.GetComponent<BoxCollider> ().size.z / 2 +
				g.transform.right.x * g.GetComponent<BoxCollider> ().center.x +
				g.transform.forward.x * g.GetComponent<BoxCollider> ().center.z;
				upper_x = g.transform.position.x +
				Mathf.Abs (g.transform.right.x) * g.GetComponent<BoxCollider> ().size.x / 2 +
				Mathf.Abs (g.transform.forward.x) * g.GetComponent<BoxCollider> ().size.z / 2 +
				g.transform.right.x * g.GetComponent<BoxCollider> ().center.x +
				g.transform.forward.x * g.GetComponent<BoxCollider> ().center.z;
				lower_z = g.transform.position.z -
				Mathf.Abs (g.transform.right.z) * g.GetComponent<BoxCollider> ().size.x / 2 -
				Mathf.Abs (g.transform.forward.z) * g.GetComponent<BoxCollider> ().size.z / 2 +
				g.transform.right.z * g.GetComponent<BoxCollider> ().center.x +
				g.transform.forward.z * g.GetComponent<BoxCollider> ().center.z;
				upper_z = g.transform.position.z +
				Mathf.Abs (g.transform.right.z) * g.GetComponent<BoxCollider> ().size.x / 2 +
				Mathf.Abs (g.transform.forward.z) * g.GetComponent<BoxCollider> ().size.z / 2 +
				g.transform.right.z * g.GetComponent<BoxCollider> ().center.x +
				g.transform.forward.z * g.GetComponent<BoxCollider> ().center.z;
				lower_y = g.transform.position.y - g.GetComponent<BoxCollider> ().size.y / 2;
				upper_y = g.transform.position.y + g.GetComponent<BoxCollider> ().size.y / 2;

				if ((y_min >= lower_y && y_min <= upper_y) || (y_max >= lower_y && y_max <= upper_y) ||
				    (y_min <= lower_y && y_min <= lower_y && y_max >= upper_y && y_max >= upper_y)) {

					if ((((x_min <= upper_x && x_min >= lower_x) || (x_max <= upper_x && x_max >= lower_x)) &&
					    ((z_min <= upper_z && z_min >= lower_z) || (z_max <= upper_z && z_max >= lower_z))) ||

					    (((x_min < lower_x && x_min < upper_x && x_max > lower_x && x_max > upper_x) ||
					    (lower_x < x_min && lower_x < x_max && upper_x > x_min && upper_x > x_max)) &&
					    ((z_min <= upper_z && z_min >= lower_z) || (z_max <= upper_z && z_max >= lower_z))) ||

					    (((z_min < lower_z && z_min < upper_z && z_max > lower_z && z_max > upper_z) ||
					    (lower_z < z_min && lower_z < z_max && upper_z > z_min && upper_z > z_max)) &&
					    ((x_min <= upper_x && x_min >= lower_x) || (x_max <= upper_x && x_max >= lower_x)))) {

						flag = false;
						break;
					}
				}
			}

			if (flag) {
				valid = true;
				child_transform.localPosition -= child_transform.right * transform.GetChild (0).GetComponent<BoxCollider> ().center.x;
				child_transform.localPosition -= child_transform.up * transform.GetChild (0).GetComponent<BoxCollider> ().center.y;
				child_transform.localPosition -= child_transform.forward * transform.GetChild (0).GetComponent<BoxCollider> ().center.z;
			}
			else {
				//----try to reposition it to eliminate the conflict
				Invalidate ();
			}

			//daca face overlap cu orice invalideaza-l
		}
		else if (type == 3 && hit.transform.tag == "Ceiling") {

			transform.localScale = new Vector3 (1, 1, 1);
			child_transform.localPosition = new Vector3 (hit.point.x, hit.point.y, hit.point.z);
			child_transform.localPosition -= child_transform.up * transform.GetChild (0).GetComponent<BoxCollider> ().size.y / 2;
			child_transform.localPosition -= child_transform.up * transform.GetChild (0).GetComponent<BoxCollider> ().center.y;

			//assign the object's master_ceiling
			master_ceiling = hit.transform.gameObject;

			//and make sure it doesn't go outside the boundaries defined by it
			float x_min, x_max, z_min, z_max;
			x_min = child_transform.position.x -
			Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 -
			Mathf.Abs (child_transform.forward.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2;
			x_max = child_transform.position.x +
			Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 +
			Mathf.Abs (child_transform.forward.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2;
			z_min = child_transform.position.z -
			Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 -
			Mathf.Abs (child_transform.forward.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2; 
			z_max = child_transform.position.z +
			Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 +
			Mathf.Abs (child_transform.forward.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2;

			Vector2[] corners = {
				new Vector2 (x_min, z_min),
				new Vector2 (x_min, z_max),
				new Vector2 (x_max, z_min),
				new Vector2 (x_max, z_max)
			};

			//check if every corner of the object has a valid ceiling above
			List<Vector4> add_values = new List<Vector4> ();
			List<GameObject> ceiling_neighbours = master_ceiling.GetComponent<WallBehaviour> ().room_neighbours;
			for (int i = 0; i < ceiling_neighbours.Count; i++) {
				add_values.Add (new Vector4 (0.2f, 0.2f, 0.2f, 0.2f));
			}

			for (int i = 0; i < ceiling_neighbours.Count - 1; i++) {
				for (int j = i + 1; j < ceiling_neighbours.Count; j++) {
					if (ceiling_neighbours [i].transform.position.x - ceiling_neighbours [i].transform.localScale.x / 2 == //--------closeenough?
					    ceiling_neighbours [j].transform.position.x + ceiling_neighbours [j].transform.localScale.x / 2) {
						add_values [i] -= new Vector4 (0.2f, 0, 0, 0);
						add_values [j] -= new Vector4 (0, 0.2f, 0, 0);
					} else if (ceiling_neighbours [i].transform.position.x + ceiling_neighbours [i].transform.localScale.x / 2 ==
					         ceiling_neighbours [j].transform.position.x - ceiling_neighbours [j].transform.localScale.x / 2) {
						add_values [i] -= new Vector4 (0, 0.2f, 0, 0);
						add_values [j] -= new Vector4 (0.2f, 0, 0, 0);
					} else if (ceiling_neighbours [i].transform.position.z - ceiling_neighbours [i].transform.localScale.z / 2 ==
					         ceiling_neighbours [j].transform.position.z + ceiling_neighbours [j].transform.localScale.z / 2) {
						add_values [i] -= new Vector4 (0, 0, 0.2f, 0);
						add_values [j] -= new Vector4 (0, 0, 0, 0.2f);
					} else if (ceiling_neighbours [i].transform.position.z + ceiling_neighbours [i].transform.localScale.z / 2 ==
					         ceiling_neighbours [j].transform.position.z - ceiling_neighbours [j].transform.localScale.z / 2) {
						add_values [i] -= new Vector4 (0, 0, 0, 0.2f);
						add_values [j] -= new Vector4 (0, 0, 0.2f, 0);
					}
				}
			}

			flag = true;
			for (int i = 0; i < 4 && flag; i++) {
				bool little_flag = false;
				for (int j = 0; j < ceiling_neighbours.Count; j++) {
					if (corners [i] [0] > ceiling_neighbours [j].transform.position.x -
					    ceiling_neighbours [j].transform.localScale.x / 2 + add_values [j] [0] &&
					    corners [i] [0] < ceiling_neighbours [j].transform.position.x +
					    ceiling_neighbours [j].transform.localScale.x / 2 - add_values [j] [1] &&
					    corners [i] [1] > ceiling_neighbours [j].transform.position.z -
					    ceiling_neighbours [j].transform.localScale.z / 2 + add_values [j] [2] &&
					    corners [i] [1] < ceiling_neighbours [j].transform.position.z +
					    ceiling_neighbours [j].transform.localScale.z / 2 - add_values [j] [3]) {

						//I found a good ceiling for current corner
						//register that and stop the search
						little_flag = true;
						break;
					}
				}

				//daca un singur punct nu are un loc bun atunci obiectul
				//nu poate sta in locul curent
				if (!little_flag)
					flag = false;
			}

			if (!flag) {
				if (hit.normal.z != 0 || hit.normal.y != 0) {
					if (x_min < master_ceiling.transform.position.x - master_ceiling.transform.localScale.x / 2 + 0.2f) {
						child_transform.position = new Vector3 (
							child_transform.position.x + master_ceiling.transform.position.x - master_ceiling.transform.localScale.x / 2 - x_min + 0.2f + 0.00001f,
							child_transform.position.y,
							child_transform.position.z
						);
					}
					else if (x_max > master_ceiling.transform.position.x + master_ceiling.transform.localScale.x / 2 - 0.2f) {
						child_transform.position = new Vector3 (
							child_transform.position.x - x_max + master_ceiling.transform.position.x + master_ceiling.transform.localScale.x / 2 - 0.2f - 0.00001f,
							child_transform.position.y,
							child_transform.position.z
						);
					}
				}
				if (hit.normal.x != 0 || hit.normal.y != 0) {
					if (z_min < master_ceiling.transform.position.z - master_ceiling.transform.localScale.z / 2 + 0.2f) {
						child_transform.position = new Vector3 (
							child_transform.position.x,
							child_transform.position.y,
							child_transform.position.z + master_ceiling.transform.position.z - master_ceiling.transform.localScale.z / 2 - z_min + 0.2f + 0.00001f
						);
					}
					else if (z_max > master_ceiling.transform.position.z + master_ceiling.transform.localScale.z / 2 - 0.2f) {
						child_transform.position = new Vector3 (
							child_transform.position.x,
							child_transform.position.y,
							child_transform.position.z - z_max + master_ceiling.transform.position.z + master_ceiling.transform.localScale.z / 2 - 0.2f - 0.00001f
						);
					}
				}

				//do one more check in case the object cannot fit on the ceiling
				//no correctionary rotations will be attempted
				x_min = child_transform.position.x -
					Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 -
					Mathf.Abs (child_transform.forward.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2;
				x_max = child_transform.position.x +
					Mathf.Abs (child_transform.right.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 +
					Mathf.Abs (child_transform.forward.x) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2;
				z_min = child_transform.position.z -
					Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 -
					Mathf.Abs (child_transform.forward.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2; 
				z_max = child_transform.position.z +
					Mathf.Abs (child_transform.right.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.x / 2 +
					Mathf.Abs (child_transform.forward.z) * transform.GetChild (0).GetComponent<BoxCollider> ().size.z / 2;

				flag=true;
				if (hit.normal.z != 0 || hit.normal.y != 0) {
					if ((x_min < master_ceiling.transform.position.x - master_ceiling.transform.localScale.x / 2 + 0.2f) || 
						(x_max > master_ceiling.transform.position.x + master_ceiling.transform.localScale.x / 2 - 0.2f)) {
						flag = false;
					}
				}
				if (hit.normal.x != 0 || hit.normal.y != 0) {
					if ((z_min < master_ceiling.transform.position.z - master_ceiling.transform.localScale.z / 2 + 0.2f) ||
						(z_max > master_ceiling.transform.position.z + master_ceiling.transform.localScale.z / 2 - 0.2f)) {
						flag = false;
					}
				}

				if (!flag) {
					Invalidate ();
					return;
				}
			}

			child_transform.localPosition -= child_transform.right * transform.GetChild (0).GetComponent<BoxCollider> ().center.x;
			child_transform.localPosition -= child_transform.forward * transform.GetChild (0).GetComponent<BoxCollider> ().center.z;

			//check if the floor doesn't contain an object with which
			//the new one overlaps----------------------> daca se intampla incearca sa redresezi -> foloseste un flag_x si flag_z ca sa retii daca deja a trebuit sa modifici pe vreo axa ca sa incapa
			float y_min, y_max;
			y_min = child_transform.position.y - transform.GetChild (0).GetComponent<BoxCollider> ().size.y / 2;
			y_max = child_transform.position.y + transform.GetChild (0).GetComponent<BoxCollider> ().size.y / 2;

			float lower_x, upper_x, lower_z, upper_z,lower_y,upper_y;
			flag = true;
			foreach (GameObject p in master_ceiling.GetComponent<WallBehaviour>().room_neighbours) {

				if (!flag)
					break;

				foreach (GameObject g in p.GetComponent<WallBehaviour>().attached_objs) {
					lower_x = g.transform.position.x -
						Mathf.Abs (g.transform.right.x) * g.GetComponent<BoxCollider> ().size.x / 2 -
						Mathf.Abs (g.transform.forward.x) * g.GetComponent<BoxCollider> ().size.z / 2 +
						g.transform.right.x * g.GetComponent<BoxCollider> ().center.x +
						g.transform.forward.x * g.GetComponent<BoxCollider> ().center.z;
					upper_x = g.transform.position.x +
						Mathf.Abs (g.transform.right.x) * g.GetComponent<BoxCollider> ().size.x / 2 +
						Mathf.Abs (g.transform.forward.x) * g.GetComponent<BoxCollider> ().size.z / 2 +
						g.transform.right.x * g.GetComponent<BoxCollider> ().center.x +
						g.transform.forward.x * g.GetComponent<BoxCollider> ().center.z;
					lower_z = g.transform.position.z -
						Mathf.Abs (g.transform.right.z) * g.GetComponent<BoxCollider> ().size.x / 2 -
						Mathf.Abs (g.transform.forward.z) * g.GetComponent<BoxCollider> ().size.z / 2 +
						g.transform.right.z * g.GetComponent<BoxCollider> ().center.x +
						g.transform.forward.z * g.GetComponent<BoxCollider> ().center.z;
					upper_z = g.transform.position.z +
						Mathf.Abs (g.transform.right.z) * g.GetComponent<BoxCollider> ().size.x / 2 +
						Mathf.Abs (g.transform.forward.z) * g.GetComponent<BoxCollider> ().size.z / 2 +
						g.transform.right.z * g.GetComponent<BoxCollider> ().center.x +
						g.transform.forward.z * g.GetComponent<BoxCollider> ().center.z;
					lower_y = g.transform.position.y - g.GetComponent<BoxCollider> ().size.y / 2;
					upper_y = g.transform.position.y + g.GetComponent<BoxCollider> ().size.y / 2;

					if ((y_min >= lower_y && y_min <= upper_y) || (y_max >= lower_y && y_max <= upper_y) ||
						(y_min <= lower_y && y_min <= lower_y && y_max >= upper_y && y_max >= upper_y)) {

						if ((((x_min <= upper_x && x_min >= lower_x) || (x_max <= upper_x && x_max >= lower_x)) &&
							((z_min <= upper_z && z_min >= lower_z) || (z_max <= upper_z && z_max >= lower_z))) ||

							(((x_min < lower_x && x_min < upper_x && x_max > lower_x && x_max > upper_x) ||
								(lower_x < x_min && lower_x < x_max && upper_x > x_min && upper_x > x_max)) &&
								((z_min <= upper_z && z_min >= lower_z) || (z_max <= upper_z && z_max >= lower_z))) ||

							(((z_min < lower_z && z_min < upper_z && z_max > lower_z && z_max > upper_z) ||
								(lower_z < z_min && lower_z < z_max && upper_z > z_min && upper_z > z_max)) &&
								((x_min <= upper_x && x_min >= lower_x) || (x_max <= upper_x && x_max >= lower_x)))) {

							flag = false;
							break;
						}
					}
				}
			}

			if (flag) {
				valid = true;
			}
			else {
				Invalidate ();
			}
		}
		else {
			Invalidate ();
		}
	}
}
