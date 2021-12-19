// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/Tiles"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Background"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite On
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 blendcoord : TEXCOORD1;
				float2 maskcoord : TEXCOORD2;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord  : TEXCOORD0;
				float2 blendcoord  : TEXCOORD1;
				float2 maskcoord  : TEXCOORD2;
			};

			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.blendcoord = IN.blendcoord;
				OUT.maskcoord = IN.maskcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif

				#if UNITY_REVERSED_Z
				OUT.vertex.z -= 0.00005f;
				#else
				OUT.vertex.z += 0.00005f;
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			fixed4 SampleSpriteTexture(float2 uv, float2 blend, float2 blendMask)
			{
				fixed4 color = tex2D(_MainTex, uv);
				fixed4 blendColor = tex2D(_MainTex, blend);
				fixed4 mask = tex2D(_MainTex, blendMask);

				float r = color.r * (1 - mask.a) + blendColor.r * mask.a;
				float g = color.g * (1 - mask.a) + blendColor.g * mask.a;
				float b = color.b * (1 - mask.a) + blendColor.b * mask.a;
				float a = color.a * (1 - mask.a) + blendColor.a * mask.a;

				return fixed4(r, g, b, a);
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture(IN.texcoord, IN.blendcoord, IN.maskcoord) * IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}