using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Win32;

namespace PlatinumToolkit;

// One registry value a tweak writes when turned on, and restores when turned off.
public record RegEntry(
    RegistryHive Hive,
    string SubKey,
    string? ValueName,   // null = the key's default (unnamed) value
    RegistryValueKind Kind,
    int EnabledValue,
    int DisabledValue);

// A single row in the UI: name/description plus the registry entries it controls.
// Implements INotifyPropertyChanged so the ToggleSwitch in the UI stays in sync
// when IsEnabled is set from code (e.g. after reading current state on load).
public class TweakItem : INotifyPropertyChanged
{
    public string Id { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string Description { get; init; } = "";
    public string Category { get; init; } = "";
    public List<RegEntry> Entries { get; init; } = new();

    private bool _isEnabled;
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value) return;
            _isEnabled = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
