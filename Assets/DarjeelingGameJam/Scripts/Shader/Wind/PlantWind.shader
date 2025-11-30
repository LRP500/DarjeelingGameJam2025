Shader "Universal Render Pipeline/2D/PlantWind"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        [MaterialToggle] _ZWrite("ZWrite", Float) = 0

        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

        [Toggle] _EnableWind ("Enable Wind", Float) = 1

        _WindDirection ("Wind Direction (XY)", Vector) = (1, 0, 0, 0)
        _BaseWindStrength ("Base Wind Strength", Float) = 0.02
        _GustStrength ("Gust Extra Strength", Float) = 0.08
        _GustInterval ("Gust Interval (seconds)", Float) = 5.0
        _GustDuration ("Gust Duration (seconds)", Float) = 1.5
        _WindNoiseScale ("Wind Noise Scale", Float) = 1.0
        _BendByHeight ("Bend By Height (0-2)", Float) = 1.0
        _WindTimeScale ("Wind Time Scale", Float) = 1.0
        _RandomSeed ("Random Seed", Float) = 1.0

        _KeyColor     ("Key Color", Color) = (1,1,1,1)
        _KeyTolerance ("Key Tolerance", Range(0,1)) = 0.1
        _KeyFeather   ("Key Feather", Range(0,1))   = 0.1

        // --- Blur DOF léger ---
        _BlurAmount    ("Blur Amount", Range(0,1)) = 0.0
        _BlurMaxRadius ("Blur Max Radius", Float)  = 2.0

        // --- Variation de teinte par objet ---
        _TintVariationAmount ("Tint Variation Amount", Range(0,0.5)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]

        // ============================
        // Pass 1 : Universal2D (lit)
        // ============================
        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"

            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION;
                half4   color       : COLOR;
                float2  uv          : TEXCOORD0;
                half2   lightingUV  : TEXCOORD1;
                #if defined(DEBUG_DISPLAY)
                    float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            UNITY_TEXTURE_STREAMING_DEBUG_VARS_FOR_TEX(_MainTex);
            float4 _MainTex_TexelSize; // pour le blur

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;

                float  _EnableWind;

                float4 _WindDirection;
                float  _BaseWindStrength;
                float  _GustStrength;
                float  _GustInterval;
                float  _GustDuration;
                float  _WindNoiseScale;
                float  _BendByHeight;
                float  _WindTimeScale;
                float  _RandomSeed;

                float4 _KeyColor;
                float  _KeyTolerance;
                float  _KeyFeather;

                float  _BlurAmount;
                float  _BlurMaxRadius;

                float  _TintVariationAmount;
            CBUFFER_END

            // === Shape lights ===
            #if USE_SHAPE_LIGHT_TYPE_0
            SHAPE_LIGHT(0)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_1
            SHAPE_LIGHT(1)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_2
            SHAPE_LIGHT(2)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_3
            SHAPE_LIGHT(3)
            #endif
            // ==============================

            float Hash11(float n)
            {
                return frac(sin(n * 17.0 + _RandomSeed * 12.9898) * 43758.5453123);
            }

            float ComputeGustAmplitude(float t)
            {
                float interval = max(_GustInterval, 0.01);
                float durationNorm = saturate(_GustDuration / interval);

                float cycle = t / interval;
                float gustIndex = floor(cycle);
                float phase = frac(cycle);

                float active = step(phase, durationNorm);
                float s = (durationNorm > 0.0) ? saturate(phase / durationNorm) : 0.0;
                float envelope = sin(s * 3.14159265);

                float randAmp = Hash11(gustIndex);
                float gustStrength = _BaseWindStrength + randAmp * _GustStrength;

                return gustStrength * active * envelope;
            }

            // --- Utils RGB <-> YIQ pour variation de teinte ---
            float3 RGB2YIQ(float3 c)
            {
                float3 yiq;
                yiq.x = dot(c, float3(0.299,     0.587,      0.114));
                yiq.y = dot(c, float3(0.595716, -0.274453,  -0.321263));
                yiq.z = dot(c, float3(0.211456, -0.522591,   0.311135));
                return yiq;
            }

            float3 YIQ2RGB(float3 c)
            {
                float3 rgb;
                rgb.r = c.x + 0.9563 * c.y + 0.6210 * c.z;
                rgb.g = c.x - 0.2721 * c.y - 0.6474 * c.z;
                rgb.b = c.x - 1.1070 * c.y + 1.7046 * c.z;
                return rgb;
            }

            // --- Variation de teinte par objet (amplitude aléatoire 0.._TintVariationAmount) ---
            float3 ApplyInstanceTint(float3 rgb)
            {
                float v = _TintVariationAmount;
                if (v <= 0.0001)
                    return rgb;

                float randHue = Hash11(_RandomSeed * 7.77);   // direction de teinte 0..1
                float randAmp = Hash11(_RandomSeed * 13.37);  // amplitude locale 0..1
                float localAmount = v * randAmp;              // 0.._TintVariationAmount (ex 0..0.5)

                float maxHueShift = 3.14159;                  // ±180° quand localAmount = 1
                float angle = (randHue * 2.0 - 1.0) * maxHueShift * localAmount;

                float3 yiq = RGB2YIQ(rgb);

                float I = yiq.y;
                float Q = yiq.z;

                float cosA = cos(angle);
                float sinA = sin(angle);

                float I2 = I * cosA - Q * sinA;
                float Q2 = I * sinA + Q * cosA;

                yiq.y = I2;
                yiq.z = Q2;

                float3 outRgb = YIQ2RGB(yiq);
                return saturate(outRgb);
            }

            // --- BLUR helper (3x3) ---
            float4 SampleMainWithBlur(float2 uv)
            {
                float blurAmount = _BlurAmount;
                if (blurAmount <= 0.0001)
                {
                    return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                }

                float2 texel  = _MainTex_TexelSize.xy;
                float  radius = blurAmount * _BlurMaxRadius;
                float2 offset = texel * radius;

                float4 col = 0;
                float  weightSum = 0;

                [unroll]
                for (int x = -1; x <= 1; x++)
                {
                    [unroll]
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 uvOff = uv + float2(x, y) * offset;
                        float  w = 1.0;
                        col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvOff) * w;
                        weightSum += w;
                    }
                }

                return col / max(weightSum, 0.0001);
            }

            Varyings CombinedShapeLightVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                SetUpSpriteInstanceProperties();

                float windBlend = saturate(_EnableWind);

                if (windBlend > 0.0001)
                {
                    float height01 = saturate(v.uv.y);
                    float2 windDir = normalize(_WindDirection.xy + float2(1e-5, 0.0));

                    float t = (_Time.y + _RandomSeed * 13.37) * _WindTimeScale;
                    float gustAmp = ComputeGustAmplitude(t);

                    float noise = sin(t * 2.0
                                      + v.positionOS.x * _WindNoiseScale
                                      + v.positionOS.y * (_WindNoiseScale * 0.37)
                                      + _RandomSeed * 5.123);

                    float bend = height01 * _BendByHeight;
                    float sway = gustAmp * noise * bend * windBlend;

                    v.positionOS.xy += windDir * sway;
                }

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = v.uv;
                o.lightingUV = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);

                o.color = v.color * _Color * unity_SpriteColor;
                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                half4 main = i.color * SampleMainWithBlur(i.uv);
                const half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);

                // Variation de teinte par objet
                main.rgb = ApplyInstanceTint(main.rgb);

                // Chroma key
                float3 diff = main.rgb - _KeyColor.rgb;
                float dist = length(diff);
                float tol = _KeyTolerance;
                float feather = max(_KeyFeather, 1e-5);
                float aMask = saturate((dist - tol) / feather);
                main.a *= aMask;

                SurfaceData2D surfaceData;
                InputData2D inputData;

                InitializeSurfaceData(main.rgb, main.a, mask, surfaceData);
                InitializeInputData(i.uv, i.lightingUV, inputData);

                SETUP_DEBUG_TEXTURE_DATA_2D_NO_TS(inputData, i.positionWS, i.positionCS, _MainTex);

                return CombinedShapeLightShared(surfaceData, inputData);
            }
            ENDHLSL
        }

        // ============================
        // Pass 2 : NormalsRendering
        // ============================
        Pass
        {
            Tags { "LightMode" = "NormalsRendering"}

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex NormalsRenderingVertex
            #pragma fragment NormalsRenderingFragment

            #pragma multi_compile_instancing
            #pragma multi_compile _ SKINNED_SPRITE

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float3 normal       : NORMAL;
                float4 tangent      : TANGENT;
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS      : SV_POSITION;
                half4   color           : COLOR;
                float2  uv              : TEXCOORD0;
                half3   normalWS        : TEXCOORD1;
                half3   tangentWS       : TEXCOORD2;
                half3   bitangentWS     : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START( UnityPerMaterial )
                half4 _Color;

                float  _EnableWind;

                float4 _WindDirection;
                float  _BaseWindStrength;
                float  _GustStrength;
                float  _GustInterval;
                float  _GustDuration;
                float  _WindNoiseScale;
                float  _BendByHeight;
                float  _WindTimeScale;
                float  _RandomSeed;

                float4 _KeyColor;
                float  _KeyTolerance;
                float  _KeyFeather;

                float  _BlurAmount;
                float  _BlurMaxRadius;

                float  _TintVariationAmount;
            CBUFFER_END

            float Hash11(float n)
            {
                return frac(sin(n * 17.0 + _RandomSeed * 12.9898) * 43758.5453123);
            }

            float ComputeGustAmplitude(float t)
            {
                float interval = max(_GustInterval, 0.01);
                float durationNorm = saturate(_GustDuration / interval);

                float cycle = t / interval;
                float gustIndex = floor(cycle);
                float phase = frac(cycle);

                float active = step(phase, durationNorm);
                float s = (durationNorm > 0.0) ? saturate(phase / durationNorm) : 0.0;
                float envelope = sin(s * 3.14159265);

                float randAmp = Hash11(gustIndex);
                float gustStrength = _BaseWindStrength + randAmp * _GustStrength;

                return gustStrength * active * envelope;
            }

            float3 RGB2YIQ(float3 c)
            {
                float3 yiq;
                yiq.x = dot(c, float3(0.299,     0.587,      0.114));
                yiq.y = dot(c, float3(0.595716, -0.274453,  -0.321263));
                yiq.z = dot(c, float3(0.211456, -0.522591,   0.311135));
                return yiq;
            }

            float3 YIQ2RGB(float3 c)
            {
                float3 rgb;
                rgb.r = c.x + 0.9563 * c.y + 0.6210 * c.z;
                rgb.g = c.x - 0.2721 * c.y - 0.6474 * c.z;
                rgb.b = c.x - 1.1070 * c.y + 1.7046 * c.z;
                return rgb;
            }

            float3 ApplyInstanceTint(float3 rgb)
            {
                float v = _TintVariationAmount;
                if (v <= 0.0001)
                    return rgb;

                float randHue = Hash11(_RandomSeed * 7.77);
                float randAmp = Hash11(_RandomSeed * 13.37);
                float localAmount = v * randAmp;

                float maxHueShift = 3.14159;
                float angle = (randHue * 2.0 - 1.0) * maxHueShift * localAmount;

                float3 yiq = RGB2YIQ(rgb);

                float I = yiq.y;
                float Q = yiq.z;

                float cosA = cos(angle);
                float sinA = sin(angle);

                float I2 = I * cosA - Q * sinA;
                float Q2 = I * sinA + Q * cosA;

                yiq.y = I2;
                yiq.z = Q2;

                float3 outRgb = YIQ2RGB(yiq);
                return saturate(outRgb);
            }

            Varyings NormalsRenderingVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(attributes);

                SetUpSpriteInstanceProperties();

                float windBlend = saturate(_EnableWind);

                if (windBlend > 0.0001)
                {
                    float height01 = saturate(attributes.uv.y);
                    float2 windDir = normalize(_WindDirection.xy + float2(1e-5, 0.0));

                    float t = (_Time.y + _RandomSeed * 13.37) * _WindTimeScale;
                    float gustAmp = ComputeGustAmplitude(t);

                    float noise = sin(t * 2.0
                                      + attributes.positionOS.x * _WindNoiseScale
                                      + attributes.positionOS.y * (_WindNoiseScale * 0.37)
                                      + _RandomSeed * 5.123);

                    float bend = height01 * _BendByHeight;
                    float sway = gustAmp * noise * bend * windBlend;

                    attributes.positionOS.xy += windDir * sway;
                }

                attributes.positionOS = UnityFlipSprite(attributes.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                o.uv = attributes.uv;
                o.color = attributes.color * _Color * unity_SpriteColor;
                o.normalWS = TransformObjectToWorldDir(attributes.normal);
                o.tangentWS = TransformObjectToWorldDir(attributes.tangent.xyz);
                o.bitangentWS = cross(o.normalWS, o.tangentWS) * attributes.tangent.w;
                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

            half4 NormalsRenderingFragment(Varyings i) : SV_Target
            {
                half4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv));

                // Variation de teinte aussi dans ce pass
                mainTex.rgb = ApplyInstanceTint(mainTex.rgb);

                // Chroma key sur la map de base
                float3 diff = mainTex.rgb - _KeyColor.rgb;
                float dist = length(diff);
                float tol = _KeyTolerance;
                float feather = max(_KeyFeather, 1e-5);
                float aMask = saturate((dist - tol) / feather);
                mainTex.a *= aMask;

                return NormalsRenderingShared(mainTex, normalTS, i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
            }
            ENDHLSL
        }

        // ============================
        // Pass 3 : UniversalForward (fallback / debug)
        // ============================
        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue"="Transparent" "RenderType"="Transparent"}

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            #if defined(DEBUG_DISPLAY)
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging2D.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/InputData2D.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/SurfaceData2D.hlsl"
            #endif

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS      : SV_POSITION;
                float4  color           : COLOR;
                float2  uv              : TEXCOORD0;
                #if defined(DEBUG_DISPLAY)
                    float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            UNITY_TEXTURE_STREAMING_DEBUG_VARS_FOR_TEX(_MainTex);
            float4 _MainTex_TexelSize; // pour le blur

            CBUFFER_START( UnityPerMaterial )
                half4 _Color;

                float  _EnableWind;

                float4 _WindDirection;
                float  _BaseWindStrength;
                float  _GustStrength;
                float  _GustInterval;
                float  _GustDuration;
                float  _WindNoiseScale;
                float  _BendByHeight;
                float  _WindTimeScale;
                float  _RandomSeed;

                float4 _KeyColor;
                float  _KeyTolerance;
                float  _KeyFeather;

                float  _BlurAmount;
                float  _BlurMaxRadius;

                float  _TintVariationAmount;
            CBUFFER_END

            float Hash11(float n)
            {
                return frac(sin(n * 17.0 + _RandomSeed * 12.9898) * 43758.5453123);
            }

            float ComputeGustAmplitude(float t)
            {
                float interval = max(_GustInterval, 0.01);
                float durationNorm = saturate(_GustDuration / interval);

                float cycle = t / interval;
                float gustIndex = floor(cycle);
                float phase = frac(cycle);

                float active = step(phase, durationNorm);
                float s = (durationNorm > 0.0) ? saturate(phase / durationNorm) : 0.0;
                float envelope = sin(s * 3.14159265);

                float randAmp = Hash11(gustIndex);
                float gustStrength = _BaseWindStrength + randAmp * _GustStrength;

                return gustStrength * active * envelope;
            }

            float3 RGB2YIQ(float3 c)
            {
                float3 yiq;
                yiq.x = dot(c, float3(0.299,     0.587,      0.114));
                yiq.y = dot(c, float3(0.595716, -0.274453,  -0.321263));
                yiq.z = dot(c, float3(0.211456, -0.522591,   0.311135));
                return yiq;
            }

            float3 YIQ2RGB(float3 c)
            {
                float3 rgb;
                rgb.r = c.x + 0.9563 * c.y + 0.6210 * c.z;
                rgb.g = c.x - 0.2721 * c.y - 0.6474 * c.z;
                rgb.b = c.x - 1.1070 * c.y + 1.7046 * c.z;
                return rgb;
            }

            float3 ApplyInstanceTint(float3 rgb)
            {
                float v = _TintVariationAmount;
                if (v <= 0.0001)
                    return rgb;

                float randHue = Hash11(_RandomSeed * 7.77);
                float randAmp = Hash11(_RandomSeed * 13.37);
                float localAmount = v * randAmp;

                float maxHueShift = 3.14159;
                float angle = (randHue * 2.0 - 1.0) * maxHueShift * localAmount;

                float3 yiq = RGB2YIQ(rgb);

                float I = yiq.y;
                float Q = yiq.z;

                float cosA = cos(angle);
                float sinA = sin(angle);

                float I2 = I * cosA - Q * sinA;
                float Q2 = I * sinA + Q * cosA;

                yiq.y = I2;
                yiq.z = Q2;

                float3 outRgb = YIQ2RGB(yiq);
                return saturate(outRgb);
            }

            // --- BLUR helper (3x3) ---
            float4 SampleMainWithBlur(float2 uv)
            {
                float blurAmount = _BlurAmount;
                if (blurAmount <= 0.0001)
                {
                    return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                }

                float2 texel  = _MainTex_TexelSize.xy;
                float  radius = blurAmount * _BlurMaxRadius;
                float2 offset = texel * radius;

                float4 col = 0;
                float  weightSum = 0;

                [unroll]
                for (int x = -1; x <= 1; x++)
                {
                    [unroll]
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 uvOff = uv + float2(x, y) * offset;
                        float  w = 1.0;
                        col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvOff) * w;
                        weightSum += w;
                    }
                }

                return col / max(weightSum, 0.0001);
            }

            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(attributes);

                SetUpSpriteInstanceProperties();

                float windBlend = saturate(_EnableWind);

                if (windBlend > 0.0001)
                {
                    float height01 = saturate(attributes.uv.y);
                    float2 windDir = normalize(_WindDirection.xy + float2(1e-5, 0.0));

                    float t = (_Time.y + _RandomSeed * 13.37) * _WindTimeScale;
                    float gustAmp = ComputeGustAmplitude(t);

                    float noise = sin(t * 2.0
                                      + attributes.positionOS.x * _WindNoiseScale
                                      + attributes.positionOS.y * (_WindNoiseScale * 0.37)
                                      + _RandomSeed * 5.123);

                    float bend = height01 * _BendByHeight;
                    float sway = gustAmp * noise * bend * windBlend;

                    attributes.positionOS.xy += windDir * sway;
                }

                attributes.positionOS = UnityFlipSprite(attributes.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = TransformObjectToWorld(attributes.positionOS);
                #endif
                o.uv = attributes.uv;
                o.color = attributes.color * _Color * unity_SpriteColor;
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 mainTex = i.color * SampleMainWithBlur(i.uv);

                // Variation de teinte
                mainTex.rgb = ApplyInstanceTint(mainTex.rgb);

                float3 diff = mainTex.rgb - _KeyColor.rgb;
                float dist  = length(diff);
                float tol   = _KeyTolerance;
                float feather = max(_KeyFeather, 1e-5);
                float aMask = saturate((dist - tol) / feather);
                mainTex.a *= aMask;

                #if defined(DEBUG_DISPLAY)
                    SurfaceData2D surfaceData;
                    InputData2D inputData;
                    half4 debugColor = 0;

                    InitializeSurfaceData(mainTex.rgb, mainTex.a, surfaceData);
                    InitializeInputData(i.uv, inputData);
                    SETUP_DEBUG_TEXTURE_DATA_2D_NO_TS(inputData, i.positionWS, i.positionCS, _MainTex);

                    if (CanDebugOverrideOutputColor(surfaceData, inputData, debugColor))
                    {
                        return debugColor;
                    }
                #endif

                return mainTex;
            }
            ENDHLSL
        }
    }
}
