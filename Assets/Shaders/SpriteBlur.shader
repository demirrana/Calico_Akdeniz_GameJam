Shader "Custom/SpriteBlur"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Softness ("Softness", Range(0, 0.02)) = 0.005
        _MixAmount ("Soft Mix", Range(0, 1)) = 0.5
        _Brightness ("Brightness", Range(0.5, 1.5)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "PreviewType"="Plane" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Softness;
            float _MixAmount;
            float _Brightness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Net sample (orijinal görüntü)
                fixed4 sharp = tex2D(_MainTex, i.uv);

                // Hafif blur sample (4 yön, az kayma)
                float s = _Softness;
                fixed4 soft = fixed4(0, 0, 0, 0);
                soft += tex2D(_MainTex, i.uv + float2( s,  0));
                soft += tex2D(_MainTex, i.uv + float2(-s,  0));
                soft += tex2D(_MainTex, i.uv + float2( 0,  s));
                soft += tex2D(_MainTex, i.uv + float2( 0, -s));
                soft *= 0.25;

                // Net ile soft'u karıştır → kenarlar yumuşar, görsel netliğini kaybeder
                fixed4 c = lerp(sharp, soft, _MixAmount);
                c.rgb *= _Brightness;

                return c * i.color;
            }
            ENDCG
        }
    }
}