sampler uImage0 : register(s0);

float uTime;
float2 uResolution;
float uBorderRadius;
float4 uSkyColor;
float4 uCloudColor;
float uCloudDensity;

float rand(float2 n) { 
    return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
}

float noise(float2 p){
    float2 ip = floor(p);
    float2 u = frac(p);
    u = u*u*(3.0-2.0*u);
    
    float res = lerp(
        lerp(rand(ip), rand(ip+float2(1.0,0.0)), u.x),
        lerp(rand(ip+float2(0.0,1.0)), rand(ip+float2(1.0,1.0)), u.x), u.y);
    return res*res;
}

float fbm(float2 x) {
    float v = 0.0;
    float a = 0.5;
    float2 shift = float2(100.0, 100.0);
    float c = cos(0.5);
    float s = sin(0.5);
    for (int i = 0; i < 4; ++i) {
        v += a * noise(x);
        float nx = c * x.x - s * x.y;
        float ny = s * x.x + c * x.y;
        x = float2(nx, ny) * 2.0 + shift;
        a *= 0.5;
    }
    return v;
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float2 st = coords;
    
    // Maintain aspect ratio for noise to avoid stretching
    float aspect = uResolution.x / uResolution.y;
    float2 noiseCoords = float2(st.x * aspect, st.y);
    
    float2 q = float2(0.,0.);
    q.x = fbm(noiseCoords * 2.0 + uTime * 0.05);
    q.y = fbm(noiseCoords * 2.0 + float2(1.0, 1.0));
    
    float2 r = float2(0.,0.);
    r.x = fbm(noiseCoords * 3.0 + q + float2(1.7,9.2) + uTime * 0.03);
    r.y = fbm(noiseCoords * 3.0 + q + float2(8.3,2.8) + uTime * 0.02);
    
    float f = fbm(noiseCoords * 2.5 + r);
    
    // Map f to cloud density
    float cloudFactor = smoothstep(1.0 - uCloudDensity, 1.0 + uCloudDensity * 0.5, f);
    
    float4 baseColor = lerp(uSkyColor, uCloudColor, cloudFactor * 0.7); // Max opacity of clouds is 70% to keep it soft
    
    // Add a slight gradient to the sky (darker at the top)
    baseColor.rgb *= lerp(1.0, 0.7, st.y);
    
    // Calculate rounded corners distance
    float2 pixelPos = st * uResolution;
    float2 halfRes = uResolution * 0.5;
    float2 box = abs(pixelPos - halfRes) - halfRes + float2(uBorderRadius, uBorderRadius);
    float dist = length(max(box, 0.0)) + min(max(box.x, box.y), 0.0);
    
    if (dist > uBorderRadius) {
        return float4(0, 0, 0, 0);
    }
    
    if (dist > uBorderRadius - 2.0) {
        baseColor.rgb += float3(0.15, 0.15, 0.15); // Subtle border
    }
    
    return baseColor * color * baseColor.a;
}

technique Technique1
{
    pass CloudPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
