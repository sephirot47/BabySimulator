using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProGrids2;
using System.Linq;

public class pg_GridRenderer
{
	static readonly HideFlags PG_HIDE_FLAGS = (HideFlags) (1 | 2);
	const string PREVIEW_OBJECT_NAME = "ProGridsGridObject";
	const string MATERIAL_OBJECT_NAME = "ProGridsMaterialObject";
	const string MESH_OBJECT_NAME = "ProGridsMeshObject";
	const string GRID_SHADER = "Hidden/ProGrids/pg_GridShader";
	const int MAX_LINES = 128;

	static GameObject gridObject;
	static Mesh gridMesh;
	static Material gridMaterial;

	/**
	 * Destroy any existing render objects, then initialize new ones.
	 */
	public static void Init()
	{
		Destroy();

		gridObject = EditorUtility.CreateGameObjectWithHideFlags(PREVIEW_OBJECT_NAME, PG_HIDE_FLAGS, new System.Type[2]{typeof(MeshFilter), typeof(MeshRenderer)});
		gridObject.GetComponent<MeshRenderer>().enabled = false;
		
		// Force the mesh to only render in SceneView
		gridObject.AddComponent<pg_SceneMeshRender>();

		gridMesh = new Mesh();
		gridMesh.name = MESH_OBJECT_NAME;
		gridMesh.hideFlags = PG_HIDE_FLAGS;
		gridObject.GetComponent<MeshFilter>().sharedMesh = gridMesh;

		gridMaterial = new Material(Shader.Find(GRID_SHADER));
		gridMaterial.name = MATERIAL_OBJECT_NAME;
		gridMaterial.hideFlags = PG_HIDE_FLAGS;
		gridObject.GetComponent<MeshRenderer>().sharedMaterial = gridMaterial;
	}

	public static void Destroy()
	{
		DestoryObjectsWithName(MESH_OBJECT_NAME, typeof(Mesh));
		DestoryObjectsWithName(MATERIAL_OBJECT_NAME, typeof(Material));
		DestoryObjectsWithName(PREVIEW_OBJECT_NAME, typeof(GameObject));
	}

	static void DestoryObjectsWithName(string Name, System.Type type)
	{
		IEnumerable go = Resources.FindObjectsOfTypeAll(type).Where(x => x.name.Contains(Name));
		
		foreach(Object t in go)
		{
			GameObject.DestroyImmediate(t);
		}
	}

	private static int tan_iter, bit_iter, max = MAX_LINES, div = 1;
	
	/**
	 * Returns the distance this grid is drawing
	 */
	public static float DrawPlane(Camera cam, Vector3 pivot, Vector3 tangent, Vector3 bitangent, float snapValue, Color color, float alphaBump)
	{
		if(!gridMesh || !gridMaterial || !gridObject)	
			Init();

		pivot = pg_Util.SnapValue(pivot, snapValue);

		Vector3 p = cam.WorldToViewportPoint(pivot);
		bool inFrustum = (p.x >= 0f && p.x <= 1f) &&
						 (p.y >= 0f && p.y <= 1f) &&
						  p.z >= 0f;

		float[] distances = GetDistanceToFrustumPlanes(cam, pivot, tangent, bitangent, 24f);

		if(inFrustum)
		{
			tan_iter = (int)(Mathf.Ceil( (Mathf.Abs(distances[0]) + Mathf.Abs(distances[2]))/snapValue ));
			bit_iter = (int)(Mathf.Ceil( (Mathf.Abs(distances[1]) + Mathf.Abs(distances[3]))/snapValue ));

			max = Mathf.Max( tan_iter, bit_iter );

			// if the max is around 3x greater than min, we're probably skewing the camera at near-plane
			// angle, so use the min instead.
			if(max > Mathf.Min(tan_iter, bit_iter) * 2)
				max = (int) Mathf.Min(tan_iter, bit_iter) * 2;

			div = 1;

			float dot = Vector3.Dot( cam.transform.position-pivot, Vector3.Cross(tangent, bitangent) );

			if(max > MAX_LINES)
			{
				if(Vector3.Distance(cam.transform.position, pivot) > 50f * snapValue && Mathf.Abs(dot) > .8f)
				{
					while(max/div > MAX_LINES)
						div += div;
				}
				else
				{
					max = MAX_LINES;
				}
			}
		}

		// origin, tan, bitan, increment, iterations, divOffset, color, primary alpha bump
		DrawFullGrid(cam, pivot, tangent, bitangent, snapValue*div, max/div, div, color, alphaBump);

		return ((snapValue*div)*(max/div));
	}

