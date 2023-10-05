using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Net;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        using var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8001/");
        listener.Start();
        Console.WriteLine("Listening on port 8001...");

        IWebDriver driver = new ChromeDriver(new ChromeOptions { PageLoadStrategy = PageLoadStrategy.None, });
        IOptions options = driver.Manage();
        options.Window.Size = new System.Drawing.Size(0, 0);
        options.Window.Minimize();

        while (true)
        {
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            Console.WriteLine($"Received request for {request.Url}");
            using HttpListenerResponse response = context.Response;

            var data = string.Empty;
            if (request.UrlReferrer != null)
            {
                driver.Url = $"{request.UrlReferrer}";
                while (string.IsNullOrEmpty(data))
                {
                    Console.WriteLine("Waiting for PageSource");
                    Thread.Sleep(100);
                    data = driver.PageSource;
                }
            }
            else
            {
                data = "ERORR: UrlReferrer is empty";
            }

            byte[] buffer = Encoding.UTF8.GetBytes(data);
            response.ContentLength64 = buffer.Length;

            using Stream outputStream = response.OutputStream;
            outputStream.Write(buffer, 0, buffer.Length);
        }
    }
}