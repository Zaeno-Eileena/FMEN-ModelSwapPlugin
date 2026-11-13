using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public partial class ModelSwapTest
{
    Material /*invisibleMat,*/ genitalMat;
    Texture originalTex;
    Texture2D resultTex;
    Color32[] srcPixels, dstPixels;
    int texW, texH;
    bool texDirty;
    float texAt;

    // ---------- genital color ----------
    Texture GetGenTex()
    {
        string p = cfgTexProp.Value;
        if (string.IsNullOrEmpty(p)) return genitalMat.mainTexture;
        return genitalMat.HasProperty(p) ? genitalMat.GetTexture(p) : null;
    }

    void SetGenTex(Texture t)
    {
        string p = cfgTexProp.Value;
        if (string.IsNullOrEmpty(p)) genitalMat.mainTexture = t;
        else if (genitalMat.HasProperty(p)) genitalMat.SetTexture(p, t);
    }

    bool ColorIsDefault()
    {
        return Mathf.Approximately(cfgHue.Value, 0f) && Mathf.Approximately(cfgSat.Value, 1f)
            && Mathf.Approximately(cfgBright.Value, 1f) && Mathf.Approximately(cfgContrast.Value, 1f);
    }

    static byte ToByte(float v) { return (byte)(Mathf.Clamp01(v) * 255f + 0.5f); }

    void ApplyGenitalColor()
    {
        if (genitalMat == null || originalTex == null) return;
        if (ColorIsDefault()) { SetGenTex(originalTex); return; }

        if (srcPixels == null)
        {
            // Copy the texture to a readable bitmap (max 2048 px) through a RenderTexture
            float scale = Mathf.Min(1f, 2048f / Mathf.Max(originalTex.width, originalTex.height));
            texW = Mathf.Max(1, Mathf.RoundToInt(originalTex.width * scale));
            texH = Mathf.Max(1, Mathf.RoundToInt(originalTex.height * scale));
            var rt = RenderTexture.GetTemporary(texW, texH, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var prev = RenderTexture.active;
            Graphics.Blit(originalTex, rt);
            RenderTexture.active = rt;
            var readable = new Texture2D(texW, texH, TextureFormat.RGBA32, false, false);
            readable.ReadPixels(new Rect(0, 0, texW, texH), 0, 0);
            readable.Apply(false);
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            srcPixels = readable.GetPixels32();
            Destroy(readable);
            dstPixels = new Color32[srcPixels.Length];
            if (resultTex) Destroy(resultTex);
            resultTex = new Texture2D(texW, texH, TextureFormat.RGBA32, true, false);
            resultTex.wrapMode = originalTex.wrapMode;
            resultTex.filterMode = originalTex.filterMode;
        }

        float h = cfgHue.Value * Mathf.Deg2Rad;
        float c = Mathf.Cos(h), s = Mathf.Sin(h);
        float m00 = 0.213f + c * 0.787f - s * 0.213f, m01 = 0.715f - c * 0.715f - s * 0.715f, m02 = 0.072f - c * 0.072f + s * 0.928f;
        float m10 = 0.213f - c * 0.213f + s * 0.143f, m11 = 0.715f + c * 0.285f + s * 0.140f, m12 = 0.072f - c * 0.072f - s * 0.283f;
        float m20 = 0.213f - c * 0.213f - s * 0.787f, m21 = 0.715f - c * 0.715f + s * 0.715f, m22 = 0.072f + c * 0.928f + s * 0.072f;
        float sat = cfgSat.Value, bri = cfgBright.Value, con = cfgContrast.Value;

        for (int i = 0; i < srcPixels.Length; i++)
        {
            var p = srcPixels[i];
            float r = p.r / 255f, g = p.g / 255f, b = p.b / 255f;
            float nr = m00 * r + m01 * g + m02 * b;
            float ng = m10 * r + m11 * g + m12 * b;
            float nb = m20 * r + m21 * g + m22 * b;
            float lum = 0.2126f * nr + 0.7152f * ng + 0.0722f * nb;
            nr = lum + (nr - lum) * sat; ng = lum + (ng - lum) * sat; nb = lum + (nb - lum) * sat;
            nr = ((nr - 0.5f) * con + 0.5f) * bri;
            ng = ((ng - 0.5f) * con + 0.5f) * bri;
            nb = ((nb - 0.5f) * con + 0.5f) * bri;
            dstPixels[i] = new Color32(ToByte(nr), ToByte(ng), ToByte(nb), p.a);
        }
        resultTex.SetPixels32(dstPixels);
        resultTex.Apply(true);
        SetGenTex(resultTex);
    }
}
