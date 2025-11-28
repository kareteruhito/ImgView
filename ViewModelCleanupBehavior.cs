using System.Xml;
using System.Xml.Schema;

using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.ComponentModel;

namespace ImgView
{
    public class ViewModelCleanupBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Closed += this.WindowClosed;
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            (this.AssociatedObject.DataContext as IDisposable)?.Dispose();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.Closed -= this.WindowClosed;
        }
    }
}
