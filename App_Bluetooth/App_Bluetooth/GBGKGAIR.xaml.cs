using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace App_Bluetooth
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class GBGKGAIR : ContentPage, INotifyPropertyChanged
	{

        public new event PropertyChangedEventHandler PropertyChanged;
        protected new virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetJsonData(string json)
        {
            JObject j = JObject.Parse(json);            
            PM25.Text = j["PM25"].ToString();
            PM10.Text = j["PM10"].ToString();
            VOC.Text = j["VOC"].ToString();
            CO2.Text = j["CO2"].ToString();
            TEMP.Text = j["TEMP"].ToString();
            HUMI.Text = j["HUMI"].ToString();
        }
        

        public GBGKGAIR ()
		{
			InitializeComponent ();
		}
	}
}