using UnityEngine;
using System.Collections;

public class LightControl : MonoBehaviour {

	public bool sunny;
	// Use this for initialization
	void Start () {
		if (sunny) {
			Light[] ls=GetComponentsInChildren<Light> ();
			for(int i=0;i<4;i++)
				ls [i].intensity = 0.4f;
			for(int i=4;i<8;i++)
				ls [i].intensity = 0.4f;
			ls [8].intensity = 0.45f;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
