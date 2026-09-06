# Shop system

Day loop part of the game: pick up loot outside, put it on the shelves, customers come in,
grab stuff, line up at the counter and the player rings them up.

Test scene: `Assets/Osama/Scenes/ShopTest.unity` (rebuild it any time with Tools > Shop > Build Test Scene).
TMP needs its essential resources imported once (Window > TextMeshPro > Import TMP Essential Resources)
or the price tags / HUD stay invisible.

## Pieces

| Script | What it does |
|---|---|
| `Data/ProductSO` | one asset per product: name, category, rarity, base price, display prefab |
| `Data/CustomerTypeSO` | one asset per customer kind: speed, how many items, patience, wanted products |
| `Data/ShopSettingsSO` | currency symbol, starting money, price multiplier per rarity |
| `ShopManager` | money, open/closed, finds a stocked slot for a customer. Singleton like `Movement` |
| `Player/PlayerInteraction` | raycast from the camera, calls `IInteractable.Interact()` once per press, feeds the prompt UI |
| `Player/PlayerCarry` | what the player holds (simple list). Swap for the inventory later, nothing else has to change |
| `World/PickupItem` | product on the ground |
| `World/Shelf` + `World/ShelfSlot` | slots hold one product each, show the model + a price tag |
| `World/CheckoutCounter` | the queue and the actual sale |
| `Customers/Customer` | state machine: enter -> shelf -> take -> checkout -> leave |
| `Customers/ICustomerMover` + `NavMeshCustomerMover` | movement, Customer only talks to the interface |
| `Customers/CustomerSpawner` | spawns customers while the shop is open |
| `UI/InteractionPromptUI`, `UI/ShopHUD` | prompt text, money, hands |

## Wiring in a real scene

- Player: add `PlayerInteraction` + `PlayerCarry` next to `Movement`, and in `PlayerInput` point the
  Interact action at `PlayerInteraction.OnInteract` instead of `Movement.OnInteract`.
- One `ShopManager` with the settings asset, one `CheckoutCounter` with queue points, shelves anywhere
  (they register themselves), a `CustomerSpawner` with spawn/entrance points, baked NavMesh.
- Price = `basePrice * rarityMultiplier`, see `ShopSettingsSO.GetSellPrice`.

## Where to plug things in later

- Day/night: `ShopManager.SetOpen(false)` stops new customers.
- Inventory: `PlayerCarry` is the only seam. `ProductSO` can link to `ItemSO` once that's merged.
- Special NPCs / quests / dialogue: `CustomerTypeSO.isSpecial` + `wantedProducts`, `Customer.OnStateChanged`.
- Smarter customers: `ShopManager.FindSlotFor`.
- Different movement: implement `ICustomerMover`.
