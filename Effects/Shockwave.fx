sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float4 uShaderSpecificData;
float2 uTargetPosition; // World position of the shockwave center
float uProgress; // Radius of the wave in pixels
float uIntensity; // Strength of the wave
float2 uScreenPosition; // World position of the screen top-left
float2 uScreenResolution; // Resolution of the screen

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    // coords are 0 to 1 UVs. Convert to world coords:
    float2 worldPos = uScreenPosition + (coords * uScreenResolution);
    float2 diff = worldPos - uTargetPosition;
    
    // Distance in world pixels
    float dist = length(diff);
    
    float waveThick = 150.0; // Wave thickness in pixels
    float waveDist = abs(dist - uProgress);
    
    if (waveDist < waveThick && dist > 0.0)
    {
        float magnitude = 1.0 - (waveDist / waveThick);
        magnitude *= uIntensity; 
        
        float2 distortion = normalize(diff) * magnitude * 0.1;
        coords -= distortion;
    }
    
    return tex2D(uImage0, coords);
}

technique Technique1
{
    pass Shockwave
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
