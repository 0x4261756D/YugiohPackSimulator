using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace YugiohPackSimulator;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
		imagePathBox.Text = Program.config!.image_path;
		databaseRootPathBox.Text = Program.config!.database_root_path;
	}

	public void ToCreatePackClick(object? sender, RoutedEventArgs args)
	{
		new CreatePackWindow().Show();
		Close();
	}
}