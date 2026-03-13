using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace канбанчик
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Глобальная обработка исключений
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var ex = (Exception)args.ExceptionObject;
                MessageBox.Show($"Критическая ошибка: {ex.Message}\n\nПриложение будет закрыто.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                // Здесь можно добавить логирование
                string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: {ex}\n");

                Current.Shutdown();
            };

            // Обработка исключений в UI потоке
            DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show($"Ошибка интерфейса: {args.Exception.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true; // Предотвращаем закрытие приложения
            };

            System.Diagnostics.Debug.WriteLine("=== WPF ПРИЛОЖЕНИЕ ЗАПУЩЕНО ===");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            System.Diagnostics.Debug.WriteLine($"=== ПРИЛОЖЕНИЕ ЗАВЕРШЕНО С КОДОМ {e.ApplicationExitCode} ===");
        }
    }
}