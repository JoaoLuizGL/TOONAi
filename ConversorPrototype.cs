using System;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;

public class JsonStructureAnalyzer
{
    public class JsonNode
    {
        public string Path { get; set; }
        public JsonValueKind Type { get; set; }
        public List<string> Fields { get; set; }
        public int? ArrayLength { get; set; }
        public List<JsonNode> Children { get; set; }
        public JsonValueKind? PrimitiveType { get; set; }  // Para arrays primitivos
        public int? MaxStringLength { get; set; }  // Comprimento máximo de strings
        
        public JsonNode()
        {
            Fields = new List<string>();
            Children = new List<JsonNode>();
        }
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Path: {Path}, Type: {Type}");
            
            if (Type == JsonValueKind.Array)
                sb.Append($", Length: {ArrayLength}");
            
            if (Fields.Any())
                sb.Append($", Fields: [{string.Join(", ", Fields)}]");
            
            return sb.ToString();
        }
    }
    
    public static JsonNode AnalyzeStructure(string json)
    {
        JsonDocument doc = JsonDocument.Parse(json);
        return AnalyzeElement(doc.RootElement, "root");
    }
    
    private static JsonNode AnalyzeElement(JsonElement element, string path)
    {
        JsonNode node = new JsonNode
        {
            Path = path,
            Type = element.ValueKind
        };
        
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                AnalyzeObject(element, node);
                break;
                
            case JsonValueKind.Array:
                AnalyzeArray(element, node);
                break;
        }
        
        return node;
    }
    
    private static void AnalyzeObject(JsonElement obj, JsonNode node)
    {
        foreach (var property in obj.EnumerateObject())
        {
            node.Fields.Add(property.Name);
            
            string childPath = node.Path == "root" ? property.Name : $"{node.Path}.{property.Name}";
            JsonNode childNode = AnalyzeElement(property.Value, childPath);
            node.Children.Add(childNode);
        }
    }
    
    private static void AnalyzeArray(JsonElement array, JsonNode node)
    {
        node.ArrayLength = array.GetArrayLength();
        
        if (node.ArrayLength == 0)
            return;
        
        var firstElement = array.EnumerateArray().First();
        
        if (firstElement.ValueKind == JsonValueKind.Object)
        {
            // Array de objetos - extrai os campos do primeiro objeto
            foreach (var property in firstElement.EnumerateObject())
            {
                node.Fields.Add(property.Name);
            }
        }
        else if (firstElement.ValueKind == JsonValueKind.Array)
        {
            // Array de arrays - analisa recursivamente
            JsonNode childNode = AnalyzeElement(firstElement, $"{node.Path}[0]");
            node.Children.Add(childNode);
        }
        else
        {
            // Array de valores primitivos - adiciona campo padrão "value"
            node.Fields.Add("value");
            node.PrimitiveType = firstElement.ValueKind;
            
            // Se for string, calcula o comprimento máximo
            if (firstElement.ValueKind == JsonValueKind.String)
            {
                int maxLength = 0;
                foreach (var item in array.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        int length = item.GetString()?.Length ?? 0;
                        if (length > maxLength)
                            maxLength = length;
                    }
                }
                node.MaxStringLength = maxLength;
            }
        }
    }
    
    public static string PrintStructure(JsonNode node, int indentLevel = 0)
    {
        StringBuilder sb = new StringBuilder();
        string indent = new string(' ', indentLevel * 2);
        
        sb.Append(indent);
        sb.Append($"└─ {node.Path}");
        sb.Append($" [{node.Type}]");
        
        if (node.Type == JsonValueKind.Array)
        {
            sb.Append($" (length: {node.ArrayLength})");
            
            if (node.Fields.Any())
            {
                sb.Append($" → fields: ({string.Join(", ", node.Fields)})");
            }
        }
        else if (node.Type == JsonValueKind.Object && node.Fields.Any())
        {
            sb.Append($" → fields: ({string.Join(", ", node.Fields)})");
        }
        
        sb.AppendLine();
        
        foreach (var child in node.Children)
        {
            sb.Append(PrintStructure(child, indentLevel + 1));
        }
        
        return sb.ToString();
    }
}

