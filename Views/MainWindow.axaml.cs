using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace CRest_Avalonia.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ReadConfigFile();
            SetRequestHeaders(new HttpClient(), SelectedBrowser);
        }

        private int apiCallStatusCode;
        private string apiCall = "";
        private readonly string jsonFilePath = @"C:\Users\Public\Documents\CRest\config.json";

        public class KeyValuePair
        {
            public required string Key { get; set; }
            public required string Value { get; set; }
        }

        // debug
        private void DebugMessage(string message)
        {
            message = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " - " + message;
            Debug.WriteLine(message);

            //DebugOutput.Items.Add(message);
            //DebugOutput.ScrollIntoView(DebugOutput.Items[^1]);
        }

        // http methods
        public string SelectedHTTPMethod { get; set; } = "GET";

        // browser selection
        public string SelectedBrowser { get; set; } = "CRest";

        // create a collection of headers in a dictionary
        public ObservableCollection<KeyValuePair> Headers { get; } = [];

        // create a collection of headers in a dictionary
        public ObservableCollection<KeyValuePair> Queries { get; } = [];

        // create a list of headers
        private static readonly KeyValuePair[] currentHeaders =
        [
            new KeyValuePair { Key = "Accept", Value = "application/json" },
            //new KeyValuePair { Key = "User-Agent", Value = "CRest/1.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) CRest/1.0" }
            //new KeyValuePair { Key = "Accept-Language", Value = "en-US" },
            //new KeyValuePair { Key = "Accept-Encoding", Value = "gzip, deflate, br" },
            //new KeyValuePair { Key = "Connection", Value = "keep-alive" },
            //new KeyValuePair { Key = "Upgrade-Insecure-Requests", Value = "1" },
            //new KeyValuePair { Key = "Cache-Control", Value = "max-age=0" },
            //new KeyValuePair { Key = "Sec-Fetch-Dest", Value = "document" },
            //new KeyValuePair { Key = "Sec-Fetch-Mode", Value = "navigate" },
            //new KeyValuePair { Key = "Sec-Fetch-Site", Value = "none" },
            //new KeyValuePair { Key = "Sec-Fetch-User", Value = "?1" },
            //new KeyValuePair { Key = "Sec-GPC", Value = "1" },
        ];

        // config file
        public class ConfigFile
        {
            public required string BasePath { get; set; }
            public List<KeyValuePair>? Headers { get; set; }
            public required string[] ApiPath { get; set; }
            public string[]? Queries { get; set; }
            public string[]? Body { get; set; }
        }

        private void ReadConfigFile()
        {
            if (!File.Exists(jsonFilePath))
            {
                DebugMessage("Config file does not exist. Creating new config file.");

                _ = Directory.CreateDirectory(@"C:\Users\Public\Documents\CRest");
                File.Create(jsonFilePath).Dispose();
            }

            DebugMessage("Reading config file.");
            List<ConfigFile> configFromFile = [];

            using StreamReader r = new(@"C:\Users\Public\Documents\CRest\config.json");
            string json = r.ReadToEnd();

            if (string.IsNullOrWhiteSpace(json))
            {
                DebugMessage("Config file is empty.");

                return;
            }
            else
            {
                configFromFile = Deserialize<List<ConfigFile>>(json);

                if (BasePathListBox.Items.Count > 0)
                {
                    DebugMessage("BasePathListBox.Items.Count > 0");
                    BasePathListBox.Items.Clear();
                    APICallListBox.Items.Clear();
                }

                for (int i = 0; i < configFromFile.Count; i++)
                {
                    ConfigFile config = configFromFile[i];
                    DebugMessage("config: " + config);
                    DebugMessage("config.basePath: " + config.BasePath);
                    DebugMessage("config.apiPath: " + string.Join(", ", config.ApiPath));
                    BasePathListBox.Items.Add(config.BasePath);
                }
            }
        }

        private void SetAPICallStatusCode(int value)
        {
            apiCallStatusCode = value;
            APICallStatusCodeTextBox.Text = value.ToString();

            if (value >= 200 && value < 300)
            {
                APICallStatusLight.Fill = Brushes.Green;
                APICallStatusNameTextBox.Text = "OK";
            }
            else if (value >= 300 && value < 400)
            {
                APICallStatusLight.Fill = Brushes.Yellow;
                APICallStatusNameTextBox.Text = "Redirect";
            }
            else if (value >= 400 && value < 500)
            {
                APICallStatusLight.Fill = Brushes.Orange;
                APICallStatusNameTextBox.Text = "Client Error";
            }
            else if (value >= 500)
            {
                APICallStatusLight.Fill = Brushes.Red;
                APICallStatusNameTextBox.Text = "Server Error";
            }
            else
            {
                APICallStatusLight.Fill = Brushes.Gray;
                APICallStatusNameTextBox.Text = "Unknown";
            }
        }

        // json formatter
        private static readonly JsonSerializerOptions s_writeOptions =
            new() { WriteIndented = true, };

        private static readonly JsonSerializerOptions s_readOptions =
            new() { AllowTrailingCommas = true };

        private static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, s_writeOptions)!;
        }

        private static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, s_readOptions)!;
        }

        private static string FormatJson(string jsonString)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(jsonString);
                return JsonSerializer.Serialize(doc.RootElement, s_writeOptions);
            }
            catch (JsonException)
            {
                return jsonString;
            }
        }

        private void APICallUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            apiCall = APICallUrl.Text!;
        }

        private void SetRequestHeaders(HttpClient client, string browserType)
        {
            client.DefaultRequestHeaders.Clear();

            foreach (var header in currentHeaders)
            {
                DebugMessage($"Adding header: {header.Key}: {header.Value}");
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
                Headers.Add(new KeyValuePair { Key = header.Key, Value = header.Value });
            }

            switch (browserType)
            {
                case "CRest":
                    client.DefaultRequestHeaders.Add(
                        "User-Agent",
                        "CRest/1.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) CRest/1.0 Chrome"
                    );
                    Headers.Add(
                        new KeyValuePair
                        {
                            Key = "User-Agent",
                            Value =
                                "CRest/1.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) CRest/1.0 Chrome"
                        }
                    );
                    break;
                case "Edge":
                    client.DefaultRequestHeaders.Add(
                        "User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36 Edg/121.0.0.0"
                    );
                    break;
                case "Chrome":
                    client.DefaultRequestHeaders.Add(
                        "User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36"
                    );
                    break;
                case "Firefox":
                    client.DefaultRequestHeaders.Add(
                        "User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:121.0) Gecko/20100101 Firefox/121.0"
                    );
                    break;
                case "Safari":
                    client.DefaultRequestHeaders.Add(
                        "User-Agent",
                        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0.3 Safari/605.1.15"
                    );
                    break;
                case "Android":
                    client.DefaultRequestHeaders.Add(
                        "User-Agent",
                        "Mozilla/5.0 (Linux; Android 10; SM-A205U) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Mobile Safari/537.36"
                    );
                    break;
                case "iPhone":
                    client.DefaultRequestHeaders.Add(
                        "User-Agent",
                        "Mozilla/5.0 (iPhone; CPU iPhone OS 13_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0 Mobile/15E148 Safari/604.1"
                    );
                    break;
                default:
                    throw new ArgumentException($"Invalid browser type: {browserType}");
            }

            foreach (var header in Headers)
            {
                StackPanel keyValuePairPanel =
                    new()
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        Margin = Avalonia.Thickness.Parse("0, 5, 0, 0")
                    };

                TextBox keyTextBox =
                    new()
                    {
                        Width = 100, // Set a width as needed
                        Margin = Avalonia.Thickness.Parse("0, 5, 5, 5"),
                        Text = header.Key
                    };

                TextBox valueTextBox =
                    new()
                    {
                        Width = 100, // Set a width as needed
                        Margin = Avalonia.Thickness.Parse("0, 5, 5, 5"),
                        Text = header.Value
                    };

                Button removeButton =
                    new()
                    {
                        Content = "-",
                        Width = 20,
                        Margin = Avalonia.Thickness.Parse("0, 5, 5, 5"),
                        Tag = keyValuePairPanel // Store a reference to the parent panel for removal
                    };

                removeButton.Click += RemoveHeaderButton_Click;

                keyValuePairPanel.Children.Add(keyTextBox);
                keyValuePairPanel.Children.Add(valueTextBox);
                keyValuePairPanel.Children.Add(removeButton);

                RequestHeadersKeyValueStackPanel.Children.Add(keyValuePairPanel);
            }
        }

        private void AddHeaderButton_Click(object sender, RoutedEventArgs e)
        {
            StackPanel keyValuePairPanel = new();

            TextBox keyTextBox =
                new()
                {
                    Width = 100, // Set a width as needed
                    Margin = Avalonia.Thickness.Parse("0, 5, 5, 5")
                };

            TextBox valueTextBox =
                new()
                {
                    Width = 100, // Set a width as needed
                    Margin = Avalonia.Thickness.Parse("0, 5, 5, 5")
                };

            Button removeButton =
                new()
                {
                    Content = "Remove",
                    Width = 20,
                    Tag = keyValuePairPanel // Store a reference to the parent panel for removal
                };
            removeButton.Click += RemoveHeaderButton_Click;

            keyValuePairPanel.Children.Add(keyTextBox);
            keyValuePairPanel.Children.Add(valueTextBox);
            keyValuePairPanel.Children.Add(removeButton);

            RequestHeadersKeyValueStackPanel.Children.Add(keyValuePairPanel);
        }

        private void RemoveHeaderButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button removeButton && removeButton.Tag is StackPanel keyValuePairPanel)
            {
                RequestHeadersKeyValueStackPanel.Children.Remove(keyValuePairPanel);
            }
        }

        private void AddQueryButton_Click(object sender, RoutedEventArgs e)
        {
            StackPanel keyValuePairPanel = new();

            TextBox keyTextBox =
                new()
                {
                    Width = 100, // Set a width as needed
                    Margin = Avalonia.Thickness.Parse("0, 5, 5, 5")
                };

            TextBox valueTextBox =
                new()
                {
                    Width = 100, // Set a width as needed
                    Margin = Avalonia.Thickness.Parse("0, 5, 5, 5")
                };

            Button removeButton =
                new()
                {
                    Content = "Remove",
                    Width = 70,
                    Tag = keyValuePairPanel // Store a reference to the parent panel for removal
                };
            removeButton.Click += RemoveQueryButton_Click;

            keyValuePairPanel.Children.Add(keyTextBox);
            keyValuePairPanel.Children.Add(valueTextBox);
            keyValuePairPanel.Children.Add(removeButton);

            QueryKeyValueStackPanel.Children.Add(keyValuePairPanel);
        }

        private void RemoveQueryButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the parent StackPanel from the Tag property
            if (sender is Button removeButton && removeButton.Tag is StackPanel keyValuePairPanel)
            {
                // Remove the keyValuePairPanel from the main StackPanel
                QueryKeyValueStackPanel.Children.Remove(keyValuePairPanel);
            }
        }

        private async void APICallExecute_Click(object sender, RoutedEventArgs e)
        {
            if (
                string.IsNullOrWhiteSpace(APICallUrl.Text)
                || !Uri.TryCreate(APICallUrl.Text, UriKind.Absolute, out var uri)
            )
            {
                // show an error message
                return;
            }

            // if the URL does not start with http:// or https://, append https://
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                uri = new Uri("https://" + uri.ToString());
            }

            string apiCall = uri.ToString();

            APICallResponseTextBox.Clear();
            ResponseHeadersKeyValueStackPanel.Children.Clear();

            DebugMessage($"Clean execute API Call URL: {apiCall}");

            try
            {
                using HttpClient client = new();

                //SetAPICallStatusCode(0);
                //SetRequestHeaders(client, SelectedBrowser);

                if (RequestHeadersKeyValueStackPanel.Children.Count > 0)
                {
                    DebugMessage("Adding custom headers");
                    foreach (
                        StackPanel keyValuePairPanel in RequestHeadersKeyValueStackPanel.Children.Cast<StackPanel>()
                    )
                    {
                        string key = ((TextBox)keyValuePairPanel.Children[0]).Text!;
                        string value = ((TextBox)keyValuePairPanel.Children[1]).Text!;

                        DebugMessage($"Adding header: {key}: {value}");
                        client.DefaultRequestHeaders.Add(key, value);
                    }
                }

                HttpResponseMessage response;

                switch (SelectedHTTPMethod)
                {
                    case "GET":
                        if (QueryKeyValueStackPanel.Children.Count > 0)
                        {
                            apiCall += "?";
                            foreach (
                                StackPanel keyValuePairPanel in QueryKeyValueStackPanel.Children.Cast<StackPanel>()
                            )
                            {
                                string key = ((TextBox)keyValuePairPanel.Children[0]).Text!;
                                string value = ((TextBox)keyValuePairPanel.Children[1]).Text!;

                                DebugMessage($"Adding query parameter: {key}: {value}");
                                apiCall += $"{key}={value}&";
                            }
                            apiCall = apiCall.Remove(apiCall.Length - 1);
                        }
                        response = await client.GetAsync(apiCall);
                        break;
                    case "POST":
                    case "PATCH":
                    case "PUT":
                        //string apiCallBody = new TextRange(
                        //    APICallBodyTextBox.Document.ContentStart,
                        //    APICallBodyTextBox.Document.ContentEnd
                        //).Text;
                        //DebugMessage($"apiCallBody: {apiCallBody}");
                        //string formattedJson = FormatJson(apiCallBody);
                        //DebugMessage($"formattedJson: {formattedJson}");
                        StringContent content = new("");
                        switch (SelectedHTTPMethod)
                        {
                            case "PATCH":
                                response = await client.PatchAsync(apiCall, content);
                                break;
                            case "PUT":
                                response = await client.PutAsync(apiCall, content);
                                break;
                            default:
                                response = await client.PostAsync(apiCall, content);
                                break;
                        }
                        break;
                    case "DELETE":
                        response = await client.DeleteAsync(apiCall);
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Invalid HTTP method: {SelectedHTTPMethod}"
                        );
                }

                SetAPICallStatusCode((int)response.StatusCode);

                string apiCallResponse = await response.Content.ReadAsStringAsync();

                switch (response.Content.Headers.ContentType?.MediaType)
                {
                    //case "image/jpeg":
                    //case "image/png":
                    //case "image/gif":
                    //    var imageBytes = await response.Content.ReadAsByteArrayAsync();

                    //    if (imageBytes != null && imageBytes.Length > 0)
                    //    {
                    //        var image = new Image();
                    //        using (var stream = new MemoryStream(imageBytes))
                    //        {
                    //            var bitmapImage = new BitmapImage();
                    //            bitmapImage.BeginInit();
                    //            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    //            bitmapImage.StreamSource = stream;
                    //            bitmapImage.EndInit();
                    //            image.Source = bitmapImage;
                    //        }

                    //        APICallResponseTextBox.Document.Blocks.Add(new BlockUIContainer(image));
                    //    }
                    //    break;
                    case "application/json":
                        try
                        {
                            var jsonDocument = JsonDocument.Parse(apiCallResponse);
                            var formattedJson = Serialize(jsonDocument);
                            APICallResponseTextBox.SetValue(TextBlock.TextProperty, formattedJson);
                        }
                        catch (JsonException)
                        {
                            APICallResponseTextBox.SetValue(
                                TextBlock.TextProperty,
                                apiCallResponse
                            );
                        }
                        break;
                    default:
                        APICallResponseTextBox.SetValue(TextBlock.TextProperty, apiCallResponse);
                        break;
                }

                foreach (var header in response.Headers)
                {
                    WrapPanel keyValuePairPanel = new();

                    TextBox keyTextBox =
                        new()
                        {
                            Width = double.NaN, // Auto width
                            Margin = Avalonia.Thickness.Parse("0, 0, 5, 0"),
                            Text = header.Key,
                            IsReadOnly = true, // Make it read-only for a clean appearance
                            /*BorderThickness = new Thickness(0)*/
                            // Remove the border
                        };

                    TextBox valueTextBox =
                        new()
                        {
                            Width = double.NaN, // Auto width
                            Margin = Avalonia.Thickness.Parse("0, 0, 5, 0"),
                            Text = string.Join(", ", header.Value),
                            IsReadOnly = true, // Make it read-only for a clean appearance
                            /*BorderThickness = new Thickness(0)*/
                            // Remove the border
                        };

                    keyValuePairPanel.Children.Add(keyTextBox);
                    keyValuePairPanel.Children.Add(valueTextBox);

                    ResponseHeadersKeyValueStackPanel.Children.Add(keyValuePairPanel);
                }

                foreach (var header in response.Content.Headers)
                {
                    WrapPanel keyValuePairPanel = new();

                    TextBox keyTextBox =
                        new()
                        {
                            Width = double.NaN, // Auto width
                            Margin = Avalonia.Thickness.Parse("0, 5, 5, 5"),
                            Text = header.Key,
                            IsReadOnly = true, // Make it read-only for a clean appearance
                            /*BorderThickness = new Thickness(0)*/
                            // Remove the border
                        };

                    TextBox valueTextBox =
                        new()
                        {
                            Width = double.NaN, // Auto width
                            Margin = Avalonia.Thickness.Parse("0, 5, 5, 5"),
                            Text = string.Join(", ", header.Value),
                            IsReadOnly = true, // Make it read-only for a clean appearance
                            /*BorderThickness = new Thickness(0)*/
                            // Remove the border
                        };

                    keyValuePairPanel.Children.Add(keyTextBox);
                    keyValuePairPanel.Children.Add(valueTextBox);

                    ResponseHeadersKeyValueStackPanel.Children.Add(keyValuePairPanel);
                }
            }
            catch (HttpRequestException ex)
            {
                //MessageBox.Show($"Error executing API call: {ex.Message}");
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (
                    string.IsNullOrWhiteSpace(APICallUrl.Text)
                    || !Uri.TryCreate(APICallUrl.Text, UriKind.Absolute, out var uri)
                )
                {
                    //MessageBox.Show("Please enter a valid API call URL");
                    return;
                }

                if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                {
                    //MessageBox.Show("API call URL must start with http:// or https://");
                    return;
                }

                if (!Uri.TryCreate(apiCall, UriKind.Absolute, out var baseUri))
                {
                    //MessageBox.Show("Invalid base URL format");
                    return;
                }

                string BasePath = baseUri.GetLeftPart(UriPartial.Authority);

                List<KeyValuePair> CurrentHeaders = [];
                foreach (StackPanel keyValuePairPanel in RequestHeadersKeyValueStackPanel.Children.Cast<StackPanel>())
                {
                    string key = ((TextBox)keyValuePairPanel.Children[0]).Text!;
                    string value = ((TextBox)keyValuePairPanel.Children[1]).Text!;

                    if (string.IsNullOrEmpty(key) && string.IsNullOrEmpty(value))
                    {
                        continue;
                    }
                    else
                    {
                        CurrentHeaders.Add(new KeyValuePair { Key = key, Value = value });
                    }
                }

                string APIPath = apiCall.Replace(BasePath, "");
                if (string.IsNullOrEmpty(APIPath))
                {
                    APIPath = "/";
                }

                string Queries = "";
                foreach (
                    StackPanel keyValuePairPanel in QueryKeyValueStackPanel.Children.Cast<StackPanel>()
                )
                {
                    string key = ((TextBox)keyValuePairPanel.Children[0]).Text!;
                    string value = ((TextBox)keyValuePairPanel.Children[1]).Text!;

                    if (string.IsNullOrEmpty(Queries))
                    {
                        Queries += "?";
                    }
                    else
                    {
                        Queries += "&";
                    }

                    if (string.IsNullOrEmpty(key) && string.IsNullOrEmpty(value))
                    {
                        continue;
                    }
                    else
                    {
                        Queries += $"{key}={value}";
                    }
                }

                //string Body = new TextRange(
                //    APICallBodyTextBox.Document.ContentStart,
                //    APICallBodyTextBox.Document.ContentEnd
                //).Text.Replace("\r\n", "");

                DebugMessage($"Save BasePath: {BasePath}");
                DebugMessage($"Save APIPath: {APIPath}");
                DebugMessage($"Save Headers: {CurrentHeaders}");
                DebugMessage($"Save Queries: {Queries}");
                //DebugMessage($"Save Body: {Body}");

                string existingJson = File.ReadAllText(jsonFilePath);
                List<ConfigFile> source = string.IsNullOrWhiteSpace(existingJson)
                    ? []
                    : Deserialize<List<ConfigFile>>(existingJson);

                ConfigFile existingConfig = source.FirstOrDefault(
                    config => config.BasePath == BasePath
                )!;

                if (existingConfig != null && existingConfig.ApiPath.Contains(APIPath))
                {
                    //MessageBox.Show("API call already exists in the config file. Updating it");

                    var headersFromFile = existingConfig.Headers?.ToList();
                    headersFromFile?.AddRange(CurrentHeaders); // assuming Headers is List<Header>
                    existingConfig.Headers = headersFromFile;

                    int index = Array.IndexOf(existingConfig.ApiPath, APIPath);
                    if (index != -1)
                    {
                        var queries = existingConfig.Queries!.ToList();
                        if (queries.Count > index)
                        {
                            queries[index] = Queries; // assuming Queries is string
                        }
                        else
                        {
                            queries.Add(Queries);
                        }
                        existingConfig.Queries = [.. queries];

                        // Update body data
                        //var bodies = existingConfig.Body?.ToList();
                        //if (bodies?.Count > index)
                        //{
                        //    bodies[index] = Body; // assuming Body is string
                        //}
                        //else
                        //{
                        //    bodies?.Add(Body);
                        //}
                        //existingConfig.Body = [.. bodies];
                    }
                }
                else if (existingConfig != null)
                {
                    DebugMessage(
                        "Base path already exists in config file. Adding API path to existing base path."
                    );

                    var apiPaths = existingConfig.ApiPath.ToList();
                    apiPaths.Add(APIPath);
                    existingConfig.ApiPath = [.. apiPaths];

                    if (existingConfig.Headers != null)
                    {
                        //var headersFromFile = existingConfig.Headers.ToList();
                        //headersFromFile.AddRange(CurrentHeaders); // assuming Headers is List<Header>
                        existingConfig.Headers = CurrentHeaders;
                    }

                    var queries = existingConfig.Queries!.ToList();
                    queries.Add(Queries); // assuming Queries is string
                    existingConfig.Queries = [.. queries];

                    // Add new body
                    //var bodies = existingConfig.Body!.ToList();
                    //bodies.Add(Body); // assuming Body is string
                    //existingConfig.Body = [.. bodies];
                }
                else if (existingConfig == null)
                {
                    DebugMessage(
                        "Base path does not exist in config file. Adding new base path and API path to config file."
                    );

                    ConfigFile newConfig =
                        new()
                        {
                            BasePath = BasePath,
                            Headers = CurrentHeaders,
                            ApiPath = [APIPath],
                            Queries = [Queries],
                            //Body = [Body]
                        };
                    source.Add(newConfig);
                }

                string updatedJson = Serialize(source);
                File.WriteAllText(jsonFilePath, updatedJson);
                DebugMessage($"JSON file updated with new data: {source}");

                ReadConfigFile();
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"An error occurred: {ex.Message}");
                DebugMessage($"An error occurred: {ex.Message}");
            }
        }

        private void BasePathListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DebugMessage("BasePathListBox_SelectionChanged");

            if (BasePathListBox.SelectedItem == null)
            {
                DebugMessage("BasePathListBox.SelectedItem == null");
                return;
            }

            APICallUrl.Text = "";

            DebugMessage("BasePathListBox.SelectedItem: " + BasePathListBox.SelectedItem);

            APICallListBox.Items.Clear();

            string json = File.ReadAllText(jsonFilePath);
            List<ConfigFile> configFromFile = Deserialize<List<ConfigFile>>(json);

            for (int i = 0; i < configFromFile.Count; i++)
            {
                ConfigFile config = configFromFile[i];
                DebugMessage("config: " + config);
                DebugMessage("config.basePath: " + config.BasePath);
                DebugMessage("config.apiPath: " + string.Join(", ", config.ApiPath));

                if (BasePathListBox.SelectedItem.ToString() == config.BasePath)
                {
                    for (int j = 0; j < config.ApiPath.Length; j++)
                    {
                        DebugMessage("config.ApiPath[j]: " + config.ApiPath[j]);
                        APICallListBox.Items.Add(config.ApiPath[j]);
                    }
                }
            }
        }

        private void APICallListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DebugMessage("APICallListBox_SelectionChanged");

            if (APICallListBox.SelectedItem == null)
            {
                DebugMessage("APICallListBox.SelectedItem == null");
                return;
            }

            DebugMessage("APICallListBox.SelectedItem: " + APICallListBox.SelectedItem);

            string basePath = BasePathListBox!.SelectedItem?.ToString()!;
            string apiPath = APICallListBox!.SelectedItem?.ToString()!;
            string apiCall = basePath + apiPath;
            APICallUrl.Text = apiCall;

            string json = File.ReadAllText(jsonFilePath);
            List<ConfigFile> configFromFile = Deserialize<List<ConfigFile>>(json);

            ConfigFile selectedConfig = configFromFile.FirstOrDefault(
                c => c.BasePath == basePath && c.ApiPath.Contains(apiPath)
            )!;

            if (selectedConfig != null)
            {
                Headers.Clear();

                RequestHeadersKeyValueStackPanel.Children.Clear();

                if (selectedConfig.Headers != null)
                {
                    foreach (var header in selectedConfig.Headers)
                    {
                        StackPanel keyValuePairPanel =
                            new()
                            {
                                Orientation = Avalonia.Layout.Orientation.Horizontal,
                                Margin = Avalonia.Thickness.Parse("0, 5, 5, 5"),
                            };

                        TextBox keyTextBox =
                            new()
                            {
                                Width = 100, // Set a width as needed
                                Margin = Avalonia.Thickness.Parse("0, 5, 5, 5"),
                                Text = header.Key
                            };

                        TextBox valueTextBox =
                            new()
                            {
                                Width = 100, // Set a width as needed
                                Margin = Avalonia.Thickness.Parse("0, 5, 5, 5"),
                                Text = header.Value
                            };

                        Button removeButton =
                            new()
                            {
                                Content = "Remove",
                                Width = 70,
                                Tag = keyValuePairPanel // Store a reference to the parent panel for removal
                            };

                        removeButton.Click += RemoveHeaderButton_Click;

                        keyValuePairPanel.Children.Add(keyTextBox);
                        keyValuePairPanel.Children.Add(valueTextBox);
                        keyValuePairPanel.Children.Add(removeButton);

                        RequestHeadersKeyValueStackPanel.Children.Add(keyValuePairPanel);
                    }
                }

                Queries.Clear();

                QueryKeyValueStackPanel.Children.Clear();

                if (
                    selectedConfig.Queries != null
                    && selectedConfig.Queries.Any(query => !string.IsNullOrWhiteSpace(query))
                )
                {
                    string queries = string.Empty;
                    int index = Array.IndexOf(
                        selectedConfig.ApiPath,
                        APICallListBox!.SelectedItem?.ToString()
                    );
                    if (index != -1 && !string.IsNullOrEmpty(selectedConfig.Queries[index]))
                    {
                        queries = selectedConfig.Queries[index].Replace("?", "");
                        string[] queryArray = queries.Split("&");

                        foreach (var query in queryArray)
                        {
                            string[] queryKeyValuePair = query.Split("=");
                            string key = queryKeyValuePair[0];
                            string value = queryKeyValuePair[1];

                            StackPanel keyValuePairPanel =
                                new()
                                {
                                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                                    Margin = Avalonia.Thickness.Parse("0, 5, 5, 5")
                                };

                            TextBox keyTextBox =
                                new()
                                {
                                    Width = 100, // Set a width as needed
                                    Margin = Avalonia.Thickness.Parse("0, 5, 5, 5"),
                                    Text = key
                                };

                            TextBox valueTextBox =
                                new()
                                {
                                    Width = 100, // Set a width as needed
                                    Margin = Avalonia.Thickness.Parse("0, 5, 5, 5"),
                                    Text = value
                                };

                            Button removeButton =
                                new()
                                {
                                    Content = "-",
                                    Width = 20,
                                    Tag = keyValuePairPanel // Store a reference to the parent panel for removal
                                };

                            removeButton.Click += RemoveQueryButton_Click;

                            keyValuePairPanel.Children.Add(keyTextBox);
                            keyValuePairPanel.Children.Add(valueTextBox);
                            keyValuePairPanel.Children.Add(removeButton);

                            QueryKeyValueStackPanel.Children.Add(keyValuePairPanel);
                        }
                    }
                }

                //APICallBodyTextBox.Document.Blocks.Clear();

                //if (
                //    selectedConfig.Body != null
                //    && selectedConfig.Body.Any(body => !string.IsNullOrWhiteSpace(body))
                //)
                //{
                //    foreach (var body in selectedConfig.Body)
                //    {
                //        APICallBodyTextBox.Document.Blocks.Add(new Paragraph(new Run(body)));
                //    }
                //}
            }
        }

        private void APIBodyFormat_Click(object sender, RoutedEventArgs e)
        {
            //if (
            //    string.IsNullOrWhiteSpace(
            //        new TextRange(
            //            APICallBodyTextBox.Document.ContentStart,
            //            APICallBodyTextBox.Document.ContentEnd
            //        ).Text
            //    )
            //)
            //{
            //    return;
            //}
            //else
            //{
            //    string apiCallBody = new TextRange(
            //        APICallBodyTextBox.Document.ContentStart,
            //        APICallBodyTextBox.Document.ContentEnd
            //    ).Text;
            //    JsonDocument jsonDocument = JsonDocument.Parse(apiCallBody);
            //    string formattedJson = Serialize(jsonDocument);
            //    APICallBodyTextBox.Document.Blocks.Clear();
            //    APICallBodyTextBox.Document.Blocks.Add(new Paragraph(new Run(formattedJson)));
            //}
        }

        private void Binding(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            // set SelectedHTTPMethod
            Debug.WriteLine("HTTPMethodComboBox_SelectionChanged");
            Debug.WriteLine("HTTPMethodComboBox.SelectedItem: " + HTTPMethodComboBox.SelectedItem);
            SelectedHTTPMethod = (string)HTTPMethodComboBox.SelectedItem!;
        }
    }
}
