#define PROTOTYPE
using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.MeshOperations;
using ProBuilder2.Common;

public class pb_FlipNormals : Editor 
{
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Flip Face Normals &n", true,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 2)]
	public static bool VerifyFlipFaceNormals()
	{
		return pb_Editor.instance != null;
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Flip Face Normals &n", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 2)]
	public static void FlipFaceNormals()
	{
		pb_Menu_Commands.MenuFlipNormals(pbUtil.GetComponents<pb_Object>(Selection.transforms));

		EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
	}	
}
