using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Turing.DoF
{
    public class DepthOfFieldRenderPass : ScriptableRenderPass
    {
        private readonly Material _material;
        private RenderTextureDescriptor _sourceDesc;
        private readonly bool _bokeh;
        private readonly int _components;
        private readonly int _radius;
        private readonly int _debugIdx;
        private static readonly int BlendParams = Shader.PropertyToID("_BlendParams");
        private static readonly int Params = Shader.PropertyToID("_Params");
        private static readonly int BlurParams = Shader.PropertyToID("_BlurParams");

        private static readonly List<Vector4>[] KernelParams =
        {
            new List<Vector4>
            {
                new Vector4(0.862325f, 1.624835f, 0.767583f, 1.862321f)
            },
            new List<Vector4>
            {
                new Vector4(0.886528f, 5.268909f, 0.411259f, -0.548794f),
                new Vector4(1.960518f, 1.558213f, 0.513282f, 4.56111f),
            },
            new List<Vector4>
            {
                new Vector4(2.17649f, 5.043495f, 1.621035f, -2.105439f),
                new Vector4(1.019306f, 9.027613f, -0.28086f, -0.162882f),
                new Vector4(2.81511f, 1.597273f, -0.366471f, 10.300301f)
            },
            new List<Vector4>
            {
                new Vector4(4.338459f, 1.553635f, -5.767909f, 46.164397f),
                new Vector4(3.839993f, 4.693183f, 9.795391f, -15.227561f),
                new Vector4(2.791880f, 8.178137f, -3.048324f, 0.302959f),
                new Vector4(1.342190f, 12.328289f, 0.010001f, 0.244650f),
            },
            new List<Vector4>
            {
                new Vector4(4.892608f, 1.685979f, -22.356787f, 85.91246f),
                new Vector4(4.71187f, 4.998496f, 35.918936f, -28.875618f),
                new Vector4(4.052795f, 8.244168f, -13.212253f, -1.578428f),
                new Vector4(2.929212f, 11.900859f, 0.507991f, 1.816328f),
                new Vector4(1.512961f, 16.116382f, 0.138051f, -0.01f),
            }
        };

        public DepthOfFieldRenderPass(bool bokeh, int components, int radius, int debugIdx)
        {
            _bokeh = bokeh;
            _components = Mathf.Clamp(components, 1, KernelParams.Length);
            _radius = radius;
            _debugIdx = debugIdx;
            var shader = Shader.Find("Hidden/Turing/DepthOfField");
            _material = CoreUtils.CreateEngineMaterial(shader);
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            _sourceDesc = cameraTextureDescriptor;
            _sourceDesc.colorFormat = RenderTextureFormat.ARGBFloat;
        }

        private Vector2 ComplexVector(float a, float b, float x)
        {
            return Mathf.Exp(-a * x * x) * new Vector2(Mathf.Cos(b * x * x), Mathf.Sin(b * x * x));
        }

        private float NormalizationFactorBokeh(List<Vector4> kernelParams)
        {
            var range = 2 * _radius + 1;
            float total = 0;
            foreach (var kp in kernelParams)
            {
                for (int i = 0; i < range; ++i)
                for (int j = 0; j < range; ++j)
                {
                    var za = ComplexVector(kp.x, kp.y, 1.2f * (i - _radius) / _radius);
                    var zb = ComplexVector(kp.x, kp.y, 1.2f * (j - _radius) / _radius);
                    total += kp.z * (za.x * zb.x - za.y * zb.y) + kp.w * (za.x * zb.y + za.y * zb.x);
                }
            }

            return 1 / Mathf.Sqrt(total);
        }

        private float NormalizationFactorGauss(float a)
        {
            var range = 2 * _radius + 1;
            float total = 0;
            for (int i = 0; i < range; ++i)
            for (int j = 0; j < range; ++j)
            {
                var x = (i - _radius) / _radius;
                var y = (j - _radius) / _radius;

                total += Mathf.Exp(-a * (x * x + y * y));
            }

            return 1 / Mathf.Sqrt(total);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game)
                return;

            if (_bokeh)
                Bokeh(context, ref renderingData);
            else
                Gauss(context, ref renderingData);
        }

        private void Bokeh(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("DepthOfField");

            var texTemp1 = Shader.PropertyToID("_TexTemp1");
            var texTemp2 = Shader.PropertyToID("_TexTemp2");
            var texReal = Shader.PropertyToID("_TexReal");
            var texImag = Shader.PropertyToID("_TexImag");
            var texOutput = Shader.PropertyToID("_TexOutput");
            var cameraTex = renderingData.cameraData.renderer.cameraColorTarget;

            var kernelParams = KernelParams[_components - 1];

            cmd.GetTemporaryRT(texTemp1, _sourceDesc);
            cmd.GetTemporaryRT(texTemp2, _sourceDesc);
            cmd.GetTemporaryRT(texReal, _sourceDesc);
            cmd.GetTemporaryRT(texImag, _sourceDesc);
            cmd.GetTemporaryRT(texOutput, _sourceDesc);

            cmd.SetRenderTarget(texOutput);
            cmd.ClearRenderTarget(true, true, Color.black);

            cmd.SetGlobalVector(BlurParams, new Vector4(_radius, NormalizationFactorBokeh(kernelParams)));

            var resTextures = new List<int>();

            for (int i = 0; i < kernelParams.Count; ++i)
            {
                var kp = kernelParams[i];
                var texRes = Shader.PropertyToID("_TexRes" + i);
                resTextures.Add(texRes);
                cmd.GetTemporaryRT(texRes, _sourceDesc);

                cmd.SetGlobalVector(Params, new Vector4(kp.x, kp.y, 1, 0)); // horizontal real
                cmd.Blit(cameraTex, texTemp1, _material, 1);
                cmd.SetGlobalVector(Params, new Vector4(kp.x, kp.y, 0, 1)); // horizontal imag
                cmd.Blit(cameraTex, texTemp2, _material, 1);

                cmd.SetGlobalTexture("_TexReal", texTemp1);
                cmd.SetGlobalTexture("_TexImag", texTemp2);
                cmd.SetGlobalVector(Params, new Vector4(kp.x, kp.y, 1, 0)); // vertical real
                cmd.Blit(cameraTex, texReal, _material, 2);
                cmd.SetGlobalVector(Params, new Vector4(kp.x, kp.y, 0, 1)); // vertical imaginary
                cmd.Blit(cameraTex, texImag, _material, 2);

                // blend
                cmd.SetGlobalTexture("_TexOther", texImag);
                cmd.SetGlobalVector(Params, new Vector4(kp.z, kp.w));
                cmd.Blit(texReal, texRes, _material, 0);
            }

            if (_debugIdx > 0)
            {
                var texRes = resTextures[Mathf.Clamp(_debugIdx - 1, 0, resTextures.Count - 1)];
                cmd.Blit(texRes, cameraTex);
            }
            else
            {
                foreach (var texRes in resTextures)
                {
                    cmd.SetGlobalTexture("_TexOther", texRes);
                    cmd.Blit(texOutput, texTemp1);
                    cmd.SetGlobalVector(Params, new Vector4(1, 1));
                    cmd.Blit(texTemp1, texOutput, _material, 0);
                }

                cmd.Blit(texOutput, cameraTex);
            }

            resTextures.ForEach(x => cmd.ReleaseTemporaryRT(x));

            cmd.ReleaseTemporaryRT(texTemp1);
            cmd.ReleaseTemporaryRT(texTemp2);
            cmd.ReleaseTemporaryRT(texReal);
            cmd.ReleaseTemporaryRT(texReal);
            cmd.ReleaseTemporaryRT(texOutput);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void Gauss(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("DepthOfField");

            var texTemp = Shader.PropertyToID("_TexTemp");
            var cameraTex = renderingData.cameraData.renderer.cameraColorTarget;

            cmd.GetTemporaryRT(texTemp, _sourceDesc);

            float gaussParam = 2f;
            cmd.SetGlobalVector(BlurParams, new Vector4(_radius, NormalizationFactorGauss(gaussParam)));

            cmd.SetGlobalVector(Params, new Vector4(gaussParam, 0, 1, 0));
            cmd.Blit(cameraTex, texTemp, _material, 3);
            cmd.SetGlobalVector(Params, new Vector4(gaussParam, 0, 0, 1));
            cmd.Blit(texTemp, cameraTex, _material, 3);

            cmd.ReleaseTemporaryRT(texTemp);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
