/**
 *	7/8/2013
 */
#define PRO

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;
using ProGrids2;
using ProCore.Common;

public class pg_Editor : EditorWindow
{

#region MEMBERS

	const float MIN_SNAP_VALUE = .01f;
	
	public static pg_Editor instance;
	Color pgButtonColor = new Color(.35f, .35f, .35f, 1f);
	Color oldColor;

	private bool useAxisConstraints = false;
	private bool snapEnabled = true;
	private SnapUnit snapUnit = SnapUnit.Meter;
	#if PRO
	private float snapValue = 1f;						// the actual snap value, taking into account unit size
	private float t_snapValue = 1f;						// what the user sees
	#else
	private float snapValue = .25f;
	private float t_snapValue = .25f;					
	#endif
	private bool drawGrid = true;
	private bool drawAngles = false;
	private float angleValue = 45f;
	private bool gridRepaint = true;

	public bool fullGrid { get; private set; }

	bool lockGrid = false;
	private Axis renderPlane = Axis.Y;
#endregion

#region CONSTANT

	const int VERSION = 20;
	const bool RESET_PREFS_REQ = true;

	#if PRO
	const int WINDOW_HEIGHT = 240;
	#else
	const int WINDOW_HEIGHT = 260;
	#endif

	const float METER = 1f;
	#if PRO
	const float CENTIMETER = .01f;
	const float MILLIMETER = .001f;
	const float INCH = 0.0253999862840074f;
	const float FOOT = 0.3048f;
	const float YARD = 1.09361f;
	const float PARSEC = 5f;
	#endif

	const int MAX_LINES = 150;				// the maximum amount of lines to display on screen in either direction
	public static float alphaBump;			// Every tenth line gets an alpha bump by this amount
	const int BUTTON_SIZE = 46;

	private Texture2D gui_SnapEnabled_on, gui_SnapEnabled_off;
	private Texture2D gui_GridEnabled_on, gui_GridEnabled_off;
	private Texture2D gui_AngleEnabled_on, gui_AngleEnabled_off;
	private Texture2D gui_SnapToGrid;//, gui_SnapToGrid_pressed;
	private Texture2D gui_Divider;
	private Texture2D gui_fullGrid_on, gui_fullGrid_off;
	private Texture2D gui_PlaneX_on, gui_PlaneX_off;
	private Texture2D gui_PlaneY_on, gui_PlaneY_off;
	private Texture2D gui_PlaneZ_on, gui_PlaneZ_off;
	private Texture2D gui_LockGrid_on, gui_LockGrid_off;

	private GUIContent gc_SnapToGrid = new GUIContent((Texture2D)null, "Snaps all selected objects to grid.");
	private GUIContent gc_SnapEnabled 	= new GUIContent((Texture2D)null, "Toggles snapping on or off.");
	private GUIContent gc_GridEnabled 	= new GUIContent((Texture2D)null, "Toggles drawing of guide lines on or off.  Note that object snapping is not affected by this setting.");
	private GUIContent gc_AngleEnabled 	= new GUIContent((Texture2D)null, "If on, ProGrids will draw angled line guides.  Angle is settable in degrees.");
	private GUIContent gc_AngleValue 	= new GUIContent((Texture2D)null, "The degree at which angle guides will be drawn.");
	private GUIContent gc_RenderPlaneX 	= new GUIContent((Texture2D)null, "Renders a grid on the X plane.");
	private GUIContent gc_RenderPlaneY 	= new GUIContent((Texture2D)null, "Renders a grid on the Y plane.");
	private GUIContent gc_RenderPlaneZ 	= new GUIContent((Texture2D)null, "Renders a grid on the Z plane.");
	private GUIContent gc_RenderPerspectiveGrid = new GUIContent("", "Renders a 3d grid in perspective mode.");
	private GUIContent gc_LockGrid 		= new GUIContent("", "Lock the perspective grid center in place.");
#endregion

#region PREFERENCES
	/** Settings **/
	public Color gridColorX, gridColorY, gridColorZ;
	public Color gridColorX_primary, gridColorY_primary, gridColorZ_primary;

	// private bool lockOrthographic;

