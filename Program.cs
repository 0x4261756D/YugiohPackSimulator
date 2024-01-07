using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace YugiohPackSimulator;

class Program
{
	public static Utils.Config config = new("UNDEFINED", new(), "UNDEFINED");
	public static List<Utils.Card> allCards = new();

	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main(string[] args)
	{
		if(File.Exists(Utils.DEFAULT_CONFIG_PATH))
		{
			config = JsonSerializer.Deserialize<Utils.Config>(File.ReadAllText(Utils.DEFAULT_CONFIG_PATH), options: Utils.jsonIncludeOption) ?? config;
		}
		else
		{
			throw new Exception($"No config file found at {Path.GetFullPath(Utils.DEFAULT_CONFIG_PATH)}");
		}
		if(!Directory.Exists(config.database_root_path))
		{
			throw new Exception($"Database root path {Path.GetFullPath(config!.database_root_path)} does not exist");
		}
		PopulateDatabaseList(config.database_root_path);
		if(!Directory.Exists(config.image_path))
		{
			Directory.CreateDirectory(config!.image_path);
		}

		BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
	}

	private static void PopulateDatabaseList(string path)
	{
		foreach(string fileText in Directory.GetFiles(path))
		{
			if(Path.GetExtension(fileText) == ".cdb")
			{
				using SqliteConnection connection = new($"Data Source={fileText}");
				connection.Open();
				SqliteCommand command = connection.CreateCommand();
				command.CommandText = "SELECT datas.id, texts.name, texts.desc FROM datas, texts WHERE datas.id = texts.id AND (datas.ot <= 3 OR datas.ot = 32) AND NOT texts.name LIKE '% Token'";
				using SqliteDataReader reader = command.ExecuteReader();
				while(reader.Read())
				{
					allCards.Add(new(id: reader.GetInt32(0), name: reader.GetString(1), desc: reader.GetString(2)));
				}
			}
		}
		foreach(string directoryText in Directory.GetDirectories(path))
		{
			PopulateDatabaseList(directoryText);
		}
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace();

	internal static void Cleanup(object? sender, ShutdownRequestedEventArgs e)
	{
		File.WriteAllText(Utils.DEFAULT_CONFIG_PATH, JsonSerializer.Serialize(config, options: Utils.jsonPrettyOption));
	}
}
