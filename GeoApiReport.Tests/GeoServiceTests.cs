using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeoApiReport.Core.Enums;
using NUnit.Framework;
using GeoApiReport.Core.Models;
using GeoApiReport.Core.Services;

namespace GeoApiReport.Tests
{
	[TestFixture]
	public sealed class GeoServiceTests
	{
		private IGeoService m_geoSvc;

		[SetUp]
		public void SetUp()
		{
			m_geoSvc = new GeoService();
		}

		[Test]
		public async Task GetRestDataFromService_RetrieveItems()
		{
			AddressModel inputModel = new AddressModel()
			{
				Addresses = new List<string>() {"1.32.232.0", "4.15.16.2"}
			};

			IList<GeolocationModel> expectedModel = new List<GeolocationModel>()
			{
				new GeolocationModel() {Address = "1.32.232.0", Country = "US"},
				new GeolocationModel() {Address = "4.15.16.2", Country = "US"}
			};

			var resultModel = await m_geoSvc.RetrieveItemsFromResourceAsync<GeolocationModel>(
				m_geoSvc.GeolocationResource, inputModel, "geolocations");

			for (int i = 0; i < expectedModel.Count; i++)
			{
				Assert.AreEqual(expectedModel[i].Address, resultModel[i].Address);
				Assert.AreEqual(expectedModel[i].Country, resultModel[i].Country);
			}
		}

		[Test]
		public async Task GetRestDataFromService_FailToRetrieve()
		{
			AddressModel inputModel = null;

			IList<GeolocationModel> expectedModel = new List<GeolocationModel>();

			Assert.That(async () => await m_geoSvc.RetrieveItemsFromResourceAsync<GeolocationModel>(
					m_geoSvc.GeolocationResource, inputModel, "geolocations"),
				Throws.ArgumentNullException);
		}

		[Test]
		public void GetUserDataResourceType_Success()
		{
			string countryCode = "DE"; 
			UserDataResourceSiteCodeEnum? expectedResult = UserDataResourceSiteCodeEnum.EU;

			UserDataResourceSiteCodeEnum? returnedResult = m_geoSvc.GetUserDataResourceType(countryCode);

			Assert.AreEqual(expectedResult.Value, returnedResult.Value);
		}

		[Test]
		public void GetUserDataResourceType_Failed()
		{
			string countryCode = "CA";
			
			UserDataResourceSiteCodeEnum? returnedResult = m_geoSvc.GetUserDataResourceType(countryCode);

			Assert.AreEqual(returnedResult, null);
		}
	}
}