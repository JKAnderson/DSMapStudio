﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;

namespace StudioCore.Scene
{
    /// <summary>
    /// The "renderer" for a scene. This pipeline is instantiated for every real or virtual viewport, and will
    /// render a scene into an internally maintained framebuffer.
    /// </summary>
    public class SceneRenderPipeline
    {
        private RenderScene Scene;

        //public DeviceBuffer ProjectionMatrixBuffer { get; private set; }
        //public DeviceBuffer ViewMatrixBuffer { get; private set; }
        //public DeviceBuffer EyePositionBuffer { get; private set; }

        public SceneParam SceneParams;
        public DeviceBuffer SceneParamBuffer { get; private set; }

        public ResourceSet ProjViewRS { get; private set; }

        public Vector3 Eye { get; private set; }

        private Renderer.RenderQueue RenderQueue;

        public float CPURenderTime { get => RenderQueue.CPURenderTime; }

        public uint EnvMapTexture = 3;

        public unsafe SceneRenderPipeline(RenderScene scene, GraphicsDevice device, int width, int height)
        {
            Scene = scene;

            var factory = device.ResourceFactory;
            //ProjectionMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            //ViewMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            //EyePositionBuffer = factory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            //Matrix4x4 proj = Matrix4x4.CreatePerspective(width, height, 0.1f, 100.0f);
            SceneParamBuffer = factory.CreateBuffer(new BufferDescription((uint)sizeof(SceneParam), BufferUsage.UniformBuffer));
            SceneParams = new SceneParam();
            SceneParams.Projection = Utils.CreatePerspective(device, true, 60.0f * (float)Math.PI / 180.0f, (float)width / (float)height, 0.1f, 2000.0f);
            SceneParams.View = Matrix4x4.CreateLookAt(new Vector3(0.0f, 2.0f, 0.0f), new Vector3(1.0f, 2.0f, 0.0f), Vector3.UnitY);
            SceneParams.EyePosition = new Vector4(0.0f, 2.0f, 0.0f, 0.0f);
            SceneParams.LightDirection = new Vector4(1.0f, -0.5f, 0.0f, 0.0f);
            SceneParams.EnvMap = EnvMapTexture;

            SceneParams.AmbientLightMult = 1.0f;
            SceneParams.DirectLightMult = 1.0f;
            SceneParams.IndirectLightMult = 1.0f;
            SceneParams.EmissiveMapMult = 1.0f;
            SceneParams.SceneBrightness = 1.0f;

            device.UpdateBuffer(SceneParamBuffer, 0, ref SceneParams, (uint)sizeof(SceneParam));
            ResourceLayout sceneParamLayout = StaticResourceCache.GetResourceLayout(
                device.ResourceFactory,
                StaticResourceCache.SceneParamLayoutDescription);
            ProjViewRS = StaticResourceCache.GetResourceSet(device.ResourceFactory, new ResourceSetDescription(sceneParamLayout,
                SceneParamBuffer));

            RenderQueue = new Renderer.RenderQueue("Viewport Render", device, this);
            Renderer.RegisterRenderQueue(RenderQueue);
        }

        public void SetViewportSetupAction(Action<GraphicsDevice, CommandList> action)
        {
            RenderQueue.SetPredrawSetupAction(action);
        }

        public unsafe void TestUpdateView(Matrix4x4 proj, Matrix4x4 view, Vector3 eye)
        {
            //cl.UpdateBuffer(ViewMatrixBuffer, 0, ref view, 64);
            Eye = eye;
            Renderer.AddBackgroundUploadTask((d, cl) =>
            {
                SceneParams.Projection = proj;
                SceneParams.View = view;
                SceneParams.EyePosition = new Vector4(eye, 0.0f);
                SceneParams.EnvMap = EnvMapTexture;
                cl.UpdateBuffer(SceneParamBuffer, 0, ref SceneParams, (uint)sizeof(SceneParam));
            });
        }

        public void BindResources(CommandList cl)
        {
            cl.SetGraphicsResourceSet(0, ProjViewRS);
        }

        public void RenderScene(BoundingFrustum frustum)
        {
            Scene.Render(RenderQueue, frustum, this);
        }
    }
}
