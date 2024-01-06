using System;
using Avalonia.Media.Imaging;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using System.Net.Http;
using Avalonia.Threading;
using System.Net;
using System.Collections.Generic;

namespace YugiohPackSimulator;

public partial class CreatePackWindow : Window
{
	public CreatePackWindow()
	{
		InitializeComponent();
		FilterCards("");
	}

	private void FilterCards(string filter)
	{
		List<TextBlock> items = [];
		foreach(Utils.Card card in Program.allCards)
		{
			if(card.name.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ||
				card.desc.Contains(filter, StringComparison.CurrentCultureIgnoreCase))
			{
				TextBlock block = new()
				{
					DataContext = card,
					Text = card.name
				};
				block.PointerEntered += ShowCard;
				items.Add(block);
			}
		}
		allCardsBox.ItemsSource = items;
	}

	private void ShowCard(object? sender, PointerEventArgs e)
	{
		if(sender == null)
		{
			return;
		}
		Utils.Card card = (Utils.Card)((Control)sender).DataContext!;
		// hoveredTextBox.Text = card.desc;
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

	public void BackClick(object? sender, RoutedEventArgs args)
	{
		new MainWindow().Show();
		Close();
	}

	public void InputKeyUp(object? sender, KeyEventArgs args)
	{
		if(sender == null)
		{
			return;
		}
		TextBox block = (TextBox)sender;
		FilterCards(block.Text ?? "");
	}
}