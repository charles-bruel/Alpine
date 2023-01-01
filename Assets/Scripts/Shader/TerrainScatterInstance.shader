Shader "Custom/TerrainScatterInstance" {
	Properties{
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _SnowTex ("Snow Level (B channel)", 2D) = "white" {}
		_DetailTex ("Snow Detail (R channel)", 2D) = "white" {}
        _Bounds ("Snow Level Area", Vector) = (-1, -1, 1, 1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_Depth ("Depth", Range(0, 1)) = 0.0
		_Threshold ("Threshold", Range(0, 1)) = 0.0
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model
			#pragma surface surf Standard addshadow fullforwardshadows vertex:vert
			#pragma multi_compile_instancing
			#pragma instancing_options procedural:setup

        	sampler2D _MainTex;
        	sampler2D _SnowTex;
			sampler2D _DetailTex;

			struct Input {
				float2 uv_MainTex;
            	float3 worldNormal;
            	float3 worldPos;
            	float height;
			};

			half _Glossiness;
			half _Metallic;
        	float4 _Bounds;
			half _Threshold;
        	half _Depth;

			struct TreePos {
				float3 pos;
				float rot;
				float scale;
				//Not needed in the shader, but this makes the data easier
				//to deal with CPU side
				int type;
			};

			struct Params {
				float SnowMultiplier;
			};

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED 
			StructuredBuffer<TreePos> dataBuffer;
			StructuredBuffer<Params> paramBuffer;
		#endif

			void setup()
			{
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				TreePos data = dataBuffer[unity_InstanceID];

				float4x4 position = {
					data.scale, 0, 0, data.pos.x,
					0, data.scale, 0, data.pos.y,
					0, 0, data.scale, data.pos.z,
					0, 0, 0, 1
				};

				unity_ObjectToWorld = position;

				unity_WorldToObject = unity_ObjectToWorld;
				unity_WorldToObject._14_24_34 *= -1;
				unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
			#endif
			}

			void vert(inout appdata_full v, out Input o) {
           		UNITY_INITIALIZE_OUTPUT(Input,o);
				v.vertex = v.vertex.xzyw;
				v.vertex.z *= -1;
				v.normal = v.normal.xzy;
				v.normal.z *= -1;
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				o.height = v.vertex.y * paramBuffer[0].SnowMultiplier;

				TreePos data = dataBuffer[unity_InstanceID];

				float s;
				float c;

				sincos(data.rot, s, c);

				v.vertex.xz = float2(
					v.vertex.x * c - v.vertex.z * s,
					v.vertex.x * s + v.vertex.z * c
				);

				v.normal.xz = float2(
					v.normal.x * c - v.normal.z * s,
					v.normal.x * s + v.normal.z * c
				);
			#else
				o.height = v.vertex.y;
			#endif
        	}

			void surf(Input IN, inout SurfaceOutputStandard o) {
            	//Look up world space texture coordinates
            	half uvx = (IN.worldPos.x - _Bounds.x) / (_Bounds.z - _Bounds.x);
            	half uvy = (IN.worldPos.z - _Bounds.y) / (_Bounds.w - _Bounds.y);
            	half2 snow_tex_uv = half2(uvx, uvy);
	
            	//This is the snow multiplier
            	half snowMultiplier = 1 - tex2D (_SnowTex, snow_tex_uv).b;
	
				snowMultiplier += tex2D(_DetailTex, IN.worldPos.xy * 0.15).r * 0.1 - 0.05;

            	//If the snow multiplier is above the threshold then we leave it as is
            	half thresholdPassed = sign(max(snowMultiplier - _Threshold, 0));
	
            	//Dramatically decrease the impact of elevation aside from threshold
            	snowMultiplier = snowMultiplier * 0.1 + 0.95;

            	//All of these are in the range [0-1]
            	//If snow is below the threshold, this is 0
            	half snowThreshold = snowMultiplier * thresholdPassed * _Depth;
            	snowThreshold = 1 - snowThreshold;
	
            	//Base color of the texture
            	fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            	o.Albedo = c.rgb;
            	o.Alpha = c.a;
	
            	//A lower value of this means shallower and more snowy
            	float snowVal = dot(float3(0, 1, 0), IN.worldNormal);
            	snowVal += tex2D(_DetailTex, IN.worldPos.xy * 0.1).r * 0.2 - 0.1;
            	snowVal *= IN.height;
	
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
