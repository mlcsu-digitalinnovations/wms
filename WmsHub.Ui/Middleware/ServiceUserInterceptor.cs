using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WmsHub.Ui.Middleware
{
	public class ServiceUserInterceptor
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ServiceUserInterceptor> _logger;

		public ServiceUserInterceptor(
			RequestDelegate next,
			ILogger<ServiceUserInterceptor> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task Invoke(HttpContext context)
		{
			string[] protectedPages = new string[] {
				"SelectEthnicity",
				"SelectEthnicityGroup",
				"ContactPreference",
				"SelectProvider",
				"WmsSelection",
				"WmsCompletion"
			};

			Microsoft.AspNetCore.Routing.RouteValueDictionary routeValues =
				context.Request.RouteValues;

			if (routeValues.ContainsKey("controller") &&
				routeValues.ContainsKey("action") &&
				routeValues.ContainsKey("id"))
			{

				if (routeValues["controller"].ToString() == "ServiceUser" &&
				protectedPages.Contains(routeValues["action"].ToString()))
				{
					// check if session is for correct referral
					string referralIdFromSession =
						context.Session.GetString("ReferralId");

					if (string.IsNullOrEmpty(referralIdFromSession) ||
					referralIdFromSession != routeValues["id"].ToString())
					{

						_logger.LogError(
							".Wms.Session cookie referral {cookie} does not match route " +
								"value referral {routeValue}.",
							referralIdFromSession,
							routeValues["id"].ToString());

						context.Response.Redirect("/ServiceUser/GoneWrong");
					}
				}
			}

			await _next.Invoke(context);
		}
	}

	public static class ServiceUserInterceptorExtensions
	{
		public static IApplicationBuilder UseServiceUserInterceptor(
			this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<ServiceUserInterceptor>();
		}
	}
}