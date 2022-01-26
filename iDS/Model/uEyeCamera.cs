using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeralTic.DX11.Resources;
using uEye;
using uEye.Defines;
using uEye.Defines.Whitebalance;
using uEye.Types;
using VVVV.Core.Logging;

namespace iDS
{
    public class uEyeCamera
    {
        private int bpp;
        internal Camera Camera;
        internal ImageFormatInfo[] FormatInfoList;
        private int mId;
        internal uEyeCameraNode Node;
        private int pitch;
        private IntPtr ptr;

        private Status statusRet;

        internal int TextureHeight;
        internal int TextureWidth;

        internal bool VideoPreview = false;
        internal bool CapturingPhoto = false;

        internal int PhotoFormatIndex;
        internal int VideoFormatIndex;
        private bool RestartVideo = false;
        internal bool VideoIsStarting = false;
        private int ManualFocus;
        private uEye.Defines.FocusZonePreset AutoFocusZone;
        private bool AutoFocus;
        private AntiFlickerMode AntiFlckrMode;
        private bool BackLightComp;
        private double AutoContrastCorrection;
        private ShutterPhotomMode AutoShutterShutterPhotometryMode;
        private bool AutoShutter;
        private GainPhotomMode AutoGainPhotometryMode;
        private bool AutoGain;
        private bool AutoWB;

        internal uEyeCamera(uEyeCameraNode Node, out bool success)
        {
            success = true;
            this.Node = Node;
            Camera = new uEye.Camera();

            statusRet = Camera.Init();
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Camera initializing failed");
                Camera = null;
                success = false;
                return;
            }

            statusRet = Camera.PixelFormat.Set(uEye.Defines.ColorMode.BGRA8Packed);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set color mode BGRA8");
                Camera = null;
                success = false;
                return;
            }

