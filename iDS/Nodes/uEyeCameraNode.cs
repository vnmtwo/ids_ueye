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
    [PluginInfo(Name = "uEyeCamera",
            Category = "Devices",
            Version = "iDs",
            AutoEvaluate = false,
            Author = "IvanRaster",
            Credits = "vnm")]

    public class uEyeCameraNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        [Input("Focus Control", IsSingle = true)]
        public Pin<uEyeCameraFocus> FInFocusControl;

        [Input("Brightness And Contrast", IsSingle = true)]
        public Pin<uEyeBrightnessAndContrastNode> FInBrightnessAndContrast;

        [Input("Sharpness", IsSingle = true)]
        public Pin<uEyeSharpnessNode> FInSharpness;

        [Input("Color", IsSingle = true)]
        public Pin<uEyeColorNode> FInColor;

        [Input("Sensor Features", IsSingle = true)]
        public Pin<uEyeSensorFeaturesNode> FInSensorFeatures1;

        //[Input("Sensor Auto Features", IsSingle = true)]
        //public Pin<uEyeSensorAutoFeatures> FInSensorFeatures;

        //[Input("Image Stabilization", IsSingle = true, DefaultBoolean = false)]
        //public IDiffSpread<bool> FInImageStabilization;

        //[Input("Hardware noise reduction", DefaultEnumEntry = "Off")]
        //public IDiffSpread<uEye.Defines.NoiseReductionMode> FInHNoiseReduction;

        //[Input("Wide dynamic range", DefaultBoolean = false)]
        //public IDiffSpread<bool> FInWDR;

        [Input("Enable Camera", IsSingle = true)]
        public IDiffSpread<bool> FInEnableCamera;

        //outputs

        [Output("Camera")]
        protected ISpread<uEyeCamera> FOutCamera;

        [Output("Capturing Photo", IsSingle =true)]
        public ISpread<bool> FOutCameraCapturing;

        [Import()]
        public ILogger FLogger;

        uEyeCamera Camera;

        //uEyeSensorAutoFeatures AutoFeatures;

        private bool FCJustConnected = false;
       // private bool AFJustConnected = false;
        private bool BCJustConnected = false;
        private bool SHJustConnected = false;
        private bool COJustConnected = false;
        private bool SFJustConnected = false;

        bool init = false;
        bool initEnum = false;

        public void Evaluate(int SpreadMax)
        {
            if (FCJustConnected && Camera!=null && Camera.Camera!=null)
            {
                ApplyFocus();
                FInFocusControl[0].Camera = Camera;
                FCJustConnected = false;
            }

            if (BCJustConnected && Camera != null && Camera.Camera != null)
            {
                FInBrightnessAndContrast[0].Camera = Camera;
                FInBrightnessAndContrast[0].ForceParameters();
                BCJustConnected = false;
            }

            if (SHJustConnected && Camera != null && Camera.Camera != null)
            {
                FInSharpness[0].Camera = Camera;
                FInSharpness[0].ForceParameters();
                SHJustConnected = false;
            }

            if (COJustConnected && Camera != null && Camera.Camera != null)
            {
                FInColor[0].Camera = Camera;
                FInColor[0].ForceParameters();
                COJustConnected = false;
            }

            //if (AFJustConnected && Camera != null && Camera.Camera != null)
            //{
            //    ApplyAutoFeatures();
            //    FInSensorFeatures[0].Camera = Camera;
            //    AFJustConnected = false;
            //}

            if (SFJustConnected && Camera !=null && Camera.Camera!=null)
            {
                FInSensorFeatures1[0].Camera = Camera;
                FInSensorFeatures1[0].ForceParameters();
                SFJustConnected = false;
            }

            if (FInEnableCamera[0] && !init)
            {
                init = true;
                if (Camera == null)
                {
                    Thread th = new Thread(() =>
                    {
                        bool s;
                        Camera = new uEyeCamera(this, out s);

                        if (s)
                        {
                            if (FInFocusControl.IsConnected)
                                FInFocusControl[0].Camera = Camera;

                            //if (FInSensorFeatures.IsConnected)
                            //    FInSensorFeatures[0].Camera = Camera;

                            if (FInBrightnessAndContrast.IsConnected)
                                FInBrightnessAndContrast[0].Camera = Camera;

                            if (FInSharpness.IsConnected)
                                FInSharpness[0].Camera = Camera;

                            if (FInColor.IsConnected)
                                FInColor[0].Camera = Camera;

                            if (FInSensorFeatures1.IsConnected)
                                FInSensorFeatures1[0].Camera = Camera;

                            ApplyFocus();
                            //ApplyAutoFeatures();
                            if (FInBrightnessAndContrast[0] != null)
                                FInBrightnessAndContrast[0].ForceParameters();
                            if (FInSharpness[0]!=null)
                                FInSharpness[0].ForceParameters();
                            if (FInColor[0] != null)
                                FInColor[0].ForceParameters();
                            if (FInSensorFeatures1[0] != null)
                                FInSensorFeatures1[0].ForceParameters();

                            //var statusRet = Camera.Camera.ImageStabilization.SetEnable(FInImageStabilization[0]);
                            //if (statusRet != uEye.Defines.Status.Success)
                            //{
                            //    FLogger.Log(LogType.Error, "Cannot set Image Stabilization");
                            //}
                            //statusRet = Camera.Camera.Device.Feature.NoiseReduction.Set(FInHNoiseReduction[0]);
                            //if (statusRet != uEye.Defines.Status.Success)
                            //{
                            //    FLogger.Log(LogType.Error, "Cannot set noise reduction");
                            //}
                            //statusRet = Camera.Camera.Device.Feature.WideDynamicRange.SetEnable(FInWDR[0]);
                            //if (statusRet != uEye.Defines.Status.Success)
                            //{
                            //    FLogger.Log(LogType.Error, "Cannot set wide dynamic range");
                            //}

                            FOutCamera.SliceCount = 1;
                            FOutCamera[0] = Camera;
                            initEnum = true;
                        }

                    });
                    th.Start();
                }
            }

            if (FInEnableCamera.IsChanged && !FInEnableCamera[0])
            {
                FOutCamera.SliceCount = 0;
                if (Camera!=null)
                    Camera.Dispose();
                Camera = null;
                FInFocusControl[0].Camera = null;
                //FInSensorFeatures[0].Camera = null;
                init = false;
            }

            if (initEnum)
            {
                int count = Camera.FormatInfoList.Count();
                var e = new string[count];

                for (int i = 0; i < count; i++)
                {
                    e[i] = Camera.FormatInfoList[i].FormatName.ToString();
                }

                int indx = Camera.FindHDResolution();

                EnumManager.UpdateEnum("uEyeTextureFormat", e[indx], e);

                initEnum = false;
            }

            //if (Camera != null && FInImageStabilization.IsChanged)
            //{
            //    if (Camera.Camera != null)
            //    {
            //        var statusRet = Camera.Camera.ImageStabilization.SetEnable(FInImageStabilization[0]);
            //        if (statusRet != uEye.Defines.Status.Success)
            //        {
            //            FLogger.Log(LogType.Error, "Cannot set Image Stabilization");
            //        }
            //    }
            //}

            //if (Camera != null && FInHNoiseReduction.IsChanged)
            //{
            //    if (Camera.Camera != null)
            //    {
            //        var statusRet = Camera.Camera.Device.Feature.NoiseReduction.Set(FInHNoiseReduction[0]);
            //        if (statusRet != uEye.Defines.Status.Success)
            //        {
            //            FLogger.Log(LogType.Error, "Cannot set noise reduction");
            //        }
            //    }
            //}

            //if (Camera != null && FInWDR.IsChanged)
            //{
            //    if (Camera.Camera != null)
            //    {
            //        var statusRet = Camera.Camera.Device.Feature.WideDynamicRange.SetEnable(FInWDR[0]);

            //        if (statusRet != uEye.Defines.Status.Success)
            //        {
            //            FLogger.Log(LogType.Error, "Cannot set wide dynamic rabge");
            //        }
            //    }
            //}
        }

        public void OnImportsSatisfied()
        {
            FOutCamera.SliceCount = 0;
            FInFocusControl.Connected += FocusControlConnected;
            //FInSensorFeatures.Connected += SensorFeaturesConnected;
            FInBrightnessAndContrast.Connected += BrightnessAndContrastConnected;
            FInSharpness.Connected += SharpnessConnected;
            FInColor.Connected += ColorConnected;
            FInSensorFeatures1.Connected += SensorFeatures1Connected;
        }

        private void SensorFeatures1Connected(object sender, PinConnectionEventArgs args)
        {
            SFJustConnected = true;
        }

        private void ColorConnected(object sender, PinConnectionEventArgs args)
        {
            COJustConnected = true;
        }

        private void SharpnessConnected(object sender, PinConnectionEventArgs args)
        {
            SHJustConnected = true;
        }

        public void Dispose()
        {
            Camera.Dispose();
        }

        private void BrightnessAndContrastConnected(object sender, PinConnectionEventArgs args)
        {
            BCJustConnected = true;
        }

        private void FocusControlConnected(object sender, PinConnectionEventArgs args)
        {
            FCJustConnected = true;
        }

        //private void SensorFeaturesConnected(object sender, PinConnectionEventArgs args)
        //{
        //    AFJustConnected = true;
        //}

        private void ApplyFocus()
        {
            if (FInFocusControl.SliceCount > 0)
            {
                if (FInFocusControl[0] != null && Camera != null)
                {
                    var t = new Thread(() =>
                    {
                        Camera.SetAutoFocusZone(FInFocusControl[0].FInAutoFocusZone[0]);
                        Camera.SetAutoFocus(FInFocusControl[0].FInAutoFocus[0]);
                        if (!FInFocusControl[0].FInAutoFocus[0])
                        {
                            Camera.SetManualFocus(FInFocusControl[0].FInManualFocus[0]);
                        }
                    });
                    t.Start();
                }
            }
        }

        //private void ApplyAutoFeatures()
        //{
        //    if (FInSensorFeatures.SliceCount > 0)
        //    {
        //        AutoFeatures = FInSensorFeatures[0];
        //        if (AutoFeatures != null && Camera != null)
        //        {
        //            var t = new Thread(() =>
        //            {
        //                Camera.SetAntiFlicker(AutoFeatures.FInAntiFlickerMode[0]);
        //                Camera.SetBackLightComp(AutoFeatures.FInBLComp[0]);
        //                Camera.SetAutoContrastCorrection(AutoFeatures.FINCC[0]);
        //                Camera.SetAutoGain(AutoFeatures.FInAutoGain[0]);
        //                Camera.SetAutoGainPhotom(AutoFeatures.FInAutoGainPhotom[0]);
        //                Camera.SetAutoShutter(AutoFeatures.FInAutoShutter[0]);
        //                Camera.SetAutoShutterPhotom(AutoFeatures.FInAutoShutterPhotom[0]);
        //                Camera.SetAutoWB(AutoFeatures.FInEnableAutoWB[0]);
        //            });
        //            t.Start();
        //        }
        //    }
        //}
    }
}