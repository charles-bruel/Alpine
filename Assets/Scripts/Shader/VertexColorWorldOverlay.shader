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
        _Aspect ("Camera Aspect", Float) = 1
        _OverlayRockColor ("Overlay Rock Color", Color) = (0.5, 0.5, 0.5, 1)
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
            fixed4 _OverlayRockColor;
            float4 _Bounds;
            float _OverlayStrength;
            sampler2D _OverlayTex;
            float _Aspect;

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

                fixed tree_channel_sign = max(0, sign(overlay.g - 0.1f));
                fixed tree_strip_sign = tree_channel_sign * max(0, sign((i.overlay_tex_uv.x * _Aspect + i.overlay_tex_uv.y) * 100 % 2 - 1));

                fixed rock_channel_sign = max(0, sign(overlay.b - 0.1f));
                fixed rock_strip_sign = rock_channel_sign * max(0, -sign((i.overlay_tex_uv.x * _Aspect + i.overlay_tex_uv.y) * 100 % 2 - 1));

                // combine
                fixed4 col = c * (1 - tree_strip_sign - rock_strip_sign) 
                                + c * (1 - _OverlayStrength) * tree_strip_sign 
                                + rock_strip_sign * _OverlayRockColor * 5 * _OverlayStrength
                                + c * (1 - 5 * _OverlayStrength) * rock_strip_sign;

                // fixed4 col = fixed4(i.overlay_tex_uv.x, i.overlay_tex_uv.y, 0, 1);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
