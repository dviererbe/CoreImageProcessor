using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CoreImageProcessor.Windows
{
    /// <summary>
    /// Interaktionslogik für ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public class ErrorWindowViewModel
        {
            public string Message
            {
                get;
                set;
            } = string.Empty;

            public string ExceptionString 
            {
                get;
                set;
            } = string.Empty;
        }

        public ErrorWindow(Exception exception, string? message = null)
        {
            if (message == null)
                message = "An error occured during the one of latest operations.";

            StringBuilder stringBuilder = new StringBuilder();
            BuildExceptionString(stringBuilder, "", exception);

            DataContext = new ErrorWindowViewModel()
            {
                Message = message,
                ExceptionString = stringBuilder.ToString()
            };

            InitializeComponent();
        }

        private void BuildExceptionString(StringBuilder builder, string offset, Exception exception)
        {
            builder.Append(offset).Append("Exception type: ").Append(exception.GetType().Name).Append(" (").Append(exception.GetType().FullName).AppendLine(")");
            builder.Append(offset).Append("Message: ").AppendLine(exception.Message);
            
            if (exception.Source != null)
                builder.Append(offset).Append("Source: ").AppendLine(exception.Source);

            if (exception.StackTrace == null)
                builder.Append(offset).AppendLine("StackTrace: NULL");
            else
            {
                builder.Append(offset).AppendLine("StackTrace:");

                string[] lines = exception.StackTrace.Replace("\r", "").Split('\n');

                foreach (string line in lines)
                {
                    builder.Append(offset + "  ").AppendLine(line);
                }
            }

            if (exception.InnerException != null)
            {
                builder.Append(offset).AppendLine("InnerException: ");
                BuildExceptionString(builder, offset + "    ", exception.InnerException);
            }

            if (exception is AggregateException aggregateException)
            {
                builder.Append(offset).AppendLine("InnerExceptions: ");

                offset = offset + "    ";

                int i = 0;
                foreach (Exception innerException in aggregateException.InnerExceptions)
                {
                    builder.Append(offset).AppendLine("InnerException " + ++i + ":");
                    BuildExceptionString(builder, offset + "  ", innerException);
                }
            }
        }

        private void OkButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
