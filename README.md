**Moved to [Codeberg](https://codeberg.org/0x4261756D/YugiohPackSimulator)**

# YugiohPackSimulator

## Images
* By default no image urls are provided
* When hovering over a card in the pack creator or revealing a card in the simulator the image gets loaded from the `image_path` folder if they exist already or downloaded there from any of the `image_urls` by appending the card's id and either `.jpg` or `.png` to them

## Pack creator
* Reads card infos from all `.cdb` files in `database_root_path` and all its subfolders
* The list of all cards can be filtered, case is ignored, name and description are considered
* A card's rare is either the topmost one or the one specified next to its name
* Each slot in the pack has a primary rare and a possible secondary rare
	* If the secondary rare frequency `f` is non-zero roughly 1 in `f` cards in that slot will be of the secondary rare instead
* Packs are stored as json files

## Simulator
* After finishing the simulation the results get saved as `.ydk` files and can be opened as decks by [EDOPro](https://projectignis.github.io/)
