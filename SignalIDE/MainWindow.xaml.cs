using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SignalTranslatorCore;
using System.ComponentModel;
using System.IO;

namespace SignalIDE
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        LexAn _lexer = new LexAn();

        string _log;

        public string Log
        {
            get { return _log; }
            set
            {
                _log = _log + Environment.NewLine + value;
                OnPropertyChanged("Log");
                if (LogRow.Height.Value < 100)
                    LogRow.Height = new GridLength(100);
                OutputField.ScrollToEnd();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            
        }

        public IDictionary<string, int> Delimiters { get {
                return _lexer.Delimiters;
            } }

        public IDictionary<string, int> Identifiers
        {
            get
            {
                return _lexer.Identifiers;
            }
        }


        public IDictionary<string, int> Constants
        {
            get
            {
                return _lexer.Constants;
            }
        }


        public string Output { get; set; } = "not compiled yet";

        

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CompileButton_Click(object sender, RoutedEventArgs e)
        {
            var str = EditField.Text;
            var inp = new StringAsFileBuffer(str);
            _lexer.Clear();

            try {
                _lexer.Scan(inp);
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message, "Lexical error");
                Log = "Error: " + ex.Message;
                return;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Compilation error");
                Log = "Error: " + ex.Message;
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Inner compiler error");
                Log = "Please contact the developer " + ex.ToString();
                return;
            }

            var result = "";
            foreach (var s in _lexer.Output)
            {
                result += s + " ";
            }

            Log = "Lexical output: " + result;
            
            Binding b = new Binding();
            b.Source = Identifiers;
            b.Mode = BindingMode.OneWay;
            BindingOperations.ClearBinding(IdentifiersTable, DataGrid.ItemsSourceProperty);
            IdentifiersTable.SetBinding(DataGrid.ItemsSourceProperty, b);

            Binding bs = new Binding();
            bs.Source = Constants;
            bs.Mode = BindingMode.OneWay;
            BindingOperations.ClearBinding(ConstTable, DataGrid.ItemsSourceProperty);
            ConstTable.SetBinding(DataGrid.ItemsSourceProperty, bs);
        }

        private void MenuItem_ClickOpen(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            bool? result = dlg.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                var lines = File.ReadAllText(dlg.FileName);
                EditField.Text = lines;
            }
        }

        private void MenuItem_ClickSave(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".signal";
            dlg.OverwritePrompt = true;
            dlg.AddExtension = true;
            dlg.Filter = "Signal files (*.signal)|*.signal";
            var result = dlg.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                File.WriteAllText(dlg.FileName, EditField.Text);
            }
        }
    }
}
