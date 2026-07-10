namespace Scroundel.Core
{
    /// <summary>
    /// Small deterministic RNG (SplitMix64). Guarantees identical shuffles for
    /// a given seed on every platform and runtime — System.Random makes no such
    /// cross-runtime promise, and daily challenges / bug repros depend on
    /// reproducibility (GAME_PLAN §5.3).
    /// </summary>
    public struct Rng
    {
        private ulong _state;

        public Rng(ulong seed) => _state = seed;

        public ulong NextUInt64()
        {
            ulong z = _state += 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        /// <summary>
        /// Uniform int in [0, maxExclusive). Modulo bias is negligible for
        /// deck-sized bounds (≤ 44) against a 64-bit generator.
        /// </summary>
        public int NextInt(int maxExclusive)
        {
            if (maxExclusive < 1)
                throw new System.ArgumentOutOfRangeException(nameof(maxExclusive));
            return (int)(NextUInt64() % (ulong)maxExclusive);
        }
    }
}
