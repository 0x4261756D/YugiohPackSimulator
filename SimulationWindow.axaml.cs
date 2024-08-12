using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace YugiohPackSimulator;

public partial class SimulationWindow : Window
{
	private readonly int amount;
	private int progress;
	private readonly List<Utils.Card>[] cardpoolByRarity;
	private readonly Utils.Pack pack;
	private readonly List<Utils.Card> cards;
	private readonly ReadOnlyDictionary<string, int> rarityIndices;
	private readonly int[] rarityProgresses;
	private readonly Random random;
	public SimulationWindow(Utils.Pack pack, ReadOnlyDictionary<string, int> rarityIndices, List<Utils.Card>[] cardpoolByRarity, int amount)
	{
		InitializeComponent();
		this.pack = pack;
		this.amount = amount;
		random = new Random();
		cards = [];
		rarityProgresses = new int[pack.rarities.Length];
		this.rarityIndices = rarityIndices;
		this.cardpoolByRarity = cardpoolByRarity;
		SimulateNextPack();
		LayoutUpdated += InitializeSizes;
	}

	private void InitializeSizes(object? sender, EventArgs e)
	{
		foreach(Control control in packPanel.Children)
		{
			Button button = (Button)((Panel)control).Children[0];
			button.Width = Bounds.Height / 5;
			button.Height = Bounds.Width / 5;
		}
		LayoutUpdated -= InitializeSizes;
	}

	private void SimulateNextPack()
	{
		progressBlock.Text = $"{progress + 1}/{amount}";
		packPanel.Children.Clear();
		foreach(Utils.Slot slot in pack.slots)
		{
			string primaryRarity = slot.primaryRarity ?? pack.defaultRarity!;
			Utils.Card card = cardpoolByRarity[rarityIndices[primaryRarity]][rarityProgresses[rarityIndices[primaryRarity]]];
			if(random.Next(slot.secondaryRarityFrequency) == 1)
			{
				string secondaryRarity = slot.secondaryRarity ?? pack.defaultRarity!;
				card = cardpoolByRarity[rarityIndices[secondaryRarity]][rarityProgresses[rarityIndices[secondaryRarity]]];
				rarityProgresses[rarityIndices[secondaryRarity]] += 1;
			}
			else
			{
				rarityProgresses[rarityIndices[primaryRarity]] += 1;
			}
			Panel panel = new()
			{
				DataContext = card,
			};
			Button button = new()
			{
				Width = this.Height / 5,
				Height = this.Width / 5,
			};
			button.Click += (_, _) =>
			{
				panel.Children.Clear();
				Image img = new()
				{
					Width = this.Height / 5,
					Height = this.Width / 5,
				};
				panel.Children.Add(img);
				Utils.ShowCard(img, card);
			};
			panel.Children.Add(button);
			packPanel.Children.Add(panel);
			cards.Add(card);
		}
		progress += 1;
		if(progress >= amount)
		{
			nextButton.Content = "Save";
		}
	}

	public void BackClick(object? sender, RoutedEventArgs args)
	{
		new MainWindow().Show();
		Close();
	}

	public void ShowAllClick(object? sender, RoutedEventArgs args)
	{
		foreach(Control control in packPanel.Children)
		{
			Panel panel = (Panel)control;
			if(panel.Children[0] is Button)
			{
				panel.Children.Clear();
				Image img = new()
				{
					Width = this.Height / 5,
					Height = this.Width / 5,
				};
				panel.Children.Add(img);
				Utils.ShowCard(img, (Utils.Card)panel.DataContext!);
			}
		}
	}

	public async void NextClick(object? sender, RoutedEventArgs args)
	{
		if(progress >= amount)
		{
			StringBuilder builder = new();
			foreach(Utils.Card card in cards)
			{
				_ = builder.Append($"{card.id}\n");
			}
			await Utils.SaveFileAtSelectedLocationAsync(Encoding.UTF8.GetBytes(builder.ToString()), this, defaultExtension: "ydk").ConfigureAwait(true);
			new MainWindow().Show();
			Close();
		}
		else
		{
			SimulateNextPack();
		}
	}
}
