using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Models;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Serialization | JsonSourceGenerationMode.Metadata,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    IncludeFields = false
)]
[JsonSerializable(typeof(RuleChange))]
[JsonSerializable(typeof(IReadOnlyList<RuleChange>))]
[SuppressMessage(
    category: "ReSharper",
    checkId: "PartialTypeWithSinglePart",
    Justification = "Required for " + nameof(JsonSerializerContext) + " code generation"
)]
internal sealed partial class RuleChangesJsonSerializerContext : JsonSerializerContext;
