using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Win32;

namespace PlatinumToolkit;

public static class RegistryHelper
{
    /// <summary>True if the current process already has admin rights.</summary>
    public static bool IsElevated()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    /// <summary>
    /// Relaunches the current executable with a UAC prompt and exits this process.
    /// With app.manifest set to requireAdministrator this normally isn't reachable -
    /// Windows elevates on launch - but it's kept as a defensive fallback.
    /// </summary>
    public static void RelaunchElevated()
    {
        var exePath = Environment.ProcessPath;
        if (exePath is null) return;

        var psi = new ProcessStartInfo(exePath)
        {
            UseShellExecute = true,
            Verb = "runas"
        };

        try
        {
            Process.Start(psi);
            Environment.Exit(0);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // User declined the UAC prompt - fall through and keep running
            // unelevated; HKLM writes will then fail individually below.
        }
    }

    private static RegistryKey OpenBaseKey(RegistryHive hive) =>
        RegistryKey.OpenBaseKey(hive, RegistryView.Default);

    public static int? ReadDword(RegistryHive hive, string subKey, string? valueName)
    {
        using var baseKey = OpenBaseKey(hive);
        using var key = baseKey.OpenSubKey(subKey, writable: false);
        if (key is null) return null;

        var value = key.GetValue(valueName);
        return value is int i ? i : null;
    }

    public static bool WriteDword(RegistryHive hive, string subKey, string? valueName, int data)
    {
        try
        {
            using var baseKey = OpenBaseKey(hive);
            using var key = baseKey.CreateSubKey(subKey, writable: true);
            key.SetValue(valueName, data, RegistryValueKind.DWord);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool ApplyTweak(TweakItem item, bool enable)
    {
        var allOk = true;
        foreach (var entry in item.Entries)
        {
            var data = enable ? entry.EnabledValue : entry.DisabledValue;
            if (!WriteDword(entry.Hive, entry.SubKey, entry.ValueName, data))
            {
                allOk = false;
            }
        }
        return allOk;
    }

    /// <summary>Checks the tweak's first entry to decide the toggle's initial state.</summary>
    public static bool IsTweakCurrentlyEnabled(TweakItem item)
    {
        if (item.Entries.Count == 0) return false;
        var first = item.Entries[0];
        var value = ReadDword(first.Hive, first.SubKey, first.ValueName);
        return value.HasValue && value.Value == first.EnabledValue;
    }

    // -----------------------------------------------------------------------
    // Example tweaks, pulled from the general (non-CPU/GPU-vendor) sections of
    // the merged Platinum+ script. Each is a small, individually documented,
    // reversible Windows setting. Add more by appending to this list - the UI
    // is generated entirely from it, grouped by Category.
    // -----------------------------------------------------------------------
    public static List<TweakItem> BuiltInTweaks() => new()
    {
        new TweakItem
        {
            Id = "telemetry",
            DisplayName = "Disable Telemetry",
            Description = "Sets AllowTelemetry to 0 under the DataCollection policy key.",
            Category = "Security",
            Entries = new()
            {
                new RegEntry(RegistryHive.LocalMachine,
                    @"SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                    "AllowTelemetry", RegistryValueKind.DWord, enabledValue: 0, disabledValue: 3)
            }
        },
        new TweakItem
        {
            Id = "gamedvr",
            DisplayName = "Disable Game DVR",
            Description = "Turns off background game recording (AppCaptureEnabled).",
            Category = "Interface Tweaks",
            Entries = new()
            {
                new RegEntry(RegistryHive.CurrentUser,
                    @"Software\Microsoft\Windows\CurrentVersion\GameDVR",
                    "AppCaptureEnabled", RegistryValueKind.DWord, enabledValue: 0, disabledValue: 1)
            }
        },
        new TweakItem
        {
            Id = "prefetcher",
            DisplayName = "Disable Prefetcher",
            Description = "Turns off Superfetch/Prefetch service-level prefetching.",
            Category = "Advanced Configuration",
            Entries = new()
            {
                new RegEntry(RegistryHive.LocalMachine,
                    @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters",
                    "EnablePrefetcher", RegistryValueKind.DWord, enabledValue: 0, disabledValue: 3)
            }
        },
        new TweakItem
        {
            Id = "ntfs_lastaccess",
            DisplayName = "Disable NTFS Last Access Timestamps",
            Description = "Stops NTFS updating last-access time on every file read.",
            Category = "Advanced Configuration",
            Entries = new()
            {
                new RegEntry(RegistryHive.LocalMachine,
                    @"SYSTEM\CurrentControlSet\Control\FileSystem",
                    "NtfsDisableLastAccessUpdate", RegistryValueKind.DWord, enabledValue: 1, disabledValue: 0)
            }
        },
        new TweakItem
        {
            Id = "hw_gpu_scheduling",
            DisplayName = "Hardware-Accelerated GPU Scheduling",
            Description = "Enables HAGS (HwSchMode) - a standard, Microsoft-documented setting.",
            Category = "GPU",
            Entries = new()
            {
                new RegEntry(RegistryHive.LocalMachine,
                    @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
                    "HwSchMode", RegistryValueKind.DWord, enabledValue: 2, disabledValue: 1)
            }
        },
    };
}
