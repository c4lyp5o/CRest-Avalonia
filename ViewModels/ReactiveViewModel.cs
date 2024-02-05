using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CRest_Avalonia.ViewModels
{
    // Instead of implementing "INotifyPropertyChanged" on our own we use "ReactiveObject" as 
    // our base class. Read more about it here: https://www.reactiveui.net
    public class ReactiveViewModel : ReactiveObject
    {
        public ReactiveViewModel()
        {
            HTTPMethodChangeCommand = ReactiveCommand.Create(HTTPMethodChange);

            // We can listen to any property changes with "WhenAnyValue" and do whatever we want in "Subscribe".
            this.WhenAnyValue(o => o.SelectedHTTPMethod)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(ReqQueryVisbility)));
            this.WhenAnyValue(o => o.SelectedHTTPMethod)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(ReqBodyVisbility)));
        }

        public ICommand HTTPMethodChangeCommand { get; }

        // http methods        
        public string? _SelectedHTTPMethod = "GET";

        public string? SelectedHTTPMethod
        {
            get
            {
                return _SelectedHTTPMethod;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _SelectedHTTPMethod, value);
            }
        }

        // browser selection
        public ObservableCollection<string> Browsers { get; } =
            ["Edge", "Chrome", "Firefox", "Safari", "Android", "iPhone"];

        public string? _SelectedBrowser = "CRest";

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

        // http method change from combobox
        //private void Binding(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        //{

        //}

        private void HTTPMethodChange()
        {
            PerformReqVisibilityAction();
        }

        private void PerformReqVisibilityAction()
        {
            this.RaisePropertyChanged(nameof(ReqQueryVisbility));
            this.RaisePropertyChanged(nameof(ReqBodyVisbility));
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
    }
}