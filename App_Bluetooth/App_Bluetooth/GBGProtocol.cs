using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;


namespace App_Bluetooth
{

    public struct TwoByte
    {
        byte hByte;
        byte lByte;

        public TwoByte(byte _hData, byte _lData)
        {
            hByte = _hData;
            lByte = _lData;
        }

        public void SetData(byte _hData, byte _lData)
        {
            hByte = _hData;
            lByte = _lData;
        }

        public static TwoByte SetFromInt(int _Data)
        {
            TwoByte bytes = new TwoByte();
            bytes.hByte = (byte)(_Data / (byte.MaxValue+1));
            bytes.lByte = (byte)(_Data % (byte.MaxValue+1));
            return bytes;            
        }

        public int GetInt()
        {
            return hByte * 256 + lByte;
        }

        public byte[] GetData()
        {
            return new byte[] { hByte, lByte };
        }
    }
    public struct UserInfo  //유저정보
    {
        byte userID;    //유저 ID 0~10
        byte [] name;

        public void SetUserInfo(int _userID, string _userName)
        {
            userID = (byte)_userID;
            name = new byte[5];


            for(int i=0;i<5;i++)
            {
                if (_userName[i] >= 'a' && _userName[i] <= 'z')
                    name[i] = (byte)(_userName[i] - 'a');
                else if (_userName[i] >= 'A' && _userName[i] <= 'Z')
                    name[i] = (byte)((_userName[i] - 'A') + 26);
                else if(_userName[i] == ' ')
                    name[i] = 0xff;
            }
        }

        public string UserName
        {
            get {
                string retValue = "";                

                for(int i=0;i<5;i++)
                {
                    byte c = 0;

                    if (name[i] < 26)
                        c = (byte)('a' + name[i]);
                    else if (name[i] < 52)
                        c = (byte)('A' + (name[i] - 26));
                    else if((name[i] == 0xff))
                        c = (byte)' ';

                    retValue += c;
                }               

                return retValue;
            }            
        }    
        
        public int UserID
        {
            get
            {
                return (int)userID;
            }
        }

        public List<byte> GetUserInfo()
        {
            List<byte> rb = new List<byte>();
            rb.Add(userID);
            rb.AddRange(name);
            return rb;
        }
    }


    public class GBGProtocol
    {
        const byte GBGP_CMD_ONLY_KG = 0xA0;         //체중만 전송
        const byte GBGP_CMD_ONLY_KG_FAIL = 0x00;    //중량(측정 실패)
        const byte GBGP_CMD_ONLY_KG_SUCCESS = 0x01; //중량(측정 완료)

        const byte GBGP_CMD_KG_AND_AIR = 0xA2;      //체중 + 공기질 측정
        const byte GBGP_CMD_KG_AND_AIR_OPT1 = 0x20; //체중 + 체지방 + 내장지방
        const byte GBGP_CMD_KG_AND_AIR_OPT2 = 0x21; //체수분 + 체근육
        const byte GBGP_CMD_KG_AND_AIR_OPT3 = 0x22; //체골량 + 기초대사량 + BMI
        const byte GBGP_CMD_KG_AND_AIR_FAIL = 0x23; //에러발생
        const byte GBGP_CMD_KG_AND_AIR_OPT4 = 0x26; //PM2.5/VOC/CO2/온도/습도



        //AppToDevice
        const byte GBGP_CMD_ETC = 0xA3;         // 시간 및 기초 대사량
        const byte GBGP_CMD_ETC_TIME = 0x30;    //날짜 및 시간 
        const byte GBGP_CMD_ETC_SETTING_ALARM = 0x31;    //총 4개 알람 설정
        const byte GBGP_CMD_ETC_AIR_CHECK_TIME = 0x32;   //공기질 측정 주기
        const byte GBGP_CMD_ETC_CAL = 0x33;   //섭취 칼로리량

        const byte GBGP_CMD_USER = 0xA4;   //사용자 정보
        const byte GBGP_CMD_USER_USERINFO1 = 0x40;   //사용자 정보(사용자번호 + 별명 + 성별 + 신장 + 나이)
        const byte GBGP_CMD_USER_USERINFO2 = 0x41;   //사용자 정보(신장 +  나이)

        const byte GBGP_CMD_AIRCHECK = 0xA5;   //공기질 경보 발생 측정 수치 설정
        const byte GBGP_CMD_AIRCHECK_DEFAULT = 0x50;   //공기질 경보 발생 측정 수치 설정(1개)

        public List<byte> AppToDevice_Date()        //AppToDevice A. 날짜 및 시간
        {

            //년도는 2010 ~ 2030 컨텐츠 만들기
            List<byte> dateTime = new List<byte>();

            dateTime.Add((byte)(DateTime.Now.Year - 2000));
            dateTime.Add((byte)DateTime.Now.Month);
            dateTime.Add((byte)DateTime.Now.Day);
            dateTime.Add((byte)DateTime.Now.Hour);
            dateTime.Add((byte)DateTime.Now.Minute);
            dateTime.Add(GBGP_CMD_ETC_TIME);
            
            return _makeProtocol(dateTime, true, GBGP_CMD_ETC);
        }

