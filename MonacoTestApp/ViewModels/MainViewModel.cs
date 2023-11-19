using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Monaco;
using MonacoTestApp.Models;

namespace MonacoTestApp.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private MonacoEditor? _monacoEditor = null;

    [ObservableProperty]
    ObservableCollection<EditorThemes> availableEditorThemesCollection = new();

    [ObservableProperty]
    EditorThemes? selectedEditorTheme = null;

    public MainViewModel()
    {
    }

    public void Initialize(MonacoEditor monacoEditor)
    {
        this._monacoEditor = monacoEditor;

        this.LoadEditorThemes();
    }

    private void LoadEditorThemes()
    {
        this.AvailableEditorThemesCollection.Clear();

        this.AvailableEditorThemesCollection.Add(EditorThemes.VisualStudioLight);
        this.AvailableEditorThemesCollection.Add(EditorThemes.VisualStudioDark);

        this.SelectedEditorTheme = this.AvailableEditorThemesCollection.First();
    }

    [RelayCommand]
    public async Task SetLightThemeAsync(CancellationToken cancellationToken)
    {

    }

    [RelayCommand]
    public async Task SetDarkThemeAsync(CancellationToken cancellationToken)
    {

    }
}
