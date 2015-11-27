﻿using System;
using System.Drawing;
using System.Windows;
using Forms = System.Windows.Forms;

namespace Pomodoro {
	class NotifyIconManager {
		public static void Init(MainModel mainModel) {
			_instance = new NotifyIconManager(mainModel);
		}

		static NotifyIconManager _instance;

		public static NotifyIconManager Instance {
			get {
				return _instance;
			}
		}

		NotifyIconManager(MainModel mainModel) {
			_mainModel = mainModel;
			_contextMenu = new Forms.ContextMenuStrip();
			Forms.ToolStripMenuItem itemExit = new Forms.ToolStripMenuItem {
				Text = "Exit",
			};
			itemExit.Click += ItemExit_Click;
			_contextMenu.Items.Add(itemExit);
			_notifyIcon = new Forms.NotifyIcon {
				Icon = new Icon(Properties.Resources.Tomato, Forms.SystemInformation.SmallIconSize),
				Text = "Pomodoro",
				Visible = true,
				ContextMenuStrip = _contextMenu,
			};
			_notifyIcon.Click += NotifyIcon_Click;
			Application.Current.Exit += Application_Exit;
		}

		void Application_Exit(object sender, ExitEventArgs e) {
			_notifyIcon.Visible = false;
		}

		void NotifyIcon_Click(object sender, EventArgs e) {
			Forms.MouseEventArgs args = (Forms.MouseEventArgs) e;
			if (args.Button == Forms.MouseButtons.Left) {
				_mainModel.IsVisible = !_mainModel.IsVisible;
			}
			else if (args.Button == Forms.MouseButtons.Right) {
				_contextMenu.Show();
			}
		}

		void ItemExit_Click(object sender, EventArgs e) {
			Application.Current.Shutdown(0);
		}

		Forms.NotifyIcon _notifyIcon;
		Forms.ContextMenuStrip _contextMenu;
		MainModel _mainModel;
	}
}