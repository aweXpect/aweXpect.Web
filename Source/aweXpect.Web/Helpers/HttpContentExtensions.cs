﻿using System;
using System.Net.Http;

namespace aweXpect.Helpers;

internal static class HttpContentExtensions
{
	public static bool IsNullOrDisposed(this HttpContent? content)
	{
		if (content == null)
		{
			return true;
		}

		try
		{
			_ = content.Headers?.ContentLength;
			return false;
		}
		catch (ObjectDisposedException)
		{
			return true;
		}
	}

	public static bool TryGetContentLength(this HttpContent? content, out long length)
	{
		try
		{
			length = content?.Headers?.ContentLength ?? 0;
			return true;
		}
		catch (Exception)
		{
			length = 0;
			return false;
		}
	}

	public static bool TryGetMediaType(this HttpContent? content, out string? contentType)
	{
		try
		{
			contentType = content?.Headers?.ContentType?.MediaType;
			return true;
		}
		catch (Exception)
		{
			contentType = null;
			return false;
		}
	}
}
