Shader "Custom/HeatMap2D"
{
    Properties
    {
        _HeatMapTex("Texture",2D) = "white"{}
        _Alpha("Alpha",range(0,1)) = 0.8
    }
        SubShader
        {
            Tags {"RenderType" = "Overlay" "Queue" = "Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest[unity_GUIZTestMode]
            ZWrite On
            LOD 100
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                sampler2D _HeatMapTex;
                half _Alpha;
                uniform int _FactorCount = 0;
                uniform float3 _Factors[100];//���Ƶ�����������Ļ���������ָ�����鳤�ȣ���������Խ��Ч��Խ��
                uniform float3 _FactorsProperties[100];
                struct appdata
                {
                    float4 vertex : POSITION;
                };
                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float3 worldVertex : TEXCOORD1;
                };
                //hsv��ɫתrgb��ɫ
                float3 hsv2rgb(float3 c)
                {
                    c = float3(c.x, clamp(c.yz, 0.0, 1.0));
                    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
                }
                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.worldVertex = mul(unity_ObjectToWorld, v.vertex).xyz;
                    return o;
                }
                fixed4 frag(v2f input) : SV_Target
                {
                    half heat = 0;
                //�����ر����������Ƶ�Ч�ʵ�
                for (int i = 0; i < _FactorCount; i++)
                {
                    float len = length(input.worldVertex.xyz - _Factors[i].xyz);
                    float value;
                    if (len < _FactorsProperties[i].x)//�ڰ뾶֮�ڵ�ͳһΪ��ɫ
                        value = 1;
                    else
                        value = _FactorsProperties[i].y / (len - _FactorsProperties[i].x);//��Χ֮��ݼ�
                    heat += value;
                    heat = clamp(heat, 0, 0.95);
                }
                heat = clamp(heat, 0.3, 1);
                fixed4 col = float4(hsv2rgb(float3(heat, 1, 1)), 1);
                //��ɫ����ʹ����ͼ������ȡ
                //fixed4 col = tex2D(_HeatMapTex, fixed2(heat, 0.5));
                col.a = _Alpha;
                return col;
            }
            ENDCG
        }
        }
            FallBack "Diffuse"
}
