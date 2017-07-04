using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DandWInsertion : MonoBehaviour {

	public GameObject options_panel;
	public GameObject button1;
	public GameObject object_display;
	public GameObject dandw_positioner;
	public Texture2D back_img;

	public void DisplayTiers(int type){//1 -> door, 2 -> window
		GameObject panel = Instantiate (options_panel);
		GameObject camera = GameObject.Find("Camera Parent");
		panel.transform.SetParent(camera.transform,false);
		panel.transform.localPosition = new Vector3 (0.8f, 0, 2);
		panel.GetComponent<PanelManagement> ().nr_items = 3;
		panel.GetComponent<PanelManagement> ().type = 1;
		if (type==1)
			panel.transform.GetChild (1).GetComponent<Text> ().text = "Doors";
		else if (type==2)
			panel.transform.GetChild (1).GetComponent<Text> ().text = "Windows";

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
		b1.GetComponentInChildren<Text> ().text = "Tier 1";
		b1.onClick.AddListener(delegate{DisplayContents(type, 1);});
		go1.transform.GetChild (0).GetComponent<Image> ().color = Color.red;

		GameObject go2 = GameObject.Instantiate(button1);
		go2.transform.SetParent (child_panel.transform,false);
		go2.transform.localPosition = new Vector3 (0, 0, 0.2f);
		Button b2 = go2.GetComponentInChildren<Button> ();
		b2.GetComponentInChildren<Text> ().text = "Tier 2";
		b2.onClick.AddListener(delegate{DisplayContents(type, 2);});

		GameObject go3 = GameObject.Instantiate(button1);
		go3.transform.SetParent (child_panel.transform,false);
		go3.transform.localPosition = new Vector3 (0, -200, 0.2f);
		Button b3 = go3.GetComponentInChildren<Button> ();
		b3.GetComponentInChildren<Text> ().text = "Cancel";

		//deactivate objects' colliders so they cannot
		//be triggered while using the menu
		GameObject.Find("UtilsHolder").GetComponent<Utils>().DeactivateColliders("Wall");
		GameObject.Find("UtilsHolder").GetComponent<Utils>().DeactivateColliders("Door");

		//lock user movement
		GameObject.Find("Camera Parent").GetComponent<CameraParentMovement>().movement_locked=true;
	}

	public void DisplayContents(int type, int tier){

		//this panel will allow the user to browse the
		//collection of available prefabs
		GameObject panel = Instantiate (options_panel);
		GameObject camera = GameObject.Find("Camera Parent");
		panel.transform.SetParent(camera.transform,false);
		panel.transform.localPosition = new Vector3 (0.8f, -0.2f, 2);
		panel.GetComponent<PanelManagement> ().nr_items = 1;
		panel.GetComponent<PanelManagement> ().type = 4;

		panel.transform.GetChild (1).localPosition = new Vector3 (0, 450, 0);
		panel.GetComponent<RectTransform> ().sizeDelta = new Vector2 (500, 1000);

		GameObject child_panel = new GameObject ("Child Panel");
		child_panel.AddComponent<CanvasRenderer>();
		child_panel.AddComponent<Image>();
		child_panel.transform.SetParent (panel.transform, false);
		child_panel.transform.localPosition = new Vector3 (0, 0, 0);
		child_panel.AddComponent<Mask> ();
		child_panel.GetComponent<CanvasRenderer> ().GetComponent<RectTransform> ().sizeDelta = new Vector2 (500, 800);
		Color c = child_panel.GetComponent<Image> ().color;
		c.a = 1.0f/255;
		child_panel.GetComponent<Image> ().color = c;
		panel.GetComponent<PanelManagement> ().SetChildPanel (child_panel);

		int height = 200;
		int im_ind = 0;

		//display a quit button
		GameObject quit_button = GameObject.Instantiate(object_display);
		quit_button.transform.SetParent (child_panel.transform,false);
		quit_button.transform.localPosition = new Vector3 (0, height, 0.2f);
		quit_button.transform.GetChild (2).GetComponent<Text> ().text = "Cancel";
		quit_button.transform.GetChild (2).GetComponent<Text> ().enabled = false;//not useful to display
		quit_button.transform.GetChild(1).GetComponent<Image>().sprite =
			Sprite.Create(back_img, new Rect(0, 0, back_img.width, back_img.height), new Vector2(0.5f, 0.5f));

		Object[] prefabs;
		if (type == 1)
			prefabs = Resources.LoadAll ("Doors/Tier " + tier + "/prefabs");
		else // type == 2
			prefabs = Resources.LoadAll ("Windows/Tier " + tier + "/prefabs");
		panel.GetComponent<PanelManagement> ().nr_items += prefabs.Length;

		GameObject scrollbar = GameObject.Find ("UIMaster").GetComponent<UIMasterScript> ().scrollbar;
		GameObject sb = GameObject.Instantiate (scrollbar);
		sb.GetComponent<Scrollbar> ().numberOfSteps = 0;
		sb.GetComponent<Scrollbar> ().size = 1.0f / panel.GetComponent<PanelManagement> ().nr_items;
		sb.GetComponent<Scrollbar> ().value = 0;
		sb.transform.SetParent (panel.transform, false);
		sb.transform.localPosition = new Vector3 (220, 0, 0);
		sb.transform.localScale = new Vector3 (1.5f, 3, 1);

		Object[] images;
		if (type == 1)
			images = Resources.LoadAll ("Doors/Tier " + tier + "/images");
		else // type == 2
			images = Resources.LoadAll ("Windows/Tier "+tier+"/images");
		
		foreach (Object o in prefabs) {
			GameObject go = GameObject.Instantiate(object_display);
			go.transform.SetParent (child_panel.transform,false);
			height -= 440;
			go.transform.localPosition = new Vector3 (0, height, 0.2f);
			if (type == 1)
				go.transform.GetChild (2).GetComponent<Text> ().text = "Doors/Tier " + tier + "/prefabs/" + o.name;
			else if (type == 2)
				go.transform.GetChild (2).GetComponent<Text> ().text = "Windows/Tier " + tier + "/prefabs/" + o.name;
			go.transform.GetChild (2).GetComponent<Text> ().enabled = false;

			//set image
			Texture2D aux_img=(Texture2D) images[im_ind];
			go.transform.GetChild(1).GetComponent<Image>().sprite = Sprite.Create(aux_img, new Rect(0, 0, aux_img.width, aux_img.height), new Vector2(0.5f, 0.5f));
			im_ind++;
		}

		child_panel.transform.GetChild(0).GetChild(0).GetComponent<Image> ().color = Color.red;
		if (type==1)
			panel.transform.GetChild (1).GetComponent<Text> ().text = "Doors";
		else if (type==2)
			panel.transform.GetChild (1).GetComponent<Text> ().text = "Windows";

		//deactivate objects' colliders so they cannot
		//be triggered while using the menu
		GameObject.Find("UtilsHolder").GetComponent<Utils>().DeactivateColliders("Wall");
		GameObject.Find("UtilsHolder").GetComponent<Utils>().DeactivateColliders("Door");

		//lock user movement
		GameObject.Find("Camera Parent").GetComponent<CameraParentMovement>().movement_locked=true;
	}

	public void StartPositioning(string path){

		GameObject op = Instantiate (dandw_positioner);
		GameObject object_prefab;
		object_prefab = Resources.Load (path) as GameObject;
		GameObject o = Instantiate (object_prefab);

		float height = GameObject.Find ("SceneLoader").GetComponent<LoadScene> ().height;
		if (path.Contains("Windows"))
			op.transform.position = new Vector3 (0, height * 9 / 16, 0);
		else
			op.transform.position = new Vector3 (0, height * 3 / 8, 0);
		o.transform.parent = op.transform;
		op.transform.localScale = new Vector3 (0, 0, 0);

		op.GetComponent<DandWPositioning> ().path = path;

		GameObject.Find ("UIMaster").GetComponent<UIMasterScript> ().stop = true;
		GameObject.Find ("UtilsHolder").GetComponent<Utils> ().SetWallsState (false);
	}
}
