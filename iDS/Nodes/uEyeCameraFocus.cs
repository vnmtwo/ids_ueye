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
    [PluginInfo(Name = "uEyeFocus",
           Category = "Devices",
           Version = "iDs",
           AutoEvaluate = true,
           Author = "IvanRaster",
           Credits = "vnm")]
    public class uEyeCameraFocus : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Trigger Auto Focus", IsSingle = true, IsBang = true)]
        public ISpread<bool> FInTriggerAuto;

        [Input("Auto Focus Zone", DefaultEnumEntry = "Center")]
        public IDiffSpread<uEye.Defines.FocusZonePreset> FInAutoFocusZone;

        [Input("Auto Focus", IsSingle = true, DefaultBoolean = true)]
        public IDiffSpread<bool> FInAutoFocus;

        [Input("Manual Focus", IsSingle = true, MinValue = 1, MaxValue = 120, StepSize = 1, DefaultValue = 60)]
        public IDiffSpread<int> FInManualFocus;

        //output

        [Output("Focus Control", Order = 1)]
        protected Pin<uEyeCameraFocus> FOut;


        public uEyeCamera Camera;

        public void Evaluate(int SpreadMax)
        {
            if (Camera != null)
            {
                if (FInTriggerAuto[0])
                {
                    var t = new Thread(() =>
                    {
                        Camera.Camera.Focus.Trigger();
                    });
                    t.Start();
                }

                if (FInAutoFocus.IsChanged)
                {
                    var t = new Thread(() =>
                    {
                        Camera.SetAutoFocus(FInAutoFocus[0]);
                        if (!FInAutoFocus[0])
                        {
                            Camera.SetManualFocus(FInManualFocus[0]);
                        }
                    });
                    t.Start();
                }

                if (FInManualFocus.IsChanged)
                {
                    var t = new Thread(() =>
                    {
                        Camera.SetManualFocus(FInManualFocus[0]);
                    });
                    t.Start();
                }

                if (FInAutoFocusZone.IsChanged)
                {
                    var t = new Thread(() =>
                    {
                        Camera.SetAutoFocusZone(FInAutoFocusZone[0]);
                    });
                    t.Start();
                }
            }
        }

        public void OnImportsSatisfied()
        {
            FOut.SliceCount = 1;
            FOut[0] = this;
            FOut.Disconnected += DisconnectedOut;
        }

        private void DisconnectedOut(object sender, PinConnectionEventArgs args)
        {
            Camera = null;
        }
    }
}
