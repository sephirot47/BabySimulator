using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Baby : MonoBehaviour 
{
	private static int numArticulations = 4;
	private float rotSpeed = 30.0f, forwardSpeed = 0.1f, sideSpeed = 0.1f, jumpForce = 12.0f;

	private Quaternion originalHipLRotation, originalHipRRotation; 
	private Transform initialTransform;

	public KeyCode forwardKey, backwardKey, leftKey, rightKey, jumpKey, resetKey; 

	private class Articulations
	{
		public static int HipR = 0, KneeR = 1,
						  HipL = 2, KneeL = 3;
	}

	List<Transform> articulations;
	
	Transform hipR, kneeR,
		      hipL, kneeL;

	void Start() 
	{
		initialTransform = transform;

		articulations = new List<Transform>();
		for (int i = 0; i < numArticulations; ++i) articulations.Add (null);

		foreach (Transform t in GetComponentsInChildren<Transform>())
		{
			GameObject go = t.gameObject;
			if(go.name == "ORG-thigh_R") articulations[Articulations.HipR] = t;
			if(go.name == "ORG-shin_R")  articulations[Articulations.KneeR] = t;
			if(go.name == "ORG-thigh_L") articulations[Articulations.HipL] = t;
			if(go.name == "ORG-shin_L")  articulations[Articulations.KneeL] = t;
		}

		originalHipLRotation = new Quaternion(articulations[Articulations.HipL].rotation.x, articulations[Articulations.HipL].rotation.y,
		                                      articulations[Articulations.HipL].rotation.z, articulations[Articulations.HipL].rotation.w);

		originalHipRRotation = new Quaternion(articulations[Articulations.HipR].rotation.x, articulations[Articulations.HipR].rotation.y,
		                                      articulations[Articulations.HipR].rotation.z, articulations[Articulations.HipR].rotation.w);
	}

	bool Jumping()
	{
		return Mathf.Abs(GetComponent<Rigidbody>().velocity.y) > 1.0f;
	}

	void Update() 
	{
		if(Jumping())
		{
			if( Input.GetKey(forwardKey) ) GetComponent<Rigidbody>().AddForce(new Vector3(0,0,1) * forwardSpeed, ForceMode.VelocityChange);
			if( Input.GetKey(backwardKey) ) GetComponent<Rigidbody>().AddForce(new Vector3(0,0,-1) * forwardSpeed, ForceMode.VelocityChange);

			if( Input.GetKey(leftKey) ) GetComponent<Rigidbody>().AddForce(new Vector3(-1,0,0) * sideSpeed, ForceMode.VelocityChange);
			if( Input.GetKey(rightKey) ) GetComponent<Rigidbody>().AddForce(new Vector3(1,0,0) * sideSpeed, ForceMode.VelocityChange);
		}

		float speed = rotSpeed;
		if(!Jumping() && (Input.GetKey(forwardKey) || Input.GetKey(backwardKey)))
		{
			if( Input.GetKey(backwardKey) ) speed *= -1;

			Transform t = articulations[Articulations.HipL];
			t.rotation *= Quaternion.AngleAxis(speed, new Vector3(0,1,0));

			t = articulations[Articulations.HipR];
			t.rotation *= Quaternion.AngleAxis(-speed, new Vector3(0,1,0));

			if(Input.GetKey(forwardKey)) GetComponent<Rigidbody>().AddTorque(new Vector3(1,0,0) * sideSpeed, ForceMode.Impulse);
			else if(Input.GetKey(backwardKey)) GetComponent<Rigidbody>().AddTorque(new Vector3(-1,0,0) * sideSpeed, ForceMode.Impulse);
		}
		
		if( Input.GetKeyDown(jumpKey) && !Jumping())
		{
			GetComponent<Rigidbody>().AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
		}

		if( Input.GetKey(leftKey) ) GetComponent<Rigidbody>().AddTorque(new Vector3(0,0,1), ForceMode.Impulse);
		if( Input.GetKey(rightKey) ) GetComponent<Rigidbody>().AddTorque(new Vector3(0,0,-1), ForceMode.Impulse);
	}
}
