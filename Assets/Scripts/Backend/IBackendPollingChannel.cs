using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public interface IBackendPollingChannel
{
    string Name { get; }
    bool IsEnabled { get; }
    float IntervalSeconds { get; }
    Task PollAsync(CancellationToken ct);

}
