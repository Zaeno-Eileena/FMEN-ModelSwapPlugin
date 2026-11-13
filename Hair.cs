using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public partial class ModelSwapTest
{
    Renderer[] femaleHairRenderers = new Renderer[0];
    Renderer[] earRenderers = new Renderer[0];
    Dictionary<string, Transform> cloneBones;
    Transform[] hairOrigBones;
    Transform hairOrigRoot;
    bool hairOrigOffscreen, hairRebound;
    Transform hairProxy;
    int proxyCount;

    Transform GetOrMakeProxy(Transform maleBone)
    {
        Transform found;
        if (cloneBones.TryGetValue(maleBone.name, out found)) return found;

        Transform parent = maleBone.parent != null ? GetOrMakeProxy(maleBone.parent) : cloneHead;
        var go = new GameObject(maleBone.name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = maleBone.localPosition;
        go.transform.localRotation = maleBone.localRotation;
        go.transform.localScale = maleBone.localScale;
        cloneBones[maleBone.name] = go.transform;
        pairs.Add(new KeyValuePair<Transform, Transform>(maleBone, go.transform)); // pose copied every frame
        proxyCount++;
        return go.transform;
    }

    void BindMaleHairToClone()
    {
        if (hairRebound || maleHair == null || cloneBones == null) return;
        hairOrigBones = maleHair.bones;
        hairOrigRoot = maleHair.rootBone;
        hairOrigOffscreen = maleHair.updateWhenOffscreen;

        // Scalable stand-in for the head joint: hair vertices attached to the head use this
        if (cloneHead != null)
        {
            hairProxy = new GameObject("hair_head_proxy").transform;
            hairProxy.SetParent(cloneHead, false);
        }

        proxyCount = 0;
        var nb = new Transform[hairOrigBones.Length];
        for (int i = 0; i < nb.Length; i++)
        {
            var src = hairOrigBones[i];
            if (src == null) continue;
            var t = GetOrMakeProxy(src);
            nb[i] = (t == cloneHead && hairProxy != null) ? hairProxy : t;
        }

        Transform root;
        maleHair.bones = nb;
        if (hairOrigRoot != null && cloneBones.TryGetValue(hairOrigRoot.name, out root)) maleHair.rootBone = root;
        maleHair.updateWhenOffscreen = true; // avoids culling glitches after rebinding
        hairRebound = true;
        Logger.LogInfo("male hair rebound: " + nb.Length + " bones, " + proxyCount + " added as stand-ins on the clone");
    }

    void UnbindMaleHair()
    {
        if (!hairRebound) return;
        if (maleHair)
        {
            maleHair.bones = hairOrigBones;
            maleHair.rootBone = hairOrigRoot;
            maleHair.updateWhenOffscreen = hairOrigOffscreen;
        }
        if (hairProxy) Destroy(hairProxy.gameObject);
        hairProxy = null;
        hairRebound = false;
    }

    void UpdateHair()
    {
        bool useMale = cfgUseMaleHair.Value;
        if (useMale) BindMaleHairToClone(); else UnbindMaleHair();
        if (hairRebound && hairProxy) hairProxy.localScale = Vector3.one * cfgHairScale.Value;
        if (maleHair) maleHair.enabled = useMale && !povNow; // hidden in POV so it can't block the camera
        foreach (var r in femaleHairRenderers) if (r) r.enabled = !useMale;
        foreach (var r in earRenderers) if (r) r.enabled = !(useMale && cfgHideEars.Value);
    }
}