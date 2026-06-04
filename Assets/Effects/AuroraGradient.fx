sampler uImage0 : register(s0);

float uTime;
float2 uResolution;
float uHoverGlow;
float uBorderRadius;
float4 uColor1;
float4 uColor2;
float4 uColor3;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float2 st = coords;
    
    // Generate organic wavy patterns for the "aurora"
    float wave1 = sin(st.x * 6.0 + uTime * 1.2) * 0.5 + 0.5;
    float wave2 = sin(st.y * 4.0 - uTime * 0.8 + wave1 * 2.0) * 0.5 + 0.5;
    float wave3 = sin((st.x + st.y) * 5.0 + uTime * 1.5) * 0.5 + 0.5;
    
    // Blend the three colors based on the waves
    float4 auroraColor = lerp(uColor1, uColor2, wave1);
    auroraColor = lerp(auroraColor, uColor3, wave2);
    
    // Add some brightness variation to the waves
    auroraColor.rgb *= (wave3 * 0.4 + 0.6);
    
    // Start with a dark backing base color (e.g., highly opaque dark slate blue/grey)
    float4 baseColor = float4(0.06, 0.08, 0.15, 0.85); // 85% opacity dark backing
    
    // Overlay the aurora waves using additive blending (avoids double-dimming due to pre-multiplied alphas)
    baseColor.rgb += auroraColor.rgb;
    
    // Calculate rounded corners distance
    float2 pixelPos = st * uResolution;
    float2 halfRes = uResolution * 0.5;
    float2 q = abs(pixelPos - halfRes) - halfRes + float2(uBorderRadius, uBorderRadius);
    float dist = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0);
    
    // Discard pixels outside the border radius for smooth rounded corners
    if (dist > uBorderRadius) {
        return float4(0, 0, 0, 0);
    }
    
    // Apply hover glow (brighten the colors slightly)
    baseColor.rgb += float3(uHoverGlow * 0.25, uHoverGlow * 0.25, uHoverGlow * 0.25);
    
    // Apply border effect (optional, simple inner stroke)
    if (dist > uBorderRadius - 2.0) {
        baseColor.rgb += float3(0.3, 0.3, 0.3); // Add a subtle bright border
    }
    
    return baseColor * color * baseColor.a;
}

technique Technique1
{
    pass AuroraPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
