using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class PanelManagement : MonoBehaviour {

	//-----------define-uri cu pozitiile copiilor -> scrollbar + butoane

	[HideInInspector]
	public int type;// 1 -> meniu cu butoane, 2 -> meniu de selectie materiale, 3 -> meniu selectie obiect
	[HideInInspector]
	public int nr_items;
	//butonul curent
	private int pos;
	private int old_vpos;//store the position in the vertical list when going sideways
	private int start_pos;//pozitia de la care se afiseaza 
	private GameObject child_panel;
	private int nr_steps=10;

	void Start () {
		pos = 0;
		start_pos = 0;
		GameObject.Find ("UIMaster").GetComponent<UIMasterScript> ().stop = true;
	}

	public void SetChildPanel(GameObject cp){
		child_panel = cp;
	}

	IEnumerator MoveItems(int dir, int size){
		for (int i = 0; i < nr_steps; i++) {
			//transform.GetChild (2).GetComponent<Scrollbar> ().value += dir * 1.0f / (nr_items * nr_steps);
			for (int j = 0; j < child_panel.transform.childCount; j++) {
				Vector3 p = child_panel.transform.GetChild (j).transform.localPosition;
				p.y += dir * size/nr_steps;
				child_panel.transform.GetChild (j).transform.localPosition = p;
			}
			yield return new WaitForSeconds (0.001f);
		}
	}

	IEnumerator PressButton (int pos){

		child_panel.transform.GetChild (pos).GetComponent<Button> ().GetComponent<Image> ().color = child_panel.transform.GetChild (pos).GetComponent<Button> ().colors.normalColor;
		PointerEventData ped = new PointerEventData(EventSystem.current);
		ExecuteEvents.Execute(child_panel.transform.GetChild (pos).GetComponent<Button> ().gameObject, ped, ExecuteEvents.pointerEnterHandler);
		ExecuteEvents.Execute(child_panel.transform.GetChild (pos).GetComponent<Button> ().gameObject, ped, ExecuteEvents.pointerDownHandler);
		yield return new WaitForSeconds(0.2f);
		ExecuteEvents.Execute(child_panel.transform.GetChild (pos).GetComponent<Button> ().gameObject, ped, ExecuteEvents.pointerUpHandler);
		ExecuteEvents.Execute(child_panel.transform.GetChild (pos).GetComponent<Button> ().gameObject, ped, ExecuteEvents.pointerExitHandler);
		yield return new WaitForSeconds(0.0001f);
		child_panel.transform.GetChild (pos).GetComponent<Button> ().GetComponent<Image> ().color = child_panel.transform.GetChild (pos).GetComponent<Button> ().colors.highlightedColor;
	}

	void ReactivateColliders(string tag){
		GameObject[] objs = GameObject.FindGameObjectsWithTag (tag);
		foreach (GameObject o in objs) {
			o.GetComponent<Collider> ().enabled = true;
		}
	}

	void Update () {
		int old_pos = pos, old_start_pos=start_pos;

		if (type == 1) {
			if (GameObject.Find("InputMaster").GetComponent<InputModule>().CheckForPress(buttons.A) &&
				child_panel.transform.GetChild (pos).transform.GetChild (1).GetComponent<Button> ().interactable) {
				GameObject.Find ("Camera Parent").GetComponent<CameraParentMovement> ().movement_locked = false;
				GameObject.Find ("UIMaster").GetComponent<UIMasterScript> ().stop = false;
				ReactivateColliders ("Wall");//--------------
				ReactivateColliders ("Floor");
				ReactivateColliders ("Ceiling");
				ReactivateColliders ("Door");
				child_panel.transform.GetChild (pos).transform.GetChild (1).GetComponent<Button> ().onClick.Invoke ();
				Destroy (gameObject);
			}
			if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.up)) {
				pos--;
				if (nr_items > 3) 
					transform.GetChild (2).gameObject.GetComponent<Scrollbar> ().value -= (float)1 / (nr_items - 1);
				if (start_pos > pos)
					start_pos--;
			}
			else if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.down)) {
				pos++;
				if (nr_items > 3)
					transform.GetChild (2).gameObject.GetComponent<Scrollbar> ().value += (float)1 / (nr_items - 1);
				if (nr_items > 3 && pos > start_pos + 2){
					start_pos++;
				}
			}
			if (pos < 0) {
				pos = nr_items - 1;
				if (nr_items > 3)
					start_pos = nr_items - 3;
				else
					start_pos = 0;

				if (nr_items > 3) 
					transform.GetChild (2).gameObject.GetComponent<Scrollbar> ().value = 1;
			}
			else if (pos == nr_items) {
				pos = 0;
				start_pos = 0;

				if (nr_items > 3) 
					transform.GetChild (2).gameObject.GetComponent<Scrollbar> ().value = 0;
			}

			if (old_pos != pos) {
				child_panel.transform.GetChild (old_pos).transform.GetChild (0).GetComponent<Image> ().color = Color.white;
				child_panel.transform.GetChild (pos).transform.GetChild (0).GetComponent<Image> ().color = Color.red;
			}

			if (old_start_pos != start_pos)
				StartCoroutine (MoveItems ((start_pos - old_start_pos), 200));
		}
		else if (type == 2) {
			if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.A)) {
				if (pos >= nr_items) {
					//ExecuteEvents.Execute (transform.GetChild (2 + vertical_pos).GetComponent<Button> ().gameObject, ped, ExecuteEvents.submitHandler);
					/*
					PointerEventData ped = new PointerEventData(EventSystem.current);
					ExecuteEvents.Execute(transform.GetChild (2 + vertical_pos).GetComponent<Button> ().gameObject, ped, ExecuteEvents.pointerEnterHandler);
					ExecuteEvents.Execute(transform.GetChild (2 + vertical_pos).GetComponent<Button> ().gameObject, ped, ExecuteEvents.pointerDownHandler);
					ExecuteEvents.Execute(transform.GetChild (2 + vertical_pos).GetComponent<Button> ().gameObject, ped, ExecuteEvents.pointerUpHandler);
					*/
					//ExecuteEvents.Execute (transform.GetChild (2 + vertical_pos).GetComponent<Button> ().gameObject, ped, ExecuteEvents.submitHandler);
					GameObject.Find ("Camera Parent").GetComponent<CameraParentMovement> ().movement_locked = false;
					GameObject.Find ("UIMaster").GetComponent<UIMasterScript> ().stop = false;
					ReactivateColliders ("Wall");
					ReactivateColliders ("Floor");
					ReactivateColliders ("Ceiling");
					ReactivateColliders ("Door");
					transform.GetChild (4 + pos - nr_items).GetChild (1).GetComponent<Button> ().onClick.Invoke ();
					Destroy (gameObject);
				}
				//transform.GetChild (2 + vertical_pos).GetComponent<Button> ().GetComponent<Image> ().color = transform.GetChild (2 + vertical_pos).GetComponent<Button> ().colors.highlightedColor;
				else {
					StartCoroutine (PressButton (pos));
					child_panel.transform.GetChild (pos).GetComponent<Button> ().onClick.Invoke ();
				}
			}

			if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.up)) {
				//if (nr_items > 4) {
				if (pos == nr_items)
					start_pos = nr_items - 4;
				else if (start_pos > pos - 1 && start_pos > 0)
					start_pos--;
				

				pos--;
				transform.GetChild (2).gameObject.GetComponent<Scrollbar> ().value -= (float)1 / (nr_items - 1);
				if (pos < 0) {
					old_vpos = 0;
					pos = nr_items + 1;
					transform.GetChild (2).gameObject.GetComponent<Scrollbar> ().value = 1;
				}
			}
			else if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.down)) {
				//if (nr_items > 4) {
				if (pos == nr_items + 1)
					start_pos = 0;
				else if (pos >= 3 && pos + 1 < nr_items && start_pos + 1 < nr_items - 3)
					start_pos++;
				

				pos++;//-------change scrollbar too
				transform.GetChild (2).gameObject.GetComponent<Scrollbar> ().value += (float)1 / (nr_items - 1);
				if (pos == nr_items)
					old_vpos = nr_items - 1;
				if (pos > nr_items + 1) {
					pos = 0;
					transform.GetChild (2).gameObject.GetComponent<Scrollbar> ().value = 0;
				}
			}
			else if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.left)) {
				if (pos == nr_items)
					pos = old_vpos;
				else if (pos == nr_items + 1)
					pos--;
				else {
					old_vpos = pos;
					pos = nr_items + 1;
				}
			}
			else if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.right)) {
				if (pos == nr_items + 1)
					pos = old_vpos;
				else if (pos == nr_items)
					pos++;
				else {
					old_vpos = pos;
					pos = nr_items;
				}
			}

			if (old_pos != pos) {
				if (old_pos >= nr_items)
					transform.GetChild (4 + old_pos - nr_items).transform.GetChild (0).GetComponent<Image> ().color = Color.white;
				else
					child_panel.transform.GetChild (old_pos).GetComponent<Button> ().GetComponent<Image> ().color = child_panel.transform.GetChild (old_pos).GetComponent<Button> ().colors.normalColor;

				if (pos >= nr_items)
					transform.GetChild (4 + pos - nr_items).transform.GetChild (0).GetComponent<Image> ().color = Color.red;
				else
					child_panel.transform.GetChild (pos).GetComponent<Button> ().GetComponent<Image> ().color = child_panel.transform.GetChild (pos).GetComponent<Button> ().colors.highlightedColor;
			}

			if (old_start_pos != start_pos)
				StartCoroutine (MoveItems ((start_pos - old_start_pos), 140));
		}
		else if (type == 3) {
			if (GameObject.Find("InputMaster").GetComponent<InputModule>().CheckForPress(buttons.A)) {
				
				GameObject.Find ("Camera Parent").GetComponent<CameraParentMovement> ().movement_locked = false;
				GameObject.Find ("UIMaster").GetComponent<UIMasterScript> ().stop = false;
				ReactivateColliders ("Wall");
				ReactivateColliders ("Floor");
				ReactivateColliders ("Ceiling");
				ReactivateColliders ("Door");

				if (pos == 0) {
					GameObject.Find ("ObjectInserter").GetComponent<ObjectInsertion> ().DisplayPrevFolder (
						transform.GetChild (1).GetComponent<Text> ().text);
				}
				else {
					if (GameObject.Find ("ObjectInserter").GetComponent<ObjectInsertion> ().directory_type [transform.GetChild (1).GetComponent<Text> ().text] == 0)
						GameObject.Find ("ObjectInserter").GetComponent<ObjectInsertion> ().DisplayContents (
							child_panel.transform.GetChild (pos).transform.GetChild (2).GetComponent<Text> ().text);
					else
						GameObject.Find ("ObjectInserter").GetComponent<ObjectInsertion> ().InsertObject (
							transform.GetChild (1).GetComponent<Text> ().text,
							child_panel.transform.GetChild (pos).transform.GetChild (2).GetComponent<Text> ().text);
				}
				Destroy (gameObject);
			}

			//start_pos here will indicate the row where the displayed items start
			if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.up)) {
				if (nr_items > 6) {
					if (start_pos == pos / 3)
						start_pos--;
				}

				pos -= 3;
				if (pos < 0) {
					if ((pos + 3) % 3 > (nr_items - 1) % 3) //if it doesn't a good slot to land on on last line
						pos = nr_items - 1;
					else
						pos = ((nr_items - 1) / 3) * 3 + (pos + 3) % 3;

					//if (pos > 5)
					start_pos = pos / 3 - 1;
					//else
					//start_pos = 0;
				}
			}
			else if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.down)) {
				if (nr_items > 6) {
					if (pos / 3 > start_pos)
						start_pos++;
				}

				pos += 3;
				if (pos > nr_items - 1) {
					if (pos / 3 == (nr_items - 1) / 3) {//if it jumps on the last line but there are some elements on it-------------see if start_pos is also good when this happens, there has to be 8 elements
						pos = nr_items - 1;
					} else {
						pos = pos % 3;
						start_pos = 0;
					}
				}
			}
			else if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.left)) {
				pos--;
				if (pos < 0) {
					pos = nr_items - 1;
					start_pos = pos / 3 - 1;
				} else if (pos % 3 == 2 && start_pos == (pos + 1) / 3) {
					start_pos--;
				}
					
			}
			else if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.right)) {
				pos++;
				if (pos == nr_items) {
					start_pos = 0;
					pos = 0;
				} else if (pos % 3 == 0 && start_pos == pos / 3 - 2) {
					start_pos++;
				}
			}

			if (old_pos != pos) {
				child_panel.transform.GetChild (old_pos).transform.GetChild (0).GetComponent<Image> ().color = Color.white;
				child_panel.transform.GetChild (pos).transform.GetChild (0).GetComponent<Image> ().color = Color.red;
			}

			if (old_start_pos != start_pos)
				StartCoroutine (MoveItems ((start_pos - old_start_pos), 440));
		}
		else if (type == 4) {
			if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.A)) {

				GameObject.Find ("Camera Parent").GetComponent<CameraParentMovement> ().movement_locked = false;
				GameObject.Find ("UIMaster").GetComponent<UIMasterScript> ().stop = false;
				ReactivateColliders ("Wall");
				ReactivateColliders ("Floor");
				ReactivateColliders ("Ceiling");
				ReactivateColliders ("Door");

				if (pos == 0) {
					if (transform.GetChild (1).GetComponent<Text> ().text == "Doors")
						GameObject.Find ("Door&WindowInserter").GetComponent<DandWInsertion> ().DisplayTiers (1);
					else if (transform.GetChild (1).GetComponent<Text> ().text == "Windows")
						GameObject.Find ("Door&WindowInserter").GetComponent<DandWInsertion> ().DisplayTiers (2);
				}
				else {
					GameObject.Find ("Door&WindowInserter").GetComponent<DandWInsertion> ().StartPositioning(
						child_panel.transform.GetChild(pos).GetChild(2).GetComponent<Text>().text);
				}
				Destroy (gameObject);
			}

			//start_pos here will indicate the row where the displayed items start
			if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.up)) {

				if (nr_items > 2) {
					transform.GetChild (3).gameObject.GetComponent<Scrollbar> ().value -= (float)1 / (nr_items - 1);
					if (start_pos == pos)
						start_pos--;
				}

				pos -= 1;
				if (pos < 0) {
					pos = nr_items - 1;
					start_pos = pos - 1;
					if (nr_items > 2)
						transform.GetChild (3).gameObject.GetComponent<Scrollbar> ().value = 1;
				}
			}
			else if (GameObject.Find ("InputMaster").GetComponent<InputModule> ().CheckForPress (buttons.down)) {
				if (nr_items > 2) {
					transform.GetChild (3).gameObject.GetComponent<Scrollbar> ().value += (float)1 / (nr_items - 1);
					if (pos > start_pos)
						start_pos++;
				}

				pos += 1;
				if (pos > nr_items - 1) {
					pos = 0;
					start_pos = 0;
					if (nr_items > 2)
						transform.GetChild (3).gameObject.GetComponent<Scrollbar> ().value = 0;
				}
			}

			if (old_pos != pos) {
				child_panel.transform.GetChild (old_pos).transform.GetChild (0).GetComponent<Image> ().color = Color.white;
				child_panel.transform.GetChild (pos).transform.GetChild (0).GetComponent<Image> ().color = Color.red;
			}

			if (old_start_pos != start_pos)
				StartCoroutine (MoveItems ((start_pos - old_start_pos), 440));
		}
	}
}
