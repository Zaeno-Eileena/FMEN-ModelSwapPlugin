using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;
using UnityEngine;
using BepInEx.Configuration;

public partial class ModelSwapTest
{
    // ---- settings, saved in BepInEx/config/local.modelswap.test.cfg ----
    ConfigEntry<string> cfgMaleRoot, cfgFemaleRoot, cfgKeepScripts, cfgTexProp;
    ConfigEntry<int> cfgBodySlot;
    ConfigEntry<bool> cfgAutoSwap, cfgHidePovHead;
    ConfigEntry<float> cfgPelvisOffset, cfgPelvisFwd, cfgPelvisSide, cfgPovOffset, cfgPovUp, cfgHelperWeight;
    ConfigEntry<float> cfgHue, cfgSat, cfgBright, cfgContrast;

    // --- hair adjustments
    ConfigEntry<bool> cfgUseMaleHair;
    ConfigEntry<string> cfgFemaleHair;
    ConfigEntry<bool> cfgHideEars;
    ConfigEntry<string> cfgEarName;
    ConfigEntry<float> cfgHairScale;

    bool saveDue;
    float saveAt;

    void MarkSave() { saveDue = true; saveAt = Time.unscaledTime; }

    void BindSettings()
    {
        Config.SaveOnConfigSet = false; // we save on a short delay instead
        cfgMaleRoot = Config.Bind("Setup", "MaleRootName", "male_v02", "Root object of the male actor");
        cfgFemaleRoot = Config.Bind("Setup", "FemaleRootName", "Lizet_02", "Root object of the female to clone");
        cfgBodySlot = Config.Bind("Setup", "MaleBodyMaterialSlot", 0, "Material slot of the male body (the other slot is the genital)");
        cfgKeepScripts = Config.Bind("Setup", "KeepScriptHints", "Bone,Spring,Jiggle,Physics", "Comma-separated name parts of clone scripts to keep");
        cfgTexProp = Config.Bind("Genital color", "TextureProperty", "", "Shader texture property to adjust. Empty = main texture");
        cfgFemaleHair = Config.Bind("Setup", "FemaleHairObjectName", "hair", "Name of the female clone's hair object");
        cfgUseMaleHair = Config.Bind("General", "UseMaleHair", false, "Show the male's hair on the clone instead of her own hair");
        cfgEarName = Config.Bind("Setup", "FemaleEarObjectName", "ear", "Name of the female clone's ear object");
        cfgHideEars = Config.Bind("General", "HideEarsWithMaleHair", false, "Hide the clone's ear object while the male hair is used");

        cfgAutoSwap = Config.Bind("General", "AutoSwap", true, "Swap automatically when the male actor appears");
        cfgHidePovHead = Config.Bind("General", "HideCloneHeadInPOV", true, "Hide the clone's head while the POV camera is active");
        cfgPelvisOffset = Config.Bind("General", "PelvisHeightOffset", 0f,
            new ConfigDescription("Clone pelvis offset up (+) / down (-)", new AcceptableValueRange<float>(-0.5f, 0.5f)));
        cfgPelvisFwd = Config.Bind("General", "PelvisForwardOffset", 0f,
            new ConfigDescription("Clone pelvis offset forward (+) / back (-), along the male actor's facing", new AcceptableValueRange<float>(-0.5f, 0.5f)));
        cfgPelvisSide = Config.Bind("General", "PelvisSideOffset", 0f,
            new ConfigDescription("Clone pelvis offset right (+) / left (-)", new AcceptableValueRange<float>(-0.5f, 0.5f)));
        cfgPovOffset = Config.Bind("General", "PovCameraOffset", 0f,
            new ConfigDescription("Moves the POV camera forward (+) / back (-)", new AcceptableValueRange<float>(-0.5f, 0.5f)));
        cfgPovUp = Config.Bind("General", "PovCameraUpOffset", 0f,
            new ConfigDescription("Moves the POV camera up (+) / down (-)", new AcceptableValueRange<float>(-0.5f, 0.5f)));
        cfgHelperWeight = Config.Bind("General", "HelperWeight", 0f,
            new ConfigDescription("How much thigh helper bones follow the thigh", new AcceptableValueRange<float>(0f, 1f)));
        cfgHairScale = Config.Bind("General", "MaleHairScale", 1f,
            new ConfigDescription("Scales the male hair around the clone's head joint (fixes crown clipping)", new AcceptableValueRange<float>(0.9f, 1.2f)));

        cfgHue = Config.Bind("Genital color", "Hue", 0f,
            new ConfigDescription("Hue shift in degrees", new AcceptableValueRange<float>(-180f, 180f)));
        cfgSat = Config.Bind("Genital color", "Saturation", 1f,
            new ConfigDescription("Saturation multiplier", new AcceptableValueRange<float>(0f, 2f)));
        cfgBright = Config.Bind("Genital color", "Brightness", 1f,
            new ConfigDescription("Brightness multiplier", new AcceptableValueRange<float>(0.5f, 1.5f)));
        cfgContrast = Config.Bind("Genital color", "Contrast", 1f,
            new ConfigDescription("Contrast multiplier", new AcceptableValueRange<float>(0.5f, 1.5f)));
        Config.Save();
    }
}
