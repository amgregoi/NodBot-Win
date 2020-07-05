using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using NodBot.Code.Enums;
using NodBot.Code.Model;
using NodBot.Code.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;

namespace NodBot.Code
{
    public class SeqGardening : SeqBase
    {
        public SeqGardening(CancellationTokenSource ct, Logger aLogger) : base(ct, aLogger) { }


    }
}