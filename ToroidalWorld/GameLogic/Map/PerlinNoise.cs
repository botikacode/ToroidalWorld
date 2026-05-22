using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToroidalWorld.GameLogic.Map;

public class PerlinNoise
{
    private int[] _p;
    private int _perlinSize;

    static readonly int[,] GRAD2 = {
    { 1, 1 }, { -1, 1 }, { 1, -1 }, { -1, -1 },
    { 1, 0 }, { -1, 0 }, { 1, 0 }, { -1, 0 },
    { 0, 1 }, { 0, -1 }, { 0, 1 }, { 0, -1 },
    { 1, 1 }, { -1, 1 }, { 1, -1 }, { -1, -1 }
};


    /// <summary>
    /// Initializes a new instance of the class <see cref="PerlinNoise"/>.
    /// </summary>
    /// <param name="seed">Seed</param>
    /// <param name="perlinSize">Size for perlinNoise, this value must be multiple of 2</param>
    public PerlinNoise(int seed, int perlinSize)
    {
        _perlinSize = perlinSize;
        var random = new Random(seed);
        int[] permutation = new int[_perlinSize];
        _p = new int[_perlinSize * 2];

        for (int i = 0; i < _perlinSize; i++)
        {
            permutation[i] = i;
        }

        // Shuffle the permutation array
        for (int i = 0; i < _perlinSize; i++)
        {
            int j = random.Next(_perlinSize);
            int temp = permutation[i];
            permutation[i] = permutation[j];
            permutation[j] = temp;
        }

        // Duplicate the permutation array to avoid overflow
        for (int i = 0; i < _perlinSize; i++)
        {
            _p[i] = permutation[i];
            _p[i + _perlinSize] = permutation[i];
        }
    }

    private float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private float Lerp(float t, float a, float b)
    {
        return a + t * (b - a);
    }

    private float Grad(int hash, float x, float y)
    {
        int h = hash & 15;
        return GRAD2[h, 0] * x + GRAD2[h, 1] * y;
    }

    public float Noise(float x, float y)
    {
        int xi = (int)x & (_perlinSize - 1);
        int yi = (int)y & (_perlinSize - 1);

        float xf = x - xi;
        float yf = y - yi;

        while (xf > 1) xf -= _perlinSize;
        while (yf > 1) yf -= _perlinSize;

        float xg = Fade(xf);
        float yg = Fade(yf);

        int px = _p[xi];
        int px1 = _p[xi + 1];

        float n00 = Grad(_p[px + yi], xf, yf);
        float n01 = Grad(_p[px1 + yi], xf - 1, yf);
        float n10 = Grad(_p[px + yi + 1], xf, yf - 1);
        float n11 = Grad(_p[px1 + yi + 1], xf - 1, yf - 1);

        return Lerp(yg, Lerp(xg, n00, n01), Lerp(xg, n10, n11));
    }

    public static int[,] GenerateNoiseMatrix(int seed, int size, float scale)
    {
        PerlinNoise perlin = new PerlinNoise(seed, size);
        int mSize = (int)(size / scale);
        int[,] noiseMatrix = new int[mSize, mSize];

        Parallel.For(0, mSize, y =>
        {
            for (int x = 0; x < mSize; x++)
            {
                noiseMatrix[x, y] = (int)(((perlin.FractalNoise(x * scale, y * scale, 8, 0.5f) + 1) / 2) * 100);
            }
        });

        return noiseMatrix;
    }

    public float FractalNoise(float x, float y, int octaves, float persistence)
    {
        float total = 0;
        float frequency = 1f;
        float amplitude = 1;
        float maxValue = 0;

        for (int i = 0; i < octaves; i++)
        {
            total += Noise(x * frequency, y * frequency) * amplitude;

            maxValue += amplitude;

            amplitude *= persistence;
            frequency *= 2;
        }

        return total / maxValue;
    }
}