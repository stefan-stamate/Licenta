using UnityEngine;
using System.Collections;

public class Utils : MonoBehaviour {

	//functie pentru activarea/dezactivarea tuturor wallurilor
	//mai exact se seteaza active-ul din WallBehaviour pe state-ul primit
	//si astfel nu se mai intampla nimic daca apesi pe ele
	//de asemenea afecteaza si floor-urile si ceiling-urile
	public void SetWallsState(bool state){

		//deactivate walls
		GameObject[] walls=GameObject.FindGameObjectsWithTag("Wall");
		foreach (GameObject w in walls) {
			w.GetComponent<WallBehaviour> ().active = state;
		}

		GameObject[] floors=GameObject.FindGameObjectsWithTag("Floor");
		foreach (GameObject f in floors) {
			f.GetComponent<WallBehaviour> ().active = state;
		}

		GameObject[] ceilings=GameObject.FindGameObjectsWithTag("Ceiling");
		foreach (GameObject c in ceilings) {
			c.GetComponent<WallBehaviour> ().active = state;
		}
	}

	public void SetDoorsState(bool state){
		GameObject[] doors = GameObject.FindGameObjectsWithTag ("Door");
		foreach (GameObject d in doors) {
			d.GetComponent<DoorBehaviour> ().active = state;
		}
	}

	public void DeactivateColliders(string tag){
		GameObject[] objs = GameObject.FindGameObjectsWithTag (tag);
		foreach (GameObject o in objs) {
			o.GetComponent<Collider> ().enabled = false;
		}
	}
}
