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

        private readonly DataService _dataService = new();

        [HttpGet]
        public IActionResult Send()
        {
            var data = _dataService.Load();
            ViewBag.Data = data;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadOfx(IFormFile ofxFile)
        {
            var data = _dataService.Load();
            ViewBag.Data = data;

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
                        Date = DateOnly.ParseExact(((string)x.Element("DTPOSTED") ?? "").Substring(0, ((string)x.Element("DTPOSTED") ?? "").Length - 14),
                        "yyyyMMdd",
                        CultureInfo.InvariantCulture),
                        Amount = decimal.Parse((string)x.Element("TRNAMT"), CultureInfo.InvariantCulture),
                        Id = (string)x.Element("FITID"),
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
                
                return View("Send");
            }
        }

        [HttpPost]

        public IActionResult Process()
        {
            var data = _dataService.Load();
            ViewBag.Data = data;
            var xmlContent = TempData["OfxString"]?.ToString();
            var xml = XDocument.Parse(xmlContent);

            var transactions = xml.Descendants("STMTTRN")
                .Select(x => new
                {
                    Date = DateOnly.ParseExact(((string)x.Element("DTPOSTED") ?? "").Substring(0, ((string)x.Element("DTPOSTED") ?? "").Length - 14),
                    "yyyyMMdd", 
                    CultureInfo.InvariantCulture),
                    Amount = decimal.Parse((string)x.Element("TRNAMT"), CultureInfo.InvariantCulture),
                    Id = (string)x.Element("FITID"),
                    Meta = (string)x.Element("MEMO")
                })
                .ToList();
            ViewBag.Message = $"{transactions.Count} transactions parsed successfully!";
            TempData["OfxString"] = xmlContent;
            return View(transactions);
        }

        [HttpPost]

        public IActionResult processCategories(List<Transaction> transactionsCategorized)
        {
            var data = _dataService.Load();
            ViewBag.Data = data;
            var xmlContent = TempData["OfxString"]?.ToString();
            var xml = XDocument.Parse(xmlContent);

            var transactions = xml.Descendants("STMTTRN")
                .Select(x => new
                {
                    Date = DateOnly.ParseExact(((string)x.Element("DTPOSTED") ?? "").Substring(0, ((string)x.Element("DTPOSTED") ?? "").Length - 14),
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture),
                    Amount = decimal.Parse((string)x.Element("TRNAMT"), CultureInfo.InvariantCulture),
                    Id = (string)x.Element("FITID"),
                    Meta = (string)x.Element("MEMO")
                } )
                .ToList();

            if (transactionsCategorized.Any(tc => data.Transactions.Any(dt => dt.Id == tc.Id)))
            {
                ViewBag.Alert = "Some transactions already exist in the database.";
                return View("Send", transactions);
            }
            else if (data.Transactions.Any()) {
                data.Transactions.AddRange(transactionsCategorized);
                _dataService.Save(data);
            }
            else
            {
                data.Transactions = transactionsCategorized;
                _dataService.Save(data);
            }

                return View("Send", transactions);
                
        }

    }
}