using GymClient.Models;
using GymClient.Utils;
using System.Globalization;
using System.IO;

namespace GymClient.Services
{
    public class CsvImportService
    {
        private readonly ApiClient _apiClient;

        public CsvImportService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public static readonly string[] DateFormats =
        {
            "d/M/yyyy",
            "dd/MM/yyyy",
            "yyyy/MM/dd",

            "d-M-yyyy",
            "dd-MM-yyyy",
            "yyyy-MM-dd",

            "d.MM.yyyy",
            "dd.MM.yyyy",
            "yyyy.MM.dd",
        };

        public async Task ImportMembersFromDialog()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Выберите CSV файл"
            };

            if (dialog.ShowDialog() != true)
                return;

            await ImportMembers(dialog.FileName);
        }

        public async Task ImportMembers(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    return;
                }

                var lines = await File.ReadAllLinesAsync(filePath);

                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split(',');
                    if (parts.Length < 5) continue;

                    var firstName = parts[0].Trim();
                    var lastName = parts[1].Trim();
                    var dateOfBirthStr = parts[2].Trim();
                    var email = parts[3].Trim();
                    var phone = parts[4].Trim();


                    if (string.IsNullOrWhiteSpace(firstName) ||
                            string.IsNullOrWhiteSpace(lastName) ||
                            string.IsNullOrWhiteSpace(dateOfBirthStr) ||
                            string.IsNullOrWhiteSpace(email) ||
                            string.IsNullOrWhiteSpace(phone))
                        continue;

                    if (!DateOnly.TryParseExact(dateOfBirthStr, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfBirth))
                    {
                        Console.WriteLine($"Неверная дата в CSV: {dateOfBirthStr}");
                        continue;
                    }

                    var member = new Member
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        DateOfBirth = dateOfBirth,
                        Email = email,
                        PhoneNumber = phone,
                        RegistrationDate = DateTime.Now
                    };
                    
                    await _apiClient.Post(member);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка импорта CSV: {ex.Message}");
                throw;
            }
        }
    }
}
