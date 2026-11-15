/*
Copyright (c) 2021 ReconditeDeity

This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 */

using System;
using System.Windows;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Plugins;

/// <summary>
/// Interaction logic for HouseGenPluginView.xaml
/// </summary>
public partial class HouseGenPluginView : FluentWindow
{
    public HouseGenPluginView()
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

    private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true; //Prevent window from being closed because we're going to hide it instead so that everything stays initialized for the remaining duration that the main application is running.
        Hide();
    }

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}
