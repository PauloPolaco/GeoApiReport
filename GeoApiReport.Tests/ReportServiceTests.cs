using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GeoApiReport.Core.Models;
using GeoApiReport.Core.Services;
using NUnit.Framework;

namespace GeoApiReport.Tests
{
	[TestFixture]
	public sealed class ReportServiceTests
	{
		private IReportService m_reportSvc;

		[SetUp]
		public void SetUp()
		{
			m_reportSvc = new ReportService();
		}

		[Test]
		public async Task InputFile_CanReadWithResults()
		{
			AddressModel addressModel = null;

			addressModel = await m_reportSvc.RetrieveIPAddressListFromFileAsync();

			Assert.That(addressModel, Is.Not.Null);
			Assert.That(addressModel.Addresses.Count, Is.GreaterThanOrEqualTo(1));
		}

		[Test]
		public async Task OutputFile_CanWriteToDisk()
		{
			IList<UserModel> reportModel = new List<UserModel>()
			{
				new UserModel()
				{
					Name = "John Doe",
					Country = "CA",
					Address = "127.0.0.1",
					Gender = "unknown",
					Age = "18"
				}
			};

			string filename = await m_reportSvc.GenerateReportAsCsvAsync(reportModel);

			Assert.That(filename, Is.Not.Null);
			Assert.That(filename.Length, Is.GreaterThanOrEqualTo(1));
		}

		[Test]
		public async Task OutputFile_FailedToWrite()
		{
			IList<UserModel> reportModel = null;

			string filename = await m_reportSvc.GenerateReportAsCsvAsync(reportModel);

			Assert.That(filename, Is.Null);
		}
	}
}