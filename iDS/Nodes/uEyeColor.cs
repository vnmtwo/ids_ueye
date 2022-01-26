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
    [PluginInfo(Name = "uEyeColor",
           Category = "Devices",
           Version = "iDs",
           AutoEvaluate = true,
           Author = "IvanRaster",
           Credits = "vnm")]
    public class uEyeColorNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Saturation", DefaultValue = 0, MinValue = -32, MaxValue = 32, StepSize = 1, IsSingle = true)]
        public IDiffSpread<int> FInSaturation;

        [Input("Whitebalance Mode", DefaultEnumEntry = "Automatic")]
        public IDiffSpread<uEye.Defines.Whitebalance.WhiteBalanceMode> FInWhiteBalanceMode;

        [Input("Red Offset", DefaultValue = 0, MinValue = -50, MaxValue = 50, StepSize = 1, IsSingle = true)]
        public IDiffSpread<int> FInRedOffset;

        [Input("Blue Offset", DefaultValue = 0, MinValue = -50, MaxValue = 50, StepSize = 1, IsSingle = true)]
        public IDiffSpread<int> FInBlueOffset;

        //Output

        [Output("Sensor Auto Features", Order = 1)]
        protected Pin<uEyeColorNode> FOut;

        public uEyeCamera Camera;


        public void Evaluate(int SpreadMax)
        {
            if (Camera != null)
            {
                if (FInSaturation.IsChanged)
                {
                    var t = new Thread(() =>
                    {
                        Camera.SetSaturation(FInSaturation[0]);
                    });
                    t.Start();
                }

                if (FInWhiteBalanceMode.IsChanged)
                {
                    var t = new Thread(() =>
                    {
                        Camera.SetWhiteBalanceMode(FInWhiteBalanceMode[0]);
                    });
                    t.Start();
                }

                if (FInRedOffset.IsChanged)
                {
                    var t = new Thread(() =>
                    {
                        Camera.SetRedOffset(FInRedOffset[0]);
                    });
                    t.Start();
                }

                if (FInBlueOffset.IsChanged)
                {
                    var t = new Thread(() =>
                    {
                        Camera.SetBlueOffset(FInBlueOffset[0]);
                    });
                    t.Start();
                }

            }
        }

        internal void ForceParameters()
        {
            var t = new Thread(() =>
            {
                Camera.SetSaturation(FInSaturation[0]);
                Camera.SetWhiteBalanceMode(FInWhiteBalanceMode[0]);
                Camera.SetRedOffset(FInRedOffset[0]);
                Camera.SetBlueOffset(FInBlueOffset[0]);

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