	public void LoadPreferences()
	{
		if( (EditorPrefs.HasKey(pg_Constant.PGVersion) ? EditorPrefs.GetInt(pg_Constant.PGVersion) : 0) < VERSION )
		{
			EditorPrefs.SetInt(pg_Constant.PGVersion, VERSION);
	
			if(RESET_PREFS_REQ)
			{
				pg_Preferences.ResetPrefs();
				// Debug.Log("Resetting Prefs");
			}
		}

		if(EditorPrefs.HasKey(pg_Constant.SnapEnabled))
		{
			snapEnabled = EditorPrefs.GetBool(pg_Constant.SnapEnabled);
		}

		SetSnapValue( 
			EditorPrefs.HasKey(pg_Constant.GridUnit) ? (SnapUnit)EditorPrefs.GetInt(pg_Constant.GridUnit) : SnapUnit.Meter,
			EditorPrefs.HasKey(pg_Constant.SnapValue) ? EditorPrefs.GetFloat(pg_Constant.SnapValue) : 1,
			EditorPrefs.HasKey(pg_Constant.SnapMultiplier) ? EditorPrefs.GetInt(pg_Constant.SnapMultiplier) : 100
			);

		if(EditorPrefs.HasKey(pg_Constant.UseAxisConstraints))
			useAxisConstraints = EditorPrefs.GetBool(pg_Constant.UseAxisConstraints);

		lockGrid = EditorPrefs.GetBool(pg_Constant.LockGrid);

		if(lockGrid)
		{
			if(EditorPrefs.HasKey(pg_Constant.LockedGridPivot))	
			{
				string piv = EditorPrefs.GetString(pg_Constant.LockedGridPivot);
				string[] pivsplit = piv.Replace("(", "").Replace(")", "").Split(',');
				
				float x, y, z;
				if( !float.TryParse(pivsplit[0], out x) ) goto NoParseForYou;
				if( !float.TryParse(pivsplit[1], out y) ) goto NoParseForYou;
				if( !float.TryParse(pivsplit[2], out z) ) goto NoParseForYou;

				pivot.x = x;
				pivot.y = y;
				pivot.z = z;

NoParseForYou:	
				;	// appease the compiler
			}

		}

		fullGrid = EditorPrefs.GetBool(pg_Constant.PerspGrid);		
		
		renderPlane = EditorPrefs.HasKey(pg_Constant.GridAxis) ? (Axis)EditorPrefs.GetInt(pg_Constant.GridAxis) : Axis.Y;
		
		SharedProperties.useAxisConstraints = useAxisConstraints;

		alphaBump = (EditorPrefs.HasKey("pg_alphaBump")) ? EditorPrefs.GetFloat("pg_alphaBump") : pg_Preferences.ALPHA_BUMP;

		gridColorX = (EditorPrefs.HasKey("gridColorX")) ? pg_Util.ColorWithString(EditorPrefs.GetString("gridColorX")) : pg_Preferences.GRID_COLOR_X;
		gridColorX_primary = new Color(gridColorX.r, gridColorX.g, gridColorX.b, gridColorX.a + alphaBump);
		gridColorY = (EditorPrefs.HasKey("gridColorY")) ? pg_Util.ColorWithString(EditorPrefs.GetString("gridColorY")) : pg_Preferences.GRID_COLOR_Y;
		gridColorY_primary = new Color(gridColorY.r, gridColorY.g, gridColorY.b, gridColorY.a + alphaBump);
		gridColorZ = (EditorPrefs.HasKey("gridColorZ")) ? pg_Util.ColorWithString(EditorPrefs.GetString("gridColorZ")) : pg_Preferences.GRID_COLOR_Z;
		gridColorZ_primary = new Color(gridColorZ.r, gridColorZ.g, gridColorZ.b, gridColorZ.a + alphaBump);

		drawGrid = (EditorPrefs.HasKey("showgrid")) ? EditorPrefs.GetBool("showgrid") : pg_Preferences.SHOW_GRID;
	}

	private GUISkin sixBySevenSkin;
#endregion

#region ENUM

	public enum Axis {
		X = 0x1,
		Y = 0x2,
		Z = 0x4,
		NegX = 0x8,
		NegY = 0x16,
		NegZ = 0x32
	}

	/**
	 * Multiplies a Vector3 using the inverse value of an axis (eg, Axis.Y becomes Vector3(1, 0, 1) )
	 */
	private Vector3 InverseAxisMask(Vector3 v, Axis axis)
	{
		switch(axis)
		{
			case Axis.X:
			case Axis.NegX:
				return Vector3.Scale(v, new Vector3(0f, 1f, 1f));
			
			case Axis.Y:
			case Axis.NegY:
				return Vector3.Scale(v, new Vector3(1f, 0f, 1f));

			case Axis.Z:
			case Axis.NegZ:
				return Vector3.Scale(v, new Vector3(1f, 1f, 0f));

			default:
				return v;
		}
	}

	private Vector3 AxisMask(Vector3 v, Axis axis)
	{
		switch(axis)
		{
			case Axis.X:
			case Axis.NegX:
				return Vector3.Scale(v, new Vector3(1f, 0f, 0f));
			
			case Axis.Y:
			case Axis.NegY:
				return Vector3.Scale(v, new Vector3(0f, 1f, 0f));

			case Axis.Z:
			case Axis.NegZ:
				return Vector3.Scale(v, new Vector3(0f, 0f, 1f));

			default:
				return v;
		}
	}

	public enum SnapUnit {
		Meter,
		#if PRO
		Centimeter,
		Millimeter,
		Inch,
		Foot,
		Yard,
		Parsec
		#endif
	}

	public float SnapUnitValue(SnapUnit su) {
		switch(su)
		{
			case SnapUnit.Meter:
				return METER;
			#if PRO
			case SnapUnit.Centimeter:
				return CENTIMETER;
			case SnapUnit.Millimeter:
				return MILLIMETER;
			case SnapUnit.Inch:
				return INCH;
			case SnapUnit.Foot:
				return FOOT;
			case SnapUnit.Yard:
				return YARD;
			case SnapUnit.Parsec:
				return PARSEC;
			#endif
			default:
				return METER;
		}
	}
#endregion

#region MENU
	
	[MenuItem("Tools/ProGrids/About", false, 0)]
	public static void MenuAboutProGrids()
	{
		pg_AboutWindow.Init("Assets/ProCore/ProGrids/About/pc_AboutEntry_ProGrids.txt", true);
	}

	[MenuItem("Tools/ProGrids/ProGrids Window", false, 15)]
	public static void InitProGrids()
	{
		EditorWindow.GetWindow(typeof(pg_Editor), false, "PG", true).autoRepaintOnSceneChange = true;
		SceneView.RepaintAll();
	}

