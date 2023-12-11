using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class UtilsColor
{

    public static int FindMostSimilarColorIndex(Color targetColor, List<Color> colorList)
    {
        if (colorList == null || colorList.Count == 0)
        {
            Debug.LogWarning("Color list is empty. Returning -1.");
            return -1; // Indicate that no valid index was found.
        }

        int closestColorIndex = 0;
        double smallestDistance = double.MaxValue;

        for (int i = 0; i < colorList.Count; i++)
        {
            double distance = ColorDistance2(targetColor, colorList[i]);
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                closestColorIndex = i;
            }
        }

        return closestColorIndex;
    }


    private static double ColorDistance(Color color1, Color color2)
    {
        // Here, replace the old distance calculation with CIEDE2000
        return CIEDE2000(color1, color2);
    }
    public static double CIEDE2000(Color color1, Color color2)
    {
        // Convert Unity Colors to CIELAB
        double[] lab1 = Rgb2Lab(color1);
        double[] lab2 = Rgb2Lab(color2);

        // Assign the LAB values
        double L1 = lab1[0], a1 = lab1[1], b1 = lab1[2];
        double L2 = lab2[0], a2 = lab2[1], b2 = lab2[2];

        double L_ave = (L1 + L2) / 2.0;
        double C1 = System.Math.Sqrt(a1 * a1 + b1 * b1);
        double C2 = System.Math.Sqrt(a2 * a2 + b2 * b2);
        double C_ave = (C1 + C2) / 2.0;

        double G = 0.5 * (1 - System.Math.Sqrt(System.Math.Pow(C_ave, 7) / (System.Math.Pow(C_ave, 7) + System.Math.Pow(25, 7))));
        double a1_prime = a1 * (1 + G);
        double a2_prime = a2 * (1 + G);

        double C1_prime = System.Math.Sqrt(a1_prime * a1_prime + b1 * b1);
        double C2_prime = System.Math.Sqrt(a2_prime * a2_prime + b2 * b2);
        double C_ave_prime = (C1_prime + C2_prime) / 2.0;

        double h1_prime = ComputeHueAngle(a1_prime, b1);
        double h2_prime = ComputeHueAngle(a2_prime, b2);

        double H_ave_prime = System.Math.Abs(h1_prime - h2_prime) > 180 ? (h1_prime + h2_prime + 360) / 2 : (h1_prime + h2_prime) / 2;
        double T = 1 - 0.17 * System.Math.Cos(H_ave_prime - 30) + 0.24 * System.Math.Cos((2 * H_ave_prime)) + 0.32 * System.Math.Cos((3 * H_ave_prime + 6)) - 0.20 * System.Math.Cos(4 * H_ave_prime - 63);

        double deltaH_prime = h2_prime - h1_prime;
        if (System.Math.Abs(deltaH_prime) > 180)
        {
            if (h2_prime <= h1_prime)
            {
                deltaH_prime += 360;
            }
            else
            {
                deltaH_prime -= 360;
            }
        }
        double deltaH_prime_2 = 2 * System.Math.Sqrt(C1_prime * C2_prime) * System.Math.Sin((Mathf.Deg2Rad * deltaH_prime / 2));

        double SL = 1 + (0.015 * System.Math.Pow((L_ave - 50), 2)) / System.Math.Sqrt(20 + System.Math.Pow(L_ave - 50, 2));
        double SC = 1 + 0.045 * C_ave_prime;
        double SH = 1 + 0.015 * C_ave_prime * T;

        double deltaTheta = 30 * System.Math.Exp(-System.Math.Pow(H_ave_prime - 275 / 25, 2));
        double RC = 2 * System.Math.Sqrt(System.Math.Pow(C_ave_prime, 7) / (System.Math.Pow(C_ave_prime, 7) + System.Math.Pow(25.0, 7)));
        double RT = -System.Math.Sin(2 * System.Math.PI / 180 * deltaTheta) * RC;

        double KL = 1, KC = 1, KH = 1;

        double deltaL_prime = L2 - L1;
        double deltaC_prime = C2_prime - C1_prime;

        double deltaE = System.Math.Sqrt(
            System.Math.Pow(deltaL_prime / (KL * SL), 2) +
            System.Math.Pow(deltaC_prime / (KC * SC), 2) +
            System.Math.Pow(deltaH_prime_2 / (KH * SH), 2) +
            RT * deltaC_prime / (KC * SC) * deltaH_prime_2 / (KH * SH));

        return deltaE;
    }

    private static double ComputeHueAngle(double a, double b)
    {
        if (a == 0 && b == 0) return 0;

        double angle = System.Math.Atan2(b, a) * Mathf.Rad2Deg;
        return angle < 0 ? angle + 360 : angle;
    }

    private static float ColorDistance2(Color color1, Color color2)
    {
        //// convert to color32
        Color32 c1 = IncreaseIntensityToMax(color1);
        Color32 c2 = color2;

        // Euclidean distance in RGB color space
        float r = c1.r - c2.r;
        float g = c1.g - c2.g;
        float b = c1.b - c2.b;
        return r * r + g * g + b * b;
    }

    public static Color IncreaseIntensityToMax(Color color)
    {
        float maxComponent = Mathf.Max(color.r, color.g, color.b);

        // If the maximum component is already 1 or color is black, no adjustment needed
        if (maxComponent == 1 || maxComponent == 0)
        {
            return color;
        }

        // Scale other components based on the ratio to the max component
        return new Color(color.r / maxComponent, color.g / maxComponent, color.b / maxComponent);
    }

    //public static int GetDistance(Color current, Color match)
    //{
    //    int redDifference;
    //    int greenDifference;
    //    int blueDifference;

    //    redDifference = current.R - match.R;
    //    greenDifference = current.G - match.G;
    //    blueDifference = current.B - match.B;

    //    return redDifference * redDifference + greenDifference * greenDifference + blueDifference * blueDifference;
    //}


    public static double[] Rgb2Lab(Color color)
    {
        // Normalize RGB values to the range 0-1
        double r = color.r;
        double g = color.g;
        double b = color.b;

        // Apply sRGB transformation
        r = (r > 0.04045) ? System.Math.Pow((r + 0.055) / 1.055, 2.4) : r / 12.92;
        g = (g > 0.04045) ? System.Math.Pow((g + 0.055) / 1.055, 2.4) : g / 12.92;
        b = (b > 0.04045) ? System.Math.Pow((b + 0.055) / 1.055, 2.4) : b / 12.92;

        // Convert to XYZ color space
        double x = (r * 0.4124 + g * 0.3576 + b * 0.1805) / 0.95047;
        double y = (r * 0.2126 + g * 0.7152 + b * 0.0722) / 1.00000;
        double z = (r * 0.0193 + g * 0.1192 + b * 0.9505) / 1.08883;

        // Convert to Lab color space
        x = (x > 0.008856) ? System.Math.Pow(x, 1.0 / 3.0) : (7.787 * x) + 16.0 / 116.0;
        y = (y > 0.008856) ? System.Math.Pow(y, 1.0 / 3.0) : (7.787 * y) + 16.0 / 116.0;
        z = (z > 0.008856) ? System.Math.Pow(z, 1.0 / 3.0) : (7.787 * z) + 16.0 / 116.0;

        // Return the LAB values as an array
        return new double[] { (116 * y) - 16, 500 * (x - y), 200 * (y - z) };
    }

    //public static int FindMostSimilarColorIndex(Color baseColor, List<Color> colors)
    //{
    //    int mostSimilarColorIndex = -1; // Initialize with a value indicating no match
    //    float smallestDifference = float.MaxValue; // Initialize with a large value

    //    float baseHue, baseSat, baseVal;
    //    Color.RGBToHSV(baseColor, out baseHue, out baseSat, out baseVal);

    //    for (int i = 0; i < colors.Count; i++)
    //    {
    //        float colorHue, colorSat, colorVal;
    //        Color.RGBToHSV(colors[i], out colorHue, out colorSat, out colorVal);

    //        float hueDifference = Mathf.Abs(baseHue - colorHue);
    //        // Considering hue's circular nature
    //        if (hueDifference > 0.5f) hueDifference = 1f - hueDifference;

    //        if (hueDifference < smallestDifference)
    //        {
    //            smallestDifference = hueDifference;
    //            mostSimilarColorIndex = i;
    //        }
    //    }

    //    return mostSimilarColorIndex;
    //}

    public static bool AreColorsSimilar(Color baseColor, Color colorToCheck, float hueThreshold = 0.1f, float saturationThreshold = 0.2f, float lightnessThreshold = 0.2f)
    {
        // Convert colors to HSL
        Color.RGBToHSV(baseColor, out float baseH, out float baseS, out float baseV);
        Color.RGBToHSV(colorToCheck, out float checkH, out float checkS, out float checkV);

        // Check if the HSL values are within the given thresholds
        bool hueClose = Mathf.Abs(baseH - checkH) < hueThreshold;
        bool saturationClose = Mathf.Abs(baseS - checkS) < saturationThreshold;
        bool lightnessClose = Mathf.Abs(baseV - checkV) < lightnessThreshold;

        return hueClose && saturationClose && lightnessClose;
    }

    public static bool IsColorCloseToBaseColor(Color baseColor, Color colorToCheck, float hueThreshold = 0.2f)
    {
        // Convert the base color and the color to check to HSV
        Color.RGBToHSV(baseColor, out float baseHue, out _, out _);
        Color.RGBToHSV(colorToCheck, out float checkHue, out _, out _);

        // Check if the hue of the colorToCheck is close to the hue of the baseColor
        return Mathf.Abs(checkHue - baseHue) < hueThreshold;
    }


    private static float ColorDifference(Color color1, Color color2)
    {
        float rDiff = color1.r - color2.r;
        float gDiff = color1.g - color2.g;
        float bDiff = color1.b - color2.b;

        return Mathf.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
    }

    public static bool AreColorsSimilarEuclideanDistance(Color baseColor, Color colorToCheck, float threshold = 0.75f)
    {
        float difference = ColorDifference(baseColor, colorToCheck);
        return difference < threshold;
    }

}
