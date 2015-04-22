using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Baby : MonoBehaviour 
{
	int jumps = 0;
	public int networkId = -1;

	private static int numArticulations = 4;
	private float rotSpeed = 500.0f, forwardSpeed = 0.1f, sideSpeed = 0.1f, jumpForce = 4.0f;

	public float bloodiness = 0.2f;

	private Quaternion originalHipLRotation, originalHipRRotation; 
	private Transform initialTransform;

	private Vector3 lastVelocity;

	public float explosionRadius = 10.0f, explosionForce = 10.0f;
	public KeyCode forwardKey, backwardKey, leftKey, rightKey, jumpKey, resetKey, explosionKey; 

	private ParticleSystem ps;

	private class Articulations
	{
		public static int HipR = 0, KneeR = 1,
						  HipL = 2, KneeL = 3;
	}

	List<Transform> articulations;

	void Start()
	{
		//networkId = -1;

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

		articulations [Articulations.HipL].rotation *= Quaternion.AngleAxis (180.0f, new Vector3 (0, 1, 0));

		//Particle system
		foreach(Transform t in transform)
		{
			if(t.gameObject.name == "Explosion")
			{
				ps = t.gameObject.GetComponent<ParticleSystem>();
				break;
			}
		}
		//
	}

	void FixedUpdate() 
	{
	}

	void Update()
	{
		if(gameObject == Core.babyMe)
		{
			if(Input.GetKeyDown(explosionKey))
			{
				Explode ();
			}

			lastVelocity = GetComponent<Rigidbody> ().velocity;
			
			if( Input.GetKey(forwardKey) ) GetComponent<Rigidbody>().AddForce(new Vector3(0,0,1) * forwardSpeed, ForceMode.VelocityChange);
			if( Input.GetKey(backwardKey) ) GetComponent<Rigidbody>().AddForce(new Vector3(0,0,-1) * forwardSpeed, ForceMode.VelocityChange);
			
			if( Input.GetKey(leftKey) ) GetComponent<Rigidbody>().AddForce(new Vector3(-1,0,0) * sideSpeed, ForceMode.VelocityChange);
			if( Input.GetKey(rightKey) ) GetComponent<Rigidbody>().AddForce(new Vector3(1,0,0) * sideSpeed, ForceMode.VelocityChange);
			
			if(Input.GetKey(leftKey)) GetComponent<Rigidbody>().AddTorque(new Vector3(0,0,1) * sideSpeed, ForceMode.Impulse);
			else if(Input.GetKey(rightKey)) GetComponent<Rigidbody>().AddTorque(new Vector3(0,0,1) * -sideSpeed, ForceMode.Impulse);
			
			Transform t = articulations[Articulations.HipL];
			if(Input.GetKey(forwardKey)) t.rotation *= Quaternion.AngleAxis(rotSpeed * Time.deltaTime, new Vector3(0,1,0));
			
			t = articulations[Articulations.HipR];
			if(Input.GetKey(backwardKey)) t.rotation *= Quaternion.AngleAxis(-rotSpeed * Time.deltaTime, new Vector3(0,1,0));
			
			t = articulations[Articulations.HipR];
			if(Input.GetKey(forwardKey)) t.rotation *= Quaternion.AngleAxis(rotSpeed * Time.deltaTime, new Vector3(0,1,0));
			
			t = articulations[Articulations.HipL];
			if(Input.GetKey(backwardKey)) t.rotation *= Quaternion.AngleAxis(-rotSpeed * Time.deltaTime, new Vector3(0,1,0));
			
			
			if( Input.GetKeyDown(jumpKey) && jumps <= 1)
			{
				++jumps;
				GetComponent<Rigidbody>().AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
			}
			
			NetworkManager.SendPositionToOthers(gameObject);
		}
	}

	public void Explode()
	{
		ParticleSystem ps = null;
		foreach(Transform t in transform)
		{
			if(t.gameObject.name == "Explosion")
			{
				ps = t.gameObject.GetComponent<ParticleSystem>();
				break;
			}
		}

		if(ps == null) return;

		ps.Stop();
		ps.Play();
		PushBabiesAround();
	}

	void PushBabiesAround()
	{
		foreach(GameObject b in Core.babies)
		{
			if(b.GetComponent<Baby>() == this) continue;
			b.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRadius);
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

	public Quaternion GetLeftLegRotation()
	{
		return articulations[Articulations.HipL].rotation;
	}
	
	public Quaternion GetRightLegRotation()
	{
		return articulations[Articulations.HipR].rotation;
	}
	
	public void SetRightLegRotation(Quaternion r)
	{
		articulations[Articulations.HipR].rotation = r;
	}

	public void SetLeftLegRotation(Quaternion r)
	{
		articulations[Articulations.HipL].rotation = r;
	}
}