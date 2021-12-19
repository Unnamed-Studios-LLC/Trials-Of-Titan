Shader "Custom/Particle"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Outline("Outline", Float) = 1
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
    }
    SubShader
    {
		Cull Off
		Lighting Off
		ZWrite On
		ZTest Off

		Pass 
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			struct appdata {
				float3 vertex : POSITION;
				float4 color : COLOR;
				float3 center : TEXCOORD0;
			};

			struct v2f {
				float4 pos : POSITION;
				float4 color : COLOR;
				float3 center : TEXCOORD0;

			};

			uniform float _Outline;
			uniform float4 _OutlineColor;

			v2f vert(appdata v) {
				v2f o;
				o.center = v.center;
				float3 vert = v.vertex - v.center;
				vert *= _Outline;
				v.vertex = vert + v.center;
				o.pos = UnityObjectToClipPos(v.vertex);

				o.color = _OutlineColor;
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				return i.color;
			}

			ENDCG
		}

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			struct appdata {
				float3 vertex : POSITION;
				float4 color : COLOR;
				float3 center : TEXCOORD0;
			};

			struct v2f {
				float4 pos : POSITION;
				float4 color : COLOR;
			};

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
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
}
