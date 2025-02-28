using System;
using System.Net.Http;
using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Helpers;
using aweXpect.Options;
using aweXpect.Results;

namespace aweXpect;

public static partial class ThatHttpRequestMessage
{
	/// <summary>
	///     Verifies that the <see cref="HttpRequestMessage" /> has the <paramref name="expected" />
	///     <see cref="HttpRequestMessage.RequestUri" />.
	/// </summary>
	public static StringEqualityTypeResult<HttpRequestMessage, IThat<HttpRequestMessage?>>
		HasRequestUri(this IThat<HttpRequestMessage?> source, string expected)
	{
		StringEqualityOptions options = new();
		return new StringEqualityTypeResult<HttpRequestMessage, IThat<HttpRequestMessage?>>(
			source.ThatIs().ExpectationBuilder
				.UpdateContexts(c => c.Close())
				.AddConstraint((expectationBuilder, it, grammar) =>
					new HasRequestUriConstraint(expectationBuilder, it, new Uri(expected).ToString(), options)),
			source,
			options);
	}

	/// <summary>
	///     Verifies that the <see cref="HttpRequestMessage" /> has the <paramref name="expected" />
	///     <see cref="HttpRequestMessage.RequestUri" />.
	/// </summary>
	public static StringEqualityTypeResult<HttpRequestMessage, IThat<HttpRequestMessage?>>
		HasRequestUri(this IThat<HttpRequestMessage?> source, Uri expected)
	{
		StringEqualityOptions options = new();
		return new StringEqualityTypeResult<HttpRequestMessage, IThat<HttpRequestMessage?>>(
			source.ThatIs().ExpectationBuilder
				.UpdateContexts(c => c.Close())
				.AddConstraint((expectationBuilder, it, grammar) =>
					new HasRequestUriConstraint(expectationBuilder, it, expected.ToString(), options)),
			source,
			options);
	}

	private readonly struct HasRequestUriConstraint(
		ExpectationBuilder expectationBuilder,
		string it,
		string expected,
		StringEqualityOptions options)
		: IValueConstraint<HttpRequestMessage>
	{
		public ConstraintResult IsMetBy(HttpRequestMessage? actual)
		{
			if (actual == null)
			{
				return new ConstraintResult.Failure<HttpRequestMessage?>(actual, ToString(),
					$"{it} was <null>");
			}

			string? requestUri = actual.RequestUri?.ToString();
			if (!options.AreConsideredEqual(requestUri, expected))
			{
				expectationBuilder.AddContext(actual);
				return new ConstraintResult.Failure<HttpRequestMessage?>(actual, ToString(),
					options.GetExtendedFailure(it, requestUri, expected));
			}

			return new ConstraintResult.Success<HttpRequestMessage?>(actual, ToString());
		}

		public override string ToString()
			=> $"has a request URI {options.GetExpectation(expected, ExpectationGrammars.None)}";
	}
}
