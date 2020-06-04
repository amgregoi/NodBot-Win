using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NodBot.Code.Overlay
{
    /// <summary>
    ///     Abstract overlay that uses a transparent WPF window to do its rendering.
    /// </summary>
    /// <seealso cref="Overlay.NET.OverlayPlugin" />
    public abstract class WpfOverlayPlugin : OverlayPlugin
    {
        /// <summary>
        ///     Gets or sets the overlay window.
        /// </summary>
        /// <value>
        ///     The overlay window.
        /// </value>
        public NodBotAI OverlayWindow { get; protected set; }
    }
}
