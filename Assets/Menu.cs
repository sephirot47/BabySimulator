using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour 
{
	public GUIStyle textStyle;

	void Start () 
	{
	}
	
	void Update () 
	{
	}

	void OnGUI()
	{
		GUI.Label(new Rect (Screen.width / 2, Screen.height * 0.8f, 100, 100), new GUIContent ("Press SPACE"), textStyle);
	}
}
