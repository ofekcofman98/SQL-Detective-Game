using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SchemaManager : Singleton<SchemaManager>
{
    public List<Table> Tables { get; private set; }
    public bool IsLoaded { get; private set; }
    public event Action OnSchemaFullyLoaded;
    private ISchemaApi m_Api;


    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        m_Api = BackendFactory.CreateSchemaApi();
        Tables = new List<Table>();

        _ = LoadSchemaAsync();
    }

    private async Task LoadSchemaAsync(CancellationToken ct = default)
    {
        try
        {
            SchemaDto schemaDto = await m_Api.GetSchemaAsync(ct);
        
            Tables.Clear();
            Dictionary<string, Table> tablesByName = BuildTables(schemaDto);
            WireForeignKeys(schemaDto, tablesByName);

            Debug.Log("Schema fully loaded from backend");
            IsLoaded = true;
            OnSchemaFullyLoaded?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load schema from backend: {ex}");
        }
    }

    private Dictionary<string, Table> BuildTables(SchemaDto schemaDto)
    {
        Dictionary<string, Table> tablesByName = new Dictionary<string, Table>(StringComparer.OrdinalIgnoreCase);

        foreach(TableDto tableDto in schemaDto.Tables)
        {
            Table table = new Table(tableDto.Name, i_IsUnlocked: false);

            List<Column> columns = tableDto.Columns
            .Select(c =>
            {
                var column = new Column(c.Name, MapType(c.DataType));
                column.ParentTable = table;
                return column;
            }).ToList();

            table.SetColumns(columns);

            Tables.Add(table);
            tablesByName[table.Name] = table;
        }

        return tablesByName; 
    }

    private void WireForeignKeys(SchemaDto schemaDto, Dictionary<string, Table> tablesByName)
    {
        foreach (TableDto tableDto in schemaDto.Tables)
        {
            if (!tablesByName.TryGetValue(tableDto.Name, out Table fromTable))
            {
                continue;
            }

            foreach (ForeignKeyDto fkDto in tableDto.ForeignKeys)
            {
                if (!tablesByName.TryGetValue(fkDto.ToTable, out Table toTable))
                {
                    continue;
                }

                Column fromColumn = fromTable.Columns.FirstOrDefault(c => c.Name == fkDto.FromColumn);
                Column toColumn = toTable.Columns.FirstOrDefault(c => c.Name == fkDto.ToColumn);

                if (fromColumn == null || toColumn == null)
                {
                    continue;
                }

                fromTable.AddForeignKey(new ForeignKey(fromColumn, toTable, toColumn));
            }
        }
    }

    private eDataType MapType(string supabaseType)
    {
        switch (supabaseType.ToLower())
        {
            case "int4":
            case "integer":
            case "bigint":
                return eDataType.Integer;

            case "text":
            case "varchar":
            case "char":
            case "uuid":
                return eDataType.String;

            case "date":
            case "timestamp":
                return eDataType.String; 

            default:
                Debug.LogWarning($"Unmapped Supabase type: {supabaseType}");
                return eDataType.String;
        }
    }

    public bool IsTableUnlocked(string tableName)
    {
        Table table = Tables.FirstOrDefault(t => t.Name == tableName);
        return table != null && table.IsUnlocked;
    }
}
