using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Baby : MonoBehaviour 
{
	int jumps = 0;

	private static int numArticulations = 4;
	private float rotSpeed = 10.0f, forwardSpeed = 0.1f, sideSpeed = 0.1f, jumpForce = 8.0f;

	public float bloodiness = 0.2f;

	private Quaternion originalHipLRotation, originalHipRRotation; 
	private Transform initialTransform;

	private Vector3 lastVelocity;
	
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

	void FixedUpdate() 
	{
		lastVelocity = GetComponent<Rigidbody> ().velocity;

		if( Input.GetKey(forwardKey) ) GetComponent<Rigidbody>().AddForce(new Vector3(0,0,1) * forwardSpeed, ForceMode.VelocityChange);
		if( Input.GetKey(backwardKey) ) GetComponent<Rigidbody>().AddForce(new Vector3(0,0,-1) * forwardSpeed, ForceMode.VelocityChange);

		if( Input.GetKey(leftKey) ) GetComponent<Rigidbody>().AddForce(new Vector3(-1,0,0) * sideSpeed, ForceMode.VelocityChange);
		if( Input.GetKey(rightKey) ) GetComponent<Rigidbody>().AddForce(new Vector3(1,0,0) * sideSpeed, ForceMode.VelocityChange);
		
		if(Input.GetKey(leftKey)) GetComponent<Rigidbody>().AddTorque(new Vector3(0,0,1) * sideSpeed, ForceMode.Impulse);
		else if(Input.GetKey(rightKey)) GetComponent<Rigidbody>().AddTorque(new Vector3(0,0,1) * -sideSpeed, ForceMode.Impulse);
		
		Transform t = articulations[Articulations.HipL];
		if(Input.GetKey(forwardKey))t.rotation *= Quaternion.AngleAxis(rotSpeed, new Vector3(0,1,0));
		
		t = articulations[Articulations.HipR];
		if(Input.GetKey(backwardKey)) t.rotation *= Quaternion.AngleAxis(-rotSpeed, new Vector3(0,1,0));
		 
		if( Input.GetKeyDown(jumpKey) && jumps <= 1)
		{
			++jumps;
			GetComponent<Rigidbody>().AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
		}
	}

	void OnCollisionEnter(Collision col)
	{
		if(col.collider.GetComponent<Transform>().gameObject.name.Contains("Floor") || col.gameObject.name.Contains("Floor") ||
		   col.collider.GetComponent<Transform>().gameObject.name.Contains("Wall") || col.gameObject.name.Contains("Wall"))
		{
			Vector3 hitPoint = col.contacts[0].point, 
					hitNormal = col.contacts[0].normal;
			float dotProduct = Vector3.Dot(lastVelocity, -hitNormal);

			Debug.Log (lastVelocity + ",   " + (-hitNormal) + ",   " + dotProduct);

			if(dotProduct >= 1.0f/bloodiness)
			{
				GameObject go = (GameObject)Instantiate(Resources.Load("BloodQuad"), hitPoint + hitNormal * 0.001f , Quaternion.LookRotation(-hitNormal));
				float a = Mathf.Max(0.5f, bloodiness * lastVelocity.magnitude) * 0.2f;
				go.transform.localScale = new Vector3(a,a,a);
			}

		}

		if(col.collider.GetComponent<Transform>().gameObject.name.Contains("Floor") || col.gameObject.name.Contains("Floor"))
		{
			jumps = 0;
		}
	}
}