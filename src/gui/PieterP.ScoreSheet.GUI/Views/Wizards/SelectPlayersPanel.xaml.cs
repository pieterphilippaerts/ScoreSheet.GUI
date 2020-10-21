using PieterP.ScoreSheet.ViewModels.Wizards;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PieterP.ScoreSheet.GUI.Views.Wizards
{
    /// <summary>
    /// Interaction logic for SelectPlayersPanel.xaml
    /// </summary>
    public partial class SelectPlayersPanel : UserControl
    {
        public SelectPlayersPanel()
        {
            InitializeComponent();
        }
        public static DependencyObject? GetScrollViewer(DependencyObject o)
        {
            if (o is ScrollViewer) { return o; }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);
                var result = GetScrollViewer(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
        private void TheListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var over = InputHitTest(e.GetPosition(this)) as DependencyObject;
                if (over != null && IsListBoxItem(over))
                {
                    var selected = TheListBox.SelectedItem;
                    if (selected != null)
                        DragDrop.DoDragDrop(TheListBox, new DataObject("application/x-member", selected), DragDropEffects.Link);
                }
            }
        }
        private bool IsListBoxItem(DependencyObject o)
        {
            if (o == null)
                return false;
            if (o.GetType() == typeof(ListBoxItem))
                return true;
            return IsListBoxItem(VisualTreeHelper.GetParent(o));
        }
        private void TheListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initial)
            {
                _initial = true;
                var scrollViewer = GetScrollViewer(TheListBox) as ScrollViewer;
                scrollViewer?.ScrollToBottom();
                if (e.AddedItems.Count > 0)
                {
                    TheListBox.ScrollIntoView(e.AddedItems[0]);
                }
                TheListBox.SelectedItem = null;
            }
        }
        private bool _initial;

        /// <summary>
        /// Event handler for when the user double clicks an item in the listbox.
        /// Appends the related player to the next empty spot on the team.
        /// </summary>
        /// <param name="sender">The clicked list item.</param>
        /// <param name="e">The event arguments.</param>
        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var memberListItem = ((FrameworkElement)sender).DataContext as MemberListItem;

            var viewModel = DataContext as SelectPlayersViewModel;
            if (viewModel != null && memberListItem != null)
                viewModel.AppendPlayer(memberListItem);
        }
    }
}