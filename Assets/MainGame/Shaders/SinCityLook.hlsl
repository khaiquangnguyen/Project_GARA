// Sin City look shared by the per-character Spine shader and the Noir World
// full-screen pass: hard ink/paper threshold with one thin mid-tone band,
// one flat saturated accent hue kept, optional negative flash.
// Tone and hue are judged in gamma space so thresholds match what you see.
#ifndef SIN_CITY_LOOK_INCLUDED
#define SIN_CITY_LOOK_INCLUDED

struct SinCityParams
{
    float shadowThreshold;
    float lightThreshold;
    float midTone;
    float softness;
    float3 ink;
    float3 paper;
    float accentHue;
    float accentHueRange;
    float accentMinSaturation;
    float accentFlatness;
    float accentAmount;
    float invert;
};

float3 SinCityToGamma(float3 c)
{
    c = max(c, 0);
    return lerp(1.055 * pow(c, 1.0 / 2.4) - 0.055, c * 12.92, step(c, 0.0031308));
}

float3 SinCityToLinear(float3 c)
{
    c = max(c, 0);
    return lerp(pow((c + 0.055) / 1.055, 2.4), c / 12.92, step(c, 0.04045));
}

float3 SinCityRgbToHsv(float3 c)
{
    float4 k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = c.g < c.b ? float4(c.bg, k.wz) : float4(c.gb, k.xy);
    float4 q = c.r < p.x ? float4(p.xyw, c.r) : float4(c.r, p.yzx);
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 SinCityHsvToRgb(float3 c)
{
    float3 p = abs(frac(c.xxx + float3(1.0, 2.0 / 3.0, 1.0 / 3.0)) * 6.0 - 3.0);
    return c.z * lerp(float3(1, 1, 1), saturate(p - 1.0), c.y);
}

// color: straight (non-premultiplied) colour in the target space.
// isLinear: 1 when the project renders in linear space.
float3 SinCityLook(float3 color, SinCityParams p, float isLinear)
{
    float3 g = isLinear > 0.5 ? SinCityToGamma(color) : saturate(color);
    float lum = dot(g, float3(0.3, 0.59, 0.11));

    float toMid = smoothstep(p.shadowThreshold - p.softness, p.shadowThreshold + p.softness, lum);
    float toPaper = smoothstep(p.lightThreshold - p.softness, p.lightThreshold + p.softness, lum);
    float tone = lerp(lerp(0, p.midTone, toMid), 1, toPaper);
    float3 result = lerp(p.ink, p.paper, tone);

    float3 hsv = SinCityRgbToHsv(g);
    float hueDistance = abs(frac(hsv.x - p.accentHue + 0.5) - 0.5);
    float accentMask = 1 - smoothstep(p.accentHueRange, p.accentHueRange + 0.03, hueDistance);
    accentMask *= smoothstep(p.accentMinSaturation, p.accentMinSaturation + 0.1, hsv.y);
    accentMask *= smoothstep(0.08, 0.18, hsv.z);
    accentMask *= p.accentAmount;

    float3 flatAccent = SinCityHsvToRgb(float3(hsv.x, 1, 1));
    flatAccent = isLinear > 0.5 ? SinCityToLinear(flatAccent) : flatAccent;
    float3 accent = lerp(color, flatAccent, p.accentFlatness);
    result = lerp(result, accent, accentMask);

    return lerp(result, 1 - saturate(result), p.invert);
}

#endif
