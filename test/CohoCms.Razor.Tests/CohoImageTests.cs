using CohoCms.Delivery;
using Microsoft.AspNetCore.Razor.TagHelpers;
using static CohoCms.Razor.Tests.Assets;

namespace CohoCms.Razor.Tests;

/// <summary>
/// <see cref="CohoImage"/> and <see cref="CohoImageTagHelper"/>, which render the same <c>img</c>,
/// so each case runs through both. The cases are coho-react's <c>CohoImage</c>'s.
/// </summary>
public sealed class CohoImageTests
{
    public static TheoryData<string> Renderers => new("component", "tag helper");

    /// <summary>An image with these parameters, rendered by <paramref name="renderer"/>; <paramref name="extra"/> are other attributes.</summary>
    private static Task<string> Image(string renderer, Action<CohoImage, CohoImageTagHelper> set, Dictionary<string, object>? extra = null)
    {
        var component = new CohoImage();
        var helper = new CohoImageTagHelper();
        set(component, helper);
        if (renderer == "tag helper")
        {
            var unbound = new TagHelperAttributeList();
            foreach (var (name, value) in extra ?? [])
            {
                unbound.Add(name, value);
            }
            return Render.TagHelper(helper, "coho-image", unbound);
        }
        var parameters = new Dictionary<string, object?>
        {
            [nameof(CohoImage.Src)] = component.Src,
            [nameof(CohoImage.Width)] = component.Width,
            [nameof(CohoImage.Height)] = component.Height,
            [nameof(CohoImage.Fit)] = component.Fit,
            [nameof(CohoImage.Format)] = component.Format,
            [nameof(CohoImage.Sizes)] = component.Sizes,
            [nameof(CohoImage.Widths)] = component.Widths,
            [nameof(CohoImage.Alt)] = component.Alt,
        };
        foreach (var (name, value) in extra ?? [])
        {
            parameters[name] = value;
        }
        return Render.Component<CohoImage>(parameters);
    }

    [Theory]
    [MemberData(nameof(Renderers))]
    public async Task An_image_at_a_width_gets_it_at_1x_and_2x_keeping_its_aspect_ratio(string renderer)
    {
        var img = Render.Attributes(await Image(renderer, (c, t) => (c.Src, c.Width, t.Src, t.Width) = (Photo, 400, Photo, 400)));

        Assert.Equal($"{Base}/i/abc/culvert.png?w=400&fm=auto", img["src"]);
        Assert.Equal($"{Base}/i/abc/culvert.png?w=400&fm=auto 1x, {Base}/i/abc/culvert.png?w=800&fm=auto 2x", img["srcset"]);
        Assert.Equal("400", img["width"]);
        Assert.Equal("200", img["height"]);
        Assert.Equal("A culvert under Elm Street", img["alt"]);
        Assert.Equal("lazy", img["loading"]);
        Assert.Equal("async", img["decoding"]);
    }

    [Theory]
    [MemberData(nameof(Renderers))]
    public async Task With_sizes_an_image_offers_a_range_of_widths_up_to_the_original(string renderer)
    {
        const string Sizes = "(min-width: 960px) 50vw, 100vw";
        int[] widths = [640, 1200, 3840];
        var img = Render.Attributes(await Image(renderer, (c, t) => (c.Src, c.Sizes, c.Widths, t.Src, t.Sizes, t.Widths) = (Photo, Sizes, widths, Photo, Sizes, widths)));

        Assert.Equal(Sizes, img["sizes"]);
        Assert.Equal($"{Base}/i/abc/culvert.png?w=640&fm=auto 640w, {Base}/i/abc/culvert.png?w=1200&fm=auto 1200w", img["srcset"]);
        Assert.Equal("2000", img["width"]);
    }

    [Theory]
    [MemberData(nameof(Renderers))]
    public async Task A_fill_crops_to_the_shape_given_around_the_focal_point(string renderer)
    {
        var img = Render.Attributes(await Image(renderer, (c, t) =>
        {
            (c.Src, c.Width, c.Height, c.Fit, c.Format) = (Photo, 300, 300, ImageFit.Fill, ImageFormat.Webp);
            (t.Src, t.Width, t.Height, t.Fit, t.Format) = (Photo, 300, 300, ImageFit.Fill, ImageFormat.Webp);
        }));

        Assert.Equal($"{Base}/i/abc/culvert.png?w=300&h=300&fit=fill&fm=webp&fx=0.25&fy=0.75", img["src"]);
        Assert.EndsWith("w=600&h=600&fit=fill&fm=webp&fx=0.25&fy=0.75 2x", img["srcset"], StringComparison.Ordinal);
        Assert.Equal("300", img["height"]);
    }

    [Theory]
    [MemberData(nameof(Renderers))]
    public async Task The_page_can_override_alt_loading_and_any_other_attribute(string renderer)
    {
        var img = Render.Attributes(await Image(
            renderer,
            (c, t) => (c.Src, c.Width, c.Alt, t.Src, t.Width, t.Alt) = (Photo, 400, "", Photo, 400, ""),
            new() { ["loading"] = "eager", ["class"] = "hero" }));

        Assert.Equal("", img["alt"]);
        Assert.Equal("eager", img["loading"]);
        Assert.Equal("hero", img["class"]);
    }

    [Theory]
    [MemberData(nameof(Renderers))]
    public async Task An_image_that_is_not_raster_is_shown_as_uploaded_with_no_srcset(string renderer)
    {
        var img = Render.Attributes(await Image(renderer, (c, t) => (c.Src, c.Width, t.Src, t.Width) = (Svg, 120, Svg, 120)));

        Assert.Equal(Svg.Url, img["src"]);
        Assert.False(img.ContainsKey("srcset"));
        Assert.Equal("", img["alt"]);
    }

    [Theory]
    [MemberData(nameof(Renderers))]
    public async Task A_missing_image_renders_nothing(string renderer)
    {
        Assert.Equal("", await Image(renderer, (c, t) => (c.Src, t.Src) = (null, null)));
    }

    [Fact]
    public async Task The_tag_helper_renders_an_img_with_no_end_tag()
    {
        var html = await Render.TagHelper(new CohoImageTagHelper { Src = Photo, Width = 400 }, "coho-image");

        Assert.StartsWith("<img loading=\"lazy\" decoding=\"async\" alt=", html, StringComparison.Ordinal);
        Assert.EndsWith("height=\"200\">", html, StringComparison.Ordinal);
    }
}
