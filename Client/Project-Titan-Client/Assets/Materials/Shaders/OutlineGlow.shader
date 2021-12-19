Shader "Custom/OutlineGlow"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_OutlineWidth("Outline Width", Float) = 1
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_GlowWidth("Glow Width", Float) = 1
		_GlowColor("Glow Color", Color) = (0,0,0,1)
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

		Cull Off
		Lighting Off
		ZWrite On
		ZTest On
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			uniform sampler2D _MainTex;
			uniform float _OutlineWidth;
			uniform float4 _OutlineColor;
			uniform float _GlowWidth;
			uniform float4 _GlowColor;

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = v.texcoord;
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				float4 color = tex2D(_MainTex, i.texcoord);

				float aLeft = tex2D(_MainTex, i.texcoord + float2(_OutlineWidth, 0)).a;
				float aRight = tex2D(_MainTex, i.texcoord - float2(_OutlineWidth, 0)).a;
				float aTop = tex2D(_MainTex, i.texcoord + float2(0, _OutlineWidth)).a;
				float aBot = tex2D(_MainTex, i.texcoord - float2(0, _OutlineWidth)).a;

				float aBotLeft = tex2D(_MainTex, i.texcoord - float2(_OutlineWidth, _OutlineWidth)).a;
				float aBotRight = tex2D(_MainTex, i.texcoord + float2(_OutlineWidth, -_OutlineWidth)).a;
				float aTopLeft = tex2D(_MainTex, i.texcoord + float2(-_OutlineWidth, _OutlineWidth)).a;
				float aTopRight = tex2D(_MainTex, i.texcoord + float2(_OutlineWidth, _OutlineWidth)).a;

				float outlineResult = (aLeft + aRight + aTop + aBot + aBotLeft + aBotRight + aTopRight + aTopLeft) > 0 ? 1 : 0;
				outlineResult *= (1 - color.a);
				float4 outline = outlineResult * _OutlineColor;

				return color * color.a + _OutlineColor * outlineResult;
			}

			ENDCG
		}
	}
}
