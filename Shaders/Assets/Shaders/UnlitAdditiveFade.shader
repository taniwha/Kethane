Shader "Kethane/Unlit/AdditiveFade"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Blend One One
		ZWrite Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float4 color : COLOR;
                float3 uva : TEXCOORD0;
            };

            struct v2f
            {
				float4 color : COLOR;
                float3 uva : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uva.xy = TRANSFORM_TEX(v.uva.xy, _MainTex);
				o.uva.z = v.uva.z;
				o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				float fade = (1 - i.uva.z) * i.color.a;
                fixed4 col = tex2D(_MainTex, i.uva.xy) * i.color * fade * sqrt(sqrt(fade));
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
