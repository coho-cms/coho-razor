using System.Net;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using CohoCms.Delivery;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace CohoCms.Razor.Tests;

/// <summary>The assets the tests render, as delivery embeds them: the same as coho-react's tests.</summary>
internal static class Assets
{
    public const string Base = "https://assets.test/p1";

    public static readonly Asset Photo = new()
    {
        Id = "a1",
        Url = $"{Base}/a/abc/culvert.png",
        ImageUrl = $"{Base}/i/abc/culvert.png",
        File = new AssetFile { Filename = "culvert.png", MediaType = "image/png", Width = 2000, Height = 1000 },
        FocalPoint = new FocalPoint(0.25, 0.75),
        Alt = "A culvert under Elm Street",
    };

    public static readonly Asset Svg = new()
    {
        Id = "a2",
        Url = $"{Base}/a/def/logo.svg",
        File = new AssetFile { Filename = "logo.svg", MediaType = "image/svg+xml" },
    };

    public static readonly Asset Pdf = new()
    {
        Id = "a3",
        Url = $"{Base}/a/123/roundabout-functionality.pdf",
        File = new AssetFile { Filename = "roundabout-functionality.pdf", MediaType = "application/pdf" },
        Title = "How a roundabout works",
    };
}

internal static partial class Render
{
    /// <summary>A component, rendered to HTML as a page would render it on the server.</summary>
    public static async Task<string> Component<T>(Dictionary<string, object?> parameters)
        where T : IComponent
    {
        await using var services = new ServiceCollection().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, NullLoggerFactory.Instance);
        return await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    }

    /// <summary>
    /// A tag helper, run as a view runs it: <paramref name="unbound"/> are the attributes the page
    /// wrote that the tag helper does not bind, and <paramref name="content"/> what is inside it.
    /// </summary>
    public static async Task<string> TagHelper(TagHelper helper, string tag, TagHelperAttributeList? unbound = null, string content = "")
    {
        var attributes = unbound ?? [];
        var context = new TagHelperContext(tag, attributes, new Dictionary<object, object>(), Guid.NewGuid().ToString("N"));
        var output = new TagHelperOutput(tag, attributes, (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent().SetContent(content)));
        await helper.ProcessAsync(context, output);
        // As a view does: content the tag helper left alone is the page's own.
        if (!output.IsContentModified)
        {
            output.Content.SetHtmlContent(await output.GetChildContentAsync());
        }
        await using var writer = new StringWriter();
        output.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    /// <summary>The attributes of the one element in <paramref name="html"/>, decoded, named in lowercase as a browser reads them.</summary>
    public static Dictionary<string, string> Attributes(string html)
    {
        return AttributePattern().Matches(html)
            .ToDictionary(match => match.Groups[1].Value.ToLowerInvariant(), match => WebUtility.HtmlDecode(match.Groups[2].Value));
    }

    [GeneratedRegex(" ([a-zA-Z-]+)=\"([^\"]*)\"")]
    private static partial Regex AttributePattern();
}
