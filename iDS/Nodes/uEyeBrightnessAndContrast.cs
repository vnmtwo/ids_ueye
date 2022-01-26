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
    public enum uEyeAutomaticMode
    {
        ControlExposureAndGain = 0,
        ControlExposure = 1,
        ControlGain = 2
    }

    [PluginInfo(Name = "uEyeBrightnessAndContrast",
           Category = "Devices",
           Version = "iDs",
           AutoEvaluate = true,
           Author = "IvanRaster",
           Credits = "vnm")]
    public class uEyeBrightnessAndContrastNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Automatic(0)/Manual(1)", IsSingle = true, IsToggle = true, DefaultBoolean = false)]
        public IDiffSpread<bool> FInAM;

        //Automatic

        [Input("Automatic Mode", IsSingle = true)]
        public IDiffSpread<uEyeAutomaticMode> FInAutomaticMode;

        [Input("Exposure Limit", DefaultValue = 100, MinValue = 0.2, MaxValue = 33.4, StepSize = 0.1, IsSingle = true)]
        public IDiffSpread<double> FInExposureLimit;

        [Input("Auto Contrast Correction", DefaultValue = 0, MinValue = -2, MaxValue = 2, StepSize = 0.33333333333333331, IsSingle = true)]
        public IDiffSpread<double> FInAutoContrastCorrection;

        //Manual

        [Input("Exposure Time", DefaultValue = 33.333, MinValue = 0.2, MaxValue = 33.4, StepSize = 0.1, IsSingle = true)]
        public IDiffSpread<double> FInExposureTime;

        [Input("Gain", DefaultValue = 0, MinValue = 0, MaxValue = 100, IsSingle =true)]
        public IDiffSpread<int> FInGain;

        [Input("Blacklevel Offset", IsSingle =true, DefaultValue =0, MinValue =0, MaxValue =255)]
        public IDiffSpread<int> FInBlackLevel;

        //Output

        [Output("Sensor Auto Features", Order = 1)]
        protected Pin<uEyeBrightnessAndContrastNode> FOut;

        public uEyeCamera Camera;


        public void Evaluate(int SpreadMax)
        {
            if (FInAM.IsChanged && Camera != null)
            {
                if (FInAM[0])
                {
                    SetManualMode();
                }
                else
                {
                    SetAutomaticMode();
                }
            }

            if (FInAutomaticMode.IsChanged && Camera != null)
            {
                SetAutomaticMode();
            }

            if (FInExposureLimit.IsChanged && Camera != null)
            {
                var t = new Thread(() =>
                {
                    Camera.SetExposureLimit(FInExposureLimit[0]);
                });
                t.Start();
            }

            if (FInAutoContrastCorrection.IsChanged && Camera != null)
            {
                var t = new Thread(() =>
                {
                    Camera.SetAutoContrastCorrection(FInAutoContrastCorrection[0]);
                });
                t.Start();
            }

            if (FInExposureTime.IsChanged && Camera != null)
            {
                var t = new Thread(() =>
                {
                    Camera.SetExposureTime(FInExposureTime[0]);
                });
                t.Start();
            }

            if (FInGain.IsChanged && Camera != null)
            {
                var t = new Thread(() =>
                {
                    Camera.SetGain(FInGain[0]);
                });
                t.Start();
            }

            if (FInBlackLevel.IsChanged && Camera != null)
            {
                var t = new Thread(() =>
                {
                    Camera.SetBlackLevel(FInBlackLevel[0]);
                });
                t.Start();
            }

        }

        internal void ForceParameters()
        {
            var t = new Thread(() =>
            {
                Camera.SetAutoContrastCorrection(FInAutoContrastCorrection[0]);
                Camera.SetGain(FInGain[0]);
                Camera.SetBlackLevel(FInBlackLevel[0]);
            });
            t.Start();

            if (FInAM[0])
            {
                SetManualMode();
            }
            else
            {
                SetAutomaticMode();
            }

        }

        private void SetManualMode()
        {
            var t = new Thread(() =>
            {
                Camera.SetControlExposure(false);
                Camera.SetControlGain(false);
                Camera.SetExposureTime(FInExposureTime[0]);
            });
            t.Start();
        }

        private void SetAutomaticMode()
        {
            if (FInAutomaticMode[0] == uEyeAutomaticMode.ControlExposureAndGain)
            {
                var t = new Thread(() =>
                {
                    Camera.SetControlExposure(true);
                    Camera.SetControlGain(true);
                    Camera.SetExposureLimit(FInExposureLimit[0]);
                });
                t.Start();
            }
            if (FInAutomaticMode[0] == uEyeAutomaticMode.ControlExposure)
            {
                var t = new Thread(() =>
                {
                    Camera.SetControlExposure(true);
                    Camera.SetControlGain(false);
                    Camera.SetExposureLimit(FInExposureLimit[0]);
                });
                t.Start();
            }
            if (FInAutomaticMode[0] == uEyeAutomaticMode.ControlGain)
            {
                var t = new Thread(() =>
                {
                    Camera.SetControlExposure(false);
                    Camera.SetControlGain(true);
                    Camera.SetExposureTime(FInExposureTime[0]);
                });
                t.Start();
            }
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
