using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace aweXpect.Web.Samples;

public class Program
{
	public static void Main(string[] args)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

		builder.Services.AddSingleton<CommentStore>();

		WebApplication app = builder.Build();

		app.MapGet("/comments", (CommentStore store) => store.GetComments());

		app.MapGet("/comments/{id}", (int id, CommentStore store, HttpContext httpContext) =>
		{
			httpContext.Response.Headers["x-vendor"] = "VENDOR";
			return store.GetComment(id);
		});

		app.Run();
	}
}
