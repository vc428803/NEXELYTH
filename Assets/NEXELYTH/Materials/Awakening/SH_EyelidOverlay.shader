Shader "NEXELYTH/Awakening/EyelidOverlay"
{
    Properties
    {
        _EyeOpen ("Eye Open", Range(0, 1)) = 0
        _Softness ("Edge Softness", Range(0.005, 0.08)) = 0.025
        _EyeWidth ("Eye Width", Range(0.65, 1.0)) = 0.90

        _UpperOpening ("Upper Lid Movement", Range(0.5, 1.5)) = 1.0
        _LowerOpening ("Lower Lid Movement", Range(0.1, 0.8)) = 0.60

        _UpperPeakOffset ("Upper Peak Offset", Range(-0.2, 0.2)) = -0.06
        _Asymmetry ("Natural Asymmetry", Range(-0.1, 0.1)) = 0.025

        _VerticalOffset ("Vertical Offset", Range(-0.1, 0.1)) = -0.015
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        ZWrite Off
        ZTest Always
        Cull Off

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            // 支援 XR / GPU Instancing
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;

                // XR / Instancing
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;

                // XR Stereo
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)

                float _EyeOpen;
                float _Softness;
                float _EyeWidth;

                float _UpperOpening;
                float _LowerOpening;

                float _UpperPeakOffset;
                float _Asymmetry;

                float _VerticalOffset;

            CBUFFER_END


            Varyings vert(Attributes input)
            {
                Varyings output;

                // 初始化 Instance ID
                UNITY_SETUP_INSTANCE_ID(input);

                // 初始化 Stereo Output
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionHCS =
                    TransformObjectToHClip(
                        input.positionOS.xyz
                    );

                output.uv =
                    input.uv;

                return output;
            }


            half4 frag(Varyings input) : SV_Target
            {
                // XR Stereo eye index
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);


                // =========================================
                // 完全閉眼
                // =========================================

                if (_EyeOpen <= 0.001)
                {
                    return half4(
                        0,
                        0,
                        0,
                        1
                    );
                }



                // =========================================
                // UV
                // =========================================

                float2 p =
                    input.uv - 0.5;

                float x =
                    p.x * 2.0;

                float normalizedX =
                    x / _EyeWidth;

                float absX =
                    abs(normalizedX);


                // =========================================
                // 超過左右眼角
                // =========================================

                if (absX >= 1.0)
                {
                    return half4(
                        0,
                        0,
                        0,
                        1
                    );
                }


                // =========================================
                // 開眼程度
                //
                // 動畫曲線交給
                // PlayerAwakeningSequence 控制。
                // =========================================

                float open =
                    saturate(_EyeOpen);


                // =========================================
                // 眼角收合
                // =========================================

                float corner =
                    1.0 - absX;

                float cornerShape =
                    smoothstep(
                        0.0,
                        0.38,
                        corner
                    );


                // =========================================
                // 上眼皮
                // =========================================

                float upperX =
                    normalizedX
                    - _UpperPeakOffset;

                float upperAbs =
                    saturate(
                        abs(upperX)
                    );

                float upperCurve =
                    1.0
                    -
                    pow(
                        upperAbs,
                        2.4
                    );

                upperCurve =
                    saturate(
                        upperCurve
                    );


                // =========================================
                // 下眼皮
                //
                // 比上眼皮更平。
                // =========================================

                float lowerCurve =
                    1.0
                    -
                    pow(
                        absX,
                        3.5
                    );

                lowerCurve =
                    saturate(
                        lowerCurve
                    );


                // =========================================
                // 微量自然不對稱
                // =========================================

                float asymmetry =
                    normalizedX
                    *
                    _Asymmetry
                    *
                    open;


                // =========================================
                // 微睜階段
                //
                // 避免 0 ~ 30% 時只剩一條水平線。
                // =========================================

                float microOpenBoost =
                    lerp(
                        1.25,
                        1.0,
                        saturate(
                            open / 0.30
                        )
                    );


                // =========================================
                // 上眼皮位置
                // =========================================

                float upperLid =
                    _VerticalOffset
                    +
                    upperCurve
                    *
                    cornerShape
                    *
                    open
                    *
                    0.48
                    *
                    _UpperOpening
                    *
                    microOpenBoost
                    +
                    asymmetry;


                // =========================================
                // 下眼皮位置
                // =========================================

                float lowerLid =
                    _VerticalOffset
                    -
                    lowerCurve
                    *
                    cornerShape
                    *
                    open
                    *
                    0.48
                    *
                    _LowerOpening
                    -
                    asymmetry * 0.25;


                // =========================================
                // 上眼皮柔邊
                // =========================================

                float upperVisible =
                    1.0
                    -
                    smoothstep(
                        upperLid - _Softness,
                        upperLid + _Softness,
                        p.y
                    );


                // =========================================
                // 下眼皮柔邊
                // =========================================

                float lowerVisible =
                    smoothstep(
                        lowerLid - _Softness,
                        lowerLid + _Softness,
                        p.y
                    );


                // =========================================
                // 可見區域
                // =========================================

                float visibleArea =
                    upperVisible
                    *
                    lowerVisible;


                // =========================================
                // 左右眼角柔化
                // =========================================

                float cornerFade =
                    smoothstep(
                        0.0,
                        0.10,
                        corner
                    );

                visibleArea *=
                    cornerFade;


                // =========================================
                // 最終眼皮遮罩
                // =========================================

                // 最後一小段睜眼過程，
                // 讓殘留的眼皮逐漸淡出，而不是瞬間消失。
                float finalFade =
                    1.0
                    -
                    smoothstep(
                        0.88,
                        1.0,
                        open
                    );

                float alpha =
                    (1.0 - visibleArea)
                    * finalFade;


                return half4(
                    0,
                    0,
                    0,
                    alpha
                );
            }

            ENDHLSL
        }
    }
}