Shader "Custom/IslandEdgeFade"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MapCenter ("Map Center", Vector) = (0, 0, 0, 0)
        _IslandRadius ("Island Radius", Float) = 35
        _EdgeBlend ("Edge Blend", Float) = 6
        _ShapeNoiseScale ("Shape Noise Scale", Float) = 0.05
        _ShapeNoiseStrength ("Shape Noise Strength", Float) = 6
        _SeedX ("Seed X", Float) = 0
        _SeedY ("Seed Y", Float) = 0
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
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
                float2 worldPos : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float2 _MapCenter;
            float _IslandRadius;
            float _EdgeBlend;
            float _ShapeNoiseScale;
            float _ShapeNoiseStrength;
            float _SeedX;
            float _SeedY;
            
            // Función de ruido Perlin simplificada
            float noise(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            float perlin(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = noise(i);
                float b = noise(i + float2(1.0, 0.0));
                float c = noise(i + float2(0.0, 1.0));
                float d = noise(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Calcular distancia desde el centro del mapa
                float2 offset = i.worldPos - _MapCenter;
                float dist = length(offset);
                
                // Agregar ruido a la forma
                float noiseValue = perlin((i.worldPos + float2(_SeedX, _SeedY)) * _ShapeNoiseScale);
                float radiusWithNoise = _IslandRadius + noiseValue * _ShapeNoiseStrength;
                
                // Calcular alpha basado en la distancia
                float edge = dist - radiusWithNoise;
                float alpha = 1.0 - smoothstep(-_EdgeBlend, _EdgeBlend, edge);
                
                col.a *= alpha;
                
                return col;
            }
            ENDCG
        }
    }
}