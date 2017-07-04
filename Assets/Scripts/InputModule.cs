using UnityEngine;
using System.Collections;

// A = Fire1
// B = Fire2
public enum buttons {A,B,up,down,left,right};

public class InputModule : MonoBehaviour {

	[HideInInspector]
	public bool useCardboard;
	private bool ApressedOnce, BpressedOnce,ApressedContinuous,BpressedContinuous;
	private bool upPressedOnce,downPressedOnce,leftPressedOnce,rightPressedOnce,
	upPressedContinuous,downPressedContinuous,leftPressedContinuous,rightPressedContinuous;

	void Invalidate(){
		ApressedOnce = BpressedOnce =
		ApressedContinuous = BpressedContinuous =
		upPressedOnce = downPressedOnce = 
		leftPressedOnce = rightPressedOnce=
		upPressedContinuous = downPressedContinuous =
		leftPressedContinuous = rightPressedContinuous = false;
	}

	void Awake () {
		useCardboard = true;
		Invalidate ();
	}

	void LateUpdate () {
		Invalidate ();

		if (Application.platform == RuntimePlatform.Android && !useCardboard) {
			foreach (Touch t in Input.touches) {
				/*
				Ray ray = Camera.main.ScreenPointToRay (t.position);
				RaycastHit hit;

				if (Physics.Raycast (ray.origin, ray.direction, out hit)) {
					print (hit.transform.name);
				}
				*/

				//print (t.position);
				//print (t.phase.ToString());
				if (t.phase == TouchPhase.Began) {
					if (t.position.y > 30 && t.position.y < 110) {
						if (t.position.x > 1160 && t.position.x < 1240)
							ApressedOnce = true;
						else if (t.position.x > 1040 && t.position.x < 1120)
							BpressedOnce = true;
					}

					if (t.position.y > 100 && t.position.y < 160 &&
					    t.position.x > 140 && t.position.x < 180)
						upPressedOnce = true;
					else if (t.position.y > 30 && t.position.y < 80) {
						if (t.position.x > 40 && t.position.x < 120)
							leftPressedOnce = true;
						else if (t.position.x > 140 && t.position.x < 180)
							downPressedOnce = true;
						else if (t.position.x > 200 && t.position.x < 240)
							rightPressedOnce = true;
					}
				}
				else {
					if (t.position.y > 30 && t.position.y < 110) {
						if (t.position.x > 1160 && t.position.x < 1240)
							ApressedContinuous = true;
						else if (t.position.x > 1040 && t.position.x < 1120)
							BpressedContinuous = true;
					}

					if (t.position.y > 110 && t.position.y < 150 &&
						t.position.x > 120 && t.position.x < 180)
						upPressedContinuous = true;
					else if (t.position.y > 30 && t.position.y < 80) {
						if (t.position.x > 40 && t.position.x < 120)
							leftPressedContinuous = true;
						else if (t.position.x > 140 && t.position.x < 180)
							downPressedContinuous = true;
						else if (t.position.x > 200 && t.position.x < 240)
							rightPressedContinuous = true;
					}
				}
			}
			//print (Apressed + " " + Bpressed + " " + upPressed + " " + downPressed + " " + leftPressed + " " + rightPressed);
		}
	}



	public bool CheckForPress(buttons v, bool continuous = false){
		if (!continuous) {
			if (Application.platform == RuntimePlatform.Android && !useCardboard) {
				switch (v) {
				case buttons.A:
					return ApressedOnce;
				case buttons.B:
					return BpressedOnce;
				case buttons.up:
					return upPressedOnce;
				case buttons.down:
					return downPressedOnce;
				case buttons.left:
					return leftPressedOnce;
				case buttons.right:
					return rightPressedOnce;
				}
				return false;
			}
			else {
				switch (v) {
				case buttons.A:
					return Input.GetButtonDown ("Fire1");
				case buttons.B:
					return Input.GetButtonDown ("Fire2");
				case buttons.up:
					return (Input.GetKeyDown ("w") || Input.GetKeyDown (KeyCode.UpArrow));
				case buttons.down:
					return (Input.GetKeyDown ("s") || Input.GetKeyDown (KeyCode.DownArrow));
				case buttons.left:
					return (Input.GetKeyDown ("a") || Input.GetKeyDown (KeyCode.LeftArrow));
				case buttons.right:
					return (Input.GetKeyDown ("d") || Input.GetKeyDown (KeyCode.RightArrow));
				}
				return false;
			}
		}
		else {
			if (Application.platform == RuntimePlatform.Android && !useCardboard) {
				switch (v) {
				case buttons.A:
					return ApressedContinuous;
				case buttons.B:
					return BpressedContinuous;
				case buttons.up:
					return upPressedContinuous;
				case buttons.down:
					return downPressedContinuous;
				case buttons.left:
					return leftPressedContinuous;
				case buttons.right:
					return rightPressedContinuous;
				}
				return false;
			}
			else {
				switch (v) {
				case buttons.A:
					return Input.GetButton ("Fire1");
				case buttons.B:
					return Input.GetButton ("Fire2");
				case buttons.up:
					return (Input.GetKey ("w") || Input.GetKey (KeyCode.UpArrow));
				case buttons.down:
					return (Input.GetKey ("s") || Input.GetKey (KeyCode.DownArrow));
				case buttons.left:
					return (Input.GetKey ("a") || Input.GetKey (KeyCode.LeftArrow));
				case buttons.right:
					return (Input.GetKey ("d") || Input.GetKey (KeyCode.RightArrow));
				}
				return false;
			}
		}
	}
}
