using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		pathBox.Text = await Utils.SelectFileAsync(this).ConfigureAwait(true);
	}
	public void StartClick(object? sender, RoutedEventArgs args)
	{
		if(!File.Exists(pathBox.Text))
		{
			return;
		}
		Utils.Pack pack = JsonSerializer.Deserialize<Utils.Pack>(File.ReadAllBytes(pathBox.Text), Utils.jsonIncludeOption)!;
		Dictionary<string, int> indices = [];
		for(int i = 0; i < pack.rarities.Length; i++)
		{
			indices[pack.rarities[i]] = i;
		}
		ReadOnlyDictionary<string, int> rarityIndices = new(indices);
		List<Utils.Card>[] cardpoolByRarity = new List<Utils.Card>[pack.rarities.Length];
		Random random = new();
		// TODO: Calculate the value, don't just guess
		int multiplicity = 3;
		for(int i = 0; i < pack.cards.Length; i++)
		{
			string rarityName = (pack.cards[i].rarity ?? pack.defaultRarity) ?? "";
			if(!rarityIndices.TryGetValue(rarityName, out int value))
			{
				new Flyout()
				{
					Content = new TextBlock
					{
						Text = $"The rarity '{rarityName}' of {pack.cards[i].name} does not exist",
					}
				}.ShowAt(this, true);
				return;
			}
			int rarityIndex = value;
			cardpoolByRarity[rarityIndex] ??= [];
			for(int j = 0; j < multiplicity; j++)
			{
				cardpoolByRarity[rarityIndex].Insert(random.Next(cardpoolByRarity[rarityIndex].Count), pack.cards[i]);
			}
		}
		SimulationWindow w = new(pack, rarityIndices, cardpoolByRarity, (int)amountBox.Value!);
		if(w.IsEnabled)
		{
			w.Show();
			Close();
		}
	}
}
