Shader "Universal Render Pipeline/2D/PlantWind"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}

        // Legacy properties (pour compatibilité avec le shader sprite legacy)
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

        // --- Toggle vent ---
        [Toggle] _EnableWind ("Enable Wind", Float) = 1

        // --- Paramètres de vent ---
        _WindDirection ("Wind Direction (XY)", Vector) = (1, 0, 0, 0)
        _BaseWindStrength ("Base Wind Strength", Float) = 0.02
        _GustStrength ("Gust Extra Strength", Float) = 0.08
        _GustInterval ("Gust Interval (seconds)", Float) = 5.0
        _GustDuration ("Gust Duration (seconds)", Float) = 1.5
        _WindNoiseScale ("Wind Noise Scale", Float) = 1.0
        _BendByHeight ("Bend By Height (0-2)", Float) = 1.0
        _WindTimeScale ("Wind Time Scale", Float) = 1.0
        _RandomSeed ("Random Seed", Float) = 1.0

        // --- Chroma key (suppression de couleur) ---
        _KeyColor     ("Key Color", Color) = (1,1,1,1)              // blanc
        _KeyTolerance ("Key Tolerance", Range(0,1)) = 0.1
        _KeyFeather   ("Key Feather", Range(0,1))   = 0.1
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
        ZWrite Off

        // ============================
        // Pass 1 : Universal2D (Renderer 2D)
        // ============================
        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            #if defined(DEBUG_DISPLAY)
                #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/InputData2D.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/SurfaceData2D.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging2D.hlsl"
            #endif

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

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
                #if defined(DEBUG_DISPLAY)
                    float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            UNITY_TEXTURE_STREAMING_DEBUG_VARS_FOR_TEX(_MainTex);

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
            CBUFFER_END

            float Hash11(float n)
            {
                return frac(sin(n * 17.0 + _RandomSeed * 0.1) * 43758.5453123);
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

            Varyings UnlitVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                SetUpSpriteInstanceProperties();

                // ---------- VENT (optionnel) ----------
                if (_EnableWind > 0.5)
                {
                    float height01 = saturate(v.uv.y);
                    float2 windDir = normalize(_WindDirection.xy + float2(1e-5, 0.0));
                    float t = _Time.y * _WindTimeScale;
                    float gustAmp = ComputeGustAmplitude(t);

                    float noise = sin(t * 2.0
                                      + v.positionOS.x * _WindNoiseScale
                                      + v.positionOS.y * (_WindNoiseScale * 0.37));

                    float bend = height01 * _BendByHeight;
                    float sway = gustAmp * noise * bend;

                    v.positionOS.xy += windDir * sway;
                }
                // --------------------------------------

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = v.uv;
                o.color = v.color * _Color * unity_SpriteColor;
                return o;
            }

            half4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 col = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // === Chroma key sur la couleur clé ===
                float3 diff = col.rgb - _KeyColor.rgb;
                float dist = length(diff);

                float tol = _KeyTolerance;
                float feather = max(_KeyFeather, 1e-5);
                float aMask = saturate((dist - tol) / feather);
                col.a *= aMask;

                #if defined(DEBUG_DISPLAY)
                    SurfaceData2D surfaceData;
                    InputData2D inputData;
                    half4 debugColor = 0;

                    InitializeSurfaceData(col.rgb, col.a, surfaceData);
                    InitializeInputData(i.uv, inputData);
                    SETUP_DEBUG_TEXTURE_DATA_2D_NO_TS(inputData, i.positionWS, i.positionCS, _MainTex);

                    if (CanDebugOverrideOutputColor(surfaceData, inputData, debugColor))
                    {
                        return debugColor;
                    }
                #endif

                return col;
            }
            ENDHLSL
        }

        // ============================
        // Pass 2 : UniversalForward
        // ============================
        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue"="Transparent" "RenderType"="Transparent"}

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            #if defined(DEBUG_DISPLAY)
                #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/InputData2D.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/SurfaceData2D.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging2D.hlsl"
            #endif

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            #pragma multi_compile_instancing
            #pragma multi_compile _ SKINNED_SPRITE
            #pragma multi_compile_fragment _ DEBUG_DISPLAY

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
                    float3  positionWS      : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            UNITY_TEXTURE_STREAMING_DEBUG_VARS_FOR_TEX(_MainTex);

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
            CBUFFER_END

            float Hash11(float n)
            {
                return frac(sin(n * 17.0 + _RandomSeed * 0.1) * 43758.5453123);
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

            Varyings UnlitVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                SetUpSpriteInstanceProperties();

                if (_EnableWind > 0.5)
                {
                    float height01 = saturate(v.uv.y);
                    float2 windDir = normalize(_WindDirection.xy + float2(1e-5, 0.0));
                    float t = _Time.y * _WindTimeScale;
                    float gustAmp = ComputeGustAmplitude(t);

                    float noise = sin(t * 2.0
                                      + v.positionOS.x * _WindNoiseScale
                                      + v.positionOS.y * (_WindNoiseScale * 0.37));

                    float bend = height01 * _BendByHeight;
                    float sway = gustAmp * noise * bend;

                    v.positionOS.xy += windDir * sway;
                }

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = v.uv;
                o.color = v.color * _Color * unity_SpriteColor;
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 col = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                float3 diff = col.rgb - _KeyColor.rgb;
                float dist = length(diff);

                float tol = _KeyTolerance;
                float feather = max(_KeyFeather, 1e-5);
                float aMask = saturate((dist - tol) / feather);
                col.a *= aMask;

                #if defined(DEBUG_DISPLAY)
                    SurfaceData2D surfaceData;
                    InputData2D inputData;
                    half4 debugColor = 0;

                    InitializeSurfaceData(col.rgb, col.a, surfaceData);
                    InitializeInputData(i.uv, inputData);
                    SETUP_DEBUG_TEXTURE_DATA_2D_NO_TS(inputData, i.positionWS, i.positionCS, _MainTex);

                    if (CanDebugOverrideOutputColor(surfaceData, inputData, debugColor))
                    {
                        return debugColor;
                    }
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}