	[MenuItem("Tools/ProGrids/Cycle SceneView Projection", false, 101)]
	public static void CyclePerspective()
	{
		if(instance == null) return;
		
		SceneView scnvw = SceneView.lastActiveSceneView;
		if(scnvw == null) return;

		int nextOrtho = EditorPrefs.GetInt(pg_Constant.LastOrthoToggledRotation);
		switch(nextOrtho)
		{
			case 0:
				scnvw.orthographic = true;
				scnvw.LookAt(scnvw.pivot, Quaternion.Euler(Vector3.zero));
				nextOrtho++;
				break;

			case 1:
				scnvw.orthographic = true;
				scnvw.LookAt(scnvw.pivot, Quaternion.Euler(Vector3.up * -90f));	
				nextOrtho++;
				break;

			case 2:
				scnvw.orthographic = true;
				scnvw.LookAt(scnvw.pivot, Quaternion.Euler(Vector3.right * 90f));	
				nextOrtho++;
				break;
			
			case 3:
				scnvw.orthographic = false;
				scnvw.LookAt(scnvw.pivot, new Quaternion(-0.1f, 0.9f, -0.2f, -0.4f) );	
				nextOrtho = 0;
				break;
		}
		EditorPrefs.SetInt(pg_Constant.LastOrthoToggledRotation, nextOrtho);
	}

	[MenuItem("Tools/ProGrids/Cycle SceneView Projection", true, 101)]
	[MenuItem("Tools/ProGrids/Increase Grid Size", true, 203)]
	[MenuItem("Tools/ProGrids/Decrease Grid Size", true, 202)]
	public static bool VerifyGridSizeAdjustment()
	{
		return instance != null;
	}

	[MenuItem("Tools/ProGrids/Decrease Grid Size", false, 202)]
	public static void DecreaseGridSize()
	{
		if(instance == null) return;
		
		int multiplier = EditorPrefs.HasKey(pg_Constant.SnapMultiplier) ? EditorPrefs.GetInt(pg_Constant.SnapMultiplier) : 100;
		float val = EditorPrefs.HasKey(pg_Constant.SnapValue) ? EditorPrefs.GetFloat(pg_Constant.SnapValue) : 1f;

		multiplier /= 2;

		instance.SetSnapValue(instance.snapUnit, val, multiplier);
		instance.Repaint();
	}

	[MenuItem("Tools/ProGrids/Increase Grid Size", false, 203)]
	public static void IncreaseGridSize()
	{
		if(instance == null) return;

		int multiplier = EditorPrefs.HasKey(pg_Constant.SnapMultiplier) ? EditorPrefs.GetInt(pg_Constant.SnapMultiplier) : 100;
		float val = EditorPrefs.HasKey(pg_Constant.SnapValue) ? EditorPrefs.GetFloat(pg_Constant.SnapValue) : 1f;

		multiplier *= 2;

		instance.SetSnapValue(instance.snapUnit, val, multiplier);
		instance.Repaint();
	}

	[MenuItem("Tools/ProGrids/Nudge Perspective Backward", true, 304)]
	[MenuItem("Tools/ProGrids/Nudge Perspective Forward", true, 305)]
	[MenuItem("Tools/ProGrids/Reset Perspective Nudge", true, 306)]
	public static bool VerifyMenuNudgePerspective()
	{
		return instance != null && !instance.fullGrid && !instance.ortho && instance.lockGrid;
	}

	[MenuItem("Tools/ProGrids/Nudge Perspective Backward", false, 304)]
	public static void MenuNudgePerspectiveBackward()
	{
		if(!instance.lockGrid) return;
		instance.offset -= instance.snapValue;
		SceneView.RepaintAll();
	}

	[MenuItem("Tools/ProGrids/Nudge Perspective Forward", false, 305)]
	public static void MenuNudgePerspectiveForward()
	{
		if(!instance.lockGrid) return;
		instance.offset += instance.snapValue;
		SceneView.RepaintAll();
	}

	[MenuItem("Tools/ProGrids/Reset Perspective Nudge", false, 306)]
	public static void MenuNudgePerspectiveReset()
	{
		if(!instance.lockGrid) return;
		instance.offset = 0;
		SceneView.RepaintAll();
	}
#endregion

#region INITIALIZATION

	#if DEBUG
		GameObject pivotGo;
		GameObject scenePivotGo;
		GameObject scenePlaneIntercept;
	#endif

	public void OnEnable()
	{
		#if DEBUG
			pivotGo = new GameObject();//GameObject.CreatePrimitive(PrimitiveType.Cube);
			scenePivotGo = new GameObject();//GameObject.CreatePrimitive(PrimitiveType.Cube);
			scenePlaneIntercept = new GameObject();//GameObject.CreatePrimitive(PrimitiveType.Cube);
		#endif

		instance = this;

		HookSceneView();
		LoadGUIResources();
		LoadPreferences();
		autoRepaintOnSceneChange = true;
		SetSharedSnapValues(snapEnabled, snapValue);
		this.minSize = new Vector2(BUTTON_SIZE+4, WINDOW_HEIGHT);
		this.maxSize = new Vector2(BUTTON_SIZE+4, WINDOW_HEIGHT);

		gridRepaint = true;
		SceneView.RepaintAll();
	}

	public void OnFocus()
	{
		SetSharedSnapValues(snapEnabled, snapValue);
		SceneView.RepaintAll();
	}

	public void OnDisable()
	{
		if(!EditorApplication.isPlayingOrWillChangePlaymode)
			pg_GridRenderer.Destroy();

		#if DEBUG
		if(pivotGo != null)
			DestroyImmediate(pivotGo);
		if(scenePivotGo != null)
			DestroyImmediate(scenePivotGo);
		
		if(scenePlaneIntercept != null)
			DestroyImmediate(scenePivotGo);
		#endif

		SceneView.RepaintAll();
		SetSharedSnapValues(false, snapValue);
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
	}

