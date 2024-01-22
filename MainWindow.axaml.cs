using System;
using System.Collections.Generic;
using System.IO;
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
		Closing += (_, _) => SaveConfig();
	}

	public void SaveConfig()
	{
		Program.config!.database_root_path = databaseRootPathBox.Text ?? "";
		Program.config!.image_path = imagePathBox.Text ?? "";
		Program.config!.image_urls = new List<string>((imageUrlBox.Text ?? "").Split(Environment.NewLine));
	}

	public void ToCreatePackClick(object? sender, RoutedEventArgs args)
	{
		SaveConfig();
		if(!Directory.Exists(Program.config!.database_root_path))
		{
			new Flyout()
			{
				Content = new TextBlock
				{
					Text = "The database root path does not exist",
				}
			}.ShowAt(this, true);
			return;
		}
		else if(Program.allCards.Count == 0)
		{
			Program.PopulateDatabaseList(Program.config!.database_root_path);
		}
		new CreatePackWindow().Show();
		Close();
	}
	public void ToConfigSimulationClick(object? sender, RoutedEventArgs args)
	{
		SaveConfig();
		new ConfigSimulationWindow().Show();
		Close();
	}
}
