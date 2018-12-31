using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MouseClickerMacro
{
	struct UPoint
	{
		public uint x;
		public uint y;

		public UPoint(uint x, uint y)
		{
			this.x = x;
			this.y = y;
		}
	}

	public partial class MainForm : Form
	{
		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
		[DllImport("user32.dll", SetLastError=true)]
		private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

		private const int MOUSEEVENTF_LEFTDOWN = 0x02;
		private const int MOUSEEVENTF_LEFTUP = 0x04;
		private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
		private const int MOUSEEVENTF_RIGHTUP = 0x10;
		public const byte KEYEVENTF_KEYUP = 0x02;
		public const byte VK_CONTROL = 0x11;
		
		private static int SCREEN_WIDTH = 1360;
		private static int SCREEN_HEIGHT = 768;

		private static volatile bool usedRunnableAlive = false;
		private static volatile bool boughtRunnableAlive = false;

		enum KeyModifier
		{
			None = 0,
			Alt = 1,
			Control = 2,
			Shift = 4,
			WinKey = 8
		}

		enum MouseEventFlags
		{
			LEFTDOWN = 0x00000002,
			LEFTUP = 0x00000004,
			MIDDLEDOWN = 0x00000020,
			MIDDLEUP = 0x00000040,
			MOVE = 0x00000001,
			ABSOLUTE = 0x00008000,
			RIGHTDOWN = 0x00000008,
			RIGHTUP = 0x00000010
		}

		public MainForm()
		{
			InitializeComponent();
		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
			if (m.Msg == 0x0312)
			{
				int id = m.WParam.ToInt32();
				switch (id)
				{
					case 0:
						if (boughtRunnableAlive) break;
						if (usedRunnableAlive)
						{
							usedRunnableAlive = false;
						}
						else
						{
							usedRunnableAlive = true;
							Thread usedThread = new Thread(new ThreadStart(UsedRunnable));
							usedThread.Start();
						}
						break;
					case 1:
						if (usedRunnableAlive) break;
						if (boughtRunnableAlive)
						{
							boughtRunnableAlive = false;
						}
						else
						{
							boughtRunnableAlive = true;
							Thread boughtThread = new Thread(new ThreadStart(BoughtRunnable));
							boughtThread.Start();
						}
						break;
				}
			}
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			RegisterHotKey(this.Handle, 0, (int)KeyModifier.None, Keys.F1.GetHashCode());
			RegisterHotKey(this.Handle, 1, (int)KeyModifier.None, Keys.F2.GetHashCode());
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			UnregisterHotKey(this.Handle, 0);
			UnregisterHotKey(this.Handle, 1);
		}

		private static UPoint convertPoint(Point point)
		{
			//Point screenPoint = this.PointToScreen(point);
			//Rectangle screen_bounds = Screen.GetBounds(screenPoint);
			Rectangle screen_bounds = new Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
			uint x = (uint)(point.X * 65535 / screen_bounds.Width);
			uint y = (uint)(point.Y * 65535 / screen_bounds.Height);
			return new UPoint(x, y);
		}

		private static void DoMouseMoveTo(int x, int y)
		{
			UPoint uPoint = convertPoint(new Point(x, y));
			mouse_event(
				(uint)(MouseEventFlags.ABSOLUTE | MouseEventFlags.MOVE),
				uPoint.x,
				uPoint.y,
				0, 
				0
			);
		}

		private static void DoMouseClick(int x, int y)
		{
			UPoint uPoint = convertPoint(new Point(x, y));
			keybd_event(VK_CONTROL, 0, 0, 0);
			keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
			mouse_event(
				(uint)(MouseEventFlags.ABSOLUTE | MouseEventFlags.MOVE | MouseEventFlags.LEFTDOWN | MouseEventFlags.LEFTUP),
				uPoint.x,
				uPoint.y,
				0, 
				0
			);
		}

		private static void UsedRunnable()
		{
			Point[] points = {
				new Point(1060, 669),
				new Point(807, 652),
				new Point(596, 472),
				new Point(684, 473)
			};
			int[] delays= {
				300,
				300,
				300,
				650,
			};
			while (usedRunnableAlive)
			{
				for (int i = 0; i < points.Length; i++)
				{
					Point p = points[i];
					Thread.Sleep(delays[i]);
					DoMouseMoveTo(p.X, p.Y);
					DoMouseClick(p.X, p.Y);
				}
			}
		}

		private void BoughtRunnable()
		{
			Point[] points = {
				new Point(1060, 669),
				new Point(807, 652),
				new Point(596, 472),
				new Point(759, 466),
				new Point(684, 473)
			};
			int[] delays = {
				300,
				300,
				300,
				650,
				300
			};
			while (boughtRunnableAlive)
			{
				for (int i = 0; i < points.Length; i++)
				{
					Point p = points[i];
					Thread.Sleep(delays[i]);
					DoMouseMoveTo(p.X, p.Y);
					DoMouseClick(p.X, p.Y);
				}
			}
		}

		private void label1_Click(object sender, EventArgs e)
		{
			MessageBox.Show("GARENA FUCKIN HELL!!");
		}
	}
}
