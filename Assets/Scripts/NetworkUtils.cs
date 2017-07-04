using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkUtils : MonoBehaviour {

	private const string roomName = "InteriorDesignVR";
	[HideInInspector]
	public bool connected=false;

	public void StartNetwork () {
		PhotonNetwork.OnEventCall += MyEvent;
		PhotonNetwork.ConnectUsingSettings("0.1");
		StartCoroutine (WaitForConnection ());
		connected = true;


	}

	IEnumerator WaitForConnection(){
		while (!PhotonNetwork.connected)
			yield return new WaitForSeconds (1);

		//display connection choices
		GameObject.Find("UIMaster").GetComponent<UIMasterScript>().DisplayConnections();
	}

	void OnJoinedRoom() {
		print ("joined room");
	}

	void OnPhotonPlayerConnected(){

		//the new client must be given the scene's current configuration
		print ("new player");//--------------------vezi daca chestia asta se triggeruieste si pe alti clienti decat master-ul

		if (!PhotonNetwork.isMasterClient)
			return;

		GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");
		//first send the walls' transforms and the id of their current material
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
			PhotonNetwork.RaiseEvent (0, values, true, null);
		}

		//now start sending the relationships between walls
		//start with twins
		int count = 0;
		foreach (GameObject w in walls) {
			int[] indexes = { count, System.Array.IndexOf (walls, w.GetComponent<WallBehaviour> ().twin) };
			PhotonNetwork.RaiseEvent (1, indexes, true, null);
			count++;
		}

		//then send the room_neighbours
		Dictionary<List<GameObject>,List<int>> rooms_components=new Dictionary<List<GameObject>,List<int>>();
		count = 0;
		foreach (GameObject w in walls) {
			if (!rooms_components.ContainsKey (w.GetComponent<WallBehaviour> ().room_neighbours))
				rooms_components.Add (w.GetComponent<WallBehaviour> ().room_neighbours, new List<int> ());

			rooms_components [w.GetComponent<WallBehaviour> ().room_neighbours].Add (count);
			count++;
		}
		foreach (List<GameObject> k in rooms_components.Keys) {
			object[] values = {
				true, //wall
				rooms_components [k].ToArray ()
			};
			PhotonNetwork.RaiseEvent (2, values, true, null);
		}

		//and finally wall_neighbours
		Dictionary<List<GameObject>,List<int>> walls_components=new Dictionary<List<GameObject>,List<int>>();
		count = 0;
		foreach (GameObject w in walls) {
			if (!walls_components.ContainsKey (w.GetComponent<WallBehaviour> ().wall_neighbours))
				walls_components.Add (w.GetComponent<WallBehaviour> ().wall_neighbours, new List<int> ());

			walls_components [w.GetComponent<WallBehaviour> ().wall_neighbours].Add (count);
			count++;
		}
		foreach (List<GameObject> k in walls_components.Keys) {
			PhotonNetwork.RaiseEvent (3, walls_components[k].ToArray(), true, null);
		}

		//after, we also need to send the aux_objs and attached_objs
		//corespondenta intre fiecare aux_obj si lista de wall_neighbours la care este atasat
		Dictionary<GameObject,List<GameObject>> object_walls=new Dictionary<GameObject,List<GameObject>>();
		foreach (List<GameObject> k in walls_components.Keys) {
			foreach(GameObject obj in k[0].GetComponent<WallBehaviour> ().aux_objs){
				if (!object_walls.ContainsKey(obj))//each aux_obj will appear in two sets of wall_neighbours
					object_walls.Add (obj, k);
			}
		}

		GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");
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
			PhotonNetwork.RaiseEvent (4, door_info, true, null);
		}
		GameObject[] windows = GameObject.FindGameObjectsWithTag("Window");
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
			PhotonNetwork.RaiseEvent (4, window_info, true, null);
		}

		//send the floors and ceilings before starting to send attached_objs
		GameObject[] floors = GameObject.FindGameObjectsWithTag ("Floor");
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
			PhotonNetwork.RaiseEvent (0, values, true, null);
		}
		GameObject[] ceilings = GameObject.FindGameObjectsWithTag ("Ceiling");
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
			PhotonNetwork.RaiseEvent (0, values, true, null);
		}

		//and their room_neighbours
		//they don't have wall_neighbours or twins
		Dictionary<List<GameObject>,List<int>> floors_components=new Dictionary<List<GameObject>,List<int>>();
		count = 0;
		foreach (GameObject f in floors) {
			if (!floors_components.ContainsKey (f.GetComponent<WallBehaviour> ().room_neighbours))
				floors_components.Add (f.GetComponent<WallBehaviour> ().room_neighbours, new List<int> ());

			floors_components [f.GetComponent<WallBehaviour> ().room_neighbours].Add (count);
			count++;
		}
		//floors and ceilings list are always identical so the same
		//indices can be used for both
		foreach (List<GameObject> k in floors_components.Keys) {
			object[] values = {
				false, //floor and ceiling
				floors_components [k].ToArray ()
			};
			PhotonNetwork.RaiseEvent (2, values, true, null);
		}

		//now send the attached_objs for each wall/floor/ceiling
		int obj_ind;
		Dictionary<GameObject, int> objects_parents = new Dictionary<GameObject, int> ();
		Dictionary<GameObject, int> objects_indexes = new Dictionary<GameObject, int> ();

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
				PhotonNetwork.RaiseEvent (5, values, true, null);
			}
		}

		count=-1;
		foreach (GameObject f in floors) {
			count++;
			foreach (GameObject o in f.GetComponent<WallBehaviour>().attached_objs) {
				//if the object is attached to this floor, and not to a wall
				//a wall that is in the same room
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
					PhotonNetwork.RaiseEvent (5, values, true, null);
				}
				else {
					//else send the actual parent of the object,
					//which is a wall and also the index of the object
					//in the parent's list of attached_objects
					object[] values = {
						1, //floor
						count, 
						o.GetComponent<ObjectBehaviour> ().type,
						objects_parents [o],
						objects_indexes [o],
					};
					PhotonNetwork.RaiseEvent (5, values, true, null);
				}
			}
		}

		count=-1;
		foreach (GameObject c in ceilings) {
			count++;
			foreach (GameObject o in c.GetComponent<WallBehaviour>().attached_objs) {
				//if the object is attached to this ceiling, and not to a wall
				//a wall that is in the same room
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
					PhotonNetwork.RaiseEvent (5, values, true, null);
				}
				else {
					//else send the actual parent of the object,
					//which is a wall and also the index of the object
					//in the parent's list of attached_objects
					object[] values = {
						3, //ceiling
						count, 
						o.GetComponent<ObjectBehaviour> ().type,
						objects_parents [o],
						objects_indexes [o],
					};
					PhotonNetwork.RaiseEvent (5, values, true, null);
				}
			}
		}
	}

	private void MyEvent(byte eventcode, object content, int senderid){
		
		if (eventcode == 0) { //walls
			//PhotonPlayer sender = PhotonPlayer.Find (senderid);  // who sent this?
			object[] values = (object[])content;
			GameObject wall_prefab = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().wall_prefab;
			GameObject le_wall = GameObject.Instantiate (wall_prefab);
			le_wall.transform.position = new Vector3 (
				(float)values [0], 
				(float)values [1], 
				(float)values [2]);
			le_wall.transform.localScale = new Vector3 (
				(float)values [3], 
				(float)values [4], 
				(float)values [5]);

			le_wall.GetComponent<WallBehaviour> ().set_mid = (int)values [6];
			le_wall.GetComponent<WallBehaviour> ().current_mid = (int)values [6];

			le_wall.GetComponent<WallBehaviour> ().destructible = (bool)values [7];
			le_wall.tag = (string)values [8];
		}
		//at this point we assume that walls have been transfered to the client
		else if (eventcode == 1) { //twins
			GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");
			int[] values = (int[])content;
			walls [values [0]].GetComponent<WallBehaviour> ().twin = walls [values [1]];
		}
		else if (eventcode == 2) { //room_neighbours
			object[] values = (object[])content;

			bool gotWall = (bool)values [0];
			if (gotWall) {
				GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");
				List<GameObject> room_neighbours = new List<GameObject> ();
				int[] ids = (int[])values [1];
				foreach (int v in ids) {
					room_neighbours.Add (walls [v]);
				}
				foreach (int v in ids) {
					walls [v].GetComponent<WallBehaviour> ().room_neighbours = room_neighbours;
				}
			}
			else {
				GameObject[] floors = GameObject.FindGameObjectsWithTag ("Floor");
				List<GameObject> floor_room_neighbours = new List<GameObject> ();
				int[] fids = (int[])values [1];
				foreach (int v in fids) {
					floor_room_neighbours.Add (floors [v]);
				}
				foreach (int v in fids) {
					floors [v].GetComponent<WallBehaviour> ().room_neighbours = floor_room_neighbours;
				}

				GameObject[] ceilings = GameObject.FindGameObjectsWithTag ("Ceiling");
				List<GameObject> ceiling_room_neighbours = new List<GameObject> ();
				int[] cids = (int[])values [1];
				foreach (int v in cids) {
					ceiling_room_neighbours.Add (ceilings [v]);
				}
				foreach (int v in cids) {
					ceilings [v].GetComponent<WallBehaviour> ().room_neighbours = ceiling_room_neighbours;
				}
			}
		}
		else if (eventcode == 3) { //wall_neighbours
			GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");
			int[] values = (int[])content;
			List<GameObject> wall_neighbours = new List<GameObject> ();
			foreach (int v in values) {
				wall_neighbours.Add (walls [v]);
			}
			foreach (int v in values) {
				walls [v].GetComponent<WallBehaviour> ().wall_neighbours = wall_neighbours;
			}
		}
		else if (eventcode == 4) {//aux_objs
			object[] values = (object[])content;

			string path = (string)values [0];
			int type;

			if (path.Contains ("Doors"))
				type = 1;
			else //contains("Windows")
				type = 2;

			float height = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().height;
			GameObject object_prefab, o, p;
			if (type == 1) {//type==1 -> door
				object_prefab = Resources.Load (path) as GameObject;
				o = Instantiate (object_prefab);
				o.transform.position = new Vector3 ((float)values [1], height * 3 / 8, (float)values [2]);
				o.transform.rotation = Quaternion.Euler (0, (float)values [3], 0);

				GameObject door_parent_prefab = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().door_interaction_prefab;
				p = GameObject.Instantiate (door_parent_prefab);
				p.GetComponent<AuxObjectParent> ().path = path;

				p.transform.position = new Vector3 ((float)values [4], height * 3 / 8, (float)values [5]);
				p.GetComponent<BoxCollider> ().center = new Vector3 ((float)values [7], 0, 0);
				p.GetComponent<DoorBehaviour> ().dir = (int)values [10];
				p.GetComponent<DoorBehaviour> ().open = (bool)values [11];

				p.transform.rotation = Quaternion.Euler (0, (float)values [6], 0);
				p.GetComponent<BoxCollider> ().size = new Vector3 ((float)values [8], height * 3 / 4, (float)values [9]);
				o.transform.parent = p.transform;

				int[] wall_neighbours = (int[])values [12];
				GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");
				foreach (int i in wall_neighbours) {
					walls [i].GetComponent<WallBehaviour> ().aux_objs.Add (p);
				}
			}
			else if (type == 2) {//type==2 -> window
				object_prefab = Resources.Load (path) as GameObject;
				o = Instantiate (object_prefab);
				o.transform.position = new Vector3 ((float)values [1], height * 9 / 16, (float)values [2]);
				o.transform.rotation = Quaternion.Euler (0, (float)values [3], 0);

				GameObject window_parent_prefab = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().window_parent;
				p = GameObject.Instantiate (window_parent_prefab);
				p.GetComponent<AuxObjectParent> ().path = path;
				p.GetComponent<BoxCollider> ().size = new Vector3 ((float)values [4], height * 10 / 16, (float)values [5]);
				p.transform.position = o.transform.position;
				o.transform.parent = p.transform;

				int[] wall_neighbours = (int[])values [6];
				GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");
				foreach (int i in wall_neighbours) {
					walls [i].GetComponent<WallBehaviour> ().aux_objs.Add (p);
				}
			}
		}
		else if (eventcode == 5) {//attached_objs
			object[] values = (object[])content;
			GameObject[] parents = null;
			if ((int)values [0] == 1) //floor
				parents = GameObject.FindGameObjectsWithTag ("Floor");
			else if ((int)values [0] == 3) //ceiling
				parents = GameObject.FindGameObjectsWithTag ("Ceiling");
			else if ((int)values [0] == 2) //wall
				parents = GameObject.FindGameObjectsWithTag ("Wall");
			
			int parent_type = (int)values [0];
			if (parent_type == 2) {//wall
				GameObject object_prefab = Resources.Load ((string)values [2]) as GameObject;
				GameObject o = Instantiate (object_prefab);
				o.transform.position = new Vector3 (
					(float)values [3],
					(float)values [4],
					(float)values [5]);
				o.transform.rotation = Quaternion.Euler (0, (float)values [6], 0);
				parents [(int)values [1]].GetComponent<WallBehaviour> ().attached_objs.Add (o);

				o.AddComponent<ObjectBehaviour> ();
				o.GetComponent<ObjectBehaviour> ().path = (string)values [2];
				o.GetComponent<ObjectBehaviour> ().type = 2;
				o.tag = "Object";
			}
			else {
				int object_type = (int)values [2];
				if (object_type == parent_type) {
					GameObject object_prefab = Resources.Load ((string)values [3]) as GameObject;
					GameObject o = Instantiate (object_prefab);
					o.transform.position = new Vector3 (
						(float)values [4],
						(float)values [5],
						(float)values [6]);
					o.transform.rotation = Quaternion.Euler (0, (float)values [7], 0);
					parents [(int)values [1]].GetComponent<WallBehaviour> ().attached_objs.Add (o);

					o.AddComponent<ObjectBehaviour> ();
					o.GetComponent<ObjectBehaviour> ().path = (string)values [3];
					o.GetComponent<ObjectBehaviour> ().type = object_type;
					o.tag = "Object";
				}
				else {
					GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");
					parents [(int)values [1]].GetComponent<WallBehaviour> ().attached_objs.Add (
						walls [(int)values [3]].GetComponent<WallBehaviour> ().attached_objs [(int)values [4]]);
				}
			}
		}
		else if (eventcode == 6) { //ChangeMaterial
			object[] values = (object[])content;
			GameObject[] gobjs = GameObject.FindGameObjectsWithTag ((string)values [0]);
			gobjs [(int)values [1]].GetComponent<WallBehaviour> ().ChangeMaterial ((int)values [2]);
		}
		else if (eventcode == 7) { //CancelChangeMaterial
			object[] values = (object[])content;
			GameObject[] gobjs = GameObject.FindGameObjectsWithTag ((string)values [0]);
			gobjs [(int)values [1]].GetComponent<WallBehaviour> ().CancelChangeMaterial ();
		}
		else if (eventcode == 8) { //ConfirmChangeMaterial
			object[] values = (object[])content;
			GameObject[] gobjs = GameObject.FindGameObjectsWithTag ((string)values [0]);
			gobjs [(int)values [1]].GetComponent<WallBehaviour> ().ConfirmChangeMaterial ();
		}
		else if (eventcode == 9) { //DeleteWall
			int[] values = (int[])content;
			GameObject[] walls = GameObject.FindGameObjectsWithTag ("Wall");//only walls can be destroyed
			walls [values [0]].GetComponent<WallBehaviour> ().DeleteSelf ();
		}
		else if (eventcode == 10) { //CreateWall
			object[] values = (object[])content;
			GameObject.Find ("WallCreator").GetComponent<WallCreation> ().CreateWall (values);
		}
		else if (eventcode == 11) { //CreateAuxObj
			object[] values = (object[])content;
			GameObject.Find ("UIMaster").GetComponent<UIMasterScript> ().CreateAuxObj (values);
		}
		else if (eventcode == 12) { //Door&WindowDestroy 
			int[] values = (int[])content;
			GameObject.Find ("Door&WindowDestroyer").GetComponent<DandWDestroy> ().DWDestroy (values);
		}
		else if (eventcode == 13) { //object deletion/repositioning
			int[] values = (int[])content;
			GameObject[] objects = GameObject.FindGameObjectsWithTag ("Object");
			objects [values [0]].GetComponent<ObjectBehaviour> ().ObjectDeletion ();
		}
	}
}
