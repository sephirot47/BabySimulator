using UnityEngine;
using System.Collections;

public class Crib : MonoBehaviour 
{
	public bool white;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider col)
	{
		if(col.GetComponentInParent<Transform>().gameObject.name.Contains("Baby"))
		{
			if(white) ++Core.scoreWhite;
			else ++Core.scoreBlack;

			Core.NextLevel();
		}
	}
}
