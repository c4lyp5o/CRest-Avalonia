using ReactiveUI;
using System.Collections.ObjectModel;

namespace CRest_Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        //public ReactiveViewModel ReactiveViewModel { get; } = new ReactiveViewModel();

        // http methods
        public ObservableCollection<string> HTTPMethods { get; } =
            ["GET", "POST", "PATCH", "PUT", "DELETE"];

        public string? _SelectedHTTPMethod;

        public string? SelectedHTTPMethod
        {
            get
            {
                return _SelectedHTTPMethod;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _SelectedHTTPMethod, value);
                this.RaisePropertyChanged(nameof(ReqQueryVisbility));
                this.RaisePropertyChanged(nameof(ReqBodyVisbility));
            }
        }

        //// browser selection
        public ObservableCollection<string> Browsers { get; } =
            ["CRest", "Edge", "Chrome", "Firefox", "Safari", "Android", "iPhone"];

        public string? _SelectedBrowser;

        public string? SelectedBrowser
        {
            get
            {
                return _SelectedBrowser;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _SelectedBrowser, value);
            }
        }

        // request query visibility
        public bool ReqQueryVisbility
        {
            get
            {
                switch (SelectedHTTPMethod)
                {
                    case "GET":
                        return true;
                    default:
                        return false;
                }
            }
        }

        // request body visibility
        public bool ReqBodyVisbility
        {
            get
            {
                switch (SelectedHTTPMethod)
                {
                    case "POST":
                    case "PATCH":
                    case "PUT":
                        return true;
                    default:
                        return false;
                }
            }
        }

        //public ReactiveCommand<Unit, Unit> HTTPMethodChange { get; }

        //private void PerformAction()
        //{
        //    Debug.WriteLine("The action was called.");
        //}

        public MainWindowViewModel()
        {
            _SelectedHTTPMethod = "GET";
            _SelectedBrowser = "CRest";

            //ExampleCommand = ReactiveCommand.Create(PerformAction);
        }
    }
}
