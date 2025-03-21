﻿using System.IO;
using System.Net.Http;

namespace aweXpect.Tests;

public sealed partial class ThatHttpResponseMessage
{
	public sealed class HasContentType
	{
		public sealed class Tests
		{
			[Fact]
			public async Task WhenContentTypeDiffersFromExpected_ShouldFail()
			{
				string expected = "text/content-type";
				HttpResponseMessage subject = ResponseBuilder
					.WithContentType("text/other-content-type")
					.WithContent("some content");

				async Task Act()
					=> await That(subject).HasContentType(expected);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has a `Content-Type` header equal to "text/content-type",
					             but it was "text/other-content-type" which differs at index 5:
					                     ↓ (actual)
					               "text/other-content-type"
					               "text/content-type"
					                     ↑ (expected)

					             HTTP-Response:
					               200 OK HTTP/1.1
					                 Content-Type: text/other-content-type
					               some content
					             """);
			}

			[Fact]
			public async Task WhenContentTypeEqualsExpected_ShouldSucceed()
			{
				string expected = "some/content";
				HttpResponseMessage subject = ResponseBuilder
					.WithContentType(expected);

				async Task Act()
					=> await That(subject).HasContentType(expected);

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenContentTypeHeaderIsNotSet_ShouldFail()
			{
				string expected = "text/content-type";
				HttpResponseMessage subject = ResponseBuilder
					.WithContent(new StreamContent(new MemoryStream([0x0, 0x1,])));

				async Task Act()
					=> await That(subject).HasContentType(expected);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has a `Content-Type` header equal to "text/content-type",
					             but it had no `Content-Type` header

					             HTTP-Response:
					               200 OK HTTP/1.1
					               *Content with length 2*
					             """);
			}

			[Fact]
			public async Task WhenSubjectIsNull_ShouldFail()
			{
				HttpResponseMessage? subject = null;

				async Task Act()
					=> await That(subject).HasContentType("some content").IgnoringCase();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has a `Content-Type` header equal to "some content" ignoring case,
					             but it was <null>
					             """);
			}
		}

		public sealed class NegatedTests
		{
			[Fact]
			public async Task WhenContentTypeDiffersFromExpected_ShouldFail()
			{
				string expected = "text/content-type";
				HttpResponseMessage subject = ResponseBuilder
					.WithContentType("text/other-content-type")
					.WithContent("some content");

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.HasContentType(expected));

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenContentTypeEqualsExpected_ShouldFail()
			{
				string expected = "some/content-type";
				HttpResponseMessage subject = ResponseBuilder
					.WithContentType(expected);

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.HasContentType(expected));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             does not have a `Content-Type` header equal to "some/content-type",
					             but it had
					             """);
			}

			[Fact]
			public async Task WhenContentTypeHeaderIsNotSet_ShouldSucceed()
			{
				string expected = "text/content-type";
				HttpResponseMessage subject = ResponseBuilder
					.WithContent(new StreamContent(new MemoryStream([0x0, 0x1,])));

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.HasContentType(expected));

				await That(Act).DoesNotThrow();
			}
		}
	}
}
