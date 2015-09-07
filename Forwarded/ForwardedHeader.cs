using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Forwarded
{
    /// <summary>
    ///     Helper for using the Forwarded header as defined in <see href="http://tools.ietf.org/html/rfc7239">RFC7239</see>
    ///     Currently supports loading the "For" token only.
    /// </summary>
    public class ForwardedHeader
    {
        public const string HeaderName = "Forwarded";
        // Source regex:
        // for=(?:(?:\"(?:(?:\[(?<ipv6>[0-9a-f:]+)\](?::\d+)?)|(?<qipv4>(?:[0-9]{1,3}\.){3}[0-9]{1,3})(?::\d+)?|(?<qname>[^\"\[\]\:]*))\")|(?<ipv4>(?:[0-9]{1,3}\.){3}[0-9]{1,3})|(?<unqname>[a-zA-Z0-9_]+))(?:$|[,;])
        // Captures three possible values of a 'for' Forwarded token: ipv4, ipv6 or host name.
        // The validation of ip is done elsewhere to keep this already lengthy regex in a reasonable form
        // Non-capturing groups are implied by RegexOptions.ExplicitCapture
        private static readonly Regex ForRegex =
            new Regex(
                "for=((\\\"((\\[(?<ipv6>[0-9a-f:]+)\\](:\\d+)?)|(?<qipv4>([0-9]{1,3}\\.){3}[0-9]{1,3})(:\\d+)?|(?<qname>[^\\\"\\[\\]\\:]*))\\\")|(?<ipv4>([0-9]{1,3}\\.){3}[0-9]{1,3})|(?<unqname>[a-zA-Z0-9_]+))($|[,;])",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        private static readonly string[] ForGroupNames =
            ForRegex.GetGroupNames().Where(n => !n.Equals(0.ToString())).ToArray();

        private readonly List<ForwardedEntry> _forwarded = new List<ForwardedEntry>();

        /// <summary>
        ///     Returns read-only collection of subsequent forwarded steps, in order of headers, then in order of appearance in
        ///     header body
        /// </summary>
        public IReadOnlyList<ForwardedEntry> For
        {
            get { return _forwarded; }
        }

        /// <summary>
        ///     Loads Forwarded header from provided <see cref="HttpRequestMessage" /> instance.
        /// </summary>
        /// <param name="request">The HTTP request</param>
        /// <returns>An object representing Forwarded headers, or new instance if the headers weren't present</returns>
        public static ForwardedHeader Load(HttpRequestMessage request)
        {
            IEnumerable<string> values;
            return !request.Headers.TryGetValues(HeaderName, out values) ? new ForwardedHeader() : Parse(values);
        }

        /// <summary>
        ///     Parses provided sequence of header bodies into Forwarded object
        /// </summary>
        /// <param name="headers">Sequence of header bodies (can be empty, cannot be null)</param>
        /// <returns>An object representing Forwarded headers, or new instance if the headers weren't present</returns>
        public static ForwardedHeader Parse(IEnumerable<string> headers)
        {
            if (headers == null)
                throw new ArgumentNullException("headers");

            var forwarded = new ForwardedHeader();

            foreach (var result in from header in headers
                                   select ForRegex.Match(header)
                                       into matchResult
                                       where matchResult.Success
                                       from result in ForGroupNames.Select(name => new { Name = name, Group = matchResult.Groups[name] })
                                           .Where(r => r.Group.Success)
                                           .Select(r => new { r.Name, r.Group.Value })
                                       select result)
            {
                switch (result.Name)
                {
                    case "ipv4":
                    case "qipv4":
                    case "ipv6":
                        IPAddress ip;
                        if (IPAddress.TryParse(result.Value, out ip))
                            forwarded._forwarded.Add(
                                new ForwardedEntry(ip));
                        break;
                    case "qname":
                    case "unqname":
                        forwarded._forwarded.Add(new ForwardedEntry(result.Value));
                        break;
                    default:
                        throw new FormatException("The ForRegex contained named group not included in switch labels: " +
                                                  result.Name);
                }
            }

            return forwarded;
        }
    }
}
