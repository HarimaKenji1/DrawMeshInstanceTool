using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPool : MonoBehaviour {

	[SerializeField]
	private BikePool pool;

	[SerializeField]
	private Transform followPoint;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("r")) {
			GameObject temp = pool.GetBike ();
			temp.SetActive (true);
			temp.GetComponent<ChasingBike> ().ReStartBike (Vector3.zero, followPoint);
		}
	}
}
