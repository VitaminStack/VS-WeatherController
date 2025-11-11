using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

using WeatherSimFunc = System.Func<Vintagestory.GameContent.WeatherSimulationRegion, bool>;

namespace WeatherController
{
    public class WeatherControllerSystem : ModSystem
    {
        internal const string ChannelCode = "weathercontroller";

        private ICoreClientAPI capi;
        private ICoreServerAPI sapi;
        private IClientNetworkChannel clientChannel;
        private IServerNetworkChannel serverChannel;
        private GuiDialogWeatherController dialog;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.Network.RegisterChannel(ChannelCode)
                .RegisterMessageType<WeatherOptionsRequest>()
                .RegisterMessageType<WeatherOptionsPacket>()
                .RegisterMessageType<WeatherControlCommand>();
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            capi = api;
            clientChannel = api.Network.GetChannel(ChannelCode)
                .SetMessageHandler<WeatherOptionsPacket>(OnOptionsReceived);

            api.Input.RegisterHotKey("weathercontroller", "Weather Controller", GlKeys.O, HotkeyType.GUIOrOtherControls);
            api.Input.SetHotKeyHandler("weathercontroller", ToggleDialog);
            api.Event.LevelFinalize += () => RequestOptions(false);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;
            serverChannel = api.Network.GetChannel(ChannelCode)
                .SetMessageHandler<WeatherOptionsRequest>(OnOptionsRequested)
                .SetMessageHandler<WeatherControlCommand>(OnControlCommand);
        }

        private bool ToggleDialog(KeyCombination combination)
        {
            EnsureDialog();
            if (dialog.IsOpened())
            {
                dialog.TryClose();
            }
            else
            {
                RequestOptions(false);
                dialog.TryOpen();
            }
            return true;
        }

        internal void RequestOptions(bool forceReload)
        {
            clientChannel?.SendPacket(new WeatherOptionsRequest { ForceReload = forceReload });
        }

        internal void SendCommand(WeatherControlCommand command)
        {
            clientChannel?.SendPacket(command);
        }

        private void OnOptionsReceived(WeatherOptionsPacket packet)
        {
            EnsureDialog();
            dialog.UpdateOptions(packet);
            if (!string.IsNullOrEmpty(packet?.Message))
            {
                capi.ShowChatMessage(packet.Message);
            }
        }

        private void EnsureDialog()
        {
            if (dialog == null)
            {
                dialog = new GuiDialogWeatherController(capi, this);
            }
        }

        private void OnOptionsRequested(IServerPlayer fromPlayer, WeatherOptionsRequest packet)
        {
            SendOptionsToPlayer(fromPlayer, packet?.ForceReload == true, null);
        }

