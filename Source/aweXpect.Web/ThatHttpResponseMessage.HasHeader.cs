﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Helpers;

namespace aweXpect;

public static partial class ThatHttpResponseMessage
{
	/// <summary>
	///     Verifies that the <see cref="HttpResponseMessage" /> has the <paramref name="expected" /> header.
	/// </summary>
	public static HasHeaderValueResult<HttpResponseMessage, IThat<HttpResponseMessage?>> HasHeader(
		this IThat<HttpResponseMessage?> source,
		string expected)
		=> new(source.ThatIs().ExpectationBuilder.AddConstraint((it, grammar) =>
				new HasHeaderConstraint(it, expected)),
			source,
			a => a.Headers.TryGetValues(expected, out IEnumerable<string>? values) ? values.ToArray() : null);

	private readonly struct HasHeaderConstraint(string it, string expected)
		: IAsyncConstraint<HttpResponseMessage>
	{
		public async Task<ConstraintResult> IsMetBy(
			HttpResponseMessage? actual,
			CancellationToken cancellationToken)
		{
			if (actual == null)
			{
				return new ConstraintResult.Failure<HttpResponseMessage?>(actual, ToString(),
					$"{it} was <null>", FurtherProcessingStrategy.IgnoreResult);
			}

			if (actual.Headers.TryGetValues(expected, out _))
			{
				return new ConstraintResult.Success<HttpResponseMessage?>(actual, ToString());
			}

			string formattedResponse =
				await HttpResponseMessageFormatter.Format(actual, "  ", cancellationToken);
			return new ConstraintResult.Failure<HttpResponseMessage?>(actual, ToString(),
					$"{it} did not contain the expected header", FurtherProcessingStrategy.IgnoreResult)
				.WithContext("HTTP-Request", formattedResponse);
		}

		public override string ToString()
			=> $"has a `{expected}` header";
	}
}
