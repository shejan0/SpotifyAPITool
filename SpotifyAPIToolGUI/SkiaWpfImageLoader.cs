using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;

public static class SkiaWpfImageLoader
{
    public static ImageSource FromBytes(byte[] data)
    {
        if (data == null || data.Length == 0)
            return null;

        // Decode using SkiaSharp (very tolerant of malformed JPEGs)
        using var bitmap = SKBitmap.Decode(data);
        if (bitmap == null)
            return null;

        return ToBitmapSource(bitmap);
    }

    private static BitmapSource ToBitmapSource(SKBitmap bitmap)
    {
        // Convert SKBitmap → SKImage → SKPixmap (safe managed access)
        using var image = SKImage.FromBitmap(bitmap);
        using var pixmap = image.PeekPixels();

        // Copy pixel data into a managed byte[] safely
        var bytes = pixmap.GetPixelSpan().ToArray();

        // Create a WPF WriteableBitmap
        var wb = new WriteableBitmap(
            pixmap.Width,
            pixmap.Height,
            96,
            96,
            PixelFormats.Bgra32,
            null);

        // Write the managed byte[] into the WriteableBitmap
        wb.WritePixels(
            new Int32Rect(0, 0, pixmap.Width, pixmap.Height),
            bytes,
            pixmap.RowBytes,
            0);

        wb.Freeze();
        return wb;
    }
}
