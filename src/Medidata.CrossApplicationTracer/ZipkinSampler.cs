using System;
using System.Collections.Generic;
using System.Linq;

namespace Medidata.CrossApplicationTracer
{
    public class ZipkinSampler : Sampler
    {
        internal readonly List<string> dontSampleList;
      
        public ZipkinSampler(string dontSampleListCsv, string configSampleRate) : 
            base(configSampleRate)
        {
            var dontSampleList = new List<string>();
            if (!string.IsNullOrWhiteSpace(dontSampleListCsv))
            {
                dontSampleList.AddRange(dontSampleListCsv.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim().ToLowerInvariant()));
            }

            this.dontSampleList = dontSampleList;
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

        public virtual bool ShouldBeSampled(System.Web.HttpContextBase httpContext, string sampled)
        {
            if ( httpContext == null )
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
                return base.ShouldBeSampled();
            }
            return false;
        }
    }
}
