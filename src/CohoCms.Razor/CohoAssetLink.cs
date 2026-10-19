using CohoCms.Delivery;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CohoCms.Razor;

/// <summary>
/// A link to an asset from Coho, such as a PDF: <c>&lt;CohoAssetLink Asset="post.Brochure" /&gt;</c>.
/// </summary>
/// <remarks>
/// Its text is its child content, or else the asset's title, or else its filename. The link
/// downloads the file under its own name; set <see cref="Download"/> to false to open it in the
/// browser instead. It carries the file's media type, so a browser or screen reader can say what
/// it is.
/// <para>
/// ⚠️ Downloading needs the asset CDN to send the file as an attachment, which it does not do
/// yet. Until it does, the link opens the file, and needs no change when it starts to.
/// </para>
/// </remarks>
public sealed class CohoAssetLink : ComponentBase
{
    /// <summary>The asset to link to: an entry's asset field. Nothing is rendered for a missing asset.</summary>
    [Parameter]
    [EditorRequired]
    public Asset? Asset { get; set; }

    /// <summary>Download the file rather than open it in the browser. Defaults to true.</summary>
    [Parameter]
    public bool Download { get; set; } = true;

    /// <summary>The link's text, in place of the asset's title.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Anything else, passed to the <c>a</c>: <c>class</c>, <c>rel</c>, <c>title</c>.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (AssetLinkMarkup.For(Asset, Download) is not { } link)
        {
            return;
        }

        builder.OpenElement(0, "a");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttribute(2, "href", link.Href);
        builder.AddAttribute(3, "download", link.Download);
        builder.AddAttribute(4, "type", link.Type);
        if (ChildContent is not null)
        {
            builder.AddContent(5, ChildContent);
        }
        else
        {
            builder.AddContent(6, link.Text);
        }
        builder.CloseElement();
    }
}
