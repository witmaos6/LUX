
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class CountryTextProfileImporter
{
    private const string FilePrefix =
        "CountryTextProfileTable_";

    private const string RequiredProfileHeader =
        "TextProfile";

    private const string RequiredFontSizeHeader =
        "FontSize";

    private const string AssetFolder =
        "Assets/Localization";

    private const string AssetPath =
        "Assets/Localization/CountryTextProfileTable.asset";

    [MenuItem("LUX/Localization/Import Country Text Profiles")]
    public static void Import()
    {
        string folder = EditorUtility.OpenFolderPanel(
            "Select Country Text Profile CSV Folder",
            Application.dataPath,
            "");

        if (string.IsNullOrEmpty(folder))
            return;

        string[] files = Directory.GetFiles(
            folder, FilePrefix + "*.csv");

        if (files.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "Import Failed",
                $"'{FilePrefix}*.csv' 파일을 찾을 수 없습니다.",
                "OK");
            return;
        }

        var importedLocales =
            new List<CountryTextProfileTable.LocaleData>();

        var usedLocaleCodes =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        try
        {
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            foreach (string file in files)
            {
                CountryTextProfileTable.LocaleData locale =
                    ParseFile(file);

                if (!usedLocaleCodes.Add(locale.localeCode))
                {
                    throw new Exception(
                        $"중복된 언어 코드입니다: {locale.localeCode}");
                }

                importedLocales.Add(locale);
            }

            // 모든 CSV 검증이 성공한 이후에만 에셋을 수정합니다.
            EnsureAssetFolder();

            CountryTextProfileTable table =
                AssetDatabase.LoadAssetAtPath
                    <CountryTextProfileTable>(AssetPath);

            if (table == null)
            {
                table =
                    ScriptableObject.CreateInstance
                        <CountryTextProfileTable>();

                AssetDatabase.CreateAsset(table, AssetPath);
            }

            table.locales = importedLocales;

            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Import Complete",
                $"언어 {importedLocales.Count}개를 가져왔습니다.\n" +
                $"저장 위치: {AssetPath}",
                "OK");
        }
        catch (Exception ex)
        {
            Debug.LogError(
                $"CountryTextProfileTable Import Failed:\n{ex}");

            EditorUtility.DisplayDialog(
                "Import Failed",
                ex.Message +
                "\n\n기존 에셋의 데이터는 변경되지 않았습니다.",
                "OK");
        }
    }

    private static CountryTextProfileTable.LocaleData ParseFile(
        string filePath)
    {
        string fileName = Path.GetFileNameWithoutExtension(
            filePath);

        string localeCode =
            fileName.Substring(FilePrefix.Length).Trim();

        if (string.IsNullOrWhiteSpace(localeCode))
        {
            throw new Exception(
                $"파일명에서 언어 코드를 찾을 수 없습니다: {fileName}");
        }

        string csv = File.ReadAllText(
            filePath, Encoding.UTF8);

        List<List<string>> rows = ParseCsv(csv);

        if (rows.Count < 2)
        {
            throw new Exception(
                $"CSV에 헤더와 데이터가 필요합니다: {fileName}");
        }

        List<string> headers = rows[0];

        for (int i = 0; i < headers.Count; i++)
            headers[i] = headers[i].Trim();

        ValidateHeaders(headers, fileName);

        int profileIndex = headers.IndexOf(
            RequiredProfileHeader);

        int fontSizeIndex = headers.IndexOf(
            RequiredFontSizeHeader);

        var locale =
            new CountryTextProfileTable.LocaleData
            {
                localeCode = localeCode
            };

        var usedProfiles =
            new HashSet<string>(StringComparer.Ordinal);

        for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
        {
            List<string> row = rows[rowIndex];

            // 완전히 빈 행은 무시합니다.
            bool isEmpty = true;

            foreach (string cell in row)
            {
                if (!string.IsNullOrWhiteSpace(cell))
                {
                    isEmpty = false;
                    break;
                }
            }

            if (isEmpty)
                continue;

            int recordNumber = rowIndex + 1;

            if (row.Count != headers.Count)
            {
                throw new Exception(
                    $"{fileName}, CSV 레코드 {recordNumber}: " +
                    $"열 개수가 올바르지 않습니다. " +
                    $"예상 {headers.Count}개, 실제 {row.Count}개");
            }

            string profileName =
                row[profileIndex].Trim();

            if (string.IsNullOrWhiteSpace(profileName))
            {
                throw new Exception(
                    $"{fileName}, CSV 레코드 {recordNumber}: " +
                    "TextProfile이 비어 있습니다.");
            }

            if (!usedProfiles.Add(profileName))
            {
                throw new Exception(
                    $"{fileName}, CSV 레코드 {recordNumber}: " +
                    $"중복된 프로필입니다: {profileName}");
            }

            string rawFontSize =
                row[fontSizeIndex].Trim();

            if (string.IsNullOrWhiteSpace(rawFontSize))
            {
                throw new Exception(
                    $"{fileName}, CSV 레코드 {recordNumber}: " +
                    $"FontSize가 비어 있습니다. " +
                    $"프로필: {profileName}");
            }

            if (!float.TryParse(
                rawFontSize,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float fontSize) ||
                float.IsNaN(fontSize) ||
                float.IsInfinity(fontSize) ||
                fontSize <= 0f)
            {
                throw new Exception(
                    $"{fileName}, CSV 레코드 {recordNumber}: " +
                    $"잘못된 FontSize 값 '{rawFontSize}'. " +
                    "0보다 큰 유한한 숫자를 입력해야 합니다.");
            }

            locale.profiles.Add(
                new CountryTextProfileTable.Profile
                {
                    profileName = profileName,
                    property =
                        new CountryTextProfileTable.Property
                        {
                            fontSize = fontSize
                        }
                });
        }

        if (locale.profiles.Count == 0)
        {
            throw new Exception(
                $"{fileName}: 유효한 프로필 데이터가 없습니다.");
        }

        return locale;
    }

    private static void ValidateHeaders(
        List<string> headers,
        string fileName)
    {
        var seen = new HashSet<string>(
            StringComparer.Ordinal);

        foreach (string header in headers)
        {
            if (string.IsNullOrWhiteSpace(header))
            {
                throw new Exception(
                    $"{fileName}: 빈 헤더가 있습니다.");
            }

            if (!seen.Add(header))
            {
                throw new Exception(
                    $"{fileName}: 중복된 헤더입니다: {header}");
            }

            if (header != RequiredProfileHeader &&
                header != RequiredFontSizeHeader)
            {
                throw new Exception(
                    $"{fileName}: 지원하지 않는 열입니다: {header}");
            }
        }

        if (!seen.Contains(RequiredProfileHeader))
        {
            throw new Exception(
                $"{fileName}: 필수 열 '{RequiredProfileHeader}'이 없습니다.");
        }

        if (!seen.Contains(RequiredFontSizeHeader))
        {
            throw new Exception(
                $"{fileName}: 필수 열 '{RequiredFontSizeHeader}'이 없습니다.");
        }

        if (headers.Count != 2)
        {
            throw new Exception(
                $"{fileName}: 현재는 TextProfile과 FontSize, " +
                "두 열만 지원합니다.");
        }
    }

    private static List<List<string>> ParseCsv(string csv)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();

        bool inQuotes = false;

        // UTF-8 BOM 제거
        if (!string.IsNullOrEmpty(csv) &&
            csv[0] == '\uFEFF')
        {
            csv = csv.Substring(1);
        }

        for (int i = 0; i < csv.Length; i++)
        {
            char c = csv[i];

            if (c == '"')
            {
                if (inQuotes &&
                    i + 1 < csv.Length &&
                    csv[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (c == ',' && !inQuotes)
            {
                row.Add(field.ToString());
                field.Clear();
                continue;
            }

            if ((c == '\r' || c == '\n') && !inQuotes)
            {
                row.Add(field.ToString());
                field.Clear();

                rows.Add(row);
                row = new List<string>();

                if (c == '\r' &&
                    i + 1 < csv.Length &&
                    csv[i + 1] == '\n')
                {
                    i++;
                }

                continue;
            }

            if (c == '\r' && inQuotes)
            {
                if (i + 1 < csv.Length &&
                    csv[i + 1] == '\n')
                {
                    i++;
                }

                field.Append('\n');
                continue;
            }

            field.Append(c);
        }

        if (inQuotes)
        {
            throw new Exception(
                "CSV에 닫히지 않은 큰따옴표가 있습니다.");
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }

        return rows;
    }

    private static void EnsureAssetFolder()
    {
        if (AssetDatabase.IsValidFolder(AssetFolder))
            return;

        if (!AssetDatabase.IsValidFolder("Assets"))
        {
            throw new Exception(
                "Unity 프로젝트의 Assets 폴더를 찾을 수 없습니다.");
        }

        string guid = AssetDatabase.CreateFolder(
            "Assets", "Localization");

        if (string.IsNullOrEmpty(guid))
        {
            throw new Exception(
                $"폴더를 생성하지 못했습니다: {AssetFolder}");
        }
    }
}
