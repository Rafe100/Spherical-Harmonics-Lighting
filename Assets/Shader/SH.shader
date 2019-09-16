Shader "Custom/SH"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

		SubShader
	{
		Tags{ "Queue" = "Background" }
		Cull Off ZWrite on

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 texcoord : TEXCOORD0;
				float3 normal: NORMAL;
			};

			float4 c0;
			float4 c1;
			float4 c2;
			float4 c3;
			float4 c4;
			float4 c5;
			float4 c6;
			float4 c7;
			float4 c8;

			sampler2D _MainTex;
			
			float PI() {
				return 3.14159265358;
			}

			float Y0(float3 v)
			{
	
				return (1.0 / 2.0) * sqrt(1.0 / PI());
			}

			float Y1(float3 v)
			{
				
				return sqrt(3.0 / (4.0 * PI())) * v.z;
			}

			float Y2(float3 v)
			{
				return sqrt(3.0 / (4.0 * PI())) * v.y;
			}

			float Y3(float3 v)
			{
				return sqrt(3.0 / (4.0 * PI())) * v.x;
			}

			float Y4(float3 v)
			{
				return 1.0 / 2.0 * sqrt(15.0 / PI()) * v.x * v.z;
			}

			float Y5(float3 v)
			{
				return 1.0 / 2.0 * sqrt(15.0 / PI()) * v.z * v.y;
			}

			float Y6(float3 v)
			{
				return 1.0 / 4.0 * sqrt(5.0 / PI()) * (-v.x * v.x - v.z * v.z + 2 * v.y * v.y);
			}

			float Y7(float3 v)
			{
				return 1.0 / 2.0 * sqrt(15.0 / PI()) * v.y * v.x;
			}

			float Y8(float3 v)
			{
				return 1.0 / 4.0 * sqrt(15.0 / PI()) * (v.x * v.x - v.z * v.z);
			}

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.vertex.xyz;
				o.normal = UnityObjectToWorldNormal(v.normal);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float3 v = i.normal.xyz;
				v = normalize(v);
				float4 original = tex2D(_MainTex, i.texcoord);
				float4 approx = c0 * Y0(v) + c1 * Y1(v) + c2 * Y2(v) + c3 * Y3(v) + c4 * Y4(v) + c5 * Y5(v) + c6 * Y6(v) + c7 * Y7(v) + c8 * Y8(v);
				//approx.a = 1.0;
				
				return approx;
			}
			ENDCG
		}
	}

		Fallback Off
}
