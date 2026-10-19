# CohoCms.Razor

Razor components and tag helpers for Coho CMS content: images resized at the edge, and links to download assets. They build their URLs from the asset delivery embeds in an entry, with no request of their own and no credential, so they work in Blazor, in Razor Pages, and in MVC views alike. They are the .NET counterparts of [`@coho-cms/react`](https://github.com/coho-cms/coho-react).

**In testing.** Releases are alphas on NuGet.

```bash
dotnet add package CohoCms.Razor --prerelease
```

## Razor Pages and MVC

Register the tag helpers in `_ViewImports.cshtml`:

```cshtml
@addTagHelper *, CohoCms.Razor
```

Then:

```cshtml
<coho-image src="@post.Hero" width="800" />
<coho-asset-link asset="@post.Brochure" />
```

## Blazor

Add the namespaces to `_Imports.razor`:

```razor
@using CohoCms.Delivery
@using CohoCms.Razor
```

Then:

```razor
<CohoImage Src="post.Hero" Width="800" />
<CohoAssetLink Asset="post.Brochure" />
```

Both render on the server: static rendering and interactive server rendering. Blazor WebAssembly is not supported, as a Coho credential has no place in a browser either.

## Images

`src` is an asset field as delivery returns it, an `Asset?`. With `width` alone, the browser gets the image at that width and at twice it, for high-density screens. With `sizes`, it gets a range of widths to choose from:

```cshtml
<coho-image src="@post.Hero" sizes="(min-width: 960px) 50vw, 100vw" />
```

| Tag helper | Component | |
|---|---|---|
| `src` | `Src` | The asset. A missing image renders nothing |
| `width`, `height` | `Width`, `Height` | The size it is shown at. The height defaults to the width at the original's aspect ratio |
| `fit` | `Fit` | `Scale` (default) keeps the whole image; `Fill`, `Crop` and `Thumb` crop to the width and height around the asset's focal point; `Pad` letterboxes |
| `format` | `Format` | `Auto` (default) lets the edge send AVIF or WebP where the browser takes them; or `Avif`, `Webp`, `Jpg`, `Png` |
| `quality` | `Quality` | 1 to 100; the edge's default is 75 |
| `sizes`, `widths` | `Sizes`, `Widths` | A responsive image: the widths to offer default to the edge's from 640, up to the original |
| `alt` | `Alt` | Overrides the asset's alt text; `alt=""` for a purely decorative image |
| anything else | anything else | Passed to the `<img>`: `loading="eager"` for an image above the fold, `class` |

A tag helper's `fit` and `format` take a member's name, such as `fit="Fill"`. A component's take the value, such as `Fit="ImageFit.Fill"`.

The `<img>` always has `width` and `height`, so the page keeps its space while the image loads, and it loads lazily unless told otherwise. A file that is not a raster image, such as an SVG, is shown as it was uploaded.

## Downloads

```cshtml
<coho-asset-link asset="@post.Brochure" />
<coho-asset-link asset="@post.Brochure" download="false">Read the guide</coho-asset-link>
```

```razor
<CohoAssetLink Asset="post.Brochure" />
<CohoAssetLink Asset="post.Brochure" Download="false">Read the guide</CohoAssetLink>
```

The link's text is its content, or the asset's title, or its filename. It downloads the file under its own name; `download="false"` opens it in the browser instead. It carries the file's media type. Other attributes, such as `class`, go to the `<a>`.

⚠️ Downloading needs the asset CDN to send the file as an attachment, which it does not do yet: until it does, the link opens the file. The link needs no change when it starts to.

## Building your own

The components are thin. The URLs come from `Image()`, `Srcset()` and `Download()` on `Asset`, in [`CohoCms.Delivery`](https://github.com/coho-cms/coho-delivery-sdk-dotnet/tree/HEAD/src/CohoCms.Delivery), which take the same options, for a CSS background, a social card or anything else.

Assets are public to anyone holding their URL, drafts included, and deleting an asset does not stop its URL working.

## Development

The .NET 10 SDK. The package targets `net8.0` and `net10.0`.

```bash
dotnet test
dotnet format
dotnet pack -c Release -o artifacts
```

`dotnet format --verify-no-changes` is what CI checks: Allman braces, an opening brace on its own line, and four-space indents.

## Publishing

Run the **Publish** workflow from a version branch. It tests, packs and publishes the version in `Directory.Build.props`, which cannot be published twice. Keep the version in step with `CohoCms.Delivery`, and the minimum in `Directory.Packages.props` on the delivery version whose asset URLs it uses.

The workflow signs in to nuget.org with Trusted Publishing: a policy on nuget.org, held by the `CohoCMS` organisation, trusts `publish.yml` in this repository, and the workflow needs only `NUGET_USER`, the nuget.org user who created the policy. No API key is stored.

## License

Apache-2.0
