using System.Globalization;
using CohoCms.Delivery;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CohoCms.Razor;

/// <summary>
/// <see cref="CohoImage"/> for MVC views and Razor Pages:
/// <c>&lt;coho-image src="@Model.Post.Hero" width="800" /&gt;</c> renders an <c>img</c>.
/// </summary>
/// <remarks>
/// Register it with <c>@addTagHelper *, CohoCms.Razor</c> in <c>_ViewImports.cshtml</c>. The
/// attributes are <see cref="CohoImage"/>'s parameters, in lowercase, and <c>fit</c> and
/// <c>format</c> take a member's name, such as <c>fit="Fill"</c>. Any other attribute, such as
/// <c>class</c> or <c>loading</c>, goes to the <c>img</c>.
/// </remarks>
[HtmlTargetElement("coho-image", TagStructure = TagStructure.WithoutEndTag)]
public sealed class CohoImageTagHelper : TagHelper
{
    /// <summary>The asset to show: an entry's image field. Nothing is rendered for a missing image.</summary>
    [HtmlAttributeName("src")]
    public Asset? Src { get; set; }

    /// <summary>The width the image is shown at, in CSS pixels. Defaults to the original's.</summary>
    [HtmlAttributeName("width")]
    public int? Width { get; set; }

    /// <summary>The height it is shown at. Defaults to the width at the original's aspect ratio.</summary>
    [HtmlAttributeName("height")]
    public int? Height { get; set; }

    /// <summary>See <see cref="CohoImage.Fit"/>.</summary>
    [HtmlAttributeName("fit")]
    public ImageFit Fit { get; set; } = ImageFit.Scale;

    /// <summary>See <see cref="CohoImage.Format"/>.</summary>
    [HtmlAttributeName("format")]
    public ImageFormat Format { get; set; } = ImageFormat.Auto;

    /// <summary>1 to 100; the edge's default is 75.</summary>
    [HtmlAttributeName("quality")]
    public int? Quality { get; set; }

    /// <summary>Ignored. The background is a setting on the transform, not a request parameter.</summary>
    // ⚠️ Kept, ignored, and marked obsolete rather than deleted — which is the opposite
    // of what the delivery SDK did with its own option, for a reason specific to here.
    // This member is *bound*, so the framework consumes the attribute. Delete it and
    // `background="…"` on the tag becomes an unbound attribute that passes straight
    // through to the rendered `img`, which is a worse outcome than ignoring it. Obsolete
    // says so at compile time, which is a louder signal than a type could give.
    [Obsolete("The asset edge no longer accepts bg. The colour a Pad is filled with, and a jpg flattened onto, is a setting on the transform. This value is ignored, and the member will be removed.")]
    [HtmlAttributeName("background")]
    public string? Background { get; set; }

    /// <summary>How wide the image is shown, as the <c>sizes</c> attribute.</summary>
    [HtmlAttributeName("sizes")]
    public string? Sizes { get; set; }

    /// <summary>The widths to offer with <see cref="Sizes"/>. Defaults to the edge's widths from 640, up to the original.</summary>
    [HtmlAttributeName("widths")]
    public IEnumerable<int>? Widths { get; set; }

    /// <summary>Overrides the asset's alt text. <c>alt=""</c> for a purely decorative image.</summary>
    [HtmlAttributeName("alt")]
    public string? Alt { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        if (ImageMarkup.For(Src, Width, Height, Fit, Format, Quality, Sizes, Widths, Alt) is not { } image)
        {
            output.SuppressOutput();
            return;
        }

        output.TagName = "img";
        output.TagMode = TagMode.StartTagOnly;
        // The page's attributes win over these defaults, and lose to what is built from the asset.
        var at = 0;
        if (!output.Attributes.ContainsName("loading"))
        {
            output.Attributes.Insert(at++, new TagHelperAttribute("loading", "lazy"));
        }
        if (!output.Attributes.ContainsName("decoding"))
        {
            output.Attributes.Insert(at, new TagHelperAttribute("decoding", "async"));
        }
        output.Attributes.SetAttribute("alt", image.Alt);
        output.Attributes.SetAttribute("src", image.Src);
        Set(output, "srcset", image.Srcset);
        Set(output, "sizes", image.Sizes);
        Set(output, "width", image.Width?.ToString(CultureInfo.InvariantCulture));
        Set(output, "height", image.Height?.ToString(CultureInfo.InvariantCulture));
    }

    internal static void Set(TagHelperOutput output, string name, string? value)
    {
        if (value is not null)
        {
            output.Attributes.SetAttribute(name, value);
        }
    }
}

/// <summary>
/// <see cref="CohoAssetLink"/> for MVC views and Razor Pages:
/// <c>&lt;coho-asset-link asset="@file" /&gt;</c> renders an <c>a</c>.
/// </summary>
/// <remarks>
/// Its text is its content, or else the asset's title, or else its filename.
/// <c>download="false"</c> opens the file in the browser instead of downloading it. Any other
/// attribute, such as <c>class</c>, goes to the <c>a</c>.
/// </remarks>
[HtmlTargetElement("coho-asset-link")]
public sealed class CohoAssetLinkTagHelper : TagHelper
{
    /// <summary>The asset to link to: an entry's asset field. Nothing is rendered for a missing asset.</summary>
    [HtmlAttributeName("asset")]
    public Asset? Asset { get; set; }

    /// <summary>Download the file rather than open it in the browser. Defaults to true.</summary>
    [HtmlAttributeName("download")]
    public bool Download { get; set; } = true;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        if (AssetLinkMarkup.For(Asset, Download) is not { } link)
        {
            output.SuppressOutput();
            return;
        }

        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("href", link.Href);
        CohoImageTagHelper.Set(output, "download", link.Download);
        CohoImageTagHelper.Set(output, "type", link.Type);
        var content = await output.GetChildContentAsync().ConfigureAwait(false);
        if (content.IsEmptyOrWhiteSpace)
        {
            output.Content.SetContent(link.Text);
        }
    }
}
