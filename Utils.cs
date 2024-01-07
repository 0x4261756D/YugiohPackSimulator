using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

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
		public int rarityIndex;
	}
	public class Pack(Card[] cards, string[] rarities, Slot[] slots)
	{
		public Card[] cards = cards;
		public string[] rarities = rarities;
		public Slot[] slots = slots;
	}

	public class Slot(int primaryRarityIndex, int secondaryRarityIndex, int secondaryRarityFrequency)
	{
		public int primaryRarityIndex = primaryRarityIndex;
		public int secondaryRarityIndex = secondaryRarityIndex;
		public int secondaryRarityFrequency = secondaryRarityFrequency;
	}

	public static async Task<string?> SelectFileAsync(Window window, string title = "Select file", bool allowMultiple = false)
	{
		TopLevel? topLevel = TopLevel.GetTopLevel(window);
		if(topLevel == null) return null;
		IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
		{
			Title = title,
			AllowMultiple = allowMultiple,
		});
		if(files.Count > 0)
		{
			return files[0].Path.AbsolutePath;
		}
		return null;
	}

	public static async Task SaveFileAtSelectedLocationAsync(byte[] content, Window window, string title = "Select file")
	{
		TopLevel? topLevel = TopLevel.GetTopLevel(window);
		if(topLevel == null) return;
		IStorageFile? file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
		{
			DefaultExtension = "json",
			ShowOverwritePrompt = true,
			Title = title
		});
		if(file == null)
		{
			return;
		}
		using Stream stream = await file.OpenWriteAsync();
		await stream.WriteAsync(buffer: content);
	}

}
