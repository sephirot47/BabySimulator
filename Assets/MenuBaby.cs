using UnityEngine;
using System.Collections;

public class MenuBaby : MonoBehaviour {

	public int baby;
	int counter = 0;

	void Start () {
	
	}
	
	void Update () 
	{
		float rotSpeed = 5.0f;
		foreach (Transform t in GetComponentsInChildren<Transform>())
		{
			GameObject go = t.gameObject;
			if(baby == 1 && go.name == "ORG-neck") go.transform.rotation *= Quaternion.AngleAxis(rotSpeed, Vector3.right);
			if(baby == 2 && go.name == "ORG-spine") go.transform.rotation *= Quaternion.AngleAxis(rotSpeed, Vector3.right);
			if(baby == 3 && go.name == "ORG-shoulder_R") go.transform.rotation *= Quaternion.AngleAxis(rotSpeed, Vector3.right);
			
			if(++counter % 2 == 0 && go.name.Contains("thigh")) go.transform.rotation = Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), 
			                                                                           						 new Vector3(Random.Range(0.0f, 100.0f), Random.Range(0.0f, 100.0f), Random.Range(0.0f, 100.0f)));
		}

		if(Input.GetKey(KeyCode.Space)) Application.LoadLevel("Scene1");
	}
}
