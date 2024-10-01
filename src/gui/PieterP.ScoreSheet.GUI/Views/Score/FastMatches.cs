using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using PieterP.ScoreSheet.GUI.Converters;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.ScoreSheet.ViewModels.Score.Optimizations;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.GUI.Views.Score {
    // WPF has problems rendering the many controls we need for a score sheet
    // In these classes, we try to achieve the maximum performance by keeping the visual tree to a bare minimum
    // and by caching as much as possible
    // we don't use an ItemsControl (= slow) and we don't create tons of TextBoxes (instead we use the much faster TextBlock)
    public class FastMatches : Panel {
        public FastMatches() {
            _currentSetCount = 0;
            _singleMatchWidth = MatchPanel.CalculateControlWidth(0);
            _matches = new List<MatchPanel>();
            _cache = new Dictionary<int, List<MatchPanel>>();
            _cache[5] = new List<MatchPanel>();
            this.DataContextChanged += (s, e) => OnDataContextChanged(e.NewValue);

            var foregroundBinding = new Binding("IsValid.Value");
            foregroundBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            foregroundBinding.Converter = SetPanel.ForegroundConverter;

            var left = new TextBox() {
                TextAlignment = TextAlignment.Center,
                Width = SetPanel.TextBoxWidth,
                Height = SetPanel.ControlHeight,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
            };
            var leftTextBinding = new Binding("Contents.Value");
            leftTextBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            left.SetBinding(TextBox.TextProperty, leftTextBinding);
            left.SetBinding(TextBox.ForegroundProperty, foregroundBinding);
            left.PreviewKeyDown += Left_PreviewKeyDown;
            left.KeyDown += SanitizeKey;
            _left = new TextBoxContainer(left);
            _left.TextBox.DataContext = _left;

            var right = new TextBox() {
                TextAlignment = TextAlignment.Center,
                Width = SetPanel.TextBoxWidth,
                Height = SetPanel.ControlHeight,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent
            };
            var rightBinding = new Binding("Contents.Value");
            rightBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            right.SetBinding(TextBox.TextProperty, rightBinding);
            right.SetBinding(TextBox.ForegroundProperty, foregroundBinding);
            right.PreviewKeyDown += Right_PreviewKeyDown;
            right.KeyDown += SanitizeKey;
            _right = new TextBoxContainer(right);
            _right.TextBox.DataContext = _right;

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
                case Key.Subtract:
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
                case Key.Back:
                    if (sender.Text.Length == 0) {
                        MoveLeft();
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
            for (int i = 0; i < _matches.Count; i++) {
                _matches[i].Arrange(new Rect(0, i * MatchPanel.ControlHeight, _singleMatchWidth, MatchPanel.ControlHeight));
            }
            return arrangeBounds;
        }

        private void OnDataContextChanged(object context) {
            var vm = context as CompetitiveMatchViewModel;
            if (vm != null) {
                if (_activeMatch != -1) {
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
                //// set the new DataContexts
                //for (int i = 0; i < matchCount; i++) {
                //    _matches[i].DataContext = vm.Matches[i];
                //}
                for (int i = 0; i < matchCount; i++) {
                    _matches[i].ChangeTarget(vm.Matches[i]);
                }
            }
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
                tb = _left.TextBox;
                _left.Contents.ChangeTarget(set.LeftScore);
                _left.IsValid.ChangeTarget(set.IsValid);
            } else {
                tb = _right.TextBox;
                _right.Contents.ChangeTarget(set.RightScore);
                _right.IsValid.ChangeTarget(set.IsValid);
            }
            set.Replace(tb, left);
            if (left) {
                _left.Reevaluate(false, true);
            } else {
                _right.Reevaluate(false, true);
            }
            tb.Focus();
            return tb;
        }

        private TextBoxContainer _left;
        private TextBoxContainer _right;

        private Dictionary<int, List<MatchPanel>> _cache;
        private List<MatchPanel> _matches;
        private int _currentSetCount;
        private double _singleMatchWidth;
        private int _activeMatch, _activeSet;
        private bool _activeLeft;

        private class TextBoxContainer {
            public TextBoxContainer(TextBox tb) {
                this.TextBox = tb;
                this.Contents = new RelayCell<string>();
                this.IsValid = new RelayCell<bool>();

                tb.GotFocus += (s, e) => Reevaluate(false);
                tb.LostFocus += (s, e) => Reevaluate(true);
                tb.KeyUp += (s, e) => Reevaluate(false);
                this.IsValid.ValueChanged += () => Reevaluate(false);

            }
            public TextBox TextBox { get; set; }
            public RelayCell<string> Contents { get; set; }
            public RelayCell<bool> IsValid { get; set; }

            public void Reevaluate(bool lostFocus, bool force = false) {
                bool valid = this.IsValid.Value;
                if (!valid) {
                    valid = !this.Contents.Value.Any(c => !char.IsDigit(c) && c != ' ');
                }
                if (lostFocus || valid)
                    PopupManager.HidePopup(TextBox);
                else
                    PopupManager.ShowPopup(TextBox, force);
            }
        }

        public static class PopupManager {
            public static void ShowPopup(UIElement owner, bool force) {
                if (!force && _currentOwner == owner && _activePopup != null && _activePopup.IsOpen)
                    return;
                if (_currentOwner != null)
                    HidePopup(_currentOwner);

                _activePopup = new SuspectSemanticsPopup();
                _activePopup.Placement = PlacementMode.Bottom;
                _activePopup.StaysOpen = true;
                _activePopup.PopupAnimation = PopupAnimation.None;
                _activePopup.AllowsTransparency = false;
                _activePopup.PlacementTarget = owner;
                _activePopup.IsOpen = true;
                _currentOwner = owner;
            }
            public static void HidePopup(UIElement owner) {
                if (_currentOwner == owner && _activePopup != null) {
                    _activePopup.IsOpen = false;
                    _activePopup = null;
                    _currentOwner = null;
                }
            }
            private static Popup? _activePopup;
            private static UIElement? _currentOwner;
        }

        public class MatchPanel : Panel {
            static MatchPanel() {
                RefreshTheme();
                PlayerConverter = new PlayersToStringConverter();
            }
            public static void RefreshTheme() {
                var validBrush = Application.Current.FindResource("EditableBackgroundBrush");
                var invalidBrush = Application.Current.FindResource("ErrorBrush");
                BackgroundConverter = new BoolToObjectConverter() {
                    TrueValue = validBrush,
                    FalseValue = invalidBrush
                };
            }
            public MatchPanel(int matchRow, int setCount, Func<int, int, bool, TextBox?> activator) {
                this.SetCount = setCount;
                this.MatchRow = matchRow;
                _controlWidth = CalculateControlWidth(setCount);
                _matchNumberText = NewTextBlock(SmallTextBoxWidth);
                _playerLeftText = NewTextBlock(SmallTextBoxWidth);
                _playerRightText = NewTextBlock(SmallTextBoxWidth);

                
                _sets = new (Border, SetPanel)[setCount];
                for (int i = 0; i < setCount; i++) {
                    _sets[i].Panel = new SetPanel(this, i, activator);
                    _sets[i].Control = new Border() {
                        BorderThickness = new Thickness(0, 1, 1, 0),
                        BorderBrush = Brushes.Black,
                        Child = _sets[i].Panel
                    };
                    var backgroundBinding = new Binding("IsValid.Value");
                    backgroundBinding.Converter = BackgroundConverter;
                    _sets[i].Control.SetBinding(TextBox.BackgroundProperty, backgroundBinding);
                    _sets[i].Control.DataContext = _sets[i].Panel;
                    this.InternalChildren.Add(_sets[i].Control);
                }


                _totalSetsLeftText = NewRelayTextBlock(LargeTextBoxWidth);
                //_totalSetsLeftText.BorderThickness = new Thickness(0);
                _totalSetsRightText = NewRelayTextBlock(LargeTextBoxWidth);
                _matchesLeftText = NewRelayTextBlock(LargeTextBoxWidth);
                //_matchesLeftText.BorderThickness = new Thickness(0);
                _matchesRightText = NewRelayTextBlock(LargeTextBoxWidth);
                _matchesRightText.Control.BorderThickness = new Thickness(0, 1, 0, 0);
            }

            private MatchInfo? _prevMatchInfo = null;

            private void UpdatePlayers(object? sender, PropertyChangedEventArgs? e) {
                if (_prevMatchInfo != null) {
                    _playerLeftText.Value.Value = (string)PlayerConverter.Convert(_prevMatchInfo.HomePlayers.Value, null, null, null);
                    _playerRightText.Value.Value = (string)PlayerConverter.Convert(_prevMatchInfo.AwayPlayers.Value, null, null, null);
                }
            }

            public void ChangeTarget(MatchInfo mi) {
                _matchNumberText.Value.Value = mi.Position.ToString();
                //_playerLeftText.Value.Value = (string)PlayerConverter.Convert(mi.HomePlayers.Value, null, null, null);
                //_playerRightText.Value.Value = (string)PlayerConverter.Convert(mi.AwayPlayers.Value, null, null, null);
                if (_prevMatchInfo != null) {
                    ((INotifyPropertyChanged)_prevMatchInfo.HomePlayers).PropertyChanged -= UpdatePlayers;
                    ((INotifyPropertyChanged)_prevMatchInfo.AwayPlayers).PropertyChanged -= UpdatePlayers;
                }
                ((INotifyPropertyChanged)mi.HomePlayers).PropertyChanged += UpdatePlayers;
                ((INotifyPropertyChanged)mi.AwayPlayers).PropertyChanged += UpdatePlayers;
                _prevMatchInfo = mi;
                UpdatePlayers(null, null);

                for (int i = 0; i < _sets.Length; i++) {
                    _sets[i].Panel.LeftScore.ChangeTarget(mi.Sets[i].LeftScore);
                    _sets[i].Panel.RightScore.ChangeTarget(mi.Sets[i].RightScore);
                    _sets[i].Panel.IsValid.ChangeTarget(mi.Sets[i].IsValid);
                }
                _totalSetsLeftText.Value.ChangeTarget(mi.HomeSets);
                _totalSetsRightText.Value.ChangeTarget(mi.AwaySets);
                _matchesLeftText.Value.ChangeTarget(mi.HomeMatches);
                _matchesRightText.Value.ChangeTarget(mi.AwayMatches);
            }
            private (Border, RelayCell<string>) NewRelayTextBlock(double width) {
                var textBlock = new TextBlock() {
                    Width = width - 1,
                    Height = ControlHeight - 1,
                    TextAlignment = TextAlignment.Center
                };
                var textBinding = new Binding("Value");
                textBlock.SetBinding(TextBlock.TextProperty, textBinding);
                var cell = new RelayCell<string>();
                textBlock.DataContext = cell; 
                var border = new Border() {
                    BorderThickness = new Thickness(0, 1, 1, 0),
                    BorderBrush = Brushes.Black
                };
                border.Child = textBlock;
                this.InternalChildren.Add(border);
                return (border, cell);
            }
            private (Border, Cell<string>) NewTextBlock(double width) {
                var textBlock = new TextBlock() {
                    Width = width - 1,
                    Height = ControlHeight - 1,
                    TextAlignment = TextAlignment.Center
                };
                var textBinding = new Binding("Value");
                textBlock.SetBinding(TextBlock.TextProperty, textBinding);
                var cell = Cell.Create<string>();
                textBlock.DataContext = cell;
                var border = new Border() {
                    BorderThickness = new Thickness(0, 1, 1, 0),
                    BorderBrush = Brushes.Black
                };
                border.Child = textBlock;
                this.InternalChildren.Add(border);
                return (border, cell);
            }

            public int MatchRow { get; set; }

            protected override Size MeasureOverride(Size constraint) {
                return new Size(_controlWidth, ControlHeight);
            }
            protected override Size ArrangeOverride(Size arrangeBounds) {
                _matchNumberText.Control.Arrange(new Rect(0, 0, SmallTextBoxWidth, ControlHeight));
                _playerLeftText.Control.Arrange(new Rect(SmallTextBoxWidth, 0, SmallTextBoxWidth, ControlHeight));
                _playerRightText.Control.Arrange(new Rect(2 * SmallTextBoxWidth, 0, SmallTextBoxWidth, ControlHeight));
                for (int i = 0; i < _sets.Length; i++) {
                    _sets[i].Control.Arrange(new Rect(3 * SmallTextBoxWidth + i * SetPanel.ControlWidth, 0, SetPanel.ControlWidth, SetPanel.ControlHeight + 1));
                }
                double offset = 3 * SmallTextBoxWidth + _sets.Length * SetPanel.ControlWidth;
                _totalSetsLeftText.Control.Arrange(new Rect(offset, 0, LargeTextBoxWidth, ControlHeight));
                _totalSetsRightText.Control.Arrange(new Rect(offset + LargeTextBoxWidth, 0, LargeTextBoxWidth, ControlHeight));
                _matchesLeftText.Control.Arrange(new Rect(offset + 2 * LargeTextBoxWidth, 0, LargeTextBoxWidth, ControlHeight));
                _matchesRightText.Control.Arrange(new Rect(offset + 3 * LargeTextBoxWidth, 0, LargeTextBoxWidth, ControlHeight));
                return arrangeBounds;
            }
            public static double CalculateControlWidth(int setCount) {
                return SmallTextBoxWidth * 3 + setCount * SetPanel.ControlWidth + LargeTextBoxWidth * 4;
            }
            public SetPanel this[int index] {
                get {
                    return (_sets[index].Control.Child as SetPanel)!;
                }
            }

            private (Border Control, Cell<string> Value) _matchNumberText;
            private (Border Control, Cell<string> Value) _playerLeftText;
            private (Border Control, Cell<string> Value) _playerRightText;
            private (Border Control, SetPanel Panel)[] _sets;
            private (Border Control, RelayCell<string> Value) _totalSetsLeftText;
            private (Border Control, RelayCell<string> Value) _totalSetsRightText;
            private (Border Control, RelayCell<string> Value) _matchesLeftText;
            private (Border Control, RelayCell<string> Value) _matchesRightText;

            public int SetCount { get; private set; }

            private const double SmallTextBoxWidth = 20 + 1;
            private const double LargeTextBoxWidth = 28 + 1;

            private double _controlWidth;
            public const double ControlHeight = SetPanel.ControlHeight + 1 /* account for the border */;

            private static IValueConverter BackgroundConverter;
            private static IValueConverter PlayerConverter;
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
            public SetPanel(MatchPanel match, int setNumber, Func<int, int, bool, TextBox?> activator) {
                _match = match;
                _setNumber = setNumber;
                _activator = activator;
                this.LeftScore = new RelayCell<string>();
                this.RightScore = new RelayCell<string>();
                this.IsValid = new RelayCell<bool>();

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
                _left.DataContext = this;
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
                _right.DataContext = this;
                this.InternalChildren.Add(_right);

                _hyphen = new TextBlock() { Text = "-", Width = HyphenWidth, Height = ControlHeight };
                _hyphen.SetBinding(TextBlock.ForegroundProperty, foregroundBinding);
                _hyphen.DataContext = this;

                var hyphenTextBinding = new MultiBinding();
                hyphenTextBinding.Bindings.Add(new Binding("LeftScore.Value"));
                hyphenTextBinding.Bindings.Add(new Binding("RightScore.Value"));
                hyphenTextBinding.Converter = VisibilityConverter;
                _hyphen.SetBinding(TextBlock.TextProperty, hyphenTextBinding);
                this.InternalChildren.Add(_hyphen);

                _activeLeft = _left;
                _activeRight = _right;
            }

            public RelayCell<string> LeftScore { get; private set; }
            public RelayCell<string> RightScore { get; private set; }
            public RelayCell<bool> IsValid { get; private set; }

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
            private Func<int, int, bool, TextBox?> _activator;

            public const double TextBoxWidth = 28;
            public const double HyphenWidth = 5;
            public const double ControlWidth = TextBoxWidth * 2 + HyphenWidth;
            public const double ControlHeight = 16;

            public static IValueConverter ForegroundConverter;
            public static IMultiValueConverter VisibilityConverter;
        }
    }
}
