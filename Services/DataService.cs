using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;
using PFO_Web.Models;

namespace PFO_Web.Services
{
    public class DataService
    {

        private readonly string _filePath;


        public DataService()

        {

            var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PFO_Web"
            );
            Directory.CreateDirectory(appDataPath);
            _filePath = Path.Combine(appDataPath, "data.xml");

        }
        public Data Load()
        {
            if (!File.Exists(_filePath))
            {
                var doc = new XDocument(
                    new XElement("data",
                        new XElement("categories",
                            new XElement("category",
                                new XElement("id", 1),
                                new XElement("name", "Other: Expense"),
                                new XElement("transactionType", TransactionType.Expense.ToString())
                            ),
                            new XElement("category",
                                new XElement("id", 2),
                                new XElement("name", "Other: Income"),
                                new XElement("transactionType", TransactionType.Income.ToString())
                            )
                        ),
                        new XElement("transactions")
                    )
                );
                doc.Save(_filePath);
                var data = new Data();
                return data;

            }
            else
            {
                var doc = XDocument.Load(_filePath);
                var data = new Data();
                data.Categories = doc.Descendants("category")
                    .Select(c => new Category
                    {
                        Id = (int)c.Element("id"),
                        Name = (string)c.Element("name"),
                        Type = (TransactionType)Enum.Parse(typeof(TransactionType), (string)c.Element("transactionType"))
                    }
                    )
                    .ToList();
                data.Transactions = doc.Descendants("transaction")
                    .Select(t => new Transaction
                    {
                        Id = (string)t.Element("id"),
                        Date = DateOnly.ParseExact(((string)t.Element("date")), "yyyyMMdd", null),
                        Amount = decimal.Parse((string)t.Element("amount")),
                        Meta = (string)t.Element("meta"),
                        Category = data.Categories.FirstOrDefault(c => c.Id == (int)t.Element("categoryId"))
                    }
                    )
                    .ToList();
                return data;
            }
        }
        public void Save(Data data)
        {
            var doc = new XDocument(
                new XElement("data",
                    new XElement("categories"),
                    new XElement("transactions")
                )
            );
            doc.Root.Element("categories").Add(
                data.Categories.Select(c => new XElement("category",
                    new XElement("id", c.Id),
                    new XElement("name", c.Name),
                    new XElement("transactionType", c.Type.ToString())
                ))
            );
            doc.Root.Element("transactions").Add(
                data.Transactions.Select(t => new XElement("transaction",
                    new XElement("id", t.Id),
                    new XElement("date", t.Date.ToString("yyyyMMdd")),
                    new XElement("amount", t.Amount),
                    new XElement("meta", t.Meta),
                    new XElement("categoryId", t.Category?.Id ?? 0)
                ))
            );
            doc.Save(_filePath);
        }

        public List<Transaction> Find(Data data, DateOnly initialDate, DateOnly lastDate)
        {
            var matchedTransactions = data.Transactions.Where(t => t.Date >= initialDate && t.Date <= lastDate)
                .ToList();
            return matchedTransactions;
        }

        public void UpdateTransactions(Data data, List<Transaction> newData)
        {
            foreach (var transaction in newData)
            {
                var existingTransaction = data.Transactions.FirstOrDefault(t => t.Id == transaction.Id);
                if (existingTransaction != null)
                {
                    existingTransaction.Date = transaction.Date;
                    existingTransaction.Amount = transaction.Amount;
                    existingTransaction.Meta = transaction.Meta;
                    existingTransaction.Category = transaction.Category;
                }
            }
            Save(data);
        }
    }
}

