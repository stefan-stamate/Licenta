using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WallCreation : MonoBehaviour {

	public GameObject wall_prefab;

	Vector3 init_point;
	GameObject init_wall;
	int init_normal;
	RaycastHit hit;
	bool found;
	int stage;
	int orientation;
	GameObject new_wall;
	float wall_height;
	List<Material> wall_colors;

	void Start(){
		stage = 0;
		found = false;
		wall_height = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().height;

		new_wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
		new_wall.transform.position = new Vector3(0, 0, 0);
		new_wall.transform.localScale = new Vector3 (0, 0, 0);
		new_wall.GetComponent<Collider> ().enabled = false;

		wall_colors = new List<Material> ();
		Object[] ms = Resources.LoadAll ("SolidColors");//0 - green, 1 - red
		foreach (Object o in ms)
			wall_colors.Add ((Material)o);
	}

	public void InitiateDelete(GameObject target){
		StartCoroutine(target.GetComponent<WallBehaviour>().DelayedDeleteSelf());
	}

	bool InSameRoom(GameObject a, GameObject b){
		return a.GetComponent<WallBehaviour> ().room_neighbours == b.GetComponent<WallBehaviour> ().room_neighbours;
	}

	void ReactivateColliders(string tag){
		GameObject[] objs = GameObject.FindGameObjectsWithTag (tag);
		foreach (GameObject o in objs) {
			o.GetComponent<Collider> ().enabled = true;
		}
	}

	public void CreateWall (object[] input){

		float upper_z, lower_z, upper_x, lower_x;
		int orientation = (int)input [0];
		float init_point_x = (float)input [1];
		float init_point_z = (float)input [2];
		float hit_point_x = (float)input [3];
		float hit_point_z = (float)input [4];

		GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");
		GameObject sec_wall;
		init_wall = walls [(int)input [5]];
		sec_wall = walls [(int)input [6]];

		//first value is the orientation
		if (orientation == 1) {
			//set up the new wall
			//there are actually 2 and they are twins
			GameObject new_wall1, new_wall2;
			new_wall1 = GameObject.Instantiate (wall_prefab);
			new_wall2 = GameObject.Instantiate (wall_prefab);

			new_wall1.transform.position = new Vector3 ((init_point_x + hit_point_x) / 2, wall_height / 2, init_point_z + 0.1f);
			new_wall1.transform.localScale = new Vector3 (Mathf.Abs (init_point_x - hit_point_x), wall_height, 0.2f);

			new_wall2.transform.position = new Vector3 ((init_point_x + hit_point_x) / 2, wall_height / 2, init_point_z - 0.1f);
			new_wall2.transform.localScale = new Vector3 (Mathf.Abs (init_point_x - hit_point_x), wall_height, 0.2f);

			new_wall1.GetComponent<WallBehaviour> ().twin = new_wall2;
			new_wall2.GetComponent<WallBehaviour> ().twin = new_wall1;

			List<GameObject> wall_neighbours1 = new List<GameObject> ();
			wall_neighbours1.Add (new_wall1);
			new_wall1.GetComponent<WallBehaviour> ().wall_neighbours = wall_neighbours1;

			List<GameObject> wall_neighbours2 = new List<GameObject> ();
			wall_neighbours2.Add (new_wall2);
			new_wall2.GetComponent<WallBehaviour> ().wall_neighbours = wall_neighbours2;

			List<GameObject> new_neighbours1 = new List<GameObject> ();//new room_neighbours
			List<GameObject> new_neighbours2 = new List<GameObject> ();

			//the 2 walls that have been used to delimitate the new wall
			//need to be split into 2 parts in order to maintain neighbourship coherence
			//the twins of this walls also need to be split

			//set the walls that will replace the first wall picked
			GameObject init_wall1, init_wall2;
			init_wall1 = GameObject.Instantiate (wall_prefab);
			init_wall2 = GameObject.Instantiate (wall_prefab);
			upper_z = init_wall.transform.position.z + init_wall.transform.localScale.z / 2;
			lower_z = init_wall.transform.position.z - init_wall.transform.localScale.z / 2;
			init_wall1.transform.position = new Vector3 (init_wall.transform.position.x, init_wall.transform.position.y, (upper_z + init_point_z) / 2);
			init_wall1.transform.localScale = new Vector3 (init_wall.transform.localScale.x, init_wall.transform.localScale.y, upper_z - init_point_z);
			init_wall2.transform.position = new Vector3 (init_wall.transform.position.x, init_wall.transform.position.y, (init_point_z + lower_z) / 2);
			init_wall2.transform.localScale = new Vector3 (init_wall.transform.localScale.x, init_wall.transform.localScale.y, init_point_z - lower_z);
			init_wall1.GetComponent<WallBehaviour> ().destructible = init_wall.GetComponent<WallBehaviour> ().destructible;
			init_wall2.GetComponent<WallBehaviour> ().destructible = init_wall.GetComponent<WallBehaviour> ().destructible;
			new_neighbours1.Add (init_wall1);
			new_neighbours2.Add (init_wall2);

			//determine the wall_neighbours
			List<GameObject> init_wall_neighbours1 = new List<GameObject> ();
			List<GameObject> init_wall_neighbours2 = new List<GameObject> ();
			foreach (GameObject wn in init_wall.GetComponent<WallBehaviour>().wall_neighbours) {
				if (wn != init_wall) {
					if (wn.transform.position.z > init_point_z)
						init_wall_neighbours1.Add (wn);
					else
						init_wall_neighbours2.Add (wn);
				}
			}
			init_wall_neighbours1.Add (init_wall1);
			init_wall1.GetComponent<WallBehaviour> ().wall_neighbours = init_wall_neighbours1;
			init_wall_neighbours2.Add (init_wall2);
			init_wall2.GetComponent<WallBehaviour> ().wall_neighbours = init_wall_neighbours2;
			foreach (GameObject wn in init_wall_neighbours1) {
				wn.GetComponent<WallBehaviour> ().wall_neighbours = init_wall_neighbours1;
			}
			foreach (GameObject wn in init_wall_neighbours2) {
				wn.GetComponent<WallBehaviour> ().wall_neighbours = init_wall_neighbours2;
			}

			//and also attached_objs
			List<GameObject> init_attached_objs1 = new List<GameObject>();
			List<GameObject> init_attached_objs2 = new List<GameObject>();
			foreach (GameObject o in init_wall.GetComponent<WallBehaviour>().attached_objs) {
				if (o.transform.position.z > init_point_z)
					init_attached_objs1.Add (o);
				else
					init_attached_objs2.Add (o);	
			}
			init_wall1.GetComponent<WallBehaviour> ().attached_objs = init_attached_objs1;
			init_wall2.GetComponent<WallBehaviour> ().attached_objs = init_attached_objs2;

			GameObject init_twin = init_wall.GetComponent<WallBehaviour> ().twin;
			List<GameObject> new_init_twin_neighbours = init_twin.GetComponent<WallBehaviour> ().room_neighbours;
			new_init_twin_neighbours.Remove (init_twin);

			//set the new walls that will replace the twin
			GameObject init_twin_wall1, init_twin_wall2;
			init_twin_wall1 = GameObject.Instantiate (wall_prefab);
			init_twin_wall2 = GameObject.Instantiate (wall_prefab);
			init_twin_wall1.transform.position = new Vector3 (init_twin.transform.position.x, init_twin.transform.position.y, (upper_z + init_point_z) / 2);
			init_twin_wall1.transform.localScale = new Vector3 (init_twin.transform.localScale.x, init_twin.transform.localScale.y, upper_z - init_point_z);
			init_twin_wall2.transform.position = new Vector3 (init_twin.transform.position.x, init_twin.transform.position.y, (init_point_z + lower_z) / 2);
			init_twin_wall2.transform.localScale = new Vector3 (init_twin.transform.localScale.x, init_twin.transform.localScale.y, init_point_z - lower_z);
			init_twin_wall1.GetComponent<WallBehaviour> ().destructible = init_twin.GetComponent<WallBehaviour> ().destructible;
			init_twin_wall2.GetComponent<WallBehaviour> ().destructible = init_twin.GetComponent<WallBehaviour> ().destructible;
			new_init_twin_neighbours.Add (init_twin_wall1);
			new_init_twin_neighbours.Add (init_twin_wall2);

			//determine the wall_neighbours for the new walls
			List<GameObject> init_twin_wall_neighbours1 = new List<GameObject> ();
			List<GameObject> init_twin_wall_neighbours2 = new List<GameObject> ();
			foreach (GameObject wn in init_twin.GetComponent<WallBehaviour>().wall_neighbours) {
				if (wn != init_twin) {
					if (wn.transform.position.z > init_point_z)
						init_twin_wall_neighbours1.Add (wn);
					else
						init_twin_wall_neighbours2.Add (wn);
				}
			}
			init_twin_wall_neighbours1.Add (init_twin_wall1);
			init_twin_wall_neighbours2.Add (init_twin_wall2);
			foreach (GameObject wn in init_twin_wall_neighbours1) {
				wn.GetComponent<WallBehaviour> ().wall_neighbours = init_twin_wall_neighbours1;
			}
			foreach (GameObject wn in init_twin_wall_neighbours2) {
				wn.GetComponent<WallBehaviour> ().wall_neighbours = init_twin_wall_neighbours2;
			}

			//and also attached_objs
			List<GameObject> init_twin_attached_objs1 = new List<GameObject>();
			List<GameObject> init_twin_attached_objs2 = new List<GameObject>();
			foreach (GameObject o in init_twin.GetComponent<WallBehaviour>().attached_objs) {
				if (o.transform.position.z > init_point_z)
					init_twin_attached_objs1.Add (o);
				else
					init_twin_attached_objs2.Add (o);	
			}
			init_twin_wall1.GetComponent<WallBehaviour> ().attached_objs = init_twin_attached_objs1;
			init_twin_wall2.GetComponent<WallBehaviour> ().attached_objs = init_twin_attached_objs2;

			//update room_neighbours for the neighbours of the twin wall
			foreach (GameObject n in new_init_twin_neighbours) {
				n.GetComponent<WallBehaviour> ().room_neighbours = new_init_twin_neighbours;
			}

			Destroy (init_twin);
			init_wall1.GetComponent<WallBehaviour> ().twin = init_twin_wall1;
			init_twin_wall1.GetComponent<WallBehaviour> ().twin = init_wall1;
			init_wall2.GetComponent<WallBehaviour> ().twin = init_twin_wall2;
			init_twin_wall2.GetComponent<WallBehaviour> ().twin = init_wall2;

			List<GameObject> init_aux_objs1 = new List<GameObject> ();
			List<GameObject> init_aux_objs2 = new List<GameObject> ();
			foreach (GameObject o in sec_wall.transform.gameObject.GetComponent<WallBehaviour>().aux_objs) {
				print ("z1");
				if (o.transform.position.z > init_point_z)
					init_aux_objs1.Add (o);
				else
					init_aux_objs2.Add (o);
			}

			foreach(GameObject w in init_wall_neighbours1)
				w.GetComponent<WallBehaviour> ().aux_objs = init_aux_objs1;
			foreach(GameObject w in init_wall_neighbours2)
				w.GetComponent<WallBehaviour> ().aux_objs = init_aux_objs2;
			foreach(GameObject w in init_twin_wall_neighbours1)
				w.GetComponent<WallBehaviour> ().aux_objs = init_aux_objs1;
			foreach(GameObject w in init_twin_wall_neighbours1)
				w.GetComponent<WallBehaviour> ().aux_objs = init_aux_objs2;


			GameObject sec_wall1, sec_wall2;
			sec_wall1 = GameObject.Instantiate (wall_prefab);
			sec_wall2 = GameObject.Instantiate (wall_prefab);
			upper_z = sec_wall.transform.position.z + sec_wall.transform.localScale.z / 2;
			lower_z = sec_wall.transform.position.z - sec_wall.transform.localScale.z / 2;
			sec_wall1.transform.position = new Vector3 (sec_wall.transform.position.x, sec_wall.transform.position.y, (upper_z + init_point_z) / 2);
			sec_wall1.transform.localScale = new Vector3 (sec_wall.transform.localScale.x, sec_wall.transform.localScale.y, upper_z - init_point_z);
			sec_wall2.transform.position = new Vector3 (sec_wall.transform.position.x, sec_wall.transform.position.y, (init_point_z + lower_z) / 2);
			sec_wall2.transform.localScale = new Vector3 (sec_wall.transform.localScale.x, sec_wall.transform.localScale.y, init_point_z - lower_z);
			sec_wall1.GetComponent<WallBehaviour> ().destructible = sec_wall.GetComponent<WallBehaviour> ().destructible;
			sec_wall2.GetComponent<WallBehaviour> ().destructible = sec_wall.GetComponent<WallBehaviour> ().destructible;
			new_neighbours1.Add (sec_wall1);
			new_neighbours2.Add (sec_wall2);

			List<GameObject> sec_wall_neighbours1 = new List<GameObject> ();
			List<GameObject> sec_wall_neighbours2 = new List<GameObject> ();
			foreach (GameObject wn in sec_wall.transform.gameObject.GetComponent<WallBehaviour>().wall_neighbours) {
				if (wn != sec_wall) {
					if (wn.transform.position.z > init_point_z)
						sec_wall_neighbours1.Add (wn);
					else
						sec_wall_neighbours2.Add (wn);
				}
			}
			sec_wall_neighbours1.Add (sec_wall1);
			sec_wall_neighbours2.Add (sec_wall2);

			foreach (GameObject wn in sec_wall_neighbours1) {
				wn.GetComponent<WallBehaviour> ().wall_neighbours = sec_wall_neighbours1;
			}
			foreach (GameObject wn in sec_wall_neighbours2) {
				wn.GetComponent<WallBehaviour> ().wall_neighbours = sec_wall_neighbours2;
			}

			//and also attached_objs
			List<GameObject> sec_attached_objs1 = new List<GameObject>();
			List<GameObject> sec_attached_objs2 = new List<GameObject>();
			foreach (GameObject o in sec_wall.GetComponent<WallBehaviour>().attached_objs) {
				if (o.transform.position.z > init_point_z)
					sec_attached_objs1.Add (o);
				else
					sec_attached_objs2.Add (o);	
			}
			sec_wall1.GetComponent<WallBehaviour> ().attached_objs = sec_attached_objs1;
			sec_wall2.GetComponent<WallBehaviour> ().attached_objs = sec_attached_objs2;

			GameObject sec_twin = sec_wall.GetComponent<WallBehaviour> ().twin;
			List<GameObject> new_sec_twin_neighbours = sec_twin.GetComponent<WallBehaviour> ().room_neighbours;
			new_sec_twin_neighbours.Remove (sec_twin);

			//set the new walls that will replace the twin
			GameObject sec_twin_wall1, sec_twin_wall2;
			sec_twin_wall1 = GameObject.Instantiate (wall_prefab);
			sec_twin_wall2 = GameObject.Instantiate (wall_prefab);
			sec_twin_wall1.transform.position = new Vector3 (sec_twin.transform.position.x, sec_twin.transform.position.y, (upper_z + init_point_z) / 2);
			sec_twin_wall1.transform.localScale = new Vector3 (sec_twin.transform.localScale.x, sec_twin.transform.localScale.y, upper_z - init_point_z);
			sec_twin_wall2.transform.position = new Vector3 (sec_twin.transform.position.x, sec_twin.transform.position.y, (init_point_z + lower_z) / 2);
			sec_twin_wall2.transform.localScale = new Vector3 (sec_twin.transform.localScale.x, sec_twin.transform.localScale.y, init_point_z - lower_z);
			sec_twin_wall1.GetComponent<WallBehaviour> ().destructible = sec_twin.GetComponent<WallBehaviour> ().destructible;
			sec_twin_wall2.GetComponent<WallBehaviour> ().destructible = sec_twin.GetComponent<WallBehaviour> ().destructible;
			new_sec_twin_neighbours.Add (sec_twin_wall1);
			new_sec_twin_neighbours.Add (sec_twin_wall2);

			//determine the wall_neighbours for the new walls
			List<GameObject> sec_twin_wall_neighbours1 = new List<GameObject> ();
			List<GameObject> sec_twin_wall_neighbours2 = new List<GameObject> ();
			foreach (GameObject wn in sec_twin.GetComponent<WallBehaviour>().wall_neighbours) {
				if (wn != sec_twin) {
					if (wn.transform.position.z > init_point_z)
						sec_twin_wall_neighbours1.Add (wn);
					else
						sec_twin_wall_neighbours2.Add (wn);
				}
			}
			sec_twin_wall_neighbours1.Add (sec_twin_wall1);
			sec_twin_wall_neighbours2.Add (sec_twin_wall2);

			foreach (GameObject wn in sec_twin_wall_neighbours1) {
				wn.GetComponent<WallBehaviour> ().wall_neighbours = sec_twin_wall_neighbours1;
			}
			foreach (GameObject wn in sec_twin_wall_neighbours2) {
				wn.GetComponent<WallBehaviour> ().wall_neighbours = sec_twin_wall_neighbours2;
			}

			//and also attached_objs
			List<GameObject> sec_twin_attached_objs1 = new List<GameObject>();
			List<GameObject> sec_twin_attached_objs2 = new List<GameObject>();
			foreach (GameObject o in sec_twin.GetComponent<WallBehaviour>().attached_objs) {
				if (o.transform.position.z > init_point_z)
					sec_twin_attached_objs1.Add (o);
				else
					sec_twin_attached_objs2.Add (o);	
			}
			sec_twin_wall1.GetComponent<WallBehaviour> ().attached_objs = sec_twin_attached_objs1;
			sec_twin_wall2.GetComponent<WallBehaviour> ().attached_objs = sec_twin_attached_objs2;

			//update room_neighbours for the neighbours of the twin wall
			foreach (GameObject n in new_sec_twin_neighbours) {
				n.GetComponent<WallBehaviour> ().room_neighbours = new_sec_twin_neighbours;
			}
			Destroy (sec_twin);
			sec_wall1.GetComponent<WallBehaviour> ().twin = sec_twin_wall1;
			sec_twin_wall1.GetComponent<WallBehaviour> ().twin = sec_wall1;
			sec_wall2.GetComponent<WallBehaviour> ().twin = sec_twin_wall2;
			sec_twin_wall2.GetComponent<WallBehaviour> ().twin = sec_wall2;

			List<GameObject> sec_aux_objs1 = new List<GameObject> ();
			List<GameObject> sec_aux_objs2 = new List<GameObject> ();
			foreach (GameObject o in sec_wall.GetComponent<WallBehaviour>().aux_objs) {
				if (o.transform.position.z > init_point_z)
					sec_aux_objs1.Add (o);
				else
					sec_aux_objs2.Add (o);
			}

			foreach(GameObject w in sec_wall_neighbours1)
				w.GetComponent<WallBehaviour> ().aux_objs = sec_aux_objs1;
			foreach(GameObject w in sec_wall_neighbours2)
				w.GetComponent<WallBehaviour> ().aux_objs = sec_aux_objs2;
			foreach(GameObject w in sec_twin_wall_neighbours1)
				w.GetComponent<WallBehaviour> ().aux_objs = sec_aux_objs1;
			foreach(GameObject w in sec_twin_wall_neighbours2)
				w.GetComponent<WallBehaviour> ().aux_objs = sec_aux_objs2;

			new_neighbours1.Add (new_wall1);
			new_neighbours2.Add (new_wall2);
			foreach (GameObject g in init_wall.GetComponent<WallBehaviour>().room_neighbours) {
				if (g != init_wall && g != sec_wall) {
					if (g.transform.position.z > init_wall.transform.position.z)
						new_neighbours1.Add (g);
					else
						new_neighbours2.Add (g);
				}
			}

			//update the neighbours in all walls
			foreach (GameObject n in new_neighbours1) {
				n.GetComponent<WallBehaviour> ().room_neighbours = new_neighbours1;
			}
			foreach (GameObject n in new_neighbours2) {
				n.GetComponent<WallBehaviour> ().room_neighbours = new_neighbours2;
			}

			//break into 2 parts the floor and ceiling too
			List<GameObject> init_ceilings = new List<GameObject> ();
			List<GameObject> init_floors = new List<GameObject> ();
			GameObject[] ceilings = GameObject.FindGameObjectsWithTag ("Ceiling");
			GameObject[] floors = GameObject.FindGameObjectsWithTag ("Floor");
			foreach (GameObject c in ceilings) {//--------------probably can do ceiling and floor at the same time, their lists should be in same order
				upper_x = c.transform.position.x + c.transform.localScale.x / 2 - 0.2f;
				lower_x = c.transform.position.x - c.transform.localScale.x / 2 + 0.2f;
				upper_z = c.transform.position.z + c.transform.localScale.z / 2 - 0.2f;
				lower_z = c.transform.position.z - c.transform.localScale.z / 2 + 0.2f;
				if (init_wall.transform.position.z > lower_z && init_wall.transform.position.z < upper_z &&
					((lower_x >= init_wall.transform.position.x && upper_x <= sec_wall.transform.position.x) ||
						(lower_x >= sec_wall.transform.position.x && upper_x <= init_wall.transform.position.x))) {
					init_ceilings.Add (c);
				}
			}
			foreach (GameObject f in floors) {
				upper_x = f.transform.position.x + f.transform.localScale.x / 2 - 0.2f;
				lower_x = f.transform.position.x - f.transform.localScale.x / 2 + 0.2f;
				upper_z = f.transform.position.z + f.transform.localScale.z / 2 - 0.2f;
				lower_z = f.transform.position.z - f.transform.localScale.z / 2 + 0.2f;
				if (init_wall.transform.position.z > lower_z && init_wall.transform.position.z < upper_z &&
					((lower_x >= init_wall.transform.position.x && upper_x <= sec_wall.transform.position.x) ||
						(lower_x >= sec_wall.transform.position.x && upper_x <= init_wall.transform.position.x))) {
					init_floors.Add (f);
				}
			}

			List<GameObject> new_ceilings1, new_ceilings2;
			new_ceilings1 = new List<GameObject> ();
			new_ceilings2 = new List<GameObject> ();

			//the room_neighbours of the floors and ceilings
			//that will be splitted need to be included in the new room_neighbours
			foreach (GameObject n in init_ceilings[0].GetComponent<WallBehaviour>().room_neighbours) {
				if (init_ceilings.IndexOf (n) == -1) {//ignore the elements that will be splitted
					if (n.transform.position.z > hit_point_z)
						new_ceilings1.Add (n);
					else
						new_ceilings2.Add (n);
				}
			}

			List<GameObject> new_ceilings_attached_objs1, new_ceilings_attached_objs2;

			foreach (GameObject c in init_ceilings) {
				GameObject new_ceiling1, new_ceiling2;
				new_ceiling1 = Instantiate (wall_prefab);
				new_ceiling2 = Instantiate (wall_prefab);

				new_ceiling1.transform.position = new Vector3 (c.transform.position.x, c.transform.position.y,
					(c.transform.position.z + c.transform.localScale.z / 2 + init_point_z) / 2);
				new_ceiling1.transform.localScale = new Vector3 (c.transform.localScale.x, c.transform.localScale.y,
					c.transform.position.z + c.transform.localScale.z / 2 - init_point_z);
				new_ceiling2.transform.position = new Vector3 (c.transform.position.x, c.transform.position.y,
					(c.transform.position.z - c.transform.localScale.z / 2 + init_point_z) / 2);
				new_ceiling2.transform.localScale = new Vector3 (c.transform.localScale.x, c.transform.localScale.y,
					init_point_z - c.transform.position.z + c.transform.localScale.z / 2);
				new_ceiling1.tag = "Ceiling";
				new_ceiling2.tag = "Ceiling";

				new_ceilings1.Add (new_ceiling1);
				new_ceilings2.Add (new_ceiling2);

				new_ceilings_attached_objs1 = new List<GameObject> ();
				new_ceilings_attached_objs2 = new List<GameObject> ();
				foreach (GameObject o in c.GetComponent<WallBehaviour>().attached_objs) {
					if (o.transform.position.z > hit_point_z)
						new_ceilings_attached_objs1.Add (o);
					else
						new_ceilings_attached_objs2.Add (o);
				}

				new_ceiling1.GetComponent<WallBehaviour> ().attached_objs = new_ceilings_attached_objs1;
				new_ceiling2.GetComponent<WallBehaviour> ().attached_objs = new_ceilings_attached_objs2;

				Destroy (c);
			}
			foreach (GameObject c in new_ceilings1) {
				c.GetComponent<WallBehaviour> ().room_neighbours = new_ceilings1;
			}
			foreach (GameObject c in new_ceilings2) {
				c.GetComponent<WallBehaviour> ().room_neighbours = new_ceilings2;
			}

			List<GameObject> new_floors1, new_floors2;
			new_floors1 = new List<GameObject> ();
			new_floors2 = new List<GameObject> ();

			//the room_neighbours of the floors and ceilings
			//that will be splitted need to be included in the new room_neighbours
			foreach (GameObject n in init_floors[0].GetComponent<WallBehaviour>().room_neighbours) {
				if (init_floors.IndexOf (n) == -1) {//ignore the elements that will be splitted
					if (n.transform.position.z > hit_point_z)
						new_floors1.Add (n);
					else
						new_floors2.Add (n);
				}
			}

			List<GameObject> new_floors_attached_objs1, new_floors_attached_objs2;

			foreach (GameObject f in init_floors) {
				GameObject new_floor1, new_floor2;
				new_floor1 = Instantiate (wall_prefab);
				new_floor2 = Instantiate (wall_prefab);

				new_floor1.transform.position = new Vector3 (f.transform.position.x, f.transform.position.y,
					(f.transform.position.z + f.transform.localScale.z / 2 + init_point_z) / 2);
				new_floor1.transform.localScale = new Vector3 (f.transform.localScale.x, f.transform.localScale.y,
					f.transform.position.z + f.transform.localScale.z / 2 - init_point_z);
				new_floor2.transform.position = new Vector3 (f.transform.position.x, f.transform.position.y,
					(f.transform.position.z - f.transform.localScale.z / 2 + init_point_z) / 2);
				new_floor2.transform.localScale = new Vector3 (f.transform.localScale.x, f.transform.localScale.y,
					init_point_z - f.transform.position.z + f.transform.localScale.z / 2);
				new_floor1.tag = "Floor";
				new_floor2.tag = "Floor";

				new_floors1.Add (new_floor1);
				new_floors2.Add (new_floor2);

				new_floors_attached_objs1 = new List<GameObject> ();
				new_floors_attached_objs2 = new List<GameObject> ();
				foreach (GameObject o in f.GetComponent<WallBehaviour>().attached_objs) {
					if (o.transform.position.z > hit_point_z)
						new_floors_attached_objs1.Add (o);
					else
						new_floors_attached_objs2.Add (o);
				}

				new_floor1.GetComponent<WallBehaviour> ().attached_objs = new_floors_attached_objs1;
				new_floor2.GetComponent<WallBehaviour> ().attached_objs = new_floors_attached_objs2;

				Destroy (f);
			}
			foreach (GameObject f in new_floors1) {
				f.GetComponent<WallBehaviour> ().room_neighbours = new_floors1;
			}
			foreach (GameObject f in new_floors2) {
				f.GetComponent<WallBehaviour> ().room_neighbours = new_floors2;
			}

			Destroy (init_wall);
			Destroy (sec_wall);
			DeactivateRaycasting ();
		}
		else if (orientation == -1) {
			//set up the new wall
			//there are actually 2 and they are twins
			GameObject new_wall1, new_wall2;
			new_wall1 = GameObject.Instantiate (wall_prefab);
			new_wall2 = GameObject.Instantiate (wall_prefab);

			new_wall1.transform.position = new Vector3 (init_point_x + 0.1f, wall_height / 2, (init_point_z + hit_point_z) / 2);
			new_wall1.transform.localScale = new Vector3 (0.2f, wall_height, Mathf.Abs (init_point_z - hit_point_z));

			new_wall2.transform.position = new Vector3 (init_point_x - 0.1f, wall_height / 2, (init_point_z + hit_point_z) / 2);
			new_wall2.transform.localScale = new Vector3 (0.2f, wall_height, Mathf.Abs (init_point_z - hit_point_z));

			new_wall1.GetComponent<WallBehaviour> ().twin = new_wall2;
			new_wall2.GetComponent<WallBehaviour> ().twin = new_wall1;

			List<GameObject> wall_neighbours1 = new List<GameObject> ();
			wall_neighbours1.Add (new_wall1);
			new_wall1.GetComponent<WallBehaviour> ().wall_neighbours = wall_neighbours1;

			List<GameObject> wall_neighbours2 = new List<GameObject> ();
			wall_neighbours2.Add (new_wall2);
			new_wall2.GetComponent<WallBehaviour> ().wall_neighbours = wall_neighbours2;

			List<GameObject> new_neighbours1 = new List<GameObject> ();//new room_neighbours
			List<GameObject> new_neighbours2 = new List<GameObject> ();

			//the 2 walls that have been used to delimitate the new wall
			//need to be split into 2 parts in order to maintain neighbourship coherence
			//the twins of this walls also need to be split

			//set the walls that will replace the first wall picked
			GameObject init_wall1, init_wall2;
			init_wall1 = GameObject.Instantiate (wall_prefab);
			init_wall2 = GameObject.Instantiate (wall_prefab);
			upper_x = init_wall.transform.position.x + init_wall.transform.localScale.x / 2;
			lower_x = init_wall.transform.position.x - init_wall.transform.localScale.x / 2;
			init_wall1.transform.position = new Vector3 ((upper_x + init_point_x) / 2, init_wall.transform.position.y, init_wall.transform.position.z);
			init_wall1.transform.localScale = new Vector3 (upper_x - init_point_x, init_wall.transform.localScale.y, init_wall.transform.localScale.z);
			init_wall2.transform.position = new Vector3 ((init_point_x + lower_x) / 2, init_wall.transform.position.y, init_wall.transform.position.z);
			init_wall2.transform.localScale = new Vector3 (init_point_x - lower_x, init_wall.transform.localScale.y, init_wall.transform.localScale.z);
			init_wall1.GetComponent<WallBehaviour> ().destructible = init_wall.GetComponent<WallBehaviour> ().destructible;
			init_wall2.GetComponent<WallBehaviour> ().destructible = init_wall.GetComponent<WallBehaviour> ().destructible;
			new_neighbours1.Add (init_wall1);
			new_neighbours2.Add (init_wall2);

			//determine the wall_neighbours
			List<GameObject> init_wall_neighbours1 = new List<GameObject> ();
			List<GameObject> init_wall_neighbours2 = new List<GameObject> ();
			foreach (GameObject wn in init_wall.GetComponent<WallBehaviour>().wall_neighbours) {
				if (wn != init_wall) {
					if (wn.transform.position.x > sec_wall.transform.position.x)
						init_wall_neighbours1.Add (wn);
					else
						init_wall_neighbours2.Add (wn);
				}
			}
			init_wall_neighbours1.Add (init_wall1);
			init_wall1.GetComponent<WallBehaviour> ().wall_neighbours = init_wall_neighbours1;
			init_wall_neighbours2.Add (init_wall2);
			init_wall2.GetComponent<WallBehaviour> ().wall_neighbours = init_wall_neighbours2;
			foreach (GameObject wn in init_wall_neighbours1) {
				wn.GetComponent<WallBehaviour> ().wall_neighbours = init_wall_neighbours1;
			}
			foreach (GameObject wn in init_wall_neighbours2) {
				wn.GetComponent<WallBehaviour> ().wall_neighbours = init_wall_neighbours2;
			}

			//and also attached_objs
			List<GameObject> init_attached_objs1 = new List<GameObject>();
			List<GameObject> init_attached_objs2 = new List<GameObject>();
			foreach (GameObject o in init_wall.GetComponent<WallBehaviour>().attached_objs) {
				if (o.transform.position.x > init_point_x)
					init_attached_objs1.Add (o);
				else
					init_attached_objs2.Add (o);	
			}
			init_wall1.GetComponent<WallBehaviour> ().attached_objs = init_attached_objs1;
			init_wall2.GetComponent<WallBehaviour> ().attached_objs = init_attached_objs2;

			//split the twin
			GameObject init_twin = init_wall.GetComponent<WallBehaviour> ().twin;
			List<GameObject> new_init_twin_neighbours = init_twin.GetComponent<WallBehaviour> ().room_neighbours;
			new_init_twin_neighbours.Remove (init_twin);

			//set the new walls that will replace the twin
			GameObject init_twin_wall1, init_twin_wall2;
			init_twin_wall1 = GameObject.Instantiate (wall_prefab);
			init_twin_wall2 = GameObject.Instantiate (wall_prefab);
			init_twin_wall1.transform.position = new Vector3 ((upper_x + init_point_x) / 2, init_twin.transform.position.y, init_twin.transform.position.z);
			init_twin_wall1.transform.localScale = new Vector3 (upper_x - init_point_x, init_twin.transform.localScale.y, init_twin.transform.localScale.z);
			init_twin_wall2.transform.position = new Vector3 ((init_point_x + lower_x) / 2, init_twin.transform.position.y, init_twin.transform.position.z);
			init_twin_wall2.transform.localScale = new Vector3 (init_point_x - lower_x, init_twin.transform.localScale.y, init_twin.transform.localScale.z);
			init_twin_wall1.GetComponent<WallBehaviour> ().destructible = init_twin.GetComponent<WallBehaviour> ().destructible;
			init_twin_wall2.GetComponent<WallBehaviour> ().destructible = init_twin.GetComponent<WallBehaviour> ().destructible;
			new_init_twin_neighbours.Add (init_twin_wall1);
			new_init_twin_neighbours.Add (init_twin_wall2);

			//determine the wall_neighbours for the new walls
			List<GameObject> init_twin_wall_neighbours1 = new List<GameObject> ();
			List<GameObject> init_twin_wall_neighbours2 = new List<GameObject> ();
			foreach (GameObject wn in init_twin.GetComponent<WallBehaviour>().wall_neighbours) {
				if (wn != init_twin) {
					if (wn.transform.position.x > sec_wall.transform.position.x)
						init_twin_wall_neighbours1.Add (wn);
					else
						init_twin_wall_neighbours2.Add (wn);
				}
			}
			init_twin_wall_neighbours1.Add (init_twin_wall1);
			init_twin_wall_neighbours2.Add (init_twin_wall2);
			foreach (GameObject wn in init_twin_wall_neighbours1) {
				wn.GetComponent<WallBehaviour> ().wall_neighbours = init_twin_wall_neighbours1;
			}
			foreach (GameObject wn in init_twin_wall_neighbours2) {
				wn.GetComponent<WallBehaviour> ().wall_neighbours = init_twin_wall_neighbours2;
			}

			//and also attached_objs
			List<GameObject> init_twin_attached_objs1 = new List<GameObject>();
			List<GameObject> init_twin_attached_objs2 = new List<GameObject>();
			foreach (GameObject o in init_twin.GetComponent<WallBehaviour>().attached_objs) {
				if (o.transform.position.x > init_point_x)
					init_twin_attached_objs1.Add (o);
				else
					init_twin_attached_objs2.Add (o);	
			}
			init_twin_wall1.GetComponent<WallBehaviour> ().attached_objs = init_twin_attached_objs1;
			init_twin_wall2.GetComponent<WallBehaviour> ().attached_objs = init_twin_attached_objs2;

			//update room_neighbours for the neighbours of the twin wall
			foreach (GameObject n in new_init_twin_neighbours) {
				n.GetComponent<WallBehaviour> ().room_neighbours = new_init_twin_neighbours;
			}

			//update room_neighbours for the neighbours of the twin wall
			foreach (GameObject n in new_init_twin_neighbours) {
				n.GetComponent<WallBehaviour> ().room_neighbours = new_init_twin_neighbours;
			}
			Destroy (init_twin);
			init_wall1.GetComponent<WallBehaviour> ().twin = init_twin_wall1;
			init_twin_wall1.GetComponent<WallBehaviour> ().twin = init_wall1;
			init_wall2.GetComponent<WallBehaviour> ().twin = init_twin_wall2;
			init_twin_wall2.GetComponent<WallBehaviour> ().twin = init_wall2;

			//the list of aux_objs on either side will be split based on
			//the new wall's position
			List<GameObject> init_aux_objs1 = new List<GameObject> ();
			List<GameObject> init_aux_objs2 = new List<GameObject> ();
			foreach (GameObject o in init_wall.GetComponent<WallBehaviour>().aux_objs) {
				if (o.transform.position.x > init_point_x)
					init_aux_objs1.Add (o);
				else
					init_aux_objs2.Add (o);
			}

			foreach(GameObject w in init_wall_neighbours1)
				w.GetComponent<WallBehaviour> ().aux_objs = init_aux_objs1;
			foreach(GameObject w in init_wall_neighbours2)
				w.GetComponent<WallBehaviour> ().aux_objs = init_aux_objs2;
			foreach(GameObject w in init_twin_wall_neighbours1)
				w.GetComponent<WallBehaviour> ().aux_objs = init_aux_objs1;
			foreach(GameObject w in init_twin_wall_neighbours2)
				w.GetComponent<WallBehaviour> ().aux_objs = init_aux_objs2;

			//set the walls that will replace the second wall picked
			GameObject sec_wall1, sec_wall2;
			sec_wall1 = GameObject.Instantiate (wall_prefab);
			sec_wall2 = GameObject.Instantiate (wall_prefab);
			upper_x = sec_wall.transform.position.x + sec_wall.transform.localScale.x / 2;
			lower_x = sec_wall.transform.position.x - sec_wall.transform.localScale.x / 2;
			sec_wall1.transform.position = new Vector3 ((upper_x + init_point_x) / 2, sec_wall.transform.position.y, sec_wall.transform.position.z);
			sec_wall1.transform.localScale = new Vector3 (upper_x - init_point_x, sec_wall.transform.localScale.y, sec_wall.transform.localScale.z);
			sec_wall2.transform.position = new Vector3 ((init_point_x + lower_x) / 2, sec_wall.transform.position.y, sec_wall.transform.position.z);
			sec_wall2.transform.localScale = new Vector3 (init_point_x - lower_x, sec_wall.transform.localScale.y, sec_wall.transform.localScale.z);
			sec_wall1.GetComponent<WallBehaviour> ().destructible = sec_wall.transform.gameObject.GetComponent<WallBehaviour> ().destructible;
			sec_wall2.GetComponent<WallBehaviour> ().destructible = sec_wall.transform.gameObject.GetComponent<WallBehaviour> ().destructible;
			new_neighbours1.Add (sec_wall1);
			new_neighbours2.Add (sec_wall2);

			List<GameObject> sec_wall_neighbours1 = new List<GameObject> ();
			List<GameObject> sec_wall_neighbours2 = new List<GameObject> ();
			foreach (GameObject wn in sec_wall.GetComponent<WallBehaviour>().wall_neighbours) {
				if (wn != sec_wall) {
					if (wn.transform.position.x > sec_wall.transform.position.x)
						sec_wall_neighbours1.Add (wn);
					else
						sec_wall_neighbours2.Add (wn);
				}
			}
			sec_wall_neighbours1.Add (sec_wall1);
			sec_wall_neighbours2.Add (sec_wall2);

			foreach (GameObject wn in sec_wall_neighbours1) {
				wn.GetComponent<WallBehaviour> ().wall_neighbours = sec_wall_neighbours1;
			}
			foreach (GameObject wn in sec_wall_neighbours2) {
				wn.GetComponent<WallBehaviour> ().wall_neighbours = sec_wall_neighbours2;
			}

			//and also attached_objs
			List<GameObject> sec_attached_objs1 = new List<GameObject>();
			List<GameObject> sec_attached_objs2 = new List<GameObject>();
			foreach (GameObject o in sec_wall.GetComponent<WallBehaviour>().attached_objs) {
				if (o.transform.position.x > init_point_x)
					sec_attached_objs1.Add (o);
				else
					sec_attached_objs2.Add (o);	
			}
			sec_wall1.GetComponent<WallBehaviour> ().attached_objs = sec_attached_objs1;
			sec_wall2.GetComponent<WallBehaviour> ().attached_objs = sec_attached_objs2;

			//if the second wall has a twin it must also be split in two
			GameObject sec_twin = sec_wall.GetComponent<WallBehaviour> ().twin;
			List<GameObject> new_sec_twin_neighbours = sec_twin.GetComponent<WallBehaviour> ().room_neighbours;
			new_sec_twin_neighbours.Remove (sec_twin);

			//set the new walls that will replace the twin
			GameObject sec_twin_wall1, sec_twin_wall2;
			sec_twin_wall1 = GameObject.Instantiate (wall_prefab);
			sec_twin_wall2 = GameObject.Instantiate (wall_prefab);
			sec_twin_wall1.transform.position = new Vector3 ((upper_x + init_point_x) / 2, sec_twin.transform.position.y, sec_twin.transform.position.z);
			sec_twin_wall1.transform.localScale = new Vector3 (upper_x - init_point_x, sec_twin.transform.localScale.y, sec_twin.transform.localScale.z);
			sec_twin_wall2.transform.position = new Vector3 ((init_point_x + lower_x) / 2, sec_twin.transform.position.y, sec_twin.transform.position.z);
			sec_twin_wall2.transform.localScale = new Vector3 (init_point_x - lower_x, sec_twin.transform.localScale.y, sec_twin.transform.localScale.z);
			sec_twin_wall1.GetComponent<WallBehaviour> ().destructible = sec_twin.GetComponent<WallBehaviour> ().destructible;
			sec_twin_wall2.GetComponent<WallBehaviour> ().destructible = sec_twin.GetComponent<WallBehaviour> ().destructible;
			new_sec_twin_neighbours.Add (sec_twin_wall1);
			new_sec_twin_neighbours.Add (sec_twin_wall2);

			//determine the wall_neighbours for the new walls
			List<GameObject> sec_twin_wall_neighbours1 = new List<GameObject> ();
			List<GameObject> sec_twin_wall_neighbours2 = new List<GameObject> ();
			foreach (GameObject wn in sec_twin.GetComponent<WallBehaviour>().wall_neighbours) {
				if (wn != sec_twin) {
					if (wn.transform.position.z > sec_wall.transform.position.z)
						sec_twin_wall_neighbours1.Add (wn);
					else
						sec_twin_wall_neighbours2.Add (wn);
				}
			}

			sec_twin_wall_neighbours1.Add (sec_twin_wall1);
			sec_twin_wall_neighbours2.Add (sec_twin_wall2);
			foreach (GameObject wn in sec_twin_wall_neighbours1) {
				wn.GetComponent<WallBehaviour> ().wall_neighbours = sec_twin_wall_neighbours1;
			}
			foreach (GameObject wn in sec_twin_wall_neighbours2) {
				wn.GetComponent<WallBehaviour> ().wall_neighbours = sec_twin_wall_neighbours2;
			}

			//and also attached_objs
			List<GameObject> sec_twin_attached_objs1 = new List<GameObject>();
			List<GameObject> sec_twin_attached_objs2 = new List<GameObject>();
			foreach (GameObject o in sec_twin.GetComponent<WallBehaviour>().attached_objs) {
				if (o.transform.position.x > init_point_x)
					sec_twin_attached_objs1.Add (o);
				else
					sec_twin_attached_objs2.Add (o);	
			}
			sec_twin_wall1.GetComponent<WallBehaviour> ().attached_objs = sec_twin_attached_objs1;
			sec_twin_wall2.GetComponent<WallBehaviour> ().attached_objs = sec_twin_attached_objs2;

			//update room_neighbours for the neighbours of the twin wall
			foreach (GameObject n in new_sec_twin_neighbours) {
				n.GetComponent<WallBehaviour> ().room_neighbours = new_sec_twin_neighbours;
			}

			Destroy (sec_twin);
			sec_wall1.GetComponent<WallBehaviour> ().twin = sec_twin_wall1;
			sec_twin_wall1.GetComponent<WallBehaviour> ().twin = sec_wall1;
			sec_wall2.GetComponent<WallBehaviour> ().twin = sec_twin_wall2;
			sec_twin_wall2.GetComponent<WallBehaviour> ().twin = sec_wall2;

			List<GameObject> sec_aux_objs1 = new List<GameObject> ();
			List<GameObject> sec_aux_objs2 = new List<GameObject> ();
			foreach (GameObject o in sec_wall.GetComponent<WallBehaviour>().aux_objs) {
				if (o.transform.position.x > init_point_x)
					sec_aux_objs1.Add (o);
				else
					sec_aux_objs2.Add (o);
			}

			foreach(GameObject w in sec_wall_neighbours1)
				w.GetComponent<WallBehaviour> ().aux_objs = sec_aux_objs1;
			foreach(GameObject w in sec_wall_neighbours2)
				w.GetComponent<WallBehaviour> ().aux_objs = sec_aux_objs2;
			foreach(GameObject w in sec_twin_wall_neighbours1)
				w.GetComponent<WallBehaviour> ().aux_objs = sec_aux_objs1;
			foreach(GameObject w in sec_twin_wall_neighbours2)
				w.GetComponent<WallBehaviour> ().aux_objs = sec_aux_objs2;


			new_neighbours1.Add (new_wall1);
			new_neighbours2.Add (new_wall2);
			foreach (GameObject g in init_wall.GetComponent<WallBehaviour>().room_neighbours) {
				if (g != init_wall && g != sec_wall) {
					if (g.transform.position.x > init_wall.transform.position.x)
						new_neighbours1.Add (g);
					else
						new_neighbours2.Add (g);
				}
			}

			//update the neighbours in all walls
			foreach (GameObject n in new_neighbours1) {
				n.GetComponent<WallBehaviour> ().room_neighbours = new_neighbours1;
			}
			foreach (GameObject n in new_neighbours2) {
				n.GetComponent<WallBehaviour> ().room_neighbours = new_neighbours2;
			}

			//break into 2 parts the floor and ceiling too
			List<GameObject> init_ceilings = new List<GameObject> ();
			List<GameObject> init_floors = new List<GameObject> ();
			GameObject[] ceilings = GameObject.FindGameObjectsWithTag ("Ceiling");
			GameObject[] floors = GameObject.FindGameObjectsWithTag ("Floor");
			foreach (GameObject c in ceilings) {//--------------probably can do ceiling and floor at the same time, their lists should be in same order
				upper_x = c.transform.position.x + c.transform.localScale.x / 2 - 0.2f;
				lower_x = c.transform.position.x - c.transform.localScale.x / 2 + 0.2f;
				upper_z = c.transform.position.z + c.transform.localScale.z / 2 - 0.2f;
				lower_z = c.transform.position.z - c.transform.localScale.z / 2 + 0.2f;
				if (init_wall.transform.position.x > lower_x && init_wall.transform.position.x < upper_x &&
					((lower_z >= init_wall.transform.position.z && upper_z <= sec_wall.transform.position.z) ||
						(lower_z >= sec_wall.transform.position.z && upper_z <= init_wall.transform.position.z))) {
					init_ceilings.Add (c);
				}
			}
			foreach (GameObject f in floors) {
				upper_x = f.transform.position.x + f.transform.localScale.x / 2 - 0.2f;
				lower_x = f.transform.position.x - f.transform.localScale.x / 2 + 0.2f;
				upper_z = f.transform.position.z + f.transform.localScale.z / 2 - 0.2f;
				lower_z = f.transform.position.z - f.transform.localScale.z / 2 + 0.2f;
				if (init_wall.transform.position.x > lower_x && init_wall.transform.position.x < upper_x &&
					((lower_z >= init_wall.transform.position.z && upper_z <= sec_wall.transform.position.z) ||
						(lower_z >= sec_wall.transform.position.z && upper_z <= init_wall.transform.position.z))) {
					init_floors.Add (f);
				}
			}

			List<GameObject> new_ceilings1, new_ceilings2;
			new_ceilings1 = new List<GameObject> ();
			new_ceilings2 = new List<GameObject> ();

			//the room_neighbours of the floors and ceilings
			//that will be splitted need to be included in the new room_neighbours
			foreach (GameObject n in init_ceilings[0].GetComponent<WallBehaviour>().room_neighbours) {
				if (init_ceilings.IndexOf (n) == -1) {//ignore the elements that will be splitted
					if (n.transform.position.x > hit_point_x)
						new_ceilings1.Add (n);
					else
						new_ceilings2.Add (n);
				}
			}

			List<GameObject> new_ceilings_attached_objs1, new_ceilings_attached_objs2;

			foreach (GameObject c in init_ceilings) {
				GameObject new_ceiling1, new_ceiling2;
				new_ceiling1 = Instantiate (wall_prefab);
				new_ceiling2 = Instantiate (wall_prefab);

				new_ceiling1.transform.position = new Vector3 ((c.transform.position.x + c.transform.localScale.x / 2 + init_point_x) / 2,
					c.transform.position.y, c.transform.position.z);
				new_ceiling1.transform.localScale = new Vector3 (c.transform.position.x + c.transform.localScale.x / 2 - init_point_x,
					c.transform.localScale.y, c.transform.localScale.z);
				new_ceiling2.transform.position = new Vector3 ((c.transform.position.x - c.transform.localScale.x / 2 + init_point_x) / 2,
					c.transform.position.y, c.transform.position.z);
				new_ceiling2.transform.localScale = new Vector3 (init_point_x - c.transform.position.x + c.transform.localScale.x / 2,
					c.transform.localScale.y, c.transform.localScale.z);
				new_ceiling1.tag = "Ceiling";
				new_ceiling2.tag = "Ceiling";

				new_ceilings1.Add (new_ceiling1);
				new_ceilings2.Add (new_ceiling2);

				new_ceilings_attached_objs1 = new List<GameObject> ();
				new_ceilings_attached_objs2 = new List<GameObject> ();
				foreach (GameObject o in c.GetComponent<WallBehaviour>().attached_objs) {
					if (o.transform.position.x > hit_point_x)
						new_ceilings_attached_objs1.Add (o);
					else
						new_ceilings_attached_objs2.Add (o);
				}

				new_ceiling1.GetComponent<WallBehaviour> ().attached_objs = new_ceilings_attached_objs1;
				new_ceiling2.GetComponent<WallBehaviour> ().attached_objs = new_ceilings_attached_objs2;

				Destroy (c);
			}
			foreach (GameObject c in new_ceilings1) {
				c.GetComponent<WallBehaviour> ().room_neighbours = new_ceilings1;
			}
			foreach (GameObject c in new_ceilings2) {
				c.GetComponent<WallBehaviour> ().room_neighbours = new_ceilings2;
			}

			List<GameObject> new_floors1, new_floors2;
			new_floors1 = new List<GameObject> ();
			new_floors2 = new List<GameObject> ();

			//the room_neighbours of the floors and ceilings
			//that will be splitted need to be included in the new room_neighbours
			foreach (GameObject n in init_floors[0].GetComponent<WallBehaviour>().room_neighbours) {
				if (init_floors.IndexOf (n) == -1) {//ignore the elements that will be splitted
					if (n.transform.position.x > hit_point_x)
						new_floors1.Add (n);
					else
						new_floors2.Add (n);
				}
			}

			List<GameObject> new_floors_attached_objs1, new_floors_attached_objs2;

			foreach (GameObject f in init_floors) {
				GameObject new_floor1, new_floor2;
				new_floor1 = Instantiate (wall_prefab);
				new_floor2 = Instantiate (wall_prefab);

				new_floor1.transform.position = new Vector3 ((f.transform.position.x + f.transform.localScale.x / 2 + init_point_x) / 2,
					f.transform.position.y, f.transform.position.z);
				new_floor1.transform.localScale = new Vector3 (f.transform.position.x + f.transform.localScale.x / 2 - init_point_x,
					f.transform.localScale.y, f.transform.localScale.z);
				new_floor2.transform.position = new Vector3 ((f.transform.position.x - f.transform.localScale.x / 2 + init_point_x) / 2,
					f.transform.position.y, f.transform.position.z);
				new_floor2.transform.localScale = new Vector3 (init_point_x - f.transform.position.x + f.transform.localScale.x / 2,
					f.transform.localScale.y, f.transform.localScale.z);
				new_floor1.tag = "Floor";
				new_floor2.tag = "Floor";

				new_floors1.Add (new_floor1);
				new_floors2.Add (new_floor2);

				new_floors_attached_objs1 = new List<GameObject> ();
				new_floors_attached_objs2 = new List<GameObject> ();
				foreach (GameObject o in f.GetComponent<WallBehaviour>().attached_objs) {
					if (o.transform.position.x > hit_point_x)
						new_floors_attached_objs1.Add (o);
					else
						new_floors_attached_objs2.Add (o);
				}

				new_floor1.GetComponent<WallBehaviour> ().attached_objs = new_floors_attached_objs1;
				new_floor2.GetComponent<WallBehaviour> ().attached_objs = new_floors_attached_objs2;

				Destroy (f);
			}
			foreach (GameObject f in new_floors1) {
				f.GetComponent<WallBehaviour> ().room_neighbours = new_floors1;
			}
			foreach (GameObject f in new_floors2) {
				f.GetComponent<WallBehaviour> ().room_neighbours = new_floors2;
			}

			Destroy (init_wall);
			Destroy (sec_wall);
			DeactivateRaycasting ();
		}
	}

	void NetworkedCreateWall(int orientation){
		//build input
		int init_id, sec_id;
		init_id = init_wall.GetComponent<WallBehaviour> ().FindOwnId ();
		sec_id = hit.transform.gameObject.GetComponent<WallBehaviour> ().FindOwnId ();
		object[] input = {
			orientation,
			init_point.x,
			init_point.z,
			hit.point.x,
			hit.point.z,
			init_id,
			sec_id,
		};

		CreateWall (input);

		if (GameObject.Find ("NetworkingHandler").GetComponent<NetworkUtils> ().connected)
			PhotonNetwork.RaiseEvent (10, input, true, null);
	}
	
	void Update () {

		//stop when button2 is pressed
		if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.B) && stage!=0) {
			print ("canceled");
			DeactivateRaycasting ();
		}
			
		if (stage != 0) {
			//check if user is looking at a wall
			DoRaycast ();

			if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.A) && found) {
				if (stage == 1) {
					//if the user clicked on a wall then the first point of the
					//new wall is set and he can proceed to the next stage
					init_point = hit.point;
					init_wall = hit.transform.gameObject;
					found = false;
					stage++;
					if (hit.normal.x != 0) {
						orientation = 1;//x axis
						init_normal = (int)hit.normal.x;
					}
					else {
						orientation = -1;//z axis
						init_normal = (int)hit.normal.z;
					}
				}
				else if (stage == 2 && InSameRoom (init_wall, hit.transform.gameObject) &&
					new_wall.GetComponent<Renderer> ().material.ToString ().Split(' ')[0] ==
					wall_colors[0].ToString().Split(' ')[0]) {

					//check if the wall the user clicked on is valid as the second boundary for the new wall
					NetworkedCreateWall (orientation);
					/*
					if (orientation == 1 && hit.normal.x != 0 && hit.point.x != init_point.x &&
					    init_point.z > hit.transform.position.z - hit.transform.localScale.z / 2 + 0.2f &&
					    init_point.z < hit.transform.position.z + hit.transform.localScale.z / 2 - 0.2f) {

						NetworkedCreateWall (1);
					}
					else if (orientation == -1 && hit.normal.z != 0 && hit.point.z != init_point.z &&
					         init_point.x > hit.transform.position.x - hit.transform.localScale.x / 2 + 0.2f &&
					         init_point.x < hit.transform.position.x + hit.transform.localScale.x / 2 - 0.2f) {

						NetworkedCreateWall (-1);
					}
					*/
				}
			}

			//draw a wall between init_point and the user's gaze point
			//just for reference
			if (stage == 2) {
				float cam_dist, wall_dist;
				GameObject camera = GameObject.Find ("Camera Parent");
				//GameObject camera = GameObject.Find ("Main Camera");
				float dir_x = Mathf.Sin (camera.transform.localEulerAngles.y * Mathf.PI / 180);
				float dir_z = Mathf.Cos (camera.transform.localEulerAngles.y * Mathf.PI / 180);
				if (found && InSameRoom(init_wall,hit.transform.gameObject) &&
					((orientation == 1 && hit.normal.x == (-1)*init_normal && hit.point.x != init_point.x && hit.transform.localScale.z > 0.2f
					&& init_point.z > hit.transform.position.z-hit.transform.localScale.z/2+0.2f && init_point.z < hit.transform.position.z+hit.transform.localScale.z/2-0.2f) ||
					(orientation == -1 && hit.normal.z == (-1)*init_normal && hit.point.z != init_point.z && hit.transform.localScale.x > 0.2f
					&& init_point.x > hit.transform.position.x-hit.transform.localScale.x/2+0.2f && init_point.x < hit.transform.position.x+hit.transform.localScale.x/2-0.2f))) {

					if (orientation == 1) {
						new_wall.transform.position = new Vector3 ((init_point.x + hit.point.x) / 2, wall_height / 2, init_point.z);
						new_wall.transform.localScale = new Vector3 (Mathf.Abs(init_point.x - hit.point.x), wall_height, 0.2f);
					}
					else {
						new_wall.transform.position = new Vector3 (init_point.x, wall_height / 2, (init_point.z + hit.point.z) / 2);
						new_wall.transform.localScale = new Vector3 (0.2f, wall_height, Mathf.Abs(init_point.z - hit.point.z));
					}

					//check for object overlap
					float x_min, x_max, z_min, z_max;
					GameObject master_floor=null, master_ceiling=null;

					GameObject[] floors = GameObject.FindGameObjectsWithTag ("Floor");
					foreach (GameObject floor in floors) {
						x_min = floor.transform.position.x - floor.transform.localScale.x / 2;
						x_max = floor.transform.position.x + floor.transform.localScale.x / 2;
						z_min = floor.transform.position.z - floor.transform.localScale.z / 2;
						z_max = floor.transform.position.z + floor.transform.localScale.z / 2;

						if (hit.point.x > x_min &&
							hit.point.x < x_max &&
							hit.point.z > z_min &&
							hit.point.z < z_max) {

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

						if (hit.point.x > x_min &&
							hit.point.x < x_max &&
							hit.point.z > z_min &&
							hit.point.z < z_max) {

							master_ceiling = ceiling;
							break;
						}
					}

					//collect all objects in the room
					List<GameObject> objects = new List<GameObject> ();
					foreach (GameObject fp in master_floor.GetComponent<WallBehaviour>().room_neighbours) {
						foreach (GameObject fc in fp.GetComponent<WallBehaviour>().attached_objs) {
							objects.Add (fc);
						}
					}
					foreach (GameObject cp in master_ceiling.GetComponent<WallBehaviour>().room_neighbours) {
						foreach (GameObject cc in cp.GetComponent<WallBehaviour>().attached_objs) {
							if (objects.IndexOf (cc) == -1)
								objects.Add (cc);
						}
					}

					float wall_xmin, wall_xmax, wall_zmin, wall_zmax;
					wall_xmin = new_wall.transform.position.x - new_wall.transform.localScale.x / 2;
					wall_xmax = new_wall.transform.position.x + new_wall.transform.localScale.x / 2;
					wall_zmin = new_wall.transform.position.z - new_wall.transform.localScale.z / 2;
					wall_zmax = new_wall.transform.position.z + new_wall.transform.localScale.z / 2;

					bool flag = true;
					foreach (GameObject g in objects) {
						x_min = g.transform.position.x -
							Mathf.Abs (g.transform.right.x) * g.GetComponent<BoxCollider> ().size.x / 2 -
							Mathf.Abs (g.transform.forward.x) * g.GetComponent<BoxCollider> ().size.z / 2 +
							g.transform.right.x * g.GetComponent<BoxCollider> ().center.x +
							g.transform.forward.x * g.GetComponent<BoxCollider> ().center.z;
						x_max = g.transform.position.x +
							Mathf.Abs (g.transform.right.x) * g.GetComponent<BoxCollider> ().size.x / 2 +
							Mathf.Abs (g.transform.forward.x) * g.GetComponent<BoxCollider> ().size.z / 2 +
							g.transform.right.x * g.GetComponent<BoxCollider> ().center.x +
							g.transform.forward.x * g.GetComponent<BoxCollider> ().center.z;
						z_min = g.transform.position.z -
							Mathf.Abs (g.transform.right.z) * g.GetComponent<BoxCollider> ().size.x / 2 -
							Mathf.Abs (g.transform.forward.z) * g.GetComponent<BoxCollider> ().size.z / 2 +
							g.transform.right.z * g.GetComponent<BoxCollider> ().center.x +
							g.transform.forward.z * g.GetComponent<BoxCollider> ().center.z;
						z_max = g.transform.position.z +
							Mathf.Abs (g.transform.right.z) * g.GetComponent<BoxCollider> ().size.x / 2 +
							Mathf.Abs (g.transform.forward.z) * g.GetComponent<BoxCollider> ().size.z / 2 +
							g.transform.right.z * g.GetComponent<BoxCollider> ().center.x +
							g.transform.forward.z * g.GetComponent<BoxCollider> ().center.z;

						if ((((wall_xmin <= x_max && wall_xmin >= x_min) || (wall_xmax <= x_max && wall_xmax >= x_min)) &&
							((wall_zmin <= z_max && wall_zmin >= z_min) || (wall_zmax <= z_max && wall_zmax >= z_min))) ||

							(((wall_xmin < x_min && wall_xmin < x_max && wall_xmax > x_min && wall_xmax > x_max) ||
								(wall_xmin < x_min && wall_xmin < x_max && wall_xmax > x_min && wall_xmax > x_max)) &&
								((wall_zmin <= z_max && wall_zmin >= z_min) || (wall_zmax <= z_max && wall_zmax >= z_min))) ||

							(((wall_zmin < z_min && wall_zmin < z_max && wall_zmax > z_min && wall_zmax > z_max) ||
								(wall_zmin < z_min && wall_zmin < z_max && wall_zmax > z_min && wall_zmax > z_max)) &&
								((wall_xmin <= x_max && wall_xmin >= x_min) || (wall_xmax <= x_max && wall_xmax >= x_min)))) {

							flag = false;
							break;
						}
					}

					if (flag)
						new_wall.GetComponent<Renderer> ().material = wall_colors [0];
					else
						new_wall.GetComponent<Renderer> ().material = wall_colors [1];
				}
				else {
					new_wall.GetComponent<Renderer> ().material = wall_colors [1];
					if (orientation == 1) {
						cam_dist = (init_point.z - camera.transform.position.z) / dir_z;
						wall_dist = camera.transform.position.x + cam_dist * dir_x - init_point.x;
						if (cam_dist > 0 && wall_dist * init_normal > 0) {
							new_wall.transform.position = new Vector3 (init_point.x + wall_dist / 2, wall_height / 2, init_point.z);
							new_wall.transform.localScale = new Vector3 (wall_dist, wall_height, 0.2f);
						}
						else {
							new_wall.transform.position = new Vector3 (0, 0, 0);
							new_wall.transform.localScale = new Vector3 (0, 0, 0);
						}
					}
					else {
						cam_dist = (init_point.x - camera.transform.position.x) / dir_x;
						wall_dist = camera.transform.position.z + cam_dist * dir_z - init_point.z;
						if (cam_dist > 0 && wall_dist * init_normal > 0) {
							new_wall.transform.position = new Vector3 (init_point.x, wall_height / 2, init_point.z + wall_dist / 2);
							new_wall.transform.localScale = new Vector3 (0.2f, wall_height, wall_dist);
						}
						else {
							new_wall.transform.position = new Vector3 (0, 0, 0);
							new_wall.transform.localScale = new Vector3 (0, 0, 0);
						}
					}
				}
			}
		}
	}

	public void ActivateRaycasting(){

		GameObject.Find("UIMaster").GetComponent<UIMasterScript>().stop=true;
		GameObject.Find ("UtilsHolder").GetComponent<Utils> ().SetWallsState (false);
		GameObject.Find ("UtilsHolder").GetComponent<Utils> ().DeactivateColliders ("Object");
		stage = 1;
	}

	private void DeactivateRaycasting(){
		stage = 0;
		found = false;
		GameObject.Find ("UIMaster").GetComponent<UIMasterScript> ().stop = false;
		GameObject.Find ("UtilsHolder").GetComponent<Utils> ().SetWallsState (true);
		ReactivateColliders ("Object");
		new_wall.transform.position = new Vector3(0, 0, 0);
		new_wall.transform.localScale = new Vector3 (0, 0, 0);
	}

	void DoRaycast(){
		GameObject camera = GameObject.Find ("Camera Parent");
		//GameObject camera = GameObject.Find ("Main Camera");

		Vector3 dir = new Vector3 (Mathf.Sin (camera.transform.localEulerAngles.y * Mathf.PI / 180), 0, Mathf.Cos (camera.transform.localEulerAngles.y * Mathf.PI / 180));
		found = false;
		if (Physics.Raycast (camera.transform.position, dir, out hit)) {
			if (hit.transform.tag == "Wall" && hit.transform.position.y==wall_height/2)
				found = true;
		} else//------------------remove this when done implementing
			print ("nu");
	}
}
