using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour 
{
	public Transform babyTransform;
	private Camera cam;

	void Start () 
	{
		cam = GetComponent<Camera>();
		babyTransform = null;
	}

	void Update () 
	{
		if(babyTransform != null)
		{
			transform.LookAt(babyTransform.position);
		}
	}
}
