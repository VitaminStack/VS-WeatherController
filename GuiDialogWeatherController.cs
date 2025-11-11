using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace WeatherController
{
    public class GuiDialogWeatherController : GuiDialogGeneric
    {
        private const string AllowStopSwitchKey = "wc-allowstop";
        private const string AutoSwitchKey = "wc-autoswitch";
        private const string AutoApplyButtonKey = "wc-auto-apply";
        private const string PrecipSliderKey = "wc-precip-slider";
        private const string PrecipApplyButtonKey = "wc-precip-apply";
        private const string PrecipResetButtonKey = "wc-precip-reset";
        private const string PatternCurrentLabelKey = "wc-pattern-current";
        private const string EventCurrentLabelKey = "wc-event-current";
        private const string WindCurrentLabelKey = "wc-wind-current";
        private const string StormCurrentLabelKey = "wc-storm-current";
        private const string PatternRegionButtonPrefix = "wc-pattern-region-";
        private const string PatternGlobalButtonPrefix = "wc-pattern-global-";
        private const string EventRegionButtonPrefix = "wc-event-region-";
        private const string EventGlobalButtonPrefix = "wc-event-global-";
        private const string WindButtonPrefix = "wc-wind-";
        private const string StormButtonPrefix = "wc-storm-";
        private const string StormStopButtonKey = "wc-storm-stop";

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
            TemporalStormModes = Array.Empty<WeatherOptionEntry>(),
            TemporalStormActive = false
        };

        private bool selectedAllowStop = true;
        private bool selectedAutoChange;
        private float selectedPrecipitation;

        private readonly List<string> patternRegionButtonKeys = new List<string>();
        private readonly List<string> patternGlobalButtonKeys = new List<string>();
        private readonly List<string> eventRegionButtonKeys = new List<string>();
        private readonly List<string> eventGlobalButtonKeys = new List<string>();
        private readonly List<string> windButtonKeys = new List<string>();
        private readonly List<string> stormButtonKeys = new List<string>();

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
            ComposeDialog();
        }



        private void ComposeDialog()
        {
            patternRegionButtonKeys.Clear();
            patternGlobalButtonKeys.Clear();
            eventRegionButtonKeys.Clear();
            eventGlobalButtonKeys.Clear();
            windButtonKeys.Clear();
            stormButtonKeys.Clear();

            SingleComposer?.Dispose();

            EnsureDialogTheme();

            const double sectionWidth = 440;
            const double sectionPadding = 12;
            const double sectionSpacing = 12;
            const double rowSpacing = 8;
            const double buttonHeight = 28;
            const double switchHeight = 26;
            const double infoTextHeight = 20;
            const double headingHeight = 20;
            const double columnSpacing = 8;
            const double controlHeight = 32;
            const double actionWidth = 120;
            const double dialogOffsetY = 40;

            WeatherOptionEntry[] patternOptions = GetValidOptions(currentOptions.WeatherPatterns);
            WeatherOptionEntry[] eventOptions = GetValidOptions(currentOptions.WeatherEvents);
            WeatherOptionEntry[] windOptions = GetValidOptions(currentOptions.WindPatterns);
            WeatherOptionEntry[] stormOptions = GetValidOptions(currentOptions.TemporalStormModes);

            double patternButtonsHeight = patternOptions.Length > 0 ? CalculateRowHeight(patternOptions.Length, buttonHeight, rowSpacing) : infoTextHeight;
            double eventButtonsHeight = eventOptions.Length > 0 ? CalculateRowHeight(eventOptions.Length, buttonHeight, rowSpacing) : infoTextHeight;
            const int windColumns = 2;
            double windButtonsHeight = windOptions.Length > 0 ? CalculateGridHeight(windOptions.Length, windColumns, buttonHeight, rowSpacing) : infoTextHeight;
            const int stormColumns = 2;
            double stormButtonsHeight = stormOptions.Length > 0 ? CalculateGridHeight(stormOptions.Length, stormColumns, buttonHeight, rowSpacing) : infoTextHeight;

            double currentY = 0;

            ElementBounds contentBounds = ElementBounds.Fixed(0, 0, sectionWidth, 0);
            contentBounds.BothSizing = ElementSizing.Fixed;

            ElementBounds backgroundBounds = ElementStdBounds.DialogBackground()
                .WithFixedPadding(GuiStyle.ElementToDialogPadding, GuiStyle.ElementToDialogPadding);
            backgroundBounds.WithChild(contentBounds);

            ElementBounds patternSection = ElementBounds.Fixed(0, currentY, sectionWidth,
                sectionPadding * 2 + headingHeight + rowSpacing + infoTextHeight + rowSpacing + patternButtonsHeight);
            currentY += patternSection.fixedHeight + sectionSpacing;

            ElementBounds eventSection = ElementBounds.Fixed(0, currentY, sectionWidth,
                sectionPadding * 2 + headingHeight + rowSpacing + switchHeight + rowSpacing + infoTextHeight + rowSpacing + eventButtonsHeight);
            currentY += eventSection.fixedHeight + sectionSpacing;

            ElementBounds windSection = ElementBounds.Fixed(0, currentY, sectionWidth,
                sectionPadding * 2 + headingHeight + rowSpacing + infoTextHeight + rowSpacing + windButtonsHeight);
            currentY += windSection.fixedHeight + sectionSpacing;

            ElementBounds stormSection = ElementBounds.Fixed(0, currentY, sectionWidth,
                sectionPadding * 2 + headingHeight + rowSpacing + infoTextHeight + rowSpacing + stormButtonsHeight + rowSpacing + buttonHeight);
            currentY += stormSection.fixedHeight + sectionSpacing;

            ElementBounds autoSection = ElementBounds.Fixed(0, currentY, sectionWidth,
                sectionPadding * 2 + headingHeight + rowSpacing + Math.Max(switchHeight, buttonHeight));
            currentY += autoSection.fixedHeight + sectionSpacing;

            ElementBounds precipSection = ElementBounds.Fixed(0, currentY, sectionWidth,
                sectionPadding * 2 + headingHeight + rowSpacing + controlHeight + rowSpacing + buttonHeight);
            currentY += precipSection.fixedHeight;

            contentBounds.WithChildren(patternSection, eventSection, windSection, stormSection, autoSection, precipSection);
            contentBounds.fixedHeight = currentY;

            ElementBounds patternLabel = ElementBounds.Fixed(sectionPadding, sectionPadding, sectionWidth - 2 * sectionPadding, headingHeight);
            ElementBounds patternCurrentLabel = ElementBounds.Fixed(sectionPadding, patternLabel.fixedY + patternLabel.fixedHeight + rowSpacing, sectionWidth - 2 * sectionPadding, infoTextHeight);
            double patternButtonStartY = patternCurrentLabel.fixedY + patternCurrentLabel.fixedHeight + rowSpacing;
            double patternButtonWidth = (sectionWidth - 2 * sectionPadding - columnSpacing) / 2;

            ElementBounds eventLabel = ElementBounds.Fixed(sectionPadding, sectionPadding, sectionWidth - 2 * sectionPadding, headingHeight);
            ElementBounds allowStopLabel = ElementBounds.Fixed(sectionPadding, eventLabel.fixedY + eventLabel.fixedHeight + rowSpacing, sectionWidth - 2 * sectionPadding - (switchHeight + 6), switchHeight);
            ElementBounds allowStopSwitch = ElementBounds.Fixed(sectionWidth - sectionPadding - switchHeight - 6, allowStopLabel.fixedY - 1, switchHeight + 6, switchHeight + 4);
            ElementBounds eventCurrentLabel = ElementBounds.Fixed(sectionPadding, allowStopLabel.fixedY + allowStopLabel.fixedHeight + rowSpacing, sectionWidth - 2 * sectionPadding, infoTextHeight);
            double eventButtonStartY = eventCurrentLabel.fixedY + eventCurrentLabel.fixedHeight + rowSpacing;
            double eventButtonWidth = patternButtonWidth;

            ElementBounds windLabel = ElementBounds.Fixed(sectionPadding, sectionPadding, sectionWidth - 2 * sectionPadding, headingHeight);
            ElementBounds windCurrentLabel = ElementBounds.Fixed(sectionPadding, windLabel.fixedY + windLabel.fixedHeight + rowSpacing, sectionWidth - 2 * sectionPadding, infoTextHeight);
            double windButtonStartY = windCurrentLabel.fixedY + windCurrentLabel.fixedHeight + rowSpacing;
            double windButtonWidth = (sectionWidth - 2 * sectionPadding - (windColumns - 1) * columnSpacing) / windColumns;

            ElementBounds stormLabel = ElementBounds.Fixed(sectionPadding, sectionPadding, sectionWidth - 2 * sectionPadding, headingHeight);
            ElementBounds stormCurrentLabel = ElementBounds.Fixed(sectionPadding, stormLabel.fixedY + stormLabel.fixedHeight + rowSpacing, sectionWidth - 2 * sectionPadding, infoTextHeight);
            double stormButtonStartY = stormCurrentLabel.fixedY + stormCurrentLabel.fixedHeight + rowSpacing;
            double stormButtonWidth = (sectionWidth - 2 * sectionPadding - (stormColumns - 1) * columnSpacing) / stormColumns;
            ElementBounds stormStopButton = ElementBounds.Fixed(sectionPadding, stormButtonStartY + stormButtonsHeight + rowSpacing, sectionWidth - 2 * sectionPadding, buttonHeight);

            ElementBounds autoLabel = ElementBounds.Fixed(sectionPadding, sectionPadding, sectionWidth - 2 * sectionPadding, headingHeight);
            ElementBounds autoSwitch = ElementBounds.Fixed(sectionPadding, autoLabel.fixedY + autoLabel.fixedHeight + rowSpacing, switchHeight + 10, switchHeight + 4);
            ElementBounds autoApplyBtn = ElementBounds.Fixed(autoSwitch.fixedX + autoSwitch.fixedWidth + rowSpacing, autoSwitch.fixedY - 1, actionWidth, buttonHeight);

            ElementBounds precipLabel = ElementBounds.Fixed(sectionPadding, sectionPadding, sectionWidth - 2 * sectionPadding, headingHeight);
            ElementBounds precipSlider = ElementBounds.Fixed(sectionPadding, precipLabel.fixedY + precipLabel.fixedHeight + rowSpacing, sectionWidth - 2 * sectionPadding, controlHeight);
            ElementBounds precipApplyBtn = ElementBounds.Fixed(sectionPadding, precipSlider.fixedY + controlHeight + rowSpacing, actionWidth, buttonHeight);
            ElementBounds precipResetBtn = ElementBounds.Fixed(precipApplyBtn.fixedX + actionWidth + rowSpacing, precipApplyBtn.fixedY, actionWidth, buttonHeight);

            patternSection.WithChildren(patternLabel, patternCurrentLabel);
            eventSection.WithChildren(eventLabel, allowStopLabel, allowStopSwitch, eventCurrentLabel);
            windSection.WithChildren(windLabel, windCurrentLabel);
            stormSection.WithChildren(stormLabel, stormCurrentLabel, stormStopButton);
            autoSection.WithChildren(autoLabel, autoSwitch, autoApplyBtn);
            precipSection.WithChildren(precipLabel, precipSlider, precipApplyBtn, precipResetBtn);

            CairoFont headingFont = CairoFont.WhiteSmallishText();
            CairoFont bodyFont = CairoFont.WhiteSmallText();

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialogAtPos(dialogOffsetY)
                .WithFixedAlignmentOffset(0, GuiStyle.DialogToScreenPadding);

            var composer = capi.Gui.CreateCompo("weathercontroller", dialogBounds)
                .AddShadedDialogBG(backgroundBounds, true)
                .AddDialogTitleBar(DialogTitle, OnDialogTitleBarClosed)
                .BeginChildElements(contentBounds)
                .AddInset(patternSection, 6, 0.8f)
                .AddStaticText("Weather pattern", headingFont, patternLabel)
                .AddStaticText("Current: -", bodyFont, patternCurrentLabel, PatternCurrentLabelKey);

            if (patternOptions.Length > 0)
            {
                double patternY = patternButtonStartY;
                foreach (WeatherOptionEntry option in patternOptions)
                {
                    string code = option.Code;
                    string name = GetOptionDisplayName(option);
                    string regionKey = CreateUniqueKey(PatternRegionButtonPrefix, code, patternRegionButtonKeys);
                    string globalKey = CreateUniqueKey(PatternGlobalButtonPrefix, code, patternGlobalButtonKeys);

                    ElementBounds regionBounds = ElementBounds.Fixed(sectionPadding, patternY, patternButtonWidth, buttonHeight);
                    ElementBounds globalBounds = ElementBounds.Fixed(sectionPadding + patternButtonWidth + columnSpacing, patternY, patternButtonWidth, buttonHeight);

                    patternSection.WithChildren(regionBounds, globalBounds);

                    composer.AddSmallButton($"Region: {name}", () => OnPatternRegionClicked(code), regionBounds, EnumButtonStyle.Normal, regionKey);
                    composer.AddSmallButton($"Global: {name}", () => OnPatternGlobalClicked(code), globalBounds, EnumButtonStyle.Normal, globalKey);

                    patternRegionButtonKeys.Add(regionKey);
                    patternGlobalButtonKeys.Add(globalKey);

                    patternY += buttonHeight + rowSpacing;
                }
            }
            else
            {
                ElementBounds messageBounds = ElementBounds.Fixed(sectionPadding, patternButtonStartY, sectionWidth - 2 * sectionPadding, infoTextHeight);
                patternSection.WithChild(messageBounds);
                composer.AddStaticText("No weather patterns available.", bodyFont, messageBounds);
            }

            composer
                .AddInset(eventSection, 6, 0.8f)
                .AddStaticText("Weather event", headingFont, eventLabel)
                .AddStaticText("Allow event to end automatically", bodyFont, allowStopLabel)
                .AddSwitch(OnAllowStopToggled, allowStopSwitch, AllowStopSwitchKey)
                .AddStaticText("Current: -", bodyFont, eventCurrentLabel, EventCurrentLabelKey);

            if (eventOptions.Length > 0)
            {
                double eventY = eventButtonStartY;
                foreach (WeatherOptionEntry option in eventOptions)
                {
                    string code = option.Code;
                    string name = GetOptionDisplayName(option);
                    string regionKey = CreateUniqueKey(EventRegionButtonPrefix, code, eventRegionButtonKeys);
                    string globalKey = CreateUniqueKey(EventGlobalButtonPrefix, code, eventGlobalButtonKeys);

                    ElementBounds regionBounds = ElementBounds.Fixed(sectionPadding, eventY, eventButtonWidth, buttonHeight);
                    ElementBounds globalBounds = ElementBounds.Fixed(sectionPadding + eventButtonWidth + columnSpacing, eventY, eventButtonWidth, buttonHeight);

                    eventSection.WithChildren(regionBounds, globalBounds);

                    composer.AddSmallButton($"Region: {name}", () => OnEventRegionClicked(code), regionBounds, EnumButtonStyle.Normal, regionKey);
                    composer.AddSmallButton($"Global: {name}", () => OnEventGlobalClicked(code), globalBounds, EnumButtonStyle.Normal, globalKey);

                    eventRegionButtonKeys.Add(regionKey);
                    eventGlobalButtonKeys.Add(globalKey);

                    eventY += buttonHeight + rowSpacing;
                }
            }
            else
            {
                ElementBounds messageBounds = ElementBounds.Fixed(sectionPadding, eventButtonStartY, sectionWidth - 2 * sectionPadding, infoTextHeight);
                eventSection.WithChild(messageBounds);
                composer.AddStaticText("No weather events available.", bodyFont, messageBounds);
            }

            composer
                .AddInset(windSection, 6, 0.8f)
                .AddStaticText("Wind pattern", headingFont, windLabel)
                .AddStaticText("Current: -", bodyFont, windCurrentLabel, WindCurrentLabelKey);

            if (windOptions.Length > 0)
            {
                double windY = windButtonStartY;
                int column = 0;
                foreach (WeatherOptionEntry option in windOptions)
                {
                    string code = option.Code;
                    string name = GetOptionDisplayName(option);
                    string key = CreateUniqueKey(WindButtonPrefix, code, windButtonKeys);

                    double x = sectionPadding + column * (windButtonWidth + columnSpacing);
                    ElementBounds bounds = ElementBounds.Fixed(x, windY, windButtonWidth, buttonHeight);

                    windSection.WithChild(bounds);

                    composer.AddSmallButton(name, () => OnWindOptionClicked(code), bounds, EnumButtonStyle.Normal, key);
                    windButtonKeys.Add(key);

                    column++;
                    if (column >= windColumns)
                    {
                        column = 0;
                        windY += buttonHeight + rowSpacing;
                    }
                }
            }
            else
            {
                ElementBounds messageBounds = ElementBounds.Fixed(sectionPadding, windButtonStartY, sectionWidth - 2 * sectionPadding, infoTextHeight);
                windSection.WithChild(messageBounds);
                composer.AddStaticText("No wind patterns available.", bodyFont, messageBounds);
            }

            composer
                .AddInset(stormSection, 6, 0.8f)
                .AddStaticText("Temporal storms", headingFont, stormLabel)
                .AddStaticText("Mode: -", bodyFont, stormCurrentLabel, StormCurrentLabelKey);

            if (stormOptions.Length > 0)
            {
                double stormY = stormButtonStartY;
                int column = 0;
                foreach (WeatherOptionEntry option in stormOptions)
                {
                    string code = option.Code;
                    string name = GetOptionDisplayName(option);
                    string key = CreateUniqueKey(StormButtonPrefix, code, stormButtonKeys);

                    double x = sectionPadding + column * (stormButtonWidth + columnSpacing);
                    ElementBounds bounds = ElementBounds.Fixed(x, stormY, stormButtonWidth, buttonHeight);

                    stormSection.WithChild(bounds);

                    composer.AddSmallButton(name, () => OnStormModeButtonClicked(code), bounds, EnumButtonStyle.Normal, key);
                    stormButtonKeys.Add(key);

                    column++;
                    if (column >= stormColumns)
                    {
                        column = 0;
                        stormY += buttonHeight + rowSpacing;
                    }
                }
            }
            else
            {
                ElementBounds messageBounds = ElementBounds.Fixed(sectionPadding, stormButtonStartY, sectionWidth - 2 * sectionPadding, infoTextHeight);
                stormSection.WithChild(messageBounds);
                composer.AddStaticText("No temporal storm modes available.", bodyFont, messageBounds);
            }

            composer.AddSmallButton("End active storm", OnStormStopClicked, stormStopButton, EnumButtonStyle.Normal, StormStopButtonKey);

            composer
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

            SingleComposer = composer;

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

            selectedAllowStop = packet.CurrentEventAllowStop ?? selectedAllowStop;
            var allowStopSwitch = SingleComposer.GetSwitch(AllowStopSwitchKey);
            allowStopSwitch?.SetValue(selectedAllowStop);

            selectedAutoChange = packet.AutoChangeEnabled;
            SingleComposer.GetSwitch(AutoSwitchKey)?.SetValue(selectedAutoChange);

            selectedPrecipitation = packet.OverridePrecipitation ?? 0f;
            var slider = SingleComposer.GetSlider(PrecipSliderKey);
            slider?.SetValues((int)Math.Round(selectedPrecipitation * 100f), -100, 100, 5, "%");

            UpdateCurrentLabels(packet);
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            if (SingleComposer == null)
            {
                return;
            }

            bool canControl = currentOptions.HasControlPrivilege;
            bool regionAvailable = currentOptions.RegionAvailable && canControl;

            foreach (string key in patternRegionButtonKeys)
            {
                var button = SingleComposer.GetButton(key);
                if (button != null)
                {
                    button.Enabled = regionAvailable;
                }
            }

            foreach (string key in patternGlobalButtonKeys)
            {
                var button = SingleComposer.GetButton(key);
                if (button != null)
                {
                    button.Enabled = canControl;
                }
            }

            foreach (string key in eventRegionButtonKeys)
            {
                var button = SingleComposer.GetButton(key);
                if (button != null)
                {
                    button.Enabled = regionAvailable;
                }
            }

            foreach (string key in eventGlobalButtonKeys)
            {
                var button = SingleComposer.GetButton(key);
                if (button != null)
                {
                    button.Enabled = canControl;
                }
            }

            foreach (string key in windButtonKeys)
            {
                var button = SingleComposer.GetButton(key);
                if (button != null)
                {
                    button.Enabled = canControl;
                }
            }

            foreach (string key in stormButtonKeys)
            {
                var button = SingleComposer.GetButton(key);
                if (button != null)
                {
                    button.Enabled = canControl;
                }
            }

            var stopButton = SingleComposer.GetButton(StormStopButtonKey);
            if (stopButton != null)
            {
                stopButton.Enabled = canControl && currentOptions.TemporalStormActive;
            }

            var allowStopSwitch = SingleComposer.GetSwitch(AllowStopSwitchKey);
            if (allowStopSwitch != null)
            {
                allowStopSwitch.Enabled = canControl;
            }

            var autoSwitch = SingleComposer.GetSwitch(AutoSwitchKey);
            if (autoSwitch != null)
            {
                autoSwitch.Enabled = canControl;
            }

            var autoApplyButton = SingleComposer.GetButton(AutoApplyButtonKey);
            if (autoApplyButton != null)
            {
                autoApplyButton.Enabled = canControl;
            }

            var precipApplyButton = SingleComposer.GetButton(PrecipApplyButtonKey);
            if (precipApplyButton != null)
            {
                precipApplyButton.Enabled = canControl;
            }

            var precipResetButton = SingleComposer.GetButton(PrecipResetButtonKey);
            if (precipResetButton != null)
            {
                precipResetButton.Enabled = canControl;
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



        private bool OnPatternRegionClicked(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return true;
            }

            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetRegionPattern,
                Code = code,
                UpdateInstantly = true
            });
            return true;
        }

        private bool OnPatternGlobalClicked(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return true;
            }

            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetGlobalPattern,
                Code = code,
                UpdateInstantly = true
            });
            return true;
        }

        private bool OnEventRegionClicked(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return true;
            }

            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetRegionEvent,
                Code = code,
                UpdateInstantly = true,
                UseAllowStop = true,
                AllowStop = selectedAllowStop
            });
            return true;
        }

        private bool OnEventGlobalClicked(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return true;
            }

            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetGlobalEvent,
                Code = code,
                UpdateInstantly = true,
                UseAllowStop = true,
                AllowStop = selectedAllowStop
            });
            return true;
        }

        private bool OnWindOptionClicked(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return true;
            }

            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetGlobalWind,
                Code = code,
                UpdateInstantly = true
            });
            return true;
        }

        private bool OnStormModeButtonClicked(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return true;
            }

            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetTemporalStormMode,
                Code = code
            });
            return true;
        }

        private bool OnStormStopClicked()
        {
            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.EndTemporalStorm
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

        private void UpdateCurrentLabels(WeatherOptionsPacket packet)
        {
            var patternLabel = SingleComposer.GetStaticText(PatternCurrentLabelKey);
            if (patternLabel != null)
            {
                patternLabel.SetValue($"Current: {FormatCurrentValue(packet.WeatherPatterns, packet.CurrentPatternCode, "None")}");
            }

            var eventLabel = SingleComposer.GetStaticText(EventCurrentLabelKey);
            if (eventLabel != null)
            {
                string eventText = FormatCurrentValue(packet.WeatherEvents, packet.CurrentEventCode, "None");
                string allowStopText = string.Empty;
                if (!string.IsNullOrEmpty(packet.CurrentEventCode))
                {
                    bool allowStop = packet.CurrentEventAllowStop ?? true;
                    allowStopText = $" (Allow stop: {(allowStop ? "Yes" : "No")})";
                }
                eventLabel.SetValue($"Current: {eventText}{allowStopText}");
            }

            var windLabel = SingleComposer.GetStaticText(WindCurrentLabelKey);
            if (windLabel != null)
            {
                windLabel.SetValue($"Current: {FormatCurrentValue(packet.WindPatterns, packet.CurrentWindCode, "None")}");
            }

            var stormLabel = SingleComposer.GetStaticText(StormCurrentLabelKey);
            if (stormLabel != null)
            {
                string mode = FormatCurrentValue(packet.TemporalStormModes, packet.CurrentTemporalStormMode, "Off");
                string status = packet.TemporalStormActive ? "Active" : "Inactive";
                stormLabel.SetValue($"Mode: {mode} (Status: {status})");
            }
        }

        private static WeatherOptionEntry[] GetValidOptions(WeatherOptionEntry[] options)
        {
            return options?.Where(option => option != null && !string.IsNullOrEmpty(option.Code)).ToArray() ?? Array.Empty<WeatherOptionEntry>();
        }

        private static string GetOptionDisplayName(WeatherOptionEntry option)
        {
            if (option == null)
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(option.Name) ? option.Code : option.Name;
        }

        private static string FormatCurrentValue(WeatherOptionEntry[] options, string code, string emptyText)
        {
            if (string.IsNullOrEmpty(code))
            {
                return emptyText;
            }

            WeatherOptionEntry match = options?.FirstOrDefault(opt => opt != null && opt.Code == code);
            if (match != null)
            {
                return GetOptionDisplayName(match);
            }

            return code;
        }

        private static double CalculateRowHeight(int rowCount, double buttonHeight, double rowSpacing)
        {
            if (rowCount <= 0)
            {
                return 0;
            }

            return rowCount * buttonHeight + (rowCount - 1) * rowSpacing;
        }

        private static double CalculateGridHeight(int itemCount, int columns, double buttonHeight, double rowSpacing)
        {
            if (itemCount <= 0 || columns <= 0)
            {
                return 0;
            }

            int rows = (itemCount + columns - 1) / columns;
            return CalculateRowHeight(rows, buttonHeight, rowSpacing);
        }

        private static string CreateUniqueKey(string prefix, string code, ICollection<string> existingKeys)
        {
            string sanitized = SanitizeKey(code);
            string baseKey = prefix + sanitized;

            if (existingKeys == null || !existingKeys.Contains(baseKey))
            {
                return baseKey;
            }

            int suffix = 1;
            string candidate;
            do
            {
                candidate = string.Format("{0}-{1}", baseKey, suffix++);
            }
            while (existingKeys.Contains(candidate));

            return candidate;
        }

        private static string SanitizeKey(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(code.Length);
            foreach (char c in code)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    builder.Append(c);
                }
                else
                {
                    builder.Append('_');
                }
            }
            return builder.ToString();
        }
    }
}
