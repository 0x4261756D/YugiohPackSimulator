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
using Avalonia.Layout;
using System.Text.Json;
using System.Collections.Immutable;

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

	public void AddRarityClick(object? sender, RoutedEventArgs args)
	{
		AddRarity("");
	}

	private void AddRarity(string text)
	{
		StackPanel panel = new()
		{
			Orientation = Orientation.Horizontal,
		};
		TextBox nameBox = new()
		{
			Watermark = "Name",
			Text = text,
		};
		panel.Children.Add(nameBox);
		Button removeButton = new()
		{
			Content = "-",
		};
		removeButton.Click += (_, _) =>
		{
			raritiesPanel.Items.Remove(panel);
			for(int i = packLayoutPanel.ItemCount - 1; i >= 0; i--)
			{
				string primaryRarity = (string)((ComboBox)((StackPanel)packLayoutPanel.Items[i]!).Children[0]).SelectedValue!;
				string secondaryRarity = (string)((ComboBox)((StackPanel)packLayoutPanel.Items[i]!).Children[1]).SelectedValue!;
				if(primaryRarity == nameBox.Text)
				{
					packLayoutPanel.Items.RemoveAt(i);
				}
				else if(secondaryRarity == nameBox.Text)
				{
					packLayoutPanel.Items.RemoveAt(i);
				}
			}
		};
		panel.Children.Add(removeButton);
		raritiesPanel.Items.Insert(0, panel);
	}

	public void AddLayoutSlotClick(object? sender, RoutedEventArgs args)
	{
		AddLayoutSlot(0, 0, 0);
	}
	public void AddLayoutSlot(int primaryRarityIndex, int secondaryRarityIndex, int secondaryFrequency)
	{
		if(raritiesPanel.ItemCount == 0)
		{
			return;
		}
		StackPanel panel = new()
		{
			Orientation = Orientation.Horizontal,
		};
		string[] rarities = new string[raritiesPanel.ItemCount];
		for(int i = 0; i < rarities.Length; i++)
		{
			Control c = (Control)raritiesPanel.Items[i]!;
			rarities[i] = ((TextBox)((StackPanel)c).Children[0]).Text!;
		}
		ComboBox primaryRarityBox = new()
		{
			ItemsSource = rarities,
			SelectedIndex = primaryRarityIndex,
		};
		panel.Children.Add(primaryRarityBox);
		ComboBox secondaryRarityBox = new()
		{
			ItemsSource = rarities,
			SelectedIndex = secondaryRarityIndex,
		};
		panel.Children.Add(secondaryRarityBox);
		NumericUpDown secondaryFrequencyBox = new()
		{
			Text = "1 in X has the secondary rare",
			Minimum = 0,
			Value = secondaryFrequency,
			FormatString = "0"
		};
		panel.Children.Add(secondaryFrequencyBox);
		Button removeButton = new()
		{
			Content = "-",
		};
		removeButton.Click += (_, _) =>
		{
			packLayoutPanel.Items.Remove(panel);
		};
		panel.Children.Add(removeButton);
		packLayoutPanel.Items.Insert(0, panel);
	}

	public async void LoadClick(object? sender, RoutedEventArgs args)
	{
		string? path = await Utils.SelectFileAsync(this);
		if(path == null || !File.Exists(path))
		{
			return;
		}
		Utils.Pack pack = JsonSerializer.Deserialize<Utils.Pack>(File.ReadAllBytes(path), Utils.jsonIncludeOption)!;
		packBox.ItemsSource = Array.ConvertAll(pack.cards, (card) => new TextBlock
		{
			Text = card.name,
			DataContext = card,
		});
		foreach(string rarity in pack.rarities)
		{
			AddRarity(rarity);
		}
		foreach(Utils.Slot slot in pack.slots)
		{
			AddLayoutSlot(primaryRarityIndex: slot.primaryRarityIndex, secondaryRarityIndex: slot.secondaryRarityIndex, secondaryFrequency: slot.secondaryRarityFrequency);
		}
	}
	public async void SaveClick(object? sender, RoutedEventArgs args)
	{
		if(packBox.ItemCount == 0)
		{
			return;
		}
		if(raritiesPanel.ItemCount == 0)
		{
			return;
		}
		if(packLayoutPanel.ItemCount == 0)
		{
			return;
		}
		Utils.Card[] cards = new Utils.Card[packBox.ItemCount];
		for(int i = 0; i < packBox.ItemCount; i++)
		{
			cards[i] = (Utils.Card)((Control)packBox.ItemsView[i]!).DataContext!;
		}
		string[] rarities = new string[raritiesPanel.ItemCount];
		Dictionary<string, int> rarityIndices = [];
		for(int i = 0; i < rarities.Length; i++)
		{
			rarities[i] = ((TextBox)((StackPanel)raritiesPanel.Items[i]!).Children[0]).Text!;
			rarityIndices[rarities[i]] = i;
		}
		Utils.Slot[] slots = new Utils.Slot[packLayoutPanel.ItemCount];
		for(int i = 0; i < slots.Length; i++)
		{
			StackPanel p = (StackPanel)packLayoutPanel.Items[i]!;
			slots[i] = new(
				primaryRarityIndex: rarityIndices[(string)((ComboBox)p.Children[0]).SelectedValue!],
				secondaryRarityIndex: rarityIndices[(string)((ComboBox)p.Children[1]).SelectedValue!],
				secondaryRarityFrequency: (int)((NumericUpDown)p.Children[2]).Value!);
		}
		Utils.Pack pack = new(cards: cards, rarities: rarities, slots: slots);
		await Utils.SaveFileAtSelectedLocationAsync(JsonSerializer.SerializeToUtf8Bytes(pack, Utils.jsonPrettyOption), this);
	}

	public void AllCardsSelectionChanged(object? sender, SelectionChangedEventArgs args)
	{
		if(args.AddedItems.Count != 1 || args.RemovedItems.Count != 0)
		{
			return;
		}
		args.Handled = true;
		TextBlock block = (TextBlock)args.AddedItems[0]!;
		allCardsBox.SelectedItem = null;
		foreach(object? c in packBox.Items)
		{
			if(c != null && ((TextBlock)c).Text == block.Text)
			{
				return;
			}
		}
		packBox.Items.Add(new TextBlock
		{
			Text = block.Text,
			DataContext = block.DataContext,
		});
	}

	public void PackSelectionChanged(object? sender, SelectionChangedEventArgs args)
	{		
		if(args.AddedItems.Count != 1 || args.RemovedItems.Count != 0)
		{
			return;
		}
		args.Handled = true;
		TextBlock block = (TextBlock)args.AddedItems[0]!;
		packBox.SelectedItem = null;
		packBox.Items.Remove(block);
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