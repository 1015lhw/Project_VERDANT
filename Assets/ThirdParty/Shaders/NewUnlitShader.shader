Shader "Custom/Outline" {
    Properties {
        _Color ("Outline Color", Color) = (1,1,1,1)
        _Thickness ("Outline Thickness", float) = 0.02
    }

    SubShader {
        Tags {"Queue"="Geometry+1" "RenderType"="Opaque"}
        LOD 200

        Pass {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma geometry geom

        struct appdata {
            float4 vertex : POSITION;
        };

        struct v2f {
            float4 vertex : SV_POSITION;
        };

        uniform float _Thickness;
        uniform float4 _Color;

        v2f vert (appdata v) {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            return o;
        }

        [maxvertexcount(6)]
        void geom(triangle v2f IN[3], inout TriangleStream<v2f> triStream) {
            v2f v0, v1, v2;
            v0 = IN[0];
            v1 = IN[1];
            v2 = IN[2];

            // calculate face normal
            float3 normal = normalize(cross(v1.vertex.xyz - v0.vertex.xyz, v2.vertex.xyz - v0.vertex.xyz));
            // extrude vertex along the normal
            v0.vertex.xyz += normal * _Thickness;
            v1.vertex.xyz += normal * _Thickness;
            v2.vertex.xyz += normal * _Thickness;

            triStream.Append(v0);
            triStream.Append(v1);
            triStream.Append(v2);
        }

        fixed4 frag (v2f i) : SV_Target {
            return _Color;
        }
        ENDCG
        }    
    }

    FallBack "Diffuse"
}
