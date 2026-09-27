using System;
using UnityEngine;

// Tunables of the Sin City look (SinCityLook.hlsl), shared by the per-character
// noirify and the Noir World screen pass. Pushes itself to a material, or to
// shader globals under a prefix.
[Serializable]
public class SinCityLook
{
    [Tooltip("Brightness (0-1) below which a pixel turns to solid ink.")]
    [Range(0f, 1f)]
    [SerializeField] private float shadowThreshold = 0.4f;

    [Tooltip("Brightness (0-1) above which a pixel turns to solid paper. Equal to Shadow Threshold = pure two-tone, no mid band.")]
    [Range(0f, 1f)]
    [SerializeField] private float lightThreshold = 0.55f;

    [Tooltip("Grey of the thin band between the two thresholds.")]
    [Range(0f, 1f)]
    [SerializeField] private float midTone = 0.35f;

    [Tooltip("Blur of the ink/paper edge, to avoid aliasing.")]
    [Range(0f, 0.2f)]
    [SerializeField] private float softness = 0.02f;

    [SerializeField] private Color inkColor = Color.black;

    [Tooltip("Push above 1 (HDR) for blown-out, glowing whites under bloom.")]
    [ColorUsage(false, true)]
    [SerializeField] private Color paperColor = Color.white;

    [Header("Kept Colour")]
    [Tooltip("The one hue that survives, flat and saturated. Only its hue is used.")]
    [SerializeField] private Color accentColor = Color.red;

    [Tooltip("How far (0-0.5 around the colour wheel) from the accent hue still counts.")]
    [Range(0f, 0.5f)]
    [SerializeField] private float accentHueRange = 0.04f;

    [Tooltip("Greyish pixels of the accent hue below this saturation go black and white.")]
    [Range(0f, 1f)]
    [SerializeField] private float accentMinSaturation = 0.45f;

    [Tooltip("0 = keep the original shading, 1 = one flat block of colour.")]
    [Range(0f, 1f)]
    [SerializeField] private float accentFlatness = 1f;

    [Tooltip("0 = no kept colour at all.")]
    [Range(0f, 1f)]
    [SerializeField] private float accentAmount = 1f;

    public void ApplyTo(Material material, float invert)
    {
        Color.RGBToHSV(accentColor, out var accentHue, out _, out _);
        material.SetFloat("_ShadowThreshold", shadowThreshold);
        material.SetFloat("_LightThreshold", Mathf.Max(lightThreshold, shadowThreshold));
        material.SetFloat("_MidTone", midTone);
        material.SetFloat("_Softness", softness);
        material.SetColor("_InkColor", inkColor);
        material.SetColor("_PaperColor", paperColor);
        material.SetFloat("_AccentHue", accentHue);
        material.SetFloat("_AccentHueRange", accentHueRange);
        material.SetFloat("_AccentMinSaturation", accentMinSaturation);
        material.SetFloat("_AccentFlatness", accentFlatness);
        material.SetFloat("_AccentAmount", accentAmount);
        material.SetFloat("_Invert", invert);
    }

    public void ApplyGlobal(string prefix, float invert)
    {
        Color.RGBToHSV(accentColor, out var accentHue, out _, out _);
        Shader.SetGlobalFloat(prefix + "ShadowThreshold", shadowThreshold);
        Shader.SetGlobalFloat(prefix + "LightThreshold", Mathf.Max(lightThreshold, shadowThreshold));
        Shader.SetGlobalFloat(prefix + "MidTone", midTone);
        Shader.SetGlobalFloat(prefix + "Softness", softness);
        // Unlike Material.SetColor, globals aren't converted to linear for us.
        var linear = QualitySettings.activeColorSpace == ColorSpace.Linear;
        Shader.SetGlobalColor(prefix + "InkColor", linear ? inkColor.linear : inkColor);
        Shader.SetGlobalColor(prefix + "PaperColor", linear ? paperColor.linear : paperColor);
        Shader.SetGlobalFloat(prefix + "AccentHue", accentHue);
        Shader.SetGlobalFloat(prefix + "AccentHueRange", accentHueRange);
        Shader.SetGlobalFloat(prefix + "AccentMinSaturation", accentMinSaturation);
        Shader.SetGlobalFloat(prefix + "AccentFlatness", accentFlatness);
        Shader.SetGlobalFloat(prefix + "AccentAmount", accentAmount);
        Shader.SetGlobalFloat(prefix + "Invert", invert);
    }
}
