#ifndef ADDITIONALLIGHTS_NDOTL_INCLUDED
#define ADDITIONALLIGHTS_NDOTL_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"

#pragma multi_compile _ _ADDITIONAL_LIGHTS
#pragma multi_compile _ _CLUSTER_LIGHT_LOOP

void AdditionalLightsNdotL_float(float3 WorldPosition, float3 WorldNormal, float3 ViewDirWS, float2 ScreenUV, out float NdotLSum)
{
    NdotLSum = 0;
#if !defined(SHADERGRAPH_PREVIEW)
    InputData inputData = (InputData)0;
    inputData.positionWS = WorldPosition;
    inputData.normalWS = WorldNormal;
    inputData.viewDirectionWS = ViewDirWS;
    inputData.normalizedScreenSpaceUV = ScreenUV;

    #if defined(_ADDITIONAL_LIGHTS)
        uint pixelLightCount = GetAdditionalLightsCount();
        LIGHT_LOOP_BEGIN(pixelLightCount)
            Light light = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1,1,1,1));
            NdotLSum += saturate(dot(WorldNormal, light.direction)) * light.distanceAttenuation * light.shadowAttenuation * length(light.color);
        LIGHT_LOOP_END
    #endif
#endif
}

#endif