	public void HookSceneView()
	{
		if(SceneView.onSceneGUIDelegate != this.OnSceneGUI)
		{
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
		}
	}

	public void LoadGUIResources()
	{
		// toggleStyle.margin = new RectOffset(5,5,5,5);

		gui_SnapEnabled_on 		= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_Snap_On");
		gui_SnapEnabled_off 	= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_Snap_Off");
		
		gui_GridEnabled_on 		= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_Vis_On");
		gui_GridEnabled_off 	= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_Vis_Off");
		
		gui_AngleEnabled_on 	= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_AngleVis_On");
		gui_AngleEnabled_off 	= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_AngleVis_Off");

		gui_SnapToGrid 			= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_PushToGrid_Normal");
		// gui_SnapToGrid_pressed	= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_PushToGrid_Pressed");
		gc_SnapToGrid.image 	= gui_SnapToGrid;

		gui_Divider				= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_Divider");

		gui_fullGrid_on			= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_PGrid_3D_On");
		gui_fullGrid_off		= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_PGrid_3D_Off");

		gui_PlaneX_on			= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_PGrid_X_On");
		gui_PlaneX_off			= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_PGrid_X_Off");

		gui_PlaneY_on			= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_PGrid_Y_On");
		gui_PlaneY_off			= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_PGrid_Y_Off");
		
		gui_PlaneZ_on			= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_PGrid_Z_On");
		gui_PlaneZ_off			= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_PGrid_Z_Off");

		gui_LockGrid_on			= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_PGrid_Lock_On");
		gui_LockGrid_off		= (Texture2D)Resources.Load("GUI/ProGridsToggles/ProGrids2_GUI_PGrid_Lock_Off");
	}
#endregion 

#region INTERFACE

	GUIStyle pbStyle = new GUIStyle();
	const int TOGGLE_WIDTH = 20;

	bool guiInitialized = false;
	public void OnGUI()
	{	
		if(!guiInitialized) 
		{
			GUISkin t_skin = EditorGUIUtility.GetBuiltinSkin(EditorGUIUtility.isProSkin ? EditorSkin.Scene : EditorSkin.Inspector);
			pbStyle.padding = new RectOffset(2,2,1,1);
			pbStyle.border = new RectOffset(6, 6, 4, 4);
			pbStyle.normal.background = t_skin.GetStyle("Button").normal.background;
			pbStyle.margin = new RectOffset(4,2,2,2);
			pbStyle.contentOffset = new Vector2(0,0);

		}
		
		if(!EditorGUIUtility.isProSkin)
		{
			oldColor = GUI.backgroundColor;
			GUI.backgroundColor = pgButtonColor;
		}

		EditorGUI.BeginChangeCheck();
		t_snapValue = EditorGUILayout.FloatField("", t_snapValue,
			GUILayout.MinWidth(BUTTON_SIZE),
			GUILayout.MaxWidth(BUTTON_SIZE));
		if(EditorGUI.EndChangeCheck()) {
			#if PRO
			// If user sets new snap value, make the default multiplier 100 again.
			SetSnapValue(snapUnit, t_snapValue, 100);
			#endif
		}

		gc_SnapEnabled.image = snapEnabled ? gui_SnapEnabled_on : gui_SnapEnabled_off;
		if( GUILayout.Button(gc_SnapEnabled, pbStyle, GUILayout.MaxWidth(BUTTON_SIZE)) )
			SetSnapEnabled(!snapEnabled);

		gc_GridEnabled.image = drawGrid ? gui_GridEnabled_on : gui_GridEnabled_off;
		if( GUILayout.Button(gc_GridEnabled, pbStyle, GUILayout.MaxWidth(BUTTON_SIZE)) )
			SetGridEnabled(!drawGrid);

		// if(!ortho)	GUI.enabled = false;
		gc_AngleEnabled.image = drawAngles ? gui_AngleEnabled_on : gui_AngleEnabled_off;
		if( GUILayout.Button(gc_AngleEnabled, pbStyle, GUILayout.MaxWidth(BUTTON_SIZE)) )
			SetDrawAngles(!drawAngles);

		EditorGUI.BeginChangeCheck();
		angleValue = EditorGUILayout.FloatField(gc_AngleValue, angleValue,
			GUILayout.MinWidth(BUTTON_SIZE),
			GUILayout.MaxWidth(BUTTON_SIZE));
		if(EditorGUI.EndChangeCheck()) {
			SceneView.RepaintAll();
		}
		// GUI.enabled = true;

		if(GUILayout.Button(gc_SnapToGrid, pbStyle, GUILayout.MaxWidth(BUTTON_SIZE)))//, GUILayout.MaxWidth(50), GUILayout.MinHeight(16)))
			SnapToGrid(Selection.transforms);

		GUILayout.Label(gui_Divider, GUILayout.MaxWidth(BUTTON_SIZE));

		gc_RenderPlaneX.image = (renderPlane & Axis.X) == Axis.X && !fullGrid ? gui_PlaneX_on : gui_PlaneX_off;
		if(GUILayout.Button(gc_RenderPlaneX, pbStyle, GUILayout.MaxWidth(BUTTON_SIZE)))
			SetRenderPlane(Axis.X);

		gc_RenderPlaneY.image = (renderPlane & Axis.Y) == Axis.Y && !fullGrid ? gui_PlaneY_on : gui_PlaneY_off;
		if(GUILayout.Button(gc_RenderPlaneY, pbStyle, GUILayout.MaxWidth(BUTTON_SIZE)))
			SetRenderPlane(Axis.Y);
		
		gc_RenderPlaneZ.image = (renderPlane & Axis.Z) == Axis.Z && !fullGrid ? gui_PlaneZ_on : gui_PlaneZ_off;
		if(GUILayout.Button(gc_RenderPlaneZ, pbStyle, GUILayout.MaxWidth(BUTTON_SIZE)))
			SetRenderPlane(Axis.Z);

		gc_RenderPerspectiveGrid.image = fullGrid ? gui_fullGrid_on : gui_fullGrid_off;
		if(GUILayout.Button(gc_RenderPerspectiveGrid, pbStyle, GUILayout.MaxWidth(BUTTON_SIZE)))
		{
			fullGrid = !fullGrid;
			gridRepaint = true;
			EditorPrefs.SetBool(pg_Constant.PerspGrid, fullGrid);
			SceneView.RepaintAll();
		}

		gc_LockGrid.image = lockGrid ? gui_LockGrid_on : gui_LockGrid_off;
		if(GUILayout.Button(gc_LockGrid, pbStyle, GUILayout.MaxWidth(BUTTON_SIZE)))
		{
			lockGrid = !lockGrid;
			EditorPrefs.SetBool(pg_Constant.LockGrid, lockGrid);
			EditorPrefs.SetString(pg_Constant.LockedGridPivot, pivot.ToString());
			
			// if we've modified the nudge value, reset the pivot here
			if(!lockGrid)
				offset = 0f;

			gridRepaint = true;

			SceneView.RepaintAll();
		}

		#if PROFILE_TIMES
		if(GUILayout.Button("Times"))
			profiler.DumpTimes();
		if(GUILayout.Button("Reset"))
			profiler.ClearValues();
		#endif	

		if(!EditorGUIUtility.isProSkin)
			GUI.backgroundColor = oldColor;
	}

