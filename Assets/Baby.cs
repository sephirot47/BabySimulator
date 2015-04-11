using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Baby : MonoBehaviour 
{
	private static int numArticulations = 10;
	private float rotSpeed = 1.0f, jumpForce = 50.0f;

	private class Articulations
	{
		public static int  Head = 0,
						   ShoulderR = 1, ElbowR = 2, 
						   ShoulderL = 3, ElbowL = 4, 
						   HipR = 5, KneeR = 6,
						   HipL = 7, KneeL = 8,
						   Spine = 9;
	}

	List<Transform> articulations;
	
	Transform head, 
			  shoulderR, elbowR, handR, 
			  shoulderL, elbowL, handL, 
			  hipR, kneeR, footR,
		      hipL, kneeL, footL,
			  spine;

	void Start() 
	{
		articulations = new List<Transform>();
		for (int i = 0; i < numArticulations; ++i) articulations.Add (null);

		articulations[Articulations.Head] = GameObject.Find("ORG-neck").GetComponent<Transform>();
		articulations[Articulations.ShoulderR] = GameObject.Find("ORG-shoulder_R").GetComponent<Transform>();
		articulations[Articulations.ElbowR] = GameObject.Find("ORG-forearm_R").GetComponent<Transform>();
		articulations[Articulations.ShoulderL] = GameObject.Find("ORG-shoulder_L").GetComponent<Transform>();
		articulations[Articulations.HipR] = GameObject.Find("ORG-thigh_R").GetComponent<Transform>();
		articulations[Articulations.KneeR] = GameObject.Find("ORG-shin_R").GetComponent<Transform>();
		articulations[Articulations.HipL] = GameObject.Find("ORG-thigh_L").GetComponent<Transform>();
		articulations[Articulations.KneeL] = GameObject.Find("ORG-shin_L").GetComponent<Transform>();
		articulations[Articulations.Spine] = GameObject.Find("ORG-spine").GetComponent<Transform>();
	}
	
	void Update() 
	{
		float speed = rotSpeed;
		if( Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.W))
		{
			if( Input.GetKey(KeyCode.W) ) speed *= -1;

			Vector3 axis = new Vector3(0, 1, 0);

			Transform t = articulations[Articulations.HipL];
			t.rotation *= Quaternion.AngleAxis(speed, axis);

			t = articulations[Articulations.HipR];
			t.rotation *= Quaternion.AngleAxis(-speed, axis);
		}
		
		if( Input.GetKeyDown(KeyCode.Space) )
		{
			GetComponent<Rigidbody>().AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
		}

		if( Input.GetKey(KeyCode.A) ) GetComponent<Rigidbody>().AddTorque(new Vector3(-jumpForce, 0, 0), ForceMode.VelocityChange);
		if( Input.GetKey(KeyCode.D) ) GetComponent<Rigidbody>().AddTorque(new Vector3( jumpForce, 0, 0), ForceMode.VelocityChange);

	}
}
