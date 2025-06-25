using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;

namespace PFO_Web.Controllers
{
    public class AddController : Controller
    {
        [HttpGet]
        public IActionResult Send()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadOfx(IFormFile ofxFile)
        {

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
                        Type = (string)x.Element("TRNTYPE"),
                        Date = (string)x.Element("DTPOSTED"),
                        Amount = decimal.Parse((string)x.Element("TRNAMT")),
                        FitId = (string)x.Element("FITID"),
                        Memo = (string)x.Element("MEMO")
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
            var xmlContent = TempData["OfxString"]?.ToString();
            var xml = XDocument.Parse(xmlContent);

            var transactions = xml.Descendants("STMTTRN")
                .Select(x => new
                {
                    Type = (string)x.Element("TRNTYPE"),
                    Date = (string)x.Element("DTPOSTED"),
                    Amount = decimal.Parse((string)x.Element("TRNAMT")),
                    FitId = (string)x.Element("FITID"),
                    Memo = (string)x.Element("MEMO")
                })
                .ToList();
            ViewBag.Message = $"{transactions.Count} transactions parsed successfully!";
            return View("Process");
        }

    }
}