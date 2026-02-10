using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Task5.Application.Interfaces;
using Task5.Domain.ValueObjects;
using Task5.Infrastructure.Generation.Seeds;

namespace Task5.Infrastructure.Generation.Covers;

internal sealed class CoverRenderer : ICoverRenderer
{
    public byte[] RenderPng(Seed64 seed, LocaleCode locale, string title, string artist)
    {
        const int size = 512;

        var rng = new DeterministicRng(seed.Value);

        using var img = new Image<Rgba32>(size, size);

        var bg1 = RandomColor(rng);
        var bg2 = RandomColor(rng);

        img.Mutate(ctx =>
        {
            ctx.Fill(new LinearGradientBrush(
              new PointF(0, 0),
              new PointF(size, size),
              GradientRepetitionMode.None,
              new ColorStop(0f, bg1),
              new ColorStop(1f, bg2)
          ));

            DrawPattern(ctx, rng, size);

            ctx.Fill(new Rgba32(0, 0, 0, 110));
        });

        DrawText(img, title, artist, rng);

        using var ms = new MemoryStream();
        img.Save(ms, new PngEncoder());
        return ms.ToArray();
    }

    private static Color RandomColor(DeterministicRng rng)
    {
        byte r = (byte)rng.NextInt(30, 226);
        byte g = (byte)rng.NextInt(30, 226);
        byte b = (byte)rng.NextInt(30, 226);
        return Color.FromRgb(r, g, b);
    }

    private static void DrawPattern(IImageProcessingContext ctx, DeterministicRng rng, int size)
    {
        int mode = rng.NextInt(0, 3);

        if (mode == 0)
        {
            int count = rng.NextInt(8, 18);
            for (int i = 0; i < count; i++)
            {
                float radius = rng.NextInt(20, 120);
                float x = rng.NextInt(-50, size + 50);
                float y = rng.NextInt(-50, size + 50);

                var col = RandomColor(rng).WithAlpha((float)(0.12 + rng.Next01() * 0.18));
                ctx.Fill(col, new EllipsePolygon(x, y, radius));
            }
        }
        else if (mode == 1)
        {
            int count = rng.NextInt(10, 22);
            for (int i = 0; i < count; i++)
            {
                float thickness = (float)(1 + rng.Next01() * 6);
                var col = RandomColor(rng).WithAlpha((float)(0.10 + rng.Next01() * 0.15));

                float y = rng.NextInt(-size, size);
                ctx.DrawLine(col, thickness,
                    new PointF(0, y),
                    new PointF(size, y + size));
            }
        }
        else
        {
            int count = rng.NextInt(10, 20);
            for (int i = 0; i < count; i++)
            {
                float w = rng.NextInt(40, 220);
                float h = rng.NextInt(20, 160);
                float x = rng.NextInt(-30, size - 10);
                float y = rng.NextInt(-30, size - 10);

                var col = RandomColor(rng).WithAlpha((float)(0.10 + rng.Next01() * 0.18));
                ctx.Fill(col, new RectangleF(x, y, w, h));
            }
        }
    }

    private static void DrawText(Image<Rgba32> img, string title, string artist, DeterministicRng rng)
    {
        if (!SystemFonts.Collection.Families.Any())
            throw new Exception("No system fonts found. Add a .ttf font file manually.");

        FontFamily family = SystemFonts.Collection.Families.First();

        var titleFont = family.CreateFont(48, FontStyle.Bold);
        var artistFont = family.CreateFont(28, FontStyle.Regular);

        string safeTitle = Clean(title);
        string safeArtist = Clean(artist);

        img.Mutate(ctx =>
        {
            var titleOptions = new RichTextOptions(titleFont)
            {
                Origin = new PointF(40, 60),
                WrappingLength = img.Width - 80
            };

            ctx.DrawText(titleOptions, safeTitle, Color.White);

            var artistOptions = new RichTextOptions(artistFont)
            {
                Origin = new PointF(40, img.Height - 90),
                WrappingLength = img.Width - 80
            };

            ctx.DrawText(artistOptions, safeArtist, Color.White);
        });
    }


    private static string Clean(string s)
    {
        s = (s ?? "").Trim();
        if (s.Length == 0) return "—";
        if (s.Length > 64) s = s[..64].TrimEnd() + "…";
        return s;
    }
}
