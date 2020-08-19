using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using PieterP.ScoreSheet.GUI.Converters;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.Shared;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.GUI.Views.Score {
    // WPF has problems rendering the many controls we need for a score sheet
    // In these classes, we try to achieve the maximum performance by keeping the visual tree to a bare minimum
    // and by caching as much as possible
    // we don't use an ItemsControl (= slow) and we don't create tons of TextBoxes (instead we use the much faster TextBlock)
    public class Matches : Panel {
        public Matches() {
            _currentSetCount = 0;
            _singleMatchWidth = MatchPanel.CalculateControlWidth(0);
            _matches = new List<MatchPanel>();
            _cache = new Dictionary<int, List<MatchPanel>>();
            _cache[5] = new List<MatchPanel>();
            this.DataContextChanged += (s, e) => OnDataContextChanged(e.NewValue);

            var foregroundBinding = new Binding("IsValid.Value");
            foregroundBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            foregroundBinding.Converter = SetPanel.ForegroundConverter;

            _left = new TextBox() {
                TextAlignment = TextAlignment.Center,
                Width = SetPanel.TextBoxWidth,
                Height = SetPanel.ControlHeight,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
            };
            var leftTextBinding = new Binding("LeftScore.Value");
            leftTextBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            _left.SetBinding(TextBox.TextProperty, leftTextBinding);
            _left.SetBinding(TextBox.ForegroundProperty, foregroundBinding);
            _left.PreviewKeyDown += Left_PreviewKeyDown;
            _left.KeyDown += SanitizeKey;

            _right = new TextBox() {
                TextAlignment = TextAlignment.Center,
                Width = SetPanel.TextBoxWidth,
                Height = SetPanel.ControlHeight,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent
            };
            var rightBinding = new Binding("RightScore.Value");
            rightBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            _right.SetBinding(TextBox.TextProperty, rightBinding);
            _right.SetBinding(TextBox.ForegroundProperty, foregroundBinding);
            _right.PreviewKeyDown += Right_PreviewKeyDown;
            _right.KeyDown += SanitizeKey;

            _activeMatch = -1;

            var tc = (Application.Current as App)?.ThemeChanged;
            if (tc != null)
                tc.ValueChanged += OnThemeChanged;
        }
        private void OnThemeChanged() {
            if (_activeMatch != -1) {
                _matches[_activeMatch][_activeSet].Replace(null, _activeLeft);
            }
            _currentSetCount = 0;
            _matches = new List<MatchPanel>();
            _cache = new Dictionary<int, List<MatchPanel>>();
            _activeMatch = -1;
            InternalChildren.Clear();
            MatchPanel.RefreshTheme();
            SetPanel.RefreshTheme();
            OnDataContextChanged(this.DataContext);
        }

        private void SanitizeKey(object sender, KeyEventArgs e) {
            // make sure it doesn't matter whether the user presses shift or not
            var kv = (int)e.Key;
            if (kv >= (int)Key.D0 && kv <= (int)Key.D9) {
                var tb = sender as TextBox;
                var lastLocation = tb!.SelectionStart;
                tb.Text = tb.Text.Insert(lastLocation, ((char)('0' + kv - (int)Key.D0)).ToString());
                tb.SelectionStart = lastLocation + 1;
                e.Handled = true;
            }
        }
        private void Right_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            TextBox_PreviewKeyDown(false, (sender as TextBox)!, e);
        }
        private void Left_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            TextBox_PreviewKeyDown(true, (sender as TextBox)!, e);
        }
        private void TextBox_PreviewKeyDown(bool left, TextBox sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Tab:
                    if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {  // move backwards
                        MoveLeft();
                    } else {  // move forwards
                        MoveRight();
                    }
                    break;
                case Key.Left:
                    if (sender.CaretIndex == 0) {
                        MoveLeft();
                    }
                    break;
                case Key.Right:
                    
                    if (sender.CaretIndex == sender.Text.Length) {
                        MoveRight();
                    }
                    break;
                case Key.Up:
                    TextBox? dest = null;
                    dest = Activate(_activeMatch - 1, _activeSet, left);
                    if (dest != null) {
                        dest.Focus();
                        e.Handled = true;
                    }
                    break;
                case Key.Down:
                    TextBox? dest2 = null;
                    dest2 = Activate(_activeMatch + 1, _activeSet, left);
                    if (dest2 != null) {
                        dest2.Focus();
                        e.Handled = true;
                    }
                    break;
            }

            void MoveRight() {
                TextBox? tb;
                if (left) {
                    tb = Activate(_activeMatch, _activeSet, false); // right textbox of the same set
                } else {
                    if (_activeSet + 1 == _currentSetCount) {
                        tb = Activate(_activeMatch + 1, 0, true); // left textbox of the first set on the next line
                    } else {
                        tb = Activate(_activeMatch, _activeSet + 1, true); // left textbox of next set
                    }
                }
                if (tb != null) {
                    tb.CaretIndex = 0;
                    tb.Focus();
                    e.Handled = true;
                }
            }
            void MoveLeft() {
                TextBox? tb;
                if (left) {
                    if (_activeSet == 0) {
                        tb = Activate(_activeMatch - 1, _currentSetCount - 1, false); // right textbox of the last set of the previous match
                    } else {
                        tb = Activate(_activeMatch, _activeSet - 1, false); // right textbox of previous set
                    }
                } else {
                    tb = Activate(_activeMatch, _activeSet, true); // left textbox of the same set
                }
                if (tb != null) {
                    tb.CaretIndex = tb.Text.Length;
                    tb.Focus();
                    e.Handled = true;
                }
            }
        }

        protected override Size MeasureOverride(Size constraint) {
            return new Size(_singleMatchWidth, MatchPanel.ControlHeight * _matches.Count);
        }
        protected override Size ArrangeOverride(Size arrangeBounds) {
            var start = DateTime.Now;

            for (int i = 0; i < _matches.Count; i++) {
                _matches[i].Arrange(new Rect(0, i * MatchPanel.ControlHeight, _singleMatchWidth, MatchPanel.ControlHeight));
            }
            var stop = DateTime.Now;
            Logger.LogDebug($"Arrange of FastMatches took { (stop - start).TotalMilliseconds }ms");
            return arrangeBounds;
        }

        private void OnDataContextChanged(object context) {
            var start = DateTime.Now;
            var vm = context as CompetitiveMatchViewModel;
            if (vm != null) {
                if (_activeMatch != -1) {
                    Logger.LogDebug($"Deactivate { (_activeLeft ? "left" : "right") } textbox of match { _activeMatch + 1}, set { _activeSet + 1 }");
                    _matches[_activeMatch][_activeSet].Replace(null, _activeLeft);
                    _activeMatch = -1;
                }

                int sets = vm.MatchSystem.SetCount * 2 - 1;
                if (sets != _currentSetCount) {
                    _singleMatchWidth = MatchPanel.CalculateControlWidth(sets);
                    foreach (var m in _matches) {
                        FreeMatch(m);
                    }
                    _matches.Clear();
                    _currentSetCount = sets;
                }
                int matchCount = vm.Matches.Count;
                // add matches if we have too few
                while (matchCount > _matches.Count) {
                    _matches.Add(AllocMatch(sets, _matches.Count));
                }
                // remove matches if we have too many
                while (matchCount < _matches.Count) {
                    var match = _matches[_matches.Count - 1];
                    _matches.RemoveAt(_matches.Count - 1);
                    FreeMatch(match);
                }
                // set the new DataContexts
                for (int i = 0; i < matchCount; i++) {
                    _matches[i].DataContext = vm.Matches[i];
                }
            }
            var stop = DateTime.Now;
        }
        private MatchPanel AllocMatch(int sets, int matchRow) {
            List<MatchPanel> list;
            if (!_cache.TryGetValue(sets, out list)) {
                list = new List<MatchPanel>();
                _cache[sets] = list;
            }
            MatchPanel match;
            if (list.Count > 0) {
                match = list[list.Count - 1];
                match.MatchRow = matchRow;
                list.RemoveAt(list.Count - 1);
            } else {
                match = new MatchPanel(matchRow, sets, Activate);
            }
            InternalChildren.Add(match);
            return match;
        }
        private void FreeMatch(MatchPanel match) {
            List<MatchPanel> list;
            if (!_cache.TryGetValue(match.SetCount, out list)) {
                list = new List<MatchPanel>();
                _cache[match.SetCount] = list;
            }
            list.Add(match);
            InternalChildren.Remove(match);
        }
        private TextBox? Activate(int matchRow, int setNumber, bool left) {
            if (matchRow < 0 || matchRow >= _matches.Count || setNumber < 0 || setNumber >= _currentSetCount)
                return null;
            
            if (_activeMatch != -1) {
                _matches[_activeMatch][_activeSet].Replace(null, _activeLeft);
            }
            _activeMatch = matchRow;
            _activeSet = setNumber;
            _activeLeft = left;
            var set = _matches[matchRow][setNumber];
            TextBox tb;
            if (left) {
                tb = _left;
            } else {
                tb = _right;
            }
            tb.DataContext = set.DataContext;
            set.Replace(tb, left);
            tb.Focus();
            return tb;
        }

        private TextBox _left;
        private TextBox _right;

        private Dictionary<int, List<MatchPanel>> _cache;
        private List<MatchPanel> _matches;
        private int _currentSetCount;
        private double _singleMatchWidth;
        private int _activeMatch, _activeSet;
        private bool _activeLeft;

        public class MatchPanel : Panel {
            static MatchPanel() {
                RefreshTheme();
            }
            public static void RefreshTheme() {
                var validBrush = Application.Current.FindResource("EditableBackgroundBrush");
                var invalidBrush = Application.Current.FindResource("ErrorBrush");
                BackgroundConverter = new BoolToObjectConverter() {
                    TrueValue = validBrush,
                    FalseValue = invalidBrush
                };
            }
            public MatchPanel(int matchRow, int setCount, Func<int, int, bool, TextBox> activator) {
                this.SetCount = setCount;
                this.MatchRow = matchRow;
                _controlWidth = CalculateControlWidth(setCount);
                var converter = new PlayersToStringConverter();
                _matchNumberText = NewTextBlock("Position", SmallTextBoxWidth);
                _playerLeftText = NewTextBlock("HomePlayers.Value", SmallTextBoxWidth, converter);
                _playerRightText = NewTextBlock("AwayPlayers.Value", SmallTextBoxWidth, converter);
                //_playerRightText.BorderThickness = new Thickness(0.5, 0.5, 1.5, 0.5);
                _sets = new Border[setCount];
                for (int i = 0; i < setCount; i++) {
                    _sets[i] = new Border() {
                        BorderThickness = new Thickness(0, 1, 1, 0),
                        BorderBrush = Brushes.Black,
                        Child = new SetPanel(this, i, activator) 
                    };
                    var backgroundBinding = new Binding("IsValid.Value");
                    backgroundBinding.Converter = BackgroundConverter;
                    _sets[i].SetBinding(TextBox.BackgroundProperty, backgroundBinding);
                    this.InternalChildren.Add(_sets[i]);
                }
                _totalSetsLeftText = NewTextBlock("HomeSets.Value", LargeTextBoxWidth);
                //_totalSetsLeftText.BorderThickness = new Thickness(0);
                _totalSetsRightText = NewTextBlock("AwaySets.Value", LargeTextBoxWidth);
                _matchesLeftText = NewTextBlock("HomeMatches.Value", LargeTextBoxWidth);
                //_matchesLeftText.BorderThickness = new Thickness(0);
                _matchesRightText = NewTextBlock("AwayMatches.Value", LargeTextBoxWidth);
                _matchesRightText.BorderThickness = new Thickness(0, 1, 0, 0);
                this.DataContextChanged += MatchPanel_DataContextChanged;
            }

            public int MatchRow { get; set; }

            private void MatchPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
                var dc = e.NewValue as MatchInfo;
                if (dc == null)
                    return;
                for (int i = 0; i < _sets.Length; i++) {
                    _sets[i].DataContext = dc.Sets[i];
                }
            }

            private Border NewTextBlock(string textBindingPath, double width, IValueConverter converter = null) {
                var textBlock = new TextBlock() {
                    Width = width - 1,
                    Height = ControlHeight - 1,
                    TextAlignment = TextAlignment.Center
                };
                var textBinding = new Binding(textBindingPath);
                textBinding.Converter = converter;
                textBlock.SetBinding(TextBlock.TextProperty, textBinding);
                var border = new Border() {
                    BorderThickness = new Thickness(0, 1, 1, 0),
                    BorderBrush = Brushes.Black
                };
                border.Child = textBlock;
                this.InternalChildren.Add(border);
                return border;
            }
            protected override Size MeasureOverride(Size constraint) {
                return new Size(_controlWidth, ControlHeight);
            }
            protected override Size ArrangeOverride(Size arrangeBounds) {
                _matchNumberText.Arrange(new Rect(0, 0, SmallTextBoxWidth, ControlHeight));
                _playerLeftText.Arrange(new Rect(SmallTextBoxWidth, 0, SmallTextBoxWidth, ControlHeight));
                _playerRightText.Arrange(new Rect(2 * SmallTextBoxWidth, 0, SmallTextBoxWidth, ControlHeight));
                for (int i = 0; i < _sets.Length; i++) {
                    _sets[i].Arrange(new Rect(3 * SmallTextBoxWidth + i * SetPanel.ControlWidth, 0, SetPanel.ControlWidth, SetPanel.ControlHeight + 1));
                }
                double offset = 3 * SmallTextBoxWidth + _sets.Length * SetPanel.ControlWidth;
                _totalSetsLeftText.Arrange(new Rect(offset, 0, LargeTextBoxWidth, ControlHeight));
                _totalSetsRightText.Arrange(new Rect(offset + LargeTextBoxWidth, 0, LargeTextBoxWidth, ControlHeight));
                _matchesLeftText.Arrange(new Rect(offset + 2 * LargeTextBoxWidth, 0, LargeTextBoxWidth, ControlHeight));
                _matchesRightText.Arrange(new Rect(offset + 3 * LargeTextBoxWidth, 0, LargeTextBoxWidth, ControlHeight));
                return arrangeBounds;
            }
            public static double CalculateControlWidth(int setCount) {
                return SmallTextBoxWidth * 3 + setCount * SetPanel.ControlWidth + LargeTextBoxWidth * 4;
            }
            public SetPanel this[int index] {
                get {
                    return (_sets[index].Child as SetPanel)!;
                }
            }

            private Border _matchNumberText;
            private Border _playerLeftText;
            private Border _playerRightText;
            private Border[] _sets;
            private Border _totalSetsLeftText;
            private Border _totalSetsRightText;
            private Border _matchesLeftText;
            private Border _matchesRightText;

            public int SetCount { get; private set; }

            private const double SmallTextBoxWidth = 20 + 1;
            private const double LargeTextBoxWidth = 28 + 1;

            private double _controlWidth;
            public const double ControlHeight = SetPanel.ControlHeight + 1 /* account for the border */;

            private static IValueConverter BackgroundConverter;
        }

        public class SetPanel : Panel {
            static SetPanel() {
                VisibilityConverter = new SetInputToHyphenConverter();
                RefreshTheme();
            }
            public static void RefreshTheme() {
                var validBrush = Application.Current.FindResource("TextBrush");
                var invalidBrush = Application.Current.FindResource("ErrorTextBrush");
                ForegroundConverter = new BoolToObjectConverter() {
                    TrueValue = validBrush,
                    FalseValue = invalidBrush
                };
            }
            public SetPanel(MatchPanel match, int setNumber, Func<int, int, bool, TextBox> activator) {
                _match = match;
                _setNumber = setNumber;
                _activator = activator;

                var foregroundBinding = new Binding("IsValid.Value");
                foregroundBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                foregroundBinding.Converter = ForegroundConverter;

                this.MouseLeftButtonDown += (sender, e) => { _activator(_match.MatchRow, _setNumber, true); e.Handled = true; }; // if the user clicks on the hyphen, activate the left textbox
                this.Cursor = Cursors.IBeam;

                _left = new TextBlock() {
                    TextAlignment = TextAlignment.Center,
                    Width = TextBoxWidth,
                    Height = ControlHeight
                };
                var leftTextBinding = new Binding("LeftScore.Value");
                leftTextBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                _left.SetBinding(TextBlock.TextProperty, leftTextBinding);
                _left.SetBinding(TextBlock.ForegroundProperty, foregroundBinding);
                this.InternalChildren.Add(_left);

                _right = new TextBlock() {
                    TextAlignment = TextAlignment.Center,
                    Width = TextBoxWidth,
                    Height = ControlHeight
                };
                var rightBinding = new Binding("RightScore.Value");
                rightBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                _right.SetBinding(TextBlock.TextProperty, rightBinding);
                _right.SetBinding(TextBlock.ForegroundProperty, foregroundBinding);
                _right.MouseLeftButtonDown += (sender, e) => { _activator(_match.MatchRow, _setNumber, false); e.Handled = true; };
                this.InternalChildren.Add(_right);

                _hyphen = new TextBlock() { Text = "-", Width = HyphenWidth, Height = ControlHeight };
                _hyphen.SetBinding(TextBlock.ForegroundProperty, foregroundBinding);

                var hyphenTextBinding = new MultiBinding();
                hyphenTextBinding.Bindings.Add(new Binding("LeftScore.Value"));
                hyphenTextBinding.Bindings.Add(new Binding("RightScore.Value"));
                hyphenTextBinding.Converter = VisibilityConverter;
                _hyphen.SetBinding(TextBlock.TextProperty, hyphenTextBinding);
                this.InternalChildren.Add(_hyphen);

                _activeLeft = _left;
                _activeRight = _right;
            }
            protected override Size MeasureOverride(Size constraint) {
                return new Size(ControlWidth, ControlHeight);
            }
            protected override Size ArrangeOverride(Size arrangeBounds) {
                double tbWidth = (arrangeBounds.Width - HyphenWidth) / 2.0;
                double cheight = arrangeBounds.Height;
                _activeLeft?.Arrange(new Rect(0, 0, tbWidth, cheight));
                _hyphen?.Arrange(new Rect(tbWidth, 0, HyphenWidth, cheight));
                _activeRight?.Arrange(new Rect(tbWidth + HyphenWidth, 0, tbWidth, cheight));
                return arrangeBounds;
            }

            public void Replace(TextBox? editor, bool left) {
                if (editor == null) {
                    if (left) {
                        InternalChildren.Remove(_activeLeft);
                        InternalChildren.Add(_left);
                        _activeLeft = _left;
                    } else {
                        InternalChildren.Remove(_activeRight);
                        InternalChildren.Add(_right);
                        _activeRight = _right;
                    }
                } else {
                    if (left && _activeLeft != editor) {
                        _activeLeft = editor;
                        InternalChildren.Remove(_left);
                        InternalChildren.Add(editor);
                    } else if (!left && _activeRight != editor) {
                        _activeRight = editor;
                        InternalChildren.Remove(_right);
                        InternalChildren.Add(editor);
                    }
                }
            }

            private FrameworkElement _activeLeft;
            private FrameworkElement _activeRight;
            private TextBlock _left;
            private TextBlock _hyphen;
            private TextBlock _right;
            private MatchPanel _match;
            private int _setNumber;
            private Func<int, int, bool, TextBox> _activator;

            public const double TextBoxWidth = 28;
            public const double HyphenWidth = 5;
            public const double ControlWidth = TextBoxWidth * 2 + HyphenWidth;
            public const double ControlHeight = 16;

            public static IValueConverter ForegroundConverter;
            public static IMultiValueConverter VisibilityConverter;
        }
    }
}