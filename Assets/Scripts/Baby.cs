using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Baby : MonoBehaviour 
{
	int jumps = 0;
	public int networkId = -1;

	public static List<Baby> babies = new List<Baby>();
	public static List<NetworkPlayer> networkPlayers = new List<NetworkPlayer>();

	NetworkPlayer networkPlayer;

	private float moveSpeed = 0.08f;
	private float rotSpeed = 500.0f, jumpForce = 4.0f;

	public float bloodiness = 0.2f;
	
	private Transform initialTransform;

	private Vector3 lastVelocity;

	public float explosionRadius = 10.0f, explosionForce = 10.0f;
	public KeyCode forwardKey, backwardKey, leftKey, rightKey, jumpKey, resetKey, explosionKey; 

	private ParticleSystem ps;
	

	void Start()
	{
		if (Core.babyMe == null) 
		{
			Core.babyMe = this.gameObject;
			Camera.main.GetComponent<CameraControl> ().target = transform;
			networkPlayer = Network.player;
			babies.Add(this);
		}

		initialTransform = transform;


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
		if (Core.babyMe == this.gameObject) 
		{
		Vector3 realRight = Vector3.Cross(Camera.main.transform.forward, Vector3.up);
		Vector3 realForward = Vector3.Cross(Camera.main.transform.right, Vector3.up);
		if( Input.GetKey(forwardKey) ) GetComponent<Rigidbody>().AddForce(realForward * moveSpeed, ForceMode.VelocityChange);
		if( Input.GetKey(backwardKey) ) GetComponent<Rigidbody>().AddForce(-realForward * moveSpeed, ForceMode.VelocityChange);
		if( Input.GetKey(leftKey) ) GetComponent<Rigidbody>().AddForce(realRight * moveSpeed, ForceMode.VelocityChange);
		if( Input.GetKey(rightKey) ) GetComponent<Rigidbody>().AddForce(-realRight * moveSpeed, ForceMode.VelocityChange);
		}
	}

	void Update()
	{
		if (this.gameObject == Core.babyMe) {
			if (Input.GetKeyDown (explosionKey)) 
			{
				Explode ();
			}

			lastVelocity = GetComponent<Rigidbody> ().velocity;

			if (Input.GetKeyDown (jumpKey) && jumps <= 1) 
			{
				++jumps;
				GetComponent<Rigidbody> ().AddForce (new Vector3 (0, jumpForce, 0), ForceMode.Impulse);
			}
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

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) 
	{
		if(!networkPlayers.Contains(info.sender))
		{
			GameObject baby = Instantiate(Resources.Load("Baby"), initialTransform.position, Quaternion.identity) as GameObject;

			networkPlayers.Add(info.sender);

			baby.GetComponent<Baby>().networkPlayer = info.sender;
			babies.Add(baby.GetComponent<Baby>());
		}

		Vector3 pos = new Vector3();
		Quaternion rot = new Quaternion();
		if(stream.isWriting)
		{
			//SEND MY INFO TO ALL OTHER CLIENTS
			pos = transform.position;
			rot = transform.rotation;
			stream.Serialize(ref pos);
			stream.Serialize(ref rot);
		} 
		else 
		{
			//GET INFO
			if(info.sender == networkPlayer)
			{
				stream.Serialize(ref pos);
				stream.Serialize(ref rot);
				transform.position = pos;
				transform.rotation = rot;
			}
		}
	}
}