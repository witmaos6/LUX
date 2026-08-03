using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameBinding
{
    MoveLeft,
    MoveRight,
    Sprint,
    Interaction,
    ArrowInteraction,
    Flashlight,
    Inventory,
    EscapeMenu
}

public static class KeyBindingSettings
{
    private const string Prefix = "KeyBinding.";

    private static readonly Dictionary<GameBinding, string> Defaults = new()
    {
        { GameBinding.MoveLeft, "<Keyboard>/leftArrow" },
        { GameBinding.MoveRight, "<Keyboard>/rightArrow" },
        { GameBinding.Sprint, "<Keyboard>/leftShift" },
        { GameBinding.Interaction, "<Keyboard>/space" },
        { GameBinding.ArrowInteraction, "<Keyboard>/downArrow" },
        { GameBinding.Flashlight, "<Keyboard>/v" },
        { GameBinding.Inventory, "<Keyboard>/c" },
        { GameBinding.EscapeMenu, "<Keyboard>/f" }
    };

    public static event Action Changed;

    public static string GetPath(GameBinding binding)
    {
        string value = PlayerPrefs.GetString(Prefix + binding, Defaults[binding]);
        return NormalizePath(string.IsNullOrWhiteSpace(value) ? Defaults[binding] : value);
    }

    public static string GetDisplayName(GameBinding binding)
    {
        string path = GetPath(binding);
        return InputControlPath.ToHumanReadableString(
            path,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

    public static void Set(GameBinding binding, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        path = NormalizePath(path);

        // Keyboard assignments are kept unique. If necessary, exchange the two keys.
        foreach (GameBinding other in Enum.GetValues(typeof(GameBinding)))
        {
            if (other == binding || !string.Equals(GetPath(other), path, StringComparison.OrdinalIgnoreCase))
                continue;

            PlayerPrefs.SetString(Prefix + other, GetPath(binding));
            break;
        }

        PlayerPrefs.SetString(Prefix + binding, path);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    private static string NormalizePath(string path)
    {
        const string runtimeKeyboardPrefix = "/Keyboard/";
        if (path.StartsWith(runtimeKeyboardPrefix, StringComparison.OrdinalIgnoreCase))
            return "<Keyboard>/" + path.Substring(runtimeKeyboardPrefix.Length);

        return path;
    }

    public static void ResetAll()
    {
        foreach (GameBinding binding in Enum.GetValues(typeof(GameBinding)))
            PlayerPrefs.DeleteKey(Prefix + binding);

        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    public static void Apply(InputSystem_Actions controls)
    {
        controls.Player.Move.ApplyBindingOverride(1, GetPath(GameBinding.MoveLeft));
        controls.Player.Move.ApplyBindingOverride(2, GetPath(GameBinding.MoveRight));
        controls.Player.Sprint.ApplyBindingOverride(0, GetPath(GameBinding.Sprint));
        controls.Player.Interaction.ApplyBindingOverride(0, GetPath(GameBinding.Interaction));
        controls.Player.ArrowInteraction.ApplyBindingOverride(0, GetPath(GameBinding.ArrowInteraction));
        controls.Player.Flashlight.ApplyBindingOverride(0, GetPath(GameBinding.Flashlight));
    }
}
