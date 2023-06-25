Shader "Unlit/BeesShader"
{
    Properties
    {

    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            // Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
            #pragma exclude_renderers gles
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            struct BeeData
            {
                float team;
                float3 position;
            };

            uniform StructuredBuffer<BeeData> matrixBuffer : register (t1);
            float4 colors[5] = { float4(1, 1, 1, 1), float4(1, 1, 1, 1), float4(1, 1, 1, 1), float4(1, 1, 1, 1), float4(1, 1, 1, 1) };

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR0;
            };

            v2f vert (appdata v, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);

                BeeData beeData = matrixBuffer[svInstanceID];

                float4x4 transformMatrix = 
                {
                    float4(1.0f, 0.0f, 0.0f, beeData.position.x),
                    float4(0.0f, 1.0f, 0.0f, beeData.position.y),
                    float4(0.0f, 0.0f, 1.0f, beeData.position.z),
                    float4(0.0f, 0.0f, 0.0f, 1.0f),
                };

                v2f o;
                o.vertex = UnityWorldToClipPos(mul(transformMatrix, v.vertex));
                o.color = colors[beeData.team];
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = i.color;
                return col;
            }
            ENDCG
        }
    }
}
