# WinUI.Monaco-Editor

![WinUI.Monaco-Editor](https://raw.githubusercontent.com/lk-code/winui.monaco-editor/main/icon_128.png)

[![.NET Version](https://img.shields.io/badge/dotnet%20version-net6.0-blue?style=flat-square)](http://www.nuget.org/packages/WinUI.Monaco)
[![License](https://img.shields.io/github/license/lk-code/winui.monaco-editor.svg?style=flat-square)](https://github.com/lk-code/winui.monaco-editor/blob/master/LICENSE)
[![Downloads](https://img.shields.io/nuget/dt/WinUI.Monaco.svg?style=flat-square)](http://www.nuget.org/packages/WinUI.Monaco)
[![NuGet](https://img.shields.io/nuget/v/WinUI.Monaco.svg?style=flat-square)](http://nuget.org/packages/WinUI.Monaco)

[![buy me a coffe](https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png)](https://www.buymeacoffee.com/lk.code)

## Introduction

The Monaco Editor (Visual Studio Code) as UI Control for WinUI 3.0

**Notice:** As part of a project restructuring, the NuGet name of the library was changed from `winui.monaco-editor` to `WinUI.Monaco`.

## Install Monaco in a WinUI 3.0 Project

```
dotnet add package WinUI.Monaco
```

## Contributors

[![Contributors](https://contrib.rocks/image?repo=lk-code/winui.monaco-editor)](https://github.com/lk-code/winui.monaco-editor/graphs/contributors)

## Development

### Update the Monaco Version

The monaco project is stored in the C#-Project Monaco under the folder `MonacoEditorSource`.

To update the monaco version follo these steps:

1. download the latest version from [Microsoft Monaco Project](https://microsoft.github.io/monaco-editor/)
2. extract the archive file
3. delete all files except for `index.html` from the folder `MonacoEditorSource`. The `index.html` is **required** for the project
4. copy all new files from the extracted archive to the folder `MonacoEditorSource`