        public List<byte> AppToDevice_4Setting(UserInfo userInfo, TwoByte time1, TwoByte time2, TwoByte time3, TwoByte time4)        //AppToDevice B. 총 4개 알람 설정
        {
            List<byte> alarm = new List<byte>();

            alarm.AddRange(userInfo.GetUserInfo());
            alarm.AddRange(time1.GetData());
            alarm.AddRange(time2.GetData());
            alarm.AddRange(time3.GetData());
            alarm.AddRange(time4.GetData());
            alarm.Add(GBGP_CMD_ETC_SETTING_ALARM);

            return _makeProtocol(alarm, true, GBGP_CMD_ETC);
        }

        public List<byte> AppToDevice_AirCheckTime(TwoByte time)  //AppToDevice C. 공기질 측정 주기
        {
            List<byte> airCheck = new List<byte>();
            airCheck.AddRange(time.GetData());
            airCheck.Add(GBGP_CMD_ETC_AIR_CHECK_TIME);
            return _makeProtocol(airCheck, true, GBGP_CMD_ETC);
        }

        public List<byte> AppToDevice_EatCalo(UserInfo userInfo, TwoByte calo1, TwoByte calo2, TwoByte calo3)  //AppToDevice D. 조식, 중식, 섭취 칼로리량
        {
            List<byte> eatCalo = new List<byte>();
            eatCalo.AddRange(userInfo.GetUserInfo());
            eatCalo.AddRange(calo1.GetData());
            eatCalo.AddRange(calo2.GetData());
            eatCalo.AddRange(calo3.GetData());
            eatCalo.Add(GBGP_CMD_ETC_CAL);
            return _makeProtocol(eatCalo, true, GBGP_CMD_ETC);
        }


        public List<byte> AppToDevice_UserInfo1(UserInfo userInfo, byte sex, byte cm, byte age) //AppToDevice 사용자정보, A사용자 번호 ~ 나이
        {
            List<byte> ui = new List<byte>();
            ui.AddRange(userInfo.GetUserInfo());
            ui.Add(sex);
            ui.Add(cm);
            ui.Add(age);
            ui.Add(GBGP_CMD_USER_USERINFO1);
            return _makeProtocol(ui, true, GBGP_CMD_USER);
        }

        public List<byte> AppToDevice_UserInfo2(byte cm, byte age) //AppToDevice 신장 + 연령
        {
            List<byte> ui = new List<byte>();
            ui.Add(cm);
            ui.Add(age);
            ui.Add(GBGP_CMD_USER_USERINFO2);
            return _makeProtocol(ui, true, GBGP_CMD_USER);
        }

        public List<byte> AppToDevice_AirCheckSetting(TwoByte PM25, TwoByte PM10, TwoByte VOC, TwoByte CO2, TwoByte Temp, TwoByte Humi) //AppToDevice 공기질 경보 발생 측정 수치 설정
        {
            List<byte> ac = new List<byte>();
            ac.AddRange(PM25.GetData());
            ac.AddRange(PM10.GetData());
            ac.AddRange(VOC.GetData());
            ac.AddRange(CO2.GetData());
            ac.AddRange(Temp.GetData());
            ac.AddRange(Humi.GetData());
            ac.Add(GBGP_CMD_AIRCHECK_DEFAULT);
            return _makeProtocol(ac, true, GBGP_CMD_AIRCHECK);
        }

