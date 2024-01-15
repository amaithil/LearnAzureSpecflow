using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SpecFlowProject1.StepDefinitions
{
    [Binding]
    public sealed class TestBlobAzureStepDefinitions
    {
        static string basePath = AppDomain.CurrentDomain.BaseDirectory.Remove(AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin"));
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef
        [Given(@"an user navigated to google web page")]
        public async Task GivenAnUserNavigatedToGoogleWebPageAsync()
        {
            IWebDriver driver = new ChromeDriver();
            Console.WriteLine("Hello World");
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=azpractstorage;AccountKey=EhKpUJMe1s0xzBZ868dXST6SeP2ZtXxzkPM6bbS8X2u75pDitTvuIxR9A48mEGU11R6/bxsFrhXb+ASt+JyDrA==;EndpointSuffix=core.windows.net";
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("newblobcontainer");
            BlockBlobClient blockBlobClient = containerClient.GetBlockBlobClient("TestData.csv");
            Console.WriteLine("Connection built to azure");
            try
            {
                var response = blockBlobClient.DownloadContent();
                using (var reader = new StreamReader(response.Value.Content.ToStream()))
                {   
                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();
                        // You can parse the CSV line using a CSV parsing library or split by commas
                        string[] values = line.Split(';');
                        driver.Manage().Window.Maximize();
                        driver.Navigate().GoToUrl(values[0]);
                        driver.FindElement(By.XPath("//textarea[@name='q']")).SendKeys(values[1]);
                        driver.FindElement(By.XPath("//textarea[@name='q']")).Submit();
                        string subdir = basePath + "\\Screenshots\\";
                        Screenshot screenshot = (driver as ITakesScreenshot).GetScreenshot();
                        string localFilePath = subdir + "screenshot" + values[1] + ".png";
                        screenshot.SaveAsFile(localFilePath);
                        string fileName = Path.GetFileName(localFilePath);
                        BlobClient blobClient = containerClient.GetBlobClient(fileName);
                        await blobClient.UploadAsync(localFilePath, true);
                    }
                }               
                Console.WriteLine("TestData.csv exists");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                // Quit the driver outside the loop
                driver.Quit();
            }
        }

        [When(@"the user searchs value")]
        public void WhenTheUserSearchsValue()
        {
        }

        [Then(@"the searched value result is displayed")]
        public void ThenTheSearchedValueResultIsDisplayed()
        {
        }
    }
}
