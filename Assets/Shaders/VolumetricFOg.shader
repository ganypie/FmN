Shader "Tutorial/VolumetricFog_RadiusOptimized"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _MaxDistance("Max distance", float) = 100
        _StepSize("Step size", Range(0.1, 20)) = 1
        _DensityMultiplier("Density multiplier", Range(0, 1)) = 1
        _NoiseOffset("Noise offset", float) = 0
        _FogNoise("Fog noise", 3D) = "white" {}
        _NoiseTiling("Noise tiling", float) = 0.1
        _DensityThreshold("Density threshold", Range(0, 1)) = 0.1
        [HDR]_LightContribution("Light contribution", Color) = (1, 1, 1, 1)
        _LightScattering("Light scattering", Range(-1, 1)) = 0.2
        _Anisotropy("Scattering Anisotropy", Range(-1,1)) = 0.2
        _GodRayStrength("God Ray Strength", Range(0,2)) = 1.0
        _QualityLevel("Quality Level (0=Low,1=Med,2=High)", Int) = 1
        _MaxSteps("Max Steps", Int) = 64
        _DensitySmooth("Density Smoothness", Range(0.01,1)) = 0.2
        _SkyFogMultiplier("Sky Fog Multiplier", Range(0,1)) = 0.2
        _FogStartDistance("Fog Start Distance", Range(-100,100)) = 0
        _EarlyExit("Early Exit Threshold", Range(0,1)) = 0.01
        _SkyLimit("Sky Ray Limit Multiplier", Range(0.1,1)) = 0.5
        _FogRadius("Fog Radius Around Player", Range(10,500)) = 100
        _FogFalloff("Fog Radius Falloff", Range(0.1,2)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float4 _Color;
            float _MaxDistance;
            float _DensityMultiplier;
            float _StepSize;
            float _NoiseOffset;
            TEXTURE3D(_FogNoise);
            float _DensityThreshold;
            float _NoiseTiling;
            float4 _LightContribution;
            float _LightScattering;
            float _Anisotropy;
            float _GodRayStrength;
            int _QualityLevel;
            int _MaxSteps;
            float _DensitySmooth;
            float _SkyFogMultiplier;
            float _FogStartDistance;
            float _EarlyExit;
            float _SkyLimit;
            float _FogRadius;
            float _FogFalloff;

            float henyey_greenstein(float angle, float g)
            {
                float denom = 1.0 + g * g - 2.0 * g * angle;
                denom = max(denom, 0.01);
                return (1.0 - g * g) / (4.0 * PI * pow(denom, 1.5f));
            }

            float get_density(float3 worldPos, float entryDist, float3 camPos)
            {
                // Радиальное затухание от игрока
                float distToCam = distance(worldPos, camPos);
                float radiusFade = saturate(1.0 - smoothstep(_FogRadius * (1 - _FogFalloff), _FogRadius, distToCam));

                // 3D-шумовая структура
                float noise3D = _FogNoise.SampleLevel(sampler_TrilinearRepeat, worldPos * 0.01 * _NoiseTiling, 0).r;
                float density = smoothstep(_DensityThreshold, 1.0, noise3D) * _DensityMultiplier;

                // Fade-in по дистанции от игрока
                float fadeIn = smoothstep(_FogStartDistance, _FogStartDistance + 10, entryDist);

                return density * fadeIn * radiusFade;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, IN.texcoord);
                float depth = SampleSceneDepth(IN.texcoord);

                if (depth >= 0.9999)
                {
                    float skyFog = _SkyFogMultiplier * _Color.a;
                    return lerp(col, _Color, skyFog);
                }

                float3 worldPos = ComputeWorldSpacePosition(IN.texcoord, depth, UNITY_MATRIX_I_VP);
                float3 entryPoint = _WorldSpaceCameraPos;
                float3 viewDir = normalize(worldPos - entryPoint);
                float viewLength = length(worldPos - entryPoint);
                float distLimit = min(viewLength, _MaxDistance);

                // Сокращаем дальность для лучей, направленных вверх
                float upDot = saturate(dot(viewDir, float3(0,1,0)));
                distLimit *= lerp(1.0, _SkyLimit, upDot);

                float stepSize = _StepSize;
                int maxSteps = _MaxSteps;
                if (_QualityLevel == 0) { stepSize *= 2.5; maxSteps /= 2; }
                else if (_QualityLevel == 1) { stepSize *= 1.5; }

                float transmittance = 1;
                float4 fogCol = 0;

                float2 pixelCoords = IN.texcoord * _BlitTexture_TexelSize.zw;
                float distTravelled = InterleavedGradientNoise(pixelCoords, (int)(_Time.y / max(HALF_EPS, unity_DeltaTime.x))) * _NoiseOffset;

                [loop]
                for (int i = 0; i < maxSteps && distTravelled < distLimit; i++)
                {
                    float3 rayPos = entryPoint + viewDir * distTravelled;
                    float entryDist = distTravelled;
                    float density = get_density(rayPos, entryDist, entryPoint);
                    if (density > 0.001)
                    {
                        Light mainLight = GetMainLight(TransformWorldToShadowCoord(rayPos));
                        float phase = henyey_greenstein(dot(viewDir, mainLight.direction), _Anisotropy);
                        float godRay = pow(saturate(dot(viewDir, mainLight.direction)), 8) * _GodRayStrength;
                        float3 fogLight = _Color.rgb + (phase * mainLight.color.rgb * mainLight.shadowAttenuation * _LightContribution.rgb * godRay);

                        fogCol.rgb += fogLight * density * stepSize;
                        transmittance *= exp(-density * stepSize);
                        if (transmittance < _EarlyExit) break;
                    }
                    distTravelled += stepSize;
                }

                fogCol.a = 1.0;
                return lerp(col, fogCol, 1.0 - saturate(transmittance));
            }
            ENDHLSL
        }
    }
}
