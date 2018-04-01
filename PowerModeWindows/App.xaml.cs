using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace PowerModeWindows
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            base.OnStartup(e);
            CreateNotifyIcon();
            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        /// <summary>
        /// 创建通知图标
        /// </summary>
        private void CreateNotifyIcon()
        {
            var notifyIcon = new NotifyIcon();
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("启用",ToggleStatus){
                     Checked=true
                },
                new MenuItem("-"),
                new MenuItem("退出",(s,e)=>{
                    notifyIcon.Visible=false;
                    Current.Shutdown();
                }),
            });
            notifyIcon.Icon = PowerModeWindows.Properties.Resources.IconSmall;
            notifyIcon.Visible = true;
        }

        /// <summary>
        /// 切换启用/禁用状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ToggleStatus(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;
            menuItem.Checked = !menuItem.Checked;
            if (menuItem.Checked)
            {
                MainWindow = new MainWindow();
                MainWindow.Show();
            }
            else
            {
                MainWindow.Close();
                MainWindow = null;
            }
        }

    }
}
