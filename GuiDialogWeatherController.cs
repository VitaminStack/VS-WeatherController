using System;
using System.Linq;
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
        private const string PatternRegionDropdownKey = "wc-pattern-region";
        private const string PatternGlobalDropdownKey = "wc-pattern-global";
        private const string EventRegionDropdownKey = "wc-event-region";
        private const string EventGlobalDropdownKey = "wc-event-global";
        private const string WindDropdownKey = "wc-wind";
        private const string StormDropdownKey = "wc-storm";
        private const string StormSummonButtonKey = "wc-storm-summon";
        private const string StormStopButtonKey = "wc-storm-stop";
        private const string PatternRegionLockSwitchKey = "wc-pattern-region-lock";
        private const string PatternGlobalLockSwitchKey = "wc-pattern-global-lock";
        private const string EventRegionLockSwitchKey = "wc-event-region-lock";
        private const string EventGlobalLockSwitchKey = "wc-event-global-lock";
        private const string WindLockSwitchKey = "wc-wind-lock";
        private const string StormLockSwitchKey = "wc-storm-lock";

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
        private bool lockRegionPattern;
        private bool lockGlobalPattern;
        private bool lockRegionEvent;
        private bool lockGlobalEvent;
        private bool lockWind;
        private bool lockStorm;

        private string[] patternCodes = Array.Empty<string>();
        private string[] patternNames = Array.Empty<string>();
        private string[] eventCodes = Array.Empty<string>();
        private string[] eventNames = Array.Empty<string>();
        private string[] windCodes = Array.Empty<string>();
        private string[] windNames = Array.Empty<string>();
        private string[] stormCodes = Array.Empty<string>();
        private string[] stormNames = Array.Empty<string>();

        private WeatherOptionEntry[] patternEntries = Array.Empty<WeatherOptionEntry>();
        private WeatherOptionEntry[] eventEntries = Array.Empty<WeatherOptionEntry>();
        private WeatherOptionEntry[] windEntries = Array.Empty<WeatherOptionEntry>();
        private WeatherOptionEntry[] stormEntries = Array.Empty<WeatherOptionEntry>();

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
            SingleComposer?.Dispose();

            EnsureDialogTheme();

            const double sectionWidth = 460;
            const double sectionPadding = 12;
            const double sectionSpacing = 12;
            const double rowSpacing = 8;
            const double dropDownHeight = 28;
            const double buttonHeight = 28;
            const double switchHeight = 26;
            const double infoTextHeight = 20;
            const double headingHeight = 20;
            const double controlHeight = 32;
            const double actionWidth = 120;
            const double dialogOffsetY = 40;
            const double lockColumnWidth = 200;
            const double switchColumnWidth = 50;
            const double lockLabelWidth = lockColumnWidth - switchColumnWidth - rowSpacing;
            const double buttonSpacing = 10;

            patternEntries = GetValidOptions(currentOptions.WeatherPatterns);
            eventEntries = GetValidOptions(currentOptions.WeatherEvents);
            windEntries = GetValidOptions(currentOptions.WindPatterns);
            stormEntries = GetValidOptions(currentOptions.TemporalStormModes);

            patternCodes = patternEntries.Select(option => option.Code).ToArray();
            patternNames = patternEntries.Select(GetOptionDisplayName).ToArray();
            eventCodes = eventEntries.Select(option => option.Code).ToArray();
            eventNames = eventEntries.Select(GetOptionDisplayName).ToArray();
            windCodes = windEntries.Select(option => option.Code).ToArray();
            windNames = windEntries.Select(GetOptionDisplayName).ToArray();
            stormCodes = stormEntries.Select(option => option.Code).ToArray();
            stormNames = stormEntries.Select(GetOptionDisplayName).ToArray();

            double patternContentHeight = patternEntries.Length > 0
                ? 2 * dropDownHeight + rowSpacing
                : infoTextHeight;
            double eventContentHeight = eventEntries.Length > 0
                ? 2 * dropDownHeight + rowSpacing
                : infoTextHeight;
            double windContentHeight = windEntries.Length > 0
                ? dropDownHeight
                : infoTextHeight;
            double stormContentHeight = stormEntries.Length > 0
                ? dropDownHeight
                : infoTextHeight;

            double contentTopOffset = GuiStyle.TitleBarHeight + sectionPadding;
            double currentY = 0;

            ElementBounds contentBounds = ElementBounds.Fixed(0, contentTopOffset, sectionWidth, 0);
            contentBounds.BothSizing = ElementSizing.Fixed;

            ElementBounds backgroundBounds = ElementStdBounds.DialogBackground()
                .WithFixedPadding(GuiStyle.ElementToDialogPadding, GuiStyle.ElementToDialogPadding);
            backgroundBounds.WithChild(contentBounds);

            ElementBounds patternSection = ElementBounds.Fixed(0, currentY, sectionWidth,
                sectionPadding * 2 + headingHeight + rowSpacing + infoTextHeight + rowSpacing + patternContentHeight);
            currentY += patternSection.fixedHeight + sectionSpacing;

            ElementBounds eventSection = ElementBounds.Fixed(0, currentY, sectionWidth,
                sectionPadding * 2 + headingHeight + rowSpacing + switchHeight + rowSpacing + infoTextHeight + rowSpacing + eventContentHeight);
            currentY += eventSection.fixedHeight + sectionSpacing;

            ElementBounds windSection = ElementBounds.Fixed(0, currentY, sectionWidth,
                sectionPadding * 2 + headingHeight + rowSpacing + infoTextHeight + rowSpacing + windContentHeight);
            currentY += windSection.fixedHeight + sectionSpacing;

            ElementBounds stormSection = ElementBounds.Fixed(0, currentY, sectionWidth,
                sectionPadding * 2 + headingHeight + rowSpacing + infoTextHeight + rowSpacing + stormContentHeight + rowSpacing + buttonHeight);
            currentY += stormSection.fixedHeight + sectionSpacing;

            ElementBounds autoSection = ElementBounds.Fixed(0, currentY, sectionWidth,
                sectionPadding * 2 + headingHeight + rowSpacing + Math.Max(switchHeight, buttonHeight));
            currentY += autoSection.fixedHeight + sectionSpacing;

            ElementBounds precipSection = ElementBounds.Fixed(0, currentY, sectionWidth,
                sectionPadding * 2 + headingHeight + rowSpacing + controlHeight + rowSpacing + buttonHeight);
            currentY += precipSection.fixedHeight;

            contentBounds.WithChildren(patternSection, eventSection, windSection, stormSection, autoSection, precipSection);
            contentBounds.fixedHeight = currentY;

            string patternCurrentLabelText = BuildPatternCurrentLabelText(currentOptions);
            string eventCurrentLabelText = BuildEventCurrentLabelText(currentOptions);
            string windCurrentLabelText = BuildWindCurrentLabelText(currentOptions);
            string stormCurrentLabelText = BuildStormCurrentLabelText(currentOptions);

            ElementBounds patternLabel = ElementBounds.Fixed(sectionPadding, sectionPadding, sectionWidth - 2 * sectionPadding, headingHeight);
            ElementBounds patternCurrentLabel = ElementBounds.Fixed(sectionPadding, patternLabel.fixedY + patternLabel.fixedHeight + rowSpacing, sectionWidth - 2 * sectionPadding, infoTextHeight);
            double patternOptionsStartY = patternCurrentLabel.fixedY + patternCurrentLabel.fixedHeight + rowSpacing;

            ElementBounds eventLabel = ElementBounds.Fixed(sectionPadding, sectionPadding, sectionWidth - 2 * sectionPadding, headingHeight);
            ElementBounds allowStopLabel = ElementBounds.Fixed(sectionPadding, eventLabel.fixedY + eventLabel.fixedHeight + rowSpacing, sectionWidth - 2 * sectionPadding - (switchHeight + 6), switchHeight);
            ElementBounds allowStopSwitch = ElementBounds.Fixed(sectionWidth - sectionPadding - switchHeight - 6, allowStopLabel.fixedY - 1, switchHeight + 6, switchHeight + 4);
            ElementBounds eventCurrentLabel = ElementBounds.Fixed(sectionPadding, allowStopLabel.fixedY + allowStopLabel.fixedHeight + rowSpacing, sectionWidth - 2 * sectionPadding, infoTextHeight);
            double eventOptionsStartY = eventCurrentLabel.fixedY + eventCurrentLabel.fixedHeight + rowSpacing;

            ElementBounds windLabel = ElementBounds.Fixed(sectionPadding, sectionPadding, sectionWidth - 2 * sectionPadding, headingHeight);
            ElementBounds windCurrentLabel = ElementBounds.Fixed(sectionPadding, windLabel.fixedY + windLabel.fixedHeight + rowSpacing, sectionWidth - 2 * sectionPadding, infoTextHeight);
            double windOptionsStartY = windCurrentLabel.fixedY + windCurrentLabel.fixedHeight + rowSpacing;

            ElementBounds stormLabel = ElementBounds.Fixed(sectionPadding, sectionPadding, sectionWidth - 2 * sectionPadding, headingHeight);
            ElementBounds stormCurrentLabel = ElementBounds.Fixed(sectionPadding, stormLabel.fixedY + stormLabel.fixedHeight + rowSpacing, sectionWidth - 2 * sectionPadding, infoTextHeight);
            double stormOptionsStartY = stormCurrentLabel.fixedY + stormCurrentLabel.fixedHeight + rowSpacing;
            double stormButtonsY = stormOptionsStartY + Math.Max(stormContentHeight, infoTextHeight) + rowSpacing;
            double stormButtonWidth = (sectionWidth - 2 * sectionPadding - buttonSpacing) / 2;
            ElementBounds stormSummonButton = ElementBounds.Fixed(sectionPadding, stormButtonsY, stormButtonWidth, buttonHeight);
            ElementBounds stormStopButton = ElementBounds.Fixed(stormSummonButton.fixedX + stormSummonButton.fixedWidth + buttonSpacing, stormButtonsY, stormButtonWidth, buttonHeight);

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
            stormSection.WithChildren(stormLabel, stormCurrentLabel, stormSummonButton, stormStopButton);
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
                .AddStaticText(patternCurrentLabelText, bodyFont, patternCurrentLabel, PatternCurrentLabelKey);

            if (patternEntries.Length > 0)
            {
                double dropWidth = sectionWidth - 2 * sectionPadding - lockColumnWidth;
                ElementBounds regionBounds = ElementBounds.Fixed(sectionPadding, patternOptionsStartY, dropWidth, dropDownHeight);
                double regionLockY = regionBounds.fixedY + (dropDownHeight - switchHeight) / 2;
                ElementBounds regionLockSwitch = ElementBounds.Fixed(regionBounds.fixedX + regionBounds.fixedWidth + rowSpacing, regionLockY, switchColumnWidth, switchHeight);
                ElementBounds regionLockLabel = ElementBounds.Fixed(regionLockSwitch.fixedX + regionLockSwitch.fixedWidth + rowSpacing, regionBounds.fixedY + 4, lockLabelWidth, dropDownHeight - 8);

                double globalY = regionBounds.fixedY + dropDownHeight + rowSpacing;
                ElementBounds globalBounds = ElementBounds.Fixed(sectionPadding, globalY, dropWidth, dropDownHeight);
                double globalLockY = globalBounds.fixedY + (dropDownHeight - switchHeight) / 2;
                ElementBounds globalLockSwitch = ElementBounds.Fixed(globalBounds.fixedX + globalBounds.fixedWidth + rowSpacing, globalLockY, switchColumnWidth, switchHeight);
                ElementBounds globalLockLabel = ElementBounds.Fixed(globalLockSwitch.fixedX + globalLockSwitch.fixedWidth + rowSpacing, globalBounds.fixedY + 4, lockLabelWidth, dropDownHeight - 8);

                patternSection.WithChildren(regionBounds, regionLockSwitch, regionLockLabel, globalBounds, globalLockSwitch, globalLockLabel);

                composer.AddDropDown(patternCodes, patternNames, GetSelectedIndex(patternCodes, currentOptions.CurrentPatternCode), OnPatternRegionSelectionChanged, regionBounds, PatternRegionDropdownKey);
                composer.AddSwitch(OnPatternRegionLockToggled, regionLockSwitch, PatternRegionLockSwitchKey);
                composer.AddStaticText("Lock region pattern", bodyFont, regionLockLabel);

                composer.AddDropDown(patternCodes, patternNames, GetSelectedIndex(patternCodes, currentOptions.CurrentPatternCode), OnPatternGlobalSelectionChanged, globalBounds, PatternGlobalDropdownKey);
                composer.AddSwitch(OnPatternGlobalLockToggled, globalLockSwitch, PatternGlobalLockSwitchKey);
                composer.AddStaticText("Lock global pattern", bodyFont, globalLockLabel);
            }
            else
            {
                ElementBounds messageBounds = ElementBounds.Fixed(sectionPadding, patternOptionsStartY, sectionWidth - 2 * sectionPadding, infoTextHeight);
                patternSection.WithChild(messageBounds);
                composer.AddStaticText("No weather patterns available.", bodyFont, messageBounds);
            }

            composer
                .AddInset(eventSection, 6, 0.8f)
                .AddStaticText("Weather event", headingFont, eventLabel)
                .AddStaticText("Allow event to end automatically", bodyFont, allowStopLabel)
                .AddSwitch(OnAllowStopToggled, allowStopSwitch, AllowStopSwitchKey)
                .AddStaticText(eventCurrentLabelText, bodyFont, eventCurrentLabel, EventCurrentLabelKey);

            if (eventEntries.Length > 0)
            {
                double dropWidth = sectionWidth - 2 * sectionPadding - lockColumnWidth;
                ElementBounds regionBounds = ElementBounds.Fixed(sectionPadding, eventOptionsStartY, dropWidth, dropDownHeight);
                double regionLockY = regionBounds.fixedY + (dropDownHeight - switchHeight) / 2;
                ElementBounds regionLockSwitch = ElementBounds.Fixed(regionBounds.fixedX + regionBounds.fixedWidth + rowSpacing, regionLockY, switchColumnWidth, switchHeight);
                ElementBounds regionLockLabel = ElementBounds.Fixed(regionLockSwitch.fixedX + regionLockSwitch.fixedWidth + rowSpacing, regionBounds.fixedY + 4, lockLabelWidth, dropDownHeight - 8);

                double globalY = regionBounds.fixedY + dropDownHeight + rowSpacing;
                ElementBounds globalBounds = ElementBounds.Fixed(sectionPadding, globalY, dropWidth, dropDownHeight);
                double globalLockY = globalBounds.fixedY + (dropDownHeight - switchHeight) / 2;
                ElementBounds globalLockSwitch = ElementBounds.Fixed(globalBounds.fixedX + globalBounds.fixedWidth + rowSpacing, globalLockY, switchColumnWidth, switchHeight);
                ElementBounds globalLockLabel = ElementBounds.Fixed(globalLockSwitch.fixedX + globalLockSwitch.fixedWidth + rowSpacing, globalBounds.fixedY + 4, lockLabelWidth, dropDownHeight - 8);

                eventSection.WithChildren(regionBounds, regionLockSwitch, regionLockLabel, globalBounds, globalLockSwitch, globalLockLabel);

                composer.AddDropDown(eventCodes, eventNames, GetSelectedIndex(eventCodes, currentOptions.CurrentEventCode), OnEventRegionSelectionChanged, regionBounds, EventRegionDropdownKey);
                composer.AddSwitch(OnEventRegionLockToggled, regionLockSwitch, EventRegionLockSwitchKey);
                composer.AddStaticText("Lock region event", bodyFont, regionLockLabel);

                composer.AddDropDown(eventCodes, eventNames, GetSelectedIndex(eventCodes, currentOptions.CurrentEventCode), OnEventGlobalSelectionChanged, globalBounds, EventGlobalDropdownKey);
                composer.AddSwitch(OnEventGlobalLockToggled, globalLockSwitch, EventGlobalLockSwitchKey);
                composer.AddStaticText("Lock global event", bodyFont, globalLockLabel);
            }
            else
            {
                ElementBounds messageBounds = ElementBounds.Fixed(sectionPadding, eventOptionsStartY, sectionWidth - 2 * sectionPadding, infoTextHeight);
                eventSection.WithChild(messageBounds);
                composer.AddStaticText("No weather events available.", bodyFont, messageBounds);
            }

            composer
                .AddInset(windSection, 6, 0.8f)
                .AddStaticText("Wind pattern", headingFont, windLabel)
                .AddStaticText(windCurrentLabelText, bodyFont, windCurrentLabel, WindCurrentLabelKey);

            if (windEntries.Length > 0)
            {
                double dropWidth = sectionWidth - 2 * sectionPadding - lockColumnWidth;
                ElementBounds windDropBounds = ElementBounds.Fixed(sectionPadding, windOptionsStartY, dropWidth, dropDownHeight);
                double windLockY = windDropBounds.fixedY + (dropDownHeight - switchHeight) / 2;
                ElementBounds windLockSwitch = ElementBounds.Fixed(windDropBounds.fixedX + windDropBounds.fixedWidth + rowSpacing, windLockY, switchColumnWidth, switchHeight);
                ElementBounds windLockLabel = ElementBounds.Fixed(windLockSwitch.fixedX + windLockSwitch.fixedWidth + rowSpacing, windDropBounds.fixedY + 4, lockLabelWidth, dropDownHeight - 8);
                windSection.WithChildren(windDropBounds, windLockSwitch, windLockLabel);
                composer.AddDropDown(windCodes, windNames, GetSelectedIndex(windCodes, currentOptions.CurrentWindCode), OnWindSelectionChanged, windDropBounds, WindDropdownKey);
                composer.AddSwitch(OnWindLockToggled, windLockSwitch, WindLockSwitchKey);
                composer.AddStaticText("Lock wind pattern", bodyFont, windLockLabel);
            }
            else
            {
                ElementBounds messageBounds = ElementBounds.Fixed(sectionPadding, windOptionsStartY, sectionWidth - 2 * sectionPadding, infoTextHeight);
                windSection.WithChild(messageBounds);
                composer.AddStaticText("No wind patterns available.", bodyFont, messageBounds);
            }

            composer
                .AddInset(stormSection, 6, 0.8f)
                .AddStaticText("Temporal storms", headingFont, stormLabel)
                .AddStaticText(stormCurrentLabelText, bodyFont, stormCurrentLabel, StormCurrentLabelKey);

            if (stormEntries.Length > 0)
            {
                double dropWidth = sectionWidth - 2 * sectionPadding - lockColumnWidth;
                ElementBounds stormDropBounds = ElementBounds.Fixed(sectionPadding, stormOptionsStartY, dropWidth, dropDownHeight);
                double stormLockY = stormDropBounds.fixedY + (dropDownHeight - switchHeight) / 2;
                ElementBounds stormLockSwitch = ElementBounds.Fixed(stormDropBounds.fixedX + stormDropBounds.fixedWidth + rowSpacing, stormLockY, switchColumnWidth, switchHeight);
                ElementBounds stormLockLabel = ElementBounds.Fixed(stormLockSwitch.fixedX + stormLockSwitch.fixedWidth + rowSpacing, stormDropBounds.fixedY + 4, lockLabelWidth, dropDownHeight - 8);
                stormSection.WithChildren(stormDropBounds, stormLockSwitch, stormLockLabel);
                composer.AddDropDown(stormCodes, stormNames, GetSelectedIndex(stormCodes, currentOptions.CurrentTemporalStormMode), OnStormModeSelectionChanged, stormDropBounds, StormDropdownKey);
                composer.AddSwitch(OnStormLockToggled, stormLockSwitch, StormLockSwitchKey);
                composer.AddStaticText("Lock storm mode", bodyFont, stormLockLabel);
            }
            else
            {
                ElementBounds messageBounds = ElementBounds.Fixed(sectionPadding, stormOptionsStartY, sectionWidth - 2 * sectionPadding, infoTextHeight);
                stormSection.WithChild(messageBounds);
                composer.AddStaticText("No temporal storm modes available.", bodyFont, messageBounds);
            }

            composer.AddSmallButton("Summon storm", OnStormSummonClicked, stormSummonButton, EnumButtonStyle.Normal, StormSummonButtonKey);
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

            lockRegionPattern = packet.RegionPatternLocked;
            SingleComposer.GetSwitch(PatternRegionLockSwitchKey)?.SetValue(lockRegionPattern);

            lockGlobalPattern = packet.GlobalPatternLocked;
            SingleComposer.GetSwitch(PatternGlobalLockSwitchKey)?.SetValue(lockGlobalPattern);

            lockRegionEvent = packet.RegionEventLocked;
            SingleComposer.GetSwitch(EventRegionLockSwitchKey)?.SetValue(lockRegionEvent);

            lockGlobalEvent = packet.GlobalEventLocked;
            SingleComposer.GetSwitch(EventGlobalLockSwitchKey)?.SetValue(lockGlobalEvent);

            lockWind = packet.WindLocked;
            SingleComposer.GetSwitch(WindLockSwitchKey)?.SetValue(lockWind);

            lockStorm = packet.TemporalStormLocked;
            SingleComposer.GetSwitch(StormLockSwitchKey)?.SetValue(lockStorm);

            UpdateCurrentLabels(packet);
            UpdateDropdownSelections(packet);
            UpdateControlStates();
        }

        private void UpdateControlStates()
        {
            if (SingleComposer == null)
            {
                return;
            }

            bool canControl = currentOptions.HasControlPrivilege;
            bool regionAvailable = currentOptions.RegionAvailable && canControl;

            var patternRegionDropdown = SingleComposer.GetDropDown(PatternRegionDropdownKey);
            if (patternRegionDropdown != null)
            {
                patternRegionDropdown.Enabled = regionAvailable && patternCodes.Length > 0;
            }

            var patternRegionLockSwitch = SingleComposer.GetSwitch(PatternRegionLockSwitchKey);
            if (patternRegionLockSwitch != null)
            {
                patternRegionLockSwitch.Enabled = regionAvailable && patternCodes.Length > 0;
            }

            var patternGlobalDropdown = SingleComposer.GetDropDown(PatternGlobalDropdownKey);
            if (patternGlobalDropdown != null)
            {
                patternGlobalDropdown.Enabled = canControl && patternCodes.Length > 0;
            }

            var patternGlobalLockSwitch = SingleComposer.GetSwitch(PatternGlobalLockSwitchKey);
            if (patternGlobalLockSwitch != null)
            {
                patternGlobalLockSwitch.Enabled = canControl && patternCodes.Length > 0;
            }

            var eventRegionDropdown = SingleComposer.GetDropDown(EventRegionDropdownKey);
            if (eventRegionDropdown != null)
            {
                eventRegionDropdown.Enabled = regionAvailable && eventCodes.Length > 0;
            }

            var eventRegionLockSwitch = SingleComposer.GetSwitch(EventRegionLockSwitchKey);
            if (eventRegionLockSwitch != null)
            {
                eventRegionLockSwitch.Enabled = regionAvailable && eventCodes.Length > 0;
            }

            var eventGlobalDropdown = SingleComposer.GetDropDown(EventGlobalDropdownKey);
            if (eventGlobalDropdown != null)
            {
                eventGlobalDropdown.Enabled = canControl && eventCodes.Length > 0;
            }

            var eventGlobalLockSwitch = SingleComposer.GetSwitch(EventGlobalLockSwitchKey);
            if (eventGlobalLockSwitch != null)
            {
                eventGlobalLockSwitch.Enabled = canControl && eventCodes.Length > 0;
            }

            var windDropdown = SingleComposer.GetDropDown(WindDropdownKey);
            if (windDropdown != null)
            {
                windDropdown.Enabled = canControl && windCodes.Length > 0;
            }

            var windLockSwitch = SingleComposer.GetSwitch(WindLockSwitchKey);
            if (windLockSwitch != null)
            {
                windLockSwitch.Enabled = canControl && windCodes.Length > 0;
            }

            var stormDropdown = SingleComposer.GetDropDown(StormDropdownKey);
            if (stormDropdown != null)
            {
                stormDropdown.Enabled = canControl && stormCodes.Length > 0;
            }

            var stormLockSwitch = SingleComposer.GetSwitch(StormLockSwitchKey);
            if (stormLockSwitch != null)
            {
                stormLockSwitch.Enabled = canControl && stormCodes.Length > 0;
            }

            var summonButton = SingleComposer.GetButton(StormSummonButtonKey);
            if (summonButton != null)
            {
                bool stormsEnabled = !string.Equals(currentOptions.CurrentTemporalStormMode, "off", StringComparison.OrdinalIgnoreCase);
                summonButton.Enabled = !currentOptions.TemporalStormActive && canControl && stormsEnabled && stormCodes.Length > 0;
            }

            var stopButton = SingleComposer.GetButton(StormStopButtonKey);
            if (stopButton != null)
            {
                bool canStop = currentOptions.TemporalStormActive && (canControl || !currentOptions.HasControlPrivilege);
                stopButton.Enabled = canStop;
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

        private void UpdateDropdownSelections(WeatherOptionsPacket packet)
        {
            if (SingleComposer == null)
            {
                return;
            }

            UpdateDropdown(PatternRegionDropdownKey, patternCodes, patternNames, packet.CurrentPatternCode);
            UpdateDropdown(PatternGlobalDropdownKey, patternCodes, patternNames, packet.CurrentPatternCode);
            UpdateDropdown(EventRegionDropdownKey, eventCodes, eventNames, packet.CurrentEventCode);
            UpdateDropdown(EventGlobalDropdownKey, eventCodes, eventNames, packet.CurrentEventCode);
            UpdateDropdown(WindDropdownKey, windCodes, windNames, packet.CurrentWindCode);
            UpdateDropdown(StormDropdownKey, stormCodes, stormNames, packet.CurrentTemporalStormMode);
        }

        private void UpdateDropdown(string key, string[] codes, string[] names, string selectedCode)
        {
            var dropdown = SingleComposer.GetDropDown(key);
            if (dropdown == null)
            {
                return;
            }

            string[] safeCodes = codes ?? Array.Empty<string>();
            string[] safeNames = names ?? safeCodes;
            if (safeNames.Length != safeCodes.Length)
            {
                safeNames = safeCodes.ToArray();
            }
            dropdown.SetList(safeCodes, safeNames);
            dropdown.SetSelectedIndex(GetSelectedIndex(safeCodes, selectedCode));
        }

        private void OnAllowStopToggled(bool on)
        {
            selectedAllowStop = on;
        }

        private void OnAutoSwitchToggled(bool on)
        {
            selectedAutoChange = on;
        }

        private void OnPatternRegionLockToggled(bool on)
        {
            lockRegionPattern = on;
            string code = currentOptions.CurrentPatternCode;
            if (!string.IsNullOrEmpty(code))
            {
                SendPatternRegionCommand(code);
            }
        }

        private void OnPatternGlobalLockToggled(bool on)
        {
            lockGlobalPattern = on;
            string code = currentOptions.CurrentPatternCode;
            if (!string.IsNullOrEmpty(code))
            {
                SendPatternGlobalCommand(code);
            }
        }

        private void OnEventRegionLockToggled(bool on)
        {
            lockRegionEvent = on;
            string code = currentOptions.CurrentEventCode;
            if (!string.IsNullOrEmpty(code))
            {
                SendEventRegionCommand(code);
            }
        }

        private void OnEventGlobalLockToggled(bool on)
        {
            lockGlobalEvent = on;
            string code = currentOptions.CurrentEventCode;
            if (!string.IsNullOrEmpty(code))
            {
                SendEventGlobalCommand(code);
            }
        }

        private void OnWindLockToggled(bool on)
        {
            lockWind = on;
            string code = currentOptions.CurrentWindCode;
            if (!string.IsNullOrEmpty(code))
            {
                SendWindCommand(code);
            }
        }

        private void OnStormLockToggled(bool on)
        {
            lockStorm = on;
            string code = currentOptions.CurrentTemporalStormMode;
            if (!string.IsNullOrEmpty(code))
            {
                SendStormCommand(code);
            }
        }

        private void OnPatternRegionSelectionChanged(string code, bool selected)
        {
            if (!selected || string.IsNullOrEmpty(code))
            {
                return;
            }

            SendPatternRegionCommand(code);
        }

        private void OnPatternGlobalSelectionChanged(string code, bool selected)
        {
            if (!selected || string.IsNullOrEmpty(code))
            {
                return;
            }

            SendPatternGlobalCommand(code);
        }

        private void OnEventRegionSelectionChanged(string code, bool selected)
        {
            if (!selected || string.IsNullOrEmpty(code))
            {
                return;
            }

            SendEventRegionCommand(code);
        }

        private void OnEventGlobalSelectionChanged(string code, bool selected)
        {
            if (!selected || string.IsNullOrEmpty(code))
            {
                return;
            }

            SendEventGlobalCommand(code);
        }

        private void OnWindSelectionChanged(string code, bool selected)
        {
            if (!selected || string.IsNullOrEmpty(code))
            {
                return;
            }

            SendWindCommand(code);
        }

        private void OnStormModeSelectionChanged(string code, bool selected)
        {
            if (!selected || string.IsNullOrEmpty(code))
            {
                return;
            }

            SendStormCommand(code);
        }

        private bool OnStormSummonClicked()
        {
            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.StartTemporalStorm,
                UpdateInstantly = true
            });

            return true;
        }

        private void SendPatternRegionCommand(string code)
        {
            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetRegionPattern,
                Code = code,
                UpdateInstantly = true,
                UseSelectionLock = true,
                SelectionLocked = lockRegionPattern
            });
        }

        private void SendPatternGlobalCommand(string code)
        {
            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetGlobalPattern,
                Code = code,
                UpdateInstantly = true,
                UseSelectionLock = true,
                SelectionLocked = lockGlobalPattern
            });
        }

        private void SendEventRegionCommand(string code)
        {
            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetRegionEvent,
                Code = code,
                UpdateInstantly = true,
                UseAllowStop = true,
                AllowStop = selectedAllowStop,
                UseSelectionLock = true,
                SelectionLocked = lockRegionEvent
            });
        }

        private void SendEventGlobalCommand(string code)
        {
            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetGlobalEvent,
                Code = code,
                UpdateInstantly = true,
                UseAllowStop = true,
                AllowStop = selectedAllowStop,
                UseSelectionLock = true,
                SelectionLocked = lockGlobalEvent
            });
        }

        private void SendWindCommand(string code)
        {
            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetGlobalWind,
                Code = code,
                UpdateInstantly = true,
                UseSelectionLock = true,
                SelectionLocked = lockWind
            });
        }

        private void SendStormCommand(string code)
        {
            system.SendCommand(new WeatherControlCommand
            {
                Action = WeatherControlAction.SetTemporalStormMode,
                Code = code,
                UseSelectionLock = true,
                SelectionLocked = lockStorm
            });
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
            WeatherOptionsPacket source = packet ?? currentOptions ?? new WeatherOptionsPacket();

            var patternLabel = SingleComposer.GetStaticText(PatternCurrentLabelKey);
            if (patternLabel != null)
            {
                patternLabel.SetValue(BuildPatternCurrentLabelText(source));
            }

            var eventLabel = SingleComposer.GetStaticText(EventCurrentLabelKey);
            if (eventLabel != null)
            {
                eventLabel.SetValue(BuildEventCurrentLabelText(source));
            }

            var windLabel = SingleComposer.GetStaticText(WindCurrentLabelKey);
            if (windLabel != null)
            {
                windLabel.SetValue(BuildWindCurrentLabelText(source));
            }

            var stormLabel = SingleComposer.GetStaticText(StormCurrentLabelKey);
            if (stormLabel != null)
            {
                stormLabel.SetValue(BuildStormCurrentLabelText(source));
            }
        }

        private string BuildPatternCurrentLabelText(WeatherOptionsPacket packet)
        {
            string code = packet?.CurrentPatternCode;
            return $"Current: {FormatCurrentValue(patternEntries, code, "None")}";
        }

        private string BuildEventCurrentLabelText(WeatherOptionsPacket packet)
        {
            string code = packet?.CurrentEventCode;
            string eventText = FormatCurrentValue(eventEntries, code, "None");
            if (!string.IsNullOrEmpty(code))
            {
                bool allowStop = packet?.CurrentEventAllowStop ?? true;
                eventText += $" (Allow stop: {(allowStop ? "Yes" : "No")})";
            }

            return $"Current: {eventText}";
        }

        private string BuildWindCurrentLabelText(WeatherOptionsPacket packet)
        {
            string code = packet?.CurrentWindCode;
            return $"Current: {FormatCurrentValue(windEntries, code, "None")}";
        }

        private string BuildStormCurrentLabelText(WeatherOptionsPacket packet)
        {
            string code = packet?.CurrentTemporalStormMode;
            string mode = FormatCurrentValue(stormEntries, code, "Off");
            string status = packet?.TemporalStormActive == true ? "Active" : "Inactive";
            return $"Mode: {mode} (Status: {status})";
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

        private static int GetSelectedIndex(string[] codes, string selectedCode)
        {
            if (codes == null || string.IsNullOrEmpty(selectedCode))
            {
                return -1;
            }

            int index = Array.IndexOf(codes, selectedCode);
            return index >= 0 ? index : -1;
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

    }
}
