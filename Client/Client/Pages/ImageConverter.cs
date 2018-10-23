using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Client.Pages
{
    public class ImageConverter  : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //Is it in your image folder?
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\images\\" + value.ToString() + ".png"))
            {
                //your image path
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\images\\" + value.ToString() + ".png";
            }
            else
            {
                //else return general image
                //return Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\images\\user.png";
                return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\images\\user.png";
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

/*per ora di questo non uso niente*/