	public static void DrawGridPerspective(Camera cam, Vector3 pivot, float snapValue, Color[] colors, float alphaBump)
	{
		if(!gridMesh || !gridMaterial || !gridObject)	
			Init();
			
		Vector3 camDir = (pivot - cam.transform.position).normalized;
		pivot = pg_Util.SnapValue(pivot, snapValue);
		
		// Used to flip the grid to match whatever direction the cam is currently
		// coming at the pivot from
		Vector3 right = camDir.x < 0f ? Vector3.right : Vector3.right * -1f;
		Vector3 up = camDir.y < 0f ? Vector3.up : Vector3.up * -1f;
		Vector3 forward = camDir.z < 0f ? Vector3.forward : Vector3.forward * -1f;

		// Get intersecting point for each axis, if it exists
		Ray ray_x = new Ray(pivot, right);
		Ray ray_y = new Ray(pivot, up);
		Ray ray_z = new Ray(pivot, forward);

		float x_dist = 10f, y_dist = 10f, z_dist = 10f;
		bool x_intersect = false, y_intersect = false, z_intersect = false;

		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
		foreach(Plane p in planes)
		{
			float dist;
			float t = 0;

			if(p.Raycast(ray_x, out dist))
			{
				t = Vector3.Distance(pivot, ray_x.GetPoint(dist));
				if(t < x_dist || !x_intersect)
				{
					x_intersect = true;
					x_dist = t;
				}
			}

			if(p.Raycast(ray_y, out dist))
			{
				t = Vector3.Distance(pivot, ray_y.GetPoint(dist));
				if(t < y_dist || !y_intersect)
				{
					y_intersect = true;
					y_dist = t;
				}
			}

			if(p.Raycast(ray_z, out dist))
			{
				t = Vector3.Distance(pivot, ray_z.GetPoint(dist));
				if(t < z_dist || !z_intersect)
				{
					z_intersect = true;
					z_dist = t;
				}
			}
		}

		int x_iter = (int)(Mathf.Ceil(Mathf.Max(x_dist, y_dist))/snapValue);
		int y_iter = (int)(Mathf.Ceil(Mathf.Max(x_dist, z_dist))/snapValue);
		int z_iter = (int)(Mathf.Ceil(Mathf.Max(z_dist, y_dist))/snapValue);
		
		int max = Mathf.Max( Mathf.Max(x_iter, y_iter), z_iter );
		int div = 1;
		while(max/div> MAX_LINES)
		{
			div++;
		}

		Vector3[] vertices_t = null;
		Color[] colors_t = null;
		int[] indices_t = null;

		List<Vector3> vertices_m = new List<Vector3>();
		List<Color> colors_m = new List<Color>();
		List<int> indices_m = new List<int>();

		// X plane
		DrawHalfGrid(cam, pivot, up, right, snapValue*div, x_iter/div, colors[0], alphaBump, out vertices_t, out colors_t, out indices_t, 0);
		vertices_m.AddRange(vertices_t);
		colors_m.AddRange(colors_t);
		indices_m.AddRange(indices_t);

		// Y plane
		DrawHalfGrid(cam, pivot, right, forward, snapValue*div, y_iter/div, colors[1], alphaBump, out vertices_t, out colors_t, out indices_t, vertices_m.Count);
		vertices_m.AddRange(vertices_t);
		colors_m.AddRange(colors_t);
		indices_m.AddRange(indices_t);

		// Z plane
		DrawHalfGrid(cam, pivot, forward, up, snapValue*div, z_iter/div, colors[2], alphaBump, out vertices_t, out colors_t, out indices_t, vertices_m.Count);
		vertices_m.AddRange(vertices_t);
		colors_m.AddRange(colors_t);
		indices_m.AddRange(indices_t);

		gridMesh.Clear();
		gridMesh.vertices = vertices_m.ToArray();
		gridMesh.subMeshCount = 1;
		gridMesh.uv = new Vector2[vertices_m.Count];
		gridMesh.colors = colors_m.ToArray();
		gridMesh.SetIndices(indices_m.ToArray(), MeshTopology.Lines, 0);

	}

