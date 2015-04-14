using UnityEngine;
using System.Collections;

public class Camera : MonoBehaviour 
{
	public Transform babyTransform;
	private Camera cam;

	void Start () 
	{
		cam = GetComponent<Camera>();
	}

	void Update () 
	{
		transform.LookAt(babyTransform.position);
	}
}
