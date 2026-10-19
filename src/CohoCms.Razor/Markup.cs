using CohoCms.Delivery;

namespace CohoCms.Razor;

/// <summary>
/// An image's <c>img</c> attributes, built from its asset: what <see cref="CohoImage"/> and
/// <see cref="CohoImageTagHelper"/> both render, so the two never disagree.
/// </summary>
internal sealed record ImageMarkup(string Src, string? Srcset, string? Sizes, int? Width, int? Height, string Alt)
{
    /// <summary>
    /// The attributes for <paramref name="asset"/>, or null to render nothing, for a missing
    /// image. The URLs come from the asset alone: no request, and no credential.
    /// </summary>
    /// <exception cref="InvalidOperationException">The asset has no URL: the deployment serving it has no asset CDN.</exception>
    public static ImageMarkup? For(
        Asset? asset,
        int? width,
        int? height,
        ImageFit fit,
        ImageFormat format,
        int? quality,
        string? background,
        string? sizes,
        IEnumerable<int>? widths,
        string? alt)
    {
        if (asset is null)
        {
            return null;
        }

        var natural = asset.File;
        var shownWidth = width ?? natural.Width;
        // Shown at the original's aspect ratio unless a crop sets the shape.
        var keepsRatio = fit is ImageFit.Scale or ImageFit.Pad;
        var shownHeight = height
            ?? (keepsRatio && shownWidth is { } shown && natural is { Width: > 0 and var originalWidth, Height: { } originalHeight }
                ? (int)Math.Round((double)shown * originalHeight / originalWidth, MidpointRounding.AwayFromZero)
                : null);

        var options = new ImageOptions
        {
            Width = shownWidth,
            Height = height,
            // Scale is the edge's own default, so it is left out of the URL.
            Fit = fit == ImageFit.Scale ? null : fit,
            Format = format,
            Quality = quality,
            Background = background,
        };
        var srcset = !asset.IsImage
            ? null
            : sizes is not null
                ? asset.Srcset(options, widths)
                : shownWidth is not null
                    ? asset.Srcset(options, densities: [1, 2])
                    : null;

        return new ImageMarkup(asset.Image(options), srcset, sizes, shownWidth, shownHeight, alt ?? asset.Alt ?? "");
    }
}

/// <summary>
/// A link's <c>a</c> attributes and text, built from its asset: what <see cref="CohoAssetLink"/>
/// and <see cref="CohoAssetLinkTagHelper"/> both render.
/// </summary>
/// <param name="Href">The file, to download or to open.</param>
/// <param name="Download">The <c>download</c> attribute's value: the filename, empty for none, or null to leave it out.</param>
/// <param name="Type">The file's media type, for the <c>type</c> attribute.</param>
/// <param name="Text">The link's text when the page gives none: the asset's title, or else its filename.</param>
internal sealed record AssetLinkMarkup(string Href, string? Download, string? Type, string Text)
{
    /// <summary>The attributes for <paramref name="asset"/>, or null to render nothing, for a missing asset or one with no URL.</summary>
    public static AssetLinkMarkup? For(Asset? asset, bool download)
    {
        if (asset?.Url is not { } url)
        {
            return null;
        }
        var filename = asset.File.Filename;
        return new AssetLinkMarkup(
            download ? asset.Download() : url,
            // Honoured only on the site's own origin; the edge's dl=1 covers the rest.
            download ? filename ?? "" : null,
            asset.File.MediaType,
            asset.Title ?? filename ?? "Download");
    }
}
