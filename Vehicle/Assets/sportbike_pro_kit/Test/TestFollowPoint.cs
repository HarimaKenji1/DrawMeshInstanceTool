using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFollowPoint : MonoBehaviour {
	private float currentSpeed = 0;

	private float totallSpeed = 0;

	private float goWhere = 0.2f;

	private float up = 0;

	public Transform follower;

	[SerializeField]
	private Transform startPoint;

	[SerializeField]
	private Transform endPoint;



	// Use this for initialization
	void Start () {
//		StartCoroutine ("ChangeSpeed");
	}
	
	// Update is called once per frame
	void Update () {
//		currentSpeed = Mathf.Lerp (currentSpeed,totallSpeed,0.1f);

		if(Input.GetKey("d"))
			transform.position += new Vector3 (0.05f, 0, 0);
		if (Input.GetKey ("a"))
			transform.position += new Vector3 (-0.05f, 0, 0);
		if (Input.GetKeyDown ("w"))
			currentSpeed += 0.05f;
//			transform.position += new Vector3 (0, 0, currentSpeed);
		if (Input.GetKeyDown ("s"))
			currentSpeed -= 0.05f;
//			transform.position += new Vector3 (0, 0, -currentSpeed);
		if (Input.GetKey ("q"))
			transform.position += new Vector3 (0, 0.05f, 0);
		if (Input.GetKey ("e"))
			transform.position += new Vector3 (0, -0.05f, 0);
		transform.position += new Vector3 (0, 0, currentSpeed);
		transform.position = new Vector3 (transform.position.x, follower.position.y, transform.position.z);
			
//		transform.RotateAround (Vector3.zero, Vector3.up, 30f * Time.deltaTime);
//		Debug.Log (currentSpeed);
	}

	IEnumerator ChangeSpeed(){
		while (true) {
			totallSpeed = 0.2f;
//			goWhere *= -1f;
			yield return new WaitForSeconds (12f);
			totallSpeed = 0.2f;
//			goWhere *= -1f;
			yield return new WaitForSeconds (12f);
		}
	}
}
