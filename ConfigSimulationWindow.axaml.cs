using System.IO;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace YugiohPackSimulator;

public partial class ConfigSimulationWindow : Window
{
	public ConfigSimulationWindow()
	{
		InitializeComponent();
	}

	public void BackClick(object? sender, RoutedEventArgs args)
	{
		new MainWindow().Show();
		Close();
	}
	public async void SelectPackClick(object? sender, RoutedEventArgs args)
	{
		pathBox.Text = await Utils.SelectFileAsync(this);
	}
	public void StartClick(object? sender, RoutedEventArgs args)
	{
		if(!File.Exists(pathBox.Text))
		{
			return;
		}
		new SimulationWindow(JsonSerializer.Deserialize<Utils.Pack>(File.ReadAllBytes(pathBox.Text), Utils.jsonIncludeOption)!, (int)amountBox.Value!).Show();
		Close();
	}
}