using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using Coding4Fun.Toolkit.Controls;
using Messaging_App;
using Messaging_App.ViewModels;

namespace Messaging_App
{
    public class MessageBloxFlip : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int ival = Binder.Instance.Messages.IndexOf((Message)value);

            // Checks the modulus, whether the index is odd or even
            if (ival % 2 == 0)
            {
                if (parameter == null)
                    return 1; // no parameter - return opacity
                else
                {
                    if (parameter.ToString() == "direction") // return chat "triangle" direction
                    {
                        return ChatBubbleDirection.LowerRight;
                    }
                    else  // return aligment
                    {
                        return HorizontalAlignment.Right;
                    }
                }
            }
            else
            {
                if (parameter == null)
                    return .8;
                else
                {
                    if (parameter.ToString() == "direction")
                    {
                        return ChatBubbleDirection.UpperLeft;
                    }
                    else
                    {
                        return HorizontalAlignment.Left;
                    }
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
