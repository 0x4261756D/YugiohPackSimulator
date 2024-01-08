using System.Collections.Generic;
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
		imageUrlBox.Text = string.Join('\n', Program.config!.image_urls);
		Closed += (_, _) =>
		{
			Program.config!.database_root_path = databaseRootPathBox.Text;
			Program.config!.image_path = imagePathBox.Text;
			Program.config!.image_urls = new List<string>(imageUrlBox.Text.Split('\n'));
		};
	}

	public void ToCreatePackClick(object? sender, RoutedEventArgs args)
	{
		new CreatePackWindow().Show();
		Close();
	}
	public void ToConfigSimulationClick(object? sender, RoutedEventArgs args)
	{
		new ConfigSimulationWindow().Show();
		Close();
	}
}