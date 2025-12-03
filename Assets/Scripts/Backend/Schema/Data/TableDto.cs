using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TableDto
{
    public string Name;
    public List<ColumnDto> Columns;
    public List<ForeignKeyDto> ForeignKeys;
}
