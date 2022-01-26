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
    [PluginInfo(Name = "uEyeSensorFeatures",
           Category = "Devices",
           Version = "iDs",
           AutoEvaluate = true,
           Author = "IvanRaster",
           Credits = "vnm")]
    public class uEyeSensorFeaturesNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Digital Zoom", DefaultValue = 1, MinValue = 1, MaxValue = 16, StepSize = 0.05, IsSingle = true)]
        public IDiffSpread<double> FInDigitalZoom;

        [Input("Photometry", DefaultEnumEntry = "CenterWeighted")]
        public IDiffSpread<uEye.Defines.Whitebalance.ShutterPhotomMode> FInPhotometry;

        [Input("Anti Flicker Mode", DefaultEnumEntry = "SensorAuto")]
        public IDiffSpread<uEye.Defines.Whitebalance.AntiFlickerMode> FInAntiFlickerMode;

        [Input("Auto Backlight compensation", DefaultBoolean = false, IsToggle = true)]
        public IDiffSpread<bool> FInABLComp;

        [Input("Image Stabilization", DefaultBoolean = false, IsToggle = true)]
        public IDiffSpread<bool> FInImgStabilization;

        [Input("Wide Dynamic Range", DefaultBoolean = false, IsToggle = true)]
        public IDiffSpread<bool> FInWideDynamicRange;
        //Output

        [Output("Sensor Auto Features", Order = 1)]
        protected Pin<uEyeSensorFeaturesNode> FOut;

        public uEyeCamera Camera;


        public void Evaluate(int SpreadMax)
        {
            if (Camera != null)
            {
                if (FInDigitalZoom.IsChanged)
                {
                    var t = new Thread(() =>
                    {
                        Camera.SetDigitalZoom(FInDigitalZoom[0]);
                    });
                    t.Start();
                }

                if (FInPhotometry.IsChanged)
                {
                    var t = new Thread(() =>
                    {
                        Camera.SetAutoShutterPhotom(FInPhotometry[0]);
                    });
                    t.Start();
                }

                if (FInAntiFlickerMode.IsChanged)
                {
                    var t = new Thread(() =>
                    {
                        Camera.SetAntiFlicker(FInAntiFlickerMode[0]);
                    });
                    t.Start();
                }

                if (FInABLComp.IsChanged)
                {
                    var t = new Thread(() =>
                    {
                        Camera.SetBackLightComp(FInABLComp[0]);
                    });
                    t.Start();
                }

                if (FInImgStabilization.IsChanged)
                {
                    var t = new Thread(() =>
                    {
                        Camera.SetImageStabilization(FInABLComp[0]);
                    });
                    t.Start();
                }

                if (FInWideDynamicRange.IsChanged)
                {
                    var t = new Thread(() =>
                    {
                        Camera.SetWideDynamicRange(FInWideDynamicRange[0]);
                    });
                    t.Start();
                }
            }
        }

        internal void ForceParameters()
        {
            var t = new Thread(() =>
            {
                Camera.SetDigitalZoom(FInDigitalZoom[0]);
                Camera.SetAutoShutterPhotom(FInPhotometry[0]);
                Camera.SetAntiFlicker(FInAntiFlickerMode[0]);
                Camera.SetBackLightComp(FInABLComp[0]);
                Camera.SetImageStabilization(FInABLComp[0]);
                Camera.SetWideDynamicRange(FInWideDynamicRange[0]);
            });
            t.Start();
        }


        public void OnImportsSatisfied()
        {
            FOut.SliceCount = 1;
            FOut[0] = this;
            FOut.Disconnected += DisconnectedFout;
        }

        private void DisconnectedFout(object sender, PinConnectionEventArgs args)
        {
            Camera = null;
        }
    }
}
