﻿using System;
using System.Windows.Forms;

namespace Pos.Internals.Extensions
{
    public class HotKeysEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HotKeysEventArgs"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="modifiers">The modifiers.</param>
        public HotKeysEventArgs(Keys key, HotKeyModifiers modifiers)
        {
            this.Key = key;
            this.Modifiers = modifiers;
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public Keys Key { get; set; }

        /// <summary>
        /// Gets or sets the modifiers.
        /// </summary>
        /// <value>
        /// The modifiers.
        /// </value>
        public HotKeyModifiers Modifiers { get; set; }
    }
}