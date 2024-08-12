using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace YugiohPackSimulator;

public partial class CreatePackWindow : Window
{
	private class LayoutSlotPanel : StackPanel
	{
		public ComboBox primaryRarityBox, secondaryRarityBox;
		public NumericUpDown secondaryFrequencyBox;
		public Button removeButton;

		public LayoutSlotPanel(ListBox raritiesPanel, string? primaryRarity, string? secondaryRarity, int secondaryFrequency)
		{
			Orientation = Orientation.Horizontal;
			string?[] rarities = new string?[raritiesPanel.ItemCount];
			for(int i = 0; i < rarities.Length; i++)
			{
				RarityPanel c = (RarityPanel)raritiesPanel.Items[i]!;
				rarities[i] = c.nameBox.Text;
			}
			primaryRarityBox = new()
			{
				ItemsSource = rarities,
				SelectedItem = primaryRarity,
			};
			primaryRarityBox.DropDownOpened += (sender, _) =>
			{
				if(sender is null)
				{
					return;
				}
				string?[] rarities = new string?[raritiesPanel.ItemCount];
				for(int i = 0; i < rarities.Length; i++)
				{
					RarityPanel c = (RarityPanel)raritiesPanel.Items[i]!;
					rarities[i] = c.nameBox.Text;
				}
				((ComboBox)sender).ItemsSource = rarities;
			};
			Children.Add(primaryRarityBox);
			secondaryRarityBox = new()
			{
				ItemsSource = rarities,
				SelectedItem = secondaryRarity,
			};
			secondaryRarityBox.DropDownOpened += (sender, _) =>
			{
				if(sender is null)
				{
					return;
				}
				string?[] rarities = new string?[raritiesPanel.ItemCount];
				for(int i = 0; i < rarities.Length; i++)
				{
					RarityPanel c = (RarityPanel)raritiesPanel.Items[i]!;
					rarities[i] = c.nameBox.Text;
				}
				((ComboBox)sender).ItemsSource = rarities;
			};
			Children.Add(secondaryRarityBox);
			secondaryFrequencyBox = new()
			{
				Text = "1 in X has the secondary rare",
				Minimum = 0,
				Value = secondaryFrequency,
				FormatString = "0"
			};
			Children.Add(secondaryFrequencyBox);
			removeButton = new()
			{
				Content = "-",
			};
			removeButton.Click += (_, _) =>
			{
				_ = ((ListBox)Parent!).Items.Remove(this);
			};
			Children.Add(removeButton);
		}
	}
	private class RarityPanel : StackPanel
	{
		public TextBox nameBox;
		public Button removeButton;
		public RadioButton isDefaultButton;

