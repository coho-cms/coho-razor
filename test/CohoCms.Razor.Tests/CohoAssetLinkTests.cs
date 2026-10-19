using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;
using static CohoCms.Razor.Tests.Assets;

namespace CohoCms.Razor.Tests;

/// <summary>
/// <see cref="CohoAssetLink"/> and <see cref="CohoAssetLinkTagHelper"/>, which render the same
/// <c>a</c>. The cases are coho-react's <c>CohoAssetLink</c>'s.
/// </summary>
public sealed class CohoAssetLinkTests
{
    public static TheoryData<string> Renderers => new("component", "tag helper");

    private static Task<string> Link(string renderer, CohoCms.Delivery.Asset? asset, bool download = true, string? text = null, string? @class = null)
    {
        if (renderer == "tag helper")
        {
            var unbound = new TagHelperAttributeList();
            if (@class is not null)
            {
                unbound.Add("class", @class);
            }
            return Render.TagHelper(new CohoAssetLinkTagHelper { Asset = asset, Download = download }, "coho-asset-link", unbound, text ?? "");
        }
        var parameters = new Dictionary<string, object?> { [nameof(CohoAssetLink.Asset)] = asset, [nameof(CohoAssetLink.Download)] = download };
        if (text is not null)
        {
            parameters[nameof(CohoAssetLink.ChildContent)] = (RenderFragment)(builder => builder.AddContent(0, text));
        }
        if (@class is not null)
        {
            parameters["class"] = @class;
        }
        return Render.Component<CohoAssetLink>(parameters);
    }

    [Theory]
    [MemberData(nameof(Renderers))]
    public async Task An_asset_link_downloads_the_file_named_by_its_title_with_its_media_type(string renderer)
    {
        var html = await Link(renderer, Pdf);

        Assert.EndsWith(">How a roundabout works</a>", html, StringComparison.Ordinal);
        var a = Render.Attributes(html);
        Assert.Equal($"{Pdf.Url}?dl=1", a["href"]);
        Assert.Equal("roundabout-functionality.pdf", a["download"]);
        Assert.Equal("application/pdf", a["type"]);
    }

    [Theory]
    [MemberData(nameof(Renderers))]
    public async Task An_asset_link_can_open_the_file_instead_with_text_of_its_own(string renderer)
    {
        var html = await Link(renderer, Pdf, download: false, text: "Read the guide", @class: "doc");

        Assert.EndsWith(">Read the guide</a>", html, StringComparison.Ordinal);
        var a = Render.Attributes(html);
        Assert.Equal(Pdf.Url, a["href"]);
        Assert.False(a.ContainsKey("download"));
        Assert.Equal("doc", a["class"]);
    }

    [Theory]
    [MemberData(nameof(Renderers))]
    public async Task With_no_title_an_asset_link_reads_as_its_filename(string renderer)
    {
        Assert.EndsWith(">roundabout-functionality.pdf</a>", await Link(renderer, Pdf with { Title = null }), StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(Renderers))]
    public async Task A_missing_asset_or_one_with_no_url_renders_no_link(string renderer)
    {
        Assert.Equal("", await Link(renderer, null));
        Assert.Equal("", await Link(renderer, Pdf with { Url = null }));
    }

    [Theory]
    [MemberData(nameof(Renderers))]
    public async Task A_title_is_text_not_markup(string renderer)
    {
        Assert.EndsWith(">&lt;b&gt;Guide&lt;/b&gt;</a>", await Link(renderer, Pdf with { Title = "<b>Guide</b>" }), StringComparison.Ordinal);
    }
}
