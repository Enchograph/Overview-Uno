using Overview.Client.Domain.Enums;
using Overview.Client.Presentation.Pages;

namespace Overview.Client.Application.Navigation;

public enum AppNavigationTarget
{
    Home = 0,
    List = 1,
    Ai = 2,
    Add = 3,
    Settings = 4
}

public sealed record AppNavigationRequest
{
    public AppNavigationTarget Target { get; init; }

    public ItemType? SuggestedItemType { get; init; }

    public static string CreateDeepLink(AppNavigationTarget target, ItemType? suggestedItemType = null)
    {
        var path = target switch
        {
            AppNavigationTarget.Home => "home",
            AppNavigationTarget.List => "list",
            AppNavigationTarget.Ai => "ai",
            AppNavigationTarget.Add => "add",
            AppNavigationTarget.Settings => "settings",
            _ => "home"
        };

        if (target == AppNavigationTarget.Add && suggestedItemType is not null)
        {
            return $"overview://{path}?type={suggestedItemType.Value.ToString().ToLowerInvariant()}";
        }

        return $"overview://{path}";
    }

    public static AppNavigationRequest? Parse(string? deepLink)
    {
        if (string.IsNullOrWhiteSpace(deepLink) ||
            !Uri.TryCreate(deepLink, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, "overview", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var target = uri.Host.ToLowerInvariant() switch
        {
            "home" => AppNavigationTarget.Home,
            "list" => AppNavigationTarget.List,
            "ai" => AppNavigationTarget.Ai,
            "add" => AppNavigationTarget.Add,
            "settings" => AppNavigationTarget.Settings,
            _ => (AppNavigationTarget?)null
        };

        if (target is null)
        {
            return null;
        }

        ItemType? suggestedItemType = null;
        var query = uri.Query.TrimStart('?');
        foreach (var segment in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = segment.Split('=', 2, StringSplitOptions.TrimEntries);
            if (pair.Length != 2 || !string.Equals(pair[0], "type", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var decodedValue = Uri.UnescapeDataString(pair[1]);
            if (Enum.TryParse<ItemType>(decodedValue, ignoreCase: true, out var parsedType))
            {
                suggestedItemType = parsedType;
            }
        }

        return new AppNavigationRequest
        {
            Target = target.Value,
            SuggestedItemType = suggestedItemType
        };
    }

    public Type ResolvePageType()
    {
        return Target switch
        {
            AppNavigationTarget.Home => typeof(HomePage),
            AppNavigationTarget.List => typeof(ListPage),
            AppNavigationTarget.Ai => typeof(AiPage),
            AppNavigationTarget.Add => typeof(AddItemPage),
            AppNavigationTarget.Settings => typeof(SettingsPage),
            _ => typeof(HomePage)
        };
    }

    public object? ResolveNavigationParameter()
    {
        if (Target != AppNavigationTarget.Add)
        {
            return null;
        }

        return new AddItemNavigationRequest
        {
            SuggestedType = SuggestedItemType
        };
    }
}