        private void OnControlCommand(IServerPlayer fromPlayer, WeatherControlCommand command)
        {
            WeatherSystemServer weather = sapi.ModLoader.GetModSystem<WeatherSystemServer>(true);
            if (weather == null)
            {
                SendOptionsToPlayer(fromPlayer, false, "Weather system is not available on this server.");
                return;
            }

            bool hasPrivilege = fromPlayer.HasPrivilege(Privilege.controlserver) || fromPlayer.HasPrivilege(Privilege.root);
            if (!hasPrivilege && command.Action != WeatherControlAction.RequestOptions && command.Action != WeatherControlAction.RefreshOptions)
            {
                SendOptionsToPlayer(fromPlayer, false, "You do not have permission to change the weather.");
                return;
            }

            bool success = false;
            string message = null;
            switch (command.Action)
            {
                case WeatherControlAction.SetRegionPattern:
                    weather.ReloadConfigs();
                    success = TryWithRegion(fromPlayer, weather, sim =>
                    {
                        bool result = sim.SetWeatherPattern(command.Code, command.UpdateInstantly);
                        if (result)
                        {
                            sim.TickEvery25ms(0.025f);
                        }
                        return result;
                    }, out message, "Weather pattern applied to this region.", "No weather simulation is active for this region.");
                    break;
                case WeatherControlAction.SetGlobalPattern:
                    weather.ReloadConfigs();
                    success = ApplyToAllRegions(weather, sim =>
                    {
                        bool result = sim.SetWeatherPattern(command.Code, command.UpdateInstantly);
                        if (result)
                        {
                            sim.TickEvery25ms(0.025f);
                        }
                        return result;
                    });
                    message = success ? "Weather pattern applied to all loaded regions." : "Weather pattern could not be applied.";
                    break;
                case WeatherControlAction.SetRegionEvent:
                    weather.ReloadConfigs();
                    success = TryWithRegion(fromPlayer, weather, sim =>
                    {
                        if (!sim.SetWeatherEvent(command.Code, command.UpdateInstantly))
                        {
                            return false;
                        }
                        if (command.UseAllowStop && sim.CurWeatherEvent != null)
                        {
                            sim.CurWeatherEvent.AllowStop = command.AllowStop;
                        }
                        sim.CurWeatherEvent?.OnBeginUse();
                        sim.TickEvery25ms(0.025f);
                        return true;
                    }, out message, "Weather event applied to this region.", "No weather simulation is active for this region.");
                    break;
                case WeatherControlAction.SetGlobalEvent:
                    weather.ReloadConfigs();
                    success = ApplyToAllRegions(weather, sim =>
                    {
                        if (!sim.SetWeatherEvent(command.Code, command.UpdateInstantly))
                        {
                            return false;
                        }
                        if (command.UseAllowStop && sim.CurWeatherEvent != null)
                        {
                            sim.CurWeatherEvent.AllowStop = command.AllowStop;
                        }
                        sim.CurWeatherEvent?.OnBeginUse();
                        sim.TickEvery25ms(0.025f);
                        return true;
                    });
                    message = success ? "Weather event applied to all loaded regions." : "Weather event could not be applied.";
                    break;
                case WeatherControlAction.SetGlobalWind:
                    weather.ReloadConfigs();
                    success = ApplyToAllRegions(weather, sim =>
                    {
                        bool result = sim.SetWindPattern(command.Code, command.UpdateInstantly);
                        if (result)
                        {
                            sim.TickEvery25ms(0.025f);
                        }
                        return result;
                    });
                    message = success ? "Wind pattern applied to all loaded regions." : "Wind pattern could not be applied.";
                    break;
                case WeatherControlAction.SetAutoChange:
                    if (command.UseAutoChange)
                    {
                        weather.autoChangePatterns = command.AutoChangeEnabled;
                        message = weather.autoChangePatterns ? "Automatic weather pattern changes enabled." : "Automatic weather pattern changes disabled.";
                        success = true;
                    }
                    else
                    {
                        message = "No auto-change value supplied.";
                    }
                    break;
                case WeatherControlAction.SetPrecipitationOverride:
                    if (command.UsePrecipitationOverride)
                    {
                        weather.OverridePrecipitation = command.PrecipitationOverride;
                        weather.broadCastConfigUpdate();
                        message = string.Format("Precipitation override set to {0:0.##}.", command.PrecipitationOverride);
                        success = true;
                    }
                    else
                    {
                        message = "No precipitation value supplied.";
                    }
                    break;
                case WeatherControlAction.ClearPrecipitationOverride:
                    weather.OverridePrecipitation = null;
                    weather.broadCastConfigUpdate();
                    message = "Precipitation override cleared.";
                    success = true;
                    break;
                case WeatherControlAction.RequestOptions:
                case WeatherControlAction.RefreshOptions:
                    success = true;
                    break;
                case WeatherControlAction.SetTemporalStormMode:
                    success = TrySetTemporalStormMode(command.Code, out message);
                    break;
                case WeatherControlAction.EndTemporalStorm:
                    success = TryEndActiveStorm(out message);
                    break;
                default:
                    message = "Unknown weather controller action.";
                    break;
            }

            if (!success && string.IsNullOrEmpty(message))
            {
                message = "Unable to update the weather.";
            }

            bool forceReload = command.Action == WeatherControlAction.RefreshOptions;
            SendOptionsToPlayer(fromPlayer, forceReload, message);
        }

        private bool TryWithRegion(IServerPlayer player, WeatherSystemServer weather, WeatherSimFunc operation, out string message, string successMessage, string missingMessage)
        {
            WeatherSimulationRegion region = GetRegionSimulation(player, weather);
            if (region == null)
            {
                message = missingMessage;
                return false;
            }

            if (operation(region))
            {
                message = successMessage;
                return true;
            }

            message = "Unable to update the weather.";
            return false;
        }

        private bool ApplyToAllRegions(WeatherSystemServer weather, WeatherSimFunc apply)
        {
            bool hasRegions = false;
            bool allSuccess = true;
            foreach (WeatherSimulationRegion region in weather.weatherSimByMapRegion.Values)
            {
                hasRegions = true;
                allSuccess &= apply(region);
            }
            return hasRegions && allSuccess;
        }

        private WeatherSimulationRegion GetRegionSimulation(IServerPlayer player, WeatherSystemServer weather)
        {
            BlockPos pos = player.Entity.Pos.AsBlockPos;
            int regionX = pos.X / sapi.World.BlockAccessor.RegionSize;
            int regionZ = pos.Z / sapi.World.BlockAccessor.RegionSize;
            long key = weather.MapRegionIndex2D(regionX, regionZ);
            weather.weatherSimByMapRegion.TryGetValue(key, out WeatherSimulationRegion region);
            return region;
        }

