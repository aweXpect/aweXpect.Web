﻿using System.Net.Http;

namespace aweXpect.Tests;

public sealed partial class ThatHttpRequestMessage
{
	public sealed class HasRequestUri
	{
		public sealed class Tests
		{
			[Fact]
			public async Task WhenSubjectIsNull_ShouldFail()
			{
				HttpRequestMessage? subject = null;

				async Task Act()
					=> await That(subject).HasRequestUri(new Uri("https://awexpect.com"));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has a request URI equal to "https://awexpect.com/",
					             but it was <null>
					             """);
			}

			[Fact]
			public async Task WhenUriDiffers_ShouldFail()
			{
				HttpRequestMessage subject = RequestBuilder
					.WithMethod(HttpMethod.Get)
					.WithUri("https://awexpect.com");

				async Task Act()
					=> await That(subject).HasRequestUri("https://awexpect.com/awexpect.Web");

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             has a request URI equal to "https://awexpect.com/awexpect.…",
					             but it was "https://awexpect.com/" with a length of 21 which is shorter than the expected length of 33 and misses:
					               "awexpect.Web"

					             HTTP-Request:
					               GET https://awexpect.com/ HTTP/1.1
					             """);
			}

			[Fact]
			public async Task WhenUriIsExpected_ShouldSucceed()
			{
				HttpRequestMessage subject = RequestBuilder
					.WithUri("https://awexpect.com");

				async Task Act()
					=> await That(subject).HasRequestUri("https://awexpect.com");

				await That(Act).DoesNotThrow();
			}
		}
	}
}
