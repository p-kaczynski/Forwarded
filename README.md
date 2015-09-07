Forwarded header
=============

## Purpose
This library provides parsing of [RFC 7239](http://tools.ietf.org/html/rfc7239) "Forwarded" HTTP headers.

## Usage
`ForwardedHeader` provides `Load` method to use with `HttpRequestMessage` or a more direct `Parse` that works with a sequence of strings. The result is an object with zero or more `ForwardedEntry` structs that represent possible data extracted from the `for` header field.

Currently other fields like `by` or `prot` are simply ignored.