        private void SendOptionsToPlayer(IServerPlayer player, bool forceReload, string message)
        {
            if (serverChannel == null || player == null)
            {
                return;
            }

            WeatherSystemServer weather = sapi.ModLoader.GetModSystem<WeatherSystemServer>(true);
            WeatherOptionsPacket packet = new WeatherOptionsPacket
            {
                WeatherPatterns = Array.Empty<WeatherOptionEntry>(),
                WeatherEvents = Array.Empty<WeatherOptionEntry>(),
                WindPatterns = Array.Empty<WeatherOptionEntry>(),
                TemporalStormModes = Array.Empty<WeatherOptionEntry>(),
                Message = message
            };

            if (weather != null)
            {
                if (forceReload)
                {
                    weather.ReloadConfigs();
                }
                else if (weather.WeatherConfigs == null || weather.WindConfigs == null || weather.WeatherEventConfigs == null)
                {
                    weather.LoadConfigs();
                }

                packet.WeatherPatterns = (weather.WeatherConfigs ?? Array.Empty<WeatherPatternConfig>())
                    .Select(c => new WeatherOptionEntry { Code = c.Code, Name = string.IsNullOrWhiteSpace(c.Name) ? c.Code : c.Name })
                    .ToArray();

                packet.WeatherEvents = (weather.WeatherEventConfigs ?? Array.Empty<WeatherEventConfig>())
                    .Select(c => new WeatherOptionEntry { Code = c.Code, Name = string.IsNullOrWhiteSpace(c.Name) ? c.Code : c.Name })
                    .ToArray();

                packet.WindPatterns = (weather.WindConfigs ?? Array.Empty<WindPatternConfig>())
                    .Select(c => new WeatherOptionEntry { Code = c.Code, Name = string.IsNullOrWhiteSpace(c.Name) ? c.Code : c.Name })
                    .ToArray();

                WeatherSimulationRegion region = GetRegionSimulation(player, weather);
                packet.RegionAvailable = region != null;
                if (region != null)
                {
                    packet.CurrentPatternCode = region.NewWePattern?.config?.Code ?? region.OldWePattern?.config?.Code;
                    packet.CurrentEventCode = region.CurWeatherEvent?.config?.Code;
                    packet.CurrentEventAllowStop = region.CurWeatherEvent?.AllowStop;
                    packet.CurrentWindCode = region.CurWindPattern?.config?.Code;
                }

                packet.AutoChangeEnabled = weather.autoChangePatterns;
                packet.OverridePrecipitation = weather.OverridePrecipitation;
                packet.RainCloudDaysOffset = weather.RainCloudDaysOffset;
            }

            SystemTemporalStability storms = sapi.ModLoader.GetModSystem<SystemTemporalStability>(false);
            if (storms != null)
            {
                packet.TemporalStormModes = GetTemporalStormOptions(storms);
                packet.CurrentTemporalStormMode = GetCurrentTemporalStormMode(storms);
                packet.TemporalStormActive = storms.StormData?.nowStormActive == true;
            }

            bool hasPrivilege = player.HasPrivilege(Privilege.controlserver) || player.HasPrivilege(Privilege.root);
            packet.HasControlPrivilege = hasPrivilege;

            serverChannel.SendPacket(packet, player);
        }

        private WeatherOptionEntry[] GetTemporalStormOptions(SystemTemporalStability storms)
        {
            var options = new List<WeatherOptionEntry>
            {
                new WeatherOptionEntry { Code = "off", Name = FormatStormModeName("off") }
            };

            IDictionary configs = GetStormConfigDictionary(storms);
            if (configs != null)
            {
                foreach (DictionaryEntry entry in configs)
                {
                    if (entry.Key is string code && !string.IsNullOrEmpty(code))
                    {
                        options.Add(new WeatherOptionEntry
                        {
                            Code = code,
                            Name = FormatStormModeName(code)
                        });
                    }
                }
            }

            return options.ToArray();
        }