	private void OpenProGridsPopup()
	{
		if( EditorUtility.DisplayDialog(
			"Upgrade to ProGrids",				// Title
			"Enables all kinds of super-cool features, like different snap values, more units of measurement, and angles.",						  // Message
			"Upgrade",							// Okay
			"Cancel"							// Cancel
			))
			// #if UNITY_4
			// AssetStore.OpenURL(pg_Constant.ProGridsUpgradeURL);
			// #else
			Application.OpenURL(pg_Constant.ProGridsUpgradeURL);
			// #endif
	}
#endregion

#region ONSCENEGUI

	private Transform lastTransform;
	const string AXIS_CONSTRAINT_KEY = "s";
	const string TEMP_DISABLE_KEY = "d";
	private bool toggleAxisConstraint = false;
	private bool toggleTempSnap = false;
	private Vector3 lastPosition = Vector3.zero;
	private Vector3 pivot = Vector3.zero, lastPivot = Vector3.zero;
	private Vector3 camDir = Vector3.zero, prevCamDir = Vector3.zero;
	private float lastDistance = 0f;	///< Distance from camera to pivot at the last time the grid mesh was updated.
	public float offset = 0f;

	#if PROFILE_TIMES
	pb_Profiler profiler = new pb_Profiler();
	#endif

	public bool ortho { get; private set; }
	private bool prevOrtho = false;

	float planeGridDrawDistance = 0f;

