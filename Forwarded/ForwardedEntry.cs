using System;
using System.Net;

namespace Forwarded
{
    public struct ForwardedEntry
    {
        private const string Unknown = "unknown";
        public IPAddress Address;
        public string Name;

        public ForwardedEntry(IPAddress address)
        {
            Address = address;
            Name = null;
        }

        public ForwardedEntry(string name)
        {
            Name = name;
            Address = null;
        }

        public bool HasAddress
        {
            get { return Address != null; }
        }

        public bool HasName
        {
            get { return Name != null; }
        }

        public bool IsUnknown
        {
            get { return HasName && Name.Equals(Unknown, StringComparison.OrdinalIgnoreCase); }
        }
    }
}