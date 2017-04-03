using UnityEngine;
using System.Collections;

public class Lamps : MonoBehaviour {

	private int l;
	public GameObject[] lamps;

	void Start () {
		l = 0;
		Instantiate(lamps[l],transform.position, Quaternion.identity);
		//lamp.transform.parent = transform;
	}
	
	public void ChangeLamp(){
		l++;
		if (l == lamps.Length) l = 0;
		Instantiate(lamps[l],transform.position, Quaternion.identity);
	}
}
