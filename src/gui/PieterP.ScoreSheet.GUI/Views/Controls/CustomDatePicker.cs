using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace PieterP.ScoreSheet.GUI.Views.Controls {
    public class CustomDatePicker : DatePicker {
        private TextBox? _textBox;
        private Popup? _popup;

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            _textBox = GetTemplateChild("PART_TextBox") as TextBox;
            _popup = GetTemplateChild("PART_Popup") as Popup;

            if (_textBox != null) {
                _textBox.IsReadOnly = true;
                _textBox.Focusable = false;
                _textBox.PreviewMouseLeftButtonUp += TextBox_MouseLeftButtonDown;
            }
        }
        private void TextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (_popup != null && !_popup.IsOpen) {
                _popup.IsOpen = true;
            }
        }
    }
}
