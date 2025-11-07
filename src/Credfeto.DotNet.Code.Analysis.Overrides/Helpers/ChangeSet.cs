using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.DotNet.Code.Analysis.Overrides.Models;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Helpers;

public static class ChangeSet
{
    public static async ValueTask<IReadOnlyList<RuleChange>> LoadAsync(
        string changesFileName,
        CancellationToken cancellationToken
    )
    {
        await using (FileStream stream = File.OpenRead(changesFileName))
        {
            return await JsonSerializer.DeserializeAsync<IReadOnlyList<RuleChange>>(
                    utf8Json: stream,
                    jsonTypeInfo: RuleChangesJsonSerializerContext.Default.IReadOnlyListRuleChange,
                    cancellationToken: cancellationToken
                ) ?? [];
        }
    }
}