	public void OnSceneGUI(SceneView scnview)
	{
		if(EditorApplication.isPlayingOrWillChangePlaymode) return;	// don't snap stuff in play mode

		Event e = Event.current;

		if (e.Equals(Event.KeyboardEvent(AXIS_CONSTRAINT_KEY)))
		{
			toggleAxisConstraint = true;
			SharedProperties.useAxisConstraints = !useAxisConstraints;
		}

		if (e.Equals(Event.KeyboardEvent(TEMP_DISABLE_KEY)))
		{
			SetSharedSnapValues(false, snapValue);
			toggleTempSnap = true;
		}

		if(e.type == EventType.KeyUp)
		{
			toggleAxisConstraint = false;
			SharedProperties.useAxisConstraints = useAxisConstraints;
			toggleTempSnap = false;
			bool used = true;

			switch(e.keyCode)
			{
				case KeyCode.Equals:	
					IncreaseGridSize();
					break;

				case KeyCode.Minus:	
					DecreaseGridSize();
					break;

				case KeyCode.LeftBracket:
					if(VerifyMenuNudgePerspective())
						MenuNudgePerspectiveBackward();
					break;

				case KeyCode.RightBracket:
					if(VerifyMenuNudgePerspective())
						MenuNudgePerspectiveForward();
					break;

				case KeyCode.Alpha0:
					if(VerifyMenuNudgePerspective())
						MenuNudgePerspectiveReset();
					break;

				case KeyCode.Backslash:
					CyclePerspective();
					break;

				default:
					used = false;
					break;
			}

			SetSharedSnapValues(snapEnabled, snapValue);

			if(used)
				e.Use();
		}

		Camera cam = Camera.current;
		
		ortho = cam.orthographic && IsRounded(scnview.rotation.eulerAngles.normalized);

		camDir = pg_Util.CeilFloor( pivot - cam.transform.position );

		if(ortho && !prevOrtho)
			pg_GridRenderer.Destroy();

		prevOrtho = ortho;

		float camDistance = Vector3.Distance(cam.transform.position, lastPivot);	// distance from camera to pivot

		if(fullGrid)
		{
			pivot = lockGrid || Selection.activeTransform == null ? pivot : Selection.activeTransform.position;
		}
		else
		{
			Vector3 sceneViewPlanePivot = pivot;

			Ray ray = new Ray(cam.transform.position, cam.transform.forward);
			Plane plane = new Plane(Vector3.up, pivot);
			float dist;

			// the only time a locked grid should ever move is if it's pivot is out 
			// of the camera's frustum.
			if( (lockGrid && !cam.InFrustum(pivot)) || !lockGrid )
			{
				if(plane.Raycast(ray, out dist))
					sceneViewPlanePivot = ray.GetPoint( Mathf.Min(dist, planeGridDrawDistance/2f) );				
				else
					sceneViewPlanePivot = ray.GetPoint( Mathf.Min(cam.farClipPlane/2f, planeGridDrawDistance/2f) );
			}

			if(lockGrid)
			{
				pivot = InverseAxisMask(sceneViewPlanePivot, renderPlane) + AxisMask(pivot, renderPlane);
			}
			else
			{
				pivot = Selection.activeTransform == null ? pivot : Selection.activeTransform.position;

				if( Selection.activeTransform == null || !cam.InFrustum(pivot) )
				{
					pivot = InverseAxisMask(sceneViewPlanePivot, renderPlane) + AxisMask(Selection.activeTransform == null ? pivot : Selection.activeTransform.position, renderPlane);
				}
			}

			#if DEBUG
				pivotGo.transform.position = pivot;
				scenePivotGo.transform.position = SceneView.lastActiveSceneView.pivot;
				scenePlaneIntercept.transform.position = sceneViewPlanePivot;

				Handles.Label(scenePivotGo.transform.position, "SceneView Pivot: " + scenePivotGo.transform.position.ToString());
				Handles.Label(scenePlaneIntercept.transform.position, "\nPivot Plane Intercept: " + scenePlaneIntercept.transform.position.ToString());
				Handles.Label(pivot, "ProGrids Pivot: " + pivot.ToString());

			#endif
		}

		if(drawGrid)
		{	
			if(ortho)
			{
				// ortho don't care about pivots
				DrawGridOrthographic(cam);
			}
			else
			{
				#if PROFILE_TIMES
				profiler.LogStart("DrawGridPerspective");
				#endif

				if( gridRepaint || pivot != lastPivot || Mathf.Abs(camDistance - lastDistance) > lastDistance/2 || camDir != prevCamDir)
				{
					prevCamDir = camDir;
					gridRepaint = false;
					lastPivot = pivot;
					lastDistance = camDistance;

					if(fullGrid)
					{
						//  if perspective and 3d, use pivot like normal
						pg_GridRenderer.DrawGridPerspective( cam, pivot, snapValue, new Color[3] { gridColorX, gridColorY, gridColorZ }, alphaBump );
					}
					else
					{
						if( (renderPlane & Axis.X) == Axis.X)
							planeGridDrawDistance = pg_GridRenderer.DrawPlane(cam, pivot + Vector3.right*offset, Vector3.up, Vector3.forward, snapValue, gridColorX, alphaBump);

						if( (renderPlane & Axis.Y) == Axis.Y)
							planeGridDrawDistance = pg_GridRenderer.DrawPlane(cam, pivot + Vector3.up*offset, Vector3.right, Vector3.forward, snapValue, gridColorY, alphaBump);

						if( (renderPlane & Axis.Z) == Axis.Z)
							planeGridDrawDistance = pg_GridRenderer.DrawPlane(cam, pivot + Vector3.forward*offset, Vector3.up, Vector3.right, snapValue, gridColorZ, alphaBump);

					}
				}
				#if PROFILE_TIMES
				profiler.LogFinish("DrawGridPerspective");
				#endif
			}
		}

		// Always keep track of the selection
		if(!Selection.transforms.Contains(lastTransform))
		{
			if(Selection.activeTransform)
			{
				lastTransform = Selection.activeTransform;
				lastPosition = Selection.activeTransform.position;
			}
		}

		if(!snapEnabled || GUIUtility.hotControl < 1)
			return;
		
		// Bugger.SetKey("Toggle Snap Off", toggleTempSnap);

		/**
		 *	Snapping (for all the junk in PG, this method is literally the only code that actually affects anything).
		 */
		if(Selection.activeTransform)
		{		
			if( !FuzzyEquals(lastTransform.position, lastPosition) )
			{
				Transform selected = lastTransform;

				if( !toggleTempSnap )
				{
					Vector3 old = selected.position;
					Vector3 mask = old - lastPosition;
					
					bool constraintsOn = toggleAxisConstraint ? !useAxisConstraints : useAxisConstraints;

					if(constraintsOn)
						selected.position = pg_Util.SnapValue(old, mask, snapValue);
					else
						selected.position = pg_Util.SnapValue(old, snapValue);					
		
					Vector3 offset = selected.position - old;

					OffsetTransforms(Selection.transforms, selected, offset);
				}

				lastPosition = selected.position;
			}
		}
	}
#endregion

#region GRAPHICS

	GameObject go;

	private void DrawGridOrthographic(Camera cam)
	{
		Axis camAxis = AxisWithVector(Camera.current.transform.TransformDirection(Vector3.forward).normalized);

		if(drawGrid) {
			switch(camAxis)
			{
				case Axis.X:
				case Axis.NegX:
					DrawGridOrthographic(cam, camAxis, gridColorX_primary, gridColorX);
					break;

				case Axis.Y:
				case Axis.NegY:
					DrawGridOrthographic(cam, camAxis, gridColorY_primary, gridColorY);
					break;

				case Axis.Z:
				case Axis.NegZ:
					DrawGridOrthographic(cam, camAxis, gridColorZ_primary, gridColorZ);
					break;
			}
		}
	}

