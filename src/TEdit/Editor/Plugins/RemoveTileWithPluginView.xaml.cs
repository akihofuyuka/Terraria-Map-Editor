using System;
using System.Windows;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Plugins;

/// <summary>
/// Interaction logic for RemoveTileWithPluginView.xaml
/// </summary>
public partial class RemoveTileWithPluginView : FluentWindow
{
    public string BlockToRemove { get; private set; }
    public string WallToRemove { get; private set; }
    public string SpriteToRemove { get; private set; }
    public RemoveTileWithPluginView()
    {
        InitializeComponent();
        // ExtendsContentIntoTitleBar causes SizeToContent to allocate non-client margin
        // without rendering into it, leaving white unpainted borders on right and bottom.
        // Size window from content desired size instead.
        var root = (FrameworkElement)Content;
        root.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var natural = root.DesiredSize;
        Width = Math.Min(Math.Max(natural.Width, MinWidth), SystemParameters.WorkArea.Width);
        Height = Math.Min(Math.Max(natural.Height, MinHeight), SystemParameters.WorkArea.Height);
    }

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void RemoveButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        BlockToRemove = BlockRemove.Text;
        WallToRemove = WallRemove.Text;
        Close();
    }
}
