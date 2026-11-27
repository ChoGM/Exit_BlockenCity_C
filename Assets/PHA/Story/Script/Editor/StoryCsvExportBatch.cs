using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// Build all CSVs first, retaining author notes by row identity. Install them together.
internal sealed class StoryCsvExportBatch
{
    private readonly string folder;
    private readonly Dictionary<string, string> files = new Dictionary<string, string>();
    internal StoryCsvExportBatch(string folder) { this.folder = folder; }

    internal void Stage(string file, IReadOnlyList<string[]> rows)
    {
        var headers = rows[0];
        if (File.Exists(Path.Combine(folder, file)) && headers.Contains("Memo"))
            RetainAuthorNotes(file, rows, headers);
        var output = new StringBuilder();
        foreach (var row in rows)
        {
            if (row.Length != headers.Length) throw new InvalidDataException(file + ": CSV 열 개수가 다릅니다.");
            output.AppendLine(string.Join(",", row.Select(Escape)));
        }
        files.Add(file, output.ToString());
    }

    private void RetainAuthorNotes(string file, IReadOnlyList<string[]> rows, string[] headers)
    {
        var keys = new List<string> { "StoryId" };
        if (headers.Contains("NodeId")) keys.Add("NodeId");
        if (headers.Contains("ChoiceIndex")) keys.AddRange(new[] { "ChoiceIndex", "TargetNodeId" });
        if (headers.Contains("ConditionIndex")) keys.AddRange(new[] { "ConditionIndex", "ConditionType", "Key" });
        if (headers.Contains("EffectIndex")) keys.AddRange(new[] { "EffectIndex", "EffectType", "Target" });
        var old = StoryCsvRow.Read(folder, file, string.Join(" ", keys))
            .ToLookup(r => MakeKey(keys.Select(r.Id)), StringComparer.Ordinal);
        var keyIndexes = keys.Select(k => Array.IndexOf(headers, k)).ToArray();
        int memo = Array.IndexOf(headers, "Memo");
        int branch = Array.IndexOf(headers, "Branch/Scene");
        for (int i = 1; i < rows.Count; i++)
        {
            string[] row = rows[i];
            var matches = old[MakeKey(keyIndexes.Select(k => (row[k] ?? "").Trim()))].ToList();
            if (matches.Count != 1) continue;
            row[memo] = matches[0].Raw("Memo");
            if (branch >= 0) row[branch] = matches[0].Raw("Branch/Scene");
        }
    }

    private static string MakeKey(IEnumerable<string> parts) =>
        string.Concat(parts.Select(s => s.Length.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" + s));

    private static string Escape(string value)
    {
        if (value == null) return "";
        return value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0 ? value :
            "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    internal void Commit()
    {
        Directory.CreateDirectory(folder);
        string token = "." + Guid.NewGuid().ToString("N");
        var installed = new List<string>();
        try
        {
            foreach (var file in files)
                File.WriteAllText(Path.Combine(folder, file.Key + token + "~"), file.Value, new UTF8Encoding(true));
            BackupExistingFiles();
            foreach (string file in files.Keys)
            {
                string path = Path.Combine(folder, file);
                if (File.Exists(path)) File.Replace(path + token + "~", path, path + token + ".old~");
                else File.Move(path + token + "~", path);
                installed.Add(path);
            }
        }
        catch
        {
            for (int i = installed.Count - 1; i >= 0; i--)
            {
                string path = installed[i];
                string backup = path + token + ".old~";
                if (File.Exists(backup)) File.Replace(backup, path, null);
                else File.Delete(path);
            }
            throw;
        }
        finally
        {
            foreach (string file in files.Keys) File.Delete(Path.Combine(folder, file + token + "~"));
        }
        foreach (string path in installed) File.Delete(path + token + ".old~");
    }

    private void BackupExistingFiles()
    {
        string parent = Directory.GetParent(Path.GetFullPath(folder)).FullName;
        string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss-ffffff", System.Globalization.CultureInfo.InvariantCulture);
        string backupFolder = Path.Combine(parent, ".story-backups", stamp);
        foreach (string name in files.Keys)
        {
            string source = Path.Combine(folder, name);
            if (!File.Exists(source)) continue;
            Directory.CreateDirectory(backupFolder);
            File.Copy(source, Path.Combine(backupFolder, name));
        }
    }
}
