using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DandWDestroy : MonoBehaviour {

	private bool active,found;
	private float height;
	private RaycastHit hit;
	private GameObject wall_prefab;

	// Use this for initialization
	void Awake () {
		active = false;
		height = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().height;
		wall_prefab = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().wall_prefab;
	}

	void DoRaycast(){
		GameObject camera = GameObject.Find ("Camera Parent");
		//GameObject camera = GameObject.Find ("Main Camera");

		Vector3 dir = new Vector3 (Mathf.Sin (camera.transform.localEulerAngles.y * Mathf.PI / 180), 0, Mathf.Cos (camera.transform.localEulerAngles.y * Mathf.PI / 180));
		found = false;
		if (Physics.Raycast (camera.transform.position, dir, out hit)) {
			if (hit.transform.tag == "Door" || hit.transform.tag == "Window")
				found = true;
		}
	}

	void NetworkedDWDestroy(GameObject o){
		
		string path = o.GetComponent<AuxObjectParent>().path;
		//object[] input;
		GameObject[] objs;
		int type;

		if (path.Contains ("Doors")) {
			objs = GameObject.FindGameObjectsWithTag ("Door");
			type = 1;
		}
		else { //contains("Windows")
			objs = GameObject.FindGameObjectsWithTag ("Window");
			type = 2;
		}

		int object_id = System.Array.IndexOf (objs, o);

		int[] input = {
			type,
			object_id,
		};

		DWDestroy (input);
		if (GameObject.Find ("NetworkingHandler").GetComponent<NetworkUtils> ().connected)
			PhotonNetwork.RaiseEvent (12, input, true, null);

		GameObject.Find("UIMaster").GetComponent<UIMasterScript>().stop=false;
		GameObject.Find ("UtilsHolder").GetComponent<Utils> ().SetWallsState (true);
		GameObject.Find ("UtilsHolder").GetComponent<Utils> ().SetDoorsState (true);
	}

	public void DWDestroy(int[] input){
		
		int type = input [0];
		GameObject[] objs;
		if (type == 1)
			objs = GameObject.FindGameObjectsWithTag ("Door");
		else //type == 2
			objs = GameObject.FindGameObjectsWithTag ("Window");

		GameObject target = objs [input [1]];

		GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");
		GameObject parent_wall=null;
		foreach (GameObject w in walls) {
			if (w.GetComponent<WallBehaviour> ().aux_objs.IndexOf (target) != -1) {
				parent_wall = w;
				break;
			}
		}

		float lat_min, lat_max;
		GameObject new_wall, new_wall_twin;
		new_wall = Instantiate (wall_prefab);
		new_wall_twin = Instantiate (wall_prefab);
		List<GameObject> aux_list;
		if (type == 1) {
			//if a door is deleted, then the 3 closest walls to it
			//have to be combined into a single one to cover the gap
			GameObject w1, w2, w3;
			GameObject w1t, w2t, w3t;
			//min_val reprezinta pozitia celui mai apropiat perete
			//care are coordonata x sau z mai mare decat a obietului
			//(are valoarea cea mai mica)
			//analog, max_val reprezinta pozitia celui mai apropiat perete
			//care se afla inaintea obiectului
			//(are valoarea cea mai mare)
			//iar min_dist reprezinta distanta pana la cel mai apropiat upper wall
			float min_val, max_val, min_dist;
			w1 = w2 = w3 = null;
			min_val = max_val = min_dist = 0;

			if (parent_wall.transform.localScale.x == 0.2f) {
				foreach (GameObject w in parent_wall.GetComponent<WallBehaviour>().wall_neighbours) {
					if (w.transform.position.y == height / 2) { //normal wall
						if (w.transform.position.z < target.transform.position.z) {
							if (w1 == null || (max_val < w.transform.position.z)) {
								w1 = w;
								max_val = w.transform.position.z;
							}
						} else if (w.transform.position.z > target.transform.position.z) {
							if (w2 == null || (min_val > w.transform.position.z)) {
								w2 = w;
								min_val = w.transform.position.z;
							}
						}
					}
					else { //upper wall
						if (w3 == null || (Mathf.Abs (w.transform.position.z - target.transform.position.z) < min_dist)) {
							w3 = w;
							min_dist = Mathf.Abs (w.transform.position.z - target.transform.position.z);
						}
					}
				}

				lat_min = w1.transform.position.z - w1.transform.localScale.z / 2;
				lat_max = w2.transform.position.z + w2.transform.localScale.z / 2;

				new_wall.transform.position = new Vector3 (
					w1.transform.position.x,
					w1.transform.position.y,
					(lat_min + lat_max) / 2
				);
				new_wall.transform.localScale = new Vector3 (
					w1.transform.localScale.x,
					w1.transform.localScale.y,
					lat_max - lat_min
				);

				new_wall_twin.transform.position = new Vector3 (
					w1.GetComponent<WallBehaviour>().twin.transform.position.x,
					w1.transform.position.y,
					(lat_min + lat_max) / 2
				);
				new_wall_twin.transform.localScale = new Vector3 (
					w1.transform.localScale.x,
					w1.transform.localScale.y,
					lat_max - lat_min
				);
			}
			else { //parent_wall.transform.localScale.z == 0.2f
				foreach (GameObject w in parent_wall.GetComponent<WallBehaviour>().wall_neighbours) {
					if (w.transform.position.y == height / 2) { //normal wall
						if (w.transform.position.x < target.transform.position.x) {
							if (w1 == null || (max_val < w.transform.position.x)) {
								w1 = w;
								max_val = w.transform.position.x;
							}
						} else if (w.transform.position.x > target.transform.position.x) {
							if (w2 == null || (min_val > w.transform.position.x)) {
								w2 = w;
								min_val = w.transform.position.x;
							}
						}
					} else { //upper wall
						if (w3 == null || (Mathf.Abs (w.transform.position.x - target.transform.position.x) < min_dist)) {
							w3 = w;
							min_dist = Mathf.Abs (w.transform.position.x - target.transform.position.x);
						}
					}
				}

				lat_min = w1.transform.position.x - w1.transform.localScale.x / 2;
				lat_max = w2.transform.position.x + w2.transform.localScale.x / 2;

				new_wall.transform.position = new Vector3 (
					(lat_min + lat_max) / 2,
					w1.transform.position.y,
					w1.transform.position.z
				);
				new_wall.transform.localScale = new Vector3 (
					lat_max - lat_min,
					w1.transform.localScale.y,
					w1.transform.localScale.z
				);

				new_wall_twin.transform.position = new Vector3 (
					(lat_min + lat_max) / 2,
					w1.transform.position.y,
					w1.GetComponent<WallBehaviour>().twin.transform.position.z
				);
				new_wall_twin.transform.localScale = new Vector3 (
					lat_max - lat_min,
					w1.transform.localScale.y,
					w1.transform.localScale.z
				);
			}

			//compute room_neighbours + wall_neighbours + aux_objs + attached_objs
			//for the new wall
			aux_list = w1.GetComponent<WallBehaviour> ().room_neighbours;
			aux_list.Remove (w1);
			aux_list.Remove (w2);
			aux_list.Remove (w3);
			aux_list.Add (new_wall);

			foreach (GameObject w in aux_list)
				w.GetComponent<WallBehaviour> ().room_neighbours = aux_list;

			aux_list = w1.GetComponent<WallBehaviour> ().wall_neighbours;
			aux_list.Remove (w1);
			aux_list.Remove (w2);
			aux_list.Remove (w3);
			aux_list.Add (new_wall);

			foreach (GameObject w in aux_list)
				w.GetComponent<WallBehaviour> ().wall_neighbours = aux_list;
				
			aux_list = w1.GetComponent<WallBehaviour> ().aux_objs;
			aux_list.Remove (target);
			new_wall.GetComponent<WallBehaviour> ().aux_objs = aux_list;

			aux_list = new List<GameObject> ();
			foreach (GameObject o in w1.GetComponent<WallBehaviour>().attached_objs)
				aux_list.Add (o);
			foreach (GameObject o in w2.GetComponent<WallBehaviour>().attached_objs)
				aux_list.Add (o);
			foreach (GameObject o in w3.GetComponent<WallBehaviour>().attached_objs)
				aux_list.Add (o);

			new_wall.GetComponent<WallBehaviour> ().attached_objs = aux_list;

			//same for the walls' twins
			w1t = w1.GetComponent<WallBehaviour> ().twin;
			w2t = w2.GetComponent<WallBehaviour> ().twin;
			w3t = w3.GetComponent<WallBehaviour> ().twin;

			aux_list = w1t.GetComponent<WallBehaviour> ().room_neighbours;
			aux_list.Remove (w1t);
			aux_list.Remove (w2t);
			aux_list.Remove (w3t);
			aux_list.Add (new_wall_twin);

			foreach (GameObject w in aux_list)
				w.GetComponent<WallBehaviour> ().room_neighbours = aux_list;

			aux_list = w1t.GetComponent<WallBehaviour> ().wall_neighbours;
			aux_list.Remove (w1t);
			aux_list.Remove (w2t);
			aux_list.Remove (w3t);
			aux_list.Add (new_wall_twin);

			foreach (GameObject w in aux_list)
				w.GetComponent<WallBehaviour> ().wall_neighbours = aux_list;

			aux_list = w1t.GetComponent<WallBehaviour> ().aux_objs;
			aux_list.Remove (target);
			new_wall_twin.GetComponent<WallBehaviour> ().aux_objs = aux_list;

			aux_list = new List<GameObject> ();
			foreach (GameObject o in w1t.GetComponent<WallBehaviour>().attached_objs)
				aux_list.Add (o);
			foreach (GameObject o in w2t.GetComponent<WallBehaviour>().attached_objs)
				aux_list.Add (o);
			foreach (GameObject o in w3t.GetComponent<WallBehaviour>().attached_objs)
				aux_list.Add (o);

			new_wall_twin.GetComponent<WallBehaviour> ().attached_objs = aux_list;

			//set the twin relationship between the 2 new walls
			new_wall.GetComponent<WallBehaviour> ().twin = new_wall_twin;
			new_wall_twin.GetComponent<WallBehaviour> ().twin = new_wall;

			Destroy (w1);
			Destroy (w2);
			Destroy (w3);
			Destroy (w1t);
			Destroy (w2t);
			Destroy (w3t);
		}
		else { //type == 2
			//if a window is deleted, then the 4 closest walls to it
			//have to be combined into a single one to cover the gap
			GameObject w1, w2, w3, w4;
			GameObject w1t, w2t, w3t, w4t;
			//min_val reprezinta pozitia celui mai apropiat perete
			//care are coordonata x sau z mai mare decat a obietului
			//(are valoarea cea mai mica)
			//analog, max_val reprezinta pozitia celui mai apropiat perete
			//care se afla inaintea obiectului
			//(are valoarea cea mai mare)
			//iar min_dist low si high reprezinta distanta pana la cel mai
			//apropiat wall low si respectiv upper wall
			float min_val, max_val, min_dist_low,min_dist_high;
			w1 = w2 = w3 = w4 = null;
			min_val = max_val = min_dist_low = min_dist_high  = 0;

			float min_dist;
			if (parent_wall.transform.localScale.x == 0.2f) {
				foreach (GameObject w in parent_wall.GetComponent<WallBehaviour>().wall_neighbours) {
					if (w.transform.position.y == height / 2) { //normal wall
						if (w.transform.position.z < target.transform.position.z) {
							if (w1 == null || (max_val < w.transform.position.z)) {
								w1 = w;
								max_val = w.transform.position.z;
							}
						}
						else if (w.transform.position.z > target.transform.position.z) {
							if (w2 == null || (min_val > w.transform.position.z)) {
								w2 = w;
								min_val = w.transform.position.z;
							}
						}
					}
					else {
						min_dist = Mathf.Abs (w.transform.position.z - target.transform.position.z);
						if (w.transform.position.y == height * 15 / 16) { //upper wall
							if (w3 == null || min_dist < min_dist_low) {
								w3 = w;
								min_dist_low = min_dist;
							}
						}
						else if (w.transform.position.y == height * 2 / 16) {
							if (w4 == null || min_dist < min_dist_high) { //lower wall
								w4 = w;
								min_dist_high = min_dist;
							}
						}
					}
				}

				lat_min = w1.transform.position.z - w1.transform.localScale.z / 2;
				lat_max = w2.transform.position.z + w2.transform.localScale.z / 2;

				new_wall.transform.position = new Vector3 (
					w1.transform.position.x,
					w1.transform.position.y,
					(lat_min + lat_max) / 2
				);
				new_wall.transform.localScale = new Vector3 (
					w1.transform.localScale.x,
					w1.transform.localScale.y,
					lat_max - lat_min
				);

				new_wall_twin.transform.position = new Vector3 (
					w1.GetComponent<WallBehaviour>().twin.transform.position.x,
					w1.transform.position.y,
					(lat_min + lat_max) / 2
				);
				new_wall_twin.transform.localScale = new Vector3 (
					w1.transform.localScale.x,
					w1.transform.localScale.y,
					lat_max - lat_min
				);
			}
			else { //parent_wall.transform.localScale.z == 0.2f
				foreach (GameObject w in parent_wall.GetComponent<WallBehaviour>().wall_neighbours) {
					if (w.transform.position.y == height / 2) { //normal wall
						if (w.transform.position.z < target.transform.position.x) {
							if (w1 == null || (max_val < w.transform.position.x)) {
								w1 = w;
								max_val = w.transform.position.x;
							}
						}
						else if (w.transform.position.x > target.transform.position.x) {
							if (w2 == null || (min_val > w.transform.position.x)) {
								w2 = w;
								min_val = w.transform.position.x;
							}
						}
					}
					else {
						min_dist = Mathf.Abs (w.transform.position.x - target.transform.position.x);
						if (w.transform.position.y == height * 15 / 16) { //upper wall
							if (w3 == null || min_dist < min_dist_low) {
								w3 = w;
								min_dist_low = min_dist;
							}
						}
						else if (w.transform.position.y == height * 2 / 16) {
							if (w4 == null || min_dist < min_dist_high) { //lower wall
								w4 = w;
								min_dist_high = min_dist;
							}
						}
					}
				}

				lat_min = w1.transform.position.x - w1.transform.localScale.x / 2;
				lat_max = w2.transform.position.x + w2.transform.localScale.x / 2;

				new_wall.transform.position = new Vector3 (
					(lat_min + lat_max) / 2,
					w1.transform.position.y,
					w1.transform.position.z
				);
				new_wall.transform.localScale = new Vector3 (
					lat_max - lat_min,
					w1.transform.localScale.y,
					w1.transform.localScale.z
				);

				new_wall_twin.transform.position = new Vector3 (
					(lat_min + lat_max) / 2,
					w1.transform.position.y,
					w1.GetComponent<WallBehaviour>().twin.transform.position.z
				);
				new_wall_twin.transform.localScale = new Vector3 (
					lat_max - lat_min,
					w1.transform.localScale.y,
					w1.transform.localScale.z
				);
			}

			//compute room_neighbours + wall_neighbours + aux_objs + attached_objs
			//for the new wall
			aux_list = w1.GetComponent<WallBehaviour> ().room_neighbours;
			aux_list.Remove (w1);
			aux_list.Remove (w2);
			aux_list.Remove (w3);
			aux_list.Remove (w4);
			aux_list.Add (new_wall);

			foreach (GameObject w in aux_list)
				w.GetComponent<WallBehaviour> ().room_neighbours = aux_list;

			aux_list = w1.GetComponent<WallBehaviour> ().wall_neighbours;
			aux_list.Remove (w1);
			aux_list.Remove (w2);
			aux_list.Remove (w3);
			aux_list.Remove (w4);
			aux_list.Add (new_wall);

			foreach (GameObject w in aux_list)
				w.GetComponent<WallBehaviour> ().wall_neighbours = aux_list;

			aux_list = w1.GetComponent<WallBehaviour> ().aux_objs;
			aux_list.Remove (target);
			new_wall.GetComponent<WallBehaviour> ().aux_objs = aux_list;

			aux_list = new List<GameObject> ();
			foreach (GameObject o in w1.GetComponent<WallBehaviour>().attached_objs)
				aux_list.Add (o);
			foreach (GameObject o in w2.GetComponent<WallBehaviour>().attached_objs)
				aux_list.Add (o);
			foreach (GameObject o in w3.GetComponent<WallBehaviour>().attached_objs)
				aux_list.Add (o);
			foreach (GameObject o in w4.GetComponent<WallBehaviour>().attached_objs)
				aux_list.Add (o);

			new_wall.GetComponent<WallBehaviour> ().attached_objs = aux_list;

			//same for the walls' twins
			w1t = w1.GetComponent<WallBehaviour> ().twin;
			w2t = w2.GetComponent<WallBehaviour> ().twin;
			w3t = w3.GetComponent<WallBehaviour> ().twin;
			w4t = w4.GetComponent<WallBehaviour> ().twin;

			aux_list = w1t.GetComponent<WallBehaviour> ().room_neighbours;
			aux_list.Remove (w1t);
			aux_list.Remove (w2t);
			aux_list.Remove (w3t);
			aux_list.Remove (w4t);
			aux_list.Add (new_wall_twin);

			foreach (GameObject w in aux_list)
				w.GetComponent<WallBehaviour> ().room_neighbours = aux_list;

			aux_list = w1t.GetComponent<WallBehaviour> ().wall_neighbours;
			aux_list.Remove (w1t);
			aux_list.Remove (w2t);
			aux_list.Remove (w3t);
			aux_list.Remove (w4t);
			aux_list.Add (new_wall_twin);

			foreach (GameObject w in aux_list)
				w.GetComponent<WallBehaviour> ().wall_neighbours = aux_list;

			aux_list = w1t.GetComponent<WallBehaviour> ().aux_objs;
			aux_list.Remove (target);
			new_wall_twin.GetComponent<WallBehaviour> ().aux_objs = aux_list;

			aux_list = new List<GameObject> ();
			foreach (GameObject o in w1t.GetComponent<WallBehaviour>().attached_objs)
				aux_list.Add (o);
			foreach (GameObject o in w2t.GetComponent<WallBehaviour>().attached_objs)
				aux_list.Add (o);
			foreach (GameObject o in w3t.GetComponent<WallBehaviour>().attached_objs)
				aux_list.Add (o);
			foreach (GameObject o in w4t.GetComponent<WallBehaviour>().attached_objs)
				aux_list.Add (o);

			new_wall_twin.GetComponent<WallBehaviour> ().attached_objs = aux_list;

			//set the twin relationship between the 2 new walls
			new_wall.GetComponent<WallBehaviour> ().twin = new_wall_twin;
			new_wall_twin.GetComponent<WallBehaviour> ().twin = new_wall;

			Destroy (w1);
			Destroy (w2);
			Destroy (w3);
			Destroy (w4);
			Destroy (w1t);
			Destroy (w2t);
			Destroy (w3t);
			Destroy (w4t);
		}

		Destroy (target);
	}
	
	// Update is called once per frame
	void Update () {

		if (!active)
			return;

		DoRaycast ();

		if (found && GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.A)) {
			active = false;
			NetworkedDWDestroy (hit.transform.gameObject);
		}
	}

	public void StartDestroy(){
		active = true;
		//found = false;
		GameObject.Find("UIMaster").GetComponent<UIMasterScript>().stop=true;
		GameObject.Find ("UtilsHolder").GetComponent<Utils> ().SetWallsState (false);
		GameObject.Find ("UtilsHolder").GetComponent<Utils> ().SetDoorsState (false);
	}


}