// TESTE COM O JSON FORNECIDO
class Program
{
    static void Main()
    {
        string json = @"{
  ""empresa"": ""Tech Solutions Ltda"",
  ""fundacao"": 2010,
  ""ativa"": true,
  ""endereco"": {
    ""rua"": ""Av. Paulista, 1000"",
    ""cidade"": ""São Paulo"",
    ""estado"": ""SP"",
    ""cep"": ""01310-100""
  },
  ""telefones"": [
    ""(11) 9999-9999"",
    ""(11) 8888-8888""
  ],
  ""departamentos"": [
    {
      ""id"": 1,
      ""nome"": ""Desenvolvimento"",
      ""gerente"": ""João Silva"",
      ""funcionarios"": 15
    },
    {
      ""id"": 2,
      ""nome"": ""Marketing"",
      ""gerente"": ""Maria Santos"",
      ""funcionarios"": 8
    }
  ],
  ""projetos_ativos"": [
    ""Sistema de Gestão"",
    ""App Mobile"",
    ""Portal Corporativo""
  ],
  ""metas"": {
    ""faturamento_anual"": 5000000.00,
    ""crescimento"": 15.5,
    ""novos_clientes"": 50
  },
  ""tags"": [""tecnologia"", ""inovação"", ""desenvolvimento"", ""consultoria""],
  ""observacoes"": null
}";

        Console.WriteLine("=== ANÁLISE DA ESTRUTURA DO JSON ===\n");
        
        var estrutura = JsonStructureAnalyzer.AnalyzeStructure(json);
        Console.WriteLine(JsonStructureAnalyzer.PrintStructure(estrutura));
        
        Console.WriteLine("\n=== INFORMAÇÕES DETALHADAS ===\n");
        
        // Mostra informações sobre cada array encontrado
        Console.WriteLine("Arrays encontrados:");
        MostrarArrays(estrutura, 0);
        
        Console.WriteLine("\n\nObjetos aninhados encontrados:");
        MostrarObjetosAninhados(estrutura, 0);
        
        Console.WriteLine("\n\nCampos unitários encontrados:");
        MostrarCamposUnitarios(estrutura, 0);
        
