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
    [PluginInfo(Name = "uEyeSharpness",
           Category = "Devices",
           Version = "iDs",
           AutoEvaluate = true,
           Author = "IvanRaster",
           Credits = "vnm")]
    public class uEyeSharpnessNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Sharpness", DefaultValue = 0, MinValue = -8, MaxValue = 8, StepSize = 1, IsSingle = true)]
        public IDiffSpread<int> FInSharpness;

        [Input("Edge Enhancement", DefaultValue = 0, MinValue = 0, MaxValue = 9, StepSize = 1, IsSingle = true)]
        public IDiffSpread<int> FInEdgeEnhancement;

        [Input("Noise Suppression", IsSingle = true, DefaultBoolean =false, IsToggle =true)]
        public IDiffSpread<bool> FInNoiseSuppression;

        //Output

        [Output("Sensor Auto Features", Order = 1)]
        protected Pin<uEyeSharpnessNode> FOut;

        public uEyeCamera Camera;


        public void Evaluate(int SpreadMax)
        {
            if (Camera != null)
            {
                if (FInSharpness.IsChanged)
                {
                    var t = new Thread(() =>
                    {
                        Camera.SetSharpness(FInSharpness[0]);
                    });
                    t.Start();
                }
                if (FInEdgeEnhancement.IsChanged)
                {
                    var t = new Thread(() =>
                    {
                        Camera.SetEdgeEnhancement(FInEdgeEnhancement[0]);
                    });
                    t.Start();
                }
                if (FInNoiseSuppression.IsChanged)
                {
                    var t = new Thread(() =>
                    {
                        Camera.SetNoiseSuppression(FInNoiseSuppression[0]);
                    });
                    t.Start();
                }
            }
        }

        internal void ForceParameters()
        {
            var t = new Thread(() =>
            {
                Camera.SetSharpness(FInSharpness[0]);
                Camera.SetEdgeEnhancement(FInEdgeEnhancement[0]);
                Camera.SetNoiseSuppression(FInNoiseSuppression[0]);

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
