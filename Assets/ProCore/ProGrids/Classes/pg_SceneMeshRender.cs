#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;

/**
 * Despite the MonoBehaviour inheritance, this is an Editor-only script.
 */
[ExecuteInEditMode]
public class pg_SceneMeshRender : MonoBehaviour
{
	// HideFlags.DontSaveInEditor isn't exposed for whatever reason, so do the bit math on ints 
	// and just cast to HideFlags.
	// HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable
	HideFlags SceneCameraHideFlags = (HideFlags) (1 | 4 | 8);

	void OnEnable()
	{
		// If something has gone terribly wrong and the grid object somehow survived a 'pg_Editor.OnDisable', self destruct here.
		if( !Resources.FindObjectsOfTypeAll(typeof(EditorWindow)).Any(x => x.ToString().Contains("pg_Editor")) )
			DestroyImmediate(gameObject);
	}

	private Material _material;
	private Material material 
	{
		get
		{
			if(_material == null)
			{
				MeshRenderer mr = GetComponent<MeshRenderer>();
				if(mr != null) _material = mr.sharedMaterial;
			}

			return _material;
		}
	}

	private Mesh _mesh;
	private Mesh mesh 
	{
		get
		{
			if(_mesh == null)
			{
				MeshFilter mf = GetComponent<MeshFilter>();
				_mesh = mf.sharedMesh;
			}

			return _mesh;
		}
	}

	void OnDestroy()
	{
		if(_mesh) DestroyImmediate(_mesh);
		if(_material) DestroyImmediate(_material);
	}

	void OnRenderObject()
	{
		// instead of relying on 'SceneCamera' string comparison, check if the hideflags match.
		// this could probably even just check for one bit match, since chances are that any 
		// game view camera isn't going to have hideflags set.
		if( (Camera.current.gameObject.hideFlags & SceneCameraHideFlags) != SceneCameraHideFlags || Camera.current.name != "SceneCamera" )
			return;

		Mesh msh = mesh;
		Material mat = material;

		if(mat == null || msh == null)
		{
			DestroyImmediate(gameObject);
			return;
		}

		material.SetPass(0);
		Graphics.DrawMeshNow(msh, Vector3.zero, Quaternion.identity, 0);
	}
}
#endif
