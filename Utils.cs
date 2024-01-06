using System.Collections.Generic;
using System.Text.Json;

namespace YugiohPackSimulator;

public class Utils
{
	public const string DEFAULT_CONFIG_PATH = "./config.json";
	public static readonly string[] ENDINGS = [".jpg", ".png"];
	public static readonly JsonSerializerOptions jsonIncludeOption = new() { IncludeFields = true };
	public static readonly JsonSerializerOptions jsonPrettyOption = new()
	{
		WriteIndented = true,
		IncludeFields = true,
	};

	public class Config(string database_root_path, List<string> image_urls, string image_path)
	{
		public string database_root_path = database_root_path;
		public List<string> image_urls = image_urls;
		public string image_path = image_path;
	}

	public class Card(int id, string name, string desc)
	{
		public int id = id;
		public string name = name;
		public string desc = desc;
	}
}
