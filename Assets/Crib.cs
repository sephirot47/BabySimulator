using UnityEngine;
using System.Collections;

public class Crib : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider col)
	{
		if(col.collider.gameObject.name.Contains("Baby"))
		{
			Debug.Log("FINISHED!");
		}
	}
}
