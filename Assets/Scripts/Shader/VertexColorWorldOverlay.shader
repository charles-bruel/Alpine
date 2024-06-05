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

Shader "Unlit/VertexColorWorldOverlay"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _Bounds ("World Area", Vector) = (-1, -1, 1, 1)
        _OverlayStrength ("Overlay Strength", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                half2 overlay_tex_uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            fixed4 _Color;
            float4 _Bounds;
            float _OverlayStrength;
            sampler2D _OverlayTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = fixed4(v.color) * _Color;

                // float3 worldPos = mul (unity_ObjectToWorld, v.vertex).xyz;

                // half uvx = (worldPos.x - _Bounds.x) / (_Bounds.z - _Bounds.x);
                // half uvy = (worldPos.z - _Bounds.y) / (_Bounds.w - _Bounds.y);

                // o.overlay_tex_uv = half2(uvx, uvy);

                o.overlay_tex_uv = (o.vertex.xy * half2(1, -1) + half2(1, 1)) * 0.5f;
                
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // base color
                fixed4 c = i.color;

                // overlay texture
                fixed4 overlay = tex2D(_OverlayTex, i.overlay_tex_uv);

                // combine
                fixed4 col = c  * (1 - _OverlayStrength) + overlay * _OverlayStrength;

                // fixed4 col = fixed4(i.overlay_tex_uv.x, i.overlay_tex_uv.y, 0, 1);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
