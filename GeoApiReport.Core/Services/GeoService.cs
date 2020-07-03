using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using GeoApiReport.Core.Enums;
using GeoApiReport.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeoApiReport.Core.Services
{
	public sealed class GeoService : IGeoService
	{
		/// <summary>
		/// Location of resource used to find geolocation data for various IP addresses.
		/// </summary>
		public Uri GeolocationResource => m_resGeolocation;
		private readonly Uri m_resGeolocation = new Uri(Resources.Resources.SITE_URL_ADDRESSES);

		/// <summary>
		/// Locations of resources used to find user data for various IP addresses.
		/// </summary>
		public Dictionary<UserDataResourceSiteCodeEnum, Uri> UserDataResources => m_resUserLocations;
		private readonly Dictionary<UserDataResourceSiteCodeEnum, Uri> m_resUserLocations =
			new Dictionary<UserDataResourceSiteCodeEnum, Uri>()
			{
				{UserDataResourceSiteCodeEnum.US, new Uri(Resources.Resources.SITE_URL_USERS_US)},
				{UserDataResourceSiteCodeEnum.EU, new Uri(Resources.Resources.SITE_URL_USERS_EU)},
				{UserDataResourceSiteCodeEnum.AS, new Uri(Resources.Resources.SITE_URL_USERS_AS)}
			};

		/// <summary>
		/// Determines which geographic region's REST service use based on provided ISO-3366-1 country code.
		/// </summary>
		/// <param name="countryCode">ISO-3366-1 country code.</param>
		/// <returns>Enum value used to get appropriate REST service for the provided ISO-3366-1 country code. </returns>

		public UserDataResourceSiteCodeEnum? GetUserDataResourceType(string countryCode)
		{
			if (String.IsNullOrEmpty(countryCode))
			{
				throw new NullReferenceException();
			}

			if (countryCode.Length != 2)
			{
				throw new ArgumentOutOfRangeException();
			}

			if (countryCode.Length == 2)
			{
				switch (countryCode.ToUpper())
				{
					case "US":
						return UserDataResourceSiteCodeEnum.US;
					case "GB":
					case "FR":
					case "DE":
						return UserDataResourceSiteCodeEnum.EU;
					case "CN":
						return UserDataResourceSiteCodeEnum.AS;
				}
			}

			return null;
		}

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
		public async Task<IList<T>> RetrieveItemsFromResourceAsync<T>(Uri url, AddressModel inputModel, string jsonRootElementName)
		{
			if (url == null || String.IsNullOrEmpty(jsonRootElementName) ||
			    (inputModel == null || inputModel.Addresses.Count < 1))
			{
				throw new ArgumentNullException("Argument cannot be null.");
			}

			string addressesJSON = JsonConvert.SerializeObject(inputModel);
			IList<T> outputModel;

			using (HttpClient client = new HttpClient())
			{
				StringContent inputContent = new StringContent(addressesJSON, Encoding.Default, "application/json");

				try
				{
					HttpResponseMessage postMessage = await client.PostAsync(url, inputContent);

					using (HttpContent outputContent = postMessage.Content)
					{
						string result = await outputContent.ReadAsStringAsync();
						JObject jsonRoot = (JObject)JsonConvert.DeserializeObject(result);

						JArray itemsJArray = (JArray)jsonRoot[jsonRootElementName];
						outputModel = itemsJArray.ToObject<List<T>>();
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
					throw;
				}
			}

			return outputModel;
		}
	}
}