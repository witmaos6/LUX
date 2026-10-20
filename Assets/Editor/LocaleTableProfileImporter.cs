using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class LocaleTableProfileImporter
{
    private const string AssetPath =
        "Assets/Localization/TextProfileDatabase.asset";

    [MenuItem("LUX/Localization/Import Text Profiles")]
    public static void Import()
    {
        string csvPath = EditorUtility.OpenFilePanel(
            "Select LocaleTable CSV",
            Application.dataPath,
            "csv"
        );

        if (string.IsNullOrEmpty(csvPath))
            return;

        string csv = File.ReadAllText(
            csvPath,
            Encoding.UTF8
        );

        // UTF-8 BOM 제거
        csv = csv.TrimStart('\uFEFF');

        List<List<string>> rows = ParseCsv(csv);

        if (rows.Count < 2)
        {
            Debug.LogError("CSV에 가져올 데이터가 없습니다.");
            return;
        }

        int keyColumn = rows[0].FindIndex(
            value => value.Trim() == "Key"
        );

        int profileColumn = rows[0].FindIndex(
            value => value.Trim() == "TextProfile"
        );

        if (keyColumn < 0 || profileColumn < 0)
        {
            Debug.LogError(
                "CSV에서 Key 또는 TextProfile 열을 찾지 못했습니다."
            );
            return;
        }

        var entries = new List<TextProfileDatabase.Entry>();
        var seenKeys = new HashSet<string>();

        for (int i = 1; i < rows.Count; i++)
        {
            var row = rows[i];

            int requiredColumn = Math.Max(keyColumn, profileColumn);

            if (row.Count <= requiredColumn)
                continue;

            string key = row[keyColumn].Trim();
            string profile = row[profileColumn].Trim();

            if (string.IsNullOrEmpty(key))
                continue;

            if (!seenKeys.Add(key))
            {
                Debug.LogWarning(
                    $"CSV {i + 1}행: 중복 Key '{key}'를 확인하세요."
                );
                continue;
            }

            entries.Add(new TextProfileDatabase.Entry
            {
                key = key,
                profile = profile
            });
        }

        string folder = Path.GetDirectoryName(AssetPath);

        if (!AssetDatabase.IsValidFolder(folder))
        {
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        var database =
            AssetDatabase.LoadAssetAtPath<TextProfileDatabase>(
                AssetPath
            );

        if (database == null)
        {
            database = ScriptableObject.CreateInstance<TextProfileDatabase>();

            AssetDatabase.CreateAsset(database, AssetPath);
        }

        Undo.RecordObject(database, "Import Text Profiles");
        database.ReplaceEntries(entries);

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"TextProfile 임포트 완료: {entries.Count}개 항목\n" +
            $"에셋 경로: {AssetPath}"
        );
    }

    private static List<List<string>> ParseCsv(string text)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var cell = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (c == '"')
            {
                if (inQuotes &&
                    i + 1 < text.Length &&
                    text[i + 1] == '"')
                {
                    cell.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                row.Add(cell.ToString());
                cell.Clear();
            }
            else if ((c == '\n' || c == '\r') && !inQuotes)
            {
                if (c == '\r' &&
                    i + 1 < text.Length &&
                    text[i + 1] == '\n')
                {
                    i++;
                }

                row.Add(cell.ToString());
                cell.Clear();

                if (row.Exists(value => !string.IsNullOrWhiteSpace(value)))
                    rows.Add(row);

                row = new List<string>();
            }
            else
            {
                cell.Append(c);
            }
        }

        if (cell.Length > 0 || row.Count > 0)
        {
            row.Add(cell.ToString());
            rows.Add(row);
        }

        return rows;
    }
}
