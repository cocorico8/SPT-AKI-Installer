﻿using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using Serilog;
using SPTInstaller.Controllers;
using SPTInstaller.Helpers;
using SPTInstaller.Models;

namespace SPTInstaller.ViewModels;

public class PreChecksViewModel : ViewModelBase
{
    private string _installPath;
    private bool _allowInstall;

    public ObservableCollection<PreCheckBase> PreChecks { get; set; } = new(ServiceHelper.GetAll<PreCheckBase>());
    public ICommand StartInstallCommand { get; set; }
    public ICommand ShowDetailedViewCommand { get; set; }

    public string InstallPath
    {
        get => _installPath;
        set => this.RaiseAndSetIfChanged(ref _installPath, value);
    }
    
    public bool AllowInstall
    {
        get => _allowInstall;
        set => this.RaiseAndSetIfChanged(ref _allowInstall, value);
    }

    public PreChecksViewModel(IScreen host, Action? dismissUpdateCard) : base(host)
    {
        var data = ServiceHelper.Get<InternalData?>();
        var installer = ServiceHelper.Get<InstallController?>();

        if (data == null || installer == null)
        {
            NavigateTo(new MessageViewModel(HostScreen, Result.FromError("Failed to get required service for prechecks")));
            return;
        }

        data.OriginalGamePath = PreCheckHelper.DetectOriginalGamePath();

        if (data.OriginalGamePath == null)
        {
            NavigateTo(new MessageViewModel(HostScreen, Result.FromError("Could not find EFT install.\n\nDo you own and have the game installed?")));
        }

        data.TargetInstallPath = Environment.CurrentDirectory;
        InstallPath = data.TargetInstallPath;

        Log.Information($"Install Path: {FileHelper.GetRedactedPath(InstallPath)}");

        StartInstallCommand = ReactiveCommand.Create(() =>
        {
            dismissUpdateCard?.Invoke();
            NavigateTo(new InstallViewModel(HostScreen));
        });

        ShowDetailedViewCommand = ReactiveCommand.Create(() => 
        {
            dismissUpdateCard?.Invoke();
            Log.Logger.Information("Opening Detailed PreCheck View");
            NavigateTo(new DetailedPreChecksViewModel(HostScreen));
        });

        Task.Run(async () =>
        {
            var result = await installer.RunPreChecks();
            AllowInstall = result.Succeeded;
        });
    }
}