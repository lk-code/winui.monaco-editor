using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using Windows.Globalization;

namespace Monaco
{
    public sealed partial class MonacoEditor : UserControl
    {
        private string _content = "";

        #region PropertyChanged Event

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Route Property

        public static readonly DependencyProperty EditorContentProperty = DependencyProperty.Register("EditorContent",
              typeof(string),
              typeof(MonacoEditor),
              new PropertyMetadata(null));
        public string EditorContent
        {
            get { return (string)GetValue(EditorContentProperty); }
            set
            {
                SetValue(EditorContentProperty, value);
                OnPropertyChanged();

                _ = this.LoadContentAsync(value);
            }
        }

        #endregion

        #region Theme Property

        public static readonly DependencyProperty EditorThemeProperty = DependencyProperty.Register("EditorTheme",
              typeof(EditorThemes),
              typeof(MonacoEditor),
              new PropertyMetadata(null));
        public EditorThemes EditorTheme
        {
            get { return (EditorThemes)GetValue(EditorThemeProperty); }
            set
            {
                SetValue(EditorThemeProperty, value);
                OnPropertyChanged();

                _ = this.SetThemeAsync(value);
            }
        }

        #endregion

        public MonacoEditor()
        {
            this.InitializeComponent();

            this.Loaded += MonacoEditor_Loaded;
        }

        private void MonacoEditor_Loaded(object sender, RoutedEventArgs e)
        {
            string monacoHtmlFile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,
                    @"MonacoEditorSource\index.html");
            this.MonacoEditorWebView.Source = new Uri(monacoHtmlFile);
        }

        /// <summary>
        /// loads the given content to the monaco editor view
        /// </summary>
        /// <param name="content">the new content</param>
        /// <returns></returns>
        public async Task LoadContentAsync(string content)
        {
            string ensuredContent = HttpUtility.JavaScriptStringEncode(content);

            this._content = ensuredContent;

            string command = $"editor.setValue(('{ensuredContent}');";

            await this.MonacoEditorWebView
                .ExecuteScriptAsync(command);
        }

        /// <summary>
        /// sets the requested theme to the monaco editor view
        /// </summary>
        /// <param name="theme">the requested theme</param>
        /// <returns></returns>
        public async Task SetThemeAsync(EditorThemes theme)
        {
            string themeValue = "vs-dark";

            switch (theme)
            {
                case EditorThemes.VisualStudioLight:
                    {
                        themeValue = "vs-light";
                    }
                    break;
                case EditorThemes.VisualStudioDark:
                    {
                        themeValue = "vs-dark";
                    }
                    break;
            }

            string command = $"editor._themeService.setTheme('{themeValue}');";

            await this.MonacoEditorWebView
                .ExecuteScriptAsync(command);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public async Task SetLanguageAsync(string language)
        {
            string command = $"editor.setModel(monaco.editor.createModel(editor.getValue(), '{language}'));";

            await this.MonacoEditorWebView
                .ExecuteScriptAsync(command);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task SelectAllAsync()
        {
            string command = $"editor.setSelection(editor.getModel().getFullModelRange());";

            await this.MonacoEditorWebView
                .ExecuteScriptAsync(command);
        }
    }
}
