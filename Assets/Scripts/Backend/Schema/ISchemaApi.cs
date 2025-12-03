using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public interface ISchemaApi
{
    Task<SchemaDto> GetSchemaAsync(CancellationToken ct = default);
}
