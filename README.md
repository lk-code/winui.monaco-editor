# WinUI.Monaco-Editor

![WinUI.Monaco-Editor](https://raw.githubusercontent.com/lk-code/winui.monaco-editor/main/icon_128.png)

[![.NET Version](https://img.shields.io/badge/dotnet%20version-net6.0-blue?style=flat-square)](http://www.nuget.org/packages/WinUI.Monaco)
[![License](https://img.shields.io/github/license/lk-code/winui.monaco-editor.svg?style=flat-square)](https://github.com/lk-code/winui.monaco-editor/blob/master/LICENSE)
[![Downloads](https://img.shields.io/nuget/dt/WinUI.Monaco.svg?style=flat-square)](http://www.nuget.org/packages/WinUI.Monaco)
[![NuGet](https://img.shields.io/nuget/v/WinUI.Monaco.svg?style=flat-square)](http://nuget.org/packages/WinUI.Monaco)

[![buy me a coffe](https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png)](https://www.buymeacoffee.com/lk.code)

## Introduction

The [Monaco Editor](https://github.com/microsoft/monaco-editor) (Visual Studio Code) as UI Control for WinUI 3.0

**Notice:** As part of a project restructuring, the NuGet name of the library was changed from `winui.monaco-editor` to `WinUI.Monaco`.

## Install Monaco in a WinUI 3.0 Project

```
dotnet add package WinUI.Monaco
```

### WinUI.Monaco Version

The library version is composed as follows:

* the current project version
* the used Monaco Editor version
* the GitHub RunNumber of the build

`1.1.${{ env.MONACO_VERSION }}.${{ github.run_number }}` (example: `1.1.44.13`)

## Handler

This Monaco control uses handlers to provide certain actions that have nothing directly to do with the actual Monaco editor:

### MonacoWebViewDevToolsHandler

This handler can be used to open the WebView2 DevTools.

```
MonacoWebViewDevToolsHandler handler = this.MonacoEditor.GetHandler<MonacoWebViewDevToolsHandler>();

handler.OpenDebugWebViewDeveloperTools();
```

### MonacoFileRecognitionHandler

With this handler you can pass a file extension and get back the code language supported by the Monaco Editor, which can then be set in the Monaco Editor.

```
MonacoFileRecognitionHandler handler = this.MonacoEditor.GetHandler<MonacoFileRecognitionHandler>();

string fileCodeLanguage = handler.RecognizeLanguageByFileType(Path.GetExtension(file.Path));
await this.MonacoEditor.SetLanguageAsync(fileCodeLanguage);
```

## Contributors

[![Contributors](https://contrib.rocks/image?repo=lk-code/winui.monaco-editor)](https://github.com/lk-code/winui.monaco-editor/graphs/contributors)

## Development

### Update the Monaco Version

The monaco project is stored in the C#-Project Monaco under the folder `monaco-editor`.

To update the monaco version follow these steps ***(Only do this if it is really necessary. The project is always automatically updated to the latest release version of the monaco project via a GitHub workflow)***:

1. download the latest version from [Microsoft Monaco Project](https://microsoft.github.io/monaco-editor/)
2. extract the archive file
3. delete all files except for `index.html` from the folder `monaco-editor`. The `index.html` is **required** for the project
4. copy all new files from the extracted archive to the folder `monaco-editor`
