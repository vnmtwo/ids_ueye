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
using System.Threading;


namespace iDS
{
    [PluginInfo(Name = "uEyeCapturePhoto",
           Category = "Devices",
           Version = "iDs",
           AutoEvaluate = true,
           Author = "IvanRaster",
           Credits = "vnm")]
    public class uEyeCapturePhotoNode : IPluginEvaluate
    {
        [Input("uEye Camera", IsSingle = true)]
        public ISpread<uEyeCamera> FInCamera;

        [Input("Photo Format", EnumName = "uEyeTextureFormat")]
        public IDiffSpread<EnumEntry> FInPhotoFormat;

        [Input("Quality", IsSingle = true, DefaultValue = 80, MinValue = 1, MaxValue = 100)]
        public ISpread<int> FInQuality;

        [Input("File Path", IsSingle = true, StringType = StringType.Filename)]
        public ISpread<string> FInFilePath;

        [Input("Capture Photo", IsSingle = true, IsBang = true)]
        public ISpread<bool> FInCapture;

        //[Output("Last Photo Texture", Order = 0)]
        //protected Pin<DX11Resource<DX11Texture2D>> FTextureOut;

        [Output("Success", Order = 1)]
        protected ISpread<bool> FOutSuccess;

        [Import()]
        public ILogger FLogger;

        uEyeCamera Camera;
        bool SuccessFlag = false;

        public void Evaluate(int SpreadMax)
        {
            if (FInCamera.SliceCount > 0)
            {
                Camera = FInCamera[0];
                if (Camera != null)
                {
                    if (FInCapture[0])
                    {
                        if (Camera.Node.FOutCameraCapturing[0])
                        {
                            FLogger.Log(LogType.Message, "Busy");
                        }
                        else
                        {
                            Camera.Node.FOutCameraCapturing[0] = true;
                            Thread th = new Thread(() =>
                            {
                                SuccessFlag = Camera.CapturePhoto(FInFilePath[0], FInQuality[0], FInPhotoFormat[0].Index);
                                Camera.Node.FOutCameraCapturing[0] = false;
                            });

                            th.Start();

                        }
                    }
                }

                if (SuccessFlag)
                {
                    FOutSuccess[0] = true;
                    SuccessFlag = false;
                }
                else
                {
                    FOutSuccess[0] = false;
                }

            }
        }
    }
}
