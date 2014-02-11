﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NuGetGallery.FunctionTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NuGetGallery.FunctionalTests.LoadTests
{
    /// <summary>
    /// This class has the various scenarios used in LoadTests.
    /// The tests does minimal validation and uses existing packages to reduce the execution time spent in test prep and asserts.
    /// </summary>
    [TestClass]
    public class LoadTests
    {
        [TestMethod]
        [Description("Tries to download a packages from v2 feed and make sure the re-direction happens properly.")]
        [Priority(0)]
        public void DownloadPackageSimulationTest()
        {           
            string packageId = "EntityFramework"; //try to down load a pre-defined test package.   
            string version = "5.0.0";
            //Just try download and not actual download. Since this will be used in load test, we don't to actually download the nupkg everytime.
            string redirectUrl = ODataHelper.TryDownloadPackageFromFeed(packageId, version).Result;
            Assert.IsNotNull( redirectUrl, " Package download from V2 feed didnt work");    
            string expectedSubString = "packages/entityframework.5.0.0.nupkg";
            Assert.IsTrue(redirectUrl.Contains(expectedSubString), " The re-direct Url {0} doesnt contain the expect substring {1}",redirectUrl ,expectedSubString); 
        }


        [TestMethod]
        [Description("Tries to simulate the launch of Manage package dialog UI")]
        [Priority(0)]
        public void ManagePackageUILaunchSimulationTest()
        {
            // api/v2/search()/$count query is made everytime Manage package UI is launched in VS.
            //This test simulates the same.
            WebRequest request = WebRequest.Create(UrlHelper.V2FeedRootUrl + @"/Search()/$count?$filter=IsLatestVersion&searchTerm=''&targetFramework='net45'&includePrerelease=false");
            // Get the response.          
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string responseText = sr.ReadToEnd();
            int searchCount = Convert.ToInt32(responseText);
            //Just check if the response is a valid int.
            Assert.IsTrue(searchCount >= 0);
        }

        [TestMethod]
        public void FindPackagesByIdForPredefinedPackage()
        {
            string packageId = "PostSharp";
            WebRequest request = WebRequest.Create(UrlHelper.V2FeedRootUrl + @"/FindPackagesById()?id='" + packageId + "'");
            // Get the response.          
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string responseText = sr.ReadToEnd();
            Assert.IsTrue(responseText.Contains(@"<id>" + UrlHelper.V2FeedRootUrl + "Packages(Id='" + packageId));
        }

        [TestMethod]
        public void FindPackagesBySpecificIdAndVersion()
        {
            string packageId = "Microsoft.Web.Infrastructure";
            string version = "1.0.0.0";
            WebRequest request = WebRequest.Create(UrlHelper.V2FeedRootUrl + @"/api/v2/Packages(Id='" + packageId + "',Version='" + version + "')");
            // Get the response.          
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string responseText = sr.ReadToEnd();
            Assert.IsTrue(responseText.Contains(@"<id>" + UrlHelper.V2FeedRootUrl + "Packages(Id='" + packageId + "',Version='" + version + "')</id>"));
        }

        [TestMethod]
        public void PackagesApiTest()
        {
            string packageId = "newtonsoft.json";            
            WebRequest request = WebRequest.Create(UrlHelper.V2FeedRootUrl + @"/api/v2/Packages()?$filter=tolower(Id) eq '" + packageId + "'&$orderby=Id" );
            // Get the response.          
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string responseText = sr.ReadToEnd();
            Assert.IsTrue(responseText.Contains(@"<id>" + UrlHelper.V2FeedRootUrl + "Packages(Id='" + packageId));
        }
    }
}
