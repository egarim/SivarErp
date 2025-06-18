// 3. CSV Import Service for Test Data
public class TestDataImportService
{
    private readonly string _dataDirectory;

    public TestDataImportService(string dataDirectory)
    {
        _dataDirectory = dataDirectory;
    }

    public TestScenarioConfig LoadTestScenario(string scenarioId)
    {
        var csvPath = Path.Combine(_dataDirectory, "TestScenarios.csv");
        var lines = File.ReadAllLines(csvPath);

        var headers = lines[0].Split(',');

        foreach (var line in lines.Skip(1))
        {
            var fields = line.Split(',');
            if (fields[0] == scenarioId)
            {
                return new TestScenarioConfig
                {
                    TestScenarioId = fields[0],
                    Description = fields[1],
                    DocumentType = fields[2],
                    DocumentNumber = fields[3],
                    BusinessEntityCode = fields[4],
                    Date = fields[5]
                };
            }
        }

        throw new ArgumentException($"Test scenario '{scenarioId}' not found");
    }

    public List<TestDocumentLine> LoadDocumentLines(string scenarioId)
    {
        var csvPath = Path.Combine(_dataDirectory, "TestDocumentLines.csv");
        var lines = File.ReadAllLines(csvPath);
        var result = new List<TestDocumentLine>();

        foreach (var line in lines.Skip(1)) // Skip header
        {
            var fields = line.Split(',');
            if (fields[0] == scenarioId)
            {
                result.Add(new TestDocumentLine
                {
                    TestScenarioId = fields[0],
                    LineNumber = int.Parse(fields[1]),
                    ItemCode = fields[2],
                    Quantity = decimal.Parse(fields[3]),
                    UnitPrice = decimal.Parse(fields[4]),
                    Amount = decimal.Parse(fields[5])
                });
            }
        }

        return result.OrderBy(x => x.LineNumber).ToList();
    }

    public Dictionary<string, string> LoadAccountMappings()
    {
        var csvPath = Path.Combine(_dataDirectory, "TestAccountMappings.csv");
        var lines = File.ReadAllLines(csvPath);
        var result = new Dictionary<string, string>();

        foreach (var line in lines.Skip(1)) // Skip header
        {
            var fields = line.Split(',');
            result[fields[0]] = fields[1]; // LogicalName -> AccountCode
        }

        return result;
    }

    public List<TestInitialTransaction> LoadInitialTransactions(string scenarioId)
    {
        var csvPath = Path.Combine(_dataDirectory, "TestInitialTransactions.csv");
        var lines = File.ReadAllLines(csvPath);
        var result = new List<TestInitialTransaction>();

        foreach (var line in lines.Skip(1)) // Skip header
        {
            var fields = line.Split(',');
            if (fields[0] == scenarioId)
            {
                result.Add(new TestInitialTransaction
                {
                    TestScenarioId = fields[0],
                    TransactionType = fields[1],
                    Description = fields[2],
                    DebitAccount = fields[3],
                    CreditAccount = fields[4],
                    Amount = decimal.Parse(fields[5])
                });
            }
        }

        return result;
    }
}
