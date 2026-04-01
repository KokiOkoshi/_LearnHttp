using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
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

        private void WebView2_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            if (sender is not WebView2 webView2) return;
            if (!e.IsSuccess) return;

            webView2.CoreWebView2.SetVirtualHostNameToFolderMapping("localhost", "dist", Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);
            webView2.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
        }

        private void CoreWebView2_NavigationStarting(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            if (sender is not CoreWebView2 coreWebView2) return;
            if (!Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri)) return;
            if (uri.Host != "localhost") return;
            if (!e.Uri.EndsWith('/')) return;

            e.Cancel = true;
            coreWebView2.Navigate(e.Uri + "index.html");
        }
    }
}