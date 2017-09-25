using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikePool : MonoBehaviour{

	[SerializeField]
	private List<GameObject> bikes = new List<GameObject>();

	public GameObject GetBike(){
		if (bikes.Count > 0) {
			GameObject temp = bikes [0];
			bikes.RemoveAt (0);
			return temp;
		}
			
		else {
			Debug.LogWarning ("There Are No Bike In The Pool");
			return null;
		}
	}

	public void StoreBike(GameObject bike){
		bikes.Add (bike);
	}



}