		public RarityPanel(string text, ListBox packLayoutPanel, bool isDefaultRarity)
		{
			Orientation = Orientation.Horizontal;
			isDefaultButton = new()
			{
				Content = "Default",
				GroupName = "isDefaultRarity",
				IsChecked = isDefaultRarity,
			};
			Children.Add(isDefaultButton);
			nameBox = new()
			{
				Watermark = "Name",
				Text = text,
			};
			Children.Add(nameBox);
			removeButton = new()
			{
				Content = "-",
			};
			removeButton.Click += (_, _) =>
			{
				_ = ((ListBox)Parent!).Items.Remove(this);
				for(int i = packLayoutPanel.ItemCount - 1; i >= 0; i--)
				{
					LayoutSlotPanel layoutSlot = (LayoutSlotPanel)packLayoutPanel.Items[i]!;
					string primaryRarity = (string)layoutSlot.primaryRarityBox.SelectedValue!;
					string secondaryRarity = (string)layoutSlot.secondaryRarityBox.SelectedValue!;
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
			Children.Add(removeButton);
		}
	}
	private class PackCardPanel : StackPanel
	{
		public TextBlock nameBlock;
		public ComboBox rarityBox;
		public Utils.Card card;
		public Button removeButton;

		public PackCardPanel(Utils.Card card, Image hoveredImageBox, string? defaultRarity, ListBox raritiesPanel)
		{
			Orientation = Orientation.Horizontal;
			this.card = card;
			nameBlock = new()
			{
				Text = card.name
			};
			nameBlock.PointerEntered += (_, _) => Utils.ShowCard(hoveredImageBox, card);
			Children.Add(nameBlock);
			string?[] rarities = new string?[raritiesPanel.ItemCount];
			for(int i = 0; i < rarities.Length; i++)
			{
				RarityPanel c = (RarityPanel)raritiesPanel.Items[i]!;
				rarities[i] = c.nameBox.Text;
			}
			rarityBox = new()
			{
				ItemsSource = rarities,
				SelectedItem = card.rarity ?? defaultRarity
			};
			rarityBox.DropDownOpened += (sender, _) =>
			{
				if(sender is null)
				{
					return;
				}
				string?[] rarities = new string?[raritiesPanel.ItemCount];
				for(int i = 0; i < rarities.Length; i++)
				{
					RarityPanel c = (RarityPanel)raritiesPanel.Items[i]!;
					rarities[i] = c.nameBox.Text;
				}
				((ComboBox)sender).ItemsSource = rarities;
			};
			Children.Add(rarityBox);
			removeButton = new()
			{
				Content = "-"
			};
			removeButton.Click += (_, _) =>
			{
				_ = ((ListBox)Parent!).Items.Remove(this);
			};
			Children.Add(removeButton);
		}
	}

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
					Text = card.name,
				};
				block.PointerEntered += (_, _) => Utils.ShowCard(hoveredImageBox, card);
				items.Add(block);
			}
		}
		allCardsBox.ItemsSource = items;
	}



	public void BackClick(object? sender, RoutedEventArgs args)
	{
		new MainWindow().Show();
		Close();
	}

	public void AddRarityClick(object? sender, RoutedEventArgs args)
	{
		AddRarity("", isDefaultRarity: false, append: false);
	}
	private void AddRarity(string text, bool isDefaultRarity, bool append)
	{
		RarityPanel panel = new(text, packLayoutPanel, isDefaultRarity);
		if(append)
		{
			_ = raritiesPanel.Items.Add(panel);
		}
		else
		{
			raritiesPanel.Items.Insert(0, panel);
		}
	}

	public void AddLayoutSlotClick(object? sender, RoutedEventArgs args)
	{
		AddLayoutSlot(null, null, 0, GetDefaultRarity(), append: false);
	}

	private string? GetDefaultRarity()
	{
		foreach(object? c in raritiesPanel.Items)
		{
			RarityPanel? p = (RarityPanel?)c;
			if(p != null && (p.isDefaultButton.IsChecked ?? false))
			{
				return p.nameBox.Text;
			}
		}
		return null;
	}

	public void AddLayoutSlot(string? primaryRarity, string? secondaryRarity, int secondaryFrequency, string? defaultRarity, bool append)
	{
		if(raritiesPanel.ItemCount == 0)
		{
			return;
		}
		LayoutSlotPanel panel = new(raritiesPanel, primaryRarity: primaryRarity ?? defaultRarity, secondaryFrequency: secondaryFrequency, secondaryRarity: secondaryRarity ?? defaultRarity);
		if(append)
		{
			_ = packLayoutPanel.Items.Add(panel);
		}
		else
		{
			packLayoutPanel.Items.Insert(0, panel);
		}
	}

	public async void LoadClick(object? sender, RoutedEventArgs args)
	{
		string? path = await Utils.SelectFileAsync(this).ConfigureAwait(true);
		if(path == null || !File.Exists(path))
		{
			return;
		}
		Utils.Pack pack = JsonSerializer.Deserialize<Utils.Pack>(File.ReadAllBytes(path), Utils.jsonIncludeOption)!;
		raritiesPanel.Items.Clear();
		foreach(string rarity in pack.rarities)
		{
			AddRarity(append: true, text: rarity, isDefaultRarity: rarity == pack.defaultRarity);
		}
		packBox.Items.Clear();
		foreach(Utils.Card card in pack.cards)
		{
			_ = packBox.Items.Add(new PackCardPanel(card, hoveredImageBox, defaultRarity: pack.defaultRarity, raritiesPanel: raritiesPanel));
		}
		packLayoutPanel.Items.Clear();
		foreach(Utils.Slot slot in pack.slots)
		{
			AddLayoutSlot(append: true, primaryRarity: slot.primaryRarity, secondaryRarity: slot.secondaryRarity, secondaryFrequency: slot.secondaryRarityFrequency, defaultRarity: pack.defaultRarity);
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
		string[] rarities = new string[raritiesPanel.ItemCount];
		string? defaultRarity = null;
		for(int i = 0; i < rarities.Length; i++)
		{
			RarityPanel panel = (RarityPanel)raritiesPanel.Items[i]!;
			rarities[i] = panel.nameBox.Text!;
			if(panel.isDefaultButton.IsChecked ?? false)
			{
				defaultRarity = rarities[i];
			}
		}
		Utils.Card[] cards = new Utils.Card[packBox.ItemCount];
		for(int i = 0; i < packBox.ItemCount; i++)
		{
			PackCardPanel p = (PackCardPanel)packBox.Items[i]!;
			p.card.rarity = ((string?)p.rarityBox.SelectedItem) ?? defaultRarity;
			cards[i] = p.card;
		}
		Utils.Slot[] slots = new Utils.Slot[packLayoutPanel.ItemCount];
		for(int i = 0; i < slots.Length; i++)
		{
			LayoutSlotPanel p = (LayoutSlotPanel)packLayoutPanel.Items[i]!;
			slots[i] = new(
				primaryRarity: (string)p.primaryRarityBox.SelectedValue!,
				secondaryRarity: (string)p.secondaryRarityBox.SelectedValue!,
				secondaryRarityFrequency: (int)p.secondaryFrequencyBox.Value!);
		}
		Utils.Pack pack = new(cards: cards, rarities: rarities, slots: slots, defaultRarity: defaultRarity);
		await Utils.SaveFileAtSelectedLocationAsync(JsonSerializer.SerializeToUtf8Bytes(pack, Utils.jsonPrettyOption), this, defaultExtension: "json").ConfigureAwait(false);
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
			if(c != null && ((PackCardPanel)c).nameBlock.Text == block.Text)
			{
				return;
			}
		}
		_ = packBox.Items.Add(new PackCardPanel((Utils.Card)block.DataContext!, hoveredImageBox, defaultRarity: GetDefaultRarity(), raritiesPanel: raritiesPanel));
		packBox.SelectedIndex = packBox.ItemCount - 1;
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
