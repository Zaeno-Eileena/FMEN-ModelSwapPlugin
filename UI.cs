using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BepInEx.Configuration;

public partial class ModelSwapTest
{
    bool showUi = true;
    GUIStyle titleStyle;
    GUIStyle centeredStyle;

    void Slider(string label, ConfigEntry<float> e, float min, float max, bool affectsTexture)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label + " " + e.Value.ToString("0.00"), GUILayout.Width(170));
        float v = GUILayout.HorizontalSlider(e.Value, min, max);
        GUILayout.EndHorizontal();
        if (v != e.Value)
        {
            e.Value = v;
            MarkSave();
            if (affectsTexture) { texDirty = true; texAt = Time.unscaledTime; }
        }
    }

    void ToggleRow(ConfigEntry<bool> e, string label)
    {
        bool v = GUILayout.Toggle(e.Value, label);
        if (v != e.Value) { e.Value = v; MarkSave(); }
    }

    void OnGUI()
    {
        var ev = Event.current;
        if (ev.type == EventType.KeyDown)
        {
            if (ev.keyCode == KeyCode.F9) { ManualToggle(); ev.Use(); }
            else if (ev.keyCode == KeyCode.F8) { showUi = !showUi; ev.Use(); }
        }
        if (!showUi) return;

        centeredStyle = new GUIStyle(GUI.skin.label);
        centeredStyle.alignment = TextAnchor.MiddleCenter;
        centeredStyle.fontStyle = FontStyle.Bold;
        centeredStyle.normal.textColor = Color.black;

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 16;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleLeft;

        GUILayout.BeginArea(new Rect(10, 10, 380, 560), GUI.skin.box);

        GUILayout.Label("Futanari Model Swap Plugin (v1.0)", titleStyle);
        GUILayout.Label(" (F8 to hide/show this panel)");

        GUILayout.Space(5);

        if (GUILayout.Button(active ? "Restore male (F9)" : "Swap to female (F9)")) ManualToggle();

        GUILayout.Space(3);
        GUILayout.Label("────────────────────────────────────────", centeredStyle);

        titleStyle.alignment = TextAnchor.UpperCenter;

        GUILayout.Label("General Settings", titleStyle);

        ToggleRow(cfgAutoSwap, "Auto-swap when male appears");
        ToggleRow(cfgHidePovHead, "Hide futa head in POV");
        ToggleRow(cfgUseMaleHair, "Use male hair on futa");
        ToggleRow(cfgHideEars, "Hide futa ears with male hair");

        Slider("Pelvis height", cfgPelvisOffset, -0.5f, 0.5f, false);
        Slider("Pelvis fwd/back", cfgPelvisFwd, -0.5f, 0.5f, false);
        Slider("Pelvis side", cfgPelvisSide, -0.5f, 0.5f, false);

        Slider("POV camera fwd/back", cfgPovOffset, -0.5f, 0.5f, false);
        Slider("POV camera up/down", cfgPovUp, -0.5f, 0.5f, false);

        Slider("Hair scale", cfgHairScale, 0.9f, 1.2f, false);

        GUILayout.Space(3);
        GUILayout.Label("────────────────────────────────────────", centeredStyle);

        //titleStyle.alignment = TextAnchor.MiddleCenter;
        
        GUILayout.Label("Genital Color", titleStyle);
        Slider("Hue", cfgHue, -180f, 180f, true);
        Slider("Saturation", cfgSat, 0f, 2f, true);
        Slider("Brightness", cfgBright, 0.5f, 1.5f, true);
        Slider("Contrast", cfgContrast, 0.5f, 1.5f, true);
        if (GUILayout.Button("Reset color"))
        {
            cfgHue.Value = 0f; cfgSat.Value = 1f; cfgBright.Value = 1f; cfgContrast.Value = 1f;
            MarkSave(); texDirty = true; texAt = Time.unscaledTime;
        }
        GUILayout.EndArea();
    }
}