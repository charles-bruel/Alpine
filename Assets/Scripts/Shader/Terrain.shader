Shader "Custom/Terrain"
{
    Properties
    {
        _Color ("Grass Color", Color) = (0,1,0,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Depth ("Depth", Range(0, 1)) = 0.0
		_Threshold ("Threshold", Range(0, 1)) = 0.0
        _Bounds ("Snow Level Area", Vector) = (-1, -1, 1, 1)
        _SnowTex ("Snow Level (B channel)", 2D) = "white" {}
        _DetailTex ("Snow Detail (R channel)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _SnowTex;
		sampler2D _DetailTex;

        struct Input
        {
			float2 uv_MainTex;
            float3 worldNormal;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        float4 _Bounds;
        fixed4 _Color;
		half _Threshold;
        half _Depth;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            //Look up world space texture coordinates
            half uvx = (IN.worldPos.x - _Bounds.x) / (_Bounds.z - _Bounds.x);
            half uvy = (IN.worldPos.z - _Bounds.y) / (_Bounds.w - _Bounds.y);
            half2 snow_tex_uv = half2(uvx, uvy);

            //This is the snow multiplier
            half snowMultiplier = 1 - tex2D (_SnowTex, snow_tex_uv).b * 0.85;

			snowMultiplier += tex2D(_DetailTex, IN.worldPos.xz * 0.001).r * 0.3 - 0.15;

            //If the snow multiplier is above the threshold then we leave it as is
            half thresholdPassed = sign(max(snowMultiplier - _Threshold, 0));

            //Dramatically decrease the impact of elevation aside from threshold
            snowMultiplier = snowMultiplier * 0.1 + 0.95;
            //If snow is below the threshold, this is 0
            half snowThreshold = snowMultiplier * thresholdPassed * _Depth;
            snowThreshold = max(1 - snowThreshold, 0);

            //Base color of the texture
            fixed4 c = _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;

            //A lower value of this means shallower and more snowy
            float snowVal = dot(float3(0, 1, 0), IN.worldNormal) * 0.05 + 0.95;
            snowVal -= tex2D(_DetailTex, IN.worldPos.xz * 0.1).r * 0.1;

            snowVal = max(snowVal, 0.01);

            //sgn is 1.0 if it's snowy and 0.0 otherwise
            //Branchless
            float sgn = max(sign(snowVal - snowThreshold), 0); //if(snowVal > snowThreshold) {
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
