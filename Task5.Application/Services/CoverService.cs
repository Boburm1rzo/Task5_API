using SkiaSharp;
using Task5.Application.Interfaces;
using Task5.Domain.Abstractions;

namespace Task5.Application.Services;


/// <summary>
/// Implementation of cover service using SkiaSharp for image generation
/// </summary>
public sealed class CoverService(
    ISeedCombiner seedCombiner,
    ISongDetailsService songDetailsService) : ICoverService
{
    public byte[] RenderCoverPng(string locale, ulong seed, int index)
    {
        if (index < 1)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be at least 1");

        // Get song details to render on cover
        var details = songDetailsService.GetDetails(string.Empty, locale, seed, 0, index);

        // Calculate effective seed for this specific cover
        var effectiveSeed = seedCombiner.Combine(seed, index);
        var random = new Random((int)effectiveSeed);

        // Create image
        const int width = 800;
        const int height = 800;

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        // Draw background
        DrawBackground(canvas, random, width, height);

        // Draw album title
        DrawText(canvas, details.Album == "Single" ? details.Title : details.Album,
            width / 2, height * 0.4f, 60, SKColors.White);

        // Draw artist name
        DrawText(canvas, details.Artist,
            width / 2, height * 0.6f, 40, SKColors.LightGray);

        // Encode to PNG
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        return data.ToArray();
    }

    private void DrawBackground(SKCanvas canvas, Random random, int width, int height)
    {
        // Generate random gradient colors
        var color1 = SKColor.FromHsv(
            random.Next(0, 360),
            random.Next(40, 100),
            random.Next(40, 80));

        var color2 = SKColor.FromHsv(
            random.Next(0, 360),
            random.Next(40, 100),
            random.Next(40, 80));

        // Create gradient
        using var paint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, height),
                new[] { color1, color2 },
                SKShaderTileMode.Clamp)
        };

        canvas.DrawRect(0, 0, width, height, paint);

        // Add some random geometric shapes for visual interest
        DrawRandomShapes(canvas, random, width, height);
    }

    private void DrawRandomShapes(SKCanvas canvas, Random random, int width, int height)
    {
        var shapeCount = random.Next(3, 8);

        for (int i = 0; i < shapeCount; i++)
        {
            var color = SKColor.FromHsv(
                random.Next(0, 360),
                random.Next(20, 60),
                random.Next(50, 90));

            using var paint = new SKPaint
            {
                Color = color.WithAlpha((byte)random.Next(30, 100)),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            var shapeType = random.Next(3);
            switch (shapeType)
            {
                case 0: // Circle
                    canvas.DrawCircle(
                        random.Next(0, width),
                        random.Next(0, height),
                        random.Next(50, 200),
                        paint);
                    break;

                case 1: // Rectangle
                    canvas.DrawRect(
                        random.Next(0, width),
                        random.Next(0, height),
                        random.Next(100, 300),
                        random.Next(100, 300),
                        paint);
                    break;

                case 2: // Line
                    using (var linePaint = new SKPaint
                    {
                        Color = color.WithAlpha((byte)random.Next(50, 150)),
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = random.Next(5, 20),
                        IsAntialias = true
                    })
                    {
                        canvas.DrawLine(
                            random.Next(0, width),
                            random.Next(0, height),
                            random.Next(0, width),
                            random.Next(0, height),
                            linePaint);
                    }
                    break;
            }
        }
    }

    private void DrawText(SKCanvas canvas, string text, float x, float y, float fontSize, SKColor color)
    {
        using var typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold);
        using var font = new SKFont(typeface, fontSize);

        using var paint = new SKPaint
        {
            Color = color,
            IsAntialias = true
        };

        var textWidth = paint.MeasureText(text);
        var adjustedX = x - textWidth / 2;

        // Draw shadow first
        using var shadowPaint = new SKPaint
        {
            Color = SKColors.Black.WithAlpha(150),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5)
        };

        canvas.DrawText(text, adjustedX + 2, y + 2, font, shadowPaint);

        // Draw main text
        canvas.DrawText(text, adjustedX, y, font, paint);
    }
}