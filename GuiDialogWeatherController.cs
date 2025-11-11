using System;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace WeatherController
{
    public class GuiDialogWeatherController : GuiDialogGeneric
    {
        private const string PatternDropKey = "wc-pattern";
        private const string EventDropKey = "wc-event";
        private const string WindDropKey = "wc-wind";
        private const string AllowStopSwitchKey = "wc-allowstop";
        private const string AutoSwitchKey = "wc-autoswitch";
        private const string StormDropKey = "wc-storm";
        private const string StormApplyButtonKey = "wc-storm-apply";
        private const string PatternRegionButtonKey = "wc-pattern-region";
        private const string PatternGlobalButtonKey = "wc-pattern-global";
        private const string EventRegionButtonKey = "wc-event-region";
        private const string EventGlobalButtonKey = "wc-event-global";
        private const string WindApplyButtonKey = "wc-wind-apply";
        private const string AutoApplyButtonKey = "wc-auto-apply";
        private const string PrecipSliderKey = "wc-precip-slider";
        private const string PrecipApplyButtonKey = "wc-precip-apply";
        private const string PrecipResetButtonKey = "wc-precip-reset";

        private readonly WeatherControllerSystem system;
        private WeatherOptionsPacket currentOptions = new WeatherOptionsPacket
        {
            WeatherPatterns = Array.Empty<WeatherOptionEntry>(),
            WeatherEvents = Array.Empty<WeatherOptionEntry>(),
            WindPatterns = Array.Empty<WeatherOptionEntry>(),
            TemporalStormModes = Array.Empty<WeatherOptionEntry>()
        };

        private string selectedPatternCode;
        private string selectedEventCode;
        private string selectedWindCode;
        private string selectedTemporalStormMode;
        private bool selectedAllowStop = true;
        private bool selectedAutoChange;
        private float selectedPrecipitation;

        public GuiDialogWeatherController(ICoreClientAPI capi, WeatherControllerSystem system)
            : base("Weather Controller", capi)
        {
            this.system = system;
            ComposeDialog();
        }

        public override bool TryOpen()
        {
            system.RequestOptions(false);
            return base.TryOpen();
        }

        public void UpdateOptions(WeatherOptionsPacket packet)
        {
            if (packet == null)
            {
                packet = new WeatherOptionsPacket
                {
                    WeatherPatterns = Array.Empty<WeatherOptionEntry>(),
                    WeatherEvents = Array.Empty<WeatherOptionEntry>(),
                    WindPatterns = Array.Empty<WeatherOptionEntry>(),
                    TemporalStormModes = Array.Empty<WeatherOptionEntry>()
                };
            }

            if (packet.WeatherPatterns == null)
            {
                packet.WeatherPatterns = Array.Empty<WeatherOptionEntry>();
            }

            if (packet.WeatherEvents == null)
            {
                packet.WeatherEvents = Array.Empty<WeatherOptionEntry>();
            }

            if (packet.WindPatterns == null)
            {
                packet.WindPatterns = Array.Empty<WeatherOptionEntry>();
            }

            if (packet.TemporalStormModes == null)
            {
                packet.TemporalStormModes = Array.Empty<WeatherOptionEntry>();
            }

            currentOptions = packet;
            UpdateFromPacket(packet);
        }

        private void ComposeDialog()
        {
            ElementBounds contentBounds = ElementBounds.Fixed(0, 0, 450, 360);
            contentBounds.BothSizing = ElementSizing.Fixed;

            ElementBounds backgroundBounds = ElementStdBounds.DialogBackground().WithFixedPadding(GuiStyle.ElementToDialogPadding, GuiStyle.ElementToDialogPadding);
            backgroundBounds.WithChild(contentBounds);

            ElementBounds patternLabel = ElementBounds.Fixed(0, 0, 200, 20);
            ElementBounds patternDrop = ElementBounds.Fixed(0, 22, 220, 30);
            ElementBounds patternRegionBtn = ElementBounds.Fixed(230, 22, 100, 30);
            ElementBounds patternGlobalBtn = ElementBounds.Fixed(340, 22, 100, 30);

            ElementBounds eventLabel = ElementBounds.Fixed(0, 70, 200, 20);
            ElementBounds eventDrop = ElementBounds.Fixed(0, 92, 220, 30);
            ElementBounds eventRegionBtn = ElementBounds.Fixed(230, 92, 100, 30);
            ElementBounds eventGlobalBtn = ElementBounds.Fixed(340, 92, 100, 30);
            ElementBounds allowStopLabel = ElementBounds.Fixed(0, 132, 220, 20);
            ElementBounds allowStopSwitch = ElementBounds.Fixed(230, 128, 30, 30);

            ElementBounds windLabel = ElementBounds.Fixed(0, 170, 200, 20);
            ElementBounds windDrop = ElementBounds.Fixed(0, 192, 220, 30);
            ElementBounds windApplyBtn = ElementBounds.Fixed(230, 192, 100, 30);

            ElementBounds stormLabel = ElementBounds.Fixed(0, 230, 220, 20);
            ElementBounds stormDrop = ElementBounds.Fixed(0, 252, 220, 30);
            ElementBounds stormApplyBtn = ElementBounds.Fixed(230, 252, 100, 30);

            ElementBounds autoLabel = ElementBounds.Fixed(0, 292, 200, 20);
            ElementBounds autoSwitch = ElementBounds.Fixed(210, 288, 30, 30);
            ElementBounds autoApplyBtn = ElementBounds.Fixed(250, 288, 100, 30);

            ElementBounds precipLabel = ElementBounds.Fixed(0, 328, 220, 20);
            ElementBounds precipSlider = ElementBounds.Fixed(0, 350, 220, 30);
            ElementBounds precipApplyBtn = ElementBounds.Fixed(230, 350, 100, 30);
            ElementBounds precipResetBtn = ElementBounds.Fixed(340, 350, 100, 30);

            contentBounds.WithChildren(patternLabel, patternDrop, patternRegionBtn, patternGlobalBtn,
                eventLabel, eventDrop, eventRegionBtn, eventGlobalBtn, allowStopLabel, allowStopSwitch,
                windLabel, windDrop, windApplyBtn,
                stormLabel, stormDrop, stormApplyBtn,
                autoLabel, autoSwitch, autoApplyBtn,
                precipLabel, precipSlider, precipApplyBtn, precipResetBtn);

            SingleComposer = capi.Gui.CreateCompo("weathercontroller", ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle))
                .AddShadedDialogBG(backgroundBounds, true)
                .AddDialogTitleBar(DialogTitle, OnDialogTitleBarClosed)
                .BeginChildElements(contentBounds)
                .AddStaticText("Weather pattern", CairoFont.WhiteSmallText(), patternLabel)
                .AddDropDown(Array.Empty<string>(), Array.Empty<string>(), 0, OnPatternChanged, patternDrop, CairoFont.WhiteSmallText(), PatternDropKey)
                .AddSmallButton("Region", OnPatternRegionClicked, patternRegionBtn, EnumButtonStyle.Normal, PatternRegionButtonKey)
                .AddSmallButton("Global", OnPatternGlobalClicked, patternGlobalBtn, EnumButtonStyle.Normal, PatternGlobalButtonKey)
                .AddStaticText("Weather event", CairoFont.WhiteSmallText(), eventLabel)
                .AddDropDown(Array.Empty<string>(), Array.Empty<string>(), 0, OnEventChanged, eventDrop, CairoFont.WhiteSmallText(), EventDropKey)
                .AddSmallButton("Region", OnEventRegionClicked, eventRegionBtn, EnumButtonStyle.Normal, EventRegionButtonKey)
                .AddSmallButton("Global", OnEventGlobalClicked, eventGlobalBtn, EnumButtonStyle.Normal, EventGlobalButtonKey)
                .AddStaticText("Allow event to end automatically", CairoFont.WhiteSmallText(), allowStopLabel)
                .AddSwitch(OnAllowStopToggled, allowStopSwitch, AllowStopSwitchKey)
                .AddStaticText("Wind pattern", CairoFont.WhiteSmallText(), windLabel)
                .AddDropDown(Array.Empty<string>(), Array.Empty<string>(), 0, OnWindChanged, windDrop, CairoFont.WhiteSmallText(), WindDropKey)
                .AddSmallButton("Apply", OnWindApplyClicked, windApplyBtn, EnumButtonStyle.Normal, WindApplyButtonKey)
                .AddStaticText("Temporal storms", CairoFont.WhiteSmallText(), stormLabel)
                .AddDropDown(Array.Empty<string>(), Array.Empty<string>(), 0, OnStormModeChanged, stormDrop, CairoFont.WhiteSmallText(), StormDropKey)
                .AddSmallButton("Apply", OnStormApplyClicked, stormApplyBtn, EnumButtonStyle.Normal, StormApplyButtonKey)
                .AddStaticText("Auto-change patterns", CairoFont.WhiteSmallText(), autoLabel)
                .AddSwitch(OnAutoSwitchToggled, autoSwitch, AutoSwitchKey)
                .AddSmallButton("Apply", OnAutoApplyClicked, autoApplyBtn, EnumButtonStyle.Normal, AutoApplyButtonKey)
                .AddStaticText("Precipitation override", CairoFont.WhiteSmallText(), precipLabel)
                .AddSlider(OnPrecipSliderChanged, precipSlider, PrecipSliderKey)
                .AddSmallButton("Apply", OnPrecipApplyClicked, precipApplyBtn, EnumButtonStyle.Normal, PrecipApplyButtonKey)
                .AddSmallButton("Reset", OnPrecipResetClicked, precipResetBtn, EnumButtonStyle.Normal, PrecipResetButtonKey)
                .EndChildElements()
                .Compose();

            UpdateFromPacket(currentOptions);
        }

        private void UpdateFromPacket(WeatherOptionsPacket packet)
        {
            if (SingleComposer == null)
            {
                return;
            }

            selectedPatternCode = UpdateDropDownSelection(PatternDropKey, packet.WeatherPatterns, packet.CurrentPatternCode, selectedPatternCode);
            selectedEventCode = UpdateDropDownSelection(EventDropKey, packet.WeatherEvents, packet.CurrentEventCode, selectedEventCode);
            selectedWindCode = UpdateDropDownSelection(WindDropKey, packet.WindPatterns, packet.CurrentWindCode, selectedWindCode);
            selectedTemporalStormMode = UpdateDropDownSelection(StormDropKey, packet.TemporalStormModes, packet.CurrentTemporalStormMode, selectedTemporalStormMode);

            selectedAllowStop = packet.CurrentEventAllowStop ?? selectedAllowStop;
            SingleComposer.GetSwitch(AllowStopSwitchKey).SetValue(selectedAllowStop);

            selectedAutoChange = packet.AutoChangeEnabled;
            SingleComposer.GetSwitch(AutoSwitchKey).SetValue(selectedAutoChange);

            selectedPrecipitation = packet.OverridePrecipitation ?? 0f;
            var slider = SingleComposer.GetSlider(PrecipSliderKey);
            slider.SetValues((int)Math.Round(selectedPrecipitation * 100f), -100, 100, 5, "%");

            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool canControl = currentOptions.HasControlPrivilege;
            bool regionAvailable = currentOptions.RegionAvailable && canControl;

            var patternRegionButton = SingleComposer.GetButton(PatternRegionButtonKey);
            var patternGlobalButton = SingleComposer.GetButton(PatternGlobalButtonKey);
            var eventRegionButton = SingleComposer.GetButton(EventRegionButtonKey);
            var eventGlobalButton = SingleComposer.GetButton(EventGlobalButtonKey);
            var windApplyButton = SingleComposer.GetButton(WindApplyButtonKey);
            var stormApplyButton = SingleComposer.GetButton(StormApplyButtonKey);
            var autoApplyButton = SingleComposer.GetButton(AutoApplyButtonKey);
            var precipApplyButton = SingleComposer.GetButton(PrecipApplyButtonKey);
            var precipResetButton = SingleComposer.GetButton(PrecipResetButtonKey);

            patternRegionButton.Enabled = regionAvailable && !string.IsNullOrEmpty(selectedPatternCode);
            patternGlobalButton.Enabled = canControl && !string.IsNullOrEmpty(selectedPatternCode);
            eventRegionButton.Enabled = regionAvailable && !string.IsNullOrEmpty(selectedEventCode);
            eventGlobalButton.Enabled = canControl && !string.IsNullOrEmpty(selectedEventCode);
            windApplyButton.Enabled = canControl && !string.IsNullOrEmpty(selectedWindCode);
            if (stormApplyButton != null)
            {
                stormApplyButton.Enabled = canControl && !string.IsNullOrEmpty(selectedTemporalStormMode);
            }
            autoApplyButton.Enabled = canControl;
            precipApplyButton.Enabled = canControl;
            precipResetButton.Enabled = canControl;

            SingleComposer.GetSwitch(AllowStopSwitchKey).Enabled = canControl;
            SingleComposer.GetSwitch(AutoSwitchKey).Enabled = canControl;
        }

        private void OnPatternChanged(string code, bool selected)
        {
            if (selected)
            {
                selectedPatternCode = code;
                UpdateButtonStates();
            }
        }

        private void OnEventChanged(string code, bool selected)
        {
            if (selected)
            {
                selectedEventCode = code;
                UpdateButtonStates();
            }
        }

        private void OnWindChanged(string code, bool selected)
        {
            if (selected)
            {
                selectedWindCode = code;
                UpdateButtonStates();
            }
        }

        private void OnStormModeChanged(string code, bool selected)
        {
            if (selected)
            {
                selectedTemporalStormMode = code;
                UpdateButtonStates();
            }
        }

        private void OnAllowStopToggled(bool on)
        {
            selectedAllowStop = on;
        }

        private void OnAutoSwitchToggled(bool on)
        {
            selectedAutoChange = on;
        }

        private bool OnPatternRegionClicked()
        {
            if (string.IsNullOrEmpty(selectedPatternCode))
            {
                return true;
            }

            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetRegionPattern,
                Code = selectedPatternCode,
                UpdateInstantly = true
            });
            return true;
        }

        private bool OnPatternGlobalClicked()
        {
            if (string.IsNullOrEmpty(selectedPatternCode))
            {
                return true;
            }

            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetGlobalPattern,
                Code = selectedPatternCode,
                UpdateInstantly = true
            });
            return true;
        }

        private bool OnEventRegionClicked()
        {
            if (string.IsNullOrEmpty(selectedEventCode))
            {
                return true;
            }

            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetRegionEvent,
                Code = selectedEventCode,
                UpdateInstantly = true,
                UseAllowStop = true,
                AllowStop = selectedAllowStop
            });
            return true;
        }

        private bool OnEventGlobalClicked()
        {
            if (string.IsNullOrEmpty(selectedEventCode))
            {
                return true;
            }

            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetGlobalEvent,
                Code = selectedEventCode,
                UpdateInstantly = true,
                UseAllowStop = true,
                AllowStop = selectedAllowStop
            });
            return true;
        }

        private bool OnWindApplyClicked()
        {
            if (string.IsNullOrEmpty(selectedWindCode))
            {
                return true;
            }

            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetGlobalWind,
                Code = selectedWindCode,
                UpdateInstantly = true
            });
            return true;
        }

        private bool OnStormApplyClicked()
        {
            if (string.IsNullOrEmpty(selectedTemporalStormMode))
            {
                return true;
            }

            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetTemporalStormMode,
                Code = selectedTemporalStormMode
            });
            return true;
        }

        private bool OnAutoApplyClicked()
        {
            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetAutoChange,
                UseAutoChange = true,
                AutoChangeEnabled = selectedAutoChange
            });
            return true;
        }

        private bool OnPrecipApplyClicked()
        {
            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetPrecipitationOverride,
                UsePrecipitationOverride = true,
                PrecipitationOverride = selectedPrecipitation
            });
            return true;
        }

        private bool OnPrecipResetClicked()
        {
            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.ClearPrecipitationOverride
            });
            return true;
        }

        private bool OnPrecipSliderChanged(int value)
        {
            selectedPrecipitation = value / 100f;
            return true;
        }

        private void OnDialogTitleBarClosed()
        {
            TryClose();
        }

        private string UpdateDropDownSelection(string dropKey, WeatherOptionEntry[] options, string currentCode, string previousSelection)
        {
            var dropDown = SingleComposer.GetDropDown(dropKey);
            if (dropDown == null)
            {
                return previousSelection;
            }

            var validOptions = options.Where(option => option != null && !string.IsNullOrEmpty(option.Code)).ToArray();
            string[] values = validOptions.Select(option => option.Code).ToArray();
            string[] names = validOptions.Select(option => string.IsNullOrWhiteSpace(option.Name) ? option.Code : option.Name).ToArray();

            dropDown.SetList(values, names);

            if (values.Length == 0)
            {
                dropDown.SetSelectedIndex(-1);
                return null;
            }

            string targetCode = null;
            if (!string.IsNullOrEmpty(currentCode) && values.Contains(currentCode))
            {
                targetCode = currentCode;
            }
            else if (!string.IsNullOrEmpty(previousSelection) && values.Contains(previousSelection))
            {
                targetCode = previousSelection;
            }
            else
            {
                targetCode = values[0];
            }

            int index = Array.IndexOf(values, targetCode);
            dropDown.SetSelectedIndex(index);
            return targetCode;
        }
    }
}
