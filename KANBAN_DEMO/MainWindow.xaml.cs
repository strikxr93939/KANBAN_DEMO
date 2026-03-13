using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace канбанчик
{
    public partial class MainWindow : Window
    {
        private KanbanCardData _currentCardData;
        private int _previewCounter = 0;
        private bool _isPreviewing = false;
        private DateTime _lastPreviewTime = DateTime.MinValue;
        private bool _isPrinting = false;
        private DateTime _lastPrintTime = DateTime.MinValue;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                LogMessage("=== КАНБАН ДЕМО (ЗАВОД N) ЗАПУЩЕН ===");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogMessage("Окно загружено");
            UpdateButtonState();
        }

        private void LogMessage(string message)
        {
            string logMessage = $"{DateTime.Now:HH:mm:ss.fff} - {message}";
            Debug.WriteLine(logMessage);
            try
            {
                string logPath = global::System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kanban_demo.log");
                File.AppendAllText(logPath, logMessage + Environment.NewLine);
            }
            catch { }
        }

        private void UpdateButtonState()
        {
            bool isValid = !string.IsNullOrEmpty(txtJob?.Text) &&
                          !string.IsNullOrEmpty(txtSuffix?.Text);

            if (btnPreview != null)
                btnPreview.IsEnabled = isValid && !_isPreviewing;
        }

        private void txtJob_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateButtonState();
            if (!string.IsNullOrEmpty(txtJob?.Text))
                LogMessage($"ЗНП изменен: '{txtJob.Text}'");
        }

        private void txtSuffix_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateButtonState();
            if (!string.IsNullOrEmpty(txtSuffix?.Text))
                LogMessage($"Суффикс изменен: '{txtSuffix.Text}'");
        }

        private void txtPages_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(txtPages?.Text, out int pages) && pages > 0)
            {
                // OK
            }
            else
            {
                txtPages.Text = "1";
            }
        }

        private void chkA6_CheckedChanged(object sender, RoutedEventArgs e)
        {
            LogMessage($"Формат A6: {(chkA6.IsChecked == true ? "включен" : "выключен")}");
        }

        private async void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            if (_isPreviewing)
            {
                LogMessage("ПРЕДПРОСМОТР УЖЕ ВЫПОЛНЯЕТСЯ - пропускаем");
                return;
            }

            if ((DateTime.Now - _lastPreviewTime).TotalMilliseconds < 1000)
            {
                LogMessage("СЛИШКОМ ЧАСТЫЙ КЛИК - пропускаем");
                _lastPreviewTime = DateTime.Now;
                return;
            }

            try
            {
                _isPreviewing = true;
                _lastPreviewTime = DateTime.Now;
                btnPreview.IsEnabled = false;
                btnPrint.IsEnabled = false;

                _previewCounter++;
                LogMessage($"=== НАЧАЛО ПРЕДПРОСМОТРА #{_previewCounter} ===");

                string job = txtJob.Text.Trim();
                string suffix = txtSuffix.Text.Trim();
                int pages = int.TryParse(txtPages.Text, out int p) ? p : 1;

                if (pages < 1) pages = 1;

                // Имитация загрузки
                await System.Threading.Tasks.Task.Delay(500);

                // ДЕМО-ДАННЫЕ ДЛЯ ВЫМЫШЛЕННОГО ЗАВОДА N
                _currentCardData = new KanbanCardData
                {
                    JobNumber = job,
                    Suffix = suffix,
                    Pages = pages,
                    IsA6Format = chkA6.IsChecked ?? false,
                    Item = "N-001",
                    ItemDescription = "Деталь №1",
                    StartDate = DateTime.Today.AddDays(-2),
                    EndDate = DateTime.Today.AddDays(3),
                    Storage = "Склад",
                    RealisedQuantity = 100,
                    CompleteQuantity = 45,
                    Lot = $"LOT-{new Random().Next(100, 999)}",
                    Status = "Запущено",
                    Logo = "Завод N",
                    Plata = "Позиция 1",
                    Folder = "A-12",
                    Color = "Красный",
                    FIFO = 0
                };

                // ОТРИСОВКА КАРТОЧКИ
                DrawKanbanCard();
                UpdateStatusPanel();

                btnPrint.IsEnabled = true;

                LogMessage($"Предпросмотр #{_previewCounter} создан");
            }
            catch (Exception ex)
            {
                LogMessage($"ОШИБКА: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isPreviewing = false;
                UpdateButtonState();
            }
        }

        private void DrawKanbanCard()
        {
            if (_currentCardData == null) return;

            var border = new Border
            {
                Width = 600,
                Height = 700,
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(10),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 10,
                    Opacity = 0.1,
                    Direction = 270,
                    ShadowDepth = 2
                }
            };

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) }); // Шапка
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(200) }); // Основная инфа
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) }); // Данные
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) }); // Штрих-код
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(180) }); // Операции
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) }); // Футер
            mainGrid.Margin = new Thickness(10);

            // ШАПКА
            var headerBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(52, 73, 94)),
                CornerRadius = new CornerRadius(8, 8, 0, 0),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titleBlock = new TextBlock
            {
                Text = "КАРТОЧКА КАНБАН",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 0, 0)
            };
            Grid.SetColumn(titleBlock, 0);
            headerGrid.Children.Add(titleBlock);

            var idBlock = new TextBlock
            {
                Text = $"#{_currentCardData.JobNumber}-{_currentCardData.Suffix}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 15, 0)
            };
            Grid.SetColumn(idBlock, 1);
            headerGrid.Children.Add(idBlock);

            headerBorder.Child = headerGrid;
            Grid.SetRow(headerBorder, 0);
            mainGrid.Children.Add(headerBorder);

            // ОСНОВНАЯ ИНФОРМАЦИЯ
            var infoGrid = new Grid();
            infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Левая колонка
            var leftPanel = new StackPanel { Margin = new Thickness(5) };
            leftPanel.Children.Add(CreateInfoRow("Изделие:", _currentCardData.Item));
            leftPanel.Children.Add(CreateInfoRow("Описание:", _currentCardData.ItemDescription));
            leftPanel.Children.Add(CreateInfoRow("Партия:", _currentCardData.Lot));
            leftPanel.Children.Add(CreateInfoRow("Склад:", _currentCardData.Storage));
            leftPanel.Children.Add(CreateInfoRow("Логотип:", _currentCardData.Logo));

            // Правая колонка
            var rightPanel = new StackPanel { Margin = new Thickness(5) };

            // Статус
            rightPanel.Children.Add(CreateStatusBlock("Статус", _currentCardData.Status,
                _currentCardData.Status == "Запущено" ? Colors.Green : Colors.Orange));

            // Количество
            var qtyPanel = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
            qtyPanel.Children.Add(CreateSimpleBlock("Запущено", _currentCardData.RealisedQuantity.ToString()));
            qtyPanel.Children.Add(CreateSimpleBlock("Завершено", _currentCardData.CompleteQuantity.ToString()));
            rightPanel.Children.Add(qtyPanel);

            // Даты
            var datePanel = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };
            datePanel.Children.Add(new TextBlock
            {
                Text = $"Начало: {_currentCardData.StartDate:dd.MM.yyyy}",
                FontSize = 11,
                Foreground = Brushes.Gray
            });
            datePanel.Children.Add(new TextBlock
            {
                Text = $"Окончание: {_currentCardData.EndDate:dd.MM.yyyy}",
                FontSize = 11,
                Foreground = Brushes.Gray
            });
            rightPanel.Children.Add(datePanel);

            Grid.SetColumn(leftPanel, 0);
            Grid.SetColumn(rightPanel, 1);
            infoGrid.Children.Add(leftPanel);
            infoGrid.Children.Add(rightPanel);

            Grid.SetRow(infoGrid, 1);
            mainGrid.Children.Add(infoGrid);

            // ДОПОЛНИТЕЛЬНЫЕ ДАННЫЕ
            var dataBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 250)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10),
                Margin = new Thickness(5, 0, 5, 5)
            };

            var dataPanel = new WrapPanel();
            dataPanel.Children.Add(CreateDataChip("Папка", _currentCardData.Folder));
            dataPanel.Children.Add(CreateDataChip("Цвет", _currentCardData.Color));
            dataPanel.Children.Add(CreateDataChip("Позиция", _currentCardData.Plata));
            dataPanel.Children.Add(CreateDataChip("FIFO", _currentCardData.FIFO == 0 ? "—" : _currentCardData.FIFO.ToString("D4")));

            dataBorder.Child = dataPanel;
            Grid.SetRow(dataBorder, 2);
            mainGrid.Children.Add(dataBorder);

            // ШТРИХ-КОД
            var barcodeBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(5),
                Padding = new Thickness(10)
            };

            var barcodePanel = new StackPanel { Orientation = Orientation.Horizontal };

            // Простая имитация штрих-кода
            var barcodeCanvas = new Canvas { Width = 150, Height = 40, Margin = new Thickness(0, 0, 15, 0) };
            for (int i = 0; i < 20; i++)
            {
                var rect = new Rectangle
                {
                    Width = new Random().Next(2, 5),
                    Height = 30,
                    Fill = Brushes.Black
                };
                Canvas.SetLeft(rect, i * 7);
                Canvas.SetTop(rect, 5);
                barcodeCanvas.Children.Add(rect);
            }
            barcodePanel.Children.Add(barcodeCanvas);

            barcodePanel.Children.Add(new TextBlock
            {
                Text = _currentCardData.FullJobNumber,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center
            });

            barcodeBorder.Child = barcodePanel;
            Grid.SetRow(barcodeBorder, 3);
            mainGrid.Children.Add(barcodeBorder);

            // ОПЕРАЦИИ
            var operationsBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(5)
            };

            var operationsGrid = new Grid();
            operationsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            operationsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Заголовки
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)) };
            string[] headers = { "№", "Операция", "Статус" };
            int[] widths = { 50, 200, 100 };

            for (int i = 0; i < headers.Length; i++)
            {
                headerPanel.Children.Add(new Border
                {
                    Child = new TextBlock
                    {
                        Text = headers[i],
                        FontSize = 11,
                        FontWeight = FontWeights.Bold,
                        Padding = new Thickness(5)
                    },
                    Width = widths[i],
                    BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                    BorderThickness = new Thickness(0, 0, 1, 0)
                });
            }
            Grid.SetRow(headerPanel, 0);
            operationsGrid.Children.Add(headerPanel);

            // Данные операций
            var rowsPanel = new StackPanel();
            string[][] operationsData = new string[][]
            {
                new[] { "01", "Подготовка", "Выполнено" },
                new[] { "02", "Обработка", "Выполнено" },
                new[] { "03", "Сборка", "В процессе" },
                new[] { "04", "Контроль", "Ожидание" },
                new[] { "05", "Упаковка", "Ожидание" }
            };

            foreach (var row in operationsData)
            {
                var rowPanel = new StackPanel { Orientation = Orientation.Horizontal };
                for (int i = 0; i < row.Length; i++)
                {
                    var textColor = (i == 2 && row[i] == "Выполнено") ? Colors.Green :
                                    (i == 2 && row[i] == "В процессе") ? Colors.Blue : Colors.Black;

                    rowPanel.Children.Add(new Border
                    {
                        Child = new TextBlock
                        {
                            Text = row[i],
                            FontSize = 10,
                            Padding = new Thickness(5),
                            Foreground = new SolidColorBrush(textColor),
                            FontWeight = row[i] == "Выполнено" ? FontWeights.Bold : FontWeights.Normal
                        },
                        Width = widths[i],
                        BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                        BorderThickness = new Thickness(0, 0, 1, 1)
                    });
                }
                rowsPanel.Children.Add(rowPanel);
            }

            var scrollViewer = new ScrollViewer
            {
                Content = rowsPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 120
            };
            Grid.SetRow(scrollViewer, 1);
            operationsGrid.Children.Add(scrollViewer);

            operationsBorder.Child = operationsGrid;
            Grid.SetRow(operationsBorder, 4);
            mainGrid.Children.Add(operationsBorder);

            // ФУТЕР
            var footerBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                CornerRadius = new CornerRadius(0, 0, 8, 8),
                Padding = new Thickness(10, 3, 10, 3)
            };

            var footerPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            footerPanel.Children.Add(new TextBlock
            {
                Text = "Канбан • Завод N • ",
                FontSize = 9,
                Foreground = Brushes.Gray
            });
            footerPanel.Children.Add(new TextBlock
            {
                Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkGray
            });

            footerBorder.Child = footerPanel;
            Grid.SetRow(footerBorder, 5);
            mainGrid.Children.Add(footerBorder);

            border.Child = mainGrid;
            previewContent.Content = border;
            previewContent.Visibility = Visibility.Visible;
            lblPreview.Visibility = Visibility.Collapsed;
        }

        private StackPanel CreateInfoRow(string label, string value)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 3, 0, 3) };

            panel.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkGray,
                Width = 60
            });

            panel.Children.Add(new TextBlock
            {
                Text = value,
                FontSize = 11,
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 150
            });

            return panel;
        }

        private Border CreateStatusBlock(string label, string value, Color color)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 250)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 4, 8, 4),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 9,
                Foreground = Brushes.Gray
            });
            panel.Children.Add(new TextBlock
            {
                Text = value,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(color)
            });

            border.Child = panel;
            return border;
        }

        private Border CreateSimpleBlock(string label, string value)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(8, 4, 8, 4),
                Margin = new Thickness(0, 0, 10, 0)
            };

            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 9,
                Foreground = Brushes.Gray
            });
            panel.Children.Add(new TextBlock
            {
                Text = value,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.Black
            });

            border.Child = panel;
            return border;
        }

        private Border CreateDataChip(string label, string value)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(230, 230, 240)),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12, 4, 12, 4),
                Margin = new Thickness(0, 0, 8, 0)
            };

            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            panel.Children.Add(new TextBlock
            {
                Text = label + ": ",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkSlateGray
            });
            panel.Children.Add(new TextBlock
            {
                Text = value,
                FontSize = 10,
                Foreground = Brushes.Black
            });

            border.Child = panel;
            return border;
        }

        private void UpdateStatusPanel()
        {
            if (_currentCardData == null)
            {
                lblJobInfo.Text = "Нет данных";
                lblPrintStatus.Text = "Статус: ожидание";
                lblPrintStatus.Foreground = new SolidColorBrush(Colors.Gray);
                return;
            }

            lblJobInfo.Text = $"{_currentCardData.JobNumber}-{_currentCardData.Suffix} | {(_currentCardData.IsA6Format ? "A6" : "A4")}";
            lblPrintStatus.Text = "Статус: РАЗРЕШЕНА";
            lblPrintStatus.Foreground = new SolidColorBrush(Colors.Green);
        }

        private async void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (_isPrinting)
            {
                MessageBox.Show("Печать уже выполняется...", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_currentCardData == null)
            {
                MessageBox.Show("Сначала выполните предпросмотр", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _isPrinting = true;
                btnPrint.IsEnabled = false;
                btnPreview.IsEnabled = false;

                LogMessage($"=== НАЧАЛО ПЕЧАТИ ===");

                int pages = int.TryParse(txtPages.Text, out int p) ? p : 1;

                int fifoNumber = new Random().Next(1000, 9999);
                _currentCardData.FIFO = fifoNumber;
                LogMessage($"FIFO: {fifoNumber:D4}");

                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "PDF файлы (*.pdf)|*.pdf",
                    FileName = $"KANBAN_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    Title = "Сохранить PDF"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    await System.Threading.Tasks.Task.Delay(800);

                    string message = $"ДЕМО: PDF сохранен\n\n" +
                                    $"Карточек: {pages}\n" +
                                    $"Формат: {(chkA6.IsChecked == true ? "A6" : "A4")}\n" +
                                    $"FIFO: {fifoNumber:D4}";

                    MessageBox.Show(message, "Готово",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    LogMessage($"Печать завершена");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Ошибка: {ex.Message}");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isPrinting = false;
                UpdateButtonState();
                btnPrint.IsEnabled = true;
            }
        }
    }
}