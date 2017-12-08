using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App_Bluetooth
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DesignedMain : ContentPage
    {
        public DesignedMain()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            backImage.Source = ImageSource.FromResource("App_Bluetooth.Design.AQ_1.png");
            backImage.Aspect = Aspect.Fill;
        }

        public void setWeight(string kg)
        {
            lblWeight.Text = kg + " kg";
        }
        public void setFat(string fat)
        {
            lblFat.Text = fat + " %";
        }

        public void setMuscle(string muscle)
        {
            lblMuscle.Text = muscle + " %";
        }

        public void setBone(string bone)
        {
            lblBone.Text = bone + " kg";
        }

        public void setWater(string water)
        {
            lblWater.Text = water + " %";
        }

        public void setKcal(string kcal)
        {
            lblKcal.Text = kcal;
        }
    }
}