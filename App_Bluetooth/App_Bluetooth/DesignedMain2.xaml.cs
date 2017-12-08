using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json.Linq;

namespace App_Bluetooth
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DesignedMain2 : ContentPage
    {
        public DesignedMain2()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            backImage.Source = ImageSource.FromResource("App_Bluetooth.Design.AQ_2.png");
            backImage.Aspect = Aspect.Fill;
        }

        public void setAirValue(string json)
        {
            JObject j = JObject.Parse(json);
            lblAir.Text = j["PM25"].ToString() + " ㎍/㎥";
            lblPM25.Text = j["PM25"].ToString() + " ㎍/㎥";
            lblPM10.Text = j["PM10"].ToString() + " ㎍/㎥";
            lblCO2.Text = j["CO2"].ToString() + " ppm";
            lblHumi.Text = j["HUMI"].ToString() + " %";
            lblTemp.Text = j["TEMP"].ToString() + " ℃";
        }
    }
}