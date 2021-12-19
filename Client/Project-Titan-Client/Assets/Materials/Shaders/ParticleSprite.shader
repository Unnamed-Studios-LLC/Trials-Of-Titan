Shader "Custom/ParticleSprite"
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
					float3 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 pos : POSITION;
					float4 color : COLOR;
					float3 texcoord : TEXCOORD0;
				};

				sampler2D _MainTex;

				v2f vert(appdata v) {
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					o.texcoord = v.texcoord;
					return o;
				}

				half4 frag(v2f i) : COLOR
				{
					return tex2D(_MainTex, i.texcoord) * i.color;
				}

				ENDCG
			}
		}
}
