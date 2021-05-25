using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace ATM.DOTS.Project03
{
    [MaterialProperty("_Color", MaterialPropertyFormat.Float4)]
    public struct MaterialColorDataComponent : IComponentData
    {
        public float4 materialColor;
    }

    [MaterialProperty("_Smoothness", MaterialPropertyFormat.Float)]
    public struct MaterialSmoothnessDataComponent : IComponentData
    {
        public float smoothness;
    }

    [MaterialProperty("_AmbientOcclusion", MaterialPropertyFormat.Float)]
    public struct MaterialAODataComponent : IComponentData
    {
        public float ambientOcclusion;
    }
}
