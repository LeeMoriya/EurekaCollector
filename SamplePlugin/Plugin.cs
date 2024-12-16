using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using EurekaCollector.Windows;
using FFXIVClientStructs.FFXIV.Client.Game;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using static EurekaCollector.Windows.EurekaData;
using Dalamud.Game.Inventory;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace EurekaCollector;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;
    [PluginService] public static IGameInventory InventoryManager { get; private set; } = null!;
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;




    private const string CommandName = "/eurekacollector";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("EurekaCollector");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public static Dalamud.Logging.Internal.ModuleLog log = new Dalamud.Logging.Internal.ModuleLog("EurekaCollector");

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the Eureka Collector interface"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "InventoryGrid0E", CheckPlayerInventory);
        AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "RetainerGrid0", CheckRetainerInventory);
        AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "InventoryBuddy", CheckChocoboSaddlebagInventory);
    }

    public unsafe void CheckPlayerInventory(AddonEvent type, AddonArgs args)
    {
        log.Information("Check Player Inventory");
        List<uint> itemIds = new List<uint>();

        var inventories = new List<GameInventoryType>
        {
            GameInventoryType.ArmoryMainHand,
            GameInventoryType.ArmoryOffHand,

            GameInventoryType.Inventory1,
            GameInventoryType.Inventory2,
            GameInventoryType.Inventory3,
            GameInventoryType.Inventory4,

            GameInventoryType.ArmoryHead,
            GameInventoryType.ArmoryBody,
            GameInventoryType.ArmoryHands,
            GameInventoryType.ArmoryLegs,
            GameInventoryType.ArmoryFeets,
            GameInventoryType.ArmoryEar,
            GameInventoryType.ArmoryNeck,
            GameInventoryType.ArmoryWrist,
            GameInventoryType.ArmoryRings,

            GameInventoryType.EquippedItems,
        };

        foreach (var inv in inventories)
        {
            var items = InventoryManager.GetInventoryItems(inv);

            for (var index = 0; index < items.Length; index++)
            {
                var item = items[index];
                itemIds.Add(item.ItemId);
            }
        }

        foreach (var kvp in weaponTracker.Keys)
        {
            foreach (var item in weaponTracker[kvp])
            {
                for (int i = 0; i < itemIds.Count; i++)
                {
                    if (itemIds[i] == item.itemId)
                    {
                        item.obtained = true;
                    }
                }
            }
        }
        UpdateWeaponCollection();
    }

    public unsafe void CheckChocoboSaddlebagInventory(AddonEvent type, AddonArgs args)
    {
        log.Information("Check Saddlebags");
        List<uint> itemIds = new List<uint>();

        for (int i = 0; i < 2; i++)
        {
            var saddleBagTypes = new[]
            {
                $"SaddleBag{i + 1}",
                $"PremiumSaddleBag{i + 1}"
            };

            foreach (var bagType in saddleBagTypes)
            {
                var items = InventoryManager.GetInventoryItems((GameInventoryType)Enum.Parse(typeof(GameInventoryType), bagType));

                foreach (var item in items)
                {
                    itemIds.Add(item.ItemId);
                }
            }
        }

        foreach (var kvp in weaponTracker.Keys)
        {
            foreach (var item in weaponTracker[kvp])
            {
                for (int i = 0; i < itemIds.Count; i++)
                {
                    if (itemIds[i] == item.itemId)
                    {
                        item.obtained = true;
                    }
                }
            }
        }
        UpdateWeaponCollection();
    }

    public unsafe void CheckRetainerInventory(AddonEvent type, AddonArgs args)
    {
        log.Information("Check Retainer");
        List<uint> itemIds = new List<uint>();

        for (int i = 0; i < 5; i++)
        {
            var items = InventoryManager.GetInventoryItems((GameInventoryType)Enum.Parse(typeof(GameInventoryType), $"RetainerPage{i + 1}"));
            for (var index = 0; index < items.Length; index++)
            {
                var item = items[index];
                itemIds.Add(item.ItemId);
            }
        }

        foreach (var kvp in weaponTracker.Keys)
        {
            foreach (var item in weaponTracker[kvp])
            {
                for (int i = 0; i < itemIds.Count; i++)
                {
                    if (itemIds[i] == item.itemId)
                    {
                        item.obtained = true;
                    }
                }
            }
        }
        UpdateWeaponCollection();
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
        MainWindow.Dispose();
        CommandManager.RemoveHandler(CommandName);

        AddonLifecycle.UnregisterListener(AddonEvent.PreRequestedUpdate, "InventoryGrid0E", CheckRetainerInventory);
        AddonLifecycle.UnregisterListener(AddonEvent.PreRequestedUpdate, "RetainerGrid0", CheckRetainerInventory);
        AddonLifecycle.UnregisterListener(AddonEvent.PreRequestedUpdate, "InventoryBuddy", CheckRetainerInventory);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
