using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace ATM.DOTS.Project04
{
    [MaterialProperty("_Color", MaterialPropertyFormat.Float4)]
    public struct BasicMaterial_ColorData : IComponentData
    {
        public float4 materialColor;
    }

    [MaterialProperty("_Smoothness", MaterialPropertyFormat.Float)]
    public struct BasicMaterial_SmoothnessData : IComponentData
    {
        public float smoothness;
    }

    [MaterialProperty("_AmbientOcclusion", MaterialPropertyFormat.Float)]
    public struct BasicMaterial_AmbientOcclusionData : IComponentData
    {
        public float ambientOcclusion;
    }

    [MaterialProperty("_Emission", MaterialPropertyFormat.Float4)]
    public struct BasicMaterial_EmissionData : IComponentData
    {
        public float4 emissionColor;
    }
}
