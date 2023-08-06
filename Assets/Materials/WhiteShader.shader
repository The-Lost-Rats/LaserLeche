Shader "Custom/WhiteShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            // I have no idea what I'm doing
            // https://discussions.unity.com/t/shader-to-set-sprite-to-solid-color/213551
            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, IN.texcoord) * IN.color;

#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
                if (_AlphaSplitEnabled)
                    color.a = tex2D(_AlphaTex, IN.texcoord).r;
#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED
                //Add your color here.
                color.rgb = half4(1, 1, 1, 1).rgb;
                color.rgb *= color.a;
                return color;
            }
        ENDCG
        }
    }
}