	int PRIMARY_COLOR_INCREMENT = 10;
	Color previousColor;
	private void DrawGridOrthographic(Camera cam, Axis camAxis, Color primaryColor, Color secondaryColor)
	{
		previousColor = Handles.color;
		Handles.color = primaryColor;
	
		// !-- TODO: Update this stuff only when necessary.  Currently it runs evvverrrryyy frame
		Vector3 bottomLeft 	= pg_Util.SnapToFloor(cam.ScreenToWorldPoint(Vector2.zero), snapValue);
		Vector3 bottomRight = pg_Util.SnapToFloor(cam.ScreenToWorldPoint(new Vector2(cam.pixelWidth, 0f)), snapValue);
		Vector3 topLeft 	= pg_Util.SnapToFloor(cam.ScreenToWorldPoint(new Vector2(0f, cam.pixelHeight)), snapValue);
		Vector3 topRight 	= pg_Util.SnapToFloor(cam.ScreenToWorldPoint(new Vector2(cam.pixelWidth, cam.pixelHeight)), snapValue);

		Vector3 axis = VectorWithAxis(camAxis);

		float width = Vector3.Distance(bottomLeft, bottomRight);
		float height = Vector3.Distance(bottomRight, topRight);

		// Shift lines to 10m forward of the camera
		bottomLeft 	+= axis*10f;
		topRight 	+= axis*10f;
		bottomRight += axis*10f;
		topLeft 	+= axis*10f;

		/** 
		 *	Draw Vertical Lines 
		 */
		Vector3 cam_right = cam.transform.right;
		Vector3 cam_up = cam.transform.up;

		float _snapVal = snapValue;

		int segs = (int)Mathf.Ceil(width / _snapVal) + 2;

		float n = 2f;
		while(segs > MAX_LINES) {
			_snapVal = _snapVal*n;
			segs = (int)Mathf.Ceil(width / _snapVal ) + 2;
			n++;
		}

		/// Screen start and end
		Vector3 bl = cam_right.Sum() > 0 ? pg_Util.SnapToFloor(bottomLeft, cam_right, _snapVal*PRIMARY_COLOR_INCREMENT) : pg_Util.SnapToCeil(bottomLeft, cam_right, _snapVal*PRIMARY_COLOR_INCREMENT);
		Vector3 start 	= bl - cam_up * (height+_snapVal*2);
		Vector3 end 	= bl + cam_up * (height+_snapVal*2);

		segs += PRIMARY_COLOR_INCREMENT; 

		/// The current line start and end
		Vector3 line_start = Vector3.zero;
		Vector3 line_end = Vector3.zero;

		for(int i = -1; i < segs; i++)
		{
			line_start = start + (i * (cam_right * _snapVal));
			line_end = end + (i * (cam_right * _snapVal) );
			Handles.color = i % PRIMARY_COLOR_INCREMENT == 0 ? primaryColor : secondaryColor;
			Handles.DrawLine( line_start, line_end );
		}

		/** 
		 * Draw Horizontal Lines
		 */
		segs = (int)Mathf.Ceil(height / _snapVal) + 2;

		n = 2;
		while(segs > MAX_LINES) {
			_snapVal = _snapVal*n;
			segs = (int)Mathf.Ceil(height / _snapVal ) + 2;
			n++;
		}

		Vector3 tl = cam_up.Sum() > 0 ? pg_Util.SnapToCeil(topLeft, cam_up, _snapVal*PRIMARY_COLOR_INCREMENT) : pg_Util.SnapToFloor(topLeft, cam_up, _snapVal*PRIMARY_COLOR_INCREMENT);
		start 	= tl - cam_right * (width+_snapVal*2);
		end 	= tl + cam_right * (width+_snapVal*2);

		segs += (int)PRIMARY_COLOR_INCREMENT; 

		for(int i = -1; i < segs; i++)
		{
			line_start = start + (i * (-cam_up * _snapVal));
			line_end = end + (i * (-cam_up * _snapVal));
			Handles.color = i % PRIMARY_COLOR_INCREMENT == 0 ? primaryColor : secondaryColor;
			Handles.DrawLine(line_start, line_end);
		}

		#if PRO
		if(drawAngles)
		{
			Vector3 cen = pg_Util.SnapValue(((topRight + bottomLeft) / 2f), snapValue);

			float half = (width > height) ? width : height;

			float opposite = Mathf.Tan( Mathf.Deg2Rad*angleValue ) * half;

			Vector3 up = cam.transform.up * opposite;
			Vector3 right = cam.transform.right * half;

			Vector3 bottomLeftAngle 	= cen - (up+right);
			Vector3 topRightAngle 		= cen + (up+right);

			Vector3 bottomRightAngle	= cen + (right-up);
			Vector3 topLeftAngle 		= cen + (up-right);

			Handles.color = primaryColor;

			// y = 1x+1
			Handles.DrawLine(bottomLeftAngle, topRightAngle);

			// y = -1x-1
			Handles.DrawLine(topLeftAngle, bottomRightAngle);	
		}
		#endif

		Handles.color = previousColor;
	}
#endregion

#region ENUM UTILITY
	
	public SnapUnit SnapUnitWithString(string str)
	{
		foreach(SnapUnit su in SnapUnit.GetValues(typeof(SnapUnit)))
		{
			if(su.ToString() == str)
				return su;
		}
		return (SnapUnit)0;
	}
	
