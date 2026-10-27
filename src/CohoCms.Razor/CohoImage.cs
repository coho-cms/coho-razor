using System.Globalization;
using CohoCms.Delivery;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CohoCms.Razor;

/// <summary>
/// An image from Coho, resized at the edge: <c>&lt;CohoImage Src="post.Hero" Width="800" /&gt;</c>.
/// </summary>
/// <remarks>
/// With <see cref="Width"/> alone, the browser gets the image at that width and at twice it,
/// for high-density screens. With <see cref="Sizes"/>, it gets a range of widths to choose
/// from. Either way the <c>img</c> carries <c>width</c> and <c>height</c>, so the page keeps its
/// space while the image loads, and the asset's alt text unless <see cref="Alt"/> is given. It
/// loads lazily unless told otherwise: pass <c>loading="eager"</c> for an image above the fold.
/// A file that is not a raster image, such as an SVG, is shown as it was uploaded. It needs no
/// request and no credential of its own.
/// </remarks>
public sealed class CohoImage : ComponentBase
{
    /// <summary>The asset to show: an entry's image field. Nothing is rendered for a missing image.</summary>
    [Parameter]
    [EditorRequired]
    public Asset? Src { get; set; }

    /// <summary>The width the image is shown at, in CSS pixels. Defaults to the original's.</summary>
    [Parameter]
    public int? Width { get; set; }

    /// <summary>
    /// The height it is shown at. Defaults to the width at the original's aspect ratio; give it
    /// with <see cref="ImageFit.Fill"/> to crop to a shape around the focal point.
    /// </summary>
    [Parameter]
    public int? Height { get; set; }

    /// <summary>
    /// <see cref="ImageFit.Scale"/>, the default, keeps the whole image; <see cref="ImageFit.Fill"/>,
    /// <see cref="ImageFit.Crop"/> and <see cref="ImageFit.Thumb"/> crop to <see cref="Width"/> by
    /// <see cref="Height"/> around the asset's focal point; <see cref="ImageFit.Pad"/> letterboxes.
    /// </summary>
    [Parameter]
    public ImageFit Fit { get; set; } = ImageFit.Scale;

    /// <summary><see cref="ImageFormat.Auto"/>, the default, lets the edge send AVIF or WebP where the browser takes them.</summary>
    [Parameter]
    public ImageFormat Format { get; set; } = ImageFormat.Auto;

    /// <summary>1 to 100; the edge's default is 75.</summary>
    [Parameter]
    public int? Quality { get; set; }

    /// <summary>Ignored. The background is a setting on the transform, not a request parameter.</summary>
    // ⚠️ Kept, ignored, and marked obsolete rather than deleted — which is the opposite
    // of what the delivery SDK did with its own option, for a reason specific to here.
    // This member is *bound*, so the framework consumes the attribute. Delete it and
    // `background="…"` on the tag becomes an unbound attribute that passes straight
    // through to the rendered `img`, which is a worse outcome than ignoring it. Obsolete
    // says so at compile time, which is a louder signal than a type could give.
    [Obsolete("The asset edge no longer accepts bg. The colour a Pad is filled with, and a jpg flattened onto, is a setting on the transform. This value is ignored, and the member will be removed.")]
    [Parameter]
    public string? Background { get; set; }

    /// <summary>How wide the image is shown, as the <c>sizes</c> attribute, such as <c>(min-width: 960px) 50vw, 100vw</c>.</summary>
    [Parameter]
    public string? Sizes { get; set; }

    /// <summary>The widths to offer with <see cref="Sizes"/>. Defaults to the edge's widths from 640, up to the original.</summary>
    [Parameter]
    public IEnumerable<int>? Widths { get; set; }

    /// <summary>Overrides the asset's alt text. <c>""</c> for a purely decorative image.</summary>
    [Parameter]
    public string? Alt { get; set; }

    /// <summary>Anything else, passed to the <c>img</c>: <c>class</c>, <c>loading</c>, <c>style</c>.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (ImageMarkup.For(Src, Width, Height, Fit, Format, Quality, Sizes, Widths, Alt) is not { } image)
        {
            return;
        }

        // The page's attributes win over the defaults before them, and lose to what is built from the asset after them.
        builder.OpenElement(0, "img");
        builder.AddAttribute(1, "loading", "lazy");
        builder.AddAttribute(2, "decoding", "async");
        builder.AddMultipleAttributes(3, AdditionalAttributes);
        builder.AddAttribute(4, "alt", image.Alt);
        builder.AddAttribute(5, "src", image.Src);
        builder.AddAttribute(6, "srcset", image.Srcset);
        builder.AddAttribute(7, "sizes", image.Sizes);
        builder.AddAttribute(8, "width", image.Width?.ToString(CultureInfo.InvariantCulture));
        builder.AddAttribute(9, "height", image.Height?.ToString(CultureInfo.InvariantCulture));
        builder.CloseElement();
    }
}
