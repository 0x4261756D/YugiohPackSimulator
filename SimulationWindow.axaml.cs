using System;
using System.Collections.Generic;
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
	private readonly int[] rarityProgresses;
	private readonly Random random;
	public SimulationWindow(Utils.Pack pack, int amount)
	{
		InitializeComponent();
		this.pack = pack;
		this.amount = amount;
		random = new Random();
		cards = [];
		rarityProgresses = new int[pack.rarities.Length];
		cardpoolByRarity = new List<Utils.Card>[pack.rarities.Length];
		// TODO: Calculate the value, don't just guess
		int multiplicity = 3;
		for(int i = 0; i < pack.cards.Length; i++)
		{
			cardpoolByRarity[pack.cards[i].rarityIndex] ??= [];
			for(int j = 0; j < multiplicity; j++)
			{
				cardpoolByRarity[pack.cards[i].rarityIndex].Insert(random.Next(cardpoolByRarity[pack.cards[i].rarityIndex].Count), pack.cards[i]);
			}
		}
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
			Utils.Card card = cardpoolByRarity[slot.primaryRarityIndex][rarityProgresses[slot.primaryRarityIndex]];
			if(random.Next(slot.secondaryRarityFrequency) == 1)
			{
				card = cardpoolByRarity[slot.secondaryRarityIndex][rarityProgresses[slot.secondaryRarityIndex]];
				rarityProgresses[slot.secondaryRarityIndex] += 1;
			}
			else
			{
				rarityProgresses[slot.primaryRarityIndex] += 1;
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
				builder.Append($"{card.id}\n");
			}
			await Utils.SaveFileAtSelectedLocationAsync(Encoding.UTF8.GetBytes(builder.ToString()), this, defaultExtension: "ydk");
			new MainWindow().Show();
			Close();
		}
		else
		{
			SimulateNextPack();
		}
	}
}