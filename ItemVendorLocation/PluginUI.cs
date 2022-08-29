﻿using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using ImGuiNET;
using ItemVendorLocation.Models;

namespace ItemVendorLocation
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    internal class PluginUI : IDisposable
    {
        private readonly Configuration configuration;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool vendorLocationsVisable;

        public ItemInfo ItemToDisplay;

        public PluginUI(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public bool VendorResultsVisible
        {
            get => vendorLocationsVisable;
            set => vendorLocationsVisable = value;
        }

        public bool SettingsVisible { get; set; } = false;

        public void Dispose()
        {
        }

        public void Draw()
        {
            DrawVendorLocationWindow();
        }

        private void DrawTable(string npcName, NpcLocation location, string costStr)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(npcName);
            ImGui.TableNextColumn();
            if (location != null)
            {
                if (ImGui.Button($"{location.TerritoryExcel.PlaceName.Value.Name} ({location.MapX:F1}, {location.MapY:F1})"))
                {
                    Service.GameGui.OpenMapWithMapLink(new MapLinkPayload(location.TerritoryType, location.MapId, location.MapX, location.MapY));
                }
            }
            else
            {
                ImGui.Text("No location");
            }

            ImGui.TableNextColumn();

            ImGui.Text(costStr);

            if (ItemToDisplay.Type == ItemType.Achievement)
            {
                ImGui.TableNextColumn();
                ImGui.Text(ItemToDisplay.AchievementDescription);
            }
        }

        public void DrawVendorLocationWindow()
        {
            if (!vendorLocationsVisable)
            {
                return;
            }

            var windowSize = ImGui.GetWindowViewport().Size;

            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 200), windowSize with { Y = windowSize.Y * 0.5f });
            if (ImGui.Begin($"{ItemToDisplay.Name} Vendors###Item Vendor Location", ref vendorLocationsVisable, ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (ImGui.BeginTable("Vendors", ItemToDisplay.Type == ItemType.Achievement ? 4 : 3, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp, new Vector2(-1, -1)))
                {
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Location");
                    ImGui.TableSetupColumn("Cost");
                    if (ItemToDisplay.Type == ItemType.Achievement)
                    {
                        ImGui.TableSetupColumn("Obtain Requirement");
                    }

                    ImGui.TableHeadersRow();

                    foreach (var npcInfo in ItemToDisplay.NpcInfos)
                    {
                        var costStr = ItemToDisplay.Costs.Aggregate("", (current, cost) => current + $"{cost.Item2} x{cost.Item1}, ");
                        costStr = costStr[..^2];

                        if (ItemToDisplay.Type == ItemType.GcShop)
                        {
                            // handle this differently
                            costStr = ItemToDisplay.Costs.Last().Item2 + " x" + ItemToDisplay.Costs.Last().Item1;
                            foreach (var cost in ItemToDisplay.Costs)
                            {
                                var substring = npcInfo.Name.ToLower().Substring(0, 2);
                                if (!cost.Item2.ToLower().StartsWith(substring))
                                {
                                    continue;
                                }

                                costStr = $"{cost.Item2} x{cost.Item1}";
                                break;
                            }
                        }

                        DrawTable(npcInfo.Name, npcInfo.Location, costStr);
                    }
                }

                ImGui.EndTable();
            }

            ImGui.End();
        }
    }
}