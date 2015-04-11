Shader "Hidden/ProGrids/pg_GridShader"
{
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		Lighting Off
		ZWrite On
		ZTest LEqual
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			AlphaTest Off

			CGPROGRAM

			#pragma target 2.0
//			#pragma exclude_renderers xbox360 d3d11 d3d11_9x 
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color : COLOR;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				return i.color;
			}
			
			ENDCG	
		}
	} 
	FallBack "Diffuse"
}
