using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

namespace App_Bluetooth
{
    public partial class MainPage : ContentPage
    {
        IBluetoothLE ble;
        IAdapter adapter;
        ObservableCollection<IDevice> deviceList;
        IDevice device;
        string recvData;

        public MainPage()
        {
            InitializeComponent();

            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
            deviceList = new ObservableCollection<IDevice>();            
            lv.ItemsSource = deviceList;

            List<string> reqList = new List<string>();
            reqList.Add("날짜/시간");
            reqList.Add("4개알람");
            reqList.Add("공기질측정주기");
            lblSendPicker.ItemsSource = reqList;                         
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
            device = await adapter.ConnectToKnownDeviceAsync(new Guid("00000000-0000-0000-0000-c7e4e3e2e1fc"));
            service = await device.GetServiceAsync(Guid.Parse("0000fff0-0000-1000-8000-00805f9b34fb"));
            recv_character = await service.GetCharacteristicAsync(Guid.Parse("0000fff1-0000-1000-8000-00805f9b34fb"));
            send_character = await service.GetCharacteristicAsync(Guid.Parse("0000fff2-0000-1000-8000-00805f9b34fb"));

            var state = device.State;
            DisplayAlert("Notice", state.ToString(), "OK !");

            recv_character.ValueUpdated += Character_ValueUpdated;
            await recv_character.StartUpdatesAsync();

        }

        private void Character_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
        {            
            var bytes = e.Characteristic.Value;
            GBGPrrotocol protocol = new GBGPrrotocol();
            string retData = "";
            if (bytes.Length > 1)
            {
                recvData = protocol.DeviceToApp_ParseRecvData(bytes.ToList());
                Device.BeginInvokeOnMainThread(() =>
                {
                    lblRecvValue.Text = recvData;
                });
            }
        }
        IList<IService> services;


        private async void btnGetServs_Clicked(object sender, EventArgs e)
        {
            services = await device.GetServicesAsync();
        }

        private async void btnSendData_Clicked(object sender, EventArgs e)
        {
            string strSelItem = (string)lblSendPicker.SelectedItem;
            GBGPrrotocol protocol = new GBGPrrotocol();

            string strReq = "";
            switch (strSelItem)
            {
                case "날짜/시간":
                    strReq = BitConverter.ToString(protocol.AppToDevice_Date().ToArray()).Replace("-", String.Empty);
                    break;
                case "4개알람":
                    UserInfo userInfo = new UserInfo();
                    userInfo.SetUserInfo(0, "ABCDE");
                    strReq = BitConverter.ToString(protocol.AppToDevice_4Setting(userInfo, new TwoByte(0, 0), new TwoByte(0, 0), new TwoByte(0, 0), new TwoByte(0, 0)).ToArray()).Replace("-", String.Empty);
                    break;
                case "공기질측정주기":
                    strReq = BitConverter.ToString(protocol.AppToDevice_AirCheckTime(new TwoByte(0, 0)).ToArray()).Replace("-", String.Empty);
                    break;
            }

            if (string.IsNullOrEmpty(strReq))
                return;
            
            await send_character.WriteAsync(StringToByteArray(strReq));
        }

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
            GBGPrrotocol protocol = new GBGPrrotocol();
            List<byte> reqData = protocol.AppToDevice_AirCheckTime(new TwoByte(0, 0));
            DisplayAlert("Test", BitConverter.ToString(reqData.ToArray()).Replace("-", String.Empty), "OK !");
        }
    }
}