	private static void DrawHalfGrid(Camera cam, Vector3 pivot, Vector3 tan, Vector3 bitan, float increment, int iterations, Color secondary, float alphaBump,
		out Vector3[] vertices,
		out Color[] colors,
		out int[] indices, int offset)
	{
		Color primary = secondary;
		primary.a += alphaBump;

		float len = increment * iterations;

		int highlightOffsetTan 		= (int)((pg_Util.ValueFromMask(pivot, tan) % (increment*10f)) / increment);
		int highlightOffsetBitan	= (int)((pg_Util.ValueFromMask(pivot, bitan) % (increment*10f)) / increment);

		iterations++;

		// this could only use 3 verts per line
		float fade = .75f;
		float fadeDist = len * fade;

		vertices = new Vector3[iterations*6-3];
		indices = new int[iterations*8-4];
		colors = new Color[iterations*6-3];

		vertices[0] = pivot;
		vertices[1] = (pivot + bitan*fadeDist);
		vertices[2] = (pivot + bitan*len);

		indices[0] = 0 + offset;
		indices[1] = 1 + offset;
		indices[2] = 1 + offset;
		indices[3] = 2 + offset;
		
		colors[0] = primary;
		colors[1] = primary;
		colors[2] = primary;
		colors[2].a = 0f;

		int n = 4;
		int v = 3;

		for(int i = 1; i < iterations; i++)
		{
			// MeshTopology doesn't exist prior to Unity 4
			vertices[v+0] = pivot + i * tan * increment;
			vertices[v+1] = (pivot + bitan*fadeDist) + i * tan * increment;
			vertices[v+2] = (pivot + bitan*len) + i * tan * increment;

			vertices[v+3] = pivot + i * bitan * increment;
			vertices[v+4] = (pivot + tan*fadeDist) + i * bitan * increment;
			vertices[v+5] = (pivot + tan*len) + i * bitan * increment;

			indices[n+0] = v + 0 + offset;
			indices[n+1] = v + 1 + offset;
			indices[n+2] = v + 1 + offset;
			indices[n+3] = v + 2 + offset;
			indices[n+4] = v + 3 + offset;
			indices[n+5] = v + 4 + offset;
			indices[n+6] = v + 4 + offset;
			indices[n+7] = v + 5 + offset;

			float alpha = (i/(float)iterations);
			alpha = alpha < fade ? 1f : 1f - ( (alpha-fade)/(1-fade) );
			
			Color col = (i+highlightOffsetTan) % 10 == 0 ? primary : secondary;
			col.a *= alpha;

			colors[v+0] = col;
			colors[v+1] = col;
			colors[v+2] = col;
			colors[v+2].a = 0f;

			col = (i+highlightOffsetBitan) % 10 == 0 ? primary : secondary;
			col.a *= alpha;

			colors[v+3] = col;
			colors[v+4] = col;
			colors[v+5] = col;
			colors[v+5].a = 0f;

			n += 8;
			v += 6;
		}
	}

