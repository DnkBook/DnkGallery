// Non-Uno global usings that would be generated if ImplicitUsings was enabled in the project file:
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;


// Markup extension methods:
global using CSharpMarkup.WinUI;
global using DnkGallery.Presentation.Core;
global using DnkGallery.Presentation.Utils;
global using Uno.Extensions.Reactive;

// Markup helpers:
global using static CSharpMarkup.WinUI.Uno.Toolkit.Helpers;
global using static CSharpMarkup.WinUI.Helpers;
global using static DnkGallery.Presentation.Core.MarkupHelpers;

// Aliases for WinUI namespaces and types
// - Use to avoid including WinUI namespaces, which can cause ambiguities with the CSharpMarkup namespaces
// - Prefix namespaces and view type aliases with "UI"
global using UI = Microsoft.UI;
global using UIBindable = Microsoft.UI.Xaml.Data.BindableAttribute;
global using UIControls = Microsoft.UI.Xaml.Controls;
// - Non-view types, e.g. enums, don't need a UI prefix because they are not mirrored as types in the CSharpMarkup namespaces
global using BindingMode = Microsoft.UI.Xaml.Data.BindingMode;
global using Visibility = Microsoft.UI.Xaml.Visibility;

global using System.Collections.ObjectModel;
