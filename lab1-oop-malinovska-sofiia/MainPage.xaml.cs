using System.Globalization;
using lab1_oop_malinovska_sofiia.Parser;
using System.IO;
using System.Text;
using Grid = Microsoft.Maui.Controls.Grid;

namespace lab1_oop_malinovska_sofiia
{
    public partial class MainPage : ContentPage
    {
        private int Cols = 10;
        private int Rows = 10;

        private Entry[,] _entries;
        private string[,] _expressions;
        private double[,] _values;
        private CellState[,] _states;
        private string[,] _errors;

        public MainPage()
        {
            InitializeComponent();
            InitArrays();
            CreateTable();
        }
        
        private void InitArrays()
        {
            _entries = new Entry[Rows, Cols];
            _expressions = new string[Rows, Cols];
            _values = new double[Rows, Cols];
            _states = new CellState[Rows, Cols];
            _errors = new string[Rows, Cols];
        }

        private void CreateTable()
        {
            grid.RowDefinitions.Add(new RowDefinition());

            for (int c = 0; c < Cols + 1; c++)
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            
            for (int c = 1; c <= Cols; c++)
            {
                var lbl = new Label
                {
                    Text = ((char)('A' + c - 1)).ToString(),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    BackgroundColor = Color.FromArgb("#111827"),
                    Padding = new Thickness(0, 4)
                };
                Grid.SetRow(lbl, 0);
                Grid.SetColumn(lbl, c);
                grid.Children.Add(lbl);
            }
            
            for (int r = 0; r < Rows; r++)
            {
                grid.RowDefinitions.Add(new RowDefinition());

                var rowLabel = new Label
                {
                    Text = (r + 1).ToString(),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.Gainsboro,
                    BackgroundColor = Color.FromArgb("#020617")
                };
                Grid.SetRow(rowLabel, r + 1);
                Grid.SetColumn(rowLabel, 0);
                grid.Children.Add(rowLabel);

                for (int c = 0; c < Cols; c++)
                {
                    var entry = new Entry
                    {
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Center,
                        BackgroundColor = Color.FromArgb("#020617"),
                        TextColor = Colors.White,
                        PlaceholderColor = Color.FromArgb("#6B7280"),
                        Margin = new Thickness(0),
                        HeightRequest = 30
                    };

                    entry.Unfocused += Entry_Unfocused;

                    Grid.SetRow(entry, r + 1);
                    Grid.SetColumn(entry, c + 1);
                    grid.Children.Add(entry);

                    _entries[r, c] = entry;
                    _expressions[r, c] = "";
                }
            }
        }

        private void Entry_Unfocused(object sender, FocusEventArgs e)
        {
            if (sender is not Entry entry) return;

            int row = Grid.GetRow(entry) - 1;
            int col = Grid.GetColumn(entry) - 1;
            if (row < 0 || col < 0 || row >= Rows || col >= Cols) return;

            _expressions[row, col] = entry.Text?.Trim() ?? "";
        }
        
        private async void CalculateButton_Clicked(object sender, EventArgs e)
        {
            int errors = EvaluateAll();

            if (errors == 0)
            {
                textInput.Text = "Обчислення виконано без помилок.";
                await DisplayAlert("Обчислення", "Усі значення успішно обчислені.", "OK");
            }
            else
            {
                textInput.Text = $"Є помилки в {errors} клітинках. Дивись підсвічені.";
                await DisplayAlert("Помилки", textInput.Text, "OK");
            }
        }
        
