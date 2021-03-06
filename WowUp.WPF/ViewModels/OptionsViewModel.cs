﻿using System;
using System.Collections.ObjectModel;
using WowUp.Common.Enums;
using WowUp.WPF.AddonProviders.Contracts;
using WowUp.WPF.Services.Contracts;
using WowUp.WPF.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace WowUp.WPF.ViewModels
{
    public class OptionsViewModel : BaseViewModel
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWarcraftService _warcraftService;
        private readonly IWowUpService _wowUpService;

        private string _wowRetailLocation;
        public string WowRetailLocation
        {
            get => _wowRetailLocation;
            set { SetProperty(ref _wowRetailLocation, value); }
        }

        private string _wowRetailPtrLocation;
        public string WowRetailPtrLocation
        {
            get => _wowRetailPtrLocation;
            set { SetProperty(ref _wowRetailPtrLocation, value); }
        }

        private string _wowClassicLocation;
        public string WowClassicLocation
        {
            get => _wowClassicLocation;
            set { SetProperty(ref _wowClassicLocation, value); }
        }

        private string _wowClassicPtrLocation;
        public string WowClassicPtrLocation
        {
            get => _wowClassicPtrLocation;
            set { SetProperty(ref _wowClassicPtrLocation, value); }
        }

        private string _wowBetaLocation;
        public string WowBetaLocation
        {
            get => _wowBetaLocation;
            set { SetProperty(ref _wowBetaLocation, value); }
        }

        private bool _isTelemetryEnabled;
        public bool IsTelemetryEnabled
        {
            get => _isTelemetryEnabled;
            set { SetProperty(ref _isTelemetryEnabled, value); }
        }

        private bool _collapseToTrayEnabled;
        public bool CollapseToTrayEnabled
        {
            get => _collapseToTrayEnabled;
            set { SetProperty(ref _collapseToTrayEnabled, value); }
        }

        private AddonChannelType _selectedAddonChannelType;
        public AddonChannelType SelectedAddonChannelType
        {
            get => _selectedAddonChannelType;
            set { SetProperty(ref _selectedAddonChannelType, value); }
        }

        private WowUpReleaseChannelType _selectedWowUpReleaseChannelType;
        public WowUpReleaseChannelType SelectedWowUpReleaseChannelType
        {
            get => _selectedWowUpReleaseChannelType;
            set { SetProperty(ref _selectedWowUpReleaseChannelType, value); }
        }

        public Command ShowLogsCommand { get; set; }
        public Command TelemetryCheckCommand { get; set; }
        public Command CollapseToTrayCheckCommand { get; set; }
        public Command SetRetailLocationCommand { get; set; }
        public Command SetRetailPtrLocationCommand { get; set; }
        public Command SetClassicLocationCommand { get; set; }
        public Command SetClassicPtrLocationCommand { get; set; }
        public Command RescanFoldersCommand { get; set; }
        public Command AddonChannelChangeCommand { get; set; }
        public Command WowUpReleaseChannelChangedCommand { get; set; }
        public Command DumpDebugDataCommand { get; set; }

        public ObservableCollection<AddonChannelType> AddonChannelNames { get; set; }
        public ObservableCollection<WowUpReleaseChannelType> WowUpChannelNames { get; set; }

        public OptionsViewModel(
            IAnalyticsService analyticsService,
            IServiceProvider serviceProvider,
            IWarcraftService warcraftService,
            IWowUpService wowUpService)
        {
            _analyticsService = analyticsService;
            _serviceProvider = serviceProvider;
            _warcraftService = warcraftService;
            _wowUpService = wowUpService;

            ShowLogsCommand = new Command(() => ShowLogsFolder());
            TelemetryCheckCommand = new Command(() => OnTelemetryChange());
            CollapseToTrayCheckCommand = new Command(() => OnCollapseToTrayChanged());
            SetRetailLocationCommand = new Command(() => OnSetLocation(WowClientType.Retail));
            SetRetailPtrLocationCommand = new Command(() => OnSetLocation(WowClientType.RetailPtr));
            SetClassicLocationCommand = new Command(() => OnSetLocation(WowClientType.Classic));
            SetClassicPtrLocationCommand = new Command(() => OnSetLocation(WowClientType.ClassicPtr));
            RescanFoldersCommand = new Command(() => OnRescanFolders());
            AddonChannelChangeCommand = new Command(() => OnAddonChannelChange(SelectedAddonChannelType));
            WowUpReleaseChannelChangedCommand = new Command(() => OnWowUpReleaseChannelChange(SelectedWowUpReleaseChannelType));
            DumpDebugDataCommand = new Command(() => DumpDebugData());

            AddonChannelNames = new ObservableCollection<AddonChannelType>
            {
                AddonChannelType.Stable,
                AddonChannelType.Beta,
                AddonChannelType.Alpha
            };

            WowUpChannelNames = new ObservableCollection<WowUpReleaseChannelType>
            {
                WowUpReleaseChannelType.Stable,
                WowUpReleaseChannelType.Beta
            };

            LoadOptions();
        }

        private void LoadOptions()
        {
            IsTelemetryEnabled = _analyticsService.IsTelemetryEnabled();
            CollapseToTrayEnabled = _wowUpService.GetCollapseToTray();
            SelectedAddonChannelType = _wowUpService.GetDefaultAddonChannel();
            SelectedWowUpReleaseChannelType = _wowUpService.GetWowUpReleaseChannel();

            WowRetailLocation = _warcraftService.GetClientLocation(WowClientType.Retail);
            WowRetailPtrLocation = _warcraftService.GetClientLocation(WowClientType.RetailPtr);
            WowClassicLocation = _warcraftService.GetClientLocation(WowClientType.Classic);
            WowClassicPtrLocation = _warcraftService.GetClientLocation(WowClientType.ClassicPtr);
            WowBetaLocation = _warcraftService.GetClientLocation(WowClientType.Beta);
        }

        private void ShowLogsFolder()
        {
            _wowUpService.ShowLogsFolder();
        }

        private void OnTelemetryChange()
        {
            _analyticsService.SetTelemetryEnabled(IsTelemetryEnabled);
        }

        private void OnCollapseToTrayChanged()
        {
            _wowUpService.SetCollapseToTray(CollapseToTrayEnabled);
        }

        private void OnSetLocation(WowClientType clientType)
        {
            var selectedPath = DialogUtilities.SelectFolder();
            if (string.IsNullOrEmpty(selectedPath))
            {
                return;
            }

            if (!_warcraftService.SetWowFolderPath(clientType, selectedPath))
            {
                System.Windows.MessageBox.Show($"Unable to set \"{selectedPath}\" as your {clientType} folder");
                return;
            }

            LoadOptions();
        }

        private void OnRescanFolders()
        {
            _warcraftService.ScanProducts();
            LoadOptions();
        }

        private void OnAddonChannelChange(AddonChannelType addonChannelType)
        {
            _wowUpService.SetDefaultAddonChannel(addonChannelType);
        }

        private void OnWowUpReleaseChannelChange(WowUpReleaseChannelType type)
        {
            _wowUpService.SetWowUpReleaseChannel(type);
        }

        private async void DumpDebugData()
        {
            IsBusy = true;

            var curseAddonProvider = _serviceProvider.GetService<ICurseAddonProvider>();

            var clientTypes = _warcraftService.GetWowClientTypes();
            foreach(var clientType in clientTypes)
            {
                var addonFolders = await _warcraftService.ListAddons(clientType);
                var scanResults = await curseAddonProvider.GetScanResults(addonFolders);

                Log.Debug($"{clientType} ADDON CURSE FINGERPRINTS");
                foreach(var scanResult in scanResults)
                {
                    Log.Debug($"{scanResult.AddonFolder.Name}|{scanResult.FolderScanner.Fingerprint}");
                }
            }

            IsBusy = false;
        }
    }
}
