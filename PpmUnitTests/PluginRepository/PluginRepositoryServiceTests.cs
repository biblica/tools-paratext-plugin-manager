﻿/*
Copyright © 2022 by Biblica, Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PpmApp.PluginRepository;
using PpmApp.Util;
using System.Collections.Generic;
using System.IO;

namespace PpmUnitTests
{
    [TestClass]
    public class PluginRepositoryServiceTests
    {
        private const string TEST_PLUGIN_SHORTNAME = "temp";
        private const string TEST_PLUGIN_VERSION_1 = "1.0";
        private const string TEST_PLUGIN_VERSION_2 = "2.0";

        /// <summary>
        /// Mock <c>S3PluginRepositoryService</c> so that we can mock away the S3 functionality.
        /// </summary>
        private Mock<PluginRepositoryService> mockPluginRepositoryService;

        /// <summary>
        /// A Test specific temporary repo service download location.
        /// </summary>
        private readonly DirectoryInfo TestTemporaryDownloadLocation = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "PPMTEST"));

        /// <summary>
        /// Test setup.
        /// </summary>
        [TestInitialize]
        [DeploymentItem(@"Resources", "Resources")]
        public void TestSetup()
        {
            var pluginFilenames = new List<string>()
            {
                $"testplugin-{TEST_PLUGIN_VERSION_1}",
                $"testplugin-{TEST_PLUGIN_VERSION_2}",
            };

            // Mock: PluginRepositoryService
            mockPluginRepositoryService = PluginRepositoryServiceTestsUtil.SetupPluginRepositoryToDownloadTestPlugins(pluginFilenames);
        }

        /// <summary>
        /// This function ensures that the DownloadPlugin function is called as expected. It doesn't actually download a file, as that's specific 
        /// to the S3 interaction. This does validate that it's called as expected.
        /// </summary>
        [TestMethod]
        public void TestDownloadPlugin()
        {
            // Test that the Download plugin works as expected
            // Download the locally available Zip files.
            mockPluginRepositoryService
                .Setup(pluginRepoService =>
                    pluginRepoService.DownloadFile($"testplugin-{TEST_PLUGIN_VERSION_1}.zip", It.IsAny<DirectoryInfo>())
                    );

            var downloadedFilepath = mockPluginRepositoryService.Object.DownloadPlugin(TEST_PLUGIN_SHORTNAME, TEST_PLUGIN_VERSION_1, TestTemporaryDownloadLocation);
        }

        /// <summary>
        /// This functions tests the GetAvailablePlugins method. It first tests that the get the latest version of the plugin works, 
        /// followed by testing the workflow that returns all versions of all plugins.
        /// </summary>
        [TestMethod]
        public void TestGetAvailablePlugins()
        {
            // Test that the Get All Available Plugins path works
            var allPlugins = mockPluginRepositoryService.Object.GetAvailablePlugins(false);

            // Both versions of the temp plugin should be returned.
            Assert.IsNotNull(allPlugins);
            Assert.AreEqual(2, allPlugins.Count);
            Assert.AreEqual(TEST_PLUGIN_SHORTNAME, allPlugins[0].ShortName);
            Assert.AreEqual(TEST_PLUGIN_VERSION_1, allPlugins[0].Version);
            Assert.AreEqual(TEST_PLUGIN_SHORTNAME, allPlugins[1].ShortName);
            Assert.AreEqual(TEST_PLUGIN_VERSION_2, allPlugins[1].Version);
        }
    }
}
