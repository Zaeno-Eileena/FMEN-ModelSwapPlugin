using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public partial class ModelSwapTest
{
    Transform maleHead;
    Camera povCam;
    Vector3 camSavedPos;
    bool camShifted, povNow;

    // Camera offset is applied right before a camera renders and undone right after,
    // so the game's own camera code never sees it. Both pipeline types are covered.
    void OnEnable()
    {
        Camera.onPreCull += CamPre;
        Camera.onPostRender += CamPost;
        RenderPipelineManager.beginCameraRendering += SrpBegin;
        RenderPipelineManager.endCameraRendering += SrpEnd;
    }

    void OnDisable()
    {
        Camera.onPreCull -= CamPre;
        Camera.onPostRender -= CamPost;
        RenderPipelineManager.beginCameraRendering -= SrpBegin;
        RenderPipelineManager.endCameraRendering -= SrpEnd;
    }

    void SrpBegin(ScriptableRenderContext ctx, Camera cam) { CamPre(cam); }
    void SrpEnd(ScriptableRenderContext ctx, Camera cam) { CamPost(cam); }

    void CamPre(Camera cam)
    {
        if (!active || !povNow || camShifted || cam == null || cam != povCam) return;
        if (Mathf.Approximately(cfgPovOffset.Value, 0f) && Mathf.Approximately(cfgPovUp.Value, 0f)) return;
        var tr = cam.transform;
        camSavedPos = tr.position;
        tr.position = camSavedPos + tr.forward * cfgPovOffset.Value + tr.up * cfgPovUp.Value;
        camShifted = true;
    }

    void CamPost(Camera cam)
    {
        if (!camShifted || cam == null || cam != povCam) return;
        cam.transform.position = camSavedPos;
        camShifted = false;
    }

    bool IsPov()
    {
        if (maleHead == null) { povCam = null; return false; }
        var cam = maleHead.GetComponentInChildren<Camera>();
        povCam = (cam != null && cam.enabled) ? cam : null;
        return povCam != null;
    }
}

