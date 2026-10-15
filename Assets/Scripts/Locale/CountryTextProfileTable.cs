
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CountryTextProfileTable",
    menuName = "LUX/Localization/Country Text Profile Table"
)]
public class CountryTextProfileTable : ScriptableObject
{
    [Serializable]
    public class LocaleData
    {
        public string localeCode;
        public List<Profile> profiles = new();
    }

    [Serializable]
    public class Profile
    {
        public string profileName;
        public Property property = new();
    }

    [Serializable]
    public class Property
    {
        public float fontSize;
    }

    public List<LocaleData> locales = new();

    public Property GetProperty(string localeCode, string profileName)
    {
        if (string.IsNullOrWhiteSpace(localeCode) ||
                    string.IsNullOrWhiteSpace(profileName))
        {
            return null;
        }

        LocaleData locale = locales.Find(
            x => string.Equals(
                x.localeCode,
                localeCode,
                StringComparison.OrdinalIgnoreCase));

        if (locale == null)
        {
            Debug.LogWarning("locale is null");
            return null;
        }
            

        Profile profile = locale.profiles.Find(
            x => string.Equals(
                x.profileName,
                profileName,
                StringComparison.Ordinal));

        return profile.property;
    }


    public Profile GetProfile(string localeCode, string profileName)
    {
        if (string.IsNullOrWhiteSpace(localeCode) ||
            string.IsNullOrWhiteSpace(profileName))
        {
            return null;
        }

        LocaleData locale = locales.Find(
            x => string.Equals(
                x.localeCode,
                localeCode,
                StringComparison.OrdinalIgnoreCase));

        if (locale == null)
            return null;

        return locale.profiles.Find(
            x => string.Equals(
                x.profileName,
                profileName,
                StringComparison.Ordinal));
    }

    public bool TryGetFontSize(string localeCode, string profileName, out float fontSize)
    {
        Profile profile = GetProfile(localeCode, profileName);

        if (profile == null)
        {
            fontSize = default;
            return false;
        }

        fontSize = profile.property.fontSize;
        return true;
    }
}
