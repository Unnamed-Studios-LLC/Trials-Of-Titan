Shader "Custom/UISprite"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_GlowColor("Glow Color", Color) = (0,0,0,1)
		_OutlineThickness("Outline Thickness", Int) = 1

		_AlphaMask("Alpha Texture", 2D) = "white" {}

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Stencil
			{
				Ref[_Stencil]
				Comp[_StencilComp]
				Pass[_StencilOp]
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}

			Cull Off
			Lighting Off
			ZWrite Off
			ZTest[unity_GUIZTestMode]
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask[_ColorMask]

			Pass
			{
				Name "Default"
			CGPROGRAM
				#pragma require geometry
				#pragma vertex vert
				//#pragma geometry geom
				#pragma fragment frag
				#pragma target 2.0

				#include "UnityCG.cginc"
				#include "UnityUI.cginc"

				#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
				#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
					float4 worldPosition : TEXCOORD1;
					//float2 scale : TEXCOORD2;
					//float2 texscale : TEXCOORD3;
					//float2 alphacoord : TEXCOORD4;
					UNITY_VERTEX_OUTPUT_STEREO
				};

				sampler2D _MainTex;
				sampler2D _AlphaMask;
				float4 _MainTex_TexelSize;
				fixed4 _Color;
				fixed4 _GlowColor;
				fixed4 _TextureSampleAdd;
				float4 _ClipRect;
				float4 _MainTex_ST;
				float _OutlineThickness;

				fixed4 OutlineColor(float2 uv, float2 texel)
				{
					bool upA = tex2D(_MainTex, uv + float2(0, texel.y)).a > 0;
					bool downA = tex2D(_MainTex, uv - float2(0, texel.y)).a > 0;
					bool rightA = tex2D(_MainTex, uv + float2(texel.x, 0)).a > 0;
					bool leftA = tex2D(_MainTex, uv - float2(texel.x, 0)).a > 0;

					bool topLeft = tex2D(_MainTex, uv + float2(-texel.x, texel.y)).a > 0;
					bool topRight = tex2D(_MainTex, uv + float2(texel.x, texel.y)).a > 0;
					bool botLeft = tex2D(_MainTex, uv + float2(-texel.x, -texel.y)).a > 0;
					bool botRight = tex2D(_MainTex, uv + float2(texel.x, -texel.y)).a > 0;

					bool outlined = upA | downA | rightA | leftA | topLeft | topRight | botLeft | botRight;
					return fixed4(0, 0, 0, outlined ? 1 : 0);
				}

				fixed GlowColor(float2 uv, float2 texel, float size)
				{
					float minDistance = 15;
					float maxSize = size * 4;
					float cur = 0;

					for (float y = -size; y <= size; y++) {
						for (float x = -size; x <= size; x++) {
							cur += tex2D(_MainTex, uv + float2(texel.x * x, texel.y * y)).a;
						}
					}

					float p = min(maxSize, cur);
					float f = cur / (maxSize * 6);
					return min(f, 0.4);// min(1 - min(minDistance / 8.0, 1) - (1 - f) * 0.7, 0.5);
				}

				v2f vert(appdata_t v)
				{
					v2f OUT;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
					OUT.worldPosition = v.vertex;
					OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
					//OUT.scale = float2(0, 0);
					//OUT.texscale = float2(0, 0);
					//OUT.alphacoord = float2(0, 0);

					OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

					OUT.color = v.color * _Color;
					return OUT;
				}

				float2 GetAlphaCoord(float2 v, float2 o1, float2 o2) {
					float2 r = float2(0, 0);
					if (v.x - 0.0001f > o1.x || v.x - 0.0001f > o2.x) {
						r.x = 1;
					}
					else {
						r.x = 0;
					}

					if (v.y - 0.0001f > o1.y || v.y - 0.0001f > o2.y) {
						r.y = 1;
					}
					else {
						r.y = 0;
					}
					return r;
				}

				/*
				[maxvertexcount(3)]
				void geom(triangle v2f input[3], inout TriangleStream<v2f> outStream) {

					v2f v1 = input[0];
					v2f v2 = input[1];
					v2f v3 = input[2];

					float2 scale;
					scale.x = max(abs(v1.vertex.x - v2.vertex.x), abs(v1.vertex.x - v3.vertex.x));
					scale.y = max(abs(v1.vertex.y - v2.vertex.y), abs(v1.vertex.y - v3.vertex.y));

					float2 texscale;
					texscale.x = max(abs(v1.texcoord.x - v2.texcoord.x), abs(v1.texcoord.x - v3.texcoord.x));
					texscale.y = max(abs(v1.texcoord.y - v2.texcoord.y), abs(v1.texcoord.y - v3.texcoord.y));

					v1.scale = scale;
					v2.scale = scale;
					v3.scale = scale;

					v1.texscale = texscale;
					v2.texscale = texscale;
					v3.texscale = texscale;

					v1.alphacoord = GetAlphaCoord(v1.texcoord, v2.texcoord, v3.texcoord);
					v2.alphacoord = GetAlphaCoord(v2.texcoord, v1.texcoord, v3.texcoord);
					v3.alphacoord = GetAlphaCoord(v3.texcoord, v2.texcoord, v1.texcoord);

					outStream.Append(v1);
					outStream.Append(v2);
					outStream.Append(v3);
				}
				*/

				fixed4 frag(v2f IN) : SV_Target
				{
					/*
					float2 screenSize = (IN.scale * _ScreenParams.xy) / 2;
					float2 texSize = _MainTex_TexelSize.zw * IN.texscale;
					float2 scale = screenSize / texSize;
					float2 texel = _MainTex_TexelSize.xy / scale;
					*/
					float2 texel = _MainTex_TexelSize.xy / 4;

					float size = 5;

					half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
					if (color.a == 0) {
						color = OutlineColor(IN.texcoord, texel * _OutlineThickness);
						if (color.a == 0)
						{
							color = _GlowColor;
							//color.a *= GlowColor(IN.texcoord, texel * (scale.x / 5), size);
							color.a *= GlowColor(IN.texcoord, texel, size);
						}
						color.a *= IN.color.a;
					}

					#ifdef UNITY_UI_CLIP_RECT
					color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
					#endif

					#ifdef UNITY_UI_ALPHACLIP
					clip(color.a - 0.001);
					#endif

					//color.a *= tex2D(_AlphaMask, IN.alphacoord).r;

					return color;
				}
			ENDCG
			}
		}
}
