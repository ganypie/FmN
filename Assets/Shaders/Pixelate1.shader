Shader "Custom/Pixelate"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        
        [Header(Pixelate Settings)]
        _PixelSize("Pixel Size", Range(1, 64)) = 8
        [Range(0, 1)]
        _Intensity("Intensity", Float) = 1.0
        
        [Header(Quality)]
        [Toggle] _PreserveAspect("Preserve Aspect Ratio", Float) = 1
        
        [Header(Edge Smoothing)]
        [Toggle] _SoftEdges("Soft Pixel Edges", Float) = 1
        [Range(0, 0.3)]
        _EdgeSoftness("Edge Softness", Float) = 0.08
        
        [Header(Sample Quality)]
        [Range(1, 3)]
        _SampleCount("Sample Quality", Int) = 2
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Cull Off
        ZWrite Off
        ZTest Always
        
        Pass
        {
            Name "PixelatePass"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            float _PixelSize;
            float _Intensity;
            float _PreserveAspect;
            float _SoftEdges;
            float _EdgeSoftness;
            int _SampleCount;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                return output;
            }

            // Мягкое сглаживание краев пикселей
            float2 GetEdgeSmoothness(float2 fracCoord)
            {
                // Вычисляем расстояние до границ пикселя для каждой оси
                float2 distToEdge = min(fracCoord, 1.0 - fracCoord);
                
                // Плавное затухание на краях для каждой оси
                return smoothstep(0.0, _EdgeSoftness, distToEdge);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 screenSize = _ScreenParams.xy;
                
                // Вычисляем размер пикселя
                float2 pixelSize = _PixelSize;
                if (_PreserveAspect > 0.5)
                {
                    pixelSize.y = pixelSize.x / (screenSize.x / screenSize.y);
                }
                
                // Количество пикселей на экране
                float2 pixelCount = screenSize / max(pixelSize, 1.0);
                
                // Преобразуем UV в пиксельные координаты
                float2 pixelCoord = input.uv * pixelCount;
                
                // Точное выравнивание: floor для индекса + 0.5 для центра
                // Это гарантирует полное покрытие без зазоров
                float2 pixelIndex = floor(pixelCoord);
                float2 pixelCenter = pixelIndex + 0.5;
                
                // Фракциональная часть для вычисления позиции внутри пикселя
                float2 fracCoord = pixelCoord - pixelIndex;
                
                // Мягкое сглаживание краев
                float2 finalCoord;
                if (_SoftEdges > 0.5 && _EdgeSoftness > 0.0)
                {
                    // Вычисляем фактор сглаживания для краев (1.0 на краях, 0.0 в центре)
                    float2 edgeFactor = GetEdgeSmoothness(fracCoord);
                    
                    // На краях пикселя интерполируем к центру для сглаживания
                    // В центре остаемся на оригинальной позиции для резкости
                    finalCoord = lerp(pixelCenter, pixelCoord, edgeFactor);
                }
                else
                {
                    // Резкие края - точный центр пикселя
                    finalCoord = pixelCenter;
                }
                
                // Применяем интенсивность
                finalCoord = lerp(pixelCoord, finalCoord, _Intensity);
                
                // Преобразуем обратно в UV
                float2 quantizedUV = finalCoord / pixelCount;
                
                // Многократное сэмплирование для смягчения резких переходов
                half4 col = 0;
                if (_SampleCount > 1 && _SoftEdges > 0.5)
                {
                    float sampleStep = 0.5 / float(_SampleCount);
                    float sampleWeight = 1.0 / float(_SampleCount * _SampleCount);
                    
                    for (int x = 0; x < _SampleCount; x++)
                    {
                        for (int y = 0; y < _SampleCount; y++)
                        {
                            float2 offset = float2(
                                (x - (_SampleCount - 1) * 0.5) * sampleStep,
                                (y - (_SampleCount - 1) * 0.5) * sampleStep
                            ) / pixelCount;
                            
                            float2 sampleUV = clamp(quantizedUV + offset, 0.0, 1.0);
                            col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, sampleUV) * sampleWeight;
                        }
                    }
                }
                else
                {
                    // Одиночное сэмплирование для производительности
                    quantizedUV = clamp(quantizedUV, 0.0, 1.0);
                    col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, quantizedUV);
                }
                
                return col;
            }
            ENDHLSL
        }
    }
    
    Fallback Off
}
