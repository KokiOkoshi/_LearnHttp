using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.IO;
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
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Http;

namespace StaticSiteViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        static FileExtensionContentTypeProvider FileExtensionContentTypeProvider { get; } = new();
        static string RootFolder { get; } = @"dist";
        static string HostName { get; } = @"appassets.localhost";
        public static Uri RootUrl { get; } = new Uri($@"https://{HostName}/");

        private void WebView2_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            if (sender is not WebView2 webView2) return;
            if (!e.IsSuccess) return;

            webView2.CoreWebView2.AddWebResourceRequestedFilter(
                @$"{RootUrl.AbsoluteUri}*",
                CoreWebView2WebResourceContext.All
            );
            webView2.CoreWebView2.WebResourceRequested += HandleWebResourceRequested;
        }

        static void HandleWebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs args)
            {
            if (sender is not CoreWebView2 coreWebView2) return;

                var uri = new Uri(args.Request.Uri);
            if (uri.Host != HostName) return;

                var resourcePath = System.IO.Path.Combine(
                    RootFolder,
                    (uri.AbsolutePath switch
                    {
                        // NOTE: ファイル名省略された場合は index.html
                        [.., '/'] x => System.IO.Path.Join(x, "index.html"),
                        var x => x,
                    }).TrimStart('/'));

                switch (resourcePath)
                {
                    // リソースが存在する場合 : ステータスコード200
                    case var existingResource when File.Exists(existingResource):
                        var mimeType = FileExtensionContentTypeProvider.TryGetContentType(resourcePath, out var result) ? result : null;
                    args.Response = coreWebView2.Environment.CreateWebResourceResponse(
                            Content: File.OpenRead(resourcePath),
                            StatusCode: StatusCodes.Status200OK,
                            ReasonPhrase: "OK",
                            Headers: $"Content-Type: {mimeType}"
                        );
                        break;

                    // リソースが存在しない場合 : ステータスコード404
                    default:
                        var page404Path = System.IO.Path.Combine(RootFolder, "404.html");
                    args.Response = coreWebView2.Environment.CreateWebResourceResponse(
                            Content: File.Exists(page404Path) ? File.OpenRead(page404Path) : null,
                            StatusCode: StatusCodes.Status404NotFound,
                            ReasonPhrase: "Not Found",
                            Headers: $"Content-Type: text/html");
                        break;
                }
        }
    }
}
