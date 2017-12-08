using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace App_Bluetooth
{
    public partial class MainPage : ContentPage
    {
        IBluetoothLE ble;
        IAdapter adapter;
        ObservableCollection<IDevice> deviceList;
        IDevice device;
        string recvData;

        GBGKGAIR kgAirPage = new GBGKGAIR();
        DesignedMain dgMain = new DesignedMain();
        DesignedMain2 dgMain2 = new DesignedMain2();

        public MainPage()
        {
            InitializeComponent();

            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
            deviceList = new ObservableCollection<IDevice>();            
            lv.ItemsSource = deviceList;

            /*
            List<string> reqList = new List<string>();
            reqList.Add("날짜/시간");
            reqList.Add("4개알람");
            reqList.Add("공기질측정주기");
            lblSendPicker.ItemsSource = reqList;
            */
        }
        

        private void btnStatus_Clicked(object sender, EventArgs e)
        {
            var state = ble.State;
            DisplayAlert("Notice", "블루투스상태 - " + state.ToString(), "OK !");           
        }

        private async void btnScanDev_Clicked(object sender, EventArgs e)
        {
            deviceList.Clear();

            adapter.DeviceDiscovered += Adapter_DeviceDiscovered;

            await adapter.StartScanningForDevicesAsync();
        }

        private void Adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            deviceList.Add(e.Device);
        }

        private void lv_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(lv.SelectedItem == null)
            {
                return;
            }

            device = lv.SelectedItem as IDevice;
        }

        IService service;
        ICharacteristic recv_character;
        ICharacteristic send_character;

        private async void btnConnect_Clicked(object sender, EventArgs e)
        {
            //await adapter.ConnectToDeviceAsync(device);
            device = await adapter.ConnectToKnownDeviceAsync(new Guid("00000000-0000-0000-0000-c7e4e3e2e157"));
            //device = await adapter.ConnectToKnownDeviceAsync(new Guid("00000000-0000-0000-0000-c7e4e3e2e1fc"));
            service = await device.GetServiceAsync(Guid.Parse("0000fff0-0000-1000-8000-00805f9b34fb"));
            recv_character = await service.GetCharacteristicAsync(Guid.Parse("0000fff1-0000-1000-8000-00805f9b34fb"));
            send_character = await service.GetCharacteristicAsync(Guid.Parse("0000fff2-0000-1000-8000-00805f9b34fb"));

            var state = device.State;
            lblRecvValue.Text = state.ToString();

            recv_character.ValueUpdated += Character_ValueUpdated;
            await recv_character.StartUpdatesAsync();
        }


        //블루투스로 데이터 계속 옴
        private void Character_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
        {            
            var bytes = e.Characteristic.Value;
            GBGProtocol protocol = new GBGProtocol();
            string retData = "";
            if (bytes.Length > 1)
            {                
                recvData = protocol.DeviceToApp_ParseRecvData(bytes.ToList(), int.Parse(protocolVer.Text));
                JObject j = JObject.Parse(recvData);
                if (!j.HasValues)
                    return;

                Device.BeginInvokeOnMainThread(() =>
                {
                lblRecvValue.Text = recvData;
                if (j.HasValues)
                {
                    if (j["type"] != null)
                    {
                            if (j["type"].ToString() == "KGAIR" && j["result"].ToString() == "SUCCESS")
                            {
                                if (j["subType"].ToString() == "PM25/PM10/VOC/CO2/TEMP/HUMI")
                                {
                                    /*
                                    kgAirPage.SetJsonData(recvData);
                                    if (!Navigation.NavigationStack.Contains(kgAirPage))
                                    {
                                        Navigation.PushAsync(kgAirPage);
                                    } 
                                    */
                                    dgMain2.setAirValue(j.ToString());
                                }
                                else if(j["subType"].ToString() == "KG/LB/FAT/VFAT")
                                {
                                    dgMain.setFat(j["FAT"].ToString());
                                }
                                else if(j["subType"].ToString() == "WATER/MUSCLE")
                                {
                                    dgMain.setWater(j["WATER"].ToString());
                                    dgMain.setMuscle(j["MUSCLE"].ToString());
                                }
                                else if(j["subType"].ToString() == "BONE/KCAL/BMI")
                                {
                                    dgMain.setKcal(j["KCAL"].ToString());
                                    dgMain.setBone(j["BONE"].ToString());
                                }
                            }
                            else if (j["type"].ToString() == "ONLYKG" && j["result"].ToString() == "SUCCESS")
                            {
                                dgMain.setWeight(j["KG"].ToString());
                            }
                        }                        
                    }
                });
            }
        }


        IList<IService> services;
        private async void btnGetServs_Clicked(object sender, EventArgs e)
        {
            services = await device.GetServicesAsync();
        }

        /*
        private async void btnSendData_Clicked(object sender, EventArgs e)
        {
            string strSelItem = (string)lblSendPicker.SelectedItem;
            GBGProtocol protocol = new GBGProtocol();

            string strReq = "";
            switch (strSelItem)
            {
                case "날짜/시간":
                    strReq = BitConverter.ToString(protocol.AppToDevice_Date().ToArray()).Replace("-", String.Empty);
                    break;
                case "4개알람":
                    UserInfo userInfo = new UserInfo();
                    userInfo.SetUserInfo(0, "ABCDE");
                    strReq = BitConverter.ToString(protocol.AppToDevice_4Setting(userInfo, TwoByte.SetFromInt(0), TwoByte.SetFromInt(0), TwoByte.SetFromInt(0), TwoByte.SetFromInt(0)).ToArray()).Replace("-", String.Empty);
                    break;
                case "공기질측정주기":
                    strReq = BitConverter.ToString(protocol.AppToDevice_AirCheckTime(TwoByte.SetFromInt(0)).ToArray()).Replace("-", String.Empty);
                    break;
            }

            if (string.IsNullOrEmpty(strReq))
                return;
            
            await send_character.WriteAsync(StringToByteArray(strReq));
        }
        */

        public byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private async void btnRecvData_Clicked(object sender, EventArgs e)
        {
            var bytes = await recv_character.ReadAsync();
            lblRecvValue.Text = BitConverter.ToString(bytes).Replace("-", " ");
        }

        private void btnTest_Clicked(object sender, EventArgs e)
        {
            GBGProtocol protocol = new GBGProtocol();
            List<byte> reqData = protocol.AppToDevice_AirCheckTime(TwoByte.SetFromInt(0));
            DisplayAlert("Test", BitConverter.ToString(reqData.ToArray()).Replace("-", String.Empty), "OK !");
        }

        private void btnKGView_Clicked(object sender, EventArgs e)
        {
            if (!Navigation.NavigationStack.Contains(dgMain))
            {
                Navigation.PushAsync(dgMain);
            }
        }

        private void btnAIRView_Clicked(object sender, EventArgs e)
        {
            if (!Navigation.NavigationStack.Contains(dgMain2))
            {
                Navigation.PushAsync(dgMain2);
            }
        }
    }
}
