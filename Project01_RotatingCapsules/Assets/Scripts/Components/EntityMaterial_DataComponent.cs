using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
 
namespace ATM.DOTS.Project01
{
    [MaterialProperty("_Color", MaterialPropertyFormat.Float4)]
    public struct MaterialColorDataComponent : IComponentData
    {
        public float4 materialColor;
    }

    [MaterialProperty("_Emission", MaterialPropertyFormat.Float4)]
    public struct MaterialEmissionDataComponent : IComponentData
    {
        public float3 emission;
    }
}