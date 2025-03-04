﻿using aweXpect.Core;
using aweXpect.Options;
using aweXpect.Results;

namespace aweXpect.Web.Results;

/// <summary>
///     Result for handling additional expectations on problem details.
/// </summary>
public class ProblemDetailsResult<TType, TThat>(
	ExpectationBuilder expectationBuilder,
	TThat returnValue,
	ProblemDetailsOptions options)
	: AndOrResult<TType, TThat, ProblemDetailsResult<TType, TThat>>(expectationBuilder, returnValue)
{
	private readonly ExpectationBuilder _expectationBuilder = expectationBuilder;
	private readonly TThat _returnValue = returnValue;

	/// <summary>
	///     Verify that the title of the problem details object matches the expected <paramref name="title" />.
	/// </summary>
	/// <remarks>
	///     Title:
	///     A short, human-readable summary of the problem type. It SHOULD NOT change from occurrence to occurrence of the
	///     problem, except for purposes of localization(e.g., using proactive content negotiation;
	///     <see href="https://datatracker.ietf.org/doc/html/rfc7231#section-3.4" />).
	/// </remarks>
	public String WithTitle(string title)
		=> new(_expectationBuilder, _returnValue, options.WithTitle(title), options);

	/// <summary>
	///     Verify that the status of the problem details object matches the expected <paramref name="status" />.
	/// </summary>
	/// <remarks>
	///     Status:
	///     The HTTP status code (<see href="https://datatracker.ietf.org/doc/html/rfc7231#section-6" />) generated by the
	///     origin server for this occurrence of the problem.
	/// </remarks>
	public ProblemDetailsResult<TType, TThat> WithStatus(int status)
	{
		options.WithStatus(status);
		return new ProblemDetailsResult<TType, TThat>(_expectationBuilder, _returnValue,
			options);
	}

	/// <summary>
	///     Verify that the detail of the problem details object matches the expected <paramref name="detail" />.
	/// </summary>
	/// <remarks>
	///     Detail:
	///     A human-readable explanation specific to this occurrence of the problem.
	/// </remarks>
	public String WithDetail(string detail)
		=> new(_expectationBuilder, _returnValue, options.WithDetail(detail), options);

	/// <summary>
	///     Verify that the instance of the problem details object matches the expected <paramref name="instance" />.
	/// </summary>
	/// <remarks>
	///     Instance:
	///     A URI reference that identifies the specific occurrence of the problem.  It may or may not yield further
	///     information if dereferenced.
	/// </remarks>
	public String WithInstance(string instance) => new(_expectationBuilder,
		_returnValue, options.WithInstance(instance), options);


	/// <summary>
	///     In addition to a <see cref="ProblemDetailsResult{TType,TThat}" /> allows specifying string equality settings.
	/// </summary>
	public class String(
		ExpectationBuilder expectationBuilder,
		TThat returnValue,
		StringEqualityOptions stringEqualityOptions,
		ProblemDetailsOptions options)
		: StringEqualityResult<TType, TThat, String>(expectationBuilder, returnValue,
			stringEqualityOptions)
	{
		private readonly ExpectationBuilder _expectationBuilder = expectationBuilder;
		private readonly TThat _returnValue = returnValue;

		/// <summary>
		///     Verify that the title of the problem details object matches the expected <paramref name="title" />.
		/// </summary>
		/// <remarks>
		///     Title:
		///     A short, human-readable summary of the problem type. It SHOULD NOT change from occurrence to occurrence of the
		///     problem, except for purposes of localization(e.g., using proactive content negotiation;
		///     <see href="https://datatracker.ietf.org/doc/html/rfc7231#section-3.4" />).
		/// </remarks>
		public String WithTitle(string title)
			=> new(_expectationBuilder, _returnValue, options.WithTitle(title), options);

		/// <summary>
		///     Verify that the status of the problem details object matches the expected <paramref name="status" />.
		/// </summary>
		/// <remarks>
		///     Status:
		///     The HTTP status code (<see href="https://datatracker.ietf.org/doc/html/rfc7231#section-6" />) generated by the
		///     origin server for this occurrence of the problem.
		/// </remarks>
		public ProblemDetailsResult<TType, TThat> WithStatus(int status)
		{
			options.WithStatus(status);
			return new ProblemDetailsResult<TType, TThat>(_expectationBuilder, _returnValue, options);
		}

		/// <summary>
		///     Verify that the detail of the problem details object matches the expected <paramref name="detail" />.
		/// </summary>
		/// <remarks>
		///     Detail:
		///     A human-readable explanation specific to this occurrence of the problem.
		/// </remarks>
		public String WithDetail(string detail)
			=> new(_expectationBuilder, _returnValue, options.WithDetail(detail), options);

		/// <summary>
		///     Verify that the instance of the problem details object matches the expected <paramref name="instance" />.
		/// </summary>
		/// <remarks>
		///     Instance:
		///     A URI reference that identifies the specific occurrence of the problem.  It may or may not yield further
		///     information if dereferenced.
		/// </remarks>
		public String WithInstance(string instance) => new(_expectationBuilder,
			_returnValue, options.WithInstance(instance), options);
	}
}