        Console.WriteLine("\n\n=== CONVERSÃO PARA TOON ===\n");
        string toonOutput = ConverterParaTOON(json);
        Console.WriteLine(toonOutput);
    }
    
    static void MostrarArrays(JsonStructureAnalyzer.JsonNode node, int level)
    {
        if (node.Type == JsonValueKind.Array)
        {
            string indent = new string(' ', level * 2);
            Console.WriteLine($"{indent}• {node.Path}");
            Console.WriteLine($"{indent}  Tamanho: {node.ArrayLength}");
            Console.WriteLine($"{indent}  Campos: {string.Join(", ", node.Fields)}");
            
            // Indica se é um array de valores primitivos (campo padrão "value")
            if (node.Fields.Count == 1 && node.Fields[0] == "value")
            {
                string tipoFormatado = FormatarTipo(node.PrimitiveType, node.MaxStringLength);
                Console.WriteLine($"{indent}  Tipo: {tipoFormatado}");
                Console.WriteLine($"{indent}  (Array de valores primitivos - campo padrão adicionado)");
            }
        }
        
        foreach (var child in node.Children)
        {
            MostrarArrays(child, level + 1);
        }
    }
    
    static string FormatarTipo(JsonValueKind? tipo, int? maxLength)
    {
        if (!tipo.HasValue)
            return "unknown";
            
        switch (tipo.Value)
        {
            case JsonValueKind.String:
                return maxLength.HasValue ? $"string({maxLength})" : "string";
            case JsonValueKind.Number:
                return "number";
            case JsonValueKind.True:
            case JsonValueKind.False:
                return "boolean";
            default:
                return tipo.Value.ToString().ToLower();
        }
    }
    
    static void MostrarCamposUnitarios(JsonStructureAnalyzer.JsonNode node, int level)
    {
        string indent = new string(' ', level * 2);
        
        // Percorre os filhos diretos procurando por valores primitivos
        foreach (var child in node.Children)
        {
            if (child.Type != JsonValueKind.Array && child.Type != JsonValueKind.Object)
            {
                string tipo = ObterTipoFormatado(child.Type);
                Console.WriteLine($"{indent}• {child.Path} → {tipo}");
            }
            else if (child.Type == JsonValueKind.Object)
            {
                // Recursivamente busca em objetos aninhados
                MostrarCamposUnitarios(child, level + 1);
            }
        }
    }
    
    static string ObterTipoFormatado(JsonValueKind tipo)
    {
        switch (tipo)
        {
            case JsonValueKind.String:
                return "string";
            case JsonValueKind.Number:
                return "number";
            case JsonValueKind.True:
            case JsonValueKind.False:
                return "boolean";
            case JsonValueKind.Null:
                return "null";
            default:
                return tipo.ToString().ToLower();
        }
    }
    
    static string ConverterParaTOON(string json)
    {
        JsonDocument doc = JsonDocument.Parse(json);
        StringBuilder sb = new StringBuilder();
        
        ProcessarElementoTOON(doc.RootElement, sb, "root", 0);
        
        return sb.ToString().TrimEnd();
    }
    
    static void ProcessarElementoTOON(JsonElement element, StringBuilder sb, string nome, int nivelIndentacao)
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
    
    static void ProcessarObjetoTOON(JsonElement obj, StringBuilder sb, string nome, int nivelIndentacao)
    {
        bool primeiro = true;
        string indent = new string('\t', nivelIndentacao);
        
        foreach (var property in obj.EnumerateObject())
        {
            // Adiciona linha em branco entre campos (exceto no primeiro)
            if (!primeiro)
                sb.AppendLine();
            primeiro = false;
            
            string propNome = property.Name;
            
            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                // Objeto aninhado - exibe o nome e processa recursivamente com mais indentação
                if (nome != "root")
                    sb.AppendLine($"{indent}{propNome}");
                else
                    sb.AppendLine($"{propNome}");
                    
                ProcessarObjetoTOON(property.Value, sb, propNome, nome == "root" ? nivelIndentacao + 1 : nivelIndentacao + 1);
            }
            else if (property.Value.ValueKind == JsonValueKind.Array)
            {
                // Array - processa como array
                string nomeCompleto = nome == "root" ? propNome : propNome;
                ProcessarArrayTOON(property.Value, sb, nomeCompleto, nivelIndentacao);
            }
            else
            {
                // Campo unitário - formato tabular
                if (nome != "root")
                    sb.AppendLine($"{indent}{propNome}");
                else
                    sb.AppendLine($"{propNome}");
                    
                sb.AppendLine($"{indent}\t{FormatarValor(property.Value)}");
            }
        }
    }
    
    static void ProcessarArrayTOON(JsonElement array, StringBuilder sb, string nome, int nivelIndentacao)
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
            // Array de objetos - formato tabular
            var fields = firstElement.EnumerateObject().Select(p => p.Name).ToList();
            
            // Cabeçalho
            sb.Append($"{indent}{nome}[{length}] (");
            sb.Append(string.Join(", ", fields));
            sb.AppendLine(")");
            
            // Valores
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
            // Array de valores primitivos
            sb.AppendLine($"{indent}{nome}[{length}] (value)");
            
            foreach (var item in array.EnumerateArray())
            {
                sb.Append($"{indent}\t");
                sb.AppendLine(FormatarValor(item));
            }
        }
    }
    
    static string FormatarValor(JsonElement element)
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
                string str = element.GetString();
                // Se contém vírgula, coloca entre aspas
                if (str.Contains(','))
                    return $"\"{str}\"";
                return str;
            default:
                return element.GetRawText();
        }
    }
    
    static void MostrarObjetosAninhados(JsonStructureAnalyzer.JsonNode node, int level)
    {
        if (node.Type == JsonValueKind.Object && node.Path != "root")
        {
            string indent = new string(' ', level * 2);
            Console.WriteLine($"{indent}• {node.Path}");
            Console.WriteLine($"{indent}  Campos: {string.Join(", ", node.Fields)}");
        }
        
        foreach (var child in node.Children)
        {
            MostrarObjetosAninhados(child, level + 1);
        }
    }
}
