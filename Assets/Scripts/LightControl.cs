using UnityEngine;
using System.Collections;

public class LightControl : MonoBehaviour {

	public bool sunny;
	// Use this for initialization
	void Start () {
		if (!sunny) {
			//GameObject[] children = transform.GetChild
			for(int i=4;i<8;i++)
				transform.GetChild(i).gameObject.GetComponent<Light>().intensity = 0;
			transform.GetChild(8).gameObject.GetComponent<Light>().intensity = 0.05f;
			transform.GetChild(8).gameObject.GetComponent<Light>().intensity = 0.12f;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
