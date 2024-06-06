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

//THIS SHADER IS OUT OF DATE WITH INSTANCE RENDERER

Shader "Custom/Instance" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model
			#pragma surface surf Standard addshadow fullforwardshadows
			#pragma multi_compile_instancing
			#pragma instancing_options procedural:setup

			sampler2D _MainTex;

			struct Input {
				float2 uv_MainTex;
			};

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			StructuredBuffer<float3> positionBuffer;
		#endif

			void rotate2D(inout float2 v, float r)
			{
				float s, c;
				sincos(r, s, c);
				v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
			}

			void setup()
			{
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				float3 temp = positionBuffer[unity_InstanceID];
				float4 data = float4(temp.xy, 0, 1);

				float rotation = data.w * data.w * _Time.y * 0.5f;
				rotate2D(data.xz, rotation);

				unity_ObjectToWorld._11_21_31_41 = float4(data.w, 0, 0, 0);
				unity_ObjectToWorld._12_22_32_42 = float4(0, data.w, 0, 0);
				unity_ObjectToWorld._13_23_33_43 = float4(0, 0, data.w, 0);
				unity_ObjectToWorld._14_24_34_44 = float4(data.xyz, 1);
				unity_WorldToObject = unity_ObjectToWorld;
				unity_WorldToObject._14_24_34 *= -1;
				unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
			#endif
			}

			half _Glossiness;
			half _Metallic;

			void surf(Input IN, inout SurfaceOutputStandard o) {
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
				o.Albedo = c.rgb;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
			}
			ENDCG
		}
			FallBack "Diffuse"
}
