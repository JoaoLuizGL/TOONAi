using System.Text;
using System.Text.Json;

namespace AutoTOONeAPI;

public static class ToonConverter
{
    public static string ConvertJsonToToon(JsonElement element)
    {
        Console.WriteLine(element);
        StringBuilder sb = new StringBuilder();
        ProcessarElementoTOON(element, sb, "root", 0);
        return sb.ToString().TrimEnd();
    }

    private static void ProcessarElementoTOON(JsonElement element, StringBuilder sb, string nome, int nivelIndentacao)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                ProcessarObjetoTOON(element, sb, nome, nivelIndentacao);
                break;
            case JsonValueKind.Array:
                ProcessarArrayTOON(element, sb, nome, nivelIndentacao);
                break;
        }
    }

    private static void ProcessarObjetoTOON(JsonElement obj, StringBuilder sb, string nome, int nivelIndentacao)
    {
        bool primeiro = true;
        string indent = new string('\t', nivelIndentacao);

        foreach (var property in obj.EnumerateObject())
        {
            if (!primeiro)
                sb.AppendLine();
            primeiro = false;

            string propNome = property.Name;

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                if (nome != "root")
                    sb.AppendLine($"{indent}{propNome}");
                else
                    sb.AppendLine($"{propNome}");

                ProcessarObjetoTOON(property.Value, sb, propNome, nome == "root" ? nivelIndentacao + 1 : nivelIndentacao + 1);
            }
            else if (property.Value.ValueKind == JsonValueKind.Array)
            {
                ProcessarArrayTOON(property.Value, sb, propNome, nivelIndentacao);
            }
            else
            {
                if (nome != "root")
                    sb.AppendLine($"{indent}{propNome}");
                else
                    sb.AppendLine($"{propNome}");

                sb.AppendLine($"{indent}\t{FormatarValor(property.Value)}");
            }
        }
    }

    private static void ProcessarArrayTOON(JsonElement array, StringBuilder sb, string nome, int nivelIndentacao)
    {
        int length = array.GetArrayLength();
        string indent = new string('\t', nivelIndentacao);

        if (length == 0)
        {
            sb.AppendLine($"{indent}{nome}[0]");
            return;
        }

        var firstElement = array.EnumerateArray().First();

        if (firstElement.ValueKind == JsonValueKind.Object)
        {
            var fields = firstElement.EnumerateObject().Select(p => p.Name).ToList();

            sb.Append($"{indent}{nome}[{length}] (");
            sb.Append(string.Join(", ", fields));
            sb.AppendLine(")");

            foreach (var item in array.EnumerateArray())
            {
                sb.Append($"{indent}\t");

                var valores = new List<string>();
                foreach (var field in fields)
                {
                    if (item.TryGetProperty(field, out JsonElement valor))
                    {
                        valores.Add(FormatarValor(valor));
                    }
                    else
                    {
                        valores.Add("null");
                    }
                }

                sb.AppendLine(string.Join(",", valores));
            }
        }
        else
        {
            sb.AppendLine($"{indent}{nome}[{length}] (value)");

            foreach (var item in array.EnumerateArray())
            {
                sb.Append($"{indent}\t");
                sb.AppendLine(FormatarValor(item));
            }
        }
    }

    private static string FormatarValor(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Null:
                return "null";
            case JsonValueKind.True:
                return "True";
            case JsonValueKind.False:
                return "False";
            case JsonValueKind.Number:
                return element.GetRawText();
            case JsonValueKind.String:
                string str = element.GetString() ?? "";
                if (str.Contains(','))
                    return $"\"{str}\"";
                return str;
            default:
                return element.GetRawText();
        }
    }
}