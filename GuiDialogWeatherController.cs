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

        private static readonly double[] DefaultLightBgColor =
        {
            0x40 / 255d,
            0x35 / 255d,
            0x29 / 255d,
            0.75
        };

        private static readonly double[] DefaultDialogBgColor =
        {
            0x40 / 255d,
            0x35 / 255d,
            0x29 / 255d,
            0.85
        };

        private static readonly double[] DefaultStrongBgColor =
        {
            0x40 / 255d,
            0x35 / 255d,
            0x29 / 255d,
            0.95
        };

        private static readonly double[] DefaultHighlightColor =
        {
            0xA8 / 255d,
            0x8B / 255d,
            0x6C / 255d,
            0.85
        };

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
            EnsureDialogTheme();

            const double sectionWidth = 440;
            const double sectionPadding = 12;
            const double sectionSpacing = 12;
            const double rowSpacing = 8;
            const double controlHeight = 32;
            const double buttonHeight = 28;
            const double switchHeight = 26;
            const double actionWidth = 120;
            const double dialogOffsetY = 40;

            double currentY = 0;

            ElementBounds contentBounds = ElementBounds.Fixed(0, 0, sectionWidth, 0);
            contentBounds.BothSizing = ElementSizing.Fixed;

            ElementBounds backgroundBounds = ElementStdBounds.DialogBackground()
                .WithFixedPadding(GuiStyle.ElementToDialogPadding, GuiStyle.ElementToDialogPadding);
            backgroundBounds.WithChild(contentBounds);

            // Weather pattern section
            double patternHeight = sectionPadding * 2 + 20 + rowSpacing + controlHeight + rowSpacing + buttonHeight;
            ElementBounds patternSection = ElementBounds.Fixed(0, currentY, sectionWidth, patternHeight);
            currentY += patternHeight + sectionSpacing;

            ElementBounds patternLabel = ElementBounds.Fixed(sectionPadding, sectionPadding, sectionWidth - 2 * sectionPadding, 20);
            double patternDropWidth = sectionWidth - (2 * sectionPadding) - actionWidth - rowSpacing;
            ElementBounds patternDrop = ElementBounds.Fixed(sectionPadding, patternLabel.fixedY + patternLabel.fixedHeight + rowSpacing, patternDropWidth, controlHeight);
            ElementBounds patternRegionBtn = ElementBounds.Fixed(patternDrop.fixedX + patternDropWidth + rowSpacing, patternDrop.fixedY, actionWidth, buttonHeight);
            ElementBounds patternGlobalBtn = ElementBounds.Fixed(patternRegionBtn.fixedX, patternRegionBtn.fixedY + buttonHeight + 4, actionWidth, buttonHeight);
            patternSection.WithChildren(patternLabel, patternDrop, patternRegionBtn, patternGlobalBtn);

            // Weather event section
            double eventHeight = sectionPadding * 2 + 20 + rowSpacing + switchHeight + rowSpacing + controlHeight + rowSpacing + buttonHeight;
            ElementBounds eventSection = ElementBounds.Fixed(0, currentY, sectionWidth, eventHeight);
            currentY += eventHeight + sectionSpacing;

            ElementBounds eventLabel = ElementBounds.Fixed(sectionPadding, sectionPadding, sectionWidth - 2 * sectionPadding, 20);
            ElementBounds allowStopLabel = ElementBounds.Fixed(sectionPadding, eventLabel.fixedY + eventLabel.fixedHeight + rowSpacing, sectionWidth - 2 * sectionPadding - (switchHeight + 6), switchHeight);
            ElementBounds allowStopSwitch = ElementBounds.Fixed(sectionWidth - sectionPadding - switchHeight - 6, allowStopLabel.fixedY - 1, switchHeight + 6, switchHeight + 4);
            double eventDropWidth = patternDropWidth;
            ElementBounds eventDrop = ElementBounds.Fixed(sectionPadding, allowStopLabel.fixedY + allowStopLabel.fixedHeight + rowSpacing, eventDropWidth, controlHeight);
            ElementBounds eventRegionBtn = ElementBounds.Fixed(eventDrop.fixedX + eventDropWidth + rowSpacing, eventDrop.fixedY, actionWidth, buttonHeight);
            ElementBounds eventGlobalBtn = ElementBounds.Fixed(eventRegionBtn.fixedX, eventRegionBtn.fixedY + buttonHeight + 4, actionWidth, buttonHeight);
            eventSection.WithChildren(eventLabel, allowStopLabel, allowStopSwitch, eventDrop, eventRegionBtn, eventGlobalBtn);

            // Wind section
            double windHeight = sectionPadding * 2 + 20 + rowSpacing + controlHeight;
            ElementBounds windSection = ElementBounds.Fixed(0, currentY, sectionWidth, windHeight);
            currentY += windHeight + sectionSpacing;

            ElementBounds windLabel = ElementBounds.Fixed(sectionPadding, sectionPadding, sectionWidth - 2 * sectionPadding, 20);
            ElementBounds windDrop = ElementBounds.Fixed(sectionPadding, windLabel.fixedY + windLabel.fixedHeight + rowSpacing, patternDropWidth, controlHeight);
            ElementBounds windApplyBtn = ElementBounds.Fixed(windDrop.fixedX + patternDropWidth + rowSpacing, windDrop.fixedY, actionWidth, buttonHeight);
            windSection.WithChildren(windLabel, windDrop, windApplyBtn);

            // Temporal storm section
            double stormHeight = sectionPadding * 2 + 20 + rowSpacing + controlHeight;
            ElementBounds stormSection = ElementBounds.Fixed(0, currentY, sectionWidth, stormHeight);
            currentY += stormHeight + sectionSpacing;

            ElementBounds stormLabel = ElementBounds.Fixed(sectionPadding, sectionPadding, sectionWidth - 2 * sectionPadding, 20);
            ElementBounds stormDrop = ElementBounds.Fixed(sectionPadding, stormLabel.fixedY + stormLabel.fixedHeight + rowSpacing, patternDropWidth, controlHeight);
            ElementBounds stormApplyBtn = ElementBounds.Fixed(stormDrop.fixedX + patternDropWidth + rowSpacing, stormDrop.fixedY, actionWidth, buttonHeight);
            stormSection.WithChildren(stormLabel, stormDrop, stormApplyBtn);

            // Auto-change section
            double autoHeight = sectionPadding * 2 + 20 + rowSpacing + Math.Max(switchHeight, buttonHeight);
            ElementBounds autoSection = ElementBounds.Fixed(0, currentY, sectionWidth, autoHeight);
            currentY += autoHeight + sectionSpacing;

            ElementBounds autoLabel = ElementBounds.Fixed(sectionPadding, sectionPadding, sectionWidth - 2 * sectionPadding, 20);
            ElementBounds autoSwitch = ElementBounds.Fixed(sectionPadding, autoLabel.fixedY + autoLabel.fixedHeight + rowSpacing, switchHeight + 10, switchHeight + 4);
            ElementBounds autoApplyBtn = ElementBounds.Fixed(autoSwitch.fixedX + autoSwitch.fixedWidth + rowSpacing, autoSwitch.fixedY - 1, actionWidth, buttonHeight);
            autoSection.WithChildren(autoLabel, autoSwitch, autoApplyBtn);

            // Precipitation section
            double precipHeight = sectionPadding * 2 + 20 + rowSpacing + controlHeight + rowSpacing + buttonHeight;
            ElementBounds precipSection = ElementBounds.Fixed(0, currentY, sectionWidth, precipHeight);
            currentY += precipHeight;

            ElementBounds precipLabel = ElementBounds.Fixed(sectionPadding, sectionPadding, sectionWidth - 2 * sectionPadding, 20);
            ElementBounds precipSlider = ElementBounds.Fixed(sectionPadding, precipLabel.fixedY + precipLabel.fixedHeight + rowSpacing, sectionWidth - 2 * sectionPadding, controlHeight);
            ElementBounds precipApplyBtn = ElementBounds.Fixed(sectionPadding, precipSlider.fixedY + controlHeight + rowSpacing, actionWidth, buttonHeight);
            ElementBounds precipResetBtn = ElementBounds.Fixed(precipApplyBtn.fixedX + actionWidth + rowSpacing, precipApplyBtn.fixedY, actionWidth, buttonHeight);
            precipSection.WithChildren(precipLabel, precipSlider, precipApplyBtn, precipResetBtn);

            contentBounds.WithChildren(patternSection, eventSection, windSection, stormSection, autoSection, precipSection);
            contentBounds.fixedHeight = currentY;

            CairoFont headingFont = CairoFont.WhiteSmallishText();
            CairoFont bodyFont = CairoFont.WhiteSmallText();

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialogAtPos(dialogOffsetY)
                .WithFixedAlignmentOffset(0, GuiStyle.DialogToScreenPadding);

            SingleComposer = capi.Gui.CreateCompo("weathercontroller", dialogBounds)
                .AddShadedDialogBG(backgroundBounds, true)
                .AddDialogTitleBar(DialogTitle, OnDialogTitleBarClosed)
                .BeginChildElements(contentBounds)
                .AddInset(patternSection, 6, 0.8f)
                .AddStaticText("Weather pattern", headingFont, patternLabel)
                .AddDropDown(Array.Empty<string>(), Array.Empty<string>(), 0, OnPatternChanged, patternDrop, bodyFont, PatternDropKey)
                .AddSmallButton("Region", OnPatternRegionClicked, patternRegionBtn, EnumButtonStyle.Normal, PatternRegionButtonKey)
                .AddSmallButton("Global", OnPatternGlobalClicked, patternGlobalBtn, EnumButtonStyle.Normal, PatternGlobalButtonKey)
                .AddInset(eventSection, 6, 0.8f)
                .AddStaticText("Weather event", headingFont, eventLabel)
                .AddStaticText("Allow event to end automatically", bodyFont, allowStopLabel)
                .AddSwitch(OnAllowStopToggled, allowStopSwitch, AllowStopSwitchKey)
                .AddDropDown(Array.Empty<string>(), Array.Empty<string>(), 0, OnEventChanged, eventDrop, bodyFont, EventDropKey)
                .AddSmallButton("Region", OnEventRegionClicked, eventRegionBtn, EnumButtonStyle.Normal, EventRegionButtonKey)
                .AddSmallButton("Global", OnEventGlobalClicked, eventGlobalBtn, EnumButtonStyle.Normal, EventGlobalButtonKey)
                .AddInset(windSection, 6, 0.8f)
                .AddStaticText("Wind pattern", headingFont, windLabel)
                .AddDropDown(Array.Empty<string>(), Array.Empty<string>(), 0, OnWindChanged, windDrop, bodyFont, WindDropKey)
                .AddSmallButton("Apply", OnWindApplyClicked, windApplyBtn, EnumButtonStyle.Normal, WindApplyButtonKey)
                .AddInset(stormSection, 6, 0.8f)
                .AddStaticText("Temporal storms", headingFont, stormLabel)
                .AddDropDown(Array.Empty<string>(), Array.Empty<string>(), 0, OnStormModeChanged, stormDrop, bodyFont, StormDropKey)
                .AddSmallButton("Apply", OnStormApplyClicked, stormApplyBtn, EnumButtonStyle.Normal, StormApplyButtonKey)
                .AddInset(autoSection, 6, 0.8f)
                .AddStaticText("Auto-change patterns", headingFont, autoLabel)
                .AddSwitch(OnAutoSwitchToggled, autoSwitch, AutoSwitchKey)
                .AddSmallButton("Apply", OnAutoApplyClicked, autoApplyBtn, EnumButtonStyle.Normal, AutoApplyButtonKey)
                .AddInset(precipSection, 6, 0.8f)
                .AddStaticText("Precipitation override", headingFont, precipLabel)
                .AddSlider(OnPrecipSliderChanged, precipSlider, PrecipSliderKey)
                .AddSmallButton("Apply", OnPrecipApplyClicked, precipApplyBtn, EnumButtonStyle.Normal, PrecipApplyButtonKey)
                .AddSmallButton("Reset", OnPrecipResetClicked, precipResetBtn, EnumButtonStyle.Normal, PrecipResetButtonKey)
                .EndChildElements()
                .Compose();

            UpdateFromPacket(currentOptions);
        }

        private void EnsureDialogTheme()
        {
            GuiStyle.DialogLightBgColor = EnsureColor(GuiStyle.DialogLightBgColor, DefaultLightBgColor);
            GuiStyle.DialogDefaultBgColor = EnsureColor(GuiStyle.DialogDefaultBgColor, DefaultDialogBgColor);
            GuiStyle.DialogStrongBgColor = EnsureColor(GuiStyle.DialogStrongBgColor, DefaultStrongBgColor);
            GuiStyle.DialogHighlightColor = EnsureColor(GuiStyle.DialogHighlightColor, DefaultHighlightColor);
        }

        private double[] EnsureColor(double[] color, double[] fallback)
        {
            if (color == null || color.Length < 3)
            {
                return (double[])fallback.Clone();
            }

            double[] adjusted = (double[])color.Clone();
            if (adjusted.Length < 4)
            {
                Array.Resize(ref adjusted, 4);
            }

            if (adjusted[3] <= 0.05)
            {
                adjusted[3] = fallback.Length > 3 ? fallback[3] : 1.0;
            }

            bool lacksColor = adjusted[0] <= 0.001 && adjusted[1] <= 0.001 && adjusted[2] <= 0.001;
            if (lacksColor && fallback.Length >= 3)
            {
                adjusted[0] = fallback[0];
                adjusted[1] = fallback[1];
                adjusted[2] = fallback[2];
            }

            return adjusted;
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
