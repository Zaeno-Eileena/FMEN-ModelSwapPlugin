using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public partial class ModelSwapTest
{
    GameObject maleRoot, clone;
    SkinnedMeshRenderer maleBody, maleFace, maleHair;
    Material[] originalMats;
    Material invisibleMat;
    bool active;

    // ---------- helpers ----------
    static SkinnedMeshRenderer FindSmr(GameObject root, string name)
    {
        return root.GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault(r => r.name == name);
    }

    static Transform FindHead(GameObject root)
    {
        var all = root.GetComponentsInChildren<Transform>(true);
        return all.FirstOrDefault(t => t.name == "head" && t.parent != null && t.parent.name.StartsWith("neck"))
               ?? all.FirstOrDefault(t => t.name == "head");
    }

    // ---------- swap / restore ----------
    bool Swap(bool quiet)
    {
        var mRoot = GameObject.Find(cfgMaleRoot.Value);
        var fRoot = GameObject.Find(cfgFemaleRoot.Value);
        if (mRoot == null || fRoot == null)
        {
            if (!quiet) Logger.LogError("Male or female root not found");
            return false;
        }
        var body = FindSmr(mRoot, "erobody_male_body");
        int slot = cfgBodySlot.Value;
        if (body == null || slot < 0 || slot >= body.sharedMaterials.Length)
        {
            if (!quiet) Logger.LogError("Male body renderer or material slot not found");
            return false;
        }

        maleRoot = mRoot;
        maleBody = body;
        maleFace = FindSmr(mRoot, "erobody_male_face");
        maleHair = FindSmr(mRoot, "erobody_male_hair");
        originalMats = body.sharedMaterials;

        // Clone under an inactive holder so no Awake/OnEnable runs before we strip it
        var hints = cfgKeepScripts.Value.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
        var holder = new GameObject("swap_holder");
        holder.SetActive(false);
        clone = Instantiate(fRoot, holder.transform);

        foreach (var mb in clone.GetComponentsInChildren<MonoBehaviour>(true))
        {
            string n = mb.GetType().Name;
            bool keep = hints.Any(h => n.Contains(h));
            Logger.LogInfo("clone script: " + n + (keep ? " (kept)" : " (removed)"));
            if (!keep) Destroy(mb);
        }
        foreach (var a in clone.GetComponentsInChildren<Animator>(true)) Destroy(a);
        foreach (var a in clone.GetComponentsInChildren<Animation>(true)) Destroy(a);
        foreach (var c in clone.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        foreach (var rb in clone.GetComponentsInChildren<Rigidbody>(true)) rb.isKinematic = true;

        clone.transform.SetParent(fRoot.transform.parent, false);
        Destroy(holder);

        // Map bones by name (only bones that exist in both rigs)
        var maleBones = new Dictionary<string, Transform>();
        foreach (var t in maleRoot.GetComponentsInChildren<Transform>(true))
            if (!maleBones.ContainsKey(t.name)) maleBones[t.name] = t;

        pairs.Clear();
        var allClone = clone.GetComponentsInChildren<Transform>(true);

        cloneBones = new Dictionary<string, Transform>();
        foreach (var t in allClone)
            if (!cloneBones.ContainsKey(t.name)) cloneBones[t.name] = t;

        foreach (var t in allClone)
        {
            Transform m;
            if (maleBones.TryGetValue(t.name, out m) && m != maleRoot.transform)
                pairs.Add(new KeyValuePair<Transform, Transform>(m, t));
        }
        Logger.LogInfo("matched bones: " + pairs.Count);

        maleBones.TryGetValue(PelvisName, out malePelvis);
        femalePelvis = allClone.FirstOrDefault(t => t.name == PelvisName);

        maleHead = FindHead(maleRoot);
        cloneHead = FindHead(clone);

        var hairObj = allClone.Where(t => t.name == cfgFemaleHair.Value)
                              .FirstOrDefault(t => t.GetComponentInChildren<Renderer>(true) != null);
        femaleHairRenderers = hairObj ? hairObj.GetComponentsInChildren<Renderer>(true) : new Renderer[0];
        Logger.LogInfo("female hair renderers: " + femaleHairRenderers.Length);

        var earObj = allClone.Where(t => t.name == cfgEarName.Value)
                             .FirstOrDefault(t => t.GetComponentInChildren<Renderer>(true) != null);
        earRenderers = earObj ? earObj.GetComponentsInChildren<Renderer>(true) : new Renderer[0];

        headInitScale = cloneHead ? cloneHead.localScale : Vector3.one;

        // Thigh helper bones
        helpers.Clear();
        foreach (var t in allClone)
        {
            if (!t.name.StartsWith("thigh_helper")) continue;
            string thighName = t.name.EndsWith("_l") ? "thigh_l" : "thigh_r";
            var thigh = allClone.FirstOrDefault(x => x.name == thighName);
            if (thigh == null) continue;
            helpers.Add(new HelperLink { helper = t, thigh = thigh, helperInit = t.localRotation, thighInit = thigh.localRotation });
        }

        // Materials: invisible body slot, private copy of the genital material
        int genSlot = (slot == 0) ? 1 : 0;
        var mats = (Material[])originalMats.Clone();

        var shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("UI/Default");
        if (shader == null) Logger.LogError("No invisible shader found");
        else
        {
            invisibleMat = new Material(shader);
            invisibleMat.color = new Color(1f, 1f, 1f, 0f);
            mats[slot] = invisibleMat;
        }

        genitalMat = null; originalTex = null; srcPixels = null;
        if (genSlot < originalMats.Length && originalMats[genSlot] != null)
        {
            genitalMat = new Material(originalMats[genSlot]);
            mats[genSlot] = genitalMat;
            foreach (var pn in genitalMat.GetTexturePropertyNames())
                Logger.LogInfo("genital tex prop: " + pn + " = " + (genitalMat.GetTexture(pn) ? genitalMat.GetTexture(pn).name : "none"));
            originalTex = GetGenTex();
        }
        maleBody.sharedMaterials = mats;
        if (maleFace) maleFace.enabled = false;
        if (maleHair) maleHair.enabled = false;

        active = true;
        ApplyGenitalColor();
        return true;
    }

    void Restore()
    {
        UnbindMaleHair();
        if (clone) Destroy(clone);
        if (maleBody && originalMats != null) maleBody.sharedMaterials = originalMats;
        if (maleFace) maleFace.enabled = true;
        if (maleHair) maleHair.enabled = true;
        if (invisibleMat) Destroy(invisibleMat);
        if (genitalMat) Destroy(genitalMat);
        if (resultTex) Destroy(resultTex);
        srcPixels = null; dstPixels = null;
        active = false; povNow = false; povCam = null; camShifted = false;
    }

    void ManualToggle()
    {
        if (active) { suppressedId = maleRoot ? maleRoot.GetInstanceID() : 0; Restore(); }
        else { suppressedId = 0; Swap(false); }
    }
}
