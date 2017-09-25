using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PathDoTween : MonoBehaviour {

	[SerializeField]
	private Transform pathPointsParent;

	private List<Vector3> pathPointList = new List<Vector3> ();

	[SerializeField]
	private Transform followPoint;

	[SerializeField]
	private Color color;

	// Use this for initialization
	void Start () {
		foreach (Transform child in pathPointsParent) {
			pathPointList.Add (child.position);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnDrawGizmos(){
//		if (enabled) { // dkoontz
//			if (pathPointList.Count > 0)
//				DrawPathHelper (pathPointList, color, "gizmos");
//
//			foreach (Vector3 pathPoint in pathPointList) {
//				Gizmos.DrawWireSphere (pathPoint, 1);
//			}
//		}
	}


	private static void DrawPathHelper(List<Vector3> path, Color color, string method){
		List<Vector3> vector3s = path;

		//Line Draw:
		Vector3 prevPt = Interp(vector3s,0);
		Gizmos.color=color;
		int SmoothAmount = path.Count*20;
		for (int i = 1; i <= SmoothAmount; i++) {
			float pm = (float) i / SmoothAmount;
			Vector3 currPt = Interp(vector3s,pm);
			if(method == "gizmos"){
				Gizmos.DrawLine(currPt, prevPt);
			}else if(method == "handles"){
				Debug.LogError("iTween Error: Drawing a path with Handles is temporarily disabled because of compatability issues with Unity 2.6!");
				//UnityEditor.Handles.DrawLine(currPt, prevPt);
			}
			prevPt = currPt;
		}
	}

	private static Vector3 Interp(List<Vector3> pts, float t){
		int numSections = pts.Count - 3;
		int currPt = Mathf.Min(Mathf.FloorToInt(t * (float) numSections), numSections - 1);
		float u = t * (float) numSections - (float) currPt;

		Vector3 a = pts[currPt];
		Vector3 b = pts[currPt + 1];
		Vector3 c = pts[currPt + 2];
		Vector3 d = pts[currPt + 3];

		return .5f * (
			(-a + 3f * b - 3f * c + d) * (u * u * u)
			+ (2f * a - 5f * b + 4f * c - d) * (u * u)
			+ (-a + c) * u
			+ 2f * b
		);
	}

}
