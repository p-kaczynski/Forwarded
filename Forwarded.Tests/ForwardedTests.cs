using System.Collections.Generic;
using System.Linq;
using System.Net;
using Xunit;
using Should;

namespace Forwarded.Tests
{
    public class ForwardedTests
    {
        [Theory]
        [MemberData("SinglePositiveAddressProvider")]
        public void Parse_Single_Positive_Addresses(string headerBody, IPAddress ipAddressExpected)
        {
            var forwarded = ForwardedHeader.Parse(new[] { headerBody });
            forwarded.For.Count.ShouldEqual(1);
            forwarded.For.Single().HasAddress.ShouldBeTrue();
            forwarded.For.Single().Address.Equals(ipAddressExpected).ShouldBeTrue();
        }

        [Theory]
        [MemberData("SinglePositiveNameProvider")]
        public void Parse_Single_Positive_Name(string headerBody, string expectedName)
        {
            var forwarded = ForwardedHeader.Parse(new[] { headerBody });
            forwarded.For.Count.ShouldEqual(1);
            forwarded.For.Single().HasName.ShouldBeTrue();
            forwarded.For.Single().Name.Equals(expectedName).ShouldBeTrue();
        }

        [Theory]
        [MemberData("SingleNegativeProvider")]
        public void Parse_Single_Negative(string headerBody)
        {
            var f = ForwardedHeader.Parse(new[] { headerBody });

            f.ShouldNotBeNull();
            f.For.ShouldBeEmpty();
        }



        public static IEnumerable<object[]> SingleNegativeProvider
        {
            get
            {
                // ipv4 - to many
                yield return new object[] { "for=192.0.2.60.33" };
                // ipv4 - to few
                yield return new object[] { "for=192.0.2" };
                // ipv4 with port unquoted
                yield return new object[] { "for=192.0.2.60:1234" };
                // ipv6 unquoted
                yield return new object[] { "for=[2001:db8:cafe::17]" };
                // ipv6 with port unquoted
                yield return new object[] { "for=[2001:db8:cafe::17]:1234" };
                // ipv6 double double colon
                yield return new object[] { "for=\"[2001:db8::cafe::17]:1234\"" };
                // ipv6 non hex
                yield return new object[] { "for=\"[2001:db8:frse::17]:1234\"" };
                // non alphanumeric + underscore token
                yield return new object[] { "for=example.com" };
            }
        }

        public static IEnumerable<object[]> SinglePositiveNameProvider
        {
            get
            {
                // ipv4
                yield return new object[] { "for=Hostname", "Hostname" };
                yield return new object[] { "for=_OBFNAME", "_OBFNAME" };
                yield return new object[] { "for=\"Hostname\"", "Hostname" };
                yield return new object[] { "for=\"_OBFNAME\"", "_OBFNAME" };
            }
        }

        public static IEnumerable<object[]> SinglePositiveAddressProvider
        {
            get
            {
                // ipv4
                yield return new object[] { "for=192.0.2.60", IPAddress.Parse("192.0.2.60") };
                // First uppercase
                yield return new object[] { "For=192.0.2.60", IPAddress.Parse("192.0.2.60") };
                // Random uppercase
                yield return new object[] { "fOR=192.0.2.60", IPAddress.Parse("192.0.2.60") };
                // quoted ipv4
                yield return new object[] { "for=\"192.0.2.60\"", IPAddress.Parse("192.0.2.60") };
                // quoted ipv4 with port
                yield return new object[] { "for=\"192.0.2.60:5412\"", IPAddress.Parse("192.0.2.60") };
                // ipv6
                yield return new object[] { "for=\"[2001:db8:cafe::17]\"", IPAddress.Parse("2001:db8:cafe::17") };
                // ipv6 with port
                yield return new object[] { "for=\"[2001:db8:cafe::17]:4711\"", IPAddress.Parse("2001:db8:cafe::17") };
            }
        }
    }
}
