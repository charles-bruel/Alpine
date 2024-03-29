//>============================================================================<
//
//    Alpine, Ski Resort Tycoon Game
//    Copyright (C) 2024  Charles Bruel
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//>============================================================================<

Shader "Custom/TerrainScatter"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _SnowTex ("Snow Level (B channel)", 2D) = "white" {}
        _DetailTex ("Snow Detail (R channel)", 2D) = "white" {}
        _Bounds ("Snow Level Area", Vector) = (-1, -1, 1, 1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _SnowTex;
		sampler2D _DetailTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 worldPos;
            float height;
            float snowThreshold;
        };
    #ifdef SHADER_API_D3D11
		StructuredBuffer<float> snowCurve;
	#endif

        half _Glossiness;
        half _Metallic;
        float4 _Bounds;
        half _Threshold;
        half _Depth;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        //WARNING: Input MUST be [0-1]
		float sampleSnowCurve(float input) {
		#ifdef SHADER_API_D3D11            
			float expanded = input * 255;
			int index = int(expanded);
			float t = expanded - float(index);
			return lerp(snowCurve[index], snowCurve[index + 1], t);
		#endif
			return input;
		}

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.height = v.color.g;

            #ifdef SHADER_API_D3D11
			//Look up world space texture coordinates
            half uvx = (v.vertex.x - _Bounds.x) / (_Bounds.z - _Bounds.x);
            half uvy = (v.vertex.z - _Bounds.y) / (_Bounds.w - _Bounds.y);
            half2 snow_tex_uv = half2(uvx, uvy);

			//This is the snow base value
			//80% comes from the texture and 20% from a noise texture
            float snowMultiplier = tex2Dlod(_SnowTex, float4(snow_tex_uv, 0, 0)).b * 0.8 + 0.1;
			snowMultiplier += tex2Dlod(_DetailTex, float4(v.vertex.xz * 0.001, 0, 0)).r * 0.2 - 0.1;

			o.snowThreshold = 1 - sampleSnowCurve(1 - snowMultiplier);
            #endif
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            //Base color of the texture
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Alpha = c.a;

            //A lower value of this means shallower and more snowy
            float snowVal = dot(float3(0, 1, 0), IN.worldNormal);
            snowVal += tex2D(_DetailTex, IN.worldPos.xz * 0.1).r * 0.2 - 0.1;
            snowVal *= IN.height;

            //sgn is 1.0 if it's snowy and 0.0 otherwise
            //Branchless
            float sgn = max(sign(snowVal - IN.snowThreshold), 0); //if(snowVal > snowThreshold) {
            o.Albedo *= (1-sgn);                               //    o.Albedo = 0;
            o.Albedo += float3(sgn, sgn, sgn);                 //    o.Albedo = (1, 1, 1); }

            //Assign other material properties
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
