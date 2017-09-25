using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_DrawLine : MonoBehaviour {

	private List<Transform> childPositions = new List<Transform>();

	// Use this for initialization
	void Start () {
		
		foreach (Transform child in transform) {
			childPositions.Add (child);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnDrawGizmos(){
		Gizmos.color = Color.blue;
		if (childPositions.Count >= 2) {
			for (int i = 1; i < childPositions.Count; i++) {
				Gizmos.DrawLine (childPositions [i - 1].position, childPositions [i].position);
			}
		}
		#if UNITY_EDITOR
		Gizmos.color = Color.blue;
		if (childPositions.Count >= 2) {
			for (int i = 1; i < childPositions.Count; i++) {
				Gizmos.DrawLine (childPositions [i - 1].position, childPositions [i].position);
			}
		}
		#endif
	}

	#if UNITY_EDITOR
	[ContextMenu("Init")]
	void Init(){
		foreach (Transform child in transform) {
			childPositions.Add (child);
		}
	}
	#endif
}
