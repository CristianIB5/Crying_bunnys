#ifndef ADDITIONALLIGHTS_NDOTL_INCLUDED
#define ADDITIONALLIGHTS_NDOTL_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"

#pragma multi_compile _ _ADDITIONAL_LIGHTS
#pragma multi_compile _ _CLUSTER_LIGHT_LOOP
#pragma multi_compile _ _ADDITIONAL_LIGHTS
#pragma multi_compile _ _CLUSTER_LIGHT_LOOP
#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS

void AdditionalLightsNdotL_float(float3 WorldPosition, float3 WorldNormal, float3 ViewDirWS, float2 ScreenUV, out float NdotLSum, out float3 LightColor)
{
    NdotLSum = 0;
    LightColor = float3(0, 0, 0);

#if !defined(SHADERGRAPH_PREVIEW)
    InputData inputData = (InputData) 0;
    inputData.positionWS = WorldPosition;
    inputData.normalWS = WorldNormal;
    inputData.viewDirectionWS = ViewDirWS;
    inputData.normalizedScreenSpaceUV = ScreenUV;

#if defined(_ADDITIONAL_LIGHTS)
        uint pixelLightCount = GetAdditionalLightsCount();
        LIGHT_LOOP_BEGIN(pixelLightCount)
            Light light = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1,1,1,1));
            
            // Calculamos atenuación y el ángulo (NdotL)
            float attenuation = light.distanceAttenuation * light.shadowAttenuation;
            float ndotl = saturate(dot(WorldNormal, light.direction));
            
            // Sumamos la intensidad (para el NdotLSum original)
            NdotLSum += ndotl * attenuation * length(light.color);
            
            // Sumamos EL COLOR puro de la luz (RGB * Intensidad)
            LightColor += light.color * ndotl * attenuation;
        LIGHT_LOOP_END
#endif
#endif
}
#endif