using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net.LShift.Diffa.Participants
{
    class DigestBuilder
    {
        public DigestBuilder(IDictionary<string, string> buckets)
        {
            throw new NotImplementedException(); // TODO
        }

        public void Add(string id, IDictionary<string, string> attributes, DateTime lastUpdated, string version)
        {
            throw new NotImplementedException(); // TODO
        }

        public IList<AggregateDigest> GetDigests()
        {
            return null; // TODO
        }
    }
}
