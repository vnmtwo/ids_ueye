//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.ComponentModel.Composition;
//using System.IO;
//using System.Diagnostics;

//using VVVV.PluginInterfaces.V1;
//using VVVV.PluginInterfaces.V2;


//using FeralTic.DX11.Resources;
//using FeralTic.DX11;

//using VVVV.DX11;

//using VVVV.Core.Logging;

//using SlimDX;
//using System.Reflection;
//using System.IO;
//using System.Threading;


//namespace iDS
//{
//    [PluginInfo(Name = "uEyeSensorAutoFeatures",
//           Category = "Devices",
//           Version = "iDs",
//           AutoEvaluate = true,
//           Author = "IvanRaster",
//           Credits = "vnm")]
//    public class uEyeSensorAutoFeatures : IPluginEvaluate, IPartImportsSatisfiedNotification
//    {
//        [Input("AntiFlicker Mode", DefaultEnumEntry = "SensorAuto")]
//        public IDiffSpread<uEye.Defines.Whitebalance.AntiFlickerMode> FInAntiFlickerMode;

//        [Input("BackLight Compensation", DefaultBoolean = false)]
//        public IDiffSpread<bool> FInBLComp;

//        [Input("Auto Contrast Correction", DefaultValue = 0, MinValue = -2, MaxValue = 2, StepSize = 0.33333333333333331)]
//        public IDiffSpread<double> FINCC;

//        [Input("Auto Gain", DefaultBoolean = true)]
//        public IDiffSpread<bool> FInAutoGain;

//        [Input("Auto Gain Photometry")]
//        public IDiffSpread<uEye.Defines.Whitebalance.GainPhotomMode> FInAutoGainPhotom;

//        [Input("Auto Shutter", DefaultBoolean = true)]
//        public IDiffSpread<bool> FInAutoShutter;

//        [Input("Auto Shutter Photometry", DefaultEnumEntry = "CenterWeighted")]
//        public IDiffSpread<uEye.Defines.Whitebalance.ShutterPhotomMode> FInAutoShutterPhotom;

//        [Input("Auto WhiteBalance", DefaultBoolean = true)]
//        public IDiffSpread<bool> FInEnableAutoWB;


//        [Output("Sensor Auto Features", Order = 1)]
//        protected Pin<uEyeSensorAutoFeatures> FOut;

//        public uEyeCamera Camera;


//        public void Evaluate(int SpreadMax)
//        {
//            if (FInAntiFlickerMode.IsChanged && Camera != null)
//            {
//                var t = new Thread(() =>
//                {
//                    Camera.SetAntiFlicker(FInAntiFlickerMode[0]);
//                });
//                t.Start();
//            }

//            if (FInBLComp.IsChanged && Camera != null)
//            {
//                var t = new Thread(() =>
//                {
//                    Camera.SetBackLightComp(FInBLComp[0]);
//                });
//                t.Start();
//            }

//            if (FINCC.IsChanged && Camera != null)
//            {
//                var t = new Thread(() =>
//                {
//                    Camera.SetAutoContrastCorrection(FINCC[0]);
//                });
//                t.Start();
//            }

//            if (FInAutoGain.IsChanged && Camera != null)
//            {
//                var t = new Thread(() =>
//                {
//                    Camera.SetAutoGain(FInAutoGain[0]);
//                });
//                t.Start();
//            }

//            if (FInAutoGainPhotom.IsChanged && Camera != null)
//            {
//                var t = new Thread(() =>
//                {
//                    Camera.SetAutoGainPhotom(FInAutoGainPhotom[0]);
//                });
//                t.Start();
//            }

//            if (FInAutoShutter.IsChanged && Camera != null)
//            {
//                var t = new Thread(() =>
//                {
//                    Camera.SetAutoShutter(FInAutoShutter[0]);
//                });
//                t.Start();
//            }

//            if (FInAutoShutterPhotom.IsChanged && Camera != null)
//            {
//                var t = new Thread(() =>
//                {
//                    Camera.SetAutoShutterPhotom(FInAutoShutterPhotom[0]);
//                });
//                t.Start();
//            }

//            if (FInEnableAutoWB.IsChanged && Camera != null)
//            {
//                var t = new Thread(() =>
//                {
//                    Camera.SetAutoWB(FInEnableAutoWB[0]);
//                });
//                t.Start();
//            }

//        }

//        public void OnImportsSatisfied()
//        {
//            FOut.SliceCount = 1;
//            FOut[0] = this;
//            FOut.Disconnected += DisconnectedFout;
//        }

//        private void DisconnectedFout(object sender, PinConnectionEventArgs args)
//        {
//            Camera = null;
//        }
//    }
//}
