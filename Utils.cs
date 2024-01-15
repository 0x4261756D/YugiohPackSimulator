using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

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
		public string? rarity = null;
	}
	public class Pack(Card[] cards, string[] rarities, Slot[] slots, string? defaultRarity)
	{
		public Card[] cards = cards;
		public string[] rarities = rarities;
		public Slot[] slots = slots;
		public string? defaultRarity = defaultRarity;
	}
	public class Slot(string primaryRarity, string secondaryRarity, int secondaryRarityFrequency)
	{
		public string primaryRarity = primaryRarity;
		public string secondaryRarity = secondaryRarity;
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

	public static async Task SaveFileAtSelectedLocationAsync(byte[] content, Window window, string defaultExtension, string title = "Select file")
	{
		TopLevel? topLevel = TopLevel.GetTopLevel(window);
		if(topLevel == null) return;
		IStorageFile? file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
		{
			DefaultExtension = defaultExtension,
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
	public static void ShowCard(Image hoveredImageBox, Utils.Card card)
	{
		foreach(string ending in Utils.ENDINGS)
		{
			string imagePathText = Path.Combine(Program.config.image_path, $"{card.id}{ending}");
			if(File.Exists(imagePathText))
			{
				hoveredImageBox.Source = new Bitmap(imagePathText);
				return;
			}
		}
		foreach(string ending in Utils.ENDINGS)
		{
			string imagePathText = Path.Combine(Program.config.image_path, $"{card.id}{ending}");
			hoveredImageBox.Source = null;
			Task.Run(async () =>
			{
				using HttpClient client = new();
				foreach(string baseUrl in Program.config.image_urls)
				{
					string url = $"{baseUrl}{card.id}{ending}";
					HttpResponseMessage response = await client.GetAsync(url);
					if(response.StatusCode == HttpStatusCode.OK)
					{
						await File.WriteAllBytesAsync(imagePathText, await response.Content.ReadAsByteArrayAsync());
						await Dispatcher.UIThread.InvokeAsync(() => hoveredImageBox.Source = new Bitmap(imagePathText));
					}
				}
			});
		}
	}

}
