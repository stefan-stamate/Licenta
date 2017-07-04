using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ObjectInsertion : MonoBehaviour {

	public GameObject options_panel, object_display, object_positioner;
	public Texture2D folder_img, back_img;

	//hardcoded dictionary of prefabs folders
	//keys are folder names and the values types of objects
	//1 - can be put on the floor or against a wall -> sofas, desks, stands etc.
	//2 - can only be put on walls -> hangers, radiators, paintings
	//3 - can only be attached to ceilings -> illumination objects
	//4 - can be freely manipulated on all axis
	private Dictionary<string, List<string>> directory_children;
	private Dictionary<string,string> directory_parent;
	[HideInInspector]
	public Dictionary<string, int> directory_type;

	// Use this for initialization
	void Start () {
		directory_children = new Dictionary<string, List<string>> ();
		directory_parent = new Dictionary<string,string> ();
		directory_type = new Dictionary<string,int> ();

		//children
		directory_children ["Root"] = new List<string>();
		directory_children ["Root"].Add ("Bathroom");
		directory_children ["Root"].Add ("Beds");
		directory_children ["Root"].Add ("Chairs");
		directory_children ["Root"].Add ("Desks");
		directory_children ["Root"].Add ("Kitchen");
		directory_children ["Root"].Add ("Lamps");
		directory_children ["Root"].Add ("Libraries");
		directory_children ["Root"].Add ("Mirrors");
		directory_children ["Root"].Add ("Paintings");
		directory_children ["Root"].Add ("Radiators");
		directory_children ["Root"].Add ("Refrigerators");
		directory_children ["Root"].Add ("Sofas");
		directory_children ["Root"].Add ("Tables");
		//directory_children ["Root"].Add ("Misc");


		directory_children ["Bathroom"] = new List<string>();
		directory_children ["Bathroom"].Add ("Bathtubs");
		directory_children ["Bathroom"].Add ("Holders");
		directory_children ["Bathroom"].Add ("Sinks");
		directory_children ["Bathroom"].Add ("Stands");
		directory_children ["Bathroom"].Add ("Storages");
		directory_children ["Bathroom"].Add ("Toilets");
		directory_children ["Bathroom"].Add ("Washing Machines");

		directory_children ["Kitchen"] = new List<string>();
		//directory_children ["Kitchen"].Add ("Kitchen appliances");
		directory_children ["Kitchen"].Add ("Kitchen shelves");
		directory_children ["Kitchen"].Add ("Kitchen stands");

		directory_children ["Lamps"] = new List<string> ();
		directory_children ["Lamps"].Add ("Lusters");
		directory_children ["Lamps"].Add ("Sconces");
		//directory_children ["Lamps"].Add ("Table Lamps");

		//parents
		directory_parent ["Bathroom"] = "Root";
		directory_parent ["Beds"] = "Root";
		directory_parent ["Chairs"] = "Root";
		directory_parent ["Desks"] = "Root";
		directory_parent ["Kitchen"] = "Root";
		directory_parent ["Lamps"] = "Root";
		directory_parent ["Libraries"] = "Root";
		directory_parent ["Mirrors"] = "Root";
		directory_parent ["Paintings"] = "Root";
		directory_parent ["Radiators"] = "Root";
		directory_parent ["Refrigerators"] = "Root";
		directory_parent ["Sofas"] = "Root";
		directory_parent ["Tables"] = "Root";

		//directory_parent ["Misc"] = "Root";
		directory_parent ["Bathtubs"] = "Bathroom";
		directory_parent ["Holders"] = "Bathroom";
		directory_parent ["Sinks"] = "Bathroom";
		directory_parent ["Stands"] = "Bathroom";
		directory_parent ["Storages"] = "Bathroom";
		directory_parent ["Toilets"] = "Bathroom";
		directory_parent ["Washing Machines"] = "Bathroom";
		//...

		//directory_parent ["Kitchen appliances"] = "Kitchen";
		directory_parent ["Kitchen shelves"] = "Kitchen";
		directory_parent ["Kitchen stands"] = "Kitchen";

		directory_parent ["Lusters"] = "Lamps";
		directory_parent ["Sconces"] = "Lamps";
		//directory_parent ["Table Lamps"] = "Lamps";

		//types
		directory_type ["Root"] = 0;
		directory_type ["Bathroom"] = 0;
		directory_type ["Kitchen"] = 0;
		directory_type ["Lamps"] = 0;

		directory_type ["Beds"] = 1;
		directory_type ["Desks"] = 1;
		//directory_type ["Pillows"] = 4;
		directory_type ["Chairs"]=1;
		directory_type ["Libraries"] = 1;
		directory_type ["Mirrors"] = 2;
		directory_type ["Paintings"] = 2;
		directory_type ["Radiators"] = 2;
		directory_type ["Refrigerators"] = 1;
		directory_type ["Sofas"] = 1;
		directory_type ["Tables"] = 1;
		//directory_type ["Misc"] = 4;

		directory_type ["Bathtubs"] = 1;
		directory_type ["Holders"] = 2;
		directory_type ["Sinks"] = 1;
		directory_type ["Stands"] = 1;
		directory_type ["Storages"] = 1;
		directory_type ["Toilets"] = 1;
		directory_type ["Washing Machines"] = 1;

		//directory_type ["Kitchen appliances"] = 4;
		directory_type ["Kitchen shelves"] = 2;
		directory_type ["Kitchen stands"] = 1;

		directory_type ["Lusters"] = 3;
		directory_type ["Sconces"] = 2;
		//directory_type ["Table Lamps"] = 4;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void DisplayContents(string folder){

		//this panel will allow the user to browse the
		//collection of available prefabs
		GameObject panel = Instantiate (options_panel);
		GameObject camera = GameObject.Find("Camera Parent");
		panel.transform.SetParent(camera.transform,false);
		panel.transform.localPosition = new Vector3 (0, 0.2f, 2);
		panel.GetComponent<PanelManagement> ().nr_items = 1;
		panel.GetComponent<PanelManagement> ().type = 3;

		GameObject child_panel = new GameObject ("Child Panel");
		child_panel.AddComponent<CanvasRenderer>();
		child_panel.AddComponent<Image>();
		child_panel.transform.SetParent (panel.transform, false);
		child_panel.transform.localPosition = new Vector3 (0, -30, 0);
		child_panel.AddComponent<Mask> ();
		child_panel.GetComponent<CanvasRenderer> ().GetComponent<RectTransform> ().sizeDelta = new Vector2 (1400, 900);
		Color c = child_panel.GetComponent<Image> ().color;
		c.a = 1.0f/255;
		child_panel.GetComponent<Image> ().color = c;
		panel.GetComponent<PanelManagement> ().SetChildPanel (child_panel);

		int ind=2;
		int height = 260;
		int im_ind = 0;//numai pentru tipurile 1-4

		//display a quit button
		GameObject quit_button = GameObject.Instantiate(object_display);
		quit_button.transform.SetParent (child_panel.transform,false);
		quit_button.transform.localPosition = new Vector3 (-400, height, 0.2f);
		quit_button.transform.GetChild (2).GetComponent<Text> ().text = "Back";
		quit_button.transform.GetChild(1).GetComponent<Image>().sprite =
			Sprite.Create(back_img, new Rect(0, 0, back_img.width, back_img.height), new Vector2(0.5f, 0.5f));

		if (directory_type [folder] == 0) {//contains only folders
			panel.GetComponent<PanelManagement> ().nr_items+=directory_children[folder].Count;

			foreach (string child in directory_children[folder]) {
				GameObject go = GameObject.Instantiate(object_display);
				go.transform.SetParent (child_panel.transform,false);
				if (ind % 3 == 1)
					go.transform.localPosition = new Vector3 (-400, height, 0.2f);
				else if (ind % 3 == 2)
					go.transform.localPosition = new Vector3 (0, height, 0.2f);
				else {
					go.transform.localPosition = new Vector3 (400, height, 0.2f);
					height -= 440;
				}
				ind++;

				go.transform.GetChild(1).GetComponent<Image>().sprite = Sprite.Create(folder_img, new Rect(0, 0, folder_img.width, folder_img.height), new Vector2(0.5f, 0.5f));
				go.transform.GetChild (2).GetComponent<Text> ().text = child;
			}
		}
		else {//contains only files
			string path;
			if (directory_parent [folder] == "Root")
				path = folder;
			else
				path = directory_parent [folder] + "/" + folder;
			
			Object[] prefabs = Resources.LoadAll ("Objects/" + path + "/prefabs");
			panel.GetComponent<PanelManagement> ().nr_items += prefabs.Length;
			Object[] images = Resources.LoadAll ("Objects/" + path + "/images");

			foreach (Object o in prefabs) {
				GameObject go = GameObject.Instantiate(object_display);
				go.transform.SetParent (child_panel.transform,false);
				if (ind % 3 == 1)
					go.transform.localPosition = new Vector3 (-400, height, 0.2f);
				else if (ind % 3 == 2)
					go.transform.localPosition = new Vector3 (0, height, 0.2f);
				else {
					go.transform.localPosition = new Vector3 (400, height, 0.2f);
					height -= 440;
				}
				ind++;

				go.transform.GetChild (2).GetComponent<Text> ().text = o.name;

				//set image
				Texture2D aux_img=(Texture2D) images[im_ind];
				go.transform.GetChild(1).GetComponent<Image>().sprite = Sprite.Create(aux_img, new Rect(0, 0, aux_img.width, aux_img.height), new Vector2(0.5f, 0.5f));
				//print(im_ind);
				//go.transform.GetChild(1).GetComponent<Image>().material.mainTexture = (Texture)images[im_ind];
				im_ind++;
			}
		}
			
		child_panel.transform.GetChild(0).GetChild(0).GetComponent<Image> ().color = Color.red;
		panel.transform.GetChild (1).GetComponent<Text> ().text = folder;//---------

		//deactivate objects' colliders so they cannot
		//be triggered while using the menu
		GameObject.Find("UtilsHolder").GetComponent<Utils>().DeactivateColliders("Wall");
		GameObject.Find("UtilsHolder").GetComponent<Utils>().DeactivateColliders("Door");

		//lock user movement
		GameObject.Find("Camera Parent").GetComponent<CameraParentMovement>().movement_locked=true;
	}

	public void DisplayPrevFolder(string folder){
		if (folder != "Root")
			DisplayContents (directory_parent [folder]);
	}

	public void InsertObject(string folder_name, string object_name) {

		GameObject op = Instantiate (object_positioner);
		GameObject object_prefab;
		string path;
		if (directory_parent [folder_name] == "Root")
			path = "Objects/" + folder_name + "/prefabs/" + object_name;
		else
			path = "Objects/" + directory_parent [folder_name] + "/" + folder_name + "/prefabs/" + object_name;
		object_prefab = Resources.Load (path, typeof(GameObject)) as GameObject;
		GameObject o = Instantiate (object_prefab);

		op.transform.position = new Vector3 (0, 0, 0);
		o.transform.parent = op.transform;
		o.transform.position = new Vector3 (0, 0, 0);
		op.transform.localScale = new Vector3 (0, 0, 0);
		op.GetComponent<ObjectPositioning> ().folder = folder_name;
		op.GetComponent<ObjectPositioning> ().type = directory_type [folder_name];
		o.GetComponent<BoxCollider> ().enabled = false;
		o.tag = "Object";

		o.AddComponent <ObjectBehaviour> ();
		o.GetComponent<ObjectBehaviour> ().path = path;
		o.GetComponent<ObjectBehaviour> ().type = directory_type [folder_name];

		GameObject.Find ("UIMaster").GetComponent<UIMasterScript> ().stop = true;
		GameObject.Find ("UtilsHolder").GetComponent<Utils> ().SetWallsState (false);
	}
}
