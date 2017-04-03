using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class PanelManagement : MonoBehaviour {

	[HideInInspector]
	public int type;// 1 -> meniu cu butoane, 2 -> meniu de selectie materiale
	[HideInInspector]
	public int nr_items;
	//butonul curent
	private int vertical_pos;
	private int horizontal_pos;

	void Start () {
		vertical_pos = 0;
		horizontal_pos = 0;
	}

	void Update () {
		int old_vpos = vertical_pos, old_hpos = horizontal_pos;

		if (type == 1) {
			if (Input.GetButtonDown ("Fire1") && transform.GetChild (2 + vertical_pos).transform.GetChild (1).GetComponent<Button> ().interactable) {
				//unlock camera movement
				GameObject.Find ("CameraParent").GetComponent<CameraParentMovement> ().movement_locked = false;
				//click
				transform.GetChild (2 + vertical_pos).transform.GetChild (1).GetComponent<Button> ().onClick.Invoke ();
				//destroy gameobject
				Destroy (gameObject);
			}

			if (Input.GetKeyDown ("w") || Input.GetKeyDown (KeyCode.UpArrow)) {
				vertical_pos--;
			} else if (Input.GetKeyDown ("s") || Input.GetKeyDown (KeyCode.DownArrow)) {
				vertical_pos++;
			}
			if (vertical_pos < 0)
				vertical_pos = nr_items - 1;
			else if (vertical_pos == nr_items)
				vertical_pos = 0;

			if (old_vpos != vertical_pos) {
				transform.GetChild (2 + old_vpos).transform.GetChild (0).GetComponent<Image> ().color = Color.white;
				transform.GetChild (2 + vertical_pos).transform.GetChild (0).GetComponent<Image> ().color = Color.red;
			}
		}
		else if (type == 2) {
			if (Input.GetButtonDown ("Fire1")) {
				if (horizontal_pos == 0) {
					//ExecuteEvents.Execute (transform.GetChild (2 + vertical_pos).GetComponent<Button> ().gameObject, ped, ExecuteEvents.submitHandler);
					/*
					PointerEventData ped = new PointerEventData(EventSystem.current);
					ExecuteEvents.Execute(transform.GetChild (2 + vertical_pos).GetComponent<Button> ().gameObject, ped, ExecuteEvents.pointerEnterHandler);
					ExecuteEvents.Execute(transform.GetChild (2 + vertical_pos).GetComponent<Button> ().gameObject, ped, ExecuteEvents.pointerDownHandler);
					ExecuteEvents.Execute(transform.GetChild (2 + vertical_pos).GetComponent<Button> ().gameObject, ped, ExecuteEvents.pointerUpHandler);
					*/
					//ExecuteEvents.Execute (transform.GetChild (2 + vertical_pos).GetComponent<Button> ().gameObject, ped, ExecuteEvents.submitHandler);
					transform.GetChild (2 + vertical_pos).GetComponent<Button> ().onClick.Invoke();
				}
				//transform.GetChild (2 + vertical_pos).GetComponent<Button> ().GetComponent<Image> ().color = transform.GetChild (2 + vertical_pos).GetComponent<Button> ().colors.highlightedColor;
				else {
					GameObject.Find ("CameraParent").GetComponent<CameraParentMovement> ().movement_locked = false;
					transform.GetChild (2 + nr_items - 1 + horizontal_pos).transform.GetChild (1).GetComponent<Button> ().onClick.Invoke ();
					Destroy (gameObject);
				}
			}

			if ((Input.GetKeyDown ("w") || Input.GetKeyDown (KeyCode.UpArrow)) && horizontal_pos==0) {
				vertical_pos--;
				if (vertical_pos < 0)
					vertical_pos = nr_items - 1;
			}
			else if ((Input.GetKeyDown ("s") || Input.GetKeyDown (KeyCode.DownArrow)) && horizontal_pos==0) {
				vertical_pos++;
				if (vertical_pos == nr_items)
					vertical_pos = 0;
			}
			else if (Input.GetKeyDown ("a") || Input.GetKeyDown (KeyCode.LeftArrow)) {
				horizontal_pos--;
				if (horizontal_pos < 0)
					horizontal_pos = 2;
			}
			else if (Input.GetKeyDown ("d") || Input.GetKeyDown (KeyCode.RightArrow)) {
				horizontal_pos++;
				if (horizontal_pos > 2)
					horizontal_pos = 0;
			}

			if (old_hpos != horizontal_pos) {
				if (old_hpos == 0) transform.GetChild (2 + vertical_pos).GetComponent<Button> ().GetComponent<Image> ().color = transform.GetChild (2 + vertical_pos).GetComponent<Button> ().colors.normalColor;				
				else transform.GetChild (2 + nr_items-1 + old_hpos).transform.GetChild (0).GetComponent<Image> ().color = Color.white;

				if (horizontal_pos==0) transform.GetChild (2 + vertical_pos).GetComponent<Button> ().GetComponent<Image> ().color = transform.GetChild (2 + vertical_pos).GetComponent<Button> ().colors.highlightedColor;
				else transform.GetChild (2 + nr_items-1 + horizontal_pos).transform.GetChild (0).GetComponent<Image> ().color = Color.red;	
			}
			else if (old_vpos != vertical_pos) {
				transform.GetChild (2 + old_vpos).GetComponent<Button> ().GetComponent<Image> ().color = transform.GetChild (2 + old_vpos).GetComponent<Button> ().colors.normalColor;
				transform.GetChild (2 + vertical_pos).GetComponent<Button> ().GetComponent<Image> ().color = transform.GetChild (2 + vertical_pos).GetComponent<Button> ().colors.highlightedColor;
			}

		}
	}
}
