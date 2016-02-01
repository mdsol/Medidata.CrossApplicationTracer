using System;

namespace Medidata.CrossApplicationTracer
{
    /// <summary>
    /// Base sampler object
    /// </summary>
    public class Sampler
    {
        private static readonly Random random = new Random();
        internal readonly float sampleRate;

        /// <summary>
        /// Base sampler object.
        /// Calculates sampling rate
        /// </summary>
        /// <param name="configSampleRate"></param>
        public Sampler(string configSampleRate)
        {
            float zipkinSampleRate;
            float.TryParse(configSampleRate, out zipkinSampleRate);
            if (zipkinSampleRate < 0 || zipkinSampleRate > 1)
            {
                throw new ArgumentException("zipkinConfig zipkinSampleRate is not between 0 and 1");
            }

            sampleRate = zipkinSampleRate;
        }

        /// <summary>
        /// Determines if should be sampled
        /// </summary>
        /// <returns></returns>
        public bool ShouldBeSampled()
        {
            if (random.NextDouble() <= sampleRate)
            {
                return true;
            }
            return false;
        }
    }
}