        private void ShowExpressionsButton_Clicked(object sender, EventArgs e)
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    var entry = _entries[r, c];
                    if (entry == null) continue;
                    entry.Text = _expressions[r, c] ?? "";
                }
            }

            textInput.Text = "Режим: відображення ВИРАЗІВ.";
        }
        private void ShowValuesButton_Clicked(object sender, EventArgs e)
        {
            EvaluateAll(); 

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    var entry = _entries[r, c];
                    if (entry == null) continue;

                    if (string.IsNullOrWhiteSpace(_expressions[r, c]))
                        entry.Text = "";
                    else if (_errors[r, c] != null)
                        entry.Text = "ERR";
                    else
                        entry.Text = _values[r, c].ToString(CultureInfo.InvariantCulture);
                }
            }

            textInput.Text = "Режим: відображення ЗНАЧЕНЬ (ERR – помилковий вираз).";
        }
        private void RecreateTable(int newRows, int newCols)
        {
            Rows = newRows;
            Cols = newCols;
            
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            
            InitArrays();

            CreateTable();
        }

        private void ResizeButton_Clicked(object sender, EventArgs e)
        {
            if (!int.TryParse(rowsEntry.Text, out int newRows) ||
                !int.TryParse(colsEntry.Text, out int newCols))
            {
                SetStatus("Некоректний ввід розміру таблиці.", true);
                return;
            }
            
            if (newRows < 1 || newRows > 20 || newCols < 1 || newCols > 26)
            {
                SetStatus("Рядки: 1–20, стовпчики: 1–26.", true);
                return;
            }

            RecreateTable(newRows, newCols);
            SetStatus($"Розмір таблиці змінено на {newRows}×{newCols}.", false);
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var sb = new StringBuilder();
                
                sb.Append(";");
                for (int col = 0; col < Cols; col++)
                {
                    sb.Append((char)('A' + col));
                    if (col < Cols - 1)
                        sb.Append(';');
                }
                sb.AppendLine();
                
                for (int row = 0; row < Rows; row++)
                {
                    sb.Append(row + 1);
                    sb.Append(';');

                    for (int col = 0; col < Cols; col++)
                    {
                        var expr = _expressions[row, col];

                        if (!string.IsNullOrWhiteSpace(expr))
                        {
                            var excelExpr = expr.Replace(" ", "");

                            if (!excelExpr.StartsWith("="))
                                excelExpr = "=" + excelExpr;

                            sb.Append(excelExpr);
                        }

                        if (col < Cols - 1)
                            sb.Append(';');
                    }


                    sb.AppendLine();
                }

                var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var path = Path.Combine(folder, "lab1_table.csv");

                await File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8);

                SetStatus($"Таблицю збережено у файл: {path}", isError: false);
            }
            catch (Exception ex)
            {
                SetStatus("Помилка під час збереження: " + ex.Message, isError: true);
            }
        }
        
        private void SetStatus(string message, bool isError)
        {
            if (textInput == null)
                return;

            textInput.Text = message;
            textInput.TextColor = isError ? Colors.OrangeRed : Colors.LightGreen;
        }

        private async void HelpButton_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert(
                "Довідка",
                "Електронна таблиця для аналізу та обчислення виразів.\n\n" +
                "Кожна клітинка містить ВИРАЗ у режимі 'Вирази'. " +
                "У режимі 'Значення' показується результат обчислення.\n\n" +
                "Підтримуються операції: +, -, *, /, mod, div, ^, inc(x), dec(x) " +
                "та посилання на клітинки типу A1, B3 тощо.",
                "OK");
        }

        private void ExitButton_Clicked(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private int EvaluateAll()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    _states[r, c] = CellState.NotVisited;
                    _errors[r, c] = null;
                }
            }

            int errorCount = 0;

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    try
                    {
                        EvaluateCell(r, c);
                    }
                    catch (Exception ex)
                    {
                        _errors[r, c] = ex.Message;
                        errorCount++;
                    }
                }
            }

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    var entry = _entries[r, c];
                    if (entry == null) continue;

                    if (_errors[r, c] != null)
                        entry.BackgroundColor = Colors.MistyRose;
                    else
                        entry.BackgroundColor = Color.FromArgb("#020617");
                }
            }

            return errorCount;
        }

        private double EvaluateCell(int row, int col)
        {
            if (row < 0 || col < 0 || row >= Rows || col >= Cols)
                return 0;

            if (_states[row, col] == CellState.Visited)
                return _values[row, col];

            if (_states[row, col] == CellState.Visiting)
                throw new Exception("Циклічне посилання");

            _states[row, col] = CellState.Visiting;

            string expr = _expressions[row, col];
            double result;

            if (string.IsNullOrWhiteSpace(expr))
            {
                result = 0;
            }
            else
            {
                var parser = new ExpressionParser(expr, name =>
                {
                    int c = char.ToUpper(name[0]) - 'A';
                    if (!int.TryParse(name[1..], out int r))
                        throw new Exception($"Невірне ім'я клітинки: {name}");
                    r--;

                    if (r < 0 || c < 0 || r >= Rows || c >= Cols)
                        throw new Exception($"Клітинка за межами таблиці: {name}");

                    return EvaluateCell(r, c);
                });

                result = parser.Parse();
            }

            _values[row, col] = result;
            _states[row, col] = CellState.Visited;
            return result;
        }

        private enum CellState
        {
            NotVisited,
            Visiting,
            Visited
        }

    }

}
