using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ForeignKeyDto
{
    public string FromTable;
    public string FromColumn;
    public string ToTable;
    public string ToColumn;
}
