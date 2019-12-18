using MSTSCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RdcMan
{
	public static class MenuHelper
	{
		public static void AddSessionMenuItems(ContextMenuStrip menu, ServerBase server)
		{
			bool isConnected = server.IsConnected;
			ToolStripMenuItem toolStripMenuItem = new DelegateMenuItem("���ӷ�����", MenuNames.SessionConnect, server.Connect);
			toolStripMenuItem.Enabled = !isConnected;
			menu.Items.Add(toolStripMenuItem);
			toolStripMenuItem = new DelegateMenuItem("���ӷ�����Ϊ...", MenuNames.SessionConnectAs, server.DoConnectAs);
			toolStripMenuItem.Enabled = !isConnected;
			menu.Items.Add(toolStripMenuItem);
			toolStripMenuItem = new DelegateMenuItem("�������ӷ�����", MenuNames.SessionReconnect, server.Reconnect);
			toolStripMenuItem.Enabled = isConnected;
			menu.Items.Add(toolStripMenuItem);
			menu.Items.Add("-");
			toolStripMenuItem = menu.Items.Add("���Ͱ���", MenuNames.SessionSendKeys);
			toolStripMenuItem.Enabled = isConnected;
			AddSendKeysMenuItems(toolStripMenuItem, () => server);
			if (RdpClient.SupportsRemoteSessionActions)
			{
				toolStripMenuItem = menu.Items.Add("Զ�̶���", MenuNames.SessionRemoteActions);
				toolStripMenuItem.Enabled = isConnected;
				AddRemoteActionsMenuItems(toolStripMenuItem, () => server);
			}
			menu.Items.Add("-");
			toolStripMenuItem = new DelegateMenuItem("�˿ڷ���������", MenuNames.SessionDisconnect, server.Disconnect);
			toolStripMenuItem.Enabled = isConnected;
			menu.Items.Add(toolStripMenuItem);
			menu.Items.Add("-");
			toolStripMenuItem = new DelegateMenuItem("ע��������", MenuNames.SessionLogOff, server.LogOff);
			toolStripMenuItem.Enabled = (!Policies.DisableLogOff && isConnected);
			menu.Items.Add(toolStripMenuItem);
			menu.Items.Add(new DelegateMenuItem("�Ự�б�", MenuNames.SessionListSessions, delegate
			{
				Program.ShowForm(new ListSessionsForm(server));
			}));
		}

		public static void AddSendKeysMenuItems(ToolStripMenuItem parentItem, Func<ServerBase> getServer)
		{
			List<SendKeysMenuItem> list = new List<SendKeysMenuItem>(new SendKeysMenuItem[4]
			{
				new SendKeysMenuItem("��ȫ�Ի���", new Keys[3]
				{
					Keys.ControlKey,
					Keys.Menu,
					Keys.Delete
				}),
				new SendKeysMenuItem("���ڲ˵�", new Keys[2]
				{
					Keys.Menu,
					Keys.Space
				}),
				new SendKeysMenuItem("���������", new Keys[3]
				{
					Keys.ControlKey,
					Keys.ShiftKey,
					Keys.Escape
				}),
				new SendKeysMenuItem("��ʼ�˵�", new Keys[2]
				{
					Keys.ControlKey,
					Keys.Escape
				})
			});
			foreach (SendKeysMenuItem item in list)
			{
				item.Click += delegate(object sender, EventArgs e)
				{
					SendKeys.Send((sender as SendKeysMenuItem).KeyCodes, getServer());
				};
				parentItem.DropDownItems.Add(item);
			}
		}

		public static void AddRemoteActionsMenuItems(ToolStripMenuItem parentItem, Func<ServerBase> getServer)
		{
			List<ToolStripMenuItem> list = new List<ToolStripMenuItem>(new ToolStripMenuItem[5]
			{
				new ToolStripMenuItem("App commands")
				{
					Tag = RemoteSessionActionType.RemoteSessionActionAppbar
				},
				new ToolStripMenuItem("Charms")
				{
					Tag = RemoteSessionActionType.RemoteSessionActionCharms
				},
				new ToolStripMenuItem("Snap")
				{
					Tag = RemoteSessionActionType.RemoteSessionActionSnap
				},
				new ToolStripMenuItem("Switch apps")
				{
					Tag = RemoteSessionActionType.RemoteSessionActionAppSwitch
				},
				new ToolStripMenuItem("��ʼ�˵�")
				{
					Tag = RemoteSessionActionType.RemoteSessionActionStartScreen
				}
			});
			foreach (ToolStripMenuItem item in list)
			{
				item.Click += delegate(object sender, EventArgs e)
				{
					getServer().ServerNode.SendRemoteAction((RemoteSessionActionType)(sender as ToolStripMenuItem).Tag);
				};
				parentItem.DropDownItems.Add(item);
			}
		}

		public static void AddDockingMenuItems(ContextMenuStrip menu, ServerBase server)
		{
			bool isConnected = server.IsConnected;
			bool isClientFullScreen = server.IsClientFullScreen;
			ToolStripMenuItem toolStripMenuItem = new DelegateMenuItem("ȫ��", MenuNames.SessionFullScreen, delegate
			{
				ServerTree.Instance.SelectedNode = server;
				server.GoFullScreen();
			});
			toolStripMenuItem.Enabled = (isConnected && !isClientFullScreen);
			menu.Items.Add(toolStripMenuItem);
			toolStripMenuItem = new DelegateMenuItem("ȡ��ͣ��", MenuNames.SessionUndock, server.Undock);
			toolStripMenuItem.Enabled = (server.IsClientDocked && !isClientFullScreen);
			menu.Items.Add(toolStripMenuItem);
			toolStripMenuItem = new DelegateMenuItem("ȡ��ͣ��������", MenuNames.SessionUndockAndConnect, delegate
			{
				server.Undock();
				server.Connect();
			});
			toolStripMenuItem.Enabled = (server.IsClientDocked && !isConnected && !isClientFullScreen);
			menu.Items.Add(toolStripMenuItem);
			toolStripMenuItem = new DelegateMenuItem("ͣ��", MenuNames.SessionDock, server.Dock);
			toolStripMenuItem.Enabled = server.IsClientUndocked;
			menu.Items.Add(toolStripMenuItem);
		}

		public static void AddMaintenanceMenuItems(ContextMenuStrip menu, ServerBase server)
		{
			ToolStripMenuItem toolStripMenuItem = new DelegateMenuItem("ɾ��������", MenuNames.EditRemove, delegate
			{
				ServerTree.Instance.ConfirmRemove(server, askUser: true);
			});
			toolStripMenuItem.Enabled = server.CanRemove(popUI: false);
			menu.Items.Add(toolStripMenuItem);
			menu.Items.Add("-");
			toolStripMenuItem = new DelegateMenuItem("���ӵ��ղؼ�", MenuNames.EditAddToFavorites, delegate
			{
				FavoritesGroup.Instance.AddReference(server);
			});
			toolStripMenuItem.Enabled = server.AllowEdit(popUI: false);
			menu.Items.Add(toolStripMenuItem);
			menu.Items.Add("-");
			toolStripMenuItem = new DelegateMenuItem("����", MenuNames.EditProperties, delegate
			{
				server.DoPropertiesDialog();
			});
			toolStripMenuItem.Enabled = server.HasProperties;
			menu.Items.Add(toolStripMenuItem);
		}

		public static void ConnectTo()
		{
			using (ConnectToDialog connectToDialog = ConnectToDialog.NewConnectToDialog(Program.TheForm))
			{
				if (connectToDialog.ShowDialog() == DialogResult.OK)
				{
					Server server = TemporaryServer.Create(connectToDialog);
					server.Connect();
					ServerTree.Instance.SelectedNode = server;
				}
			}
		}

		public static void FindServers()
		{
			using (FindServersDialog findServersDialog = new FindServersDialog())
			{
				if (findServersDialog.ShowDialog() == DialogResult.OK)
				{
					ServerBase serverBase = findServersDialog.SelectedServers.FirstOrDefault();
					if (serverBase != null)
					{
						ServerTree.Instance.SelectedNode = serverBase;
					}
				}
			}
		}

		public static void AddFavorite(RdcTreeNode node)
		{
			ServerBase serverBase = node as ServerBase;
			if (serverBase != null)
			{
				FavoritesGroup.Instance.AddReference(serverBase);
			}
		}

		public static void ShowGlobalOptionsDialog()
		{
			using (GlobalOptionsDialog globalOptionsDialog = GlobalOptionsDialog.New())
			{
				if (globalOptionsDialog.ShowDialog() == DialogResult.OK)
				{
					globalOptionsDialog.UpdatePreferences();
					Program.Preferences.NeedToSave = true;
					Program.Preferences.Save();
					Program.TheForm.LockWindowSize();
					Program.TheForm.SetMainMenuVisibility();
					ServerTree.Instance.UpdateColors();
					ServerTree.Instance.SortAllNodes();
					ServerTree.Instance.OnGroupChanged(ServerTree.Instance.RootNode, ChangeType.PropertyChanged);
					Program.TheForm.UpdateAutoSaveTimer();
				}
			}
		}
	}
}