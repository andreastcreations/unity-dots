using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace ATM.DOTS.Project04
{
    /// <summary>
    /// Stores color data.
    /// </summary>
    [MaterialProperty("_Color", MaterialPropertyFormat.Float4)]
    public struct BasicMaterial_ColorData : IComponentData
    {
        public float4 materialColor;
    }

    /// <summary>
    /// Stores smoothness data.
    /// </summary>
    [MaterialProperty("_Smoothness", MaterialPropertyFormat.Float)]
    public struct BasicMaterial_SmoothnessData : IComponentData
    {
        public float smoothness;
    }

    /// <summary>
    /// Stores ambient occlusion data.
    /// </summary>
    [MaterialProperty("_AmbientOcclusion", MaterialPropertyFormat.Float)]
    public struct BasicMaterial_AmbientOcclusionData : IComponentData
    {
        public float ambientOcclusion;
    }

    /// <summary>
    /// Stores emission color data.
    /// </summary>
    [MaterialProperty("_Emission", MaterialPropertyFormat.Float4)]
    public struct BasicMaterial_EmissionData : IComponentData
    {
        public float4 emissionColor;
    }
}
