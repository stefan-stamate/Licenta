using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class LoadSaveManager {

	public static void SaveScene(){

		SceneState state = new SceneState ();

		GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");
		List<object[]> walls_data = new List<object[]> ();
		foreach (GameObject w in walls) {
			object[] values = {
				w.transform.position.x,
				w.transform.position.y,
				w.transform.position.z,
				w.transform.localScale.x,
				w.transform.localScale.y,
				w.transform.localScale.z,
				w.GetComponent<WallBehaviour> ().set_mid,
				w.GetComponent<WallBehaviour> ().destructible,
				w.tag,
			};
			walls_data.Add (values);
		}
		state.walls = walls_data;

		GameObject[] floors = GameObject.FindGameObjectsWithTag ("Floor");
		List<object[]> floors_data = new List<object[]> ();
		foreach (GameObject f in floors) {
			object[] values = {
				f.transform.position.x,
				f.transform.position.y,
				f.transform.position.z,
				f.transform.localScale.x,
				f.transform.localScale.y,
				f.transform.localScale.z,
				f.GetComponent<WallBehaviour> ().set_mid,
				f.GetComponent<WallBehaviour> ().destructible,
				f.tag,
			};
			floors_data.Add (values);
		}
		state.floors = floors_data;

		GameObject[] ceilings = GameObject.FindGameObjectsWithTag ("Ceiling");
		List<object[]> ceilings_data = new List<object[]> ();
		foreach (GameObject c in ceilings) {
			object[] values = {
				c.transform.position.x,
				c.transform.position.y,
				c.transform.position.z,
				c.transform.localScale.x,
				c.transform.localScale.y,
				c.transform.localScale.z,
				c.GetComponent<WallBehaviour> ().set_mid,
				c.GetComponent<WallBehaviour> ().destructible,
				c.tag,
			};
			ceilings_data.Add (values);
		}
		state.ceilings = ceilings_data;

		//twins
		List<int> twins_indexes = new List<int> ();
		foreach (GameObject w in walls) {
			twins_indexes.Add (System.Array.IndexOf (walls, w.GetComponent<WallBehaviour> ().twin));
		}
		state.twins = twins_indexes.ToArray ();

		//wall_neighbours
		Dictionary<List<GameObject>,List<int>> walls_components = new Dictionary<List<GameObject>,List<int>> ();
		int count = 0;
		foreach (GameObject w in walls) {
			if (!walls_components.ContainsKey (w.GetComponent<WallBehaviour> ().wall_neighbours))
				walls_components.Add (w.GetComponent<WallBehaviour> ().wall_neighbours, new List<int> ());

			walls_components [w.GetComponent<WallBehaviour> ().wall_neighbours].Add (count);
			count++;
		}

		state.wall_neighbours = new List<int[]> ();
		foreach (List<GameObject> k in walls_components.Keys) {
			state.wall_neighbours.Add (walls_components [k].ToArray ());
		}

		//room_neighbours
		Dictionary<List<GameObject>,List<int>> rooms_components = new Dictionary<List<GameObject>,List<int>> ();
		count = 0;
		foreach (GameObject w in walls) {
			if (!rooms_components.ContainsKey (w.GetComponent<WallBehaviour> ().room_neighbours))
				rooms_components.Add (w.GetComponent<WallBehaviour> ().room_neighbours, new List<int> ());

			rooms_components [w.GetComponent<WallBehaviour> ().room_neighbours].Add (count);
			count++;
		}

		state.wall_room_neighbours = new List<int[]> ();
		foreach (List<GameObject> k in rooms_components.Keys) {
			state.wall_room_neighbours.Add (rooms_components [k].ToArray ());
		}

		Dictionary<List<GameObject>,List<int>> floors_components=new Dictionary<List<GameObject>,List<int>>();
		count = 0;
		foreach (GameObject f in floors) {
			if (!floors_components.ContainsKey (f.GetComponent<WallBehaviour> ().room_neighbours))
				floors_components.Add (f.GetComponent<WallBehaviour> ().room_neighbours, new List<int> ());

			floors_components [f.GetComponent<WallBehaviour> ().room_neighbours].Add (count);
			count++;
		}

		state.fc_room_neighbours = new List<int[]> ();
		foreach (List<GameObject> k in floors_components.Keys) {
			state.fc_room_neighbours.Add (floors_components [k].ToArray ());
		}

		//aux_objs
		Dictionary<GameObject,List<GameObject>> object_walls=new Dictionary<GameObject,List<GameObject>>();
		foreach (List<GameObject> k in walls_components.Keys) {
			foreach(GameObject obj in k[0].GetComponent<WallBehaviour> ().aux_objs){
				if (!object_walls.ContainsKey(obj))
					object_walls.Add (obj, k);
			}
		}

		GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");
		List<object[]> doors_data = new List<object[]> ();
		foreach (GameObject d in doors) {
			object[] door_info = {
				d.GetComponent<AuxObjectParent> ().path,
				d.transform.GetChild (0).position.x,
				d.transform.GetChild (0).position.z,
				d.transform.GetChild (0).rotation.eulerAngles.y,
				d.transform.position.x,
				d.transform.position.z,
				d.transform.rotation.eulerAngles.y,
				d.GetComponent<BoxCollider> ().center.x,
				d.GetComponent<BoxCollider> ().size.x,
				d.GetComponent<BoxCollider> ().size.z,
				d.GetComponent<DoorBehaviour> ().dir,
				d.GetComponent<DoorBehaviour> ().open,
				walls_components [object_walls [d]].ToArray ()
			};
			doors_data.Add (door_info);
		}
		state.doors_data = doors_data;

		GameObject[] windows = GameObject.FindGameObjectsWithTag("Window");
		List<object[]> windows_data = new List<object[]> ();
		foreach (GameObject w in windows) {
			object[] window_info = {
				w.GetComponent<AuxObjectParent> ().path,
				w.transform.GetChild (0).position.x,
				w.transform.GetChild (0).position.z,
				w.transform.GetChild (0).rotation.eulerAngles.y,
				w.GetComponent<BoxCollider> ().size.x,
				w.GetComponent<BoxCollider> ().size.z,
				walls_components [object_walls [w]].ToArray ()
			};
			windows_data.Add (window_info);
		}
		state.windows_data = windows_data;

		//attached_objs
		int obj_ind;
		Dictionary<GameObject, int> objects_parents = new Dictionary<GameObject, int> ();
		Dictionary<GameObject, int> objects_indexes = new Dictionary<GameObject, int> ();
		List<object[]> attached_objs = new List<object[]> ();

		count=-1;
		foreach (GameObject w in walls) {
			count++;
			obj_ind = -1;
			foreach (GameObject o in w.GetComponent<WallBehaviour>().attached_objs) {
				obj_ind++;
				objects_parents.Add (o, count);
				objects_indexes.Add (o, obj_ind);
				object[] values = {
					2, //wall
					count, //id
					o.GetComponent<ObjectBehaviour> ().path,
					o.transform.position.x,
					o.transform.position.y,
					o.transform.position.z,
					o.transform.rotation.eulerAngles.y,
				};
				attached_objs.Add (values);
			}
		}

		count=-1;
		foreach (GameObject f in floors) {
			count++;
			foreach (GameObject o in f.GetComponent<WallBehaviour>().attached_objs) {
				if (o.GetComponent<ObjectBehaviour> ().type == 1) {
					object[] values = {
						1, //floor
						count,
						o.GetComponent<ObjectBehaviour> ().type,
						o.GetComponent<ObjectBehaviour> ().path,
						o.transform.position.x,
						o.transform.position.y,
						o.transform.position.z,
						o.transform.rotation.eulerAngles.y,
					};
					attached_objs.Add (values);
				}
				else {

					object[] values = {
						1, //floor
						count, 
						o.GetComponent<ObjectBehaviour> ().type,
						objects_parents [o],
						objects_indexes [o],
					};
					attached_objs.Add (values);
				}
			}
		}

		count=-1;
		foreach (GameObject c in ceilings) {
			count++;
			foreach (GameObject o in c.GetComponent<WallBehaviour>().attached_objs) {
				if (o.GetComponent<ObjectBehaviour> ().type == 3) {
					object[] values = {
						3, //ceiling
						count,
						o.GetComponent<ObjectBehaviour> ().type,
						o.GetComponent<ObjectBehaviour> ().path,
						o.transform.position.x,
						o.transform.position.y,
						o.transform.position.z,
						o.transform.rotation.eulerAngles.y,
					};
					attached_objs.Add (values);
				}
				else {
					object[] values = {
						3, //ceiling
						count, 
						o.GetComponent<ObjectBehaviour> ().type,
						objects_parents [o],
						objects_indexes [o],
					};
					attached_objs.Add (values);
				}
			}
		}

		state.attached_objs = attached_objs;

		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath + "/saves/" + System.DateTime.Now.Day + "," +
		                  System.DateTime.Now.Month + "," + System.DateTime.Now.Hour + "," + System.DateTime.Now.Minute + ".sav");
		bf.Serialize (file, state);
		file.Close ();
	}

	public static void LoadScene(string fileName){

		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Open(Application.persistentDataPath + "/saves/" + fileName + ".sav", FileMode.Open);
		SceneState state = (SceneState)bf.Deserialize(file);
		file.Close();

		GameObject wall_prefab, door_parent_prefab, window_parent_prefab;
		wall_prefab = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().wall_prefab;
		door_parent_prefab = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().door_interaction_prefab;
		window_parent_prefab = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().window_parent;
		float height = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().height;

		//walls
		List<GameObject> walls = new List<GameObject>();
		foreach (object[] obj in state.walls) {
			GameObject new_wall = GameObject.Instantiate (wall_prefab);
			new_wall.transform.position = new Vector3 (
				(float)obj [0], 
				(float)obj [1], 
				(float)obj [2]);
			new_wall.transform.localScale = new Vector3 (
				(float)obj [3], 
				(float)obj [4], 
				(float)obj [5]);

			new_wall.GetComponent<WallBehaviour> ().set_mid = (int)obj [6];
			new_wall.GetComponent<WallBehaviour> ().current_mid = (int)obj [6];

			new_wall.GetComponent<WallBehaviour> ().destructible = (bool)obj [7];
			new_wall.tag = (string)obj [8];

			walls.Add (new_wall);
		}

		//floors
		List<GameObject> floors = new List<GameObject>();
		foreach (object[] obj in state.floors) {
			GameObject new_floor = GameObject.Instantiate (wall_prefab);
			new_floor.transform.position = new Vector3 (
				(float)obj [0], 
				(float)obj [1], 
				(float)obj [2]);
			new_floor.transform.localScale = new Vector3 (
				(float)obj [3], 
				(float)obj [4], 
				(float)obj [5]);

			new_floor.GetComponent<WallBehaviour> ().set_mid = (int)obj [6];
			new_floor.GetComponent<WallBehaviour> ().current_mid = (int)obj [6];

			new_floor.GetComponent<WallBehaviour> ().destructible = (bool)obj [7];
			new_floor.tag = (string)obj [8];

			floors.Add (new_floor);
		}

		//ceilings
		List<GameObject> ceilings = new List<GameObject>();
		foreach (object[] obj in state.ceilings) {
			GameObject new_ceiling = GameObject.Instantiate (wall_prefab);
			new_ceiling.transform.position = new Vector3 (
				(float)obj [0], 
				(float)obj [1], 
				(float)obj [2]);
			new_ceiling.transform.localScale = new Vector3 (
				(float)obj [3], 
				(float)obj [4], 
				(float)obj [5]);

			new_ceiling.GetComponent<WallBehaviour> ().set_mid = (int)obj [6];
			new_ceiling.GetComponent<WallBehaviour> ().current_mid = (int)obj [6];

			new_ceiling.GetComponent<WallBehaviour> ().destructible = (bool)obj [7];
			new_ceiling.tag = (string)obj [8];

			ceilings.Add (new_ceiling);
		}

		int count = 0;
		foreach (int twin_id in state.twins) {
			walls [count].GetComponent<WallBehaviour> ().twin = walls [twin_id];
			count++;
		}

		foreach (int[] wall_neighbours_ids in state.wall_neighbours) {
			List<GameObject> new_wall_neighbours = new List<GameObject> ();
			foreach (int nid in wall_neighbours_ids) {
				new_wall_neighbours.Add (walls [nid]);
			}
			foreach (GameObject n in new_wall_neighbours) {
				n.GetComponent<WallBehaviour> ().wall_neighbours = new_wall_neighbours;
			}
		}

		foreach (int[] room_neighbours_ids in state.wall_room_neighbours) {
			List<GameObject> new_room_neighbours = new List<GameObject> ();
			foreach (int nid in room_neighbours_ids) {
				new_room_neighbours.Add (walls [nid]);
			}
			foreach (GameObject n in new_room_neighbours) {
				n.GetComponent<WallBehaviour> ().room_neighbours = new_room_neighbours;
			}
		}

		foreach (int[] fc_neighbours_ids in state.fc_room_neighbours) {
			List<GameObject> new_floor_neighbours = new List<GameObject> ();
			List<GameObject> new_ceiling_neighbours = new List<GameObject> ();
			foreach (int nid in fc_neighbours_ids) {
				new_floor_neighbours.Add (floors [nid]);
				new_ceiling_neighbours.Add (ceilings [nid]);
			}
			foreach (GameObject n in new_floor_neighbours) {
				n.GetComponent<WallBehaviour> ().room_neighbours = new_floor_neighbours;
			}
			foreach (GameObject n in new_ceiling_neighbours) {
				n.GetComponent<WallBehaviour> ().room_neighbours = new_ceiling_neighbours;
			}

		}

		GameObject object_prefab, o, p;
		string path;
		foreach (object[] door_info in state.doors_data) {
			path = (string)door_info [0];
			object_prefab = Resources.Load (path) as GameObject;
			o = GameObject.Instantiate (object_prefab);
			o.transform.position = new Vector3 ((float)door_info [1], height * 3 / 8, (float)door_info [2]);
			o.transform.rotation = Quaternion.Euler (0, (float)door_info [3], 0);

			p = GameObject.Instantiate (door_parent_prefab);
			p.GetComponent<AuxObjectParent> ().path = path;

			p.transform.position = new Vector3 ((float)door_info [4], height * 3 / 8, (float)door_info [5]);
			p.GetComponent<BoxCollider> ().center = new Vector3 ((float)door_info [7], 0, 0);
			p.GetComponent<DoorBehaviour> ().dir = (int)door_info [10];
			p.GetComponent<DoorBehaviour> ().open = (bool)door_info [11];

			p.transform.rotation = Quaternion.Euler (0, (float)door_info [6], 0);
			p.GetComponent<BoxCollider> ().size = new Vector3 ((float)door_info [8], height * 3 / 4, (float)door_info [9]);
			o.transform.parent = p.transform;

			int[] wall_neighbours = (int[])door_info [12];
			foreach (int i in wall_neighbours) {
				walls [i].GetComponent<WallBehaviour> ().aux_objs.Add (p);
			}
		}

		foreach(object[] window_info in state.windows_data){
			path = (string)window_info [0];
			object_prefab = Resources.Load (path) as GameObject;
			o = GameObject.Instantiate (object_prefab);
			o.transform.position = new Vector3 ((float)window_info [1], height * 9 / 16, (float)window_info [2]);
			o.transform.rotation = Quaternion.Euler (0, (float)window_info [3], 0);

			p = GameObject.Instantiate (window_parent_prefab);
			p.GetComponent<AuxObjectParent> ().path = path;
			p.GetComponent<BoxCollider> ().size = new Vector3 ((float)window_info [4], height * 10 / 16, (float)window_info [5]);
			p.transform.position = o.transform.position;
			o.transform.parent = p.transform;

			int[] wall_neighbours = (int[])window_info [6];
			foreach (int i in wall_neighbours) {
				walls [i].GetComponent<WallBehaviour> ().aux_objs.Add (p);
			}
		}

		List<GameObject> parents = null;
		foreach (object[] object_info in state.attached_objs) {
			
			if ((int)object_info [0] == 1) //floor
				parents = floors;
			else if ((int)object_info [0] == 3) //ceiling
				parents = ceilings;
			else if ((int)object_info [0] == 2) //wall
				parents = walls;

			int parent_type = (int)object_info [0];
			if (parent_type == 2) {//wall
				object_prefab = Resources.Load ((string)object_info [2]) as GameObject;
				o = GameObject.Instantiate (object_prefab);
				o.transform.position = new Vector3 (
					(float)object_info [3],
					(float)object_info [4],
					(float)object_info [5]);
				o.transform.rotation = Quaternion.Euler (0, (float)object_info [6], 0);
				parents [(int)object_info [1]].GetComponent<WallBehaviour> ().attached_objs.Add (o);

				o.AddComponent<ObjectBehaviour> ();
				o.GetComponent<ObjectBehaviour> ().path = (string)object_info [2];
				o.GetComponent<ObjectBehaviour> ().type = 2;
				o.tag = "Object";
			}
			else {
				int object_type = (int)object_info [2];
				if (object_type == parent_type) {
					object_prefab = Resources.Load ((string)object_info [3]) as GameObject;
					o = GameObject.Instantiate (object_prefab);
					o.transform.position = new Vector3 (
						(float)object_info [4],
						(float)object_info [5],
						(float)object_info [6]);
					o.transform.rotation = Quaternion.Euler (0, (float)object_info [7], 0);
					parents [(int)object_info [1]].GetComponent<WallBehaviour> ().attached_objs.Add (o);

					o.AddComponent<ObjectBehaviour> ();
					o.GetComponent<ObjectBehaviour> ().path = (string)object_info [3];
					o.GetComponent<ObjectBehaviour> ().type = object_type;
					o.tag = "Object";
				}
				else {
					parents [(int)object_info [1]].GetComponent<WallBehaviour> ().attached_objs.Add (
						walls [(int)object_info [3]].GetComponent<WallBehaviour> ().attached_objs [(int)object_info [4]]);
				}
			}
		}

		//activate reticle
		GameObject.Find("GvrReticle").GetComponent<MeshRenderer>().enabled = true;
	}
}

[System.Serializable]
public class SceneState {
	public List<object[]> walls, floors, ceilings;
	public int[] twins;
	public List<int[]> wall_neighbours, wall_room_neighbours, fc_room_neighbours;
	public List<object[]> doors_data, windows_data, attached_objs;
}
