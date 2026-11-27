using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

// Record numbers include the header. A multiline dialogue cell counts as one record.
internal sealed class StoryCsvRow
{
    private readonly Dictionary<string, string> cells;
    internal string Location { get; }
    internal StoryCsvRow(Dictionary<string, string> cells, string location)
    {
        this.cells = cells;
        Location = location;
    }

    internal string Raw(string name) => cells.TryGetValue(name, out var value) ? value : "";
    internal string Id(string name) => Raw(name).Trim();
    internal string Required(string name)
    {
        string value = Id(name);
        if (value.Length == 0) throw Error(name + " 값이 비었습니다.");
        return value;
    }

    internal int Integer(string name, int? fallback = null, int min = int.MinValue, int max = int.MaxValue)
    {
        string value = Id(name);
        if (value.Length == 0 && fallback.HasValue) return fallback.Value;
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result)
            || result < min || result > max)
            throw Error($"{name}: 정수 범위 밖의 값 '{value}'");
        return result;
    }

    internal float Seconds(string name, float fallback)
    {
        string value = Id(name);
        if (value.Length == 0) return fallback;
        if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result)
            || float.IsInfinity(result) || float.IsNaN(result) || result < 0f)
            throw Error($"{name}: 0 이상의 유한한 숫자가 필요합니다 ('{value}').");
        return result;
    }

    internal bool Flag(string name, bool fallback = false)
    {
        string value = Id(name);
        if (value.Length == 0) return fallback;
        if (value == "1") return true;
        if (value == "0") return false;
        if (bool.TryParse(value, out bool result)) return result;
        throw Error($"{name}: TRUE/FALSE 또는 1/0이 필요합니다 ('{value}').");
    }

    internal T EnumValue<T>(string name, T fallback) where T : struct, Enum
    {
        string value = Id(name);
        if (value.Length == 0) return fallback;
        if (Enum.GetNames(typeof(T)).Contains(value, StringComparer.OrdinalIgnoreCase))
            return (T)Enum.Parse(typeof(T), value, true);
        throw Error($"{name}: 알 수 없는 값 '{value}'");
    }

    internal InvalidDataException Error(string message) => new InvalidDataException($"{Location}: {message}");

    internal static List<StoryCsvRow> Read(string folder, string name, string requiredHeaders)
    {
        string path = Path.Combine(folder, name);
        if (!File.Exists(path)) throw new FileNotFoundException($"CSV 파일 없음: {path}", path);
        string csv = File.ReadAllText(path, new UTF8Encoding(false, true));
        var records = Parse(csv, name);
        if (records.Count == 0) throw new InvalidDataException($"{name}: 헤더 없음");
        var headers = records[0].Select(h => h.Trim().TrimStart('\uFEFF')).ToArray();
        var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string header in headers)
            if (header.Length == 0 || !unique.Add(header))
                throw new InvalidDataException($"{name}: 빈 헤더 또는 중복 헤더 '{header}'");
        foreach (string header in requiredHeaders.Split(' '))
            if (!unique.Contains(header)) throw new InvalidDataException($"{name}: '{header}' 열 없음");
        var rows = new List<StoryCsvRow>();
        for (int i = 1; i < records.Count; i++)
        {
            var record = records[i];
            if (record.All(string.IsNullOrWhiteSpace)) continue;
            if (record.Count != headers.Length)
                throw new InvalidDataException($"{name} 레코드 {i + 1}: 열 개수가 다릅니다 ({record.Count}/{headers.Length}).");
            var cells = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int column = 0; column < headers.Length; column++) cells.Add(headers[column], record[column]);
            rows.Add(new StoryCsvRow(cells, $"{name} 레코드 {i + 1}"));
        }
        return rows;
    }

    // RFC-style quoted fields, escaped quotes and both kinds of line endings.
    private static List<List<string>> Parse(string csv, string file)
    {
        var records = new List<List<string>>();
        var record = new List<string>();
        var field = new StringBuilder();
        bool quoted = false, closed = false;
        for (int i = 0; i < csv.Length; i++)
        {
            char ch = csv[i];
            if (quoted)
            {
                if (ch != '"') field.Append(ch);
                else if (i + 1 < csv.Length && csv[i + 1] == '"') { field.Append('"'); i++; }
                else { quoted = false; closed = true; }
                continue;
            }
            if (ch == ',' || ch == '\r' || ch == '\n')
            {
                record.Add(field.ToString());
                field.Clear();
                closed = false;
                if (ch == ',') continue;
                if (ch == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n') i++;
                records.Add(record);
                record = new List<string>();
            }
            else if (ch == '"' && field.Length == 0 && !closed) quoted = true;
            else if (ch == '"' || closed)
                throw new InvalidDataException($"{file} 레코드 {records.Count + 1}: 잘못된 따옴표");
            else field.Append(ch);
        }
        if (quoted) throw new InvalidDataException($"{file} 레코드 {records.Count + 1}: 닫히지 않은 따옴표");
        if (closed || field.Length != 0 || record.Count > 0)
        {
            record.Add(field.ToString());
            records.Add(record);
        }
        return records;
    }
}
