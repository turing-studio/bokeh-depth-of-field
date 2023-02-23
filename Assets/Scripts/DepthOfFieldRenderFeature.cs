using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Turing.DoF
{
    public class DepthOfFieldRenderFeature : ScriptableRendererFeature
    {
        public bool bokeh = true;
        [Range(1, 5)] public int components = 1;
        [Range(0, 5)] public int debugIdx = 0;
        [Range(1, 50)] public int radius = 20;

        private DepthOfFieldRenderPass _renderPass;

        public override void Create()
        {
            _renderPass = new DepthOfFieldRenderPass(bokeh, components, radius, debugIdx);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_renderPass);
        }
    }
}
