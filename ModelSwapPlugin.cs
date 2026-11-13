using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Rendering;

[BepInPlugin("com.github.zaeno-eileena.fmen.modelswapplugin", "FMEN-ModelSwapPlugin", "1.0.0")]
public partial class ModelSwapTest : BaseUnityPlugin
{
    const string PelvisName = "pelvis";

    float nextPoll;
    int suppressedId;

    void Awake()
    {
        BindSettings();
    }

    // ---------- per-frame logic ----------
    void Poll()
    {
        if (!active)
        {
            if (!cfgAutoSwap.Value) return;
            var m = GameObject.Find(cfgMaleRoot.Value);
            if (m == null) { suppressedId = 0; return; }
            if (m.GetInstanceID() != suppressedId) Swap(true);
        }
        else
        {
            bool pov = IsPov();
            if (pov != povNow) { povNow = pov; Logger.LogInfo("POV camera: " + pov); }
        }
    }

    void Update()
    {
        float t = Time.unscaledTime;
        if (saveDue && t - saveAt > 1f) { Config.Save(); saveDue = false; }
        if (texDirty && t - texAt > 0.35f) { texDirty = false; if (active) ApplyGenitalColor(); }
        if (t >= nextPoll) { nextPoll = t + 0.25f; Poll(); }
    }

    void OnApplicationQuit() { if (saveDue) Config.Save(); }
}