namespace Underdeck.Core
{
    /// <summary>
    /// SplitMix64, bit-for-bit identical to the reference JS engine in
    /// prototype/index.html. A given seed must produce the identical dungeon
    /// on every platform — this is what makes Daily Descent fair and bug
    /// reports reproducible.
    /// </summary>
    public struct SplitMix64
    {
        private ulong _state;

        public SplitMix64(ulong seed) => _state = seed;

        public ulong Next()
        {
            ulong z = _state += 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        /// <summary>Uniform int in [0, n). Matches the JS engine's `rngInt`.</summary>
        public int NextInt(int n) => (int)(Next() % (ulong)n);
    }

    public static class RngUtil
    {
        /// <summary>Seeded Fisher–Yates. Matches the JS engine's `shuffleWith`.</summary>
        public static void Shuffle<T>(System.Collections.Generic.IList<T> list, ref SplitMix64 rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.NextInt(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }

    /// <summary>
    /// Seed derivation matching the JS engine exactly: the ability RNG stream
    /// is the run seed XORed with 0xC0FFEE; each depth beyond the first adds
    /// depth*7919 to the base seed before building that depth's dungeon.
    /// </summary>
    public static class Seeding
    {
        public static ulong Parse(string seed) =>
            ulong.Parse(seed, System.Globalization.CultureInfo.InvariantCulture);

        public static ulong AbilityStream(ulong baseSeed) => baseSeed ^ 0xC0FFEEUL;

        public static ulong DepthDeck(ulong baseSeed, int depth) =>
            depth == 1 ? baseSeed : baseSeed + (ulong)(depth * 7919);
    }
}
