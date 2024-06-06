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

Shader "Custom/InstanceOverlay" {
	Properties{
        _Color ("Albedo (RGB)", Color) = (1, 1, 1, 1)
	}
	SubShader {
			Tags { "RenderType" = "Transparent" }
			LOD 200

			Pass {
            Blend One One

			CGPROGRAM
			#pragma vertex vert
            #pragma fragment frag
			#pragma target 4.5
			
            #include "UnityCG.cginc"

        	// #pragma multi_compile_instancing
        	// #pragma instancing_options procedural:setup

			fixed4 _Color;

			float4x4 _Camera;

			struct InputData {
				float2 pos;
				float scale;
			};

			struct v2f
            {
                float4 pos : SV_POSITION;
            };

		#if SHADER_TARGET >= 45
			StructuredBuffer<InputData> dataBuffer;
		#endif

			v2f vert (appdata_full v, uint instanceID : SV_InstanceID) {
				v2f o;
				float4 world_pos = v.vertex;
				world_pos.yz = world_pos.zy;
				world_pos.z *= -1;
				#if SHADER_TARGET >= 45
				world_pos.xyz *= dataBuffer[instanceID].scale * 500;
				world_pos.xz += dataBuffer[instanceID].pos * 1;
				#endif
				o.pos =  world_pos;
				o.pos = mul(_Camera, o.pos);
				o.pos.y *= -1;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target {
				fixed4 col = _Color;
                return col;
            }
			ENDCG
		}
	}
}
