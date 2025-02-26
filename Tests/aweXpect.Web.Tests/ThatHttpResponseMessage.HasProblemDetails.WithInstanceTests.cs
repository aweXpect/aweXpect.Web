﻿using System.Net.Http;

namespace aweXpect.Tests;

public sealed partial class ThatHttpResponseMessage
{
	public sealed partial class HasProblemDetails
	{
		public sealed class WithInstanceTests
		{
			[Fact]
			public async Task WhenInstanceDiffersInCase_WithIgnoringCase_ShouldSucceed()
			{
				HttpResponseMessage subject = ResponseBuilder
					.WithContent("""
					             {
					               "type": "my-type",
					               "instance": "bar"
					             }
					             """);

				async Task Act()
					=> await That(subject).HasProblemDetails().WithInstance("BAR").IgnoringCase();

				await That(Act).DoesNotThrow();
			}

			[Theory]
			[InlineData("foo", "bar")]
			[InlineData("foo", "FOO")]
			public async Task WhenInstanceDoesNotMatch_ShouldFail(string actualInstance, string expectedInstance)
			{
				HttpResponseMessage subject = ResponseBuilder
					.WithContent($$"""
					               {
					                 "type": "my-type",
					                 "instance": "{{actualInstance}}"
					               }
					               """);

				async Task Act()
					=> await That(subject).HasProblemDetails().WithInstance(expectedInstance);

				await That(Act).Throws<XunitException>()
					.WithMessage($$"""
					               Expected that subject
					               has a ProblemDetails content with any type and instance "{{expectedInstance}}",
					               but it had instance "{{actualInstance}}" which differs at index 0:
					                  ↓ (actual)
					                 "{{actualInstance}}"
					                 "{{expectedInstance}}"
					                  ↑ (expected)

					               HTTP-Request:
					                 HTTP/1.1 200 OK
					                   Content-Type: text/plain; charset=utf-8
					                   Content-Length: 47
					                 {
					                   "type": "my-type",
					                   "instance": "{{actualInstance}}"
					                 }
					                 The originating request was <null>
					               """);
			}

			[Fact]
			public async Task WhenInstanceMatches_ShouldSucceed()
			{
				HttpResponseMessage subject = ResponseBuilder
					.WithContent("""
					             {
					               "type": "my-type",
					               "instance": "foo"
					             }
					             """);

				async Task Act()
					=> await That(subject).HasProblemDetails().WithInstance("foo");

				await That(Act).DoesNotThrow();
			}
		}
	}
}