	public Axis AxisWithVector(Vector3 val)
	{
		Vector3 v = new Vector3(Mathf.Abs(val.x), Mathf.Abs(val.y), Mathf.Abs(val.z));

		if(v.x > v.y && v.x > v.z) {
			if(val.x > 0)
				return Axis.X;
			else
				return Axis.NegX;
		}
		else
		if(v.y > v.x && v.y > v.z) {
			if(val.y > 0)
				return Axis.Y;
			else
				return Axis.NegY;			
		}
		else {
			if(val.z > 0)
				return Axis.Z;
			else
				return Axis.NegZ;
		}
	}

	public Vector3 VectorWithAxis(Axis axis)
	{
		switch(axis)
		{
			case Axis.X:
				return Vector3.right;
			case Axis.Y:
				return Vector3.up;
			case Axis.Z:
				return Vector3.forward;
			case Axis.NegX:
				return -Vector3.right;
			case Axis.NegY:
				return -Vector3.up;
			case Axis.NegZ:
				return -Vector3.forward;

			default:
				return Vector3.forward;
		}
	}

	public bool IsRounded(Vector3 v)
	{
		return ( Mathf.Approximately(v.x, 1f) || Mathf.Approximately(v.y, 1f) || Mathf.Approximately(v.z, 1f) ) || v == Vector3.zero;
	}

	public Vector3 RoundAxis(Vector3 v)
	{
		return VectorWithAxis(AxisWithVector(v));
	}
#endregion


#region MOVING TRANSFORMS

	static bool FuzzyEquals(Vector3 lhs, Vector3 rhs)
	{
		return Mathf.Abs(lhs.x - rhs.x) < .001f && Mathf.Abs(lhs.y - rhs.y) < .001f && Mathf.Abs(lhs.z - rhs.z) < .001f;
	}

	public void OffsetTransforms(Transform[] trsfrms, Transform ignore, Vector3 offset)
	{
		foreach(Transform t in trsfrms)
		{
			if(t != ignore)
				t.position += offset;
		}
	}
#endregion

#region SETTINGS

	/**
	 *	ALL SETTERS ARE RESPONSIBLE FOR UPDATING PROBUILDER
	 */
	public void SetSnapEnabled(bool enable)
	{
		EditorPrefs.SetBool(pg_Constant.SnapEnabled, enable);

		if(Selection.activeTransform)
		{
			lastTransform = Selection.activeTransform;
			lastPosition = Selection.activeTransform.position;
		}

		snapEnabled = enable;
		gridRepaint = true;
		SceneView.RepaintAll();
		SetSharedSnapValues(snapEnabled, snapValue);
	}

	public void SetSnapValue(SnapUnit su, float val, int multiplier)
	{
		int clamp_multiplier = (int)(Mathf.Min(Mathf.Max(25, multiplier), 102400));

		float value_multiplier = clamp_multiplier / 100f;

		/**
		 * multiplier is a value modifies the snap val.  100 = no change,
		 * 50 is half val, 200 is double val, etc.
		 */

		#if PRO
		snapValue = SnapUnitValue(su) * val * value_multiplier;
		SceneView.RepaintAll();
		SetSharedSnapValues(snapEnabled, snapValue);
		
		EditorPrefs.SetInt(pg_Constant.GridUnit, (int)su);
		EditorPrefs.SetFloat(pg_Constant.SnapValue, val);
		EditorPrefs.SetInt(pg_Constant.SnapMultiplier, clamp_multiplier);
		
		// Bugger.SetKey("Grid Unit", (int)su);
		// Bugger.SetKey("Snap Value", val);
		// Bugger.SetKey("Snap Multiplier", clamp_multiplier);

		// update gui (only necessary when calling with editorpref values)
		t_snapValue = val * value_multiplier;
		snapUnit = su;

		switch(su)
		{
			case SnapUnit.Inch:
				PRIMARY_COLOR_INCREMENT = 12;	// blasted imperial units
				break;

			case SnapUnit.Foot:
				PRIMARY_COLOR_INCREMENT = 3;
				break;

			default:
				PRIMARY_COLOR_INCREMENT = 10;
				break;
		}

		gridRepaint = true;

		#else
		Debug.LogWarning("Ye ought not be seein' this ye scurvy pirate.");
		#endif
	} 

	public void SetRenderPlane(Axis axis)
	{
		offset = 0f;
		fullGrid = false;
		renderPlane = axis;
		EditorPrefs.SetBool(pg_Constant.PerspGrid, fullGrid);
		EditorPrefs.SetInt(pg_Constant.GridAxis, (int)renderPlane);
		gridRepaint = true;
		SceneView.RepaintAll();
	}

	public void SetGridEnabled(bool enable)
	{
		drawGrid = enable;
		if(!drawGrid)
			pg_GridRenderer.Destroy();

		EditorPrefs.SetBool("showgrid", enable);

		gridRepaint = true;
		SceneView.RepaintAll();
	}

	public void SetDrawAngles(bool enable)
	{
		drawAngles = enable;
		gridRepaint = true;
		SceneView.RepaintAll();
	}
	
	private void SnapToGrid(Transform[] transforms)
	{
		Undo.RecordObjects(transforms as Object[], "Snap to Grid");

		foreach(Transform t in transforms)
			t.position = pg_Util.SnapValue(t.position, snapValue);
		
		gridRepaint = true;
		SharedProperties.PushToGrid(snapValue);
	}

#endregion

#region GLOBAL SETTING
	
	public void SetSharedSnapValues(bool enable, float snapVal)
	{
		SharedProperties.snapEnabled = enable;
		SharedProperties.snapValue = snapVal;
	}
#endregion
}