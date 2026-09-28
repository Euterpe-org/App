using System.Text.Json.Serialization;
using Euterpe.Contracts.Account;
using Euterpe.Contracts.Charts;
using Euterpe.Contracts.Credits;
using Euterpe.Contracts.Distribution;
using Euterpe.Contracts.Mods;
using Euterpe.Contracts.Telemetry;
using Euterpe.Models.Charts.CustomAlbums;

namespace Euterpe.Core.JsonContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(AppTokenRequest))]
[JsonSerializable(typeof(AppTokenResponse))]
[JsonSerializable(typeof(Chart[]))]
[JsonSerializable(typeof(CheckChartUpdatesRequest))]
[JsonSerializable(typeof(CheckChartUpdatesResponse))]
[JsonSerializable(typeof(Cinema))]
[JsonSerializable(typeof(CreditsResponse))]
[JsonSerializable(typeof(CurrentUserResponse))]
[JsonSerializable(typeof(Dependency[]))]
[JsonSerializable(typeof(Lib[]))]
[JsonSerializable(typeof(LogoutRequest))]
[JsonSerializable(typeof(Mod[]))]
[JsonSerializable(typeof(RefreshRequest))]
[JsonSerializable(typeof(RefreshResponse))]
[JsonSerializable(typeof(SessionEvent))]
[JsonSerializable(typeof(TokenPayload))]
[JsonSerializable(typeof(UserInfo))]
public sealed partial class SnakeCaseJsonContext : JsonSerializerContext;
