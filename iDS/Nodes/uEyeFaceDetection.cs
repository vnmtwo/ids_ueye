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
    [PluginInfo(Name = "uEyeFaceDetection",
           Category = "Devices",
           Version = "iDs",
           AutoEvaluate = true,
           Author = "IvanRaster",
           Credits = "vnm")]
    public class uEyeFaceDetectionNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("uEye Camera", IsSingle = true)]
        public ISpread<uEyeCamera> FInCamera;

        [Input("Enable Face Detect", IsSingle = true)]
        public ISpread<bool> FInEnableFaceDetect;

        [Input("Enable Face Border", IsSingle = true)]
        public ISpread<bool> FInEnableFaceBorder;

        [Input("Face Border Width", DefaultValue = 1, MinValue = 1, MaxValue = 16)]
        public IDiffSpread<int> FInFaceBorderWidth;

        [Output("Faces Count", Order = 3)]
        protected ISpread<int> FOutFacesCount;

        [Output("Faces Position", Order = 4)]
        protected ISpread<Vector2> FOutFacesPosition;

        [Output("Faces Scale", Order = 5)]
        protected ISpread<Vector2> FOutFacesScale;

        [Output("Faces Posture", Order = 6)]
        protected ISpread<int> FoutFacesPosture;

        [Import()]
        public ILogger FLogger;

        bool FaceDetectEnabled = false;
        uEye.Types.FaceDetectionInformation[] FaceInfoList;
        uEye.Types.FaceDetectionInformation Face;
        uEye.Camera Camera;

        public void Evaluate(int SpreadMax)
        {
            if (FInCamera.SliceCount > 0 && FInCamera[0] != null)
            {
                Camera = FInCamera[0].Camera;
            }


            if (Camera != null)
            {
                if (FInFaceBorderWidth.IsChanged)
                {
                    var statusRet = Camera.FaceDetection.SetOverlayLineWidth((uint)FInFaceBorderWidth[0]);
                    if (statusRet != uEye.Defines.Status.Success)
                    {
                        FLogger.Log(LogType.Error, "Setting overlay line width is failed");
                    }
                }

                if (FInEnableFaceDetect[0] && !FInCamera[0].Node.FOutCameraCapturing[0])
                {
                    if (FaceDetectEnabled)
                    {
                        Camera.FaceDetection.GetFaceList(out FaceInfoList);
                        if (FaceInfoList != null)
                        {
                            FOutFacesCount[0] = FaceInfoList.Length;
                            FOutFacesPosition.SliceCount = 0;
                            FOutFacesScale.SliceCount = 0;
                            FoutFacesPosture.SliceCount = 0;

                            for (int i = 0; i < FaceInfoList.Length; i++)
                            {
                                Face = FaceInfoList[i];
                                FOutFacesPosition.Add(new Vector2(Face.FacePosition.X, Face.FacePosition.Y));
                                FOutFacesScale.Add(new Vector2(Face.FaceSize.Width, Face.FaceSize.Height));
                                FoutFacesPosture.Add(Face.FacePosture);

                            }
                        }
                        else
                        {
                            FOutFacesCount[0] = 0;
                            FOutFacesPosition.SliceCount = 0;
                            FOutFacesScale.SliceCount = 0;
                            FoutFacesPosture.SliceCount = 0;
                        }
                    }
                    else
                    {
                        Camera.FaceDetection.SetEnable(true);
                        FaceDetectEnabled = true;
                    }
                }
                else
                {
                    if (FaceDetectEnabled)
                    {
                        Camera.FaceDetection.SetEnable(false);
                        FaceDetectEnabled = false;
                        FOutFacesCount[0] = 0;
                        FOutFacesPosition.SliceCount = 0;
                        FOutFacesScale.SliceCount = 0;
                        FoutFacesPosture.SliceCount = 0;
                    }
                }

                if (FInEnableFaceBorder[0])
                {
                    if (FaceDetectEnabled)
                    {
                        var statusRet = Camera.FaceDetection.SetMaxNumOverlay(5);
                        if (statusRet != uEye.Defines.Status.Success)
                        {
                            FLogger.Log(LogType.Error, "Face detect failed");
                        }
                    }
                }
                else
                {
                    if (FaceDetectEnabled)
                    {
                        Camera.FaceDetection.SetMaxNumOverlay(0);
                    }
                }
            }
        }

        public void OnImportsSatisfied()
        {
            FOutFacesCount[0] = 0;
            FOutFacesPosition.SliceCount = 0;
            FoutFacesPosture.SliceCount = 0;
            FOutFacesScale.SliceCount = 0;
        }
    }
}
