using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels {
    public class RelayCommand : RelayCommand<object> {
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
            : base(
                  c => execute(), 
                  c => { 
                      if (canExecute != null)
                          return canExecute();
                      return true;
                  }) { }
        public RelayCommand(Action execute, Cell<bool> canExecute)
            : base(
                  c => execute(),
                  canExecute) { }
    }

    public class RelayCommand<T> : ICommand {
        #region Fields

        readonly Action<T> _execute;
        readonly Predicate<T>? _canExecute = null;
        readonly Cell<bool>? _canExecuteCell = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="DelegateCommand{T}"/>.
        /// </summary>
        /// <param name="execute">Delegate to execute when Execute is called on the command.  This can be null to just hook up a CanExecute delegate.</param>
        /// <remarks><seealso cref="CanExecute"/> will always return true.</remarks>
        public RelayCommand(Action<T> execute)
            : this(execute, (Predicate<T>?)null) {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> execute, Predicate<T>? canExecute) {
            _execute = execute;
            _canExecute = canExecute;
        }

        public RelayCommand(Action<T> execute, Cell<bool> canExecute) {
            _execute = execute;
            _canExecuteCell = canExecute;
            canExecute.ValueChanged += RaiseCanExecuteChanged;
        }

        public event EventHandler CanExecuteChanged;

        #endregion

        #region ICommand Members

        ///<summary>
        ///Defines the method that determines whether the command can execute in its current state.
        ///</summary>
        ///<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        ///<returns>
        ///true if this command can be executed; otherwise, false.
        ///</returns>
        public bool CanExecute(object parameter) {
            if (_canExecuteCell != null)
                return _canExecuteCell.Value;
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        public void RaiseCanExecuteChanged() {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        ///<summary>
        ///Occurs when changes occur that affect whether or not the command should execute.
        ///</summary>
        //public event EventHandler CanExecuteChanged {
        //    add { CommandManager.RequerySuggested += value; }
        //    remove { CommandManager.RequerySuggested -= value; }
        //}

        ///<summary>
        ///Defines the method to be called when the command is invoked.
        ///</summary>
        ///<param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        public void Execute(object parameter) {
            _execute((T)parameter);
        }

        #endregion
    }
}
