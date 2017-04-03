using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class LoadScene : MonoBehaviour {

	public class Door{
		public float x_length,z_length;
		public float x_pos,z_pos;
		public int orientation;// pe ce directie se deschide usa; 1-> x, 2->z
		public int hinge_pos;//pe ce directie este hinge-ul 1->sus/dreapta(+), 2->jos/stanga(-)
	};

	public class Window{
		public float x_pos,z_pos;
		public float length;
		public int orientation;// pe ce directie s-ar deschide geamul; 1-> x, 2->z
		//public int tier;
	}

	public class Wall{
		public float x_length,z_length;
		public float x_pos,z_pos;
		public int orientation;// 1-> +0.1x, 2-> +0.1z, 3-> -0.1x, 4-> -0.1z

		//acelasi perete ar putea aparea in doua 2 fete distincte
		public Wall twin_wall;
		public GameObject gameObject;

		public bool with_door;
		public Door d;

		public bool with_window;
		public Window w;

		public Wall(){
			x_pos=z_pos=x_length=z_length=orientation=0;

			with_door=false;
			with_window=false;
			d=null;
			twin_wall=null;
			gameObject=null;
		}
	};

	public float uni_scale;
	public float height;

	public GameObject wall_prefab;
	public GameObject door_body_prefab;
	public GameObject door_interaction_prefab;
	public GameObject window_prefab;


	private string fileName="floorplan.obj";
	private string floorplan_info="";
	private float original_height=3;
	private float original_uni_scale=5;
	private float wall_thickness = 0.2f;
	private float door_scale=1.3f;
	private float door_unit_size=0.25f;//spune care este lungimea usii din prefab si trebuie sa ma raportez la ea cand setez scale-ul unei usi noi

	IEnumerator GetFloorplanData(string filePath) {
		if (filePath.Contains("://")) {
			WWW www = new WWW(filePath);
			yield return www;
			floorplan_info = www.text;
		} else
			floorplan_info = System.IO.File.ReadAllText(filePath);
	}

	IEnumerator GenerateHouse(){
		string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
		yield return StartCoroutine(GetFloorplanData(filePath));

		string[] lines = floorplan_info.Split("\n"[0]);
		List<Vector3> positions = new List<Vector3> ();
		List<List<Wall>> rooms = new List<List<Wall>> ();
		List<Door> doors = new List<Door> ();
		List<List<int>> valid_points = new List<List<int>> ();
		List<List<int>> window_points = new List<List<int>> ();//keep them separate so they don't end up as rooms
		List<bool> has_window= new List<bool>();
		int max_valid_point=-1;//store the number of points in the given floorplan

		foreach (string l in lines)
		{	
			//string l = r.ReadLine( );
			//Debug.Log(l);
			string[] tokens = l.Split(" "[0]);
			if(tokens.Length==0) continue;
			if(tokens[0].Length==0) continue;
			if(tokens[0][0]=='#') continue;
			if (/*tokens.Length>3 && */tokens [0] == "v") {
				//float x=Mathf.Abs (float.Parse (tokens [1]));
				//float y=Mathf.Abs (float.Parse (tokens [2]));
				//float z=Mathf.Abs (float.Parse (tokens [3]));

				//if (x < 1.0f / 10000) x = 0;
				//if (y < 1.0f / 10000) y = 0;
				//if (z < 1.0f / 10000) z = 0;
				positions.Add (new Vector3 (float.Parse (tokens [1]),float.Parse (tokens [2]),float.Parse (tokens [3])));
			}
			else if (tokens [0] == "f") {
				//analizez punctele fetei si determin daca este o usa sau nu
				//conditia ca sa nu fie usa este sa contina numai unghiuri drepte

				List<int> in_points = new List<int> ();
				List<int> aux_points = new List<int> ();
				for (int i = 1; i < tokens.Length - 1; i++) {
					string s = tokens [i].Split ("/" [0]) [0];
					int p=int.Parse(s)-1;
					in_points.Add (p);
					if (max_valid_point < p)
						max_valid_point = p;
				}

				Vector2 d1, d2;
				d1 = new Vector2 (positions[in_points [0]].x,positions[in_points [0]].z) - new Vector2 (positions[in_points [in_points.Count-1]].x,positions[in_points [in_points.Count-1]].z) ;
				d2 = new Vector2 (positions[in_points [1]].x,positions[in_points [1]].z) - new Vector2 (positions[in_points [0]].x,positions[in_points [0]].z) ;
				if ((d1.x != 0 && d1.y == 0 && d2.x == 0 && d2.y != 0) ||
					(d1.x == 0 && d1.y != 0 && d2.x != 0 && d2.y == 0) ||
					(d1.x == 0 && d1.y!=0 && d2.x==0 && d2.y!=0) ||
					(d1.x!=0 && d1.y==0 && d2.x!=0 && d2.y==0))
					aux_points.Add (in_points[0]);

				for (int i = 1; i < in_points.Count - 1; i++) {
					d1 = new Vector2 (positions[in_points [i]].x,positions[in_points [i]].z) - new Vector2 (positions[in_points [i-1]].x,positions[in_points [i-1]].z) ;
					d2 = new Vector2 (positions[in_points [i+1]].x,positions[in_points [i+1]].z) - new Vector2 (positions[in_points [i]].x,positions[in_points [i]].z) ;
					if ((d1.x != 0 && d1.y == 0 && d2.x == 0 && d2.y != 0) ||
						(d1.x == 0 && d1.y != 0 && d2.x != 0 && d2.y == 0) ||
						(d1.x == 0 && d1.y!=0 && d2.x==0 && d2.y!=0) ||
						(d1.x!=0 && d1.y==0 && d2.x!=0 && d2.y==0))
						aux_points.Add (in_points[i]);
				}

				d1 = new Vector2 (positions[in_points [in_points.Count-1]].x,positions[in_points [in_points.Count-1]].z) - new Vector2 (positions[in_points [in_points.Count-2]].x,positions[in_points [in_points.Count-2]].z) ;
				d2 = new Vector2 (positions[in_points [0]].x,positions[in_points [0]].z) - new Vector2 (positions[in_points [in_points.Count-1]].x,positions[in_points [in_points.Count-1]].z) ;
				if ((d1.x != 0 && d1.y == 0 && d2.x == 0 && d2.y != 0) ||
					(d1.x == 0 && d1.y != 0 && d2.x != 0 && d2.y == 0) ||
					(d1.x == 0 && d1.y!=0 && d2.x==0 && d2.y!=0) ||
					(d1.x!=0 && d1.y==0 && d2.x!=0 && d2.y==0))
					aux_points.Add (in_points[in_points.Count-1]);

				if (aux_points.Count == 1) {
					//this is the case of a door
					//we have to add the other 2 points that form the 90 degree angle 
					//because they are imperative to knowing where to put the door
					int p=in_points.IndexOf(aux_points[0]);
					if (p == 0) aux_points.Add (in_points [in_points.Count - 1]);
					else aux_points.Add (in_points [p - 1]);

					if (p==in_points.Count - 1) aux_points.Add (in_points [0]);
					else aux_points.Add (in_points [p + 1]);
				}

				//un window va avea exact 4 unghiuri drepte pentru ca nu are atasata o usa
				if (in_points.Count == aux_points.Count && aux_points.Count == 4) window_points.Add (aux_points);
				else valid_points.Add (aux_points);
			}
		}

		//remove the points from all rooms that actually belong to a door
		for (int i = 0; i < valid_points.Count; i++) {
			if (valid_points [i].Count == 3) {
				for (int k = 0; k < valid_points[i].Count; k++) {
					for (int j = 0; j < valid_points.Count; j++) {
						if (valid_points [j].Count > 3)
							for (int l = 0; l < valid_points [j].Count; l++) {
								if (valid_points [i] [k] == valid_points [j] [l]) {
									valid_points [j].RemoveAt (l);
									l--;
								}
							}
					}
				}
			}
		}
			
		//remove the points from all rooms that actually belong to a window
		//these are also the relevant points of the window
		for (int l = 0; l < window_points.Count; l++) {
			for (int k = 0; k < window_points[l].Count; k++) {
				bool used = false;
				for (int i = 0; i < valid_points.Count; i++) {
						if (valid_points [i].Count > 3)
							for (int j = 0; j < valid_points [i].Count; j++) {
								if (valid_points [i] [j] ==  window_points [l] [k]) {
									used = true;
									valid_points [i].RemoveAt (j);
									j--;
								}
							}
				}
				if (!used) {
					window_points [l].RemoveAt (k);
					k--;
				}
			}
		}
			
		//fetele care au 3 puncte sunt usi, iar cele cu >3 sunt camere
		for (int i = 0; i < valid_points.Count; i++) {
			if (valid_points [i].Count > 3) {
				List<Wall> room = new List<Wall> ();
				for (int j = 0; j < valid_points [i].Count; j++) {
					Wall w = new Wall ();
					if (j < valid_points [i].Count - 1) {
						w.x_pos = (positions [valid_points [i] [j]].x + positions [valid_points [i] [j + 1]].x) / 2;
						w.z_pos = (positions [valid_points [i] [j]].z + positions [valid_points [i] [j + 1]].z) / 2;
						w.x_length = Mathf.Abs (positions [valid_points [i] [j]].x - positions [valid_points [i] [j + 1]].x) / 2;
						w.z_length = Mathf.Abs (positions [valid_points [i] [j]].z - positions [valid_points [i] [j + 1]].z) / 2;
					}
					else {
						w.x_pos = (positions [valid_points [i] [j]].x + positions [valid_points [i] [0]].x) / 2;
						w.z_pos = (positions [valid_points [i] [j]].z + positions [valid_points [i] [0]].z) / 2;
						w.x_length = Mathf.Abs (positions [valid_points [i] [j]].x - positions [valid_points [i] [0]].x) / 2;
						w.z_length = Mathf.Abs (positions [valid_points [i] [j]].z - positions [valid_points [i] [0]].z) / 2;
					}

					for (int l = 0; l < rooms.Count; l++) {
						for (int k = 0; k < rooms [l].Count; k++) {
							if (w.x_pos == rooms [l] [k].x_pos && w.z_pos == rooms [l] [k].z_pos) {
								w.twin_wall = rooms [l] [k];
								rooms [l] [k].twin_wall = w;
							}
						}
					}
					room.Add (w);
				}
				rooms.Add (room);
			}
		}

		//determine which wall coresponds to each door
		for (int l = 0; l < valid_points.Count; l++) {
			if (valid_points [l].Count == 3) {
				Door d=new Door();
				bool with_wall = false;

				for (int i = 0; i < rooms.Count && !with_wall; i++) {
					for (int j = 0; j < rooms[i].Count && !with_wall; j++) {
						if (!rooms[i][j].with_door) {//-------see if a door fits into more than one wall???
							if (positions [valid_points [l] [0]].x == rooms [i] [j].x_pos && positions [valid_points [l] [1]].x == rooms [i] [j].x_pos &&
								positions [valid_points [l] [0]].z > rooms [i] [j].z_pos - rooms [i] [j].z_length && positions [valid_points [l] [0]].z < rooms [i] [j].z_pos + rooms [i] [j].z_length &&
								positions [valid_points [l] [1]].z > rooms [i] [j].z_pos - rooms [i] [j].z_length && positions [valid_points [l] [1]].z < rooms [i] [j].z_pos + rooms [i] [j].z_length) {

								d.x_pos = positions [valid_points [l] [0]].x;
								d.z_pos = (positions [valid_points [l] [0]].z + positions [valid_points [l] [1]].z) / 2;
								d.orientation = 1;
								d.x_length = positions [valid_points [l] [2]].x - positions [valid_points [l] [0]].x;//asta imi spune in ce directie se deschide usa
								d.z_length = Mathf.Abs (positions [valid_points [l] [0]].z - positions [valid_points [l] [1]].z)/2;//asta e latimea usii

								if (d.z_length < 0.1)
									d.z_length = 0.13f;
								else d.z_length = 0.20f;

								if (positions [valid_points [l] [0]].z > positions [valid_points [l] [1]].z)
									d.hinge_pos = 2;//---1
								else
									d.hinge_pos = 1;//---2
							}
							else if (positions [valid_points [l] [0]].x == rooms [i] [j].x_pos && positions [valid_points [l] [2]].x == rooms [i] [j].x_pos &&
								positions [valid_points [l] [0]].z > rooms [i] [j].z_pos - rooms [i] [j].z_length && positions [valid_points [l] [0]].z < rooms [i] [j].z_pos + rooms [i] [j].z_length &&
								positions [valid_points [l] [2]].z > rooms [i] [j].z_pos - rooms [i] [j].z_length && positions [valid_points [l] [2]].z < rooms [i] [j].z_pos + rooms [i] [j].z_length) {

								d.x_pos = positions [valid_points [l] [0]].x;
								d.z_pos = (positions [valid_points [l] [0]].z + positions [valid_points [l] [2]].z) / 2;
								d.orientation = 1;
								d.x_length = positions [valid_points [l] [1]].x - positions [valid_points [l] [0]].x;
								d.z_length = Mathf.Abs (positions [valid_points [l] [0]].z - positions [valid_points [l] [2]].z)/2;

								if (d.z_length < 0.1)
									d.z_length = 0.13f;
								else d.z_length = 0.20f;

								if (positions [valid_points [l] [0]].z > positions [valid_points [l] [2]].z)
									d.hinge_pos = 2;
								else
									d.hinge_pos = 1;
							}
							else if (positions [valid_points [l] [0]].z == rooms [i] [j].z_pos && positions [valid_points [l] [1]].z == rooms [i] [j].z_pos &&
								positions [valid_points [l] [0]].x > rooms [i] [j].x_pos - rooms [i] [j].x_length && positions [valid_points [l] [0]].x < rooms [i] [j].x_pos + rooms [i] [j].x_length &&
								positions [valid_points [l] [1]].x > rooms [i] [j].x_pos - rooms [i] [j].x_length && positions [valid_points [l] [1]].x < rooms [i] [j].x_pos + rooms [i] [j].x_length) {

								d.x_pos = (positions [valid_points [l] [0]].x + positions [valid_points [l] [1]].x) / 2;
								d.z_pos = positions [valid_points [l] [0]].z;
								d.orientation = 2;
								d.z_length = positions [valid_points [l] [2]].z - positions [valid_points [l] [0]].z;
								d.x_length = Mathf.Abs (positions [valid_points [l] [0]].x - positions [valid_points [l] [1]].x)/2;

								if (d.x_length < 0.1)
									d.x_length = 0.13f;
								else d.x_length = 0.20f;

								if (positions [valid_points [l] [0]].x > positions [valid_points [l] [1]].x)
									d.hinge_pos = 1;
								else
									d.hinge_pos = 2;
							}
							else if (positions [valid_points [l] [0]].z == rooms [i] [j].z_pos && positions [valid_points [l] [2]].z == rooms [i] [j].z_pos &&
								positions [valid_points [l] [0]].x > rooms [i] [j].x_pos - rooms [i] [j].x_length && positions [valid_points [l] [0]].x < rooms [i] [j].x_pos + rooms [i] [j].x_length &&
								positions [valid_points [l] [2]].x > rooms [i] [j].x_pos - rooms [i] [j].x_length && positions [valid_points [l] [2]].x < rooms [i] [j].x_pos + rooms [i] [j].x_length) {

								d.x_pos = (positions [valid_points [l] [0]].x + positions [valid_points [l] [2]].x) / 2;
								d.z_pos = positions [valid_points [l] [0]].z;
								d.orientation = 2;
								d.z_length = positions [valid_points [l] [1]].z - positions [valid_points [l] [0]].z;
								d.x_length = Mathf.Abs (positions [valid_points [l] [0]].x - positions [valid_points [l] [2]].x)/2;

								if (d.x_length < 0.1)
									d.x_length = 0.13f;
								else d.x_length = 0.20f;

								if (positions [valid_points [l] [0]].x > positions [valid_points [l] [2]].x)
									d.hinge_pos = 1;
								else
									d.hinge_pos = 2;
							}

							if (d.x_length != 0) {
								with_wall = true;
								rooms [i] [j].with_door = true;
								rooms [i] [j].d = d;
								doors.Add (d);

								if (rooms [i] [j].twin_wall != null) {
									rooms [i] [j].twin_wall.with_door = true;
									rooms [i] [j].twin_wall.d = d;
								}
							}
						}
					}
				}
			}
		}

		//the remaining points in window_points are turned into actual window types
		//and are assigned to a wall in rooms
		for (int l = 0; l < window_points.Count; l++) {
			Window w = new Window ();
			if (positions [window_points [l] [0]].x == positions [window_points [l] [1]].x) {
				w.x_pos = positions [window_points [l] [0]].x;
				w.z_pos = (positions [window_points [l] [0]].z + positions [window_points [l] [1]].z) / 2;
				w.length = Mathf.Abs (positions [window_points [l] [0]].z - w.z_pos);
				w.orientation = 1;
			}
			else if (positions [window_points [l] [0]].z == positions [window_points [l] [1]].z) {
				w.z_pos = positions [window_points [l] [0]].z;
				w.x_pos = (positions [window_points [l] [0]].x + positions [window_points [l] [1]].x) / 2;
				w.length = Mathf.Abs (positions [window_points [l] [0]].x - w.x_pos);
				w.orientation = 2;
			}
			if (w.length > 0.3f) w.length = 0.5f;
			else w.length = 0.3f;

			bool with_wall = false;
			for (int i = 0; i < rooms.Count && !with_wall; i++) {
				for (int j = 0; j < rooms [i].Count && !with_wall; j++) {
					if (w.orientation == 1 && rooms [i] [j].x_pos == w.x_pos &&
					    ((w.z_pos - w.length < rooms [i] [j].z_pos - rooms [i] [j].z_length && w.z_pos + w.length > rooms [i] [j].z_pos + rooms [i] [j].z_length) ||
					    (w.z_pos - w.length > rooms [i] [j].z_pos - rooms [i] [j].z_length && w.z_pos + w.length < rooms [i] [j].z_pos + rooms [i] [j].z_length))) {

						with_wall = true;
						rooms [i] [j].with_window = true;
						rooms [i] [j].w = w;
					}
					else if (w.orientation == 2 && rooms [i] [j].z_pos == w.z_pos &&
					         ((w.x_pos - w.length < rooms [i] [j].x_pos - rooms [i] [j].x_length && w.x_pos + w.length > rooms [i] [j].x_pos + rooms [i] [j].x_length) ||
					         (w.x_pos - w.length > rooms [i] [j].x_pos - rooms [i] [j].x_length && w.x_pos + w.length < rooms [i] [j].x_pos + rooms [i] [j].x_length))) {

						with_wall = true;
						rooms [i] [j].with_window = true;
						rooms [i] [j].w = w;
					}
				}
			}
			if (!with_wall) print("eroare");

			//windows.Add (w);
		}

		//determine the orientation of the walls
		//this is necessary because the walls need to be moved a bit
		//so they do not overlap
		for (int i = 0; i < rooms.Count; i++) {
			//get the wall with the smallest x_pos value
			//this one is sure to have orientation 1
			float min_x = rooms[i][0].x_pos;
			int min_pos = 0;
			for (int j = 1; j < rooms [i].Count; j++) {
				if (min_x > rooms [i] [j].x_pos) {
					min_x = rooms [i] [j].x_pos;
					min_pos = j;
				}
			}

			rooms [i] [min_pos].orientation = 1;
			int l = min_pos,prev_orientation,prev_pos;
			float dif_x, dif_z;
			//now set the orientation for every other wall based on its predecessor's
			for (int j = 0; j < rooms [i].Count - 1; j++) {
				prev_orientation = rooms [i] [l].orientation;
				prev_pos = l;
				l++;
				if (l == rooms [i].Count)
					l = 0;

				dif_x = rooms [i] [l].x_pos - rooms [i] [prev_pos].x_pos;
				dif_z = rooms [i] [l].z_pos - rooms [i] [prev_pos].z_pos;

				if (prev_orientation == 1) {
					if ((dif_x < 0 && dif_z < 0) || (dif_x > 0 && dif_z > 0))
						rooms [i] [l].orientation = 4;
					else if ((dif_x > 0 && dif_z < 0) || (dif_x < 0 && dif_z > 0))
						rooms [i] [l].orientation = 2;
					else
						rooms [i] [l].orientation = 1;
				}
				else if (prev_orientation == 2) {
					if ((dif_x < 0 && dif_z < 0) || (dif_x > 0 && dif_z > 0))
						rooms [i] [l].orientation = 3;
					else if ((dif_x > 0 && dif_z < 0) || (dif_x < 0 && dif_z > 0))
						rooms [i] [l].orientation = 1;
					else
						rooms [i] [l].orientation = 2;
				}
				else if (prev_orientation == 3) {
					if ((dif_x < 0 && dif_z < 0) || (dif_x > 0 && dif_z > 0))
						rooms [i] [l].orientation = 2;
					else if ((dif_x > 0 && dif_z < 0) || (dif_x < 0 && dif_z > 0))
						rooms [i] [l].orientation = 4;
					else
						rooms [i] [l].orientation = 3;
				}
				else if (prev_orientation == 4) {
					if ((dif_x < 0 && dif_z < 0) || (dif_x > 0 && dif_z > 0))
						rooms [i] [l].orientation = 1;
					else if ((dif_x > 0 && dif_z < 0) || (dif_x < 0 && dif_z > 0))
						rooms [i] [l].orientation = 3;
					else
						rooms [i] [l].orientation = 4;
				}
			}
		}

		//see if a room has any window attached
		for (int i = 0; i < rooms.Count; i++) {
			bool with_window=false;
			for (int j = 0; j < rooms [i].Count; j++) {
				if (rooms[i][j].with_window) with_window=true;
			}
			has_window.Add (with_window);
		}

		//render the rooms' walls
		float x_aux,z_aux;
		List<GameObject> outer_walls = new List<GameObject> ();
		for (int i = 0; i < rooms.Count; i++) {
			List<GameObject> walls=new List<GameObject>();
			for (int j = 0; j < rooms[i].Count; j++) {
				x_aux = rooms[i][j].x_pos * uni_scale;
				z_aux = rooms[i][j].z_pos * uni_scale;

				float x_add = 0, z_add = 0;
				if (rooms [i] [j].orientation == 1) 
					x_add += 0.1f;
				else if (rooms [i] [j].orientation == 3)
					x_add -= 0.1f;
				else if (rooms [i] [j].orientation == 2)
					z_add += 0.1f;
				else if (rooms [i] [j].orientation == 4)
					z_add -= 0.1f;

				if (rooms [i] [j].with_door) {
					GameObject w1 = GameObject.Instantiate (wall_prefab);//--------cele 3 ar trebui sa fie distruse toate odata
					GameObject w2 = GameObject.Instantiate (wall_prefab);
					GameObject w3 = GameObject.Instantiate (wall_prefab);

					//any room that has no window will appear darker because light has nowhere to come through
					if (!has_window[i]) w1.layer = w2.layer = w3.layer = 8;

					w1.transform.position = new Vector3 (rooms [i] [j].d.x_pos * uni_scale + x_add, height * 7 / 8, rooms [i] [j].d.z_pos * uni_scale + z_add);
					if (rooms [i] [j].d.orientation == 1) {
						w1.transform.localScale = new Vector3 (wall_thickness, height * 2 / 8, rooms [i] [j].d.z_length * 2 * door_scale * uni_scale);

						w2.transform.position = new Vector3 (x_aux + x_add, height / 2, (rooms [i] [j].d.z_pos - rooms [i] [j].d.z_length * door_scale + rooms [i] [j].z_pos - rooms [i] [j].z_length) / 2 * uni_scale + z_add);
						w3.transform.position = new Vector3 (x_aux + x_add, height / 2, (rooms [i] [j].d.z_pos + rooms [i] [j].d.z_length * door_scale + rooms [i] [j].z_pos + rooms [i] [j].z_length) / 2 * uni_scale + z_add);

						w2.transform.localScale = new Vector3 (wall_thickness, height, (rooms [i] [j].d.z_pos - rooms [i] [j].d.z_length * door_scale - rooms [i] [j].z_pos + rooms [i] [j].z_length) * uni_scale);
						w3.transform.localScale = new Vector3 (wall_thickness, height, (rooms [i] [j].z_pos + rooms [i] [j].z_length - rooms [i] [j].d.z_pos - rooms [i] [j].d.z_length * door_scale) * uni_scale);
					}
					else if (rooms [i] [j].d.orientation == 2) {
						w1.transform.localScale = new Vector3 (rooms [i] [j].d.x_length * 2 * door_scale * uni_scale, height * 2 / 8, wall_thickness);

						w2.transform.position = new Vector3 ((rooms [i] [j].d.x_pos - rooms [i] [j].d.x_length * door_scale + rooms [i] [j].x_pos - rooms [i] [j].x_length) / 2 * uni_scale + x_add, height / 2, z_aux + z_add);
						w3.transform.position = new Vector3 ((rooms [i] [j].d.x_pos + rooms [i] [j].d.x_length * door_scale + rooms [i] [j].x_pos + rooms [i] [j].x_length) / 2 * uni_scale + x_add, height / 2, z_aux + z_add);

						w2.transform.localScale = new Vector3 ((rooms [i] [j].d.x_pos - rooms [i] [j].d.x_length * door_scale - rooms [i] [j].x_pos + rooms [i] [j].x_length) * uni_scale, height, wall_thickness);
						w3.transform.localScale = new Vector3 ((rooms [i] [j].x_pos + rooms [i] [j].x_length - rooms [i] [j].d.x_pos - rooms [i] [j].d.x_length * door_scale) * uni_scale, height, wall_thickness);
					}
						
					walls.Add (w1);
					walls.Add (w2);
					walls.Add (w3);

					//-------set twins in walls' scripts

					if (rooms [i] [j].twin_wall == null) {//if it doesn't have a twin then it's an outer wall
						GameObject ow1 = GameObject.Instantiate (wall_prefab);//--------cele 3 ar trebui sa fie distruse toate odata
						GameObject ow2 = GameObject.Instantiate (wall_prefab);
						GameObject ow3 = GameObject.Instantiate (wall_prefab);

						ow1.transform.position = new Vector3 (rooms [i] [j].d.x_pos * uni_scale - x_add, height * 7 / 8, rooms [i] [j].d.z_pos * uni_scale - z_add);
						if (rooms [i] [j].d.orientation == 1) {
							ow1.transform.localScale = new Vector3 (wall_thickness, height * 2 / 8, rooms [i] [j].d.z_length * 2 * door_scale * uni_scale);

							ow2.transform.position = new Vector3 (x_aux - x_add, height / 2, (rooms [i] [j].d.z_pos - rooms [i] [j].d.z_length * door_scale + rooms [i] [j].z_pos - rooms [i] [j].z_length) / 2 * uni_scale - z_add);
							ow3.transform.position = new Vector3 (x_aux - x_add, height / 2, (rooms [i] [j].d.z_pos + rooms [i] [j].d.z_length * door_scale + rooms [i] [j].z_pos + rooms [i] [j].z_length) / 2 * uni_scale - z_add);

							ow2.transform.localScale = new Vector3 (wall_thickness, height, (rooms [i] [j].d.z_pos - rooms [i] [j].d.z_length * door_scale - rooms [i] [j].z_pos + rooms [i] [j].z_length) * uni_scale);
							ow3.transform.localScale = new Vector3 (wall_thickness, height, (rooms [i] [j].z_pos + rooms [i] [j].z_length - rooms [i] [j].d.z_pos - rooms [i] [j].d.z_length * door_scale) * uni_scale);
						}
						else if (rooms [i] [j].d.orientation == 2) {
							ow1.transform.localScale = new Vector3 (rooms [i] [j].d.x_length * 2 * door_scale * uni_scale, height * 2 / 8, wall_thickness);

							ow2.transform.position = new Vector3 ((rooms [i] [j].d.x_pos - rooms [i] [j].d.x_length * door_scale + rooms [i] [j].x_pos - rooms [i] [j].x_length) / 2 * uni_scale - x_add, height / 2, z_aux - z_add);
							ow3.transform.position = new Vector3 ((rooms [i] [j].d.x_pos + rooms [i] [j].d.x_length * door_scale + rooms [i] [j].x_pos + rooms [i] [j].x_length) / 2 * uni_scale - x_add, height / 2, z_aux - z_add);

							ow2.transform.localScale = new Vector3 ((rooms [i] [j].d.x_pos - rooms [i] [j].d.x_length * door_scale - rooms [i] [j].x_pos + rooms [i] [j].x_length) * uni_scale, height, wall_thickness);
							ow3.transform.localScale = new Vector3 ((rooms [i] [j].x_pos + rooms [i] [j].x_length - rooms [i] [j].d.x_pos - rooms [i] [j].d.x_length * door_scale) * uni_scale, height, wall_thickness);
						}

						outer_walls.Add (ow1);
						outer_walls.Add (ow2);
						outer_walls.Add (ow3);
					}
				}
				else if (rooms [i] [j].with_window) {
					GameObject w1 = GameObject.Instantiate (wall_prefab);//--------cele 4 ar trebui sa fie distruse toate odata
					GameObject w2 = GameObject.Instantiate (wall_prefab);
					GameObject w3 = GameObject.Instantiate (wall_prefab);
					GameObject w4 = GameObject.Instantiate (wall_prefab);

					//any wall that has a window will appear darker because it doesn't receive as much light
					w1.layer = w2.layer = w3.layer = w4.layer = 8;

					w1.transform.position = new Vector3 (rooms [i] [j].w.x_pos * uni_scale + x_add, height * 15 / 16, rooms [i] [j].w.z_pos * uni_scale + z_add);
					w4.transform.position = new Vector3 (rooms [i] [j].w.x_pos * uni_scale + x_add, height * 2 / 16, rooms [i] [j].w.z_pos * uni_scale + z_add);
					if (rooms [i] [j].w.orientation == 1) {
						w1.transform.localScale = new Vector3 (wall_thickness, height * 1 / 8, rooms [i] [j].w.length * 2 * uni_scale);
						w4.transform.localScale = new Vector3 (wall_thickness, height * 2 / 8, rooms [i] [j].w.length * 2 * uni_scale);

						w2.transform.position = new Vector3 (x_aux + x_add, height / 2, (rooms [i] [j].z_pos - rooms [i] [j].z_length + rooms [i] [j].w.z_pos - rooms [i] [j].w.length) / 2 * uni_scale + z_add);
						w3.transform.position = new Vector3 (x_aux + x_add, height / 2, (rooms [i] [j].z_pos + rooms [i] [j].z_length + rooms [i] [j].w.z_pos + rooms [i] [j].w.length) / 2 * uni_scale + z_add);
						w2.transform.localScale = new Vector3 (wall_thickness, height, (rooms [i] [j].w.z_pos - rooms [i] [j].w.length - rooms [i] [j].z_pos + rooms [i] [j].z_length) * uni_scale);
						w3.transform.localScale = new Vector3 (wall_thickness, height, (rooms [i] [j].z_pos + rooms [i] [j].z_length - rooms [i] [j].w.z_pos - rooms [i] [j].w.length) * uni_scale);
					}
					else if (rooms [i] [j].w.orientation == 2) {
						w1.transform.localScale = new Vector3 (rooms [i] [j].w.length * 2 * uni_scale, height * 2 / 8, wall_thickness);
						w4.transform.localScale = new Vector3 (rooms [i] [j].w.length * 2 * uni_scale, height * 2 / 8, wall_thickness);

						w2.transform.position = new Vector3 ((rooms [i] [j].x_pos - rooms [i] [j].x_length + rooms [i] [j].w.x_pos - rooms [i] [j].w.length) / 2 * uni_scale + x_add, height / 2, z_aux + z_add);
						w3.transform.position = new Vector3 ((rooms [i] [j].x_pos + rooms [i] [j].x_length + rooms [i] [j].w.x_pos + rooms [i] [j].w.length) / 2 * uni_scale + x_add, height / 2, z_aux + z_add);
						w2.transform.localScale = new Vector3 ((rooms [i] [j].w.x_pos - rooms [i] [j].w.length - rooms [i] [j].x_pos + rooms [i] [j].x_length) * uni_scale, height, wall_thickness);
						w3.transform.localScale = new Vector3 ((rooms[i][j].x_pos+rooms[i][j].x_length-rooms[i][j].w.x_pos-rooms[i][j].w.length) * uni_scale, height, wall_thickness);
					}

					walls.Add (w1);
					walls.Add (w2);
					walls.Add (w3);
					walls.Add (w4);

					//the windows can be also be rendered here because they don't appear twice like doors
					GameObject w = GameObject.Instantiate(window_prefab);
					w.transform.localScale = new Vector3 (w.transform.localScale.x * uni_scale / original_uni_scale, w.transform.localScale.y * height/original_height, w.transform.localScale.z);
					w.transform.position = new Vector3 (rooms [i] [j].w.x_pos*uni_scale - x_add, height * 9 / 16, rooms [i] [j].w.z_pos*uni_scale - z_add);//------fara x_add, z_add???
					if (rooms [i] [j].w.orientation == 1) w.transform.localRotation = Quaternion.Euler (0, 90, 0);

					//also, for every window a new point light will be created to further improve lighting
					GameObject lightGameObject = new GameObject("Window Light");
					Light lightComp = lightGameObject.AddComponent<Light>();
					lightGameObject.transform.position = new Vector3(w.transform.position.x,w.transform.position.y+6,w.transform.position.z);

					if (rooms [i] [j].orientation == 1)
						lightGameObject.transform.position = new Vector3 (lightGameObject.transform.position.x - 4, lightGameObject.transform.position.y, lightGameObject.transform.position.z);
					else if (rooms [i] [j].orientation == 2)
						lightGameObject.transform.position = new Vector3 (lightGameObject.transform.position.x, lightGameObject.transform.position.y, lightGameObject.transform.position.z - 4);
					else if (rooms [i] [j].orientation == 3)
						lightGameObject.transform.position = new Vector3 (lightGameObject.transform.position.x + 4, lightGameObject.transform.position.y, lightGameObject.transform.position.z);
					else if (rooms [i] [j].orientation == 4)
						lightGameObject.transform.position = new Vector3 (lightGameObject.transform.position.x, lightGameObject.transform.position.y, lightGameObject.transform.position.z + 4);

					lightComp.range = 80;
					lightComp.intensity = 1;
					lightComp.bounceIntensity = 0;
					lightComp.shadows = LightShadows.Hard;
					lightComp.cullingMask = 513;

					if (rooms [i] [j].twin_wall == null) {//if it doesn't have a twin then it's an outer wall
						GameObject ow1 = GameObject.Instantiate (wall_prefab);
						GameObject ow2 = GameObject.Instantiate (wall_prefab);
						GameObject ow3 = GameObject.Instantiate (wall_prefab);
						GameObject ow4 = GameObject.Instantiate (wall_prefab);

						ow1.transform.position = new Vector3 (rooms [i] [j].w.x_pos * uni_scale - x_add, height * 15 / 16, rooms [i] [j].w.z_pos * uni_scale - z_add);
						ow4.transform.position = new Vector3 (rooms [i] [j].w.x_pos * uni_scale - x_add, height * 2 / 16, rooms [i] [j].w.z_pos * uni_scale - z_add);
						if (rooms [i] [j].w.orientation == 1) {
							ow1.transform.localScale = new Vector3 (wall_thickness, height * 1 / 8, rooms [i] [j].w.length * 2 * uni_scale);
							ow4.transform.localScale = new Vector3 (wall_thickness, height * 2 / 8, rooms [i] [j].w.length * 2 * uni_scale);

							ow2.transform.position = new Vector3 (x_aux - x_add, height / 2, (rooms [i] [j].z_pos - rooms [i] [j].z_length + rooms [i] [j].w.z_pos - rooms [i] [j].w.length) / 2 * uni_scale - z_add);
							ow3.transform.position = new Vector3 (x_aux - x_add, height / 2, (rooms [i] [j].z_pos + rooms [i] [j].z_length + rooms [i] [j].w.z_pos + rooms [i] [j].w.length) / 2 * uni_scale - z_add);
							ow2.transform.localScale = new Vector3 (wall_thickness, height, (rooms [i] [j].w.z_pos - rooms [i] [j].w.length - rooms [i] [j].z_pos + rooms [i] [j].z_length) * uni_scale);
							ow3.transform.localScale = new Vector3 (wall_thickness, height, (rooms [i] [j].z_pos + rooms [i] [j].z_length - rooms [i] [j].w.z_pos - rooms [i] [j].w.length) * uni_scale);
						}
						else if (rooms [i] [j].w.orientation == 2) {
							ow1.transform.localScale = new Vector3 (rooms [i] [j].w.length * 2 * uni_scale, height * 2 / 8, wall_thickness);
							ow4.transform.localScale = new Vector3 (rooms [i] [j].w.length * 2 * uni_scale, height * 2 / 8, wall_thickness);

							ow2.transform.position = new Vector3 ((rooms [i] [j].x_pos - rooms [i] [j].x_length + rooms [i] [j].w.x_pos - rooms [i] [j].w.length) / 2 * uni_scale - x_add, height / 2, z_aux - z_add);
							ow3.transform.position = new Vector3 ((rooms [i] [j].x_pos + rooms [i] [j].x_length + rooms [i] [j].w.x_pos + rooms [i] [j].w.length) / 2 * uni_scale - x_add, height / 2, z_aux - z_add);
							ow2.transform.localScale = new Vector3 ((rooms [i] [j].w.x_pos - rooms [i] [j].w.length - rooms [i] [j].x_pos + rooms [i] [j].x_length) * uni_scale, height, wall_thickness);
							ow3.transform.localScale = new Vector3 ((rooms[i][j].x_pos+rooms[i][j].x_length-rooms[i][j].w.x_pos-rooms[i][j].w.length) * uni_scale, height, wall_thickness);
						}

						outer_walls.Add (ow1);
						outer_walls.Add (ow2);
						outer_walls.Add (ow3);
						outer_walls.Add (ow4);
					}

				}
				else {
					GameObject w = GameObject.Instantiate (wall_prefab);
					rooms [i] [j].gameObject = w;//keeps a reference to its own wall gameObject so it can send it to its twin
					w.transform.position = new Vector3 (x_aux + x_add, height / 2, z_aux + z_add);
					if (!has_window [i]) w.layer = 8;
					if (rooms [i] [j].x_length == 0)
						w.transform.localScale = new Vector3 (wall_thickness, height, rooms [i] [j].z_length * 2 * uni_scale);
					else
						w.transform.localScale = new Vector3 (rooms [i] [j].x_length * 2 * uni_scale, height, wall_thickness);
					walls.Add (w);

					if (rooms [i] [j].twin_wall == null) {
						GameObject ow = GameObject.Instantiate (wall_prefab);
						ow.transform.position = new Vector3 (x_aux - x_add, height / 2, z_aux - z_add);
						if (rooms [i] [j].x_length == 0)
							ow.transform.localScale = new Vector3 (wall_thickness, height, rooms [i] [j].z_length * 2 * uni_scale);
						else
							ow.transform.localScale = new Vector3 (rooms [i] [j].x_length * 2 * uni_scale, height, wall_thickness);
						outer_walls.Add (ow);
					}
				}
			}
			//set the neighbours for every wall created
			for (int j = 0; j < walls.Count; j++) {
				WallBehaviour w = walls[j].GetComponent<WallBehaviour> ();
				w.neighbours = walls;
			}
		}

		//the assignation of the twin gameObjects in the scripts has to
		//be done after the loop because half the time a twin's gameObject not be created before
		//so you there is no script in which to put the reference to the gameObject
		for (int i = 0; i < rooms.Count; i++) {
			for (int j = 0; j < rooms [i].Count; j++) {
				if (rooms [i] [j].twin_wall != null && !rooms[i][j].with_door && !rooms[i][j].with_window) {
					rooms [i] [j].twin_wall.gameObject.GetComponent<WallBehaviour> ().twin = rooms [i] [j].gameObject;
				}
			}
		}

		for (int j = 0; j < outer_walls.Count; j++) {
			WallBehaviour w = outer_walls[j].GetComponent<WallBehaviour> ();
			w.neighbours = outer_walls;
		}

		//render doors
		for (int i = 0; i < doors.Count; i++) {
			GameObject d = GameObject.Instantiate (door_body_prefab);
			d.transform.position = new Vector3 (doors [i].x_pos*uni_scale, height * 3 / 8, doors [i].z_pos*uni_scale);
			int rot;
			if (doors [i].orientation == 1) {
				rot = 90;
				d.transform.localScale = new Vector3 (doors [i].z_length * 2 / door_unit_size * d.transform.localScale.x * uni_scale/original_uni_scale, d.transform.localScale.y * height/original_height, d.transform.localScale.z);
			}
			else {
				rot = 0;
				d.transform.localScale = new Vector3 (doors [i].x_length * 2 / door_unit_size * d.transform.localScale.x * uni_scale/original_uni_scale, d.transform.localScale.y * height/original_height, d.transform.localScale.z);
			}
			if (doors [i].hinge_pos == 2)
				rot += 180;
			d.transform.rotation = Quaternion.Euler (0, rot, 0);

			GameObject e = GameObject.Instantiate (door_interaction_prefab);
			e.transform.localScale = new Vector3 (e.transform.localScale.x * uni_scale / original_uni_scale, e.transform.localScale.y * height/original_height, e.transform.localScale.z);
			if (doors [i].orientation == 1) {
				if (doors [i].hinge_pos == 1) {
					e.transform.position = new Vector3 (doors [i].x_pos * uni_scale, height * 3 / 8, (doors [i].z_pos - doors [i].z_length * door_scale) * uni_scale);
					e.GetComponent<BoxCollider> ().center = new Vector3 (-doors [i].z_length * door_scale * uni_scale / e.transform.localScale.x, 0, 0);
					e.GetComponent<DoorBehaviour> ().dir = (int)Mathf.Sign (doors [i].x_length) * (-1);
				}
				else {
					e.transform.position = new Vector3 (doors [i].x_pos * uni_scale, height * 3 / 8, (doors [i].z_pos + doors [i].z_length * door_scale) * uni_scale);
					e.GetComponent<BoxCollider> ().center = new Vector3 (doors [i].z_length * door_scale * uni_scale / e.transform.localScale.x,0,0);
					e.GetComponent<DoorBehaviour> ().dir = (int)Mathf.Sign (doors [i].x_length);
				}

				e.transform.rotation = Quaternion.Euler (0, 90, 0);
				e.GetComponent<BoxCollider> ().size = new Vector3 (e.GetComponent<BoxCollider> ().size.x * doors [i].z_length*2 / door_unit_size, e.GetComponent<BoxCollider> ().size.y, e.GetComponent<BoxCollider> ().size.z);
			}
			else{
				if (doors [i].hinge_pos == 1) {
					e.transform.position = new Vector3 ((doors [i].x_pos + doors [i].x_length * door_scale) * uni_scale, height * 3 / 8, doors [i].z_pos * uni_scale);
					e.GetComponent<BoxCollider> ().center = new Vector3 (-doors [i].x_length * door_scale * uni_scale / e.transform.localScale.x, 0, 0);
					e.GetComponent<DoorBehaviour> ().dir = (int)Mathf.Sign (doors [i].z_length) * (-1);
				}
				else {
					e.transform.position = new Vector3 ((doors [i].x_pos - doors [i].x_length * door_scale) * uni_scale, height * 3 / 8, doors [i].z_pos * uni_scale);
					e.GetComponent<BoxCollider> ().center = new Vector3 (doors [i].x_length * door_scale * uni_scale / e.transform.localScale.x, 0, 0);
					e.GetComponent<DoorBehaviour> ().dir = (int)Mathf.Sign (doors [i].z_length);
				}

				e.transform.rotation = Quaternion.Euler (0, 0, 0);
				e.GetComponent<BoxCollider> ().size = new Vector3 (e.GetComponent<BoxCollider> ().size.x * doors [i].x_length*2 / door_unit_size, e.GetComponent<BoxCollider> ().size.y, e.GetComponent<BoxCollider> ().size.z);

			}
			d.transform.parent = e.transform;
		}

		//eliminate points that are useless
		//i.e. they are in between two points whose line pass through that point
		int last;
		for (int i = 0; i < valid_points.Count; i++) {
			if (valid_points [i].Count < 5)
				continue;

			last = valid_points [i].Count - 1;
			List<int> eliminated=new List<int>();
			if (positions [valid_points [i] [last]].x == positions [valid_points [i] [0]].x && positions [valid_points [i] [0]].x == positions [valid_points [i] [1]].x &&
				((positions [valid_points [i] [0]].z < positions [valid_points [i] [last]].z && positions [valid_points [i] [0]].z > positions [valid_points [i] [1]].z) ||
					(positions [valid_points [i] [0]].z > positions [valid_points [i] [last]].z && positions [valid_points [i] [0]].z < positions [valid_points [i] [1]].z)))
				eliminated.Add (valid_points [i] [0]);
			else if (positions [valid_points [i] [last]].z == positions [valid_points [i] [0]].z && positions [valid_points [i] [0]].z == positions [valid_points [i] [1]].z &&
				((positions [valid_points [i] [0]].x < positions [valid_points [i] [last]].x && positions [valid_points [i] [0]].x > positions [valid_points [i] [1]].x) ||
					(positions [valid_points [i] [0]].x > positions [valid_points [i] [last]].x && positions [valid_points [i] [0]].x < positions [valid_points [i] [1]].x)))
				eliminated.Add (valid_points [i] [0]);

			for (int j = 1; j < valid_points [i].Count-1; j++) {
				if (positions [valid_points [i] [j - 1]].x == positions [valid_points [i] [j]].x && positions [valid_points [i] [j]].x == positions [valid_points [i] [j + 1]].x &&
					((positions [valid_points [i] [j]].z < positions [valid_points [i] [j - 1]].z && positions [valid_points [i] [j]].z > positions [valid_points [i] [j + 1]].z) ||
						(positions [valid_points [i] [j]].z > positions [valid_points [i] [j - 1]].z && positions [valid_points [i] [j]].z < positions [valid_points [i] [j + 1]].z)))
					eliminated.Add (valid_points [i] [j]);
				else if (positions [valid_points [i] [j - 1]].z == positions [valid_points [i] [j]].z && positions [valid_points [i] [j]].z == positions [valid_points [i] [j + 1]].z &&
					((positions [valid_points [i] [j]].x < positions [valid_points [i] [j - 1]].x && positions [valid_points [i] [j]].x > positions [valid_points [i] [j + 1]].x) ||
						(positions [valid_points [i] [j]].x > positions [valid_points [i] [j - 1]].x && positions [valid_points [i] [j]].x < positions [valid_points [i] [j + 1]].x)))
					eliminated.Add (valid_points [i] [j]);
			}

			if (positions [valid_points [i] [last - 1]].x == positions [valid_points [i] [valid_points [i].Count - 1]].x && positions [valid_points [i] [last]].x == positions [valid_points [i] [0]].x &&
				((positions [valid_points [i] [last]].z < positions [valid_points [i] [valid_points [i].Count - 2]].z && positions [valid_points [i] [last]].z > positions [valid_points [i] [0]].z) ||
					(positions [valid_points [i] [last]].z > positions [valid_points [i] [valid_points [i].Count - 2]].z && positions [valid_points [i] [last]].z < positions [valid_points [i] [0]].z)))
				eliminated.Add (valid_points [i] [last]);
			else if (positions [valid_points [i] [last - 1]].z == positions [valid_points [i] [last]].z && positions [valid_points [i] [last]].z == positions [valid_points [i] [0]].z &&
				((positions [valid_points [i] [last]].x < positions [valid_points [i] [last - 1]].x && positions [valid_points [i] [last]].x > positions [valid_points [i] [0]].x) ||
					(positions [valid_points [i] [last]].x > positions [valid_points [i] [last - 1]].x && positions [valid_points [i] [last]].x < positions [valid_points [i] [0]].x)))
				eliminated.Add (valid_points [i] [last]);

			for (int j = 0; j < eliminated.Count; j++) {
				valid_points [i].Remove (eliminated [j]);
			}
		}

		float x_min,x_max,z_min,z_max;
		//instantiate floor and ceiling
		for (int i=0;i<valid_points.Count;i++){
			if (valid_points [i].Count < 4)
				continue;

			//get the max and min values of each room
			x_min = x_max = positions [valid_points [i] [0]].x;
			z_min = z_max = positions [valid_points [i] [0]].z;
			for (int j = 1; j < valid_points [i].Count; j++) {
				if (positions [valid_points [i] [j]].x > x_max)
					x_max = positions [valid_points [i] [j]].x;
				if (positions [valid_points [i] [j]].x < x_min)
					x_min = positions [valid_points [i] [j]].x;

				if (positions [valid_points [i] [j]].z > z_max)
					z_max = positions [valid_points [i] [j]].z;
				if (positions [valid_points [i] [j]].z < z_min)
					z_min = positions [valid_points [i] [j]].z;
			}

			//create new valid points to be used to define floors and ceilings (they each need 4 points that form a rectangle)
			for (int j = 0; j < valid_points [i].Count; j++) {
				if (positions [valid_points [i] [j]].x != x_min && positions [valid_points [i] [j]].x != x_max && positions [valid_points [i] [j]].z != z_min && positions [valid_points [i] [j]].z != z_max) {
					for (int l = 0; l < valid_points [i].Count - 1; l++) {
						if (positions [valid_points [i] [l]].z == positions [valid_points [i] [l + 1]].z &&
							((positions [valid_points [i] [l]].x < positions [valid_points [i] [j]].x && positions [valid_points [i] [l + 1]].x > positions [valid_points [i] [j]].x) ||
								(positions [valid_points [i] [l]].x > positions [valid_points [i] [j]].x && positions [valid_points [i] [l + 1]].x < positions [valid_points [i] [j]].x))) {//----s-ar putea sa nu fie bine

							//insert a new point which will be used to construct the rectangles that form the floor and ceiling
							positions.Add (new Vector3 (positions [valid_points [i] [j]].x, 0,positions [valid_points [i] [l]].z));
							//print(valid_points [i]);-----------------not tested yet!!!
							valid_points [i].Insert (++l, ++max_valid_point);
							//print(valid_points [i]);
						}
					}

					if (positions [valid_points [i] [valid_points [i].Count - 1]].z == positions [valid_points [i] [0]].z &&
						((positions [valid_points [i] [valid_points [i].Count - 1]].x < positions [valid_points [i] [j]].x && positions [valid_points [i] [0]].x > positions [valid_points [i] [j]].x) ||
							(positions [valid_points [i] [ valid_points [i].Count - 1]].x > positions [valid_points [i] [j]].x && positions [valid_points [i] [0]].x < positions [valid_points [i] [j]].x))) {

						positions.Add (new Vector3 (positions [valid_points [i] [j]].x, 0,positions [valid_points [i] [0]].z));
						valid_points[i].Insert(0,++max_valid_point);
					}
				}
			}
			//print(valid_points[i].Count);

			//build the rectangles that form the floor and ceiling
			bool[] used=new bool[valid_points[i].Count];
			List<GameObject> floor=new List<GameObject>();
			List<GameObject> ceiling=new List<GameObject>();
			last = valid_points [i].Count - 1;

			for (int j = 0; j < valid_points [i].Count-1; j++) {
				if (positions[valid_points[i][j]].z==positions[valid_points[i][j+1]].z && (!used [j] || !used [j + 1])) {
					used [j] = used [j + 1] = true;
					bool flag = false;
					//search for another line, more precisely another pair of consecutive points
					//that can be combined with the selected ones to create a rectangle
					for (int l = 0; l < valid_points [i].Count - 1 && !flag; l++) {
						if (positions [valid_points [i] [l]].z == positions [valid_points [i] [l + 1]].z && (!used [l] || !used [l + 1])
						    && ((positions [valid_points [i] [l]].x == positions [valid_points [i] [j]].x && positions [valid_points [i] [l + 1]].x == positions [valid_points [i] [j + 1]].x) ||
						    (positions [valid_points [i] [l]].x == positions [valid_points [i] [j + 1]].x && positions [valid_points [i] [l + 1]].x == positions [valid_points [i] [j]].x))) {
							flag = true;
							used [l] = used [l + 1] = true;
							//build the rectangles for ceiling and floor for those 4 points
							GameObject c = GameObject.Instantiate (wall_prefab);
							c.transform.position = new Vector3 ((positions [valid_points [i] [j]].x + positions [valid_points [i] [j + 1]].x) / 2 * uni_scale, height + 0.2f, (positions [valid_points [i] [j]].z + positions [valid_points [i] [l]].z) / 2 * uni_scale);
							c.transform.localScale = new Vector3 (Mathf.Abs (positions [valid_points [i] [j]].x - positions [valid_points [i] [j + 1]].x) * uni_scale, 0.4f, Mathf.Abs (positions [valid_points [i] [j]].z - positions [valid_points [i] [l]].z) * uni_scale);// ---wall_thickness/2
							ceiling.Add (c);

							GameObject f = GameObject.Instantiate (wall_prefab);
							f.transform.position = new Vector3 ((positions [valid_points [i] [j]].x + positions [valid_points [i] [j + 1]].x) / 2 * uni_scale, -0.1f, (positions [valid_points [i] [j]].z + positions [valid_points [i] [l]].z) / 2 * uni_scale);
							f.transform.localScale = new Vector3 (Mathf.Abs (positions [valid_points [i] [j]].x - positions [valid_points [i] [j + 1]].x) * uni_scale, wall_thickness, Mathf.Abs (positions [valid_points [i] [j]].z - positions [valid_points [i] [l]].z) * uni_scale);// ---wall_thickness/2
							f.layer = 9;
							floor.Add (f);
							f.tag = "Floor";
						}
					}
					if (!flag) {//the first and the last also need to be checked
						if (positions [valid_points [i] [0]].z == positions [valid_points [i] [last]].z && (!used [0] || !used [last])
						    && ((positions [valid_points [i] [0]].x == positions [valid_points [i] [j]].x && positions [valid_points [i] [last]].x == positions [valid_points [i] [j + 1]].x) ||
						    (positions [valid_points [i] [0]].x == positions [valid_points [i] [j + 1]].x && positions [valid_points [i] [last]].x == positions [valid_points [i] [j]].x))) {

							used [0] = used [last] = true;
							GameObject c = GameObject.Instantiate (wall_prefab);
							c.transform.position = new Vector3 ((positions [valid_points [i] [j]].x + positions [valid_points [i] [j + 1]].x) / 2 * uni_scale, height + 0.2f, (positions [valid_points [i] [j]].z + positions [valid_points [i] [0]].z) / 2 * uni_scale);
							c.transform.localScale = new Vector3 (Mathf.Abs (positions [valid_points [i] [j]].x - positions [valid_points [i] [j + 1]].x) * uni_scale, 0.4f, Mathf.Abs (positions [valid_points [i] [j]].z - positions [valid_points [i] [0]].z) * uni_scale);// ---wall_thickness/2
							ceiling.Add (c);

							GameObject f = GameObject.Instantiate (wall_prefab);
							f.transform.position = new Vector3 ((positions [valid_points [i] [j]].x + positions [valid_points [i] [j + 1]].x) / 2 * uni_scale, -0.1f, (positions [valid_points [i] [j]].z + positions [valid_points [i] [0]].z) / 2 * uni_scale);
							f.transform.localScale = new Vector3 (Mathf.Abs (positions [valid_points [i] [j]].x - positions [valid_points [i] [j + 1]].x) * uni_scale, wall_thickness, Mathf.Abs (positions [valid_points [i] [j]].z - positions [valid_points [i] [0]].z) * uni_scale);
							f.layer = 9;
							floor.Add (f);
							f.tag = "Floor";
						}
					}
				}
			}

			//set the neighbours for every wall created
			for (int j = 0; j < ceiling.Count; j++) {
				WallBehaviour w = ceiling[j].GetComponent<WallBehaviour> ();
				w.neighbours = ceiling;
				w = floor[j].GetComponent<WallBehaviour> ();
				w.neighbours = floor;
			}
		}
	}

	void Start () {

		StartCoroutine (GenerateHouse ());
	}
	
	void Update () {
	}
}
