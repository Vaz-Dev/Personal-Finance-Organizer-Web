using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PFO_Web.Services;
using PFO_Web.Models;

namespace PFO_Web.Controllers
{
    public class AddController : Controller
    {

        private readonly XmlService _xmlService = new();

        [HttpGet]
        public IActionResult Send()
        {
            var data = _xmlService.Load();
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> UploadOfx(IFormFile ofxFile)
        {
            var data = _xmlService.Load();

            if (ofxFile == null || ofxFile.Length == 0)
                return BadRequest("No file uploaded.");

            using (var reader = new StreamReader(ofxFile.OpenReadStream()))
            {
                string? line;
                var xmlLines = new List<string>();
                bool ofxStarted = false;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (ofxStarted)
                    {
                        xmlLines.Add(line);
                    }
                    else if (line.TrimStart().StartsWith("<OFX>"))
                    {
                        ofxStarted = true;
                        xmlLines.Add(line);
                    }
                }

                if (!ofxStarted)
                    return BadRequest("No <OFX> tag found in file.");

                var xmlContent = string.Join(Environment.NewLine, xmlLines);
                var xml = XDocument.Parse(xmlContent);

                var transactions = xml.Descendants("STMTTRN")
                    .Select(x => new
                    {
                        Date = DateTime.ParseExact(((string)x.Element("DTPOSTED") ?? "").Substring(0, ((string)x.Element("DTPOSTED") ?? "").Length - 14),
                        "yyyyMMdd",
                        CultureInfo.InvariantCulture),
                        Amount = decimal.Parse((string)x.Element("TRNAMT"), CultureInfo.InvariantCulture),
                        FitId = (string)x.Element("FITID"),
                        Meta = (string)x.Element("MEMO")
                    })
                    .ToList();

                try
                {
                    TempData["OfxString"] = xmlContent;
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error processing transactions: {ex.Message}");
                }
                ViewBag.Message = $"{transactions.Count} transactions parsed successfully!";
                
                return View("Send", data);
            }
        }

        [HttpPost]

        public IActionResult Process()
        {
            var xmlContent = TempData["OfxString"]?.ToString();
            var xml = XDocument.Parse(xmlContent);
            var data = _xmlService.Load();

            var transactions = xml.Descendants("STMTTRN")
                .Select(x => new
                {
                    Date = DateOnly.ParseExact(((string)x.Element("DTPOSTED") ?? "").Substring(0, ((string)x.Element("DTPOSTED") ?? "").Length - 14),
                    "yyyyMMdd", 
                    CultureInfo.InvariantCulture),
                    Amount = decimal.Parse((string)x.Element("TRNAMT"), CultureInfo.InvariantCulture),
                    FitId = (string)x.Element("FITID"),
                    Meta = (string)x.Element("MEMO")
                })
                .ToList();
            ViewBag.Message = $"{transactions.Count} transactions parsed successfully!";
            ViewBag.Transactions = transactions;
            return View(data);
        }

        [HttpPost]

        public IActionResult processCategories(List<Transaction> transactions)
        {
            var data = _xmlService.Load();

            return View("Process", data);
        }

    }
}