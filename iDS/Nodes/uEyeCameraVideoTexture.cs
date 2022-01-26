using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.IO;
using System.Diagnostics;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;


using FeralTic.DX11.Resources;
using FeralTic.DX11;

using VVVV.DX11;

using VVVV.Core.Logging;

using SlimDX;
using System.Reflection;
using System.IO;


namespace iDS
{
    [PluginInfo(Name = "uEyeCameraTexture",
           Category = "Devices",
           Version = "iDs",
           AutoEvaluate = false,
           Author = "IvanRaster",
           Credits = "vnm")]
    public class uEyeCameraVideoTexture : IPluginEvaluate, IDX11ResourceHost, IPartImportsSatisfiedNotification
    {
        [Input("uEye Camera", IsSingle = true)]
        public ISpread<uEyeCamera> FInCamera;

        [Input("Enable", IsSingle = true)]
        public ISpread<bool> FInEnable;

        [Input("Texture Format", EnumName = "uEyeTextureFormat")]
        public IDiffSpread<EnumEntry> FInTextureFormat;

        [Output("Texture", Order = 0)]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureOut;

        DX11DynamicTexture2D t;
        uEyeCamera Camera;

        public void Evaluate(int SpreadMax)
        {
            if (FInCamera.SliceCount > 0)
            { 
                Camera = FInCamera[0];

                if (FInEnable[0])
                {
                    if (Camera != null && !Camera.VideoPreview && !Camera.CapturingPhoto && !Camera.VideoIsStarting)
                    {
                        Camera.StartVideo(FInTextureFormat[0].Index);
                    }
                }
                else
                {
                    if (Camera!=null && (Camera.VideoPreview || Camera.VideoIsStarting))
                    {
                        Camera.StopVideo();
                    }
                }
            }

        }

        public void Update(DX11RenderContext context)
        {
            if (Camera != null && Camera.VideoPreview && !Camera.Node.FOutCameraCapturing[0])
            {
                if (Camera.TextureWidth > 0)
                {
                    if (!this.FTextureOut[0].Contains(context))
                    {
                        t = new DX11DynamicTexture2D(context, Camera.TextureWidth, Camera.TextureHeight, SlimDX.DXGI.Format.B8G8R8A8_UNorm);
                        this.FTextureOut[0][context] = t;
                    }
                    else
                    {
                        if (t == null || t.Width != Camera.TextureWidth || t.Height != Camera.TextureHeight)
                        {

                            this.FTextureOut[0].Dispose(context);
                            DX11DynamicTexture2D t2 = new DX11DynamicTexture2D(context, Camera.TextureWidth, Camera.TextureHeight, SlimDX.DXGI.Format.B8G8R8A8_UNorm);
                            this.FTextureOut[0][context] = t2;
                            t = t2;
                        }
                    }

                    Camera.WriteTexData(t);
                }
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            FTextureOut.SafeDisposeAll(context);
        }

        public void OnImportsSatisfied()
        {
            FTextureOut.SliceCount = 1;
            if (this.FTextureOut[0] == null) { this.FTextureOut[0] = new DX11Resource<DX11Texture2D>(); }

        }
    }
}