        private string GetCurrentTemporalStormMode(SystemTemporalStability storms)
        {
            string configured = sapi.World.Config.GetString("temporalStorms", null);
            if (string.IsNullOrEmpty(configured))
            {
                configured = GetStormWorldConfig(storms);
                if (string.IsNullOrEmpty(configured))
                {
                    object activeConfig = GetStormField(storms, "config");
                    IDictionary configs = GetStormConfigDictionary(storms);
                    if (configs != null && activeConfig != null)
                    {
                        foreach (DictionaryEntry entry in configs)
                        {
                            if (entry.Value == activeConfig)
                            {
                                configured = entry.Key as string;
                                break;
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(configured))
            {
                configured = "off";
            }

            return configured;
        }

        private bool TrySetTemporalStormMode(string requestedMode, out string message)
        {
            message = null;
            if (string.IsNullOrEmpty(requestedMode))
            {
                message = "No temporal storm mode supplied.";
                return false;
            }

            string normalized = requestedMode.Trim().ToLowerInvariant();
            SystemTemporalStability storms = sapi.ModLoader.GetModSystem<SystemTemporalStability>(false);
            if (storms == null)
            {
                message = "Temporal storms system is not available.";
                return false;
            }

            IDictionary configs = GetStormConfigDictionary(storms);
            if (normalized != "off" && (configs == null || !configs.Contains(normalized)))
            {
                message = string.Format("Temporal storm mode '{0}' is not recognized.", requestedMode);
                return false;
            }

            sapi.World.Config.SetString("temporalStorms", normalized);
            sapi.WorldManager.SaveGame.WorldConfiguration.SetString("temporalStorms", normalized);

            SetStormField(storms, "worldConfigStorminess", normalized);
            SetStormField(storms, "stormsEnabled", normalized != "off");

            object stormConfig = null;
            if (normalized != "off" && configs != null)
            {
                stormConfig = configs[normalized];
            }

            SetStormField(storms, "config", stormConfig);

            if (normalized == "off")
            {
                var data = storms.StormData;
                if (data != null)
                {
                    data.nowStormActive = false;
                }
                message = "Temporal storms disabled.";
            }
            else
            {
                InvokeStormMethod(storms, "prepareNextStorm");
                message = string.Format("Temporal storms set to {0}.", FormatStormModeName(normalized));
            }

            return true;
        }

        private bool TryEndActiveStorm(out string message)
        {
            SystemTemporalStability storms = sapi.ModLoader.GetModSystem<SystemTemporalStability>(false);
            if (storms == null)
            {
                message = "Temporal storms system is not available.";
                return false;
            }

            var data = storms.StormData;
            if (data == null || !data.nowStormActive)
            {
                message = "No temporal storm is currently active.";
                return false;
            }

            data.stormGlitchStrength = 0f;
            data.nowStormActive = false;
            data.stormDayNotify = 99;
            data.stormActiveTotalDays = sapi.World.Calendar.TotalDays;

            if (sapi.World is IServerWorldAccessor serverWorld)
            {
                var stormMobCache = GetStormField(storms, "stormMobCache") as HashSet<AssetLocation>;
                if (stormMobCache != null)
                {
                    foreach (Entity entity in serverWorld.LoadedEntities.Values.ToList())
                    {
                        if (stormMobCache.Contains(entity.Code))
                        {
                            entity.Attributes?.RemoveAttribute("ignoreDaylightFlee");
                            if (sapi.World.Rand.NextDouble() < 0.5)
                            {
                                sapi.World.DespawnEntity(entity, new EntityDespawnData
                                {
                                    Reason = EnumDespawnReason.Expire
                                });
                            }
                        }
                    }
                }
            }

            InvokeStormMethod(storms, "prepareNextStorm");

            var serverChannel = GetStormField(storms, "serverChannel") as IServerNetworkChannel;
            serverChannel?.BroadcastPacket(storms.StormData, Array.Empty<IServerPlayer>());

            message = "Temporal storm ended.";
            return true;
        }

        private IDictionary GetStormConfigDictionary(SystemTemporalStability storms)
        {
            FieldInfo configsField = typeof(SystemTemporalStability).GetField("configs", BindingFlags.Instance | BindingFlags.NonPublic);
            return configsField?.GetValue(storms) as IDictionary;
        }

        private string GetStormWorldConfig(SystemTemporalStability storms)
        {
            return GetStormField(storms, "worldConfigStorminess") as string;
        }

        private object GetStormField(SystemTemporalStability storms, string fieldName)
        {
            FieldInfo field = typeof(SystemTemporalStability).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            return field?.GetValue(storms);
        }

        private void SetStormField(SystemTemporalStability storms, string fieldName, object value)
        {
            FieldInfo field = typeof(SystemTemporalStability).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(storms, value);
        }

        private void InvokeStormMethod(SystemTemporalStability storms, string methodName)
        {
            MethodInfo method = typeof(SystemTemporalStability).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method?.Invoke(storms, Array.Empty<object>());
        }

        private string FormatStormModeName(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return string.Empty;
            }

            switch (code.ToLowerInvariant())
            {
                case "off":
                    return "Off";
                case "veryrare":
                    return "Very Rare";
                case "rare":
                    return "Rare";
                case "sometimes":
                    return "Sometimes";
                case "often":
                    return "Often";
                case "veryoften":
                    return "Very Often";
                default:
                    return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(code);
            }
        }
    }
}