	/**
	 * Draws a plane grid using pivot point, the right and forward directions, and how far each direction should extend
	 */
	private static void DrawFullGrid(Camera cam, Vector3 pivot, Vector3 tan, Vector3 bitan, float increment, int iterations, int div, Color secondary, float alphaBump)
	{
		Color primary = secondary;
		primary.a += alphaBump;

		float len = iterations * increment;

		iterations++;

		Vector3 start = pivot - tan*(len/2f) - bitan*(len/2f);
		start = pg_Util.SnapValue(start, bitan+tan, increment);

		float inc = increment;
		int highlightOffsetTan = (int)((pg_Util.ValueFromMask(start, tan) % (inc*10f)) / inc);
		int highlightOffsetBitan = (int)((pg_Util.ValueFromMask(start, bitan) % (inc*10f)) / inc);


		float fade = .8f;
		Vector3[] lines = new Vector3[iterations*8];
		int[] indices = new int[iterations*12];
		Color[] colors = new Color[iterations*8];
		int v = 0, t = 0;

		for(int i = 0; i < iterations; i++)
		{
			Vector3 a = start + tan*i*increment;
			Vector3 b = start + bitan*i*increment;
			
			lines[v+0] = a;
			lines[v+1] = a + (bitan*(1f-fade)*len);
			lines[v+2] = a + (bitan*fade*len);
			lines[v+3] = a + bitan*len;
			
			lines[v+4] = b;
			lines[v+5] = b + (tan*(1f-fade)*len);
			lines[v+6] = b + (tan*fade*len);
			lines[v+7] = b + tan*len;
			
			indices[t+0] = v;
			indices[t+1] = v+1;
			indices[t+2] = v+1;
			indices[t+3] = v+2;
			indices[t+4] = v+2;
			indices[t+5] = v+3;

			indices[t+6] = v+4;
			indices[t+7] = v+5;
			indices[t+8] = v+5;
			indices[t+9] = v+6;
			indices[t+10] = v+6;
			indices[t+11] = v+7;

			int h = iterations/2;
			float alpha = Mathf.Abs(h-i) / (float)h;
			alpha = alpha < fade ? 1f : 1f - ( (alpha-fade)/(1-fade) );
			
			Color col = (i+highlightOffsetTan) % 10 == 0 ? primary : secondary;
			col.a *= alpha;
			
			// tan
			colors[v+0] = col;
			colors[v+0].a = 0f;
			colors[v+1] = col;

			colors[v+2] = col;
			colors[v+3] = col;
			colors[v+3].a = 0f;

			col = (i+highlightOffsetBitan) % 10 == 0 ? primary : secondary;
			col.a *= alpha;

			// bitan
			colors[v+4] = col;
			colors[v+4].a = 0f;
			colors[v+5] = col;

			colors[v+6] = col;
			colors[v+7] = col;
			colors[v+7].a = 0f;

			v += 8;
			t += 12;
		}

		gridMesh.Clear();
		gridMesh.vertices = lines;
		gridMesh.subMeshCount = 1;
		gridMesh.uv = new Vector2[lines.Length];
		gridMesh.colors = colors;
		gridMesh.SetIndices(indices, MeshTopology.Lines, 0);
		// else
		// {
		// 	for(int i = 0; i < iterations; i++)
		// 	{
		// 		Vector3 a = start + tan*i*increment;
		// 		Vector3 b = start + bitan*i*increment;

		// 		Handles.color = (i+highlightOffsetTan) % 10 == 0 ? primary : secondary;
		// 		Handles.DrawLine(a, a + bitan * len );
		// 		Handles.color = (i+highlightOffsetBitan) % 10 == 0 ? primary : secondary;
		// 		Handles.DrawLine(b, b + tan * len );
		// 	}			
		// }
	}

	/**
	 *	\brief Returns the distance from pivot to frustum plane in the order of
	 *	float[] { tan, bitan, -tan, -bitan }
	 */
	private static float[] GetDistanceToFrustumPlanes(Camera cam, Vector3 pivot, Vector3 tan, Vector3 bitan, float minDist)
	{
		Ray[] rays = new Ray[4]
		{
			new Ray(pivot, tan),
			new Ray(pivot, bitan),
			new Ray(pivot, -tan),
			new Ray(pivot, -bitan)
		 };
		
		float[] intersects = new float[4] { minDist, minDist, minDist, minDist };
		bool[] intersection_found = new bool[4] { false, false, false, false };

		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
		foreach(Plane p in planes)
		{
			float dist;
			float t = 0;
			
			for(int i = 0; i < 4; i++)
			{
				if(p.Raycast(rays[i], out dist))
				{
					t = Vector3.Distance(pivot, rays[i].GetPoint(dist));

					if(t < intersects[i] || !intersection_found[i])
					{
						intersection_found[i] = true;
						intersects[i] = Mathf.Max(minDist, t);
					}
				}
			}
		}
		return intersects;
	}
}
