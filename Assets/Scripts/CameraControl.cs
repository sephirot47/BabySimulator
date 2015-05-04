using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour 
{
	
	public Transform target;
	public float targetHeight = 0.3f;
	public float distance = 12;
	public float maxDistance = 15;
	public float minDistance = 0.5f;
	public float xSpeed = 250.0f;
	public float ySpeed = 1000.0f;
	public float yMinLimit = -40;
	public float yMaxLimit = 80;
	public float zoomRate = 20;
	public float rotationDampening = 3.0f;
	public bool rotationDampeningEnabled = false;
	public Vector2 smoothing, sensitivity;
	private Vector2 _smoothMouse, _mouseAbsolute;
	
	public float camSmoothing = 0.2f;
	
	private Vector3 smoothCamVel = Vector3.zero; //don't touch
	private Vector3 camPosition;
	private float x = 0.0f;
	private float y = 0.0f;
	
	void Start () 
	{
		Vector3 angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;
		
		camPosition = transform.position; 
		
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody>()) GetComponent<Rigidbody>().freezeRotation = true;
	}
	
	void LateUpdate () 
	{
		if(target == null) return;

		/**** Work In Progress ****\/
		// Get raw mouse input for a cleaner reading on more sensitive mice.
		var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
		
		// Scale input against the sensitivity setting and multiply that against the smoothing value.
		mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));
		
		// Interpolate mouse movement over time to apply smoothing delta.
		_smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
		_smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);
		
		// Find the absolute mouse movement value from point zero.
		_mouseAbsolute += _smoothMouse;
*/
		x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
		y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

		float targetRotationAngle = target.eulerAngles.y;
		float currentRotationAngle = transform.eulerAngles.y;
		if(rotationDampeningEnabled) 
			x = Mathf.LerpAngle(currentRotationAngle, targetRotationAngle, rotationDampening * Time.deltaTime);
		
		distance -= (Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime) * zoomRate * Mathf.Abs(distance);
		distance = Mathf.Clamp(distance, minDistance, maxDistance);
		
		y = ClampAngle(y, yMinLimit, yMaxLimit);
		
		// ROTATE CAMERA:
		Quaternion rotation = Quaternion.Euler(y, x, 0);
		transform.rotation = rotation;
		
		// POSITION CAMERA:
		transform.position = target.position -(rotation * Vector3.forward * distance + new Vector3(0, -targetHeight, 0));

		/*
		// IS VIEW BLOCKED?
		RaycastHit hit; 
		Vector3 trueTargetPosition = target.position + Vector3.up * 0.5f;
		// Cast the line to check:
		Debug.Log(trueTargetPosition + " -> " + transform.position);
		Debug.DrawRay (trueTargetPosition, transform.position, Color.green);

		if( Physics.Linecast(trueTargetPosition, transform.position, out hit)) 
		{  
			// If so, shorten distance so camera is in front of object:
			float tempDistance = Vector3.Distance(trueTargetPosition, hit.point) - 0.28f;
			// Finally, rePOSITION the CAMERA:
			transform.position = target.position - (rotation * Vector3.forward * tempDistance + new Vector3(0,-targetHeight,0));
		}
		*/
	}
	
	private float ClampAngle(float angle, float min, float max) 
	{
		if (angle < -360.0f) angle += 360.0f;
		if (angle > 360.0f) angle -= 360.0f;
		return Mathf.Clamp (angle, min, max);
	}
}
