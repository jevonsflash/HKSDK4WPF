using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Renderer.Core;
using System.Runtime.InteropServices;
using NVRCsharpDemo;

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private bool m_bInitSDK = false;
        private Int32 m_lRealHandle = -1;
        private Int32 m_lUserID = -1;
        private uint iLastErr = 0;
        private string str;
        CHCNetSDK.REALDATACALLBACK RealData;
        private CHCNetSDK.REALDATACALLBACK m_fRealData = null;
        private PlayCtrl.DECCBFUN MyChuli = null;
        private Int32 m_lPort = -1;
        private IntPtr m_ptrRealHandle;
        private bool m_bJpegCapture = true;
        private void Init()
        {
            m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            this.wbSource = new WriteableBitmapSource();
            T1.Source = this.wbSource.ImageSource;
            this.wbSource.SetupSurface(1920, 1080, FrameFormat.YV12);


            if (m_bInitSDK == false)
            {
                MessageBox.Show("NET_DVR_Init error!");
                return;
            }
            else
            {
                //保存SDK日志 To save the SDK log
                CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog\\", true);
            }
            //
            // TODO: 在 InitializeComponent 调用后添加任何构造函数代码
            //
        }

        private void Uninit()
        {
            if (m_lRealHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
            }
            if (m_lUserID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(m_lUserID);
            }
            if (m_bInitSDK == true)
            {
                CHCNetSDK.NET_DVR_Cleanup();
            }

        }

        private void Login(string DVRIPAddress, int DVRPortNumber, string DVRUserName, string DVRPassword)
        {
            if (m_lUserID < 0)
            {


                CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();

                //登录设备 Login the device
                m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);
                if (m_lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Login_V30 failed, error code= " + iLastErr; //登录失败，输出错误号
                    MessageBox.Show(str);
                    return;
                }
                else
                {
                    //登录成功
                    MessageBox.Show("Login Success!");

                }

            }
            else
            {
                //注销登录 Logout the device
                if (m_lRealHandle >= 0)
                {
                    MessageBox.Show("Please stop live view firstly");
                    return;
                }

                if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Logout failed, error code= " + iLastErr;
                    MessageBox.Show(str);
                    return;
                }
                m_lUserID = -1;

            }
            return;
        }

        private void StartPreview(int Channel)
        {
            if (m_lRealHandle < 0)
            {
                CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
                // lpPreviewInfo.hPlayWnd = RealPlayWnd.Handle;//预览窗口
                lpPreviewInfo.lChannel = Channel;//预te览的设备通道
                lpPreviewInfo.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                lpPreviewInfo.dwLinkMode = 4;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                lpPreviewInfo.bBlocked = true; //0- 非阻塞取流，1- 阻塞取流

                lpPreviewInfo.dwDisplayBufNum = 15; //播放库播放缓冲区最大缓冲帧数


                RealData = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack);//预览实时流回调函数


                IntPtr pUser = new IntPtr();//用户数据
                                            //  m_ptrRealHandle = panel1.Handle;
                                            //打开预览 Start live view 
                m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, RealData, pUser);

                if (m_lRealHandle < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr; //预览失败，输出错误号
                    MessageBox.Show(str);
                    return;
                }
                else
                {

                }

            }
            else
            {
                //停止预览 Stop live view 
                if (!CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
                    MessageBox.Show(str);
                    return;
                }
                m_lRealHandle = -1;
                //  btnPreview.Text = "Live View";

            }
            return;
        }

        private void RealDataCallBack2(int lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr pUser)
        {
            // throw new NotImplementedException();
        }

        public delegate void MyDebugInfo(string str);



        PlayCtrl.DISPLAYCBFUN m_fDisplayFun;
        private void RemoteDisplayCBFun(int nPort, IntPtr pBuf, int nSize, int nWidth, int nHeight, int nStamp, int nType, int nReserved)
        {

            this.Dispatcher.Invoke(new MShow(Show2), pBuf);


            //MyDebugInfo AlarmInfo = new MyDebugInfo(DebugInfo);
            //if (!m_bJpegCapture)
            //{
            //    return;
            //}
            //else
            //{
            //    uint nLastErr = 100;
            //    // save picture as you want


            //    IntPtr ptrJpeg = new IntPtr();
            //    uint nBuffersize = (uint)(2 * nWidth * nHeight);
            //    ptrJpeg = Marshal.AllocHGlobal((int)nBuffersize);
            //    uint dwJpegSize = 0;


            //    if (!PlayCtrl.PlayM4_GetJPEG(nPort, ptrJpeg, nBuffersize, ref dwJpegSize))
            //    {
            //        nLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
            //        this.Dispatcher.BeginInvoke(AlarmInfo, "Jpeg Capture fail");
            //    }
            //    else
            //    {



            //        byte[] by = new byte[dwJpegSize];
            //        Marshal.Copy(ptrJpeg, by, 0, (int)dwJpegSize);
            //        this.Dispatcher.BeginInvoke(new ShowJpeg(MShowJpeg), System.Windows.Threading.DispatcherPriority.ApplicationIdle, by);
            //        Marshal.FreeHGlobal(ptrJpeg);

            //    }

            //    //if (!PlayCtrl.PlayM4_ConvertToJpegFile(pBuf, nSize, nWidth, nHeight, pFrameInfo.nType, "Capture.jpg"))
            //    //{
            //    //    //Debug.WriteLine("PlayM4_ConvertToJpegFile fail");
            //    //    nLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
            //    //    this.BeginInvoke(AlarmInfo, "Jpeg Capture fail");


            //    //}
            //    //else
            //    //{
            //    //    this.BeginInvoke(AlarmInfo, "Jpeg Capture Succ");
            //    //    //Debug.WriteLine("PlayM4_ConvertToJpegFile Succ");
            //    //}

            //}
        }
        public void RealDataCallBack(Int32 lRealHandle, UInt32 dwDataType, IntPtr pBuffer, UInt32 dwBufSize, IntPtr pUser)
        {
            MyDebugInfo AlarmInfo = new MyDebugInfo(DebugInfo);
            IntPtr pObject = new IntPtr();
            GCHandle hObject = new GCHandle();

            hObject = GCHandle.Alloc(pBuffer, GCHandleType.Pinned);

            pObject = hObject.AddrOfPinnedObject();
            switch (dwDataType)
            {
                case CHCNetSDK.NET_DVR_SYSHEAD:     // sys head
                    if (!PlayCtrl.PlayM4_GetPort(ref m_lPort))
                    {
                        MessageBox.Show("Get Port Fail");
                    }



                    if (dwBufSize > 0)
                    {
                        //set as stream mode, real-time stream under preview
                        if (!PlayCtrl.PlayM4_SetStreamOpenMode(m_lPort, PlayCtrl.STREAME_REALTIME))
                        {
                            this.Dispatcher.BeginInvoke(AlarmInfo, "PlayM4_SetStreamOpenMode fail");
                        }
                        //start player
                        if (!PlayCtrl.PlayM4_OpenStream(m_lPort, pObject, dwBufSize, 320 * 280))
                        {
                            m_lPort = -1;
                            this.Dispatcher.BeginInvoke(AlarmInfo, "PlayM4_OpenStream fail");
                            break;
                        }

                        //MyChuli = new PlayCtrl.DECCBFUN(Chuli);
                        //if (!PlayCtrl.PlayM4_SetDecCallBack(m_lPort, MyChuli))
                        //{
                        //    this.Dispatcher.BeginInvoke(AlarmInfo, "PlayM4_SetDecCallBack fail");
                        //}



                        ////set soft decode display callback function to capture
                        m_fDisplayFun = new PlayCtrl.DISPLAYCBFUN(RemoteDisplayCBFun);
                        if (!PlayCtrl.PlayM4_SetDisplayCallBack(m_lPort, m_fDisplayFun))
                        {
                            this.Dispatcher.BeginInvoke(AlarmInfo, "PlayM4_SetDisplayCallBack fail");
                        }

                        ////start play, set play window
                        //this.BeginInvoke(AlarmInfo, "About to call PlayM4_Play");

                        if (!PlayCtrl.PlayM4_Play(m_lPort, m_ptrRealHandle))
                        {
                            m_lPort = -1;
                            this.Dispatcher.BeginInvoke(AlarmInfo, "PlayM4_Play fail");
                            break;
                        }

                        ////set frame buffer number

                        //if (!PlayCtrl.PlayM4_SetDisplayBuf(m_lPort, 15))
                        //{
                        //    this.BeginInvoke(AlarmInfo, "PlayM4_SetDisplayBuf fail");
                        //}

                        ////set display mode
                        //if (!PlayCtrl.PlayM4_SetOverlayMode(m_lPort, 0, 0/* COLORREF(0)*/))//play off screen // todo!!!
                        //{
                        //    this.BeginInvoke(AlarmInfo, " PlayM4_SetOverlayMode fail");
                        //}
                    }

                    break;
                case CHCNetSDK.NET_DVR_STREAMDATA:     // video stream data
                    if (dwBufSize > 0 && m_lPort != -1)
                    {
                        try
                        {
                            //送入码流数据进行解码 Input the stream data to decode
                            PlayCtrl.PlayM4_InputData(m_lPort, pBuffer, dwBufSize);


                        }
                        catch (Exception e)
                        {

                            throw;
                        }


                    }
                    break;

                case CHCNetSDK.NET_DVR_AUDIOSTREAMDATA:     //  Audio Stream Data
                    if (dwBufSize > 0 && m_lPort != -1)
                    {
                        if (!PlayCtrl.PlayM4_InputVideoData(m_lPort, pObject, dwBufSize))
                        {
                            this.Dispatcher.BeginInvoke(AlarmInfo, "PlayM4_InputVideoData Fail");
                        }
                    }

                    break;
                default:
                    break;
            }
            if (hObject.IsAllocated)
                hObject.Free();
        }
        public void DebugInfo(string str)
        {
            if (str.Length > 0)
            {
                str += "\n";
                // listBox1.Items.Add(str);
            }

        }




        private delegate void MShow(IntPtr pBuf);
        private void Show2(IntPtr pBuf)
        {
            this.wbSource.Render(pBuf);
            T1.Source = this.wbSource.ImageSource;
        }

        WriteableBitmapSource wbSource;
        System.Windows.Threading.DispatcherOperation ShowJpegOper = null;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Init();
            Login("192.168.100.111", 8000, "admin", "Password0123");
            StartPreview(3);

        }




    }
}
