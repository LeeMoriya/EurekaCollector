using Dalamud.Interface.Textures;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using static EurekaCollector.Windows.EurekaData;
using System.IO;
using System.Reflection;
using Dalamud.Plugin;
using System.Net.Http.Json;

namespace EurekaCollector.Windows;


public class MainWindow : Window, IDisposable
{
    public bool youGotIt = false;
    public int jumpTo = -1;
    public float targetScrollY = -1.0f;
    public float currentScrollY = 0.0f;
    public float scrollSpeed = 7.0f;
    public string lastTab = "";

    private Plugin Plugin;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, string goatImagePath)
        : base("Eureka Collector##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(920, 610),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
        Load();
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.PushStyleColor(ImGuiCol.TabActive, new Vector4(0.4f, 0.4f, 0.4f, 1f));
        ImGui.PushStyleColor(ImGuiCol.TabUnfocusedActive, new Vector4(0.4f, 0.4f, 0.4f, 1f));

        if (ImGui.BeginTabBar("MainTabBar"))
        {
            // Weapons Tab
            if (ImGui.BeginTabItem("Weapons"))
            {
                ImGui.Text("Open the inventories where your weapons are stored to update the collection.");

                if (ImGui.BeginTabBar("WeaponsTabBar"))
                {
                    var itemSheet = Plugin.DataManager.GetExcelSheet<Item>();

                    for (int i = 0; i < weaponTracker.Count; i++)
                    {
                        var weaponEntry = weaponTracker.ElementAt(i);

                        // Push style for job-specific tabs
                        ImGui.PushStyleColor(ImGuiCol.Tab, GetJobColor(i, false));
                        ImGui.PushStyleColor(ImGuiCol.TabActive, GetJobColor(i, true));
                        ImGui.PushStyleColor(ImGuiCol.TabHovered, GetJobColor(i, true));
                        ImGui.PushStyleColor(ImGuiCol.MenuBarBg, GetJobColor(i, true));

                        string tabName = $"{weaponEntry.Key}{(weaponEntry.Value.Last().obtained ? "  ✓" : "")}";
                        if (ImGui.BeginTabItem(tabName))
                        {
                            if (lastTab != weaponEntry.Key)
                            {
                                jumpTo = -1;
                                currentScrollY = 0f;
                                targetScrollY = -1.0f;
                            }
                            lastTab = weaponEntry.Key;

                            ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.3f, 0.3f, 0.3f, 0.1f));
                            if (ImGui.BeginListBox($"##ListBox_{i}", new Vector2(300f, 500f)))
                            {
                                for (int s = 0; s < weaponEntry.Value.Count; s++)
                                {
                                    float centerPos = ImGui.GetColumnWidth() * 0.5f;
                                    if (s == 0) //Anemos
                                    {
                                        string zone = "Anemos";
                                        ImGui.Separator();
                                        ImGui.SetCursorPosX(centerPos - (ImGui.CalcTextSize(zone).X * 0.5f));
                                        ImGui.Text(zone);
                                        ImGui.Separator();
                                    }
                                    else if (s == 4) // Pagos
                                    {
                                        string zone = "Pagos";
                                        ImGui.Separator();
                                        ImGui.SetCursorPosX(centerPos - (ImGui.CalcTextSize(zone).X * 0.5f));
                                        ImGui.Text(zone);
                                        ImGui.Separator();
                                    }
                                    else if (s == 7) // Pyros
                                    {
                                        string zone = "Pyros";
                                        ImGui.Separator();
                                        ImGui.SetCursorPosX(centerPos - (ImGui.CalcTextSize(zone).X * 0.5f));
                                        ImGui.Text(zone);
                                        ImGui.Separator();
                                    }
                                    else if (s == 10) // Hydatos
                                    {
                                        string zone = "Hydatos";
                                        ImGui.Separator();
                                        ImGui.SetCursorPosX(centerPos - (ImGui.CalcTextSize(zone).X * 0.5f));
                                        ImGui.Text(zone);
                                        ImGui.Separator();
                                    }

                                    var weaponInfo = weaponEntry.Value[s];
                                    var itemRow = itemSheet.GetRow((uint)weaponInfo.itemId);

                                    GameIconLookup lookup = new GameIconLookup(itemRow.Icon, false, true);
                                    var icon = Plugin.TextureProvider.GetFromGameIcon(lookup);
                                    string itemName = itemRow.Singular.ExtractText();

                                    float buttonWidth = ImGui.GetContentRegionAvail().X;

                                    Vector4 hoverColor = weaponInfo.obtained ? new Vector4(0.13333334f, 0.54509807f, 0.13333334f, 0.5f) : new Vector4(0.2f, 0.2f, 0.2f, 0.5f);

                                    ImGui.PushStyleColor(ImGuiCol.Text, weaponInfo.obtained ? new Vector4(1f, 1f, 1f, 1f) : new Vector4(0.5f, 0.5f, 0.5f, 1f));
                                    ImGui.PushStyleColor(ImGuiCol.Button, weaponInfo.obtained ? new Vector4(0.13333334f, 0.54509807f, 0.13333334f, 0.3f) : new Vector4(0f, 0f, 0f, 0.2f));
                                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.8f, 0.8f, 0.8f, 1f));
                                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, hoverColor);

                                    if (ImGui.Button($"##Btn_{weaponInfo.itemId}", new Vector2(buttonWidth, 50f)))
                                    {
                                        jumpTo = s;
                                        targetScrollY = 0f;
                                    }

                                    ImGui.PopStyleColor(3);

                                    ImGui.SameLine(10f);
                                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5f);
                                    ImGui.Image(icon.GetWrapOrEmpty().ImGuiHandle, new Vector2(40f, 40f), new Vector2(0f, 0f), new Vector2(1f, 1f), weaponInfo.obtained ? new Vector4(1f, 1f, 1f, 1f) : new Vector4(0.5f, 0.5f, 0.5f, 1f));

                                    if (weaponInfo.shieldId > 0)
                                    {
                                        var shieldRow = itemSheet.GetRow((uint)weaponInfo.shieldId);
                                        var shieldLookup = new GameIconLookup(shieldRow.Icon, false, true);
                                        var shieldIcon = Plugin.TextureProvider.GetFromGameIcon(shieldLookup);
                                        ImGui.SameLine(10f);
                                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 50f);
                                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5f);
                                        ImGui.Image(shieldIcon.GetWrapOrEmpty().ImGuiHandle, new Vector2(40f, 40f), new Vector2(0f, 0f), new Vector2(1f, 1f), weaponInfo.obtained ? new Vector4(1f, 1f, 1f, 1f) : new Vector4(0.5f, 0.5f, 0.5f, 1f));
                                    }

                                    ImGui.SameLine();
                                    ImGui.AlignTextToFramePadding();
                                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5f);
                                    ImGui.Text(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(itemName.ToLower()));

                                    if(weaponInfo.shieldId > 0)
                                    {
                                        var shieldRow = itemSheet.GetRow((uint)weaponInfo.shieldId);
                                        var shieldLookup = new GameIconLookup(shieldRow.Icon, false, true);
                                        var shieldIcon = Plugin.TextureProvider.GetFromGameIcon(shieldLookup);
                                        string shieldName = shieldRow.Singular.ExtractText();

                                        float shieldTextSize = ImGui.CalcTextSize(shieldName).X * 0.5f;

                                        ImGui.SameLine(10f);
                                        ImGui.AlignTextToFramePadding();
                                        ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(98f, 20f));

                                        ImGui.PushStyleColor(ImGuiCol.Text, weaponInfo.obtained ? new Vector4(0.8f, 0.8f, 0.8f, 1f) : new Vector4(0.5f, 0.5f, 0.5f, 1f));
                                        ImGui.Text(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(shieldName.ToLower()));
                                        ImGui.PopStyleColor();
                                    }
                                    ImGui.PopStyleColor();
                                }
                                ImGui.EndListBox();
                            }

                            // Right Column
                            ImGui.SameLine();
                            ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(0f, 0f));

                            if (ImGui.BeginListBox($"##{weaponEntry.Key}_Info", new Vector2(600f, 500f)))
                            {


                                List<float> stepPositions = new List<float>();
                                for (int t = 0; t < weaponEntry.Value.Count; t++)
                                {
                                    stepPositions.Add(ImGui.GetCursorPosY());
                                    string text = $"Step {t + 1}";
                                    ImGui.SetCursorPosX(ImGui.GetColumnWidth() * 0.5f - (ImGui.CalcTextSize(text).X * 0.5f));
                                    ImGui.Text(text);
                                    ImGui.Separator();

                                    var stepInfo = GetStepRequirements(t, weaponEntry.Key);

                                    if (stepInfo != null)
                                    {
                                        ImGui.Dummy(new Vector2(0f, 20f));
                                        ImGui.SetCursorPosX(ImGui.GetColumnWidth() * 0.5f - (ImGui.CalcTextSize("Required Items:").X * 0.5f));
                                        ImGui.Text("Required Items:");
                                        ImGui.Dummy(new Vector2(0f, 20f));

                                        if (stepInfo.itemReqs.Count > 0)
                                        {
                                            float height = 0f;
                                            for (int r = 0; r < stepInfo.itemReqs.Count; r++)
                                            {
                                                var itemRow = itemSheet.GetRow((uint)stepInfo.itemReqs.ElementAt(r).Key);
                                                string itemName = itemRow.Name.ExtractText();
                                                GameIconLookup lookup = new GameIconLookup(itemRow.Icon, false, true);
                                                var icon = Plugin.TextureProvider.GetFromGameIcon(lookup);

                                                if (r == 0)
                                                {
                                                    height = ImGui.GetCursorPos().Y;
                                                }

                                                ImGui.SetCursorPos(new Vector2(ImGui.GetColumnWidth() * 0.5f - (stepInfo.itemReqs.Count >= 3 ? 35f : 20f) + //Get the center of the column, then shift left depending on req count
                                                    (stepInfo.itemReqs.Count > 1 ? (60f * r) - 
                                                    (40f * stepInfo.itemReqs.Count * 0.5f) + 
                                                    (10f * stepInfo.itemReqs.Count * 0.5f) : 0f), 
                                                    height != 0f ? height : ImGui.GetCursorPosY()));

                                                ImGui.Image(icon.GetWrapOrEmpty().ImGuiHandle, new Vector2(40f, 40f));
                                                if (ImGui.IsItemHovered())
                                                {
                                                    Vector2 mousePos = ImGui.GetMousePos();

                                                    string tooltipText = $"{itemName}\n\n{GetItemDesc(stepInfo.itemReqs.ElementAt(r).Key)}";
                                                    Vector2 textSize = ImGui.CalcTextSize(tooltipText);

                                                    float tooltipWidth = textSize.X + 20;
                                                    float tooltipHeight = textSize.Y + 20;
                                                    Vector2 tooltipPos = new Vector2(mousePos.X - tooltipWidth / 2, mousePos.Y + 20);

                                                    ImGui.SetNextWindowPos(tooltipPos);
                                                    ImGui.SetNextWindowSize(new Vector2(tooltipWidth, tooltipHeight));

                                                    ImGui.BeginTooltip();
                                                    ImGui.Text(tooltipText);
                                                    ImGui.EndTooltip();
                                                }
                                                string quantity = $"x{stepInfo.itemReqs.ElementAt(r).Value.ToString()}";
                                                ImGui.SetCursorPosX(ImGui.GetColumnWidth() * 0.5f - ImGui.CalcTextSize(quantity).X * 0.5f - (stepInfo.itemReqs.Count >= 3 ? 15f : 0f) + (stepInfo.itemReqs.Count > 1 ? (60f * r) - (40f * stepInfo.itemReqs.Count * 0.5f) + (10f * stepInfo.itemReqs.Count * 0.5f) : 0f));
                                                ImGui.Text(quantity);
                                            }
                                        }
                                        ImGui.Dummy(new Vector2(0f, 40f));
                                    }
                                }

                                if (jumpTo != -1 && jumpTo < stepPositions.Count)
                                {
                                    targetScrollY = stepPositions[jumpTo];
                                    jumpTo = -1;
                                }

                                if (targetScrollY >= 0f)
                                {
                                    currentScrollY = ImGui.GetScrollY();
                                    float delta = targetScrollY - currentScrollY;
                                    if (Math.Abs(delta) > 1f)
                                    {
                                        ImGui.SetScrollY(currentScrollY + delta * scrollSpeed * ImGui.GetIO().DeltaTime);
                                    }
                                    else
                                    {
                                        ImGui.SetScrollY(targetScrollY);
                                        targetScrollY = -1.0f;
                                    }
                                }

                                ImGui.EndListBox();
                            }

                            ImGui.PopStyleColor();
                            ImGui.EndTabItem();
                        }

                        ImGui.PopStyleColor(4);
                    }

                    ImGui.EndTabBar();
                }

                ImGui.EndTabItem();
            }

            //Other tabs
            if (ImGui.BeginTabItem("Armor")) ImGui.EndTabItem();
            if (ImGui.BeginTabItem("Logograms")) ImGui.EndTabItem();
            if (ImGui.BeginTabItem("Magicite")) ImGui.EndTabItem();
            if (ImGui.BeginTabItem("Items")) ImGui.EndTabItem();
            if (ImGui.BeginTabItem("Settings")) ImGui.EndTabItem();

            ImGui.EndTabBar();
        }

        ImGui.PopStyleColor(2);
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

    public static string GetItemDesc(int itemId)
    {
        if (itemId == 21801)
        {
            return "Obtain Anemos Crystals by killing Notorious Monsters and exchange them with Gerolt.\nAlso rarely dropped by level-appropriate mobs.";
        }
        if (itemId == 21802)
        {
            return "Achieve a gold rating in the FATE 'Wail in the Willows' by defeating the Notorious Monster, Pazuzu whilst elemental level 19 or higher.\nPazuzu is spawned by farming level 25 Shadow Wraiths outside it's arena, located at X:7.3, Y:21.7\nThis Notorious Monster only spawns during the Gales weather";
        }
        if (itemId == 23309)
        {
            return "Obtain vitilized aether by killing Notorious Monsters and level-appropriate mobs.\nExchange the aether for Frosted Protean Crystals at the Crystal Forge located at X:6.1, Y:21.5.\nYou will need to drop down onto the ledge from above at X:8.7, Y:21.5";
        }
        if (itemId == 22976)
        {
            return "Obtained by defeating Notorious Monsters around Pagos";
        }
        if (itemId == 22975)
        {
            return "Achieve a gold rating in the FATE 'Louhi on Ice' by defeating the Notorious Monster, Louhi whilst elemental level 34 or higher.\nLouhi is spawned by farming level 40 Val Corpses around it's arena, located at X:35.7, Y:18.7\nTo reach this area you will need to drop down onto the ledge at X:32.0, Y:20.0\nThis Notorious Monster only spawns at night";
        }
        if (itemId == 24124)
        {
            return "Obtained by defeating Notorious Monsters around Pyros";
        }
        if (itemId == 24123)
        {
            return "Achieve a gold rating in the FATE 'Lost Epic' by defeating the Notorious Monster, Penthesilea whilst elemental level 49 or higher.\nPenthesilea is spawned by farming level 55 Val Bloodgliders outside it's arena, located at X:35.5, Y:6.1\nThis Notorious Monster only spawns during the Heat Waves weather";
        }
        if (itemId == 24807)
        {
            return "Obtained by defeating Notorious Monsters around Hydatos";
        }
        if (itemId == 24806)
        {
            return "Achieve a gold rating in the FATE 'Crystalline Provenance' by defeating the Notorious Monster, Provenance Watcher whilst elemental level 59 or higher.\nProvenance Watcher is spawned by farming level 65 Crystal Claws outside it's arena, located at X:32.8, Y:19.5\n";
        }
        if (itemId == 24808)
        {
            return "Obtained by opening Treasure Coffers found within The Baldesion Arsenal. See the Handbook tab for more information.\nThis step increases your weapon's effectiveness within Eureka zones and does not have any visual differences";
        }
        return "Obtained by completing your level 70 job quest. Can be re-purchased at any Calamity Salvager.";
    }

    public class StepInfo()
    {
        public string description = "";
        public Dictionary<int, int> itemReqs = new Dictionary<int, int>();
    }

    public static StepInfo GetStepRequirements(int currentStep, string job)
    {
        StepInfo stepInfo = new StepInfo();
        stepInfo.itemReqs = new Dictionary<int, int>();

        switch (currentStep)
        {
            case 0:
                if (job == "PLD") //WIP - I can't remember how obtaining Sword and Shield works and if they have different requirements or not
                {
                    stepInfo.itemReqs.Add(relicIds[job][0], 1);
                    stepInfo.itemReqs.Add(pldShields[0], 1);

                    stepInfo.itemReqs.Add(21801, 100); // 100 Protean Crystals
                    break;
                }
                stepInfo.itemReqs.Add(relicIds[job][0], 1);
                stepInfo.itemReqs.Add(21801, 100); // 100 Protean Crystals
                break;

            case 1:
                stepInfo.itemReqs.Add(21801, 400); // 400 Protean Crystals
                break;

            case 2:
                stepInfo.itemReqs.Add(21801, 800); // 800 Protean Crystals
                break;
            case 3:
                stepInfo.itemReqs.Add(21802, 3); // 3 Pazuzu's Feathers
                break;
            case 4:
                stepInfo.itemReqs.Add(23309, 5); // 5 Frosted Pagos Crystals
                break;
            case 5:
                stepInfo.itemReqs.Add(23309, 10); // 10 Frosted Pagos Crystals
                stepInfo.itemReqs.Add(22976, 500); // 500 Pagos Crystals
                break;
            case 6:
                stepInfo.itemReqs.Add(23309, 16); // 16 Frosted Pagos Crystals
                stepInfo.itemReqs.Add(22975, 5); // 5 Louhi's Ice Crystals
                break;
            case 7:
                stepInfo.itemReqs.Add(24124, 150); // Pyros Pyros Crystals
                break;
            case 8:
                stepInfo.itemReqs.Add(24124, 200); // Pyros Pyros Crystals
                break;
            case 9:
                stepInfo.itemReqs.Add(24124, 300); // 300 Pyros Crystals
                stepInfo.itemReqs.Add(24123, 5); // 5 Penny Flame 
                break;

            case 10:
                stepInfo.itemReqs.Add(24807, 50); // 50 Hydatos Crystals
                break;
            case 11:
                stepInfo.itemReqs.Add(24807, 100); // 100 Hydatos Crystals
                break;
            case 12:
                stepInfo.itemReqs.Add(24807, 100); // 100 Hydatos Crystals
                break;
            case 13:
                stepInfo.itemReqs.Add(24807, 100); // 100 Hydatos Crystals
                stepInfo.itemReqs.Add(24806, 5); // 5 Crystalline Scales
                break;
            case 14:
                stepInfo.itemReqs.Add(24808, 100); // 100 Eureka Fragments
                break;

        }

        return stepInfo;
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
        //Base, +1, +2, Anemos, Pagos, +1, Elemental, +1, +2, Pyros, Hydatos, +1 Base-Eureka, Eureka, Physeos
        {"WAR", new []{ 21944, 21960, 21976, 21992, 22927, 22943, 22959, 24041, 24057, 24073, 24645, 24661, 24677, 24693, 24709 } },
        {"PLD", new []{ 21942, 21958, 21974, 21990, 22925, 22941, 22957, 24039, 24055, 24071, 24643, 24659, 24675, 24691, 24707 } },
        {"DRK", new []{ 21948, 21964, 21980, 21996, 22931, 22947, 22963, 24045, 24061, 24077, 24649, 24665, 24681, 24697, 24713 } },

        {"WHM", new []{ 21950, 21966, 21982, 21998, 22933, 22949, 22965, 24047, 24063, 24079, 24651, 24667, 24683, 24699, 24715 } },
        {"SCH", new []{ 21953, 21969, 21985, 22001, 22936, 22952, 22968, 24050, 24066, 24082, 24654, 24670, 24686, 24702, 24718 } },
        {"AST", new []{ 21954, 21970, 21986, 22002, 22937, 22953, 22969, 24051, 24067, 24083, 24655, 24671, 24687, 24703, 24719 } },

        {"DRG", new []{ 21945, 21961, 21977, 21993, 22928, 22944, 22960, 24042, 24058, 24074, 24646, 24662, 24678, 24694, 24710 } },
        {"MNK", new []{ 21943, 21959, 21975, 21991, 22926, 22942, 22958, 24040, 24056, 24072, 24644, 24660, 24676, 24692, 24708 } },
        {"NIN", new []{ 21947, 21963, 21979, 21995, 22930, 22946, 22962, 24044, 24060, 24076, 24648, 24664, 24680, 24696, 24712 } },
        {"SAM", new []{ 21955, 21971, 21987, 22003, 22938, 22954, 22970, 24052, 24068, 24084, 24656, 24672, 24688, 24704, 24720 } },

        {"BRD", new []{ 21946, 21962, 21978, 21994, 22929, 22945, 22961, 24043, 24059, 24075, 24647, 24663, 24679, 24695, 24711 } },
        {"MCH", new []{ 21949, 21965, 21981, 21997, 22932, 22948, 22964, 24046, 24062, 24078, 24650, 24666, 24682, 24698, 24714 } },

        {"BLM", new []{ 21951, 21967, 21983, 21999, 22934, 22950, 22966, 24048, 24064, 24080, 24652, 24668, 24684, 24700, 24716 } },
        {"SMN", new []{ 21952, 21968, 21984, 22000, 22935, 22951, 22967, 24049, 24065, 24081, 24653, 24669, 24685, 24701, 24717 } },
        {"RDM", new []{ 21956, 21972, 21988, 22004, 22939, 22955, 22971, 24053, 24069, 24085, 24657, 24673, 24689, 24705, 24721 } },
    };

    public static List<int> pldShields = new List<int>()
    {
        21957,21973,21989,22005,22940,22956,22972,24054,24070,24086,24658,24674,24690,24706,24722
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
                if(kvp.Key == "PLD")
                {
                    weapon.shieldId = pldShields[i];
                }
                weapons.Add(weapon);
            }
            weaponTracker.Add(kvp.Key, weapons);
        }
        return weaponTracker;
    }

    public static void UpdateWeaponCollection(bool save)
    {
        //If a relic was found, mark all previous steps as completed
        foreach (var kvp in weaponTracker)
        {
            bool progress = false;
            for (int i = weaponTracker.Values.Count - 1; i >= 0; i--)
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
        if (save)
        {
            Save();
        }
    }

    public static void Save()
    {
        //Weapons
        List<int> weaponIDs = new List<int>();
        foreach (KeyValuePair<string, List<RelicWeapon>> job in weaponTracker)
        {
            int lastObtained = -1;
            foreach (RelicWeapon weapon in job.Value)
            {
                if (weapon.obtained)
                {
                    lastObtained = weapon.itemId; // We only need the ID of the last obtained relic
                }
            }
            if (lastObtained != -1)
            {
                weaponIDs.Add(lastObtained);
            }
        }

        var itemSaveData = new Dictionary<string, object>()
        {
            {"Weapons", weaponIDs},
        };

        string json = JsonConvert.SerializeObject(itemSaveData, Formatting.Indented);
        string saveLocation = Plugin.PluginInterface.ConfigDirectory.FullName;
        string characterName = "Player";

        Plugin.log.Information($"Save location: {saveLocation}");

        if (Plugin.ClientState.LocalPlayer != null)
        {
            characterName = Plugin.ClientState.LocalContentId.ToString();
        }
        else
        {
            return; //Don't save if player is null
        }

        if (saveLocation != null)
        {
            File.WriteAllText($"{saveLocation}{Path.DirectorySeparatorChar}SaveData_{characterName}.json", json);
            Plugin.log.Information($"Saved Eureka Collection to: {saveLocation}{Path.DirectorySeparatorChar}SaveData_{characterName}.json");
        }
    }

    public static void Load()
    {
        string saveLocation = Plugin.PluginInterface.ConfigDirectory.FullName;
        string characterName = "Player";
        if (Plugin.ClientState.LocalPlayer != null)
        {
            characterName = Plugin.ClientState.LocalContentId.ToString();
        }
        else
        {
            return; //Can't load if player is null
        }


        string json = File.ReadAllText($"{saveLocation}{Path.DirectorySeparatorChar}SaveData_{characterName}.json");
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        if (data != null)
        {
            if (data.ContainsKey("Weapons") && data["Weapons"] != null)
            {
                var weapons = JsonConvert.DeserializeObject<List<int>>(data["Weapons"].ToString());

                for (int i = 0; i < weapons.Count; i++)
                {
                    foreach (KeyValuePair<string, List<RelicWeapon>> job in weaponTracker)
                    {
                        foreach (RelicWeapon weapon in job.Value)
                        {
                            if (weapon.itemId == weapons[i])
                            {
                                weapon.obtained = true;
                            }
                        }
                    }
                }
                UpdateWeaponCollection(false);
            }
        }
        Plugin.log.Information("Finished loading Eureka Collection");
    }

    public static void Reset() //Clears collection from memory, preserving save data
    {

    }

    public static void HardReset() //Completely resets collection and deletes save data
    {

    }

    public class RelicWeapon()
    {
        public bool obtained;
        public int itemId;
        public int currentStep;
        public string name = "null";
        public int shieldId;
    }
}
