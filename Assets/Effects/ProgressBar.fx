sampler uImage0 : register(s0);

float uTime;
float2 uResolution;
float uBorderThickness;
float uBorderRadius;
float uFillPercent;
float4 uBackgroundColor;
float4 uBorderColor;
float4 uFillColor1;
float4 uFillColor2;
float4 uPulseColor;
float uIsCapped;

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float2 st = coords;
    float2 pixelPos = st * uResolution;
    
    // Calculate distance to rounded rectangle border (pill shape)
    float2 halfRes = uResolution * 0.5;
    float cornerRadius = uBorderRadius;
    float2 q = abs(pixelPos - halfRes) - halfRes + float2(cornerRadius, cornerRadius);
    float dist = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0);
    
    // Discard pixels outside the pill
    if (dist > cornerRadius) {
        return float4(0, 0, 0, 0);
    }
    
    // Calculate if it's border or inner
    bool isBorder = dist > cornerRadius - uBorderThickness;
    
    // Calculate fill
    float fillX = uResolution.x * uFillPercent;
    bool isFilled = pixelPos.x <= fillX;
    
    float4 finalColor;
    
    if (isBorder) {
        finalColor = uBorderColor;
    } else {
        if (isFilled) {
            // Gradient fill from FillColor1 to FillColor2
            float4 fillCol = lerp(uFillColor1, uFillColor2, st.x);
            
            if (uIsCapped > 0.5) {
                // Pulse effect when capped
                float pulse = (sin(uTime * 4.0) + 1.0) * 0.5;
                fillCol = lerp(uPulseColor, float4(1, 1, 1, 1), pulse * 0.5);
            } else {
                // Moving glow wave when not capped
                float glowWidth = 30.0;
                float totalRange = uResolution.x + glowWidth;
                float glowPos = (uTime * 150.0) - floor((uTime * 150.0) / totalRange) * totalRange - glowWidth;
                float glowFactor = 1.0 - clamp(abs(pixelPos.x - glowPos) / glowWidth, 0.0, 1.0);
                fillCol = lerp(fillCol, float4(1, 1, 1, 1), glowFactor * 0.5);
            }
            finalColor = fillCol;
        } else {
            finalColor = uBackgroundColor;
        }
    }
    
    return finalColor * color;
}

technique Technique1
{
    pass ProgressBarPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
