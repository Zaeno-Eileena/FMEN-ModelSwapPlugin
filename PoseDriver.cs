using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public partial class ModelSwapTest
{
    readonly List<KeyValuePair<Transform, Transform>> pairs = new List<KeyValuePair<Transform, Transform>>();
    class HelperLink { public Transform helper, thigh; public Quaternion helperInit, thighInit; }
    readonly List<HelperLink> helpers = new List<HelperLink>();

    Transform malePelvis, femalePelvis, cloneHead;
    Vector3 headInitScale = Vector3.one;

    void LateUpdate()
    {
        if (!active) return;
        if (maleRoot == null || clone == null) { Restore(); return; }

        clone.transform.SetPositionAndRotation(maleRoot.transform.position, maleRoot.transform.rotation);
        foreach (var p in pairs) p.Value.localRotation = p.Key.localRotation;

        foreach (var h in helpers)
        {
            var delta = h.thigh.localRotation * Quaternion.Inverse(h.thighInit);
            h.helper.localRotation = Quaternion.Slerp(Quaternion.identity, delta, cfgHelperWeight.Value) * h.helperInit;
        }

        if (malePelvis && femalePelvis)
        {
            var rt = maleRoot.transform;
            femalePelvis.position = malePelvis.position
                + Vector3.up * cfgPelvisOffset.Value
                + rt.forward * cfgPelvisFwd.Value
                + rt.right * cfgPelvisSide.Value;
        }

        if (cloneHead)
            cloneHead.localScale = (povNow && cfgHidePovHead.Value) ? Vector3.one * 0.0001f : headInitScale;

        UpdateHair();
    }
}