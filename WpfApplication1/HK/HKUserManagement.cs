using System.Windows;
using NVRCsharpDemo;

namespace WpfApplication1.HK
{
    public class HKUserManagement
    {
        public static void Login(string DVRIPAddress, int DVRPortNumber, string DVRUserName, string DVRPassword)
        {
            if (CHCActivityData.m_lUserID < 0)
            {


                CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();

                //登录设备 Login the device
                CHCActivityData.m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName,
                    DVRPassword, ref DeviceInfo);
                if (CHCActivityData.m_lUserID < 0)
                {
                    var iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    var str = "NET_DVR_Login_V30 failed, error code= " + iLastErr; //登录失败，输出错误号
                    MessageBox.Show(str);
                    return;
                }
                else
                {
                    //登录成功
                    // MessageBox.Show("Login Success!");

                }

            }
            else
            {
                //注销登录 Logout the device

                if (!CHCNetSDK.NET_DVR_Logout(CHCActivityData.m_lUserID))
                {
                    var iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    var str = "NET_DVR_Logout failed, error code= " + iLastErr;
                    MessageBox.Show(str);
                    return;
                }
                CHCActivityData.m_lUserID = -1;


            }


        }

    }
}
