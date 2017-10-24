// Upgrade NOTE: replaced 'SeperateSpecular' with 'SeparateSpecular'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Custom/crosshairshader" {


    Properties
    {
        /*_MainTex ("Base (RGB), Alpha (A)") = "white" {}

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _ColorMask ("Color Mask", Float) = 15*/

        _MainTex ("Albedo Texture", 2D) = "white" {}
        _TintColor("Tint Color", Color) = (1,1,1,1)
        _Transparency("Transparency", Range(0.0,0.5)) = 0.25
        _CutoutThresh("Cutout Threshold", Range(0.0,1.0)) = 0
        _Distance("Distance", Float) = 0
        _Amplitude("Amplitude", Float) = 0
        _Speed ("Speed", Float) = 0
        _Amount("Amount", Range(0.0,1.0)) = 0
    }


    SubShader
    {
        LOD 100

        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        /*Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
        }*/

        Cull Off
        //Lighting On
        ZWrite Off
        ZTest Always
        Offset -1, -1
        //Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha
        /*ColorMask [_ColorMask]*/

        Pass
        {   
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _TintColor;
            float _Transparency;
            float _CutoutThresh;
            float _Distance;
            float _Amplitude;
            float _Speed;
            float _Amount;
            
            v2f vert (appdata v)
            {
                v2f o;
                //v.vertex.x += sin(_Time.y * _Speed + v.vertex.y * _Amplitude) * _Distance * _Amount;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) + _TintColor;
                col.a = _Transparency;
                clip(col.r - _CutoutThresh);
                return col;
            }
            ENDCG
        /*
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
            float4 vertex : POSITION;
            float2 texcoord : TEXCOORD0;
            fixed4 color : COLOR;
            };

            struct v2f
            {
            float4 vertex : SV_POSITION;
            half2 texcoord : TEXCOORD0;
            fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;


            v2f vert (appdata_t v)
            {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
            o.color = v.color;
            return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
            fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;
            clip (col.a - 0.01);
            return col;
            }
            ENDCG*/
        }
    }
}