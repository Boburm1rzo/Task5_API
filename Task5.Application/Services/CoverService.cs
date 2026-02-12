using Microsoft.Extensions.Caching.Memory;
using SkiaSharp;
using Task5.Application.Interfaces;
using Task5.Domain.Abstractions;

namespace Task5.Application.Services;

public sealed class CoverService(
    IMemoryCache cache,
    ISeedCombiner seedCombiner) : ICoverService
{
    public byte[] RenderCoverPng(string locale, ulong seed, int index, int size = 256)
    {
        if (index < 1)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be at least 1");

        size = Math.Clamp(size, 128, 800);

        var cacheKey = $"cover:{locale}:{seed}:{index}:{size}";
        if (cache.TryGetValue(cacheKey, out byte[]? cached) && cached is not null)
            return cached;

        var effectiveSeed = seedCombiner.Combine(seed, index);
        var random = new Random(unchecked((int)effectiveSeed));

        var width = size;
        var height = size;

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        DrawBackground(canvas, random, width, height);

        if (size >= 512)
        {
            var title = $"Track {index}";
            var artist = "Unknown Artist";

            DrawText(canvas, title, width / 2f, height * 0.42f, width * 0.07f, SKColors.White);
            DrawText(canvas, artist, width / 2f, height * 0.60f, width * 0.05f, SKColors.LightGray);
        }

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 85);
        var bytes = data.ToArray();

        cache.Set(cacheKey, bytes, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(20),
            Size = bytes.Length
        });

        return bytes;
    }

    private void DrawBackground(SKCanvas canvas, Random random, int width, int height)
    {
        var color1 = SKColor.FromHsv(random.Next(0, 360), random.Next(40, 100), random.Next(40, 80));
        var color2 = SKColor.FromHsv(random.Next(0, 360), random.Next(40, 100), random.Next(40, 80));

        using var paint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, height),
                new[] { color1, color2 },
                SKShaderTileMode.Clamp)
        };

        canvas.DrawRect(0, 0, width, height, paint);
        DrawRandomShapes(canvas, random, width, height);
    }

    private void DrawRandomShapes(SKCanvas canvas, Random random, int width, int height)
    {
        var shapeCount = random.Next(3, 7);
        for (int i = 0; i < shapeCount; i++)
        {
            var color = SKColor.FromHsv(random.Next(0, 360), random.Next(20, 60), random.Next(50, 90));

            using var paint = new SKPaint
            {
                Color = color.WithAlpha((byte)random.Next(35, 110)),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            switch (random.Next(3))
            {
                case 0:
                    canvas.DrawCircle(random.Next(0, width), random.Next(0, height), random.Next(width / 14, width / 4), paint);
                    break;
                case 1:
                    canvas.DrawRect(random.Next(0, width), random.Next(0, height), random.Next(width / 6, width / 2), random.Next(width / 6, width / 2), paint);
                    break;
                case 2:
                    using (var linePaint = new SKPaint
                    {
                        Color = color.WithAlpha((byte)random.Next(60, 160)),
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = random.Next(2, Math.Max(3, width / 60)),
                        IsAntialias = true
                    })
                    {
                        canvas.DrawLine(random.Next(0, width), random.Next(0, height), random.Next(0, width), random.Next(0, height), linePaint);
                    }
                    break;
            }
        }
    }

    private void DrawText(SKCanvas canvas, string text, float x, float y, float fontSize, SKColor color)
    {
        using var typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold);
        using var font = new SKFont(typeface, fontSize);

        using var paint = new SKPaint { Color = color, IsAntialias = true };
        var textWidth = paint.MeasureText(text);
        var adjustedX = x - textWidth / 2;

        using var shadowPaint = new SKPaint
        {
            Color = SKColors.Black.WithAlpha(130),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4)
        };

        canvas.DrawText(text, adjustedX + 2, y + 2, font, shadowPaint);
        canvas.DrawText(text, adjustedX, y, font, paint);
    }
}
