using System.Collections.ObjectModel;

namespace BuildVision.UI.Models.Indicators.Core
{
    public static class ValueIndicatorsFactory
    {
        public static ObservableCollection<ValueIndicator> CreateCollection()
        {
            return new ObservableCollection<ValueIndicator>
            {
                new ErrorsIndicator(),
                new WarningsIndicator(),
                new MessagesIndicator(),

                new SeparatorIndicator(),

                new SuccessProjectsIndicator(),
                new UpToDateProjectsIndicator(),
                new WarningProjectsIndicator(),
                new ErrorProjectsIndicator()
            };
        }
    }
}
