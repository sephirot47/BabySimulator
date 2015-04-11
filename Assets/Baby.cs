using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Baby : MonoBehaviour 
{
	private static int numArticulations = 10;
	private float rotSpeed = 100.5f;

	private class Articulations
	{
		public static int  Head = 0,
						   ShoulderR = 1, ElbowR = 2, 
						   ShoulderL = 3, ElbowL = 4, 
						   HipR = 5, KneeR = 6,
						   HipL = 7, KneeL = 8,
						   Spine = 9;


		public static KeyCode   HeadKey = KeyCode.Y,
								ShoulderRKey = KeyCode.U, ElbowRKey = KeyCode.I, 
								ShoulderLKey = KeyCode.T, ElbowLKey = KeyCode.R, 
								HipRKey = KeyCode.G, KneeRKey = KeyCode.F,
								HipLKey = KeyCode.J, KneeLKey = KeyCode.K,
								SpineKey = KeyCode.H;
	}

	List<Transform> articulations;
	List<int> activeArticulations;

	int selectedArticulation = 0;

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

		activeArticulations = new List<int>();

		articulations[Articulations.Head] = GameObject.Find("ORG-neck").GetComponent<Transform>();

		articulations[Articulations.ShoulderR] = GameObject.Find("ORG-shoulder_R").GetComponent<Transform>();
		articulations[Articulations.ElbowR] = GameObject.Find("ORG-forearm_R").GetComponent<Transform>();

		articulations[Articulations.ShoulderL] = GameObject.Find("ORG-shoulder_L").GetComponent<Transform>();
		articulations[Articulations.ElbowL] = GameObject.Find("ORG-forearm_L").GetComponent<Transform>();

		articulations[Articulations.HipR] = GameObject.Find("ORG-thigh_R").GetComponent<Transform>();
		articulations[Articulations.KneeR] = GameObject.Find("ORG-shin_R").GetComponent<Transform>();

		articulations[Articulations.HipL] = GameObject.Find("ORG-thigh_L").GetComponent<Transform>();
		articulations[Articulations.KneeL] = GameObject.Find("ORG-shin_L").GetComponent<Transform>();

		articulations[Articulations.Spine] = GameObject.Find("ORG-spine").GetComponent<Transform>();
	}
	
	void Update () 
	{
		ReadActiveArticulations();
		
		float x = Input.GetAxis("Horizontal") * rotSpeed;
		float y = Input.GetAxis("Vertical") * rotSpeed;

		for(int i = 0; i < activeArticulations.Count; ++i)
		{
			Transform articulationTransform = articulations[ activeArticulations[i] ];

			articulationTransform.rotation *= Quaternion.AngleAxis(x, new Vector3(0, 1, 0));
			articulationTransform.rotation *= Quaternion.AngleAxis(y, new Vector3(1, 0, 0));
		}
	}

	void ReadActiveArticulations()
	{
		activeArticulations.Clear();

		if( Input.GetKey(Articulations.HeadKey) ) activeArticulations.Add(Articulations.Head);

		if( Input.GetKey(Articulations.ShoulderRKey) ) activeArticulations.Add(Articulations.ShoulderR);
		if( Input.GetKey(Articulations.ElbowRKey) ) activeArticulations.Add(Articulations.ElbowR);

		if( Input.GetKey(Articulations.ShoulderLKey) ) activeArticulations.Add(Articulations.ShoulderL);
		if( Input.GetKey(Articulations.ElbowLKey) ) activeArticulations.Add(Articulations.ElbowL);

		if( Input.GetKey(Articulations.HipRKey) ) activeArticulations.Add(Articulations.HipR);
		if( Input.GetKey(Articulations.KneeRKey) ) activeArticulations.Add(Articulations.KneeR);
		
		if( Input.GetKey(Articulations.HipLKey) ) activeArticulations.Add(Articulations.HipL);
		if( Input.GetKey(Articulations.KneeLKey) ) activeArticulations.Add(Articulations.KneeL);
		
		if( Input.GetKey(Articulations.SpineKey) ) activeArticulations.Add(Articulations.Spine);
	}
}
