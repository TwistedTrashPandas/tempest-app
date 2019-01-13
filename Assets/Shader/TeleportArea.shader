Shader "Unlit/TeleportArea"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MinY ("MinY", Float) = -1
        _MaxY ("MaxY", Float) = 1
        _Exponent ("Exponent", Float) = 1
        _WaveColor("WaveColor", Color) = (1, 1, 1, 1)
        _WaveSpeed ("WaveSpeed", Float) = 1
        _WaveCount ("WaveCount", Float) = 1
        _WaveSize ("WaveSize", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 position : POSITION;
            };

            struct v2f
            {
                float4 screenPosition : SV_POSITION;
                float4 position : POSITION1;
            };

            float4 _Color;
            float _MinY;
            float _MaxY;
            float _Exponent;
            float4 _WaveColor;
            float _WaveSpeed;
            float _WaveCount;
            float _WaveSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.screenPosition = UnityObjectToClipPos(v.position);
                o.position = v.position;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float height = _MaxY - _MinY;
                float alpha = saturate((_MaxY - i.position.y) / height);

                float wave = pow(abs(sin(_WaveCount * i.position.y + _WaveSpeed * _Time.y)), _WaveSize);

                return pow(alpha, _Exponent) * (_Color + wave * _WaveColor);
            }
            ENDCG
        }
    }
}
