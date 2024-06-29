using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;


public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public string Currency { get; set; }

    public Transaction(int id, decimal amount, DateTime date, string description, string currency)
    {
        Id = id;
        Amount = amount;
        Date = date;
        Description = description;
        Currency = currency;
    }

    public override string ToString()
    {
        string idColumn = $"|  {Id}  ".PadRight(5);
        string amountColumn = $"|  {Amount} {Currency} ".PadRight(15);
        string dateColumn = $"|  {Date.ToShortDateString()}".PadRight(15);
        string descriptionColumn = $"|  {Description}     |".PadRight(30);

        return $"\n========================================================\n" +
               $"|  ID: |     Amount:   |     Date:     |  Description: |\n" +
               $"-------|---------------|---------------|---------------|\n" +
               $"{idColumn} {amountColumn} {dateColumn} {descriptionColumn}  \n" +
               $"========================================================";
    }
}

public class FinancialManager
{
    private List<Transaction> transactions = new List<Transaction>();
    private int nextId = 1;
    private readonly string filePath = @"D:\transactions.txt";
    private string currentCurrency = "UAH";

    public FinancialManager()
    {
        LoadTransactions();
    }

    public void AddTransaction()
    {
        decimal amount;
        while (true)
        {
            Console.Write("Введите сумму: ");
            try
            {
                amount = Convert.ToDecimal(Console.ReadLine(), CultureInfo.InvariantCulture);
                break;
            }
            catch (FormatException)
            {
                Console.WriteLine("Недопустимая сумма. Пожалуйста, введите допустимое десятичное число.");
            }
        }

        DateTime date;
        while (true)
        {
            Console.Write("Введите дату (MM.DD.YYYY): ");
            try
            {
                date = DateTime.ParseExact(Console.ReadLine(), "MM.dd.yyyy", CultureInfo.InvariantCulture);
                break;
            }
            catch (FormatException)
            {
                Console.WriteLine("Неверный формат даты. Введите дату в формате MM.DD.YYYY.");
            }
        }

        Console.Write("Введите описание: ");
        string? description = Console.ReadLine();

        Transaction transaction = new Transaction(nextId++, amount, date, description, currentCurrency);
        transactions.Add(transaction);
        SaveTransactions();
    }

    public void DeleteTransaction(int id)
    {
        Transaction? transactionToRemove = transactions.Find(t => t.Id == id);
        if (transactionToRemove != null)
        {
            transactions.Remove(transactionToRemove);
            SaveTransactions();
            Console.WriteLine($"Транзакция с ID {id} удалена.");
        }
        else
        {
            Console.WriteLine("Транзакция не найдна.");
        }
    }

    public void DisplayTransactions()
    {
        foreach (var transaction in transactions)
        {
            Console.WriteLine(transaction);
        }
    }

    public decimal GetBalance()
    {
        decimal balance = 0;
        foreach (var transaction in transactions)
        {
            balance += transaction.Amount;
        }
        return balance;
    }

    public void SetCurrency(string currency)
    {
        currentCurrency = currency;
    }

    public string GetCurrency()
    {
        return currentCurrency;
    }

    private void SaveTransactions()
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var transaction in transactions)
                {
                    string line = $"{transaction.Id},{transaction.Amount},{transaction.Date:MM.dd.yyyy},{transaction.Description},{transaction.Currency}";
                    writer.WriteLine(line);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка при сохранении транзакции: {ex.Message}");
        }
    }

    private void LoadTransactions()
    {
        try
        {
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split(',');

                        if (parts.Length == 5)
                        {
                            int id = int.Parse(parts[0]);
                            decimal amount = decimal.Parse(parts[1], CultureInfo.InvariantCulture);
                            DateTime date = DateTime.ParseExact(parts[2], "MM.dd.yyyy", CultureInfo.InvariantCulture);
                            string description = parts[3];
                            string currency = parts[4];

                            Transaction transaction = new Transaction(id, amount, date, description, currency);
                            transactions.Add(transaction);
                        }
                    }

                    if (transactions.Count > 0)
                    {
                        nextId = transactions[^1].Id + 1;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка при загрузке транзакции: {ex.Message}");
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.Title = "Менеджер финансов";

        FinancialManager manager = new FinancialManager();

        while (true)
        {
            Console.WriteLine("\nМеню:\n");
            Console.WriteLine("1. Добавить транзакцию");
            Console.WriteLine("2. Показать транзакции");
            Console.WriteLine("3. Показать баланс");
            Console.WriteLine("4. Установить валюту");
            Console.WriteLine("5. Удалить транзакцию");
            Console.WriteLine("6. Выход\n");
            Console.Write("Выберите действие: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    manager.AddTransaction();
                    break;

                case "2":
                    manager.DisplayTransactions();
                    break;

                case "3":
                    Console.Write("\n====================================\n");
                    Console.WriteLine($"| Текущий баланс: {manager.GetBalance()} {manager.GetCurrency()}        |");
                    Console.Write("====================================\n");
                    break;

                case "4":
                    Console.Write("Введите новую валюту (e.g., USD, EUR, UAH): ");
                    string newCurrency = Console.ReadLine().ToUpper();
                    manager.SetCurrency(newCurrency);
                    Console.WriteLine($"Установлена валюта {manager.GetCurrency()}");
                    break;

                case "5":
                    int idToDelete;
                    while (true)
                    {
                        Console.Write("Выберите ID транзакции для удаления: ");
                        try
                        {
                            idToDelete = Convert.ToInt32(Console.ReadLine());
                            break;
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Неверное ID. Пожалуйста, введите допустимое целое число ID.");
                        }
                        catch (OverflowException)
                        {
                            Console.WriteLine("Число слишком велико. Пожалуйста, введите допустимое целое число ID.");
                        }
                    }
                    manager.DeleteTransaction(idToDelete);
                    break;

                case "6":
                    return;
                default:
                    Console.WriteLine("Неверный выбор. Попробуйте еще раз.");
                    break;
            }
        }
    }
}
