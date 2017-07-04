using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DandWPositioning : MonoBehaviour {

	[HideInInspector]
	public string path;

	private bool valid;
	private bool done;
	private float uni_scale;
	private float tier1_door_size;
	private float tier2_door_size;
	private float tier1_window_size;
	private float tier2_window_size;
	private float height;
	private Vector3 hit_normal;
	private GameObject hit_gameobject;
	private int type,tier;

	// Use this for initialization
	void Start () {
		valid = false;
		done = false;
		uni_scale = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().uni_scale;
		tier1_door_size = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().tier1_door_size;
		tier2_door_size = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().tier2_door_size;
		tier1_window_size = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().tier1_window_size;
		tier2_window_size = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().tier2_window_size;
		height = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().height;

		if (path.Contains ("Doors"))
			type = 1;
		else //contains("Windows")
			type = 2;

		if (path.Contains ("Tier 1"))
			tier = 1;
		else // contains("Tier 2")
			tier = 2;
	}

	void Invalidate(){
		valid = false;
		transform.localScale = new Vector3 (0, 0, 0);
		transform.GetChild (0).transform.position = new Vector3 (0, 0, 0);
	}

	bool CloseEnough(float a, float b){
		return Mathf.Abs (a - b) < 0.0001f;
	}

	private IEnumerator AfterCompletion(){
		//set a delay so that the user's button pressing
		//does not trigger a response from a wall or similar object
		yield return new WaitForSeconds (0.1f);
		GameObject.Find ("UtilsHolder").GetComponent<Utils> ().SetWallsState (true);
		GameObject.Find("UIMaster").GetComponent<UIMasterScript>().stop=false;

		GameObject.Find ("UIMaster").GetComponent<UIMasterScript> ().NetworkedCreateAuxObj (path, hit_gameobject,
			hit_normal, transform.GetChild(0).transform.position);

		Destroy (gameObject);
	}
	
	// Update is called once per frame
	void Update () {

		if (done)
			return;

		if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.A) && valid) {
			done = true;
			transform.GetChild(0).localPosition -= hit_normal * 0.2f;
			StartCoroutine (AfterCompletion());
			return;
		}

		if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.B)) {
			GameObject.Find("Door&WindowInserter").GetComponent<DandWInsertion>().DisplayContents(type,tier);
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

		if (hit.transform.tag == "Wall" && hit.transform.position.y==height/2) {
			//check the wall first

			float wall_width;
			transform.localScale = new Vector3 (1, 1, 1);
			if (hit.normal.x == -1) {
				child_transform.rotation = Quaternion.Euler (0, 270, 0);
				wall_width = hit.transform.localScale.z;
			}
			else if (hit.normal.x == 1) {
				child_transform.rotation = Quaternion.Euler (0, 90, 0);
				wall_width = hit.transform.localScale.z;
			}
			else if (hit.normal.z == 1) {
				child_transform.rotation = Quaternion.Euler (0, 0, 0);
				wall_width = hit.transform.localScale.x;
			}
			else if (hit.normal.z == -1) {
				child_transform.rotation = Quaternion.Euler (0, 180, 0);
				wall_width = hit.transform.localScale.x;
			}
			else {
				Invalidate ();
				return;
			}

			child_transform.localPosition = new Vector3 (hit.point.x, 0, hit.point.z);
			child_transform.localPosition += hit.normal * 0.2f;

			float obj_width;
			if (type == 1) {
				if (tier == 1)
				obj_width = tier1_door_size * 2 * uni_scale;
				else // tier == 2
				obj_width = tier2_door_size * 2 * uni_scale;
			}
			else { //type==2
				if (tier == 1)
					obj_width = tier1_window_size * 2 * uni_scale;
				else // tier == 2
					obj_width = tier2_window_size * 2 * uni_scale;
			}
			//invalidate if the door/window cannot fit on the wall
			if (wall_width-0.5f <= obj_width) {
				Invalidate ();
				return;
			}

			//check if the object goes outside its boundaries
			bool is_min, is_max;
			is_min = is_max = true;
			foreach (GameObject n in hit.transform.gameObject.GetComponent<WallBehaviour>().room_neighbours) {
				if (hit.normal.x != 0 && n.transform.localScale.x == 0.2f &&
					CloseEnough (n.transform.position.x, hit.transform.position.x)) {
					if (n.transform.position.z < hit.transform.position.z)
						is_min = false;
					if (n.transform.position.z > hit.transform.position.z)
						is_max = false;
				}
				else if (hit.normal.z != 0 && n.transform.localScale.z == 0.2f &&
					CloseEnough (n.transform.position.z, hit.transform.position.z)){
					if (n.transform.position.x < hit.transform.position.x)
						is_min = false;
					if (n.transform.position.x > hit.transform.position.x)
						is_max = false;
				}
			}

			float lat_min, lat_max;
			if (hit.normal.x != 0) {
				lat_min = hit.transform.position.z - hit.transform.localScale.z / 2 + 0.25f;
				lat_max = hit.transform.position.z + hit.transform.localScale.z / 2 - 0.25f;
			}
			else {// hit.normal.z != 0
				lat_min = hit.transform.position.x - hit.transform.localScale.x / 2 + 0.25f;
				lat_max = hit.transform.position.x + hit.transform.localScale.x / 2 - 0.25f;
			}

			if (is_max) 
				lat_max -= 0.2f;
			if (is_min)
				lat_min += 0.2f;

			float obj_lat_min, obj_lat_max,obj_len_min,obj_len_max;
			if (hit.normal.x != 0) {
				obj_lat_min = hit.point.z - obj_width/2;
				obj_lat_max = hit.point.z + obj_width/2;
			}
			else {// hit.normal.z != 0
				obj_lat_min = hit.point.x - obj_width/2;
				obj_lat_max = hit.point.x + obj_width/2;
			}
				
			if (type == 1) {
				obj_len_min = 0;
				obj_len_max = height * 3 / 4;
			}
			else { //type=2
				obj_len_min = height * 4 / 16;
				obj_len_max = height * 14 / 16;
			}

			//we use this bools to know if the object had to be repositioned
			//on one side or the other
			bool flag_left, flag_right;
			flag_left = flag_right = false;
			if (obj_lat_min < lat_min) {
				flag_left = true;
				if (hit.normal.x != 0)
					child_transform.position -= new Vector3 (0, 0, 1) * (obj_lat_min - lat_min);
				else // hit.normal.z != 0
					child_transform.position -= new Vector3 (1, 0, 0) * (obj_lat_min - lat_min);
			}
			else if (obj_lat_max > lat_max) {
				flag_right = true;
				if (hit.normal.x != 0)
					child_transform.position -= new Vector3 (0, 0, 1) * (obj_lat_max - lat_max);
				else // hit.normal.z != 0
					child_transform.position -= new Vector3 (1, 0, 0) * (obj_lat_max - lat_max);
			}

			//recompute object's boundaries if position was changed
			if (flag_left || flag_right) {
				if (hit.normal.x != 0) {
					obj_lat_min = child_transform.position.z - obj_width/2;
					obj_lat_max = child_transform.position.z + obj_width/2;
				}
				else {// hit.normal.z != 0
					obj_lat_min = child_transform.position.x - obj_width/2;
					obj_lat_max = child_transform.position.x + obj_width/2;
				}
			}

			//check if the object overlaps with an attached_obj
			List<GameObject> attached_objs = new List<GameObject>();
			foreach (GameObject n in hit.transform.gameObject.GetComponent<WallBehaviour>().room_neighbours) {
				if ((hit.normal.x != 0 && n.transform.localScale.x == 0.2f &&
				    CloseEnough (n.transform.position.x, hit.transform.position.x)) ||
				    (hit.normal.z != 0 && n.transform.localScale.z == 0.2f &&
				    CloseEnough (n.transform.position.z, hit.transform.position.z))) {
					
					foreach (GameObject o in n.GetComponent<WallBehaviour>().attached_objs) {
						attached_objs.Add (o);
					}
				}
			}

			float len_min, len_max;
			bool changed = false;
			foreach (GameObject o in attached_objs) {
				
				if (hit.normal.x != 0) {
					lat_min = o.transform.position.z - Mathf.Abs(o.transform.right.z) * o.GetComponent<BoxCollider>().size.x / 2 +
						o.transform.right.z * o.GetComponent<BoxCollider> ().center.x;
					lat_max = o.transform.position.z + Mathf.Abs(o.transform.right.z) * o.GetComponent<BoxCollider>().size.x / 2 +
						o.transform.right.z * o.GetComponent<BoxCollider> ().center.x;
				}
				else {// hit.normal.z != 0
					lat_min = o.transform.position.x - Mathf.Abs(o.transform.right.x) * o.GetComponent<BoxCollider>().size.x / 2 +
						o.transform.right.x * o.GetComponent<BoxCollider> ().center.x;
					lat_max = o.transform.position.x + Mathf.Abs(o.transform.right.x) * o.GetComponent<BoxCollider>().size.x / 2 +
						o.transform.right.x * o.GetComponent<BoxCollider> ().center.x;
				}

				len_min = o.transform.position.y - o.GetComponent<BoxCollider> ().size.y / 2;
				len_max = o.transform.position.y + o.GetComponent<BoxCollider> ().size.y / 2;

				if ((obj_len_min >= len_min && obj_len_min <= len_max) || (obj_len_max >= len_min && obj_len_max <= len_max) ||
					(obj_len_min <= len_min && obj_len_min <= len_min && obj_len_max >= len_max && obj_len_max >= len_max)) {

					//in case the new object is contained by the existing one
					//then it has to be invalidated
					if (obj_lat_min >= lat_min && obj_lat_max >= lat_min && obj_lat_min <= lat_max && obj_lat_max <= lat_max) {
						Invalidate ();
						return;
					}


					if (obj_lat_min <= lat_min && obj_lat_min <= lat_max && obj_lat_max >= lat_min && obj_lat_max >= lat_max) {
						float left_dist = obj_lat_max - lat_min, right_dist = lat_max - obj_lat_min;
						if (left_dist <= right_dist && !flag_left) {
							changed = true;
							if (hit.normal.x != 0)
								child_transform.position -= new Vector3 (0, 0, 1) * left_dist;
							else // hit.normal.z != 0
								child_transform.position -= new Vector3 (1, 0, 0) * left_dist;
						}
						else if (left_dist >= right_dist && !flag_right) {
							changed = true;
							if (hit.normal.x != 0)
								child_transform.position += new Vector3 (0, 0, 1) * right_dist;
							else // hit.normal.z != 0
								child_transform.position += new Vector3 (1, 0, 0) * right_dist;
						}
						else {
							Invalidate ();
							return;
						}
					}
					else if (obj_lat_min >= lat_min && obj_lat_min <= lat_max) {
						if (flag_right) {
							Invalidate ();
							return;
						}
						changed = true;
						if (hit.normal.x != 0)
							child_transform.position -= new Vector3 (0, 0, 1) * (obj_lat_min - lat_max);
						else // hit.normal.z != 0
							child_transform.position -= new Vector3 (1, 0, 0) * (obj_lat_min - lat_max);
					}
					else if (obj_lat_max >= lat_min && obj_lat_max <= lat_max) {
						if (flag_left) {
							Invalidate ();
							return;
						}
						changed = true;
						if (hit.normal.x != 0)
							child_transform.position -= new Vector3 (0, 0, 1) * (obj_lat_max - lat_min);
						else // hit.normal.z != 0
							child_transform.position -= new Vector3 (1, 0, 0) * (obj_lat_max - lat_min);
					}
				}
			}

			if (changed) {
				if (hit.normal.x != 0) {
					obj_lat_min = child_transform.position.z - obj_width / 2;
					obj_lat_max = child_transform.position.z + obj_width / 2;
				}
				else {// hit.normal.z != 0
					obj_lat_min = child_transform.position.x - obj_width / 2;
					obj_lat_max = child_transform.position.x + obj_width / 2;
				}
			}

			foreach (GameObject o in attached_objs) {

				if (hit.normal.x != 0) {
					lat_min = o.transform.position.z - Mathf.Abs(o.transform.right.z) * o.GetComponent<BoxCollider>().size.x / 2 +
						o.transform.right.z * o.GetComponent<BoxCollider> ().center.x;
					lat_max = o.transform.position.z + Mathf.Abs(o.transform.right.z) * o.GetComponent<BoxCollider>().size.x / 2 +
						o.transform.right.z * o.GetComponent<BoxCollider> ().center.x;
				}
				else {// hit.normal.z != 0
					lat_min = o.transform.position.x - Mathf.Abs(o.transform.right.x) * o.GetComponent<BoxCollider>().size.x / 2 +
						o.transform.right.x * o.GetComponent<BoxCollider> ().center.x;
					lat_max = o.transform.position.x + Mathf.Abs(o.transform.right.x) * o.GetComponent<BoxCollider>().size.x / 2 +
						o.transform.right.x * o.GetComponent<BoxCollider> ().center.x;
				}

				len_min = o.transform.position.y - o.GetComponent<BoxCollider> ().size.y / 2;
				len_max = o.transform.position.y + o.GetComponent<BoxCollider> ().size.y / 2;

				if ((obj_len_min >= len_min && obj_len_min <= len_max) || (obj_len_max >= len_min && obj_len_max <= len_max) ||
					(obj_len_min <= len_min && obj_len_min <= len_min && obj_len_max >= len_max && obj_len_max >= len_max)) {


					if ((obj_lat_min > lat_min && obj_lat_min < lat_max) || (obj_lat_max > lat_min && obj_lat_max < lat_max) ||
						(obj_lat_min > lat_min && obj_lat_max > lat_min && obj_lat_min < lat_max && obj_lat_max < lat_max)) {
						Invalidate ();
						return;
					}
				}
			}

			//verifica din nou daca iese in afara
			if (hit.normal.x != 0) {
				lat_min = hit.transform.position.z - hit.transform.localScale.z / 2 + 0.25f;
				lat_max = hit.transform.position.z + hit.transform.localScale.z / 2 - 0.25f;
			}
			else {// hit.normal.z != 0
				lat_min = hit.transform.position.x - hit.transform.localScale.x / 2 + 0.25f;
				lat_max = hit.transform.position.x + hit.transform.localScale.x / 2 - 0.25f;
			}

			if (is_max) 
				lat_max -= 0.2f;
			if (is_min)
				lat_min += 0.2f;

			if ((obj_lat_min < lat_min) || (obj_lat_max > lat_max)) {
				Invalidate ();
				return;
			}

			valid = true;
			hit_normal = hit.normal;
			hit_gameobject = hit.transform.gameObject;

		}
		else {
			Invalidate ();
		}
	}
}
