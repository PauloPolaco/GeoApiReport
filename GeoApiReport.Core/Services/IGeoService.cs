using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using GeoApiReport.Core.Enums;
using GeoApiReport.Core.Models;

namespace GeoApiReport.Core.Services
{
	public interface IGeoService
	{
		/// <summary>
		/// Location of resource used to find geolocation data for various IP addresses.
		/// </summary>
		Uri GeolocationResource { get; }

		/// <summary>
		/// Locations of resources used to find user data for various IP addresses.
		/// </summary>
		Dictionary<UserDataResourceSiteCodeEnum, Uri> UserDataResources { get; }

		/// <summary>
		/// Determines which geographic region's REST service use based on provided ISO-3366-1 country code.
		/// </summary>
		/// <param name="countryCode">ISO-3366-1 country code.</param>
		/// <returns>Enum value used to get appropriate REST service for the provided ISO-3366-1 country code. </returns>
		UserDataResourceSiteCodeEnum? GetUserDataResourceType(string countryCode);

		/// <summary>
		/// Contacts a REST service to retrieve a JSON array based on a list
		/// of provided list of IP addresses and converts it into a POCO.
		/// </summary>
		/// <typeparam name="T">Type of POCO object that maps to JSON output.</typeparam>
		/// <param name="url">Location of REST service.</param>
		/// <param name="inputModel">List of IP addresses to be processed.</param>
		/// <param name="jsonRootElementName">Root element of JSON response.</param>
		/// <returns>Result set as a POCO based on JSON results.</returns>
		[HttpPost]
		Task<IList<T>> RetrieveItemsFromResourceAsync<T>(Uri url, AddressModel inputModel, string jsonRootElementName);
	}
}