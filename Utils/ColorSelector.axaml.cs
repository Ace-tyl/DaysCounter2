using System;
using Avalonia.Controls;
using Avalonia.Media;

namespace DaysCounter2;

public partial class ColorSelector : Window
{
    public Color? selectedColor;

    public ColorSelector(Color initialColor)
    {
        InitializeComponent();
        ColorPicker.Color = initialColor;
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        selectedColor = ColorPicker.Color;
        Close();
    }

    public static Color Interpolate(Color c1, Color c2, double p)
    {
        p = Math.Clamp(p, 0, 1);
        double a1 = c1.A, r1 = c1.R, g1 = c1.G, b1 = c1.B;
        double a2 = c2.A, r2 = c2.R, g2 = c2.G, b2 = c2.B;
        double rgb_sense;
        if (a1 < 0.01)
        {
            rgb_sense = 1;
        }
        else if (a2 < 0.01)
        {
            rgb_sense = 0;
        }
        else
        {
            rgb_sense = Math.Pow(p, a1 / a2);
        }
        double a3 = a1 + (a2 - a1) * p;
        double r3 = r1 + (r2 - r1) * rgb_sense;
        double g3 = g1 + (g2 - g1) * rgb_sense;
        double b3 = b1 + (b2 - b1) * rgb_sense;
        return Color.FromArgb((byte)Math.Round(a3), (byte)Math.Round(r3), (byte)Math.Round(g3), (byte)Math.Round(b3));
    }

    // Overlay c1 on top of c2
    public static Color Overlay(Color c1, Color c2)
    {
        Color c1_solid = new Color(255, c1.R, c1.G, c1.B);
        return Interpolate(c2, c1_solid, c1.A / 255.0);
    }
}