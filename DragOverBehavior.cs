using System.Diagnostics;
using System.Xml.Serialization;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using System;

namespace ImgView
{
    public class DragOverBehavior : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.DragOver += this.DragOver;
        }

        private void DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
                return;
            }

            e.Effects = DragDropEffects.None;
            e.Handled = false;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.DragOver -= this.DragOver;
        }
    }
}