            statusRet = Camera.Size.ImageFormat.GetList(out FormatInfoList);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Format List getting failed");
                Camera = null;
                success = false;
                return;
            }

            statusRet = Camera.Display.Mode.Set(uEye.Defines.DisplayMode.DiB);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set display mode DiB");
                Camera = null;
                success = false;
                return;
            }

        }

        internal void Dispose()
        {
            if (Camera!=null)
            {
                VideoIsStarting = false;
                VideoPreview = false;

                Camera.Acquisition.Stop();
                statusRet = Camera.Memory.Free(mId);
                Camera.Exit();
                Camera = null;
            }
        }

        internal bool StartVideo(int formatIndex = 4)
        {
            VideoIsStarting = true;
            RestartVideo = false;

            if (formatIndex >= FormatInfoList.Count())
            {
                formatIndex = 4;
                Node.FLogger.Log(LogType.Error, "Format index out of range");
            }
            VideoFormatIndex = formatIndex;

            statusRet = Camera.Size.ImageFormat.Set((uint)FormatInfoList[formatIndex].FormatID);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set image format");
                return false;
            }

            // Allocate Memory
            statusRet = Camera.Memory.Allocate();
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Allocate Memory failed");
                return false;
            }

            // Start Live Video
            statusRet = Camera.Acquisition.Capture();
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Debug, "Acquisition Capture " + statusRet.ToString());
                return false;
            }

            Camera.Memory.GetActive(out mId);
            Camera.Memory.Inquire(mId, out TextureWidth, out TextureHeight, out bpp, out pitch);

            VideoPreview = true;
            VideoIsStarting = false;
            return true;
        }

        internal void StopVideo()
        {
            if (VideoPreview)
            {
                VideoPreview = false;

                statusRet = Camera.Acquisition.Stop();
                statusRet = Camera.Memory.Free(mId);
            }
        }

        internal bool CapturePhoto(string FilePath, int quality=80, int photoFormatIndex=0)
        {
            CapturingPhoto = true;

            PhotoFormatIndex = photoFormatIndex;

            bool reinit = true;

            if (VideoPreview)
                RestartVideo = true;
            else
                RestartVideo = false;

            if (!VideoPreview || PhotoFormatIndex != VideoFormatIndex && VideoPreview)
            {
                StopVideo();
                reinit = true;
            }
            else
            {
                reinit = false;
            }

            statusRet = Camera.Trigger.Set(uEye.Defines.TriggerMode.Software);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set trigger mode software");
                CapturingPhoto = false;
                return false;
            }

            if (reinit)
            {
                statusRet = Camera.Size.ImageFormat.Set((uint)FormatInfoList[photoFormatIndex].FormatID);
                if (statusRet != uEye.Defines.Status.Success)
                {
                    Node.FLogger.Log(LogType.Error, "Can not set image format");
                    CapturingPhoto = false;
                    return false;
                }

                statusRet = Camera.Memory.Allocate();
                if (statusRet != uEye.Defines.Status.Success)
                {
                    Node.FLogger.Log(LogType.Error, "Allocate Memory failed");
                    CapturingPhoto = false;
                    return false;
                }
            }

            statusRet = Camera.Acquisition.Freeze(uEye.Defines.DeviceParameter.Wait);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Acquisition Freeze");
                CapturingPhoto = false;
                return false;
            }


            System.Drawing.Imaging.ImageFormat ImageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
            bool ValidFormat = false;

            if (FilePath.ToLower().EndsWith(".jpg") || FilePath.ToLower().EndsWith(".jpeg")){
                ImageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                ValidFormat = true;
            }

            if (FilePath.ToLower().EndsWith(".png")){
                ImageFormat = System.Drawing.Imaging.ImageFormat.Png;
                ValidFormat = true;
            }

            if (FilePath.ToLower().EndsWith(".bmp"))
            {
                ImageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                ValidFormat = true;
            }

            if (!ValidFormat)
            {
                Node.FLogger.Log(LogType.Error, "Invalid file format");
                return false;
            }

            statusRet = Camera.Image.Save(FilePath, ImageFormat, quality);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not save Image, err: " + statusRet.ToString());
                CapturingPhoto = false;
                return false;
            }

            if (reinit)
            {
                Camera.Memory.GetActive(out mId);
                Camera.Acquisition.Stop();
                Camera.Memory.Free(mId);

                statusRet = Camera.Trigger.Set(uEye.Defines.TriggerMode.Continuous);
                if (statusRet != uEye.Defines.Status.Success)
                {
                    Node.FLogger.Log(LogType.Error, "Can not set trigger mode COntinus");
                }

                CapturingPhoto = false;
                if (RestartVideo)
                    StartVideo();
            }
            else
            {
                CapturingPhoto = false;
                statusRet = Camera.Trigger.Set(uEye.Defines.TriggerMode.Continuous);
                if (statusRet != uEye.Defines.Status.Success)
                {
                    Node.FLogger.Log(LogType.Error, "Can not set trigger mode COntinus");
                }
                if (RestartVideo)
                {
                    statusRet = Camera.Acquisition.Capture();
                    VideoPreview = true;
                }
            }
            return true;

        }



        internal void WriteTexData(DX11DynamicTexture2D t)
        {
            Camera.Memory.Lock(mId);
            Camera.Memory.ToIntPtr(mId, out ptr);
            t.WriteData(ptr, TextureWidth * TextureHeight * 4);
            Camera.Memory.Unlock(mId);

        }

        internal int FindHDResolution()
        {
            int count = FormatInfoList.Count();
            for (int i = 0; i < count; i++)
            {
                if (FormatInfoList[i].Size.Width == 1920 && FormatInfoList[i].Size.Height == 1080)
                {
                    return i;
                }
            }

            return count - 1;
        }

        internal void SetManualFocus(int v)
        {
            ManualFocus = v;
            statusRet = Camera.Focus.Manual.Set((uint)v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Manual Focus Value");
            }
        }

        internal void SetAutoFocusZone(uEye.Defines.FocusZonePreset focusZonePreset)
        {
            AutoFocusZone = focusZonePreset;
            statusRet = Camera.Focus.Zone.Preset.Set(focusZonePreset);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not Auto Focus Zone");
            }
        }

        internal void SetAutoFocus(bool v)
        {
            AutoFocus = v;
            statusRet = Camera.Focus.Auto.SetEnable(v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not Auto Focus");
            }
        }

        internal void SetControlGain(bool v)
        {
            statusRet = 0;
            statusRet = Camera.AutoFeatures.Sensor.Gain.SetEnable(v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Control Gain " + statusRet.ToString());
            }
        }

        internal void SetControlExposure(bool v)
        {
            statusRet = 0;
            statusRet = Camera.AutoFeatures.Sensor.Shutter.SetEnable(v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Control Shutter " + statusRet.ToString());
            }
        }


        internal void SetExposureLimit(double v)
        {
            statusRet = 0;
            statusRet = Camera.AutoFeatures.Software.Shutter.SetMax(v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Max Exposure " + statusRet.ToString());
            }
        }

        internal void SetExposureTime(double v)
        {
            statusRet = 0;
            statusRet = Camera.Timing.Exposure.Set(v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Exposure Time " + statusRet.ToString());
            }
        }

        internal void SetGain(int v)
        {
            statusRet = 0;
            statusRet = Camera.Gain.Hardware.Scaled.SetMaster(v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set gain " + statusRet.ToString());
            }
        }

        internal void SetBlackLevel(int v)
        {
            statusRet = 0;
            statusRet = Camera.BlackLevel.Offset.Set(v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set offset " + statusRet.ToString());
            }
        }

        internal void SetNoiseSuppression(bool p)
        {
            NoiseReductionMode m;
            m=p?NoiseReductionMode.Adaptive:NoiseReductionMode.Off;

            statusRet = 0;
            statusRet = Camera.Device.Feature.NoiseReduction.Set(m);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Noise Suppression " + statusRet.ToString());
            }
        }

        internal void SetEdgeEnhancement(int p)
        {
            statusRet = 0;
            statusRet = Camera.EdgeEnhancement.Set(p);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Edge Enhancement " + statusRet.ToString());
            }
        }

        internal void SetSharpness(int p)
        {
            statusRet = 0;
            statusRet = Camera.Sharpness.Set(p);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Sharpness " + statusRet.ToString());
            }
        }

        internal void SetSaturation(int v)
        {
            statusRet = 0;
            statusRet = Camera.Saturation.Set(v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Saturation " + statusRet.ToString());
            }
        }

        internal void SetBlueOffset(int v)
        {
            int r, b;

            Camera.AutoFeatures.Software.WhiteBalance.Offset.Get(out r, out b);

            statusRet = 0;
            statusRet = Camera.AutoFeatures.Software.WhiteBalance.Offset.Set(r, v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Blue Offset " + statusRet.ToString());
            }
        }

        internal void SetWhiteBalanceMode(WhiteBalanceMode whiteBalanceMode)
        {
            statusRet = 0;
            statusRet = Camera.AutoFeatures.Sensor.Whitebalance.SetEnable(whiteBalanceMode);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set White Balance Offset " + statusRet.ToString());
            }
        }

        internal void SetRedOffset(int v)
        {
            int r, b;

            Camera.AutoFeatures.Software.WhiteBalance.Offset.Get(out r, out b);

            statusRet = 0;
            statusRet = Camera.AutoFeatures.Software.WhiteBalance.Offset.Set(v, b);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Red Offset " + statusRet.ToString());
            }
        }

        internal void SetAntiFlicker(uEye.Defines.Whitebalance.AntiFlickerMode mode)
        {
            AntiFlckrMode = mode;
            statusRet = 0;
            statusRet = Camera.AutoFeatures.Sensor.AntiFlicker.Set(mode);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Anti Flicker " + statusRet.ToString());
            }
        }


        internal void SetWideDynamicRange(bool v)
        {
            statusRet = 0;
            statusRet = Camera.Device.Feature.WideDynamicRange.SetEnable(v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Wide Dynamic Range " + statusRet.ToString());
            }
        }

        internal void SetImageStabilization(bool v)
        {
            statusRet = 0;
            statusRet = Camera.ImageStabilization.SetEnable(v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Image Stabilization " + statusRet.ToString());
            }
        }

        internal void SetDigitalZoom(double v)
        {
            statusRet = 0;
            statusRet = Camera.Zoom.Set(v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Zoom " + statusRet.ToString());
            }
        }

        internal void SetBackLightComp(bool v)
        {
            BackLightComp = v;
            statusRet = 0;
            statusRet = Camera.AutoFeatures.Sensor.BacklightCompensation.SetEnable(v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Back light compensation " + statusRet.ToString());
            }
        }

        internal void SetAutoContrastCorrection(double v)
        {
            AutoContrastCorrection = v;
            statusRet = Camera.AutoFeatures.Sensor.Contrast.Correction.Set(v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Contrast Correction");
            }
        }

        internal void SetAutoShutterPhotom(ShutterPhotomMode shutterPhotomMode)
        {
            AutoShutterShutterPhotometryMode = shutterPhotomMode;
            statusRet = 0;
            statusRet = Camera.AutoFeatures.Sensor.Shutter.SetPhotom(shutterPhotomMode);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Auto shutter photometry " + statusRet.ToString());
            }
        }

        internal void SetAutoShutter(bool v)
        {
            AutoShutter = v;
            statusRet = Camera.AutoFeatures.Sensor.Shutter.SetEnable(v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Auto shutter");
            }
        }

        internal void SetAutoGainPhotom(GainPhotomMode gainPhotomMode)
        {
            AutoGainPhotometryMode = gainPhotomMode;
            statusRet = Camera.AutoFeatures.Sensor.Gain.SetPhotom(gainPhotomMode);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Photometry Mode");
            }
        }

        internal void SetAutoGain(bool v)
        {
            AutoGain = v;
            statusRet = Camera.AutoFeatures.Sensor.Gain.SetEnable(v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Auto Gain");
            }
        }

        internal void SetAutoWB(bool v)
        {
            AutoWB = v;
            statusRet = Camera.AutoFeatures.Sensor.Whitebalance.SetEnable(v);
            if (statusRet != uEye.Defines.Status.Success)
            {
                Node.FLogger.Log(LogType.Error, "Can not set Auto WB");
            }
        }
    }
}
