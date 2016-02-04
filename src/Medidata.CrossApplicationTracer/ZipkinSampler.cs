using System;
using System.Collections.Generic;
using System.Linq;

namespace Medidata.CrossApplicationTracer
{
    /// <summary>
    /// Handles sampling rate and path blacklist
    /// </summary>
    public class ZipkinSampler
    {
        private static Random random = new Random();
 
        internal readonly List<string> dontSampleList;
        internal readonly float sampleRate;
      
        /// <summary>
        /// Zipkin Samplre constuctor
        /// </summary>
        /// <param name="dontSampleListCsv"></param>
        /// <param name="configSampleRate"></param>
        public ZipkinSampler(string dontSampleListCsv, string configSampleRate)
        {
            var dontSampleList = new List<string>();
            if (!String.IsNullOrWhiteSpace(dontSampleListCsv))
            {
                dontSampleList.AddRange(dontSampleListCsv.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim().ToLowerInvariant()));
            }

            float zipkinSampleRate;
            float.TryParse(configSampleRate, out zipkinSampleRate);
            if ( zipkinSampleRate < 0 || zipkinSampleRate > 1)
            {
                throw new ArgumentException("zipkinConfig zipkinSampleRate is not between 0 and 1");
            }

            this.dontSampleList = dontSampleList;
            this.sampleRate = zipkinSampleRate;
        }

        internal bool IsInDontSampleList(string path)
        {
            if (path != null)
            {
                if (dontSampleList.Any(uri => path.StartsWith(uri, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Calculates if current request should be sampled
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="sampled"></param>
        /// <returns></returns>
        public virtual bool ShouldBeSampled(System.Web.HttpContextBase httpContext, string sampled)
        {
            if (httpContext == null)
            {
                return false;
            }

            bool result;
            if (!string.IsNullOrWhiteSpace(sampled) && Boolean.TryParse(sampled, out result))
            {
                return result;
            }

            if (!IsInDontSampleList(httpContext.Request.Path))
            {
                if (random.NextDouble() <= sampleRate)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
