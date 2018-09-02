using System;
using BuildVision.Common;
using BuildVision.UI;
using BuildVision.UI.Contracts;
using BuildVision.UI.Common.Logging;

namespace BuildVision.UI.Models.Indicators.Core
{
    public abstract class ValueIndicator : BindableBase
    {
        private int? _value;
        private bool _isEnabled;
        private bool _isUpdateError;
        private string _lastErrorMessage;

        public const string ResourcesUri = @"Resources/ValueIndicator.Resources.xaml";

        public abstract string Header { get; }

        public abstract string Description { get; }

        protected abstract int? GetValue(IBuildInfo buildContext);

        public int? Value => _value;
        public bool IsEnabled => _isEnabled;

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
            set => SetProperty(ref _isUpdateError, value);
        }

        public string LastErrorMessage
        {
            get { return _lastErrorMessage; }
            set
            {
                SetProperty(ref _lastErrorMessage, value);
                OnPropertyChanged(nameof(ToolTip));
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

        public void UpdateValue(IBuildInfo buildContext)
        {
            UpdateValueAction(buildContext);
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
                    throw new ArgumentOutOfRangeException(nameof(resetMode));
            }

            RaiseValueChanged();
        }

        private void UpdateValueAction(IBuildInfo buildContext)
        {
            IsUpdateError = false;
            _isEnabled = true;

            try
            {
                var currentValue = GetValue(buildContext);
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
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(StringValue));
            OnPropertyChanged(nameof(IsEnabled));
        }
    }
}
