using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ChasingBike : MonoBehaviour {

	private NavMeshAgent agent;

	public Transform parent;

	private List<Transform> destiantions = new List<Transform> ();

	private int currentTargetIndex = 0;

//	public GameObject ctrlHub;// making a link to corresponding bike's script

	private controlHub outsideControls;// making a link to corresponding bike's script

	private Rigidbody rig;

	private float currentHorizontal = 0;
	private float currentVerticle = 0;

	private float stoppingDistance = 5f;

	private float brakingDistance = 30f;

	private bool ifBrake = false;

	private BoxCollider bc;

	private float modelWidth = 0;

	private float modelHeight = 0;

	private bool[] ifHitList = new bool[9];

	private bool ifGoRight = false;

	private bool ifGoLeft = false;

	public Transform followPoint;

	private float deltaTime = 0.1f;

	private float time = 0;

	private float followPointSpeed = 0;

	private Vector3 followPointVelocity = new Vector3 (0, 0, 0);

	private Vector3 prePointPosition;

	private float preSpeed = 0;		//前一次的速度

	private Vector3 followPointDirection;

	private Vector3 prePointDirection;

	private bool ifFollowPointSpeedDown = false;

	private bool ifFasterThanFollowPoint = false;

	private float distanceToPoint = 0;

	private float distanceToSpeedDown = 10f;

	private float speedDownAddtion = 0;

	private float distanceToLowerAngle = 15f;

	private bool ifClose = false;

	private bool ifLowerAngle = false;

	private bool ifDistable = false;

	private bool ifCrashed = false;

	private bool once = true;

//	private bool ifRestart = false;

	private Vector3 restorePosition = Vector3.zero;

	public Text speedPerHour;

	[SerializeField]
	private float crashAngle01;//crashed status is on if bike have more Z(side fall) angle than this				纵向crash角度范围								// 70 sport, 55 chopper
	[SerializeField]
	private float crashAngle02;//crashed status is on if bike have less Z(side fall) angle than this 												// 290 sport, 305 chopper
	[SerializeField]
	private float crashAngle03;//crashed status is on if bike have more X(front fall) angle than this 				横向crash角度范围								// 70 sport, 70 chopper
	[SerializeField]
	private float crashAngle04;//crashed status is on if bike have more X(back fall) angle than this									

	[SerializeField]
	private Transform Rider;

	[SerializeField]
	private Transform startPoint;

	[SerializeField]
	private Transform endPoint;

	//如果当前速度方向与追踪点的位置方向不一致，则进行角度补正

	// Use this for initialization
	void Start () {
		outsideControls = GetComponent<controlHub>();// making a link to corresponding bike's script

		foreach (Transform child in parent) {
			destiantions.Add (child);
		}

		rig = GetComponent<Rigidbody> ();

		bc = GetComponent<BoxCollider> ();

		modelWidth = bc.size.x * transform.localScale.x;
		modelHeight = bc.size.x * transform.localScale.y;

		prePointPosition = followPoint.position;

//		Debug.Log (modelWidth);
	}
	
	// Update is called once per frame
	void Update () {
		if (followPoint != null) {
			GetFollowPointSpeed ();
			OnAgentChasing ();
		}
		CheckDisAbled ();
		CheckCrashed ();

//		if (Input.GetKeyDown ("r")) {
//			ReStartBike (new Vector3(0,1f,0),followPoint);
//		}

		if (Input.GetKeyDown ("q")) {
			BikeCrash ();
		}

		if ( (ifCrashed || ifDistable) && once) {
			once = false;
			StartCoroutine ("DisAbledVehicle");
		}

//		if (ifRestart) {
//			outsideControls.restartBike = true;
//			transform.position = restorePosition;
//		} else {
//			outsideControls.restartBike = false;
//			outsideControls.fullRestartBike = false;
//		}
		
		speedPerHour.text = (rig.velocity.magnitude * 3.6f) + "km/h";
//		Debug.Log (rig.velocity.magnitude * 3.6f);
//		Debug.Log ("ifGoLeft : " + ifGoLeft + "  ifGoRight : " + ifGoRight);
	}

	private void OnAgentChasing(){

//		AgentDone ();

//		CheckObstacle ();
//		DealWithObstacle ();
		if(followPoint != null)
			GetDistance();

		if (rig.velocity.magnitude >= followPointSpeed)
			ifFasterThanFollowPoint = true;
		else
			ifFasterThanFollowPoint = false;

//		Debug.Log ("velocity : " + rig.velocity.magnitude + " followSpeed : " + followPointSpeed);

		Vector3 desiredVelocity = new Vector3(followPoint.position.x,0,followPoint.position.z) - new Vector3(transform.position.x,0,transform.position.z);
		Vector3 velocity = Quaternion.Inverse (transform.rotation) * desiredVelocity;
		float angle = Mathf.Atan2 (velocity.x,velocity.z) * 180f / Mathf.PI;

		angle += AngleRevise ();

//		Debug.Log (angle / 250f + " " + ifClose);
		if (angle >= 0 || ifGoLeft) {
			if(ifLowerAngle)
				currentHorizontal = Mathf.Lerp (currentHorizontal, Mathf.Min (angle / 100f, 0.9f), 0.5f);
			else
				currentHorizontal = Mathf.Lerp (currentHorizontal, Mathf.Min (angle / 50f, 0.9f), 0.5f);
		} 
		if(angle < 0 || ifGoRight){
			if(ifLowerAngle)
				currentHorizontal = Mathf.Lerp (currentHorizontal, Mathf.Max (angle / 100f, -0.9f), 0.5f);
			else
				currentHorizontal = Mathf.Lerp (currentHorizontal, Mathf.Max (angle / 50f, -0.9f), 0.5f);
		}
//		Debug.Log (currentHorizontal);
//		if (Mathf.Abs (currentHorizontal) <= 0.05f && ifClose)
//			currentHorizontal = 0;
		outsideControls.Horizontal = currentHorizontal;
		if (Vector3.Dot (transform.forward.normalized,desiredVelocity) >= 0) {			//	如果目标点在前方
			if (Mathf.Abs (angle) > 50f /* || ifBrake */) {				//偏移角度大于50°
				outsideControls.rearBrakeOn = true;
				currentVerticle = Mathf.Lerp (currentVerticle, Mathf.Min (velocity.magnitude / 10f, 0.3f),0.2f) * 1.112f;
//				Debug.Log(1);
			} else {		//偏移角度小于50°
				if (ifFasterThanFollowPoint && ifFollowPointSpeedDown && ifClose) {		//速度大于追踪点，且追踪点正在减速
					currentVerticle = Mathf.Lerp (currentVerticle, Mathf.Min (velocity.magnitude / 10f, 0.3f),0.2f) * 1.112f;
					outsideControls.rearBrakeOn = true;
//					Debug.Log(" 2 : " + Mathf.Lerp (currentVerticle, Mathf.Min (velocity.magnitude / 20f, 0.1f),0.2f));
				} else if (ifFasterThanFollowPoint && !ifFollowPointSpeedDown && ifClose) {		//速度大于追踪点，追踪点没在减速
					currentVerticle = Mathf.Lerp (currentVerticle, Mathf.Min (velocity.magnitude / 10f, 0.3f),0.2f) * 1.112f;
					outsideControls.rearBrakeOn = true;
//					Debug.Log(" 3 : " + Mathf.Lerp (currentVerticle, Mathf.Min (velocity.magnitude / 20f, 0.1f),0.2f));
				} else {	//速度小于追踪点
					currentVerticle = Mathf.Lerp (currentVerticle, Mathf.Min (velocity.magnitude / 20f, 1f), 0.5f) / 1.112f;
					outsideControls.rearBrakeOn = false;
//					Debug.Log(4);
				}
			}
		} else {		//在后方
				outsideControls.rearBrakeOn = true;
				currentVerticle = Mathf.Lerp (currentVerticle, Mathf.Min (velocity.magnitude, 0.1f), 0.5f) * 1.112f;
//			Debug.Log(5);
		}
		if (currentVerticle >= 1f / 1.112f)
			currentVerticle = 1f / 1.112f;
		outsideControls.Vertical = currentVerticle;
		if(rig.velocity.magnitude <= 1f)
			outsideControls.rearBrakeOn = false;
	}

//	}

	private void GoToNextPosition(){
		currentTargetIndex++;
		if (currentTargetIndex > destiantions.Count - 1)
			currentTargetIndex = 0;
	}

	private void AgentDone(){
		float distance = Vector3.Distance (transform.position,destiantions [currentTargetIndex].position);
		if (distance <= brakingDistance && rig.velocity.magnitude > 20f)
			ifBrake = true;
		else
			ifBrake = false;
		if (distance <= stoppingDistance)
			GoToNextPosition ();
	}

	private void CheckObstacle(){
		//射线密度，射线方位安排，射线判定
		int i = 0;
		for (float x = bc.center.x - modelWidth / 2; x <= bc.center.x + modelWidth / 2; x += modelWidth / 2) {
			for (float y = bc.center.y + modelHeight / 2; y >= bc.center.y - modelHeight / 2; y -= modelHeight / 2) {
				Vector4 temp = new Vector4 (x, y, bc.size.z / 2,1);
				temp = transform.localToWorldMatrix * temp;
				Ray ray = new Ray (new Vector3(temp.x,temp.y,temp.z), transform.forward);
				if (i >= ifHitList.Length)
					i = 0;
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit, 30f)) {
					ifHitList [i] = true;
				}else
					ifHitList [i] = false;

//				Debug.Log ( i + " : " + ifHitList [i]);

				i++;
//				Debug.Log (y);

//				Debug.DrawRay(ray.origin,ray.direction);

			}
		}
	}

	private void DealWithObstacle(){
		if (ifHitList [0] || ifHitList [1] || ifHitList [2]) {		//左边撞了
			//GoRight
			ifGoRight = true;
			ifGoLeft = false;
		} else if (ifHitList [3] || ifHitList [4] || ifHitList [5] || ifHitList [6] || ifHitList [7] || ifHitList [8]) {	//中间或者右边撞了		//左边没撞
			//GoLeft
			ifGoRight = true;
			ifGoLeft = false;
		} else {
			ifGoLeft = false;
			ifGoRight = false;
		}
	}

	private void GetFollowPointSpeed(){		//计算追踪点的移动速度，并判断点是否减速，以此来控制自身的加减速
		if (time >= deltaTime && followPoint != null) {
			followPointVelocity = (followPoint.position - prePointPosition).normalized;
			followPointSpeed = Vector3.Distance (prePointPosition,followPoint.position) / time;		// f/s
			if (followPointSpeed  < preSpeed)
				ifFollowPointSpeedDown = true;
			else
				ifFollowPointSpeedDown = false;
			prePointPosition = followPoint.position;
			preSpeed = followPointSpeed;

			followPointDirection = followPoint.position - prePointPosition;
//			if (Vector3.Angle (prePointDirection, followPointDirection)) {
//				
//			}
			prePointDirection = followPointDirection;

			time = 0;
		}
		time += Time.deltaTime;
	}

	private void GetDistance(){		//得到与追踪点的距离，判断是否进入开始减速，速度同步的范围
		distanceToPoint = Vector3.Distance (transform.position, followPoint.position);
		speedDownAddtion = rig.velocity.magnitude * 3.6f / 5f;
		if (distanceToPoint <= distanceToSpeedDown + speedDownAddtion)
			ifClose = true;
		else
			ifClose = false;
		if (distanceToPoint <= distanceToLowerAngle)
			ifLowerAngle = true;
		else
			ifLowerAngle = false;
	}

	private float AngleRevise(){
		Vector3 velocity = Quaternion.Inverse (transform.rotation) * followPointVelocity;
		float angle = Mathf.Atan2 (velocity.x,velocity.z) * 180f / Mathf.PI;
		return angle;
	}

	private void CheckDisAbled(){		//检查是否到达结束点范围内
		float distance = Vector3.Distance (transform.position, endPoint.position);

		if (distance <= 10f) {
			ifDistable = true;
		}
	}

	private void CheckCrashed(){	//检查是否翻车
		if ((this.transform.localEulerAngles.z >= crashAngle01 && this.transform.localEulerAngles.z <= crashAngle02) || (this.transform.localEulerAngles.x >= crashAngle03 && this.transform.localEulerAngles.x <= crashAngle04) || outsideControls.crash) {
			ifCrashed = true;
//			Debug.Log ("Crash!");
		}
	}

	public void ReStartBike(Vector3 postion,Transform newPoint){
		StopCoroutine("DisAbledVehicle");
		outsideControls.restartBike = true;
		restorePosition = postion + new Vector3(0,1.2f,0);
		transform.position = restorePosition;
		followPoint = newPoint;

		followPointSpeed = 0;
		followPointDirection = Vector3.zero;
		prePointDirection = Vector3.zero;
		ifBrake = false;
		ifClose = false;
		ifCrashed = false;
		ifDistable = false;
		ifFasterThanFollowPoint = false;
		currentHorizontal = 0;
		currentVerticle = 0;
		outsideControls.Horizontal = 0;
		outsideControls.Vertical = 0;
		once = true;
	} 

	public void StoreBike(){
		followPoint = null;
		followPointSpeed = 0;
		followPointDirection = Vector3.zero;
		prePointDirection = Vector3.zero;
		ifBrake = false;
		ifClose = false;
		ifCrashed = false;
		ifDistable = false;
		ifFasterThanFollowPoint = false;
		currentHorizontal = 0;
		currentVerticle = 0;
		outsideControls.Horizontal = 0;
		outsideControls.Vertical = 0;
		outsideControls.crash = false;
		once = true;
		GameObject.Find ("BikePoolManager").GetComponent<BikePool> ().StoreBike (gameObject);
	}

	public void BeenHit(){
		
	}

	public void BikeCrash(){
		outsideControls.crash = true;
	}

	IEnumerator DisAbledVehicle(){		//disable摩托组件的协程
		yield return new WaitForSeconds (5f);
		StoreBike();
//		gameObject.SetActive (false);
//			Destroy (rig);
//			GetComponent<BoxCollider> ().enabled = false;
//			GetComponent<controlHub> ().enabled = false;
//			Destroy(GetComponent ("pro_bike5") );
//			Destroy(GetComponent ("skidMarks") );
//			Destroy(Rider.GetComponent ("biker_logic_mecanim") );
//			enabled = false;
		StopCoroutine ("DisAbledVehicle");
	}
		
		
}
