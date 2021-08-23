using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PieterP.ScoreSheet.GUI.Views.Controls {
    public class HorizontalFillPanel : Panel {
        protected override Size MeasureOverride(Size availableSize) {
            var desiredSize = new Size();
            if (this.Children.Count > 0) {
                var childSize = new Size(availableSize.Width / this.Children.Count, availableSize.Height);
                foreach (FrameworkElement child in Children) {
                    child.Measure(childSize);
                    desiredSize.Width += child.DesiredSize.Width;
                    desiredSize.Height = Math.Max(desiredSize.Height, child.DesiredSize.Height);
                }
            }
            return desiredSize;
        }
        protected override Size ArrangeOverride(Size finalSize) {
            if (this.Children.Count > 0) {
                var childWidth = finalSize.Width / this.Children.Count;
                double leftOffset = 0;
                foreach (FrameworkElement child in Children) {
                    var fw = Math.Min(childWidth, child.MaxWidth /* Infinite if not set*/ );
                    // special case for when we are using a contentcontrol
                    if (VisualTreeHelper.GetChildrenCount(child) > 0) {
                        var subChild = VisualTreeHelper.GetChild(child, 0) as FrameworkElement;
                        if (subChild != null)
                            fw = Math.Min(fw, subChild.MaxWidth /* Infinite if not set*/ );
                    }
                    child.Arrange(new Rect(leftOffset, 0, fw, finalSize.Height));
                    leftOffset += fw;
                }
            }
            return finalSize;
        }
    }
}