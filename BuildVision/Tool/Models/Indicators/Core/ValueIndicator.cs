using System;

using AlekseyNagovitsyn.BuildVision.Core.Logging;
using AlekseyNagovitsyn.BuildVision.Tool.Building;
using AlekseyNagovitsyn.BuildVision.Tool.ViewModels;

using EnvDTE;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators.Core
{
    public abstract class ValueIndicator : NotifyPropertyChangedBase
    {
        private int? _value;
        private bool _isEnabled;
        private bool _isUpdateError;
        private string _lastErrorMessage;

        public const string ResourcesUri = @"Tool/Views/ValueIndicator.Resources.xaml";

        public abstract string Header { get; }

        public abstract string Description { get; }

        protected abstract int? GetValue(DTE dte, BuildInfo buildContext);

        public int? Value
        {
            get { return _value; }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
        }

        public virtual string StringValue
        {
            get
            {
                if (_isUpdateError)
                    return Resources.GridCellNAText;

                return Value != null ? Value.ToString() : "0";
            }
        }

        public bool IsUpdateError
        {
            get { return _isUpdateError; }
            set
            {
                if (_isUpdateError != value)
                {
                    _isUpdateError = value;
                    OnPropertyChanged("IsUpdateError");
                }
            }
        }

        public string LastErrorMessage
        {
            get { return _lastErrorMessage; }
            set
            {
                if (_lastErrorMessage != value)
                {
                    _lastErrorMessage = value;
                    OnPropertyChanged("LastErrorMessage");
                    OnPropertyChanged("ToolTip");
                }
            }
        }

        public string ToolTip
        {
            get
            {
                if (!IsUpdateError)
                    return Header;

                return string.Format("{0}{1}{2}", 
                    Header, 
                    Environment.NewLine,
                    string.IsNullOrEmpty(LastErrorMessage) ? "Unknown error" : LastErrorMessage);
            }
        }

        public virtual double Width
        {
            get { return double.NaN; }
        }

        public void UpdateValue(DTE dte, BuildInfo buildContext)
        {
            UpdateValueAction(dte, buildContext);
        }

        public void ResetValue(ResetIndicatorMode resetMode)
        {
            switch (resetMode)
            {
                case ResetIndicatorMode.ResetValue:
                    IsUpdateError = false;
                    _value = null;
                    _isEnabled = true;
                    break;

                case ResetIndicatorMode.Disable:
                    IsUpdateError = false;
                    _value = null;
                    _isEnabled = false;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("resetMode");
            }

            RaiseValueChanged();
        }

        private void UpdateValueAction(DTE dte, BuildInfo buildContext)
        {
            IsUpdateError = false;
            _isEnabled = true;

            try
            {
                var currentValue = GetValue(dte, buildContext);
                _value = currentValue;
                LastErrorMessage = null;
            }
            catch (Exception ex)
            {
                _value = null;
                IsUpdateError = true;
                LastErrorMessage = ex.Message;
                ex.TraceUnknownException();
            }

            RaiseValueChanged();
        }

        private void RaiseValueChanged()
        {
            OnPropertyChanged("Value");
            OnPropertyChanged("StringValue");
            OnPropertyChanged("IsEnabled");
        }
    }
}