        public string DeviceToApp_ParseRecvData(List<byte> recvData, int protocolVer = 1)    //return to JSON Data
        {
            var json = new JObject();

            if (recvData.Count > 1)
            {
                int size = recvData[4];

                if (recvData[3] == GBGP_CMD_ONLY_KG)
                {
                    json.Add("type", "ONLYKG");

                    switch (recvData[4 + size])
                    {
                        case GBGP_CMD_ONLY_KG_FAIL:
                            json.Add("result", "FAIL");
                            break;
                        case GBGP_CMD_ONLY_KG_SUCCESS:
                            json.Add("result", "SUCCESS");
                            json.Add("KG", Math.Round(new TwoByte(recvData[5], recvData[6]).GetInt() * 0.01f,2));
                            json.Add("LB", Math.Round(new TwoByte(recvData[7], recvData[8]).GetInt() * 0.01f,2));
                            break;
                    }
                }
                else if (recvData[3] == GBGP_CMD_KG_AND_AIR)
                {
                    json.Add("type", "KGAIR");
                    

                    switch (recvData[4 + size])
                    {
                        case GBGP_CMD_KG_AND_AIR_OPT1:                            
                            json.Add("result", "SUCCESS");
                            json.Add("subType", "KG/LB/FAT/VFAT");
                            if (protocolVer == 1)
                            {
                                json.Add("KG", new TwoByte(recvData[5], recvData[6]).GetInt());
                                json.Add("LB", new TwoByte(recvData[7], recvData[8]).GetInt());
                                json.Add("FAT", Math.Round(new TwoByte(recvData[9], recvData[10]).GetInt() * 0.1f, 1));
                                json.Add("VFAT", Math.Round(new TwoByte(recvData[11], recvData[12]).GetInt() * 0.1f, 1));
                            }
                            else if(protocolVer == 2)
                            {
                                json.Add("userID", recvData[5]);
                                json.Add("KG", new TwoByte(recvData[6], recvData[7]).GetInt());
                                json.Add("LB", new TwoByte(recvData[8], recvData[9]).GetInt());
                                json.Add("FAT", Math.Round(new TwoByte(recvData[10], recvData[11]).GetInt() * 0.1f, 1));
                                json.Add("VFAT", Math.Round(new TwoByte(recvData[12], recvData[13]).GetInt() * 0.1f, 1));
                            }
                            break;
                        case GBGP_CMD_KG_AND_AIR_OPT2:
                            json.Add("result", "SUCCESS");
                            json.Add("subType", "WATER/MUSCLE");
                            if (protocolVer == 1)
                            {
                                json.Add("WATER", Math.Round(new TwoByte(recvData[5], recvData[6]).GetInt() * 0.1f, 1));
                                json.Add("MUSCLE", Math.Round(new TwoByte(recvData[7], recvData[8]).GetInt() * 0.1f, 1));
                            }
                            else if(protocolVer == 2)
                            {
                                json.Add("userID", recvData[5]);
                                json.Add("WATER", Math.Round(new TwoByte(recvData[6], recvData[7]).GetInt() * 0.1f, 1));
                                json.Add("MUSCLE", Math.Round(new TwoByte(recvData[8], recvData[9]).GetInt() * 0.1f, 1));
                            }
                            break;
                        case GBGP_CMD_KG_AND_AIR_OPT3:
                            json.Add("result", "SUCCESS");
                            json.Add("subType", "BONE/KCAL/BMI");
                            if(protocolVer == 1)
                            {
                                json.Add("BONE", Math.Round(new TwoByte(recvData[5], recvData[6]).GetInt() * 0.1f, 1));
                                json.Add("KCAL", new TwoByte(recvData[7], recvData[8]).GetInt());
                                json.Add("BMI", new TwoByte(recvData[9], recvData[10]).GetInt());
                            }
                            else if(protocolVer == 2)
                            {
                                json.Add("userID", recvData[5]);
                                json.Add("BONE", Math.Round(new TwoByte(recvData[6], recvData[7]).GetInt() * 0.1f, 1));
                                json.Add("KCAL", new TwoByte(recvData[8], recvData[9]).GetInt());
                                json.Add("BMI", new TwoByte(recvData[10], recvData[11]).GetInt());
                            }                           
                            break;
                        case GBGP_CMD_KG_AND_AIR_FAIL:
                            json.Add("result", "FAIL");
                            break;
                        case GBGP_CMD_KG_AND_AIR_OPT4:
                            json.Add("result", "SUCCESS");
                            json.Add("subType", "PM25/PM10/VOC/CO2/TEMP/HUMI");
                            json.Add("PM25", new TwoByte(recvData[5], recvData[6]).GetInt());
                            json.Add("PM10", new TwoByte(recvData[7], recvData[8]).GetInt());
                            json.Add("VOC", new TwoByte(recvData[9], recvData[10]).GetInt());
                            json.Add("CO2", new TwoByte(recvData[11], recvData[12]).GetInt());
                            json.Add("TEMP", Math.Round(new TwoByte(recvData[13], recvData[14]).GetInt() * 0.1f,1));
                            json.Add("HUMI", Math.Round(new TwoByte(recvData[15], recvData[16]).GetInt() * 0.1f,1));
                            break;
                    }
                }
            }
            return json.ToString();
        }


        List<byte> _makeProtocol(List<byte> contents, bool appToDevice, byte CMD)
        {            
            List<byte> rb = new List<byte>();
            rb.Add(0xF5);
            rb.Add(0xFA);            

            if(appToDevice)
            {
                rb.Add(0x01);
            }
            else
            {
                rb.Add(0x00);
            }

            rb.Add(CMD);
            rb.Add((byte)contents.Count);
            rb.AddRange(contents);

            var b = rb.Skip(2).ToList();

            rb.Add(CalculateBCC(rb.Skip(2).ToList()));  //BCC체크

            return rb;
        }

        byte CalculateBCC(List<byte> theBytes)      //BCC체크
        {
            byte toReturn = 0;
            foreach(byte b in theBytes)
            {
                toReturn ^= b;
            }
            return toReturn;
        }
    }
}

