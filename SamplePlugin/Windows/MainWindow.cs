using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using static EurekaCollector.Windows.EurekaData;

namespace EurekaCollector.Windows;


public class MainWindow : Window, IDisposable
{
    public bool youGotIt = false;
    private Plugin Plugin;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, string goatImagePath)
        : base("Eureka Collector##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(600, 440),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
    }

    public void Dispose() { }
    
    public override void Draw()
    {
        ImGui.PushStyleColor(ImGuiCol.TabActive, new Vector4(0.4f, 0.4f, 0.4f, 1f));
        ImGui.PushStyleColor(ImGuiCol.TabUnfocusedActive, new Vector4(0.4f, 0.4f, 0.4f, 1f));
        if (ImGui.BeginTabBar("TabBar"))
        {
            // Weapons Tab
            if (ImGui.BeginTabItem("Weapons"))
            {
                ImGui.Text("Open the inventories where your weapons are stored to update the collection");
                if (ImGui.BeginTabBar("WeaponsTabBar"))
                {
                    ImGui.PopStyleColor(2);
                    var itemSheet = Plugin.DataManager.GetExcelSheet<Item>();

                    for (int i = 0; i < weaponTracker.Count; i++)
                    {
                        var weaponEntry = weaponTracker.ElementAt(i);

                        ImGui.PushStyleColor(ImGuiCol.Tab, GetJobColor(i, false));
                        ImGui.PushStyleColor(ImGuiCol.TabActive, GetJobColor(i, true));
                        ImGui.PushStyleColor(ImGuiCol.TabHovered, GetJobColor(i, true));
                        ImGui.PushStyleColor(ImGuiCol.MenuBarBg, GetJobColor(i, true));

                        if (ImGui.BeginTabItem(weaponEntry.Key))
                        {
                            for (int s = 0; s < weaponEntry.Value.Count; s++)
                            {
                                var weaponInfo = weaponEntry.Value[s];

                                if (weaponEntry.Value.Count > 2)
                                {
                                    var itemRow = itemSheet.GetRow((uint)weaponInfo.itemId);
                                    string itemName = itemRow.Singular.ExtractText();

                                    ImGui.PushStyleColor(ImGuiCol.Text, weaponInfo.obtained ? new Vector4(0.2f, 1f, 0.2f, 1f) : new Vector4(0.9f, 0.1f, 0.1f, 1f));
                                    ImGui.Text(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(itemName.ToLower()));
                                    ImGui.PopStyleColor();
                                }
                            }
                            ImGui.EndTabItem();
                        }

                        ImGui.PopStyleColor(4);
                    }

                    ImGui.EndTabBar();
                }

            }
            if (ImGui.BeginTabItem("Armor"))
            {
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Logograms"))
            {
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Magicite"))
            {
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Items"))
            {
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Settings"))
            {
                ImGui.EndTabItem();
            }

            // End the tab bar
            ImGui.EndTabBar();
        }
    }
}
public static class EurekaData
{
    public static Vector4 GetJobColor(int index, bool active)
    {
        float alpha = active ? 1f : 0.5f;
        if (index < 3)
        {
            return new Vector4(0.2f, 0.4f, 0.8f, alpha);
        }
        else if (index < 6)
        {
            return new Vector4(0.2f, 0.7f, 0.2f, alpha);
        }
        else
        {
            return new Vector4(0.8f, 0.2f, 0.2f, alpha);
        }
    }

    public static string[] validJobs =
    [
        //Tanks
        "WAR",
            "PLD",
            "DRK",
            //Healers
            "WHM",
            "SCH",
            "AST",
            //Melee
            "DRG",
            "MNK",
            "NIN",
            "SAM",
            //Phys Range
            "BRD",
            "MCH",
            //Magic Range
            "BLM",
            "SMN",
            "RDM"
    ];

    public static Dictionary<string, int[]> relicIds = new Dictionary<string, int[]>()
        {
            //Antiquated, Base, +1, +2, Anemos, Pagos, +1, Elemental, +1, +2, Pyros, Hydatos, +1 Base-Eureka, Eureka, Physeos
            {"WAR", new []{ 17819, 21944, 21960, 21976, 21992, 22927, 22943, 22959, 24041, 24057, 24073, 24645, 24661, 24677, 24693, 24709 } },
            {"PLD", new []{ 17817, 21942, 21958, 21974, 21990, 22925, 22941, 22957, 24039, 24055, 24071, 24643, 24659, 24675, 24691, 24707 } },
            {"DRK", new []{ 17823, 21948, 21964, 21980, 21996, 22931, 22947, 22963, 24045, 24061, 24077, 24649, 24665, 24681, 24697, 24713 } },

            {"WHM", new []{ 17825, 21950, 21966, 21982, 21998, 22933, 22949, 22965, 24047, 24063, 24079, 24651, 24667, 24683, 24699, 24715 } },
            {"SCH", new []{ 17828, 21953, 21969, 21985, 22001, 22936, 22952, 22968, 24050, 24066, 24082, 24654, 24670, 24686, 24702, 24718 } },
            {"AST", new []{ 17829, 21954, 21970, 21986, 22002, 22937, 22953, 22969, 24051, 24067, 24083, 24655, 24671, 24687, 24703, 24719 } },

            {"DRG", new []{ 17820, 21945, 21961, 21977, 21993, 22928, 22944, 22960, 24042, 24058, 24074, 24646, 24662, 24678, 24694, 24710 } },
            {"MNK", new []{ 17818, 21943, 21959, 21975, 21991, 22926, 22942, 22958, 24040, 24056, 24072, 24644, 24660, 24676, 24692, 24708 } },
            {"NIN", new []{ 17822, 21947, 21963, 21979, 21995, 22930, 22946, 22962, 24044, 24060, 24076, 24648, 24664, 24680, 24696, 24712 } },
            {"SAM", new []{ 17830, 21955, 21971, 21987, 22003, 22938, 22954, 22970, 24052, 24068, 24084, 24656, 24672, 24688, 24704, 24720 } },

            {"BRD", new []{ 17821, 21946, 21962, 21978, 21994, 22929, 22945, 22961, 24043, 24059, 24075, 24647, 24663, 24679, 24695, 24711 } },
            {"MCH", new []{ 17824, 21949, 21965, 21981, 21997, 22932, 22948, 22964, 24046, 24062, 24078, 24650, 24666, 24682, 24698, 24714 } },

            {"BLM", new []{ 17826, 21951, 21967, 21983, 21999, 22934, 22950, 22966, 24048, 24064, 24080, 24652, 24668, 24684, 24700, 24716 } },
            {"SMN", new []{ 17827, 21952, 21968, 21984, 22000, 22935, 22951, 22967, 24049, 24065, 24081, 24653, 24669, 24685, 24701, 24717 } },
            {"RDM", new []{ 17831, 21956, 21972, 21988, 22004, 22939, 22955, 22971, 24053, 24069, 24085, 24657, 24673, 24689, 24705, 24721 } },

        };

    public static Dictionary<string, List<RelicWeapon>> weaponTracker = GenerateRelicWeapons();

    public static Dictionary<string, List<RelicWeapon>> GenerateRelicWeapons()
    {
        var weaponTracker = new Dictionary<string, List<RelicWeapon>>();
        foreach (var kvp in relicIds)
        {
            List<RelicWeapon> weapons = new List<RelicWeapon>();
            for (int i = 0; i < kvp.Value.Length; i++)
            {
                RelicWeapon weapon = new RelicWeapon() { currentStep = i, itemId = kvp.Value[i] };
                weapons.Add(weapon);
            }
            weaponTracker.Add(kvp.Key, weapons);
        }
        return weaponTracker;
    }

    public static void UpdateWeaponCollection()
    {
        //If a relic was found, mark all previous steps as completed
        foreach (var kvp in weaponTracker)
        {
            bool progress = false;
            for (int i = weaponTracker.Values.Count; i >= 0; i--)
            {
                if (kvp.Value.ElementAt(i).obtained)
                {
                    progress = true;
                }
                else if (progress)
                {
                    kvp.Value.ElementAt(i).obtained = true;
                }
            }
        }
    }

    public class RelicWeapon()
    {
        public bool obtained;
        public int itemId;
        public int currentStep;
        public string name = "null";